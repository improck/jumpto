using UnityEditor;
using UnityEngine;
using System.Collections;


namespace JumpTo
{
	public abstract class GuiJumpLinkView : ScriptableObject
	{
		public virtual void OnWindowEnable() { }
		public virtual void OnWindowDisable() { }
		public virtual void OnWindowClose() { }

		public abstract void OnGui();

		public virtual void OnSerialize() { }
		public virtual void OnDeserialize() { }
	}
}
