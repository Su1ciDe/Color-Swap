using Fiber.Managers;
using UnityEngine;

namespace Fiber.LevelSystem
{
	public class Level : MonoBehaviour
	{
		public int Money => LevelManager.Instance.LevelNo * 2 + 4;

		public virtual void Load()
		{
			gameObject.SetActive(true);
		}

		public virtual void Play()
		{
		}
	}
}