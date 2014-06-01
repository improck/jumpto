using UnityEditor;
using UnityEngine;
using System.Collections;


namespace JumpTo
{
	public class GuiJumpLinkListView : GuiJumpLinkView
	{
		[SerializeField] private bool m_ProjectLinksUnfolded = true;
		[SerializeField] private bool m_HierarchyLinksUnfolded = true;


		public override void OnGui()
		{
			Vector2 iconSizeBak = EditorGUIUtility.GetIconSize();
			EditorGUIUtility.SetIconSize(GraphicAssets.Instance.IconSize);

			Color bgColor = GUI.backgroundColor;

			JumpLinks jumpLinks = JumpLinks.Instance;

			m_ProjectLinksUnfolded = EditorGUILayout.Foldout(m_ProjectLinksUnfolded, "Project References");
			if (m_ProjectLinksUnfolded)
			{
				for (int i = 0; i < jumpLinks.ProjectLinks.Count; i++)
				{
					GUILayout.BeginHorizontal();
					GUILayout.Space(17.0f);

					if (jumpLinks.ProjectLinks == null) { Debug.LogError("project links is null"); GUILayout.EndHorizontal(); continue; }
					else if (jumpLinks.ProjectLinks[i] == null) { Debug.LogError("project links i is null: " + i); GUILayout.EndHorizontal(); continue; }
					else if (GraphicAssets.Instance.LinkLabelStyle == null) { Debug.LogError("label style is null"); GUILayout.EndHorizontal(); continue; }
					GUILayout.Label(jumpLinks.ProjectLinks[i].LinkLabelContent, GraphicAssets.Instance.LinkLabelStyle, GUILayout.Height(JumpLinks.LinkHeight));
					jumpLinks.ProjectLinks[i].Area = GUILayoutUtility.GetLastRect();

					GUILayout.EndHorizontal();
				}
			}

			EditorGUILayout.Separator();

			m_HierarchyLinksUnfolded = EditorGUILayout.Foldout(m_HierarchyLinksUnfolded, "Hierarchy References");
			if (m_HierarchyLinksUnfolded)
			{
				for (int i = 0; i < jumpLinks.HierarchyLinks.Count; i++)
				{
					GUILayout.BeginHorizontal();
					GUILayout.Space(17.0f);

					if (jumpLinks.HierarchyLinks == null) { Debug.LogError("hierarchy links is null"); GUILayout.EndHorizontal(); continue; }
					else if (jumpLinks.HierarchyLinks[i] == null) { Debug.LogError("hierarchy links i is null: " + i); GUILayout.EndHorizontal(); continue; }
					else if (GraphicAssets.Instance.LinkLabelStyle == null) { Debug.LogError("label style is null"); GUILayout.EndHorizontal(); continue; }
					GUILayout.Label(jumpLinks.HierarchyLinks[i].LinkLabelContent, GraphicAssets.Instance.LinkLabelStyle, GUILayout.Height(JumpLinks.LinkHeight));
					jumpLinks.HierarchyLinks[i].Area = GUILayoutUtility.GetLastRect();

					GUILayout.EndHorizontal();
				}
			}

			GUI.backgroundColor = bgColor;
			EditorGUIUtility.SetIconSize(iconSizeBak);
		}
	}
}
