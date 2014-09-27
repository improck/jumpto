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

		public static void SetWindowTitleContent(EditorWindow window, Texture2D tabIcon, string text)
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
	}


	internal static class SerializedObjectExtension
	{
		private static PropertyInfo m_InspectorModeProperty = null;


		public static InspectorMode GetInspectorMode(this SerializedObject serializedObject)
		{
			if (m_InspectorModeProperty == null)
			{
				m_InspectorModeProperty = typeof(SerializedObject).GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);
			}

			return (InspectorMode)m_InspectorModeProperty.GetValue(serializedObject, null);
		}

		public static void SetInspectorMode(this SerializedObject serializedObject, InspectorMode inspectorMode)
		{
			if (m_InspectorModeProperty == null)
			{
				m_InspectorModeProperty = typeof(SerializedObject).GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);
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
}
