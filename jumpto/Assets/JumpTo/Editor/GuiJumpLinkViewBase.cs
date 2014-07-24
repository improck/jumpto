using UnityEditor;
using UnityEngine;
using System.Collections.Generic;


namespace JumpTo
{
	public abstract class GuiJumpLinkViewBase<T> : GuiBase where T : JumpLink
	{
		[SerializeField] protected Vector2 m_ScrollViewPosition;
		[SerializeField] protected bool m_HasFocus = false;

		protected Rect m_DrawRect;
		protected Rect m_ScrollViewRect;
		protected Rect m_InsertionDrawRect;
		protected Rect m_ControlRect;
		protected int m_Grabbed = -1;
		protected int m_InsertionIndex = -1;
		protected bool m_ContextClick = false;
		protected bool m_DragOwner = false;
		protected bool m_DragInsert = false;
		protected Vector2 m_GrabPosition = Vector2.zero;

		protected GUIContent m_ControlTitle;
		protected GUIContent m_MenuPingLink;
		protected GUIContent m_MenuSetAsSelection;
		protected GUIContent m_MenuAddToSelection;
		protected GUIContent m_MenuRemoveLink;
		protected GUIContent m_MenuRemoveAll;
		protected GUIContent m_MenuSetAsSelectionPlural;
		protected GUIContent m_MenuAddToSelectionPlural;
		protected GUIContent m_MenuRemoveLinkPlural;

		protected EditorWindow m_Window;

		protected JumpLinkContainer<T> m_LinkContainer = null;


		public bool IsDragOwner { get { return m_DragOwner; } }
		public bool HasFocus { get { return m_HasFocus; } set { m_HasFocus = value; } }
		public bool IsFocusedControl { get { return m_Window == EditorWindow.focusedWindow && m_HasFocus; } }


		protected abstract void ShowContextMenu();
		protected abstract void OnDoubleClick();


		public override void OnWindowEnable(EditorWindow window)
		{
			m_MenuPingLink = new GUIContent(ResLoad.Instance.GetText(ResId.MenuContextPingLink));
			m_MenuSetAsSelection = new GUIContent(ResLoad.Instance.GetText(ResId.MenuContextSetAsSelection));
			m_MenuAddToSelection = new GUIContent(ResLoad.Instance.GetText(ResId.MenuContextAddToSelection));
			m_MenuRemoveLink = new GUIContent(ResLoad.Instance.GetText(ResId.MenuContextRemoveLink));
			m_MenuRemoveAll = new GUIContent(ResLoad.Instance.GetText(ResId.MenuContextRemoveAll));
			m_MenuSetAsSelectionPlural = new GUIContent(ResLoad.Instance.GetText(ResId.MenuContextSetAsSelectionPlural));
			m_MenuAddToSelectionPlural = new GUIContent(ResLoad.Instance.GetText(ResId.MenuContextAddToSelectionPlural));
			m_MenuRemoveLinkPlural = new GUIContent(ResLoad.Instance.GetText(ResId.MenuContextRemoveLinkPlural));

			m_Window = window;

			m_LinkContainer = JumpLinks.Instance.GetJumpLinkContainer<T>();
		}

		protected override void OnGui()
		{
			m_ControlRect.width = m_Size.x;
			m_ControlRect.height = m_Size.y;

			List<T> links = m_LinkContainer.Links;

			m_DrawRect.Set(1.0f, 1.0f, m_Size.x - 2.0f, m_Size.y - 1.0f);

			GUI.Box(m_DrawRect, m_ControlTitle, GraphicAssets.Instance.LinkBoxStyle);

			//HACK: just testing the remove all feature
			if (Event.current.type == EventType.MouseUp &&
				Event.current.button == 1 &&
				m_DrawRect.Contains(Event.current.mousePosition) &&
				Event.current.mousePosition.y < 17.0f)
			{
				ShowTitleContextMenu();
				Event.current.Use();
			}

			m_DrawRect.x = 2.0f;
			m_DrawRect.y += 17.0f;
			m_DrawRect.width = m_Size.x - m_DrawRect.x * 2.0f;
			m_DrawRect.height = m_Size.y - (m_DrawRect.y + 1.0f);

			m_ScrollViewRect.height = links.Count * GraphicAssets.LinkHeight;

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
					OnMouseDrag(links);
				}
				break;
			case EventType.DragUpdated:
				{
					OnDragUpdated(links);
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
					OnRepaint(links);
				}
				break;
			}
			#endregion

