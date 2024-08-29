using System;
using Fiber.Managers;
using GridSystem;
using Lofelt.NiceVibrations;
using UnityEngine.Events;

namespace GoalSystem
{
	public class Goal : IDisposable
	{
		public int Count { get; set; }
		public int CurrentCount { get; set; }
		public TileType TileType { get; set; }

		public event UnityAction<int, int> OnGoalUpdate; // int currentCount, int count
		public event UnityAction<Goal> OnGoalComplete; // int currentCount, int count

		public Goal(TileType tileType, int count)
		{
			TileType = tileType;
			Count = count;

			NodeTile.OnTileBlast += OnTileBlast;
		}

		private void OnTileBlast(NodeTile tile)
		{
			if (TileType == TileType._8All)
			{
			}
			else if (tile.TileType != TileType) return;

			CurrentCount++;

			OnGoalUpdate?.Invoke(CurrentCount, Count);
			if (CurrentCount >= Count)
			{
				Complete();
			}
		}

		private void Complete()
		{
			NodeTile.OnTileBlast -= OnTileBlast;

			HapticManager.Instance.PlayHaptic(HapticPatterns.PresetType.Success);

			OnGoalComplete?.Invoke(this);
		}

		public void Dispose()
		{
			NodeTile.OnTileBlast -= OnTileBlast;
		}
	}
}