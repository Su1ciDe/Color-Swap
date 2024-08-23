using GridSystem;
using TriInspector;
using UnityEngine;

namespace GamePlay.Obstacles
{
	public abstract class NodeObstacle : BaseObstacle
	{
		[field:Title("Properties")]
		[field: SerializeField, ReadOnly] public Node CurrentNode { get; private set; }

		public void Setup(Node node)
		{
			CurrentNode = node;
		}

		public override void DestroyObstacle()
		{
			if (CurrentNode)
			{
				CurrentNode.OnObstacleDestroyed();
			}
			
			base.DestroyObstacle();
		}
	}
}