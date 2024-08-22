using GridSystem;
using UnityEngine;

namespace Interfaces
{
	public interface INode
	{
		public GridCell CurrentGridCell { get; set; }
		public Transform GetTransform();
	}
}