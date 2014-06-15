using UnityEditor;
using UnityEngine;


namespace JumpTo
{
	public class GuiJumpLinkListView : GuiBase
	{
		[SerializeField] private float m_Divider = 0.5f;
		[SerializeField] private GuiProjectJumpLinkView m_ProjectView;
		[SerializeField] private GuiHierarchyJumpLinkView m_HierarchyView;

		private RectRef m_DrawRect = new RectRef();
		private readonly Vector2 IconSize = new Vector2(16.0f, 16.0f);


		public override void OnWindowEnable()
		{
			if (m_ProjectView == null)
			{
				m_ProjectView = GuiProjectJumpLinkView.Create();
			}

			if (m_HierarchyView == null)
			{
				m_HierarchyView = GuiHierarchyJumpLinkView.Create();
			}

			m_ProjectView.OnWindowEnable();
			m_HierarchyView.OnWindowEnable();
		}

		protected override void OnGui()
		{
			Vector2 iconSizeBak = EditorGUIUtility.GetIconSize();
			EditorGUIUtility.SetIconSize(IconSize);

			Color bgColorBak = GUI.backgroundColor;

			switch (JumpToSettings.Instance.Visibility)
			{
			case JumpToSettings.VisibleList.ProjectAndHierarchy:
				{
					//draw the top/left box
					if (JumpToSettings.Instance.Vertical)
						m_DrawRect.Set(0.0f, 0.0f, m_Size.x, Mathf.Floor(m_Size.y * m_Divider));
					else
						m_DrawRect.Set(0.0f, 0.0f, m_Size.x * m_Divider, m_Size.y);

					if (JumpToSettings.Instance.ProjectFirst)
						m_ProjectView.Draw(m_DrawRect);
					else
						m_HierarchyView.Draw(m_DrawRect);
					
					//TODO: draw a divider

					//draw the bottom/right box
					if (JumpToSettings.Instance.Vertical)
						m_DrawRect.Set(0.0f, Mathf.Floor(m_Size.y * m_Divider), m_Size.x, Mathf.Floor(m_Size.y * (1.0f - m_Divider)));
					else
						m_DrawRect.Set(m_Size.x * m_Divider, 0.0f, m_Size.x * (1.0f - m_Divider), m_Size.y);

					if (JumpToSettings.Instance.ProjectFirst)
						m_HierarchyView.Draw(m_DrawRect);
					else
						m_ProjectView.Draw(m_DrawRect);
				}
				break;
			case JumpToSettings.VisibleList.ProjectOnly:
				{
					m_DrawRect.Set(0.0f, 0.0f, m_Size.x, m_Size.y);

					m_ProjectView.Draw(m_DrawRect);
				}
				break;
			case JumpToSettings.VisibleList.HierarchyOnly:
				{
					m_DrawRect.Set(0.0f, 0.0f, m_Size.x, m_Size.y);

					m_HierarchyView.Draw(m_DrawRect);
				}
				break;
			}

			GUI.backgroundColor = bgColorBak;

			EditorGUIUtility.SetIconSize(iconSizeBak);
		}

		public static GuiJumpLinkListView Create()
		{
			GuiJumpLinkListView instance = ScriptableObject.CreateInstance<GuiJumpLinkListView>();
			instance.hideFlags = HideFlags.HideAndDontSave;

			return instance;
		}
	}
}
