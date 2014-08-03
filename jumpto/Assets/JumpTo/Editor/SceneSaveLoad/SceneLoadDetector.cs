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
			//TODO: instance gets leaked on compile
			DestroyImmediate(s_Instance);
			s_Instance = null;
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
