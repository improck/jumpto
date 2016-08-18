using UnityEditor;
using UnityEngine;
using System.Collections.Generic;


namespace ImpRock.JumpTo.Editor
{
	internal sealed class GuiJumpLinkListView : GuiBase
	{
		[SerializeField] private Vector2 m_ScrollViewPosition;
		[SerializeField] private GuiProjectJumpLinkView m_ProjectView;
		[SerializeField] private List<GuiHierarchyJumpLinkView> m_HierarchyViews = new List<GuiHierarchyJumpLinkView>();

		private RectRef m_DrawRect = new RectRef();
		private Rect m_ScrollViewRect;

		private JumpToEditorWindow m_Window;

		private readonly Vector2 IconSize = new Vector2(16.0f, 16.0f);
		

		public override void OnWindowEnable(EditorWindow window)
		{
			m_Window = window as JumpToEditorWindow;

			if (m_ProjectView == null && m_Window.JumpLinksInstance.ProjectLinks.Links.Count > 0)
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
			
			if (m_ProjectView != null)
				m_ProjectView.OnWindowEnable(window);

			for (int i = 0; i < m_HierarchyViews.Count; i++)
			{
				m_HierarchyViews[i].OnWindowEnable(window);
			}

			JumpLinks.OnHierarchyLinkAdded += HierarchyLinkAddedHandler;
			JumpLinks.OnProjectLinkAdded += ProjectLinkAddedHandler;
			SceneStateMonitor.OnLoadedSceneCountChanged += LoadedSceneCountChangeHandler;
		}

		protected override void OnGui()
		{
			HandleDragAndDrop();

			Vector2 iconSizeBak = EditorGUIUtility.GetIconSize();
			EditorGUIUtility.SetIconSize(IconSize);

			m_DrawRect.Set(0.0f, 0.0f, m_Size.x, m_Size.y);

			//TODO: maybe only calculate this if the height changes?
			m_ScrollViewRect.height = m_ProjectView != null ? m_ProjectView.TotalHeight : 0.0f;
			for (int i = 0; i < m_HierarchyViews.Count; i++)
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

			m_ScrollViewPosition = GUI.BeginScrollView(m_DrawRect, m_ScrollViewPosition, m_ScrollViewRect);

			if (m_ProjectView != null)
			{
				m_DrawRect.height = m_ProjectView.TotalHeight;
				m_ProjectView.Draw(m_DrawRect);
			}

			for (int i = 0; i < m_HierarchyViews.Count; i++)
			{
				m_DrawRect.y += m_DrawRect.height;
				m_DrawRect.height = m_HierarchyViews[i].TotalHeight;
				m_HierarchyViews[i].Draw(m_DrawRect);
			}

			GUI.EndScrollView(true);

			EditorGUIUtility.SetIconSize(iconSizeBak);
		}

		private void HandleDragAndDrop()
		{
			Event currentEvent = Event.current;
			switch (currentEvent.type)
			{
			//raised repeatedly while a drag op is hovering
			case EventType.DragUpdated:
				{
					//TODO: revisit drag owner concept
					if (m_ProjectView != null && m_ProjectView.IsDragOwner)
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
					if (m_ProjectView != null && m_ProjectView.IsDragOwner)
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

		private void LoadedSceneCountChangeHandler(int oldCount, int currentCount)
		{
			//TODO: figure out what scenes were loaded or unloaded, and update the hierarchy views
		}

		private void HierarchyLinkAddedHandler(int sceneId)
		{
			if (m_HierarchyViews.Find(v => v.SceneId == sceneId) != null)
				return;

			GuiHierarchyJumpLinkView view = GuiBase.Create<GuiHierarchyJumpLinkView>();
			view.SceneId = sceneId;
			view.OnWindowEnable(m_Window);

			m_HierarchyViews.Add(view);
			
			m_Window.Repaint();
		}

		private void ProjectLinkAddedHandler()
		{
			if (m_ProjectView != null)
				return;

			m_ProjectView = GuiBase.Create<GuiProjectJumpLinkView>();
		}
	}
}
