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
		private Rect m_InsertionDrawRect;
		private int m_Selected = -1;
		private int m_Grabbed = -1;
		private int m_InsertionIndex = -1;
		private bool m_ContextClick = false;
		private bool m_DragOwner = false;
		private bool m_DragInsert = false;
		private Vector2 m_GrabPosition = Vector2.zero;
		
		private GUIContent m_MenuPingLink;
		private GUIContent m_MenuSetAsSelection;
		private GUIContent m_MenuAddToSelection;
		private GUIContent m_MenuOpenLink;
		private GUIContent m_MenuRemoveLink;

		private EditorWindow m_Window;


		public bool IsDragOwner { get { return m_DragOwner; } }

		
		public override void OnWindowEnable(EditorWindow window)
		{
			m_MenuPingLink = new GUIContent(ResLoad.Instance.GetText(ResId.MenuContextPingLink));
			m_MenuSetAsSelection = new GUIContent(ResLoad.Instance.GetText(ResId.MenuContextSetAsSelection));
			m_MenuAddToSelection = new GUIContent(ResLoad.Instance.GetText(ResId.MenuContextAddToSelection));
			m_MenuOpenLink = new GUIContent(ResLoad.Instance.GetText(ResId.MenuContextOpenLink));
			m_MenuRemoveLink = new GUIContent(ResLoad.Instance.GetText(ResId.MenuContextRemoveLink));

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

			//if the vertical scrollbar is visible, adjust view rect
			//	width by the width of the scrollbar (17.0f)
			if (m_ScrollViewRect.height > m_DrawRect.height)
				m_ScrollViewRect.width = m_DrawRect.width - 17.0f;
			else
				m_ScrollViewRect.width = m_DrawRect.width;

			m_ScrollViewPosition = GUI.BeginScrollView(m_DrawRect, m_ScrollViewPosition, m_ScrollViewRect);

			#region Event Switch
			switch (Event.current.type)
			{
			case EventType.MouseDown:
				{
					OnMouseDown();
				}
				break;
			//not raised during DragAndDrop operation
			case EventType.MouseUp:
				{
					OnMouseUp();
				}
				break;
			//MouseDrag for inter-/intra-window dragging
			case EventType.MouseDrag:
				{
					OnMouseDrag(projectLinks);
				}
				break;
			case EventType.DragUpdated:
				{
					OnDragUpdated(projectLinks);
				}
				break;
			case EventType.DragPerform:
				{
					OnDragPerform();
				}
				break;
			case EventType.DragExited:
				{
					OnDragExited();
				}
				break;
			case EventType.Repaint:
				{
					OnRepaint(projectLinks);
				}
				break;
			}
			#endregion

			GUI.EndScrollView(true);
		}

		private void OnMouseDown()
		{
			Event currentEvent = Event.current;

			if (currentEvent.button != 2)
			{
				//TODO: handle multiple selection

				int hit = JumpLinks.Instance.ProjectLinkHitTest(currentEvent.mousePosition);
				if (currentEvent.button == 0)
				{
					if (hit != m_Selected)
						m_Window.Repaint();

					m_Selected = hit;
					m_Grabbed = hit;
					m_GrabPosition = currentEvent.mousePosition;

					//on double-click
					if (currentEvent.clickCount == 2 && m_Selected > -1)
						OpenAssets();

					currentEvent.Use();
				}
				else if (currentEvent.button == 1)
				{
					if (hit != m_Selected)
						m_Window.Repaint();

					m_Selected = hit;
					m_ContextClick = true;

					currentEvent.Use();
				}
			}
		}

		private void OnMouseUp()
		{
			if (m_Selected > -1)
			{
				m_Grabbed = -1;

				Event currentEvent = Event.current;
				if (m_ContextClick && currentEvent.button == 1)
				{
					ShowContextMenu();

					currentEvent.Use();
				}

				m_ContextClick = false;

				//if (!currentEvent.control && !currentEvent.command)
				//{

				//}
				//else
				//{
				//	//TODO: handle multiple selection
				//}
			}
		}

		private void OnMouseDrag(List<ProjectJumpLink> projectLinks)
		{
			Event currentEvent = Event.current;
			if (m_ScrollViewRect.Contains(currentEvent.mousePosition))
			{
				if (m_Grabbed > -1 && Vector2.Distance(m_GrabPosition, currentEvent.mousePosition) > 4.0f)
					//NOTE: this was preventing a drag to other windows when there was only one link in the list
					//!(projectLinks[m_Grabbed].Area.RectInternal.Contains(currentEvent.mousePosition)))
				{
					m_DragOwner = true;

					Debug.Log("Start Drag");
					DragAndDrop.objectReferences = new Object[] { projectLinks[m_Grabbed].LinkReference };
					DragAndDrop.StartDrag("Project Reference(s)");
					//NOTE: tried to set the visual mode here. always got reset to none.

					currentEvent.Use();
				}
			}
		}

		private void OnDragUpdated(List<ProjectJumpLink> projectLinks)
		{
			if (m_DragOwner)
			{
				Event currentEvent = Event.current;
				//Debug.Log(currentEvent.type + ", project view");

				if (m_ScrollViewRect.Contains(currentEvent.mousePosition))
				{
					DragAndDrop.visualMode = DragAndDropVisualMode.Move;

					m_DragInsert = true;

					int hit = JumpLinks.Instance.ProjectLinkHitTest(currentEvent.mousePosition);
					if (hit > -1)
					{
						m_InsertionIndex = hit;

						m_InsertionDrawRect = projectLinks[hit].Area;
						m_InsertionDrawRect.x += 8.0f;
						m_InsertionDrawRect.width -= 8.0f;
						m_InsertionDrawRect.height = GraphicAssets.Instance.DragDropInsertionStyle.fixedHeight;

						if ((currentEvent.mousePosition.y - m_InsertionDrawRect.y) < (GraphicAssets.LinkHeight * 0.5f))
						{
							m_InsertionIndex = hit;
							m_InsertionDrawRect.y -= GraphicAssets.LinkHeight;
						}
						else
						{
							m_InsertionIndex++;
						}
					}
				}
				else
				{
					DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
					m_DragInsert = false;
				}

				currentEvent.Use();
			}
		}

		private void OnDragPerform()
		{
			Event currentEvent = Event.current;
			if (m_DragOwner && m_ScrollViewRect.Contains(currentEvent.mousePosition))
			{
				DragAndDrop.AcceptDrag();

				JumpLinks.Instance.MoveProjectLink(m_Grabbed, m_InsertionIndex);

				m_DragInsert = false;
				m_DragOwner = false;
				m_Grabbed = -1;
				m_InsertionIndex = -1;

				currentEvent.Use();

				m_Window.Repaint();
			}
		}

		private void OnDragExited()
		{
			Debug.Log(Event.current.type);

			m_DragInsert = false;
			m_DragOwner = false;
			m_Grabbed = -1;
			m_InsertionIndex = -1;
		}

		private void OnRepaint(List<ProjectJumpLink> projectLinks)
		{
			//draw inside of scroll view
			m_DrawRect.Set(0.0f, 0.0f, m_ScrollViewRect.width, GraphicAssets.LinkHeight);

			for (int i = 0; i < projectLinks.Count; i++)
			{
				GraphicAssets.Instance.LinkLabelStyle.normal.textColor = projectLinks[i].LinkColor;

				projectLinks[i].Area.width = m_ScrollViewRect.width;

				if (m_Selected > -1 && m_Selected == i)
					GraphicAssets.Instance.LinkLabelStyle.Draw(m_DrawRect, projectLinks[i].LinkLabelContent, false, false, true, m_Window == EditorWindow.focusedWindow);
				else
					GraphicAssets.Instance.LinkLabelStyle.Draw(m_DrawRect, projectLinks[i].LinkLabelContent, false, false, false, false);

				m_DrawRect.y += m_DrawRect.height;
			}

			if (m_DragInsert && m_InsertionIndex > -1)
			{
				GraphicAssets.Instance.DragDropInsertionStyle.Draw(m_InsertionDrawRect, false, false, false, false);
			}
		}

		private void ShowContextMenu()
		{
			GenericMenu menu = new GenericMenu();
			//TODO: if multiple selection, change to "Remove Links"
			menu.AddItem(m_MenuPingLink, false, PingSelectedLink);
			menu.AddItem(m_MenuSetAsSelection, false, SetAsSelection);
			menu.AddItem(m_MenuAddToSelection, false, AddToSelection);
			menu.AddItem(m_MenuOpenLink, false, OpenAssets);
			menu.AddSeparator(string.Empty);
			menu.AddItem(m_MenuRemoveLink, false, RemoveSelected);

			menu.ShowAsContext();
		}

		private void RemoveSelected()
		{
			JumpLinks.Instance.RemoveProjectLink(m_Selected);
			m_Selected = -1;
		}

		private void PingSelectedLink()
		{
			EditorGUIUtility.PingObject(JumpLinks.Instance.ProjectLinks[m_Selected].LinkReference);
		}

		private void SetAsSelection()
		{
			Object[] selection = { JumpLinks.Instance.ProjectLinks[m_Selected].LinkReference };
			Selection.objects = selection;
		}

		private void AddToSelection()
		{
			Object[] selection = Selection.objects;

			//TODO: filter out links that are already selected

			Object[] addSelected = new Object[selection.Length + 1];
			selection.CopyTo(addSelected, 0);
			addSelected[selection.Length] = JumpLinks.Instance.ProjectLinks[m_Selected].LinkReference;
			Selection.objects = addSelected;
		}

		private void OpenAssets()
		{
			AssetDatabase.OpenAsset(JumpLinks.Instance.ProjectLinks[m_Selected].LinkReference);
		}
	}
}
