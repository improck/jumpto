using UnityEditor;
using UnityEngine;


namespace SceneStateDetection
{
	[System.Serializable]
	public class SceneStateControl
	{
		#region Pseudo-Singleton
		private static SceneStateControl s_Instance = null;

		public static SceneStateControl Instance { get { return s_Instance; } }

		public static SceneStateControl Create()
		{
			return new SceneStateControl();
		}


		protected SceneStateControl() { s_Instance = this; }
		#endregion


		public static event System.Action<string>	OnSceneWillSave;
		public static event System.Action<string>	OnSceneSaved;
		public static event System.Action			OnSceneWillLoad;
		public static event System.Action<string>	OnSceneLoaded;

		private static string s_SceneAssetSavePath = string.Empty;

		[SerializeField] private bool m_HierarchyChanged = false;
		
		
		public static void SceneWillSave(string sceneAssetPath)
		{
			s_SceneAssetSavePath = sceneAssetPath;

			SceneLoadDetector.TemporarilyDestroyInstance();

			//NOTE: for Save As, the currentScene has not been updated at
			//		this point.
			//string currentScene = EditorApplication.currentScene;
			Debug.Log("Scene Will Save: " + s_SceneAssetSavePath + "\n" + AssetDatabase.AssetPathToGUID(s_SceneAssetSavePath));

			if (OnSceneWillSave != null)
				OnSceneWillSave(s_SceneAssetSavePath);

			EditorApplication.delayCall += DelayedSceneSave;
		}

		public static void SceneIsUnloading()
		{
			s_Instance.m_HierarchyChanged = false;
			EditorApplication.hierarchyWindowChanged += OnHierarchyWindowChanged;

			if (OnSceneWillLoad != null)
				OnSceneWillLoad();
		}

		public static void SceneWillLoad()
		{	
			EditorApplication.delayCall += DelayedSceneLoad;
		}

		private static void DelayedSceneSave()
		{
			string currentScene = EditorApplication.currentScene;
			Debug.Log("Delayed Scene Save: " + currentScene + "\n" + AssetDatabase.AssetPathToGUID(currentScene));

			if (OnSceneSaved != null)
				OnSceneSaved(s_SceneAssetSavePath);

			s_SceneAssetSavePath = string.Empty;
		}

		private static void OnHierarchyWindowChanged()
		{
			s_Instance.m_HierarchyChanged = true;
			EditorApplication.hierarchyWindowChanged -= OnHierarchyWindowChanged;
		}

		private static void DelayedSceneLoad()
		{
			string currentScene = EditorApplication.currentScene;
			if (!string.IsNullOrEmpty(currentScene))
			{
				Debug.Log("Delayed Scene Load: " + currentScene + "\n" + AssetDatabase.AssetPathToGUID(currentScene));
			}
			else
			{
				Debug.Log("Delayed Scene Load: (Unsaved)");
			}

			//if the hierarchy changed prior to the delayed scene load
			//	then the scene load was the result of a scene asset being
			//	opened or a new scene being created. else, it was triggered
			//	by an assembly reload which deserializes the scene but does
			//	NOT call the hierarchyWindowChanged event
			if (s_Instance.m_HierarchyChanged)
			{
				Debug.Log("Hierarchy has changed");

				s_Instance.m_HierarchyChanged = false;

				if (OnSceneLoaded != null)
					OnSceneLoaded(EditorApplication.currentScene);
			}
			else
				Debug.Log("Hierarchy has NOT changed");

			EditorApplication.hierarchyWindowChanged -= OnHierarchyWindowChanged;
		}
	}
}
