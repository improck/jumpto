using UnityEditor;
using UnityEngine;
using System.Collections.Generic;


namespace ImpRock.JumpTo.Editor
{
	internal sealed class GuiJumpLinkListView : GuiBase
	{
		[SerializeField] private float m_Divider = 0.5f;
		[SerializeField] protected Vector2 m_ScrollViewPosition;
		[SerializeField] private GuiProjectJumpLinkView m_ProjectView;
		[SerializeField] private List<GuiHierarchyJumpLinkView> m_HierarchyViews = new List<GuiHierarchyJumpLinkView>();

		private RectRef m_DrawRect = new RectRef();
		private Rect m_DividerRect;
		protected Rect m_ScrollViewRect;

		private JumpToEditorWindow m_Window;

		private const float DividerHalfThickness = 3.0f;
		public const int DividerMin = 122;

		private readonly Vector2 IconSize = new Vector2(16.0f, 16.0f);
		private readonly int DividerHash = "divider".GetHashCode();


		public override void OnWindowEnable(EditorWindow window)
		{
			m_Window = window as JumpToEditorWindow;

			if (m_ProjectView == null)
			{
				m_ProjectView = GuiBase.Create<GuiProjectJumpLinkView>();
			}

			if (m_HierarchyViews.Count == 0)
			{
				List<int> sceneIds = m_Window.JumpLinksInstance.HierarchyLinks.Keys;
				for (int i = 0; i < sceneIds.Count; i++)
				{
					GuiHierarchyJumpLinkView view = GuiBase.Create<GuiHierarchyJumpLinkView>();
					view.SceneId = sceneIds[i];

					m_HierarchyViews.Add(view);
				}
			}

			//m_Window.JumpLinksInstance.RefreshLinksY();

			m_ProjectView.OnWindowEnable(window);

			for (int i = 0; i < m_HierarchyViews.Count; i++)
			{
				m_HierarchyViews[i].OnWindowEnable(window);
			}

			if (m_Window.JumpToSettingsInstance.DividerPosition >= 0.0f)
				m_Divider = m_Window.JumpToSettingsInstance.DividerPosition;
		}

