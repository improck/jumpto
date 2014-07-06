using UnityEditor;
using UnityEngine;
using System.Collections.Generic;


namespace JumpTo
{
	public class GuiHierarchyJumpLinkView : GuiBase
	{
		[SerializeField] private Vector2 m_ScrollViewPosition;

		private Rect m_ScrollViewRect;
		private Rect m_DrawRect;
		private int m_Selected = -1;
		private bool m_ContextClick = false;

		private GUIContent m_MenuSetAsSelection;
		private GUIContent m_MenuAddToSelection;
		private GUIContent m_MenuFrameLink;
		private GUIContent m_MenuRemoveLink;

		private EditorWindow m_Window;


		public override void OnWindowEnable(EditorWindow window)
		{
			m_MenuSetAsSelection = new GUIContent(ResLoad.Instance.GetText(ResId.MenuContextSetAsSelection));
			m_MenuAddToSelection = new GUIContent(ResLoad.Instance.GetText(ResId.MenuContextAddToSelection));
			m_MenuFrameLink = new GUIContent(ResLoad.Instance.GetText(ResId.MenuContextFrameLink));
			m_MenuRemoveLink = new GUIContent(ResLoad.Instance.GetText(ResId.MenuContextRemoveLink));

			m_Window = window;
		}

		protected override void OnGui()
		{
			List<HierarchyJumpLink> hierarchyLinks = JumpLinks.Instance.HierarchyLinks;

			m_DrawRect.Set(1.0f, 1.0f, m_Size.x - 2.0f, m_Size.y - 1.0f);

			GUI.Box(m_DrawRect, ResLoad.Instance.GetText(ResId.LabelHierarchyLinks), GraphicAssets.Instance.LinkBoxStyle);

			m_DrawRect.x = 2.0f;
			m_DrawRect.y += 17.0f;
			m_DrawRect.width = m_Size.x - m_DrawRect.x * 2.0f;
			m_DrawRect.height = m_Size.y - (m_DrawRect.y + 1.0f);

			m_ScrollViewRect.height = hierarchyLinks.Count * GraphicAssets.LinkHeight;

			if (m_ScrollViewRect.height > m_DrawRect.height)
				m_ScrollViewRect.width = m_DrawRect.width - 17.0f;	//width of the scrollbar
			else
				m_ScrollViewRect.width = m_DrawRect.width;

			m_ScrollViewPosition = GUI.BeginScrollView(m_DrawRect, m_ScrollViewPosition, m_ScrollViewRect);

			Event currentEvent = Event.current;
			switch (currentEvent.type)
			{
			case EventType.MouseDown:
				{
					if (currentEvent.button != 2)
					{
						m_Selected = -1;

						for (int i = 0; i < hierarchyLinks.Count; i++)
						{
							if (hierarchyLinks[i].Area.RectInternal.Contains(currentEvent.mousePosition))
							{
								m_Selected = i;

								//on right mouse down
								if (currentEvent.button == 1)
								{
									m_ContextClick = true;
								}

								if (currentEvent.clickCount == 2)
								{
									FrameLink();
								}

								currentEvent.Use();

								break;
							}
						}
					}

					m_Window.Repaint();
				}
				break;
			case EventType.MouseUp:
				{
					if (m_Selected > -1)
					{
						if (m_ContextClick)
						{
							m_ContextClick = false;
							ShowContextMenu();
						}

						//if (!currentEvent.control && !currentEvent.command)
						//{

						//}
						//else
						//{
						//	//TODO: handle multiple selection
						//}

						currentEvent.Use();
						m_Window.Repaint();
					}
				}
				break;
			case EventType.Repaint:
				{
					//draw inside of scroll view
					m_DrawRect.Set(0.0f, 0.0f, m_ScrollViewRect.width, GraphicAssets.LinkHeight);

					for (int i = 0; i < hierarchyLinks.Count; i++)
					{
						GraphicAssets.Instance.LinkLabelStyle.normal.textColor = hierarchyLinks[i].LinkColor;

						//TODO: move this to JumpLinks
						hierarchyLinks[i].Area.Set(m_DrawRect.x, m_DrawRect.y, m_DrawRect.width - 16.0f, m_DrawRect.height);

						if (m_Selected > -1 && m_Selected == i)
							GraphicAssets.Instance.LinkLabelStyle.Draw(m_DrawRect, hierarchyLinks[i].LinkLabelContent, false, false, true, m_Window == EditorWindow.focusedWindow);
						else
							GraphicAssets.Instance.LinkLabelStyle.Draw(m_DrawRect, hierarchyLinks[i].LinkLabelContent, false, false, false, false);

						m_DrawRect.y += m_DrawRect.height;
					}
				}
				break;
			}

			GUI.EndScrollView(true);
		}

		private void ShowContextMenu()
		{
			GenericMenu menu = new GenericMenu();
			//TODO: if multiple selection, change to "Remove Links"
			menu.AddItem(m_MenuSetAsSelection, false, SetAsSelection);
			menu.AddItem(m_MenuAddToSelection, false, AddToSelection);
			
			if (ValidateSceneView())
				menu.AddItem(m_MenuFrameLink, false, FrameLink);
			else
				menu.AddDisabledItem(m_MenuFrameLink);

			menu.AddSeparator(string.Empty);

			menu.AddItem(m_MenuRemoveLink, false, RemoveSelected);

			menu.ShowAsContext();
		}

		private void RemoveSelected()
		{
			JumpLinks.Instance.RemoveHierarchyLink(m_Selected);
			m_Selected = -1;
		}

		private void SetAsSelection()
		{
			Object[] selection = { JumpLinks.Instance.HierarchyLinks[m_Selected].LinkReference };
			Selection.objects = selection;
		}

		private void AddToSelection()
		{
			Object[] selection = Selection.objects;

			//TODO: filter out links that are already selected

			Object[] addSelected = new Object[selection.Length + 1];
			selection.CopyTo(addSelected, 0);
			addSelected[selection.Length] = JumpLinks.Instance.HierarchyLinks[m_Selected].LinkReference;
			Selection.objects = addSelected;
		}

		private void FrameLink()
		{
			SceneView sceneView = SceneView.lastActiveSceneView;
			if (sceneView == null)
				sceneView = SceneView.currentDrawingSceneView;

			if (sceneView != null)
			{
				Object[] selection = Selection.objects;
				Object[] tempSelection = { JumpLinks.Instance.HierarchyLinks[m_Selected].LinkReference };
				Selection.objects = tempSelection;
				sceneView.FrameSelected();
				Selection.objects = selection;
			}
		}

		private bool ValidateSceneView()
		{
			return SceneView.lastActiveSceneView != null || SceneView.currentDrawingSceneView != null;
		}
	}
}

