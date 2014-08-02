using UnityEditor;
using UnityEngine;
using System.IO;


namespace JumpTo
{
	public class JumpToSettings : ScriptableObject
	{
		#region Pseudo-Singleton
		private static JumpToSettings s_Instance = null;

		public static JumpToSettings Instance { get { return s_Instance; } /*set { s_Instance = value; }*/ }


		protected JumpToSettings() { s_Instance = this; }
		#endregion


		[System.Serializable]
		public enum VisibleList
		{
			ProjectOnly,
			HierarchyOnly,
			ProjectAndHierarchy
		}


		[SerializeField] private VisibleList m_VisibleList = VisibleList.ProjectAndHierarchy;
		[SerializeField] private bool m_ProjectFirst = true;
		[SerializeField] private bool m_Vertical = true;


		public VisibleList Visibility { get { return m_VisibleList; } set { m_VisibleList = value; } }
		public bool ProjectFirst { get { return m_ProjectFirst; } set { m_ProjectFirst = value; } }
		public bool Vertical { get { return m_Vertical; } set { m_Vertical = value; } }


		public static JumpToSettings Create()
		{
			JumpToSettings instance = ScriptableObject.CreateInstance<JumpToSettings>();
			instance.hideFlags = HideFlags.HideAndDontSave;

			return instance;
		}


		public static void Save()
		{
			using (StreamWriter streamWriter = new StreamWriter(Application.dataPath + "\\..\\settings.jumpto"))
			{
				streamWriter.WriteLine(s_Instance.m_VisibleList);
				streamWriter.WriteLine(s_Instance.m_ProjectFirst);
				streamWriter.WriteLine(s_Instance.m_Vertical);
			}
		}

		public static void Load()
		{
			string settingsFilePath = Application.dataPath + "\\..\\settings.jumpto";
			if (!File.Exists(settingsFilePath))
				return;

			using (StreamReader streamReader = new StreamReader(settingsFilePath))
			{
				s_Instance.m_VisibleList = (VisibleList)System.Enum.Parse(typeof(VisibleList), streamReader.ReadLine());
				s_Instance.m_ProjectFirst = bool.Parse(streamReader.ReadLine());
				s_Instance.m_Vertical = bool.Parse(streamReader.ReadLine());
			}
		}
	}
}
