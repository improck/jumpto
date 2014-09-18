using UnityEditor;


namespace SceneStateDetection
{
	/// <summary>
	/// Attempts to detect when a scene is saved as an asset in the project.
	/// </summary>
	internal sealed class SceneSaveDetector : UnityEditor.AssetModificationProcessor
	{
		public static string[] OnWillSaveAssets(string[] assetPaths)
		{
			//UnityEngine.Debug.Log("OnWillSaveAssets(): " + assetPaths.Length);
			
			//linear search for a scene asset within the paths
			for (int i = 0; i < assetPaths.Length; i++)
			{
				if (assetPaths[i].EndsWith(".unity"))
				{
					//signal that a scene is about to be saved, then
					//	stop searching since you can only have one
					//	scene open at a time (for now)
					SceneStateControl.SceneWillSave(assetPaths[i]);
					break;
				}
			}

			//return the asset paths without any modifications
			return assetPaths;
		}
	}
}
