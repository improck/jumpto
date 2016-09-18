//http://forum.unity3d.com/threads/is-there-a-unity-editor-gui-skin-template-floating-around-somewhere.170632/

using UnityEngine;
using UnityEditor;


public static class EditorSkinsExtractor
{
    [MenuItem("Assets/Extract Editor Skins")]
    public static void MenuItem_ExtractEditorSkins()
    {
		string unityVersion = Application.unityVersion;
		string skinsFolder = "EditorSkins_" + unityVersion;
		string fullPath = Application.dataPath + "/" + skinsFolder;

		if (!System.IO.Directory.Exists(fullPath))
		{
			AssetDatabase.CreateFolder("Assets", skinsFolder);
		}

		if (System.IO.Directory.Exists(fullPath))
		{
			string relativePath = "Assets/" + skinsFolder + "/EditorSkin-";

			ExtractSkin(relativePath, EditorSkin.Game);
			ExtractSkin(relativePath, EditorSkin.Inspector);
			ExtractSkin(relativePath, EditorSkin.Scene);
		}
		else
		{
			Debug.LogError("Failed to create folder");
		}
	}

	private static void ExtractSkin(string relativePath, EditorSkin skinType)
	{
		GUISkin skin = ScriptableObject.Instantiate(EditorGUIUtility.GetBuiltinSkin(skinType)) as GUISkin;
		AssetDatabase.CreateAsset(skin, relativePath + skinType + ".guiskin");
	}
}
