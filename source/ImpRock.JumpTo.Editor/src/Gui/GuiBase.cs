using UnityEditor;
using UnityEngine;
using System.Collections;


namespace JumpTo
{
	internal abstract class GuiBase : ScriptableObject
	{
		protected Vector2 m_Size;


		public virtual void OnWindowEnable(EditorWindow window) { }
		public virtual void OnWindowDisable(EditorWindow window) { }
		public virtual void OnWindowClose(EditorWindow window) { }

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

		public static T Create<T>() where T : GuiBase
		{
			T instance = ScriptableObject.CreateInstance<T>();
			instance.hideFlags = HideFlags.HideAndDontSave;

			return instance;
		}
	}
}
