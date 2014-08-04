using UnityEditor;
using UnityEngine;


namespace SceneStateDetection
{
	public static class SceneSaveLoadControl
	{
		public static event EditorApplication.CallbackFunction OnSceneSaved;
		public static event EditorApplication.CallbackFunction OnSceneLoaded;

		//private static bool m_HierarchyChanged = false;


		public static void WaitForSceneAssetSave()
		{
			SceneLoadDetector.TemporarilyDestroyInstance();

			EditorApplication.delayCall += DelayedSceneAssetSave;
		}

		public static void WaitForSceneLoad()
		{
			//TODO: OnSceneLoaded is being called on recompile, which is wrong
			//SEE: OnHierarchyWindowChanged()
			//m_HierarchyChanged = false;
			//EditorApplication.hierarchyWindowChanged += OnHierarchyWindowChanged;

			EditorApplication.delayCall += DelayedSceneLoad;
		}

		private static void DelayedSceneAssetSave()
		{
			//string currentScene = EditorApplication.currentScene;
			//Debug.Log("Scene Save: " + currentScene + "\n" + AssetDatabase.AssetPathToGUID(currentScene));

			if (OnSceneSaved != null)
				OnSceneSaved();
		}

		private static void OnHierarchyWindowChanged()
		{
			//TODO: this doesn't work because m_HierarchyChanged isn't serialized on assembly reload
			Debug.Log("Marking hierarchy as changed");
			//m_HierarchyChanged = true;

			EditorApplication.hierarchyWindowChanged -= OnHierarchyWindowChanged;
		}

		private static void DelayedSceneLoad()
		{
			//if the hierarchy changed prior to the delayed scene load
			//	then the scene load was the result of a scene asset being
			//	opened or a new scene being created. else, it was triggered
			//	by an assembly reload which deserializes the scene but does
			//	NOT call the hierarchyWindowChanged event
			//if (m_HierarchyChanged)
			{
				//m_HierarchyChanged = false;

				string currentScene = EditorApplication.currentScene;
				if (!string.IsNullOrEmpty(currentScene))
				{
					Debug.Log("Scene Load: " + currentScene + "\n" + AssetDatabase.AssetPathToGUID(currentScene));
				}
				else
				{
					Debug.Log("Scene Load: (Unsaved)");
				}

				if (OnSceneLoaded != null)
					OnSceneLoaded();
			}
		}
	}


	//public class SceneLoadDataStore : ScriptableObject
	//{
	//	[SerializeField] private bool m_HierarchyChanged = false;

	//	public bool HierarchyChanged { get { return m_HierarchyChanged; } set { m_HierarchyChanged = value; } }


	//	public static SceneLoadDataStore Create()
	//	{
	//		SceneLoadDataStore instance = ScriptableObject.CreateInstance<SceneLoadDataStore>();
	//		instance.hideFlags = HideFlags.HideAndDontSave;

	//		return instance;
	//	}
	//}
}
