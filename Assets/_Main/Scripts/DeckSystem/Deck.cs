using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Fiber.Managers;
using Fiber.Utilities;
using GamePlay.Player;
using GridSystem;
using TriInspector;
using UnityEngine;
using UnityEngine.Events;

namespace DeckSystem
{
	public class Deck : Singleton<Deck>
	{
		public int NodeCount => nodeQueue.Count + deckQueue.Count;

		[Title("Settings")]
		[SerializeField] private Vector3 positionOffset;
		[SerializeField] private float scaleOffset;
		[SerializeField] private float moveDuration = 0.5F;

		[Title("References")]
		[SerializeField] private Transform enterPoint;
		[SerializeField] private Transform exitPoint;

		[Title("Setup")]
		[SerializeField] private int visibleNodes = 1;
		[SerializeField] private NodeOption[] nodeOptions;

		public Node CurrentNode => currentNode;
		private Node currentNode;
		private readonly Queue<Node> nodeQueue = new Queue<Node>();
		private readonly Queue<Node> deckQueue = new Queue<Node>();


		public static event UnityAction<Node, Node> OnSwapStart; // Node current, Node next
		public static event UnityAction<Node, Node> OnSwapEnd; // Node current, Node next

		private void OnEnable()
		{
			LevelManager.OnLevelLoad += OnLevelLoaded;
			LevelManager.OnLevelStart += OnLevelStarted;

			PlayerInputs.OnUp += OnFingerUp;
		}

		private void OnDisable()
		{
			LevelManager.OnLevelLoad -= OnLevelLoaded;
			LevelManager.OnLevelStart -= OnLevelStarted;

			PlayerInputs.OnUp -= OnFingerUp;
		}

		private void OnLevelLoaded()
		{
			foreach (var nodeOption in nodeOptions)
			{
				var node = Instantiate(GameManager.Instance.PrefabsSO.NodePrefab, transform);
				node.Setup(nodeOption.Nodes, null);

				nodeQueue.Enqueue(node);
				node.gameObject.SetActive(false);
			}
		}

		private void OnLevelStarted()
		{
			for (int i = 0; i < visibleNodes; i++)
			{
				if (nodeQueue.TryDequeue(out var node))
				{
					if (i.Equals(0))
						currentNode = node;
					else
						deckQueue.Enqueue(node);

					node.gameObject.SetActive(true);
					node.transform.localPosition = i * positionOffset;
					node.transform.localScale = (1 + i * scaleOffset) * Vector3.one;
				}
			}
		}

		private void OnFingerUp(GridCell selectedCell)
		{
			if (selectedCell.CurrentNode is { Obstacle: null } node)
			{
				Swap(node);
			}
		}

		public void Swap(Node selectedNode)
		{
			Player.Instance.Inputs.CanInput = false;

			OnSwapStart?.Invoke(currentNode, selectedNode);

			var cell = selectedNode.CurrentGridCell;
			currentNode.JumpTo(cell.transform.position).OnComplete(() =>
			{
				Player.Instance.Inputs.CanInput = true;

				cell.SetNode(currentNode);
				GridManager.Instance.OnSwap(cell);
				OnSwapEnd?.Invoke(currentNode, selectedNode);
			});

			selectedNode.JumpTo(transform.position).OnComplete(() =>
			{
				currentNode = selectedNode;
				currentNode.transform.SetParent(transform);
				selectedNode.CurrentGridCell = null;
			});
		}

		public async UniTask NextNode()
		{
			Player.Instance.Inputs.CanInput = false;

			var tempNode = currentNode;
			if (nodeQueue.TryDequeue(out var node))
			{
				node.gameObject.SetActive(true);
				node.transform.position = enterPoint.transform.position;
				node.transform.localScale = Vector3.zero;
				if (!deckQueue.Contains(node))
					deckQueue.Enqueue(node);
			}

			var i = 0;
			foreach (var nodesInQueue in deckQueue)
			{
				nodesInQueue.transform.DOLocalMove(i * positionOffset, moveDuration).SetEase(Ease.InOutSine);
				nodesInQueue.transform.DOScale(1 + i * scaleOffset, moveDuration).SetEase(Ease.InOutSine);
				i++;
			}

			if (deckQueue.TryDequeue(out var nextNode))
			{
				currentNode = nextNode;
			}
			else
			{
				CheckLose();
			}

			if (tempNode)
			{
				await tempNode.transform.DOMove(exitPoint.transform.position, moveDuration).SetEase(Ease.OutExpo).OnComplete(() => Destroy(tempNode.gameObject)).AsyncWaitForCompletion();
			}
			else
			{
				await UniTask.WaitForSeconds(moveDuration);
			}

			Player.Instance.Inputs.CanInput = true;
		}

		private async void CheckLose()
		{
			await UniTask.WaitForSeconds(2);

			LevelManager.Instance.Lose();
		}
	}
}