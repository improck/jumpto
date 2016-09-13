using UnityEditor;


namespace ImpRock.JumpTo.Editor
{
	/// <summary>
	/// Attempts to detect when scenes are saved as assets in the project.
	/// </summary>
	internal sealed class SceneSaveDetector : UnityEditor.AssetModificationProcessor
	{
		public static event System.Action<string> OnSceneWillSave;
		public static event System.Action<string> OnSceneSaved;
		public static event System.Action<string> OnSceneWillDelete;
		public static event System.Action<string> OnSceneDeleted;


		public static string[] OnWillSaveAssets(string[] assetPaths)
		{
			//linear search for scenes asset within the paths
			for (int i = 0; i < assetPaths.Length; i++)
			{
				if (assetPaths[i].EndsWith(".unity"))
				{
					//signal that a scene is about to be saved
					SceneWillSave(assetPaths[i]);
				}
			}

			//return the asset paths without any modifications
			return assetPaths;
		}

		public static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions options)
		{
			if (assetPath.EndsWith(".unity"))
			{
				SceneWillDelete(assetPath);
			}

			//this means "i did not delete the file manually," in
			//	which case Unity will continue to delete the file
			//	as normal
			return AssetDeleteResult.DidNotDelete;
		}

		private static void SceneWillSave(string sceneAssetPath)
		{
			if (OnSceneWillSave != null)
				OnSceneWillSave(sceneAssetPath);

			if (OnSceneSaved != null)
			{
				EditorApplication.delayCall +=
					delegate ()
					{
						OnSceneSaved(sceneAssetPath);
					};
			}
		}

		private static void SceneWillDelete(string sceneAssetPath)
		{
			if (OnSceneWillDelete != null)
				OnSceneWillDelete(sceneAssetPath);

			if (OnSceneDeleted != null)
			{
				EditorApplication.delayCall +=
					delegate ()
					{
						OnSceneDeleted(sceneAssetPath);
					};
			}
		}
	}
}
