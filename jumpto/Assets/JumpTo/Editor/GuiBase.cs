using UnityEditor;
using UnityEngine;
using System.Collections;


namespace JumpTo
{
	public abstract class GuiBase : ScriptableObject
	{
		public virtual void OnWindowEnable() { }
		public virtual void OnWindowDisable() { }
		public virtual void OnWindowClose() { }

		public abstract void OnGui(RectRef position);

		public virtual void OnSerialize() { }
		public virtual void OnDeserialize() { }
	}
}
