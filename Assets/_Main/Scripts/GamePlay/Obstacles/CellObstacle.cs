using GridSystem;
using Interfaces;
using UnityEngine;

namespace GamePlay.Obstacles
{
	public abstract class CellObstacle : BaseObstacle, INode
	{
		public GridCell CurrentGridCell { get; set; }

		public virtual void Setup(GridCell gridCell)
		{
			CurrentGridCell = gridCell;
		}

		public Transform GetTransform() => transform;
	}
}