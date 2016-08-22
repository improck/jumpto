﻿using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Reflection;
using System.Text;
using System.Collections.Generic;


namespace ImpRock.JumpTo.Editor
{
	internal static class JumpToUtility
	{
		public static string GetRootOrderPath(Transform transform)
		{
			SerializedObject so = null;
			Stack<string> pathStack = new Stack<string>();
			while (transform != null)
			{
				so = new SerializedObject(transform);
				pathStack.Push("/" + so.FindProperty("m_RootOrder").intValue.ToString());

				transform = transform.parent;
			}

			StringBuilder pathBuilder = new StringBuilder();
			while (pathStack.Count > 0)
			{
				pathBuilder.Append(pathStack.Pop());
			}

			return pathBuilder.ToString();
		}

		public static string GetTransformPath(Transform transform)
		{
			Stack<string> pathStack = new Stack<string>();
			while (transform != null)
			{
				pathStack.Push("/" + transform.name);

				transform = transform.parent;
			}

			StringBuilder pathBuilder = new StringBuilder();
			while (pathStack.Count > 0)
			{
				pathBuilder.Append(pathStack.Pop());
			}

			return pathBuilder.ToString();
		}

		public static void GetAllLocalIds(GameObject[] orderedRootObjects,
			Dictionary<int, GameObject> idToGameObjects, Dictionary<int, GameObject> idToPrefabs)
		{
			idToGameObjects.Clear();
			idToPrefabs.Clear();

			if (orderedRootObjects == null || orderedRootObjects.Length == 0)
				return;
			
			HierarchyProperty hierarchyProperty = new HierarchyProperty(HierarchyType.GameObjects);
			//int[] expanded = new int[0];
			if (!hierarchyProperty.Find(orderedRootObjects[0].GetInstanceID(), null))
				return;
			
			SerializedObject serializedObject = new SerializedObject(hierarchyProperty.pptrValue);
			serializedObject.SetInspectorMode(InspectorMode.Debug);
			int localId = serializedObject.GetLocalIdInFile();
			PrefabType prefabType = PrefabUtility.GetPrefabType(hierarchyProperty.pptrValue);
			if (prefabType != PrefabType.ModelPrefabInstance &&
				prefabType != PrefabType.PrefabInstance)
			{
				idToGameObjects.Add(localId, hierarchyProperty.pptrValue as GameObject);
			}
			else
			{
				idToPrefabs.Add(localId, hierarchyProperty.pptrValue as GameObject);
			}

			while (hierarchyProperty.Next(null))
			{
				prefabType = PrefabUtility.GetPrefabType(hierarchyProperty.pptrValue);
				if (prefabType != PrefabType.ModelPrefabInstance &&
					prefabType != PrefabType.PrefabInstance)
				{
					serializedObject = new SerializedObject(hierarchyProperty.pptrValue);
					serializedObject.SetInspectorMode(InspectorMode.Debug);
					localId = serializedObject.GetLocalIdInFile();
					idToGameObjects.Add(localId, hierarchyProperty.pptrValue as GameObject);
				}
				else
				{
					Object prefabObject = PrefabUtility.GetPrefabObject(hierarchyProperty.pptrValue);
					serializedObject = new SerializedObject(prefabObject);
					serializedObject.SetInspectorMode(InspectorMode.Debug);
					localId = serializedObject.GetLocalIdInFile();

					if (!idToPrefabs.ContainsKey(localId))
					{
						//NOTE: assuming here that the first one it runs into is the root of
						//		a prefab that hasn't been added yet.
						idToPrefabs.Add(localId, hierarchyProperty.pptrValue as GameObject);
					}
				}
			}
		}

		public static int FindSceneContaining(Object linkReference)
		{
			Transform linkRoot = (linkReference as GameObject).transform.root;

			int sceneCount = SceneManager.sceneCount;
			List<GameObject> rootObjects = new List<GameObject>();
			for (int i = 0; i < sceneCount; i++)
			{
				Scene scene = SceneManager.GetSceneAt(i);
				if (scene.isLoaded)
				{
					//NOTE: this clears the list internally
					scene.GetRootGameObjects(rootObjects);

					foreach (GameObject gameObject in rootObjects)
					{
						if (gameObject.transform.root == linkRoot)
							return scene.GetHashCode();
					}
				}
			}

			return 0;
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
	//public class EditorWindowCachedTitleContentWrapper
	//{
	//	private EditorWindow m_Window = null;
	//	private PropertyInfo m_CachedTitleContentProperty = null;


	//	public GUIContent TitleContent
	//	{
	//		get
	//		{
	//			return m_CachedTitleContentProperty.GetValue(m_Window, null) as GUIContent;
	//		}
	//	}


	//	public EditorWindowCachedTitleContentWrapper(EditorWindow window)
	//	{
	//		m_Window = window;

	//		m_CachedTitleContentProperty =
	//			typeof(EditorWindow).GetProperty("cachedTitleContent", BindingFlags.Instance | BindingFlags.NonPublic);
	//	}
	//}
}
