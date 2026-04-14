using System;

[AttributeUsage(AttributeTargets.Field)]
public class SECTR_ToolTip : Attribute
{
	private string tipText;

	private string dependentProperty;

	private float min;

	private float max;

	private Type enumType;

	private bool hasRange;

	private bool devOnly;

	private bool sceneObjectOverride;

	private bool allowSceneObjects;

	private bool treatAsLayer;

	public string TipText
	{
		get
		{
			return tipText;
		}
	}

	public string DependentProperty
	{
		get
		{
			return dependentProperty;
		}
	}

	public float Min
	{
		get
		{
			return min;
		}
	}

	public float Max
	{
		get
		{
			return max;
		}
	}

	public Type EnumType
	{
		get
		{
			return enumType;
		}
	}

	public bool HasRange
	{
		get
		{
			return hasRange;
		}
	}

	public bool DevOnly
	{
		get
		{
			return devOnly;
		}
	}

	public bool SceneObjectOverride
	{
		get
		{
			return sceneObjectOverride;
		}
	}

	public bool AllowSceneObjects
	{
		get
		{
			return allowSceneObjects;
		}
	}

	public bool TreatAsLayer
	{
		get
		{
			return treatAsLayer;
		}
	}

	public SECTR_ToolTip(string tipText)
	{
		this.tipText = tipText;
	}

	public SECTR_ToolTip(string tipText, float min, float max)
	{
		this.tipText = tipText;
		this.min = min;
		this.max = max;
		hasRange = true;
	}

	public SECTR_ToolTip(string tipText, string dependentProperty)
	{
		this.tipText = tipText;
		this.dependentProperty = dependentProperty;
	}

	public SECTR_ToolTip(string tipText, string dependentProperty, float min, float max)
	{
		this.tipText = tipText;
		this.dependentProperty = dependentProperty;
		this.min = min;
		this.max = max;
		hasRange = true;
	}

	public SECTR_ToolTip(string tipText, bool devOnly)
	{
		this.tipText = tipText;
		this.devOnly = devOnly;
	}

	public SECTR_ToolTip(string tipText, bool devOnly, bool treatAsLayer)
	{
		this.tipText = tipText;
		this.devOnly = devOnly;
		this.treatAsLayer = treatAsLayer;
	}

	public SECTR_ToolTip(string tipText, string dependentProperty, Type enumType)
	{
		this.tipText = tipText;
		this.dependentProperty = dependentProperty;
		this.enumType = enumType;
	}

	public SECTR_ToolTip(string tipText, string dependentProperty, bool allowSceneObjects)
	{
		this.tipText = tipText;
		this.dependentProperty = dependentProperty;
		sceneObjectOverride = true;
		this.allowSceneObjects = allowSceneObjects;
	}
}