		protected override void OnGui()
		{
			HandleDragAndDrop();

			Vector2 iconSizeBak = EditorGUIUtility.GetIconSize();
			EditorGUIUtility.SetIconSize(IconSize);

			Color bgColorBak = GUI.backgroundColor;

			//float adjWidth = m_Size.x;
			//float adjHeight = m_Size.y;

			//RefreshDividerPosition();

			//adjHeight = Mathf.Floor(m_Size.y * m_Divider);
			//m_DrawRect.Set(0.0f, 0.0f, m_Size.x, adjHeight - DividerHalfThickness);
			m_DrawRect.Set(0.0f, 0.0f, m_Size.x, m_Size.y);

			//TODO: maybe only calculate this if the height changes?
			m_ScrollViewRect.height = m_ProjectView.TotalHeight;
			for (int i = 0; i > m_HierarchyViews.Count; i++)
			{
				m_ScrollViewRect.height += m_HierarchyViews[i].TotalHeight;
			}

			//TODO: adjust rect for scrollbar
			//if the vertical scrollbar is visible, adjust view rect
			//	width by the width of the scrollbar (17.0f)
			//if (m_ScrollViewRect.height > m_DrawRect.height)
			//	m_ScrollViewRect.width = m_DrawRect.width - 15.0f;
			//else
			//	m_ScrollViewRect.width = m_DrawRect.width;

			//m_DividerRect.x = 0.0f;
			//m_DividerRect.y = m_DrawRect.height;
			//m_DividerRect.width = m_Size.x;
			//m_DividerRect.height = DividerHalfThickness * 2.0f;

			m_ScrollViewPosition = GUI.BeginScrollView(m_DrawRect, m_ScrollViewPosition, m_ScrollViewRect);

			m_DrawRect.height = m_ProjectView.TotalHeight;
			m_ProjectView.Draw(m_DrawRect);

			//TODO: iterate over all hierarchy views instead of this
			if (m_HierarchyViews.Count > 0)
			{
				m_DrawRect.height = m_HierarchyViews[0].TotalHeight;
				m_HierarchyViews[0].Draw(m_DrawRect);
			}

			GUI.EndScrollView(true);

			//TODO: draw ALL hierarchy views
			//for (int i = 0; i < m_HierarchyViews.Count; i++)
			//{
			//	//TODO: update draw rect
			//	m_HierarchyViews[i].Draw(m_DrawRect);
			//}

			//OnDividerGui();

			//switch (m_Window.JumpToSettingsInstance.Visibility)
			//{
			//case JumpToSettings.VisibleList.ProjectAndHierarchy:
			//	{
			//		float adjWidth = m_Size.x;
			//		float adjHeight = m_Size.y;

			//		RefreshDividerPosition();

			//		//draw the top/left box
			//		if (m_Window.JumpToSettingsInstance.Vertical)
			//		{
			//			adjHeight = Mathf.Floor(m_Size.y * m_Divider);
			//			m_DrawRect.Set(0.0f, 0.0f, m_Size.x, adjHeight - DividerHalfThickness);

			//			m_DividerRect.x = 0.0f;
			//			m_DividerRect.y = m_DrawRect.height;
			//			m_DividerRect.width = m_Size.x;
			//			m_DividerRect.height = DividerHalfThickness * 2.0f;
			//		}
			//		else
			//		{
			//			adjWidth = Mathf.Floor(m_Size.x * m_Divider);
			//			m_DrawRect.Set(0.0f, 0.0f, adjWidth - DividerHalfThickness, m_Size.y);

			//			m_DividerRect.x = m_DrawRect.width; ;
			//			m_DividerRect.y = 0.0f;
			//			m_DividerRect.width = DividerHalfThickness * 2.0f;
			//			m_DividerRect.height = m_Size.y;
			//		}

			//		if (m_Window.JumpToSettingsInstance.ProjectFirst)
			//		{
			//			m_ProjectView.Draw(m_DrawRect);
			//			if (m_ProjectView.HasFocus)
			//				m_HierarchyView.HasFocus = false;
			//		}
			//		else
			//		{
			//			m_HierarchyView.Draw(m_DrawRect);
			//			if (m_HierarchyView.HasFocus)
			//				m_ProjectView.HasFocus = false;
			//		}

			//		//draw the bottom/right box
			//		if (m_Window.JumpToSettingsInstance.Vertical)
			//		{
			//			m_DrawRect.Set(0.0f, adjHeight + DividerHalfThickness, m_Size.x, (m_Size.y - adjHeight) - DividerHalfThickness);
			//		}
			//		else
			//		{
			//			m_DrawRect.Set(adjWidth + DividerHalfThickness, 0.0f, (m_Size.x - adjWidth) - DividerHalfThickness, m_Size.y);
			//		}

			//		if (m_Window.JumpToSettingsInstance.ProjectFirst)
			//		{
			//			m_HierarchyView.Draw(m_DrawRect);
			//			if (m_HierarchyView.HasFocus)
			//				m_ProjectView.HasFocus = false;
			//		}
			//		else
			//		{
			//			m_ProjectView.Draw(m_DrawRect);
			//			if (m_ProjectView.HasFocus)
			//				m_HierarchyView.HasFocus = false;
			//		}

			//		OnDividerGui();
			//	}
			//	break;
			//case JumpToSettings.VisibleList.ProjectOnly:
			//	{
			//		m_DrawRect.Set(0.0f, 0.0f, m_Size.x, m_Size.y - 1.0f);

			//		m_ProjectView.HasFocus = true;
			//		m_HierarchyView.HasFocus = false;
			//		m_ProjectView.Draw(m_DrawRect);
			//	}
			//	break;
			//case JumpToSettings.VisibleList.HierarchyOnly:
			//	{
			//		m_DrawRect.Set(0.0f, 0.0f, m_Size.x, m_Size.y - 1.0f);

			//		m_ProjectView.HasFocus = false;
			//		m_HierarchyView.HasFocus = true;
			//		m_HierarchyView.Draw(m_DrawRect);
			//	}
			//	break;
			//}

			GUI.backgroundColor = bgColorBak;

			EditorGUIUtility.SetIconSize(iconSizeBak);
		}

		//private void OnDividerGui()
		//{
		//	int controlId = GUIUtility.GetControlID(DividerHash, FocusType.Passive, m_DividerRect);

		//	if (m_Window.JumpToSettingsInstance.Vertical)
		//		EditorGUIUtility.AddCursorRect(m_DividerRect, MouseCursor.SplitResizeUpDown, controlId);
		//	else
		//		EditorGUIUtility.AddCursorRect(m_DividerRect, MouseCursor.SplitResizeLeftRight, controlId);

