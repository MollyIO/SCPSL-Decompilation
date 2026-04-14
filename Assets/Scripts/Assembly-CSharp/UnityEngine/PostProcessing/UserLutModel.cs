using System;
using System.Runtime.CompilerServices;

namespace UnityEngine.PostProcessing
{
	[Serializable]
	public class UserLutModel : PostProcessingModel
	{
		[Serializable]
		public struct Settings
		{
			[Tooltip("Custom lookup texture (strip format, e.g. 256x16).")]
			public Texture2D lut;

			[Tooltip("Blending factor.")]
			[Range(0f, 1f)]
			public float contribution;

			public static Settings defaultSettings
			{
				[CompilerGenerated]
				get
				{
					return new Settings
					{
						lut = null,
						contribution = 1f
					};
				}
			}
		}

		[SerializeField]
		private Settings m_Settings = Settings.defaultSettings;

		public Settings settings
		{
			get
			{
				return m_Settings;
			}
			set
			{
				m_Settings = value;
			}
		}

		public override void Reset()
		{
			m_Settings = Settings.defaultSettings;
		}
	}
}
