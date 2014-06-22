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
		private int m_Selected = -1;

		private EditorWindow m_Window;


		public override void OnWindowEnable(EditorWindow window)
		{
			m_Window = window;
		}

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

			switch (Event.current.type)
			{
			case EventType.MouseDown:
				{
					m_Selected = -1;

					for (int i = 0; i < projectLinks.Count; i++)
					{
						if (projectLinks[i].Area.RectInternal.Contains(Event.current.mousePosition))
						{
							m_Selected = i;
							break;
						}
					}

					m_Window.Repaint();
				}
				break;
			case EventType.MouseUp:
				{
					if (m_Selected > -1)
					{
						if (!Event.current.control && !Event.current.command)
							Selection.activeObject = projectLinks[m_Selected].LinkReference;
						else
						{
							//TODO: handle multiple selection
						}

						m_Window.Repaint();
					}
				}
				break;
			case EventType.Repaint:
				{
					//draw inside of scroll view
					m_DrawRect.Set(0.0f, 0.0f, m_ScrollViewRect.width, GraphicAssets.LinkHeight);

					for (int i = 0; i < projectLinks.Count; i++)
					{
						GraphicAssets.Instance.LinkLabelStyle.normal.textColor = projectLinks[i].LinkColor;

						projectLinks[i].Area.Set(m_DrawRect.x, m_DrawRect.y, m_DrawRect.width - 16.0f, m_DrawRect.height);

						if (m_Selected > -1 && m_Selected == i)
							GraphicAssets.Instance.LinkLabelStyle.Draw(m_DrawRect, projectLinks[i].LinkLabelContent, false, false, true, m_Window == EditorWindow.focusedWindow);
						else
							GraphicAssets.Instance.LinkLabelStyle.Draw(m_DrawRect, projectLinks[i].LinkLabelContent, false, false, false, false);

						m_DrawRect.y += m_DrawRect.height;
					}
				}
				break;
			}

			GUI.EndScrollView(true);
		}
	}
}
