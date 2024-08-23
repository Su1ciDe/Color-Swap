using GridSystem;
using UnityEngine;

namespace GamePlay.Obstacles
{
	public abstract class BaseObstacle : MonoBehaviour
	{
		public virtual void OnBlastNear(Node node)
		{
		}

		public virtual void DestroyObstacle()
		{
			Destroy(gameObject);
		}
	}
}