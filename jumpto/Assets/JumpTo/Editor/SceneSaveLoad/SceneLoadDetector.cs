using UnityEditor;
using UnityEngine;


namespace JumpTo
{
	public class SceneLoadDetector : ScriptableObject
	{
		private static SceneLoadDetector s_Instance;


		public static void EnsureExistence()
		{
			EditorApplication.delayCall += WaitToCreate;
		}

		public static void TemporarilyDestroyInstance()
		{
			if (s_Instance != null)
			{
				//NOTE: as soon as OnDestroy() gets called, a new instance
				//		is scheduled to be created in the next editor frame
				DestroyImmediate(s_Instance);
				s_Instance = null;
			}
		}

		private static void WaitToCreate()
		{
			if (Resources.FindObjectsOfTypeAll<SceneLoadDetector>().Length == 0)
			{
				s_Instance = CreateInstance<SceneLoadDetector>();
				s_Instance.hideFlags = HideFlags.HideInHierarchy;

				SceneSaveLoadControl.WaitForSceneLoad();
			}
		}

		void OnDestroy()
		{
			EnsureExistence();
		}
	}
}
