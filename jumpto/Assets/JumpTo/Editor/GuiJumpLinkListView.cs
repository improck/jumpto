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
		private Rect m_DividerRect;

		private const float DividerHalfThickness = 3.0f;
		public const float DividerMin = 74.0f;

		private readonly Vector2 IconSize = new Vector2(16.0f, 16.0f);
		private readonly int DividerHash = "divider".GetHashCode();


		public override void OnWindowEnable(EditorWindow window)
		{
			if (m_ProjectView == null)
			{
				m_ProjectView = GuiBase.Create<GuiProjectJumpLinkView>();
			}

			if (m_HierarchyView == null)
			{
				m_HierarchyView = GuiBase.Create<GuiHierarchyJumpLinkView>();
			}

			//JumpLinks.Instance.RefreshLinksY();

			m_ProjectView.OnWindowEnable(window);
			m_HierarchyView.OnWindowEnable(window);
		}

		protected override void OnGui()
		{
			HandleDragAndDrop();

			Vector2 iconSizeBak = EditorGUIUtility.GetIconSize();
			EditorGUIUtility.SetIconSize(IconSize);

			Color bgColorBak = GUI.backgroundColor;

			switch (JumpToSettings.Instance.Visibility)
			{
			case JumpToSettings.VisibleList.ProjectAndHierarchy:
				{
					float adjWidth = m_Size.x;
					float adjHeight = m_Size.y;

					//draw the top/left box
					if (JumpToSettings.Instance.Vertical)
					{
						adjHeight = Mathf.Floor(m_Size.y * m_Divider);
						m_DrawRect.Set(0.0f, 0.0f, m_Size.x, adjHeight - DividerHalfThickness);

						m_DividerRect.x = 0.0f;
						m_DividerRect.y = m_DrawRect.height;
						m_DividerRect.width = m_Size.x;
						m_DividerRect.height = DividerHalfThickness * 2.0f;
					}
					else
					{
						adjWidth = Mathf.Floor(m_Size.x * m_Divider);
						m_DrawRect.Set(0.0f, 0.0f, adjWidth - DividerHalfThickness, m_Size.y - 1.0f);

						m_DividerRect.x = m_DrawRect.width;;
						m_DividerRect.y = 0.0f;
						m_DividerRect.width = DividerHalfThickness * 2.0f;
						m_DividerRect.height = m_Size.y;
					}

					if (JumpToSettings.Instance.ProjectFirst)
					{
						m_ProjectView.Draw(m_DrawRect);
						if (m_ProjectView.HasFocus)
							m_HierarchyView.HasFocus = false;
					}
					else
					{
						m_HierarchyView.Draw(m_DrawRect);
						if (m_HierarchyView.HasFocus)
							m_ProjectView.HasFocus = false;
					}
					
					//TODO: draw a divider icon

					//draw the bottom/right box
					if (JumpToSettings.Instance.Vertical)
					{
						m_DrawRect.Set(0.0f, adjHeight + DividerHalfThickness, m_Size.x, ((m_Size.y - adjHeight) - DividerHalfThickness) - 1.0f);
					}
					else
					{
						m_DrawRect.Set(adjWidth + DividerHalfThickness, 0.0f, (m_Size.x - adjWidth) - DividerHalfThickness, m_Size.y - 1.0f);
					}

					if (JumpToSettings.Instance.ProjectFirst)
					{
						m_HierarchyView.Draw(m_DrawRect);
						if (m_HierarchyView.HasFocus)
							m_ProjectView.HasFocus = false;
					}
					else
					{
						m_ProjectView.Draw(m_DrawRect);
						if (m_ProjectView.HasFocus)
							m_HierarchyView.HasFocus = false;
					}

					OnDividerGui();
				}
				break;
			case JumpToSettings.VisibleList.ProjectOnly:
				{
					m_DrawRect.Set(0.0f, 0.0f, m_Size.x, m_Size.y - 1.0f);

					m_ProjectView.HasFocus = true;
					m_HierarchyView.HasFocus = false;
					m_ProjectView.Draw(m_DrawRect);
				}
				break;
			case JumpToSettings.VisibleList.HierarchyOnly:
				{
					m_DrawRect.Set(0.0f, 0.0f, m_Size.x, m_Size.y - 1.0f);

					m_ProjectView.HasFocus = false;
					m_HierarchyView.HasFocus = true;
					m_HierarchyView.Draw(m_DrawRect);
				}
				break;
			}

			GUI.backgroundColor = bgColorBak;

			EditorGUIUtility.SetIconSize(iconSizeBak);
		}

		private void OnDividerGui()
		{
			int controlId = GUIUtility.GetControlID(DividerHash, FocusType. Passive, m_DividerRect);

			if (JumpToSettings.Instance.Vertical)
				EditorGUIUtility.AddCursorRect(m_DividerRect, MouseCursor.SplitResizeUpDown, controlId);
			else
				EditorGUIUtility.AddCursorRect(m_DividerRect, MouseCursor.SplitResizeLeftRight, controlId);

			Event current = Event.current;
			switch (current.GetTypeForControl(controlId))
			{
			case EventType.MouseDown:
				{
					if (current.button == 0 && m_DividerRect.Contains(current.mousePosition))
					{
						GUIUtility.hotControl = controlId;
						current.Use();
					}
				}
				break;
			case EventType.MouseUp:
				{
					if (GUIUtility.hotControl == controlId)
					{
						GUIUtility.hotControl = 0;
						current.Use();
					}
				}
				break;
			case EventType.MouseDrag:
				{
					if (GUIUtility.hotControl == controlId && current.button == 0)
					{
						if (JumpToSettings.Instance.Vertical)
						{
							m_Divider = Mathf.Clamp(current.mousePosition.y, DividerMin, m_Size.y - DividerMin) / m_Size.y;
						}
						else
						{
							m_Divider = Mathf.Clamp(current.mousePosition.x, DividerMin, m_Size.x - DividerMin) / m_Size.x;
						}

						current.Use();
					}
				}
				break;
			}
		}

		private void HandleDragAndDrop()
		{
			Event currentEvent = Event.current;
			switch (currentEvent.type)
			{
			//raised repeatedly while a drag op is hovering
			case EventType.DragUpdated:
				{
					if (m_ProjectView.IsDragOwner || m_HierarchyView.IsDragOwner)
						break;

					//drag most like came from another window, figure out
					//	if it can be accepted and where it would go
					if (DragAndDrop.visualMode == DragAndDropVisualMode.None)
					{
						if (DragAndDrop.objectReferences.Length > 0)
						{
							//reject component links
							if (DragAndDrop.objectReferences[0] is Component)
								DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;

							//TODO: component links??

							switch (JumpToSettings.Instance.Visibility)
							{
							case JumpToSettings.VisibleList.ProjectAndHierarchy:
								{
									DragAndDrop.visualMode = DragAndDropVisualMode.Link;
								}
								break;
							case JumpToSettings.VisibleList.HierarchyOnly:
								{
									UnityEngine.Object[] objectReferences = DragAndDrop.objectReferences;
									for (int i = 0; i < objectReferences.Length; i++)
									{
										if (JumpLinks.WouldBeHierarchyLink(objectReferences[i]))
										{
											DragAndDrop.visualMode = DragAndDropVisualMode.Link;
											break;
										}
									}
								}
								break;
							case JumpToSettings.VisibleList.ProjectOnly:
								{
									UnityEngine.Object[] objectReferences = DragAndDrop.objectReferences;
									for (int i = 0; i < objectReferences.Length; i++)
									{
										if (JumpLinks.WouldBeProjectLink(objectReferences[i]))
										{
											DragAndDrop.visualMode = DragAndDropVisualMode.Link;
											break;
										}
									}
								}
								break;
							}	//switch on visibility
						}
					}

					currentEvent.Use();
				}	//case DragUpdated
				break;
			//raised on mouse-up if DragAndDrop.visualMode != None or Rejected
			case EventType.DragPerform:
				{
					if (m_ProjectView.IsDragOwner || m_HierarchyView.IsDragOwner)
						break;

					DragAndDrop.AcceptDrag();

					switch (JumpToSettings.Instance.Visibility)
					{
					case JumpToSettings.VisibleList.ProjectAndHierarchy:
						{
							OnDropProjectAndHierarchy();
						}
						break;
					case JumpToSettings.VisibleList.HierarchyOnly:
						{
							OnDropHierarchyOnly();
						}
						break;
					case JumpToSettings.VisibleList.ProjectOnly:
						{
							OnDropProjectOnly();
						}
						break;
					}

					currentEvent.Use();

					//reset drag & drop data
					DragAndDrop.PrepareStartDrag();
				}	//case DragPerform
				break;
			//raised after DragPerformed OR if escape is pressed;
			//	use for any cleanup of stuff initialized during
			//	DragUpdated event
			//***** case EventType.DragExited: *****
			}
		}

		private void OnDropProjectAndHierarchy()
		{
			UnityEngine.Object[] objectReferences = DragAndDrop.objectReferences;
			if (objectReferences.Length > 0)
			{
				for (int i = 0; i < objectReferences.Length; i++)
				{
					JumpLinks.Instance.CreateJumpLink(objectReferences[i]);
				}
			}
		}

		private void OnDropHierarchyOnly()
		{
			UnityEngine.Object[] objectReferences = DragAndDrop.objectReferences;
			if (objectReferences.Length > 0)
			{
				for (int i = 0; i < objectReferences.Length; i++)
				{
					JumpLinks.Instance.CreateOnlyHierarchyJumpLink(objectReferences[i]);
				}
			}
		}

		private void OnDropProjectOnly()
		{
			UnityEngine.Object[] objectReferences = DragAndDrop.objectReferences;
			if (objectReferences.Length > 0)
			{
				for (int i = 0; i < objectReferences.Length; i++)
				{
					JumpLinks.Instance.CreateOnlyProjectJumpLink(objectReferences[i]);
				}
			}
		}
	}
}
