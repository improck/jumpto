

namespace ImpRock.JumpTo.Editor
{
	/// <summary>
	/// Attempts to detect when scenes are saved as assets in the project.
	/// </summary>
	internal sealed class SceneSaveDetector : UnityEditor.AssetModificationProcessor
	{
		public static string[] OnWillSaveAssets(string[] assetPaths)
		{
			//UnityEngine.Debug.Log("OnWillSaveAssets(): " + assetPaths.Length);
			
			//linear search for scenes asset within the paths
			for (int i = 0; i < assetPaths.Length; i++)
			{
				if (assetPaths[i].EndsWith(".unity"))
				{
					//signal that a scene is about to be saved
					SceneStateControl.SceneWillSave(assetPaths[i]);
				}
			}

			//return the asset paths without any modifications
			return assetPaths;
		}
	}
}
