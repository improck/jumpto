using UnityEditor;
using UnityEngine;
using System.Collections;


namespace JumpTo
{
	public class JumpToAssetModProc : UnityEditor.AssetModificationProcessor
	{
		public static string[] OnWillSaveAssets(string[] assetPaths)
		{
			Debug.Log("OnWillSaveAssets() " + assetPaths.Length);

			//NOTE: OnWillSaveAssets() gets called on Save As, but assetPaths
			//		is empty (0 length). A few posts on the Internet say that
			//		this can happen under other circumstances as well. Treating
			//		it like a scene save anyway, just in case.
			if (assetPaths == null || assetPaths.Length == 0)
			{
				SerializationControl.Instance.WaitForSceneAssetSave(null);
			}
			//for a regular asset save
			else
			{
				for (int i = 0; i < assetPaths.Length; i++)
				{
					Debug.Log(assetPaths[i]);
					if (assetPaths[i].EndsWith(".unity"))
					{
						Debug.Log("About to save " + assetPaths[i]);
						
						//SerializationControl.Instance.SceneAssetWillSave = true;
						SerializationControl.Instance.WaitForSceneAssetSave(assetPaths[i]);

						break;
					}
				}
			}

			return assetPaths;
		}
	}
}