		//	Event current = Event.current;
		//	switch (current.GetTypeForControl(controlId))
		//	{
		//	case EventType.MouseDown:
		//		{
		//			if (current.button == 0 && m_DividerRect.Contains(current.mousePosition))
		//			{
		//				GUIUtility.hotControl = controlId;
		//				current.Use();
		//			}
		//		}
		//		break;
		//	case EventType.MouseUp:
		//		{
		//			if (GUIUtility.hotControl == controlId)
		//			{
		//				GUIUtility.hotControl = 0;
		//				current.Use();

		//				m_Window.JumpToSettingsInstance.DividerPosition = m_Divider;
		//			}
		//		}
		//		break;
		//	case EventType.MouseDrag:
		//		{
		//			if (GUIUtility.hotControl == controlId && current.button == 0)
		//			{
		//				if (m_Window.JumpToSettingsInstance.Vertical)
		//				{
		//					m_Divider = Mathf.Clamp((int)current.mousePosition.y, DividerMin, (int)m_Size.y - DividerMin) / m_Size.y;
		//				}
		//				else
		//				{
		//					m_Divider = Mathf.Clamp((int)current.mousePosition.x, DividerMin, (int)m_Size.x - DividerMin) / m_Size.x;
		//				}

		//				current.Use();
		//			}
		//		}
		//		break;
		//	case EventType.Repaint:
		//		{
		//			GraphicAssets.Instance.DividerHorizontalStyle.Draw(m_DividerRect, false, false, false, false);
		//			if (m_Window.JumpToSettingsInstance.Vertical)
		//			{
		//				GraphicAssets.Instance.DividerHorizontalStyle.Draw(m_DividerRect, false, false, false, false);
		//			}
		//			else
		//			{
		//				GraphicAssets.Instance.DividerVerticalStyle.Draw(m_DividerRect, false, false, false, false);
		//			}
		//		}
		//		break;
		//	}
		//}

		private void HandleDragAndDrop()
		{
			Event currentEvent = Event.current;
			switch (currentEvent.type)
			{
			//raised repeatedly while a drag op is hovering
			case EventType.DragUpdated:
				{
					//TODO: revisit drag owner concept
					if (m_ProjectView.IsDragOwner)
					{
						break;
					}
					else
					{
						bool dragOwned = false;
						for (int i = 0; i < m_HierarchyViews.Count; i++)
						{
							if (m_HierarchyViews[i].IsDragOwner)
							{
								dragOwned = true;
								break;
							}
						}

						if (dragOwned)
							break;
					}

					//drag most likely came from another window, figure out
					//	if it can be accepted and where it would go
					if (DragAndDrop.visualMode == DragAndDropVisualMode.None)
					{
						if (DragAndDrop.objectReferences.Length > 0)
						{
							//reject component links
							if (DragAndDrop.objectReferences[0] is Component)
								DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;

							//TODO: component links??

							switch (m_Window.JumpToSettingsInstance.Visibility)
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
					//TODO: revisit drag owner concept
					if (m_ProjectView.IsDragOwner)
					{
						break;
					}
					else
					{
						bool dragOwned = false;
						for (int i = 0; i < m_HierarchyViews.Count; i++)
						{
							if (m_HierarchyViews[i].IsDragOwner)
							{
								dragOwned = true;
								break;
							}
						}

						if (dragOwned)
							break;
					}

					DragAndDrop.AcceptDrag();

					switch (m_Window.JumpToSettingsInstance.Visibility)
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
					m_Window.JumpLinksInstance.CreateJumpLink(objectReferences[i]);
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
					m_Window.JumpLinksInstance.CreateOnlyHierarchyJumpLink(objectReferences[i]);
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
					m_Window.JumpLinksInstance.CreateOnlyProjectJumpLink(objectReferences[i]);
				}
			}
		}

		//public void RefreshDividerPosition()
		//{
		//	if (m_Window.JumpToSettingsInstance.Vertical)
		//	{
		//		m_Divider = Mathf.Clamp(m_Divider * (int)m_Size.y, DividerMin, (int)m_Size.y - DividerMin) / m_Size.y;
		//	}
		//	else
		//	{
		//		m_Divider = Mathf.Clamp(m_Divider * (int)m_Size.x, DividerMin, (int)m_Size.x - DividerMin) / m_Size.x;
		//	}
		//}
	}
}
