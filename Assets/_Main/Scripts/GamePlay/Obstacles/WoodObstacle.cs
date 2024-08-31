using Fiber.AudioSystem;
using Fiber.Utilities;
using UnityEngine;

namespace GamePlay.Obstacles
{
	public class WoodObstacle : CellObstacle
	{
		public override void DestroyObstacle()
		{
			base.DestroyObstacle();

			ParticlePooler.Instance.Spawn("Wood", transform.position + .1f * Vector3.up);
			AudioManager.Instance.PlayAudio(AudioName.Wood).SetVolume(0.7f);
		}
	}
}