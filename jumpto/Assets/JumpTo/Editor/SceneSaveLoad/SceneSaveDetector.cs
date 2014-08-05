using UnityEditor;


namespace SceneStateDetection
{
	/// <summary>
	/// Attempts to detect when a scene is saved as an asset in the project.
	/// </summary>
	public class SceneSaveDetector : UnityEditor.AssetModificationProcessor
	{
		public static string[] OnWillSaveAssets(string[] assetPaths)
		{
			//NOTE: OnWillSaveAssets() gets called on Save As, but assetPaths
			//		is empty (0 length). A few posts on the Internet say that
			//		this can happen under other circumstances as well. Treating
			//		it like a scene save anyway, just in case.
			if (assetPaths == null || assetPaths.Length == 0)
			{
				SceneStateControl.SceneWillSave();
			}
			//for a regular asset save
			else
			{
				//linear search for a scene asset within the paths
				for (int i = 0; i < assetPaths.Length; i++)
				{
					if (assetPaths[i].EndsWith(".unity"))
					{
						//signal that a scene is about to be saved, then
						//	stop searching
						SceneStateControl.SceneWillSave();
						break;
					}
				}
			}

			//return the asset paths without any modifications
			return assetPaths;
		}
	}
}
