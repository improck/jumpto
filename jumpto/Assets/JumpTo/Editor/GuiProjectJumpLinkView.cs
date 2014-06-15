using UnityEditor;
using UnityEngine;
using System.Collections.Generic;


namespace JumpTo
{
	public class GuiProjectJumpLinkView : GuiBase
	{
		[SerializeField] private Vector2 m_ScrollViewPosition;

		private Rect m_ScrollViewRect;
		private Rect m_DrawRect;


		protected override void OnGui()
		{
			List<ProjectJumpLink> projectLinks = JumpLinks.Instance.ProjectLinks;

			m_DrawRect.Set(1.0f, 1.0f, m_Size.x - 2.0f, m_Size.y - 1.0f);

			GUI.Box(m_DrawRect, ResLoad.Instance.GetText(ResId.LabelProjectLinks), GraphicAssets.Instance.LinkBoxStyle);

			m_DrawRect.x = 2.0f;
			m_DrawRect.y += 17.0f;
			m_DrawRect.width = m_Size.x - m_DrawRect.x * 2.0f;
			m_DrawRect.height = m_Size.y - (m_DrawRect.y + 1.0f);

			m_ScrollViewRect.height = projectLinks.Count * GraphicAssets.LinkHeight;

			if (m_ScrollViewRect.height > m_DrawRect.height)
				m_ScrollViewRect.width = m_DrawRect.width - 17.0f;	//width of the scrollbar
			else
				m_ScrollViewRect.width = m_DrawRect.width;

			m_ScrollViewPosition = GUI.BeginScrollView(m_DrawRect, m_ScrollViewPosition, m_ScrollViewRect);

			//draw inside of scroll view
			m_DrawRect.Set(0.0f, 0.0f, m_ScrollViewRect.width, GraphicAssets.LinkHeight);

			for (int i = 0; i < projectLinks.Count; i++)
			{
				GraphicAssets.Instance.LinkLabelStyle.normal.textColor = projectLinks[i].LinkColor;
				GUI.Label(m_DrawRect, projectLinks[i].LinkLabelContent, GraphicAssets.Instance.LinkLabelStyle);

				projectLinks[i].Visible = true;
				projectLinks[i].Area.Set(m_DrawRect.x + 16.0f, m_DrawRect.y, m_DrawRect.width - 16.0f, m_DrawRect.height);

				m_DrawRect.y += m_DrawRect.height;
			}

			GUI.EndScrollView(true);
		}
	}
}
