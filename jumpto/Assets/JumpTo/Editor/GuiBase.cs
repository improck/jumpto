using UnityEditor;
using UnityEngine;
using System.Collections;


namespace JumpTo
{
	public abstract class GuiBase : ScriptableObject
	{
		protected Vector2 m_Size;


		public virtual void OnWindowEnable() { }
		public virtual void OnWindowDisable() { }
		public virtual void OnWindowClose() { }

		protected abstract void OnGui();

		public virtual void OnSerialize() { }
		public virtual void OnDeserialize() { }

		//position -	The area within the parent control
		//				in which to draw this control
		public void Draw(RectRef position)
		{
			//make all gui things within OnGui() relative
			//	to position
			GUI.BeginGroup(position);

			m_Size.x = position.width;
			m_Size.y = position.height;
			OnGui();

			GUI.EndGroup();
		}
	}
}