			GUI.EndScrollView(true);
		}

		protected void OnMouseDown()
		{
			Event currentEvent = Event.current;

			if (!m_ControlRect.Contains(currentEvent.mousePosition))
				return;

			if (!m_HasFocus)
				m_Window.Repaint();

			m_HasFocus = true;

			if (currentEvent.button != 2)
			{
				//determine where the mouse clicked
				int hit = m_LinkContainer.LinkHitTest(currentEvent.mousePosition);
				if (currentEvent.button == 0)
				{
					//if links are currently selected
					if (m_LinkContainer.HasSelection)
					{
						//and the click was not on a link (below last link)
						if (hit == -1)
						{
							//clear selection
							m_LinkContainer.LinkSelectionClear();
							m_Window.Repaint();
						}
						//or the click was on a link and the control/command key was down
						else if (currentEvent.control || currentEvent.command)
						{
							//toggle clicked link selection state
							if (!m_LinkContainer[hit].Selected)
								m_LinkContainer.LinkSelectionAdd(hit);
							else
								m_LinkContainer.LinkSelectionRemove(hit);

							m_Window.Repaint();
						}
						//or the click was on a link and the shift key was down
						else if (currentEvent.shift)
						{
							m_LinkContainer.LinkSelectionSetRange(m_LinkContainer.ActiveSelection, hit);
							m_Window.Repaint();
						}
						//or the clicked link was not already selected
						else if (!m_LinkContainer[hit].Selected)
						{
							//set the selection to the clicked link
							m_LinkContainer.LinkSelectionSet(hit);
							m_Window.Repaint();
						}
					}
					else if (hit > -1)
					{
						//set the selection to the clicked link
						m_LinkContainer.LinkSelectionSet(hit);
						m_Window.Repaint();
					}

					m_Grabbed = hit;
					//NOTE: this may need to be set to the midline of the grabbed link
					m_GrabPosition = currentEvent.mousePosition;

					//on double-click
					if (currentEvent.clickCount == 2 && hit > -1 && m_LinkContainer[hit].Selected)
						OnDoubleClick();

					currentEvent.Use();
				}
				else if (currentEvent.button == 1)
				{
					//if links are currently selected
					if (m_LinkContainer.HasSelection)
					{
						//and the click was not on a link (below last link)
						if (hit == -1)
						{
							//m_LinkContainer.LinkSelectionClear();
							//TODO: show a new context menu

							//m_Window.Repaint();
						}
						//or the link that was clicked was already selected
						else if (m_LinkContainer[hit].Selected)
						{
							m_LinkContainer.ActiveSelection = hit;
						}
						//or a new link was selected
						else
						{
							m_LinkContainer.LinkSelectionSet(hit);
							m_Window.Repaint();
						}
					}
					//no links are selected, if a link was clicked
					else if (hit > -1)
					{
						m_LinkContainer.LinkSelectionSet(hit);
						m_Window.Repaint();
					}

					m_ContextClick = true;

					currentEvent.Use();
				}
			}
		}

		protected void OnMouseUp()
		{
			Event currentEvent = Event.current;

			if (m_LinkContainer.HasSelection)
			{
				m_Grabbed = -1;

				if (currentEvent.button == 1 && m_ContextClick)
				{
					ShowContextMenu();

					currentEvent.Use();
				}

				m_ContextClick = false;
			}
		}

		protected void OnMouseDrag(List<T> links)
		{
			Event currentEvent = Event.current;
			if (m_ScrollViewRect.Contains(currentEvent.mousePosition))
			{
				if (m_Grabbed > -1 && Vector2.Distance(m_GrabPosition, currentEvent.mousePosition) > 4.0f)
				//NOTE: this was preventing a drag to other windows when there was only one link in the list
				//	!(links[m_Grabbed].Area.RectInternal.Contains(currentEvent.mousePosition)))
				{
					m_DragOwner = true;

					Debug.Log("Start Drag");
					DragAndDrop.objectReferences = m_LinkContainer.SelectedLinkReferences;
					DragAndDrop.StartDrag("Project Reference(s)");
					//NOTE: tried to set the visual mode here. always got reset to none.

					currentEvent.Use();
				}
			}
		}

		protected void OnDragUpdated(List<T> links)
		{
			if (m_DragOwner)
			{
				Event currentEvent = Event.current;
				//Debug.Log(currentEvent.type + ", project view");

				if (m_ScrollViewRect.Contains(currentEvent.mousePosition))
				{
					DragAndDrop.visualMode = DragAndDropVisualMode.Move;

					m_DragInsert = true;

					int hit = m_LinkContainer.LinkHitTest(currentEvent.mousePosition);
					if (hit > -1)
					{
						m_InsertionIndex = hit;

						m_InsertionDrawRect = links[hit].Area;
						m_InsertionDrawRect.x += 8.0f;
						m_InsertionDrawRect.width -= 8.0f;
						m_InsertionDrawRect.height = GraphicAssets.Instance.DragDropInsertionStyle.fixedHeight;

						if ((currentEvent.mousePosition.y - m_InsertionDrawRect.y) < (GraphicAssets.LinkHeight * 0.5f))
						{
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

		protected void OnDragPerform()
		{
			Event currentEvent = Event.current;
			if (m_DragOwner && m_ScrollViewRect.Contains(currentEvent.mousePosition))
			{
				DragAndDrop.AcceptDrag();

				m_LinkContainer.MoveSelected(m_InsertionIndex);

				m_DragInsert = false;
				m_DragOwner = false;
				m_Grabbed = -1;
				m_InsertionIndex = -1;

				currentEvent.Use();

				m_Window.Repaint();
			}
		}

		protected void OnDragExited()
		{
			m_DragInsert = false;
			m_DragOwner = false;
			m_Grabbed = -1;
			m_InsertionIndex = -1;
		}

		protected void OnRepaint(List<T> links)
		{
			//draw inside of scroll view
			m_DrawRect.Set(0.0f, 0.0f, m_ScrollViewRect.width, GraphicAssets.LinkHeight);

			for (int i = 0; i < links.Count; i++)
			{
				GraphicAssets.Instance.LinkLabelStyle.normal.textColor = links[i].LinkColor;

				links[i].Area.width = m_ScrollViewRect.width;

				if (links[i].Selected)
					GraphicAssets.Instance.LinkLabelStyle.Draw(m_DrawRect, links[i].LinkLabelContent, false, false, true, IsFocusedControl);
				else
					GraphicAssets.Instance.LinkLabelStyle.Draw(m_DrawRect, links[i].LinkLabelContent, false, false, false, false);

				m_DrawRect.y += m_DrawRect.height;
			}

			if (m_DragInsert && m_InsertionIndex > -1)
			{
				GraphicAssets.Instance.DragDropInsertionStyle.Draw(m_InsertionDrawRect, false, false, false, false);
			}
		}

		protected void ShowTitleContextMenu()
		{
			if (m_LinkContainer.Links.Count > 0)
			{
				GenericMenu menu = new GenericMenu();

				menu.AddItem(m_MenuRemoveAll, false, RemoveAll);

				menu.ShowAsContext();
			}
		}

		protected void RemoveAll()
		{
			//if confirmed, remove all
			if (EditorUtility.DisplayDialog(ResLoad.Instance.GetText(ResId.DialogRemoveAllTitle),
				ResLoad.Instance.GetText(ResId.DialogRemoveAllMessage),
				ResLoad.Instance.GetText(ResId.DialogYes),
				ResLoad.Instance.GetText(ResId.DialogNo)))
			{
				m_LinkContainer.RemoveAll();
			}
		}

		protected void RemoveSelected()
		{
			m_LinkContainer.RemoveSelected();
		}

		protected void PingSelectedLink()
		{
			T activeSelection = m_LinkContainer.ActiveSelectedObject;
			if (activeSelection != null)
				EditorGUIUtility.PingObject(activeSelection.LinkReference);
		}

		protected void SetAsSelection()
		{
			Object[] selectedLinks = m_LinkContainer.SelectedLinkReferences;
			Selection.objects = selectedLinks;
		}

		protected void AddToSelection()
		{
			Object[] selectedLinks = m_LinkContainer.SelectedLinkReferences;

			//NOTE: may or may not be the most efficient way to do this, but
			//		it's easy to read
			if (selectedLinks != null)
			{
				List<Object> selectionAdd = new List<Object>(Selection.objects);
				for (int i = 0; i < selectedLinks.Length; i++)
				{
					if (!selectionAdd.Contains(selectedLinks[i]))
						selectionAdd.Add(selectedLinks[i]);
				}

				Selection.objects = selectionAdd.ToArray();
			}
		}
	}
}
