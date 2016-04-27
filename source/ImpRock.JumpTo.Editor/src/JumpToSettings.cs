using UnityEditor;
using UnityEngine;
using System.IO;


namespace JumpTo
{
	internal sealed class JumpToSettings : EditorScriptableObject<JumpToSettings>
	{
		[System.Serializable]
		internal enum VisibleList
		{
			ProjectOnly,
			HierarchyOnly,
			ProjectAndHierarchy
		}


		[SerializeField] private VisibleList m_VisibleList = VisibleList.ProjectAndHierarchy;
		[SerializeField] private bool m_ProjectFirst = true;
		[SerializeField] private bool m_Vertical = true;
		[SerializeField] private float m_DividerPosition = -1.0f;


		public VisibleList Visibility { get { return m_VisibleList; } set { m_VisibleList = value; } }
		public bool ProjectFirst { get { return m_ProjectFirst; } set { m_ProjectFirst = value; } }
		public bool Vertical { get { return m_Vertical; } set { m_Vertical = value; } }
		public float DividerPosition { get { return m_DividerPosition; } set { m_DividerPosition = value; } }
	}
}
