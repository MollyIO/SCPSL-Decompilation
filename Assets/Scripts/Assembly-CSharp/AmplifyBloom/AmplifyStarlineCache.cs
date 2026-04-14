using System;
using UnityEngine;

namespace AmplifyBloom
{
	[Serializable]
	public class AmplifyStarlineCache
	{
		[SerializeField]
		internal AmplifyPassCache[] Passes;

		public AmplifyStarlineCache()
		{
			Passes = new AmplifyPassCache[4];
			for (int i = 0; i < 4; i++)
			{
				Passes[i] = new AmplifyPassCache();
			}
		}

		public void Destroy()
		{
			for (int i = 0; i < 4; i++)
			{
				Passes[i].Destroy();
			}
			Passes = null;
		}
	}
}
