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


		public static event EditorApplication.CallbackFunction OnSceneSaved;
		public static event EditorApplication.CallbackFunction OnSceneLoaded;

		[SerializeField] private bool m_HierarchyChanged = false;

		
		public static void SceneWillSave()
		{
			SceneLoadDetector.TemporarilyDestroyInstance();

			EditorApplication.delayCall += DelayedSceneSave;
		}

		public static void SceneIsUnloading()
		{
			s_Instance.m_HierarchyChanged = false;
			EditorApplication.hierarchyWindowChanged += OnHierarchyWindowChanged;
		}

		public static void SceneWillLoad()
		{	
			EditorApplication.delayCall += DelayedSceneLoad;
		}

		private static void DelayedSceneSave()
		{
			//string currentScene = EditorApplication.currentScene;
			//Debug.Log("Scene Save: " + currentScene + "\n" + AssetDatabase.AssetPathToGUID(currentScene));

			if (OnSceneSaved != null)
				OnSceneSaved();
		}

		private static void OnHierarchyWindowChanged()
		{
			s_Instance.m_HierarchyChanged = true;

			EditorApplication.hierarchyWindowChanged -= OnHierarchyWindowChanged;
		}

		private static void DelayedSceneLoad()
		{
			//if the hierarchy changed prior to the delayed scene load
			//	then the scene load was the result of a scene asset being
			//	opened or a new scene being created. else, it was triggered
			//	by an assembly reload which deserializes the scene but does
			//	NOT call the hierarchyWindowChanged event
			if (s_Instance.m_HierarchyChanged)
			{
				//string currentScene = EditorApplication.currentScene;
				//if (!string.IsNullOrEmpty(currentScene))
				//{
				//	Debug.Log("Scene Load: " + currentScene + "\n" + AssetDatabase.AssetPathToGUID(currentScene));
				//}
				//else
				//{
				//	Debug.Log("Scene Load: (Unsaved)");
				//}

				s_Instance.m_HierarchyChanged = false;

				if (OnSceneLoaded != null)
					OnSceneLoaded();
			}

			EditorApplication.hierarchyWindowChanged -= OnHierarchyWindowChanged;
		}
	}
}
