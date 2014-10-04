using UnityEditor;
using UnityEngine;
using System.Reflection;


namespace JumpTo
{
	internal static class JumpToUtility
	{
		public static string GetTransformPath(Transform transform)
		{
			string path = string.Empty;
			while (transform != null)
			{
				path = "/" + transform.name + path;
				transform = transform.parent;
			}

			return path;
		}

		public static void SetWindowTitleContent(this EditorWindow window, Texture2D tabIcon, string text)
		{
			//technique found at:
			//	https://code.google.com/p/hounitylibs/source/browse/trunk/HOEditorUtils/HOPanelUtils.cs

			const BindingFlags bFlags = BindingFlags.Instance | BindingFlags.NonPublic;
            PropertyInfo cachedTitleContentProperty = typeof(EditorWindow).GetProperty("cachedTitleContent", bFlags);
            if (cachedTitleContentProperty == null) return;

			GUIContent guiContent = cachedTitleContentProperty.GetValue(window, null) as GUIContent;
			guiContent.image = tabIcon;
			guiContent.text = text;
		}

		public static GUIContent GetWindowTitleContent(this EditorWindow window)
		{
			//technique found at:
			//	https://code.google.com/p/hounitylibs/source/browse/trunk/HOEditorUtils/HOPanelUtils.cs

			const BindingFlags bFlags = BindingFlags.Instance | BindingFlags.NonPublic;
			PropertyInfo cachedTitleContentProperty = typeof(EditorWindow).GetProperty("cachedTitleContent", bFlags);
			if (cachedTitleContentProperty == null) return null;

			return cachedTitleContentProperty.GetValue(window, null) as GUIContent;
		}
	}


	internal static class SerializedObjectExtension
	{
		private static PropertyInfo m_InspectorModeProperty = null;


		public static InspectorMode GetInspectorMode(this SerializedObject serializedObject)
		{
			if (m_InspectorModeProperty == null)
			{
				m_InspectorModeProperty =
					typeof(SerializedObject).GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);
			}

			return (InspectorMode)m_InspectorModeProperty.GetValue(serializedObject, null);
		}

		public static void SetInspectorMode(this SerializedObject serializedObject, InspectorMode inspectorMode)
		{
			if (m_InspectorModeProperty == null)
			{
				m_InspectorModeProperty =
					typeof(SerializedObject).GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);
			}

			m_InspectorModeProperty.SetValue(serializedObject, inspectorMode, null);
		}

		public static int GetLocalIdInFile(this SerializedObject serializedObject)
		{
			//NOTE: the property name is actually misspelled in the object!
			SerializedProperty localIdProp = serializedObject.FindProperty("m_LocalIdentfierInFile");
			if (localIdProp != null)
				return localIdProp.intValue;

			return -1;
		}
	}


	//tip found at:
	//	https://code.google.com/p/hounitylibs/source/browse/trunk/HOEditorUtils/HOPanelUtils.cs
	public class EditorWindowCachedTitleContentWrapper
	{
		private EditorWindow m_Window = null;
		private PropertyInfo m_CachedTitleContentProperty = null;


		public GUIContent TitleContent
		{
			get
			{
				return m_CachedTitleContentProperty.GetValue(m_Window, null) as GUIContent;
			}
		}


		public EditorWindowCachedTitleContentWrapper(EditorWindow window)
		{
			m_Window = window;

			m_CachedTitleContentProperty =
				typeof(EditorWindow).GetProperty("cachedTitleContent", BindingFlags.Instance | BindingFlags.NonPublic);
		}
	}
}
