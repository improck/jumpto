using UnityEditor;


namespace JumpTo
{
	public class SerializationControl
	{
		#region Singleton
		private static SerializationControl s_Instance = null;

		public static SerializationControl Instance { get { if (s_Instance == null) { s_Instance = new SerializationControl(); } return s_Instance; } }

		public static void DestroyInstance()
		{
			if (s_Instance != null)
			{
				s_Instance.CleanUp();
				s_Instance = null;
			}
		}
		#endregion


		public SerializationControl()
		{
			SceneSaveLoadControl.OnSceneSaved += OnSceneSaved;
			SceneSaveLoadControl.OnSceneLoaded += OnSceneLoaded;
		}

		private void CleanUp()
		{
			SceneSaveLoadControl.OnSceneSaved -= OnSceneSaved;
			SceneSaveLoadControl.OnSceneLoaded -= OnSceneLoaded;
		}

		private void OnSceneSaved()
		{
		}

		private void OnSceneLoaded()
		{
		}
	}
}
