using Cysharp.Threading.Tasks;
using GridSystem;
using UnityEngine;

namespace Interfaces
{
	public interface INode
	{
		public bool IsFalling { get; }
		public GridCell CurrentGridCell { get; set; }
		public  UniTask Fall(Vector3 targetPosition);
		public float Velocity { get; }


		public Transform GetTransform();
	}
}