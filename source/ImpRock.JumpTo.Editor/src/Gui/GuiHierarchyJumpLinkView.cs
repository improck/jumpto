﻿using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;


namespace ImpRock.JumpTo.Editor
{
	internal sealed class GuiHierarchyJumpLinkView : GuiJumpLinkViewBase<HierarchyJumpLink>
	{
		private GUIContent m_MenuFrameLink;
		private GUIContent m_MenuFrameLinkPlural;
		private GUIContent m_MenuSaveLinks;
		private string m_TitleSuffix;
		private SceneState m_SceneState = null;
		
		[SerializeField] private bool m_IsDirty = false;
		[SerializeField] private int m_SceneId = 0;

		public int SceneId { get { return m_SceneId; } set { m_SceneId = value; } }
		

		public override void OnWindowEnable(EditorWindow window)
		{
			//NOTE: set SceneId property before this is called
			m_LinkContainer = (window as JumpToEditorWindow).JumpLinksInstance.HierarchyLinks[m_SceneId];
			
			base.OnWindowEnable(window);

			m_LinkContainer.OnLinksChanged += OnHierarchyLinksChanged;

			m_ControlTitle = new GUIContent();
			m_MenuFrameLink = new GUIContent(JumpToResources.Instance.GetText(ResId.MenuContextFrameLink));
			m_MenuFrameLinkPlural = new GUIContent(JumpToResources.Instance.GetText(ResId.MenuContextFrameLinkPlural));
			m_MenuSaveLinks = new GUIContent(JumpToResources.Instance.GetText(ResId.MenuContextSaveLinks));

			m_TitleSuffix = " " + JumpToResources.Instance.GetText(ResId.LabelHierarchyLinksSuffix);

			SetupSceneState();
		}

		protected override void OnCreate()
		{
			base.OnCreate();

			//TODO: freshly loaded hierarchy links should not be marked dirty
			m_IsDirty = true;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			m_LinkContainer.OnLinksChanged -= OnHierarchyLinksChanged;
			m_SceneState.OnNameChange -= OnSceneNameChanged;
			m_SceneState.OnIsDirtyChange -= OnSceneIsDirtyChanged;
			m_SceneState.OnIsLoadedChange -= OnSceneIsLoadedChanged;
			m_SceneState.OnClose -= OnSceneClosed;
		}

		protected override Color DetermineNormalTextColor(HierarchyJumpLink link)
		{
			if (!link.Active)
				return GraphicAssets.Instance.LinkTextColors[(int)link.ReferenceType] - GraphicAssets.Instance.DisabledColorModifier;
			else
				return GraphicAssets.Instance.LinkTextColors[(int)link.ReferenceType];
		}

		protected override Color DetermineOnNormalTextColor(HierarchyJumpLink link)
		{
			if (!link.Active)
				return GraphicAssets.Instance.SelectedLinkTextColors[(int)link.ReferenceType] - GraphicAssets.Instance.DisabledColorModifier;
			else
				return GraphicAssets.Instance.SelectedLinkTextColors[(int)link.ReferenceType];
		}

		protected override void ShowLinkContextMenu()
		{
			GenericMenu menu = new GenericMenu();

			//NOTE: a space followed by an underscore (" _") will cause all text following that
			//		to appear right-justified and all caps in a GenericMenu. the name is being
			//		parsed for hotkeys, and " _" indicates 'no modifiers' in the hotkey string.
			//		See: http://docs.unity3d.com/ScriptReference/MenuItem.html
			m_MenuPingLink.text = JumpToResources.Instance.GetText(ResId.MenuContextPingLink) + " \""
				+ m_LinkContainer.ActiveSelectedObject.LinkReference.name + "\"";

			int selectionCount = m_LinkContainer.SelectionCount;
			if (selectionCount == 0)
			{
			}
			else if (selectionCount == 1)
			{
				menu.AddItem(m_MenuPingLink, false, PingSelectedLink);
			
				if (ValidateSceneView())
					menu.AddItem(m_MenuFrameLink, false, FrameLink);
				else
					menu.AddDisabledItem(m_MenuFrameLink);

				menu.AddSeparator(string.Empty);

				menu.AddItem(m_MenuRemoveLink, false, RemoveSelected);
			}
			else if (selectionCount > 1)
			{
				menu.AddItem(m_MenuPingLink, false, PingSelectedLink);

				if (ValidateSceneView())
					menu.AddItem(m_MenuFrameLinkPlural, false, FrameLink);
				else
					menu.AddDisabledItem(m_MenuFrameLinkPlural);

				menu.AddSeparator(string.Empty);

				menu.AddItem(m_MenuRemoveLinkPlural, false, RemoveSelected);
			}

			menu.ShowAsContext();
		}

		protected override void ShowTitleContextMenu()
		{
			GenericMenu menu = null;

			if (m_IsDirty)
			{
				menu = new GenericMenu();
				menu.AddItem(m_MenuSaveLinks, false, SaveLinks);
			}

			if (m_LinkContainer.Links.Count > 0)
			{
				if (menu == null)
					menu = new GenericMenu();

				menu.AddItem(m_MenuRemoveAll, false, RemoveAll);
			}

			if (menu != null)
				menu.ShowAsContext();
		}

		protected override void OnDoubleClick()
		{
			FrameLink();
		}

		private void SetupSceneState()
		{
			if (m_SceneState != null)
			{
				m_SceneState.OnNameChange -= OnSceneNameChanged;
				m_SceneState.OnIsDirtyChange -= OnSceneIsDirtyChanged;
				m_SceneState.OnIsLoadedChange -= OnSceneIsLoadedChanged;
				m_SceneState.OnClose -= OnSceneClosed;

				m_SceneState = null;
			}

			if (m_SceneId != 0)
			{
				m_SceneState = SceneStateMonitor.Instance.GetSceneState(m_SceneId);
				if (m_SceneState != null)
				{
					RefreshControlTitle();

					m_SceneState.OnNameChange += OnSceneNameChanged;
					m_SceneState.OnIsDirtyChange += OnSceneIsDirtyChanged;
					m_SceneState.OnIsLoadedChange += OnSceneIsLoadedChanged;
					m_SceneState.OnClose += OnSceneClosed;
				}
				else
				{
					Debug.LogError("JumpTo: Attempted to create a link view for an invalid scene (ID: " + m_SceneId + ").");
				}
			}
			else
			{
				Debug.LogError("JumpTo: Attempted to create a link view for scene ID 0, which is invalid.");
			}
		}

		private void RefreshControlTitle()
		{
			string title = (m_SceneState.Name.Length != 0 ? m_SceneState.Name : "(Untitled)") + m_TitleSuffix;
			if (m_IsDirty)
				title += '*';

			m_ControlTitle.text = title;
		}

		private void FrameLink()
		{
			SceneView sceneView = SceneView.lastActiveSceneView;
			if (sceneView == null)
				sceneView = SceneView.currentDrawingSceneView;

			if (sceneView != null)
			{
				Object[] selectedLinks = m_LinkContainer.SelectedLinkReferences;
				if (selectedLinks != null)
				{
					Object[] selection = Selection.objects;
					Selection.objects = selectedLinks;
					sceneView.FrameSelected();
					Selection.objects = selection;
				}
			}
		}

		private void SaveLinks()
		{
			if ((m_LinkContainer as HierarchyJumpLinkContainer).HasLinksToUnsavedInstances)
			{
				JumpToResources resources = JumpToResources.Instance;
				if (EditorUtility.DisplayDialog(resources.GetText(ResId.DialogSaveLinksWarningTitle),
					resources.GetText(ResId.DialogSaveLinksWarningMessage),
					resources.GetText(ResId.DialogOk), resources.GetText(ResId.DialogCancel)))
				{
					m_Window.SerializationControlInstance.SaveHierarchyLinks(m_SceneId);

					//keep the scene marked dirty so that they can save the scene, and then the links again
					m_IsDirty = true;
					RefreshControlTitle();
				}
			}
			else
			{
				m_IsDirty = !m_Window.SerializationControlInstance.SaveHierarchyLinks(m_SceneId);
				RefreshControlTitle();
			}
		}

		private bool ValidateSceneView()
		{
			System.Collections.ArrayList sceneViews = SceneView.sceneViews;
			return sceneViews != null && sceneViews.Count > 0;
		}

		private void OnHierarchyLinksChanged()
		{
			if (m_LinkContainer.Links.Count > 0)
			{
				m_IsDirty = true;
				RefreshControlTitle();
				FindTotalHeight();
			}
			else
			{
				m_Window.SerializationControlInstance.SaveHierarchyLinks(m_SceneId);
				m_MarkedForClose = true;
			}
		}

		private void OnSceneNameChanged(int sceneId, string sceneName)
		{
			RefreshControlTitle();
		}

		private void OnSceneIsDirtyChanged(int sceneId, bool isDirty)
		{
			m_IsDirty = m_IsDirty || isDirty;
			RefreshControlTitle();
		}

		private void OnSceneIsLoadedChanged(int sceneId, bool isLoaded)
		{
			Debug.Log("Scene load change: " + m_ControlTitle.text + " isLoaded " + isLoaded);
			if (!isLoaded)
			{
				m_LinkContainer.OnLinksChanged -= OnHierarchyLinksChanged;
				m_MarkedForClose = true;

				//NOTE: can't save here because the scene is already gone
				//if (m_IsDirty)
				//	m_Window.SerializationControlInstance.SaveHierarchyLinks(m_SceneId);
			}
		}

		private void OnSceneClosed(int sceneId)
		{
			Debug.Log("Scene closed: " + m_ControlTitle.text);
			m_LinkContainer.OnLinksChanged -= OnHierarchyLinksChanged;
			m_MarkedForClose = true;
		}
	}
}
