using UnityEditor;
using UnityEngine;


namespace JumpTo
{
	public static class SceneSaveLoadControl
	{
		public static event EditorApplication.CallbackFunction OnSceneSaved;
		public static event EditorApplication.CallbackFunction OnSceneLoaded;


		public static void WaitForSceneAssetSave()
		{
			SceneLoadDetector.TemporarilyDestroyInstance();

			EditorApplication.delayCall += DelayedSceneAssetSave;
		}

		public static void WaitForSceneLoad()
		{
			EditorApplication.delayCall += DelayedSceneLoad;
		}

		private static void DelayedSceneAssetSave()
		{
			string currentScene = EditorApplication.currentScene;

			Debug.Log("Scene Save: " + currentScene + "\n" + AssetDatabase.AssetPathToGUID(currentScene));

			if (OnSceneSaved != null)
				OnSceneSaved();
		}

		private static void DelayedSceneLoad()
		{
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
