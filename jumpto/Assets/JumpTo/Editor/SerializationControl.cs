using UnityEditor;
using UnityEngine;
using System.Collections;


namespace JumpTo
{
	public class SerializationControl
	{
		#region Singleton
		private static SerializationControl s_Instance = null;

		public static SerializationControl Instance { get { if (s_Instance == null) { s_Instance = new SerializationControl(); } return s_Instance; } }

		public static void DestroyInstance() { s_Instance = null; }
		#endregion


		private string m_SceneAssetPath = string.Empty;

		public void WaitForSceneAssetSave(string sceneAssetPath)
		{
			m_SceneAssetPath = sceneAssetPath;

			EditorApplication.delayCall += OnSceneAssetSave;
		}

		void OnSceneAssetSave()
		{
			if (string.IsNullOrEmpty(m_SceneAssetPath))
			{
				m_SceneAssetPath = EditorApplication.currentScene;
			}

			if (!string.IsNullOrEmpty(m_SceneAssetPath))
			{
				Debug.Log("Scene: " + m_SceneAssetPath + "\n" + AssetDatabase.AssetPathToGUID(m_SceneAssetPath));
			}
		}
	}
}
