using GridSystem;
using UnityEngine.Events;

namespace Interfaces
{
	public interface IInputs
	{
		public bool CanInput { get; set; }

		public event UnityAction<GridCell> OnDown;
		public event UnityAction<GridCell> OnMove;
		public event UnityAction<GridCell> OnUp;
	}
}