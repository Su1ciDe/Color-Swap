using UnityEngine;

namespace Utilities
{
	public class BlastParticle : MonoBehaviour
	{
		private Renderer[] renderers;

		private void Awake()
		{
			renderers = GetComponentsInChildren<Renderer>();
		}

		public void Setup(Color color)
		{
			for (var i = 0; i < renderers.Length; i++)
			{
				renderers[i].material.color = color;
			}
		}
	}
}