using UnityEditor;
using UnityEngine;


namespace JumpTo
{
	public class GuiJumpLinkListView : GuiBase
	{
		[SerializeField] private bool m_ProjectLinksUnfolded = true;
		[SerializeField] private bool m_HierarchyLinksUnfolded = true;

		private readonly Vector2 IconSize = new Vector2(16.0f, 16.0f);

		private RectRef m_DrawRect = new RectRef();


		public override void OnGui(RectRef position)
		{
			m_DrawRect.x = position.x;
			m_DrawRect.y = position.y;
			m_DrawRect.width = position.width;
			m_DrawRect.height = 16.0f;

			Vector2 iconSizeBak = EditorGUIUtility.GetIconSize();
			EditorGUIUtility.SetIconSize(IconSize);

			Color bgColorBak = GUI.backgroundColor;

			JumpLinks jumpLinks = JumpLinks.Instance;

			m_ProjectLinksUnfolded = EditorGUI.Foldout(m_DrawRect, m_ProjectLinksUnfolded, "Project References");

			m_DrawRect.y += m_DrawRect.height;

			if (m_ProjectLinksUnfolded)
			{
				if (jumpLinks.ProjectLinks.Count > 0)
				{
					m_DrawRect.height = Mathf.Max(IconSize.y, GraphicAssets.LinkHeight);

					ProjectJumpLink projectLink;

					for (int i = 0; i < jumpLinks.ProjectLinks.Count; i++)
					{
						projectLink = jumpLinks.ProjectLinks[i];
						GraphicAssets.Instance.LinkLabelStyle.normal.textColor = projectLink.LinkColor;
						GUI.Label(m_DrawRect, projectLink.LinkLabelContent, GraphicAssets.Instance.LinkLabelStyle);
					
						projectLink.Visible = true;
						projectLink.Area.Set(m_DrawRect.x + 16.0f, m_DrawRect.y, m_DrawRect.width - 16.0f, m_DrawRect.height);

						m_DrawRect.y += m_DrawRect.height;
					}
				}
			}
			else
			{
				for (int i = 0; i < jumpLinks.ProjectLinks.Count; i++)
				{
					jumpLinks.ProjectLinks[i].Visible = false;
				}
			}

			m_DrawRect.y += 16.0f;
			m_DrawRect.height = 16.0f;

			m_HierarchyLinksUnfolded = EditorGUI.Foldout(m_DrawRect, m_HierarchyLinksUnfolded, "Hierarchy References");

			m_DrawRect.y += m_DrawRect.height;

			if (m_HierarchyLinksUnfolded)
			{
				if (jumpLinks.HierarchyLinks.Count > 0)
				{
					m_DrawRect.height = Mathf.Max(IconSize.y, GraphicAssets.LinkHeight);

					HierarchyJumpLink hierarchyLink;

					for (int i = 0; i < jumpLinks.HierarchyLinks.Count; i++)
					{
						hierarchyLink = jumpLinks.HierarchyLinks[i];
						GraphicAssets.Instance.LinkLabelStyle.normal.textColor = hierarchyLink.LinkColor;
						GUI.Label(m_DrawRect, hierarchyLink.LinkLabelContent, GraphicAssets.Instance.LinkLabelStyle);

						hierarchyLink.Visible = true;
						hierarchyLink.Area.Set(m_DrawRect.x + 16.0f, m_DrawRect.y, m_DrawRect.width - 16.0f, m_DrawRect.height);

						m_DrawRect.y += m_DrawRect.height;
					}
				}
			}
			else
			{
				for (int i = 0; i < jumpLinks.HierarchyLinks.Count; i++)
				{
					jumpLinks.HierarchyLinks[i].Visible = false;
				}
			}

			GUI.backgroundColor = bgColorBak;
			EditorGUIUtility.SetIconSize(iconSizeBak);

			position.y = m_DrawRect.y;

			switch (Event.current.type)
			{
			case EventType.MouseDown:
				{
					ProjectJumpLink projectLink = jumpLinks.ProjectLinkHitTest(Event.current.mousePosition);
					if (projectLink != null)
					{
						Debug.Log("MouseDown: " + projectLink.LinkReference.name);
					}
				}
				break;
			}
		}
	}
}
