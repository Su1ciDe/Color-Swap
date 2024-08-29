using Fiber.AudioSystem;
using Fiber.Utilities;
using UnityEngine;

namespace GamePlay.Obstacles
{
	public class IceObstacle : NodeObstacle
	{
		public override void DestroyObstacle()
		{
			base.DestroyObstacle();

			ParticlePooler.Instance.Spawn("Ice", transform.position + .1f * Vector3.up);
			AudioManager.Instance.PlayAudio(AudioName.Ice).SetVolume(0.7f);
		}
	}
}