using UnityEditor;
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
		[SerializeField] private bool m_HierarchyLinksChanged = false;	//TODO: get rid of this
		[SerializeField] private int m_SceneId = 0;

		public int SceneId { get { return m_SceneId; } set { m_SceneId = value; } }
		
		
		public override void OnWindowEnable(EditorWindow window)
		{
			//NOTE: set SceneId property before this is called
			m_LinkContainer = (window as JumpToEditorWindow).JumpLinksInstance.HierarchyLinks[m_SceneId];
			
			base.OnWindowEnable(window);

			m_LinkContainer.OnLinksChanged += OnHierarchyLinksChanged;

			m_MenuFrameLink = new GUIContent(JumpToResources.Instance.GetText(ResId.MenuContextFrameLink));
			m_MenuFrameLinkPlural = new GUIContent(JumpToResources.Instance.GetText(ResId.MenuContextFrameLinkPlural));
			m_MenuSaveLinks = new GUIContent(JumpToResources.Instance.GetText(ResId.MenuContextSaveLinks));

			//HACK: should not be doing this in-place
			//TODO: monitor scene name changes, too
			string sceneName = string.Empty;
			int loadedSceneCount = EditorSceneManager.loadedSceneCount;
			for (int i = 0; i < loadedSceneCount; i++)
			{
				Scene scene = EditorSceneManager.GetSceneAt(i);
				if (scene.GetHashCode() == m_SceneId)
				{
					sceneName = scene.name;
					break;
				}
			}

			m_TitleSuffix = " " + JumpToResources.Instance.GetText(ResId.LabelHierarchyLinksSuffix);

			//TODO: this doesn't work correctly anymore
			if (m_HierarchyLinksChanged)
				m_ControlTitle = new GUIContent(sceneName + m_TitleSuffix + '*');
			else
				m_ControlTitle = new GUIContent(sceneName + m_TitleSuffix);
		}

		public override void OnWindowDisable(EditorWindow window)
		{
			m_LinkContainer.OnLinksChanged -= OnHierarchyLinksChanged;
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

		protected override void ShowContextMenu()
		{
			GenericMenu menu = new GenericMenu();

			//NOTE: a space followed by an underscore (" _") will cause all text following that
			//		to appear right-justified and all caps in a GenericMenu. the name is being
			//		parsed for hotkeys, and " _" indicates 'no modifiers' in the hotkey string.
			//		See: http://docs.unity3d.com/ScriptReference/MenuItem.html
			m_MenuPingLink.text = JumpToResources.Instance.GetText(ResId.MenuContextPingLink) + " \"" + m_LinkContainer.ActiveSelectedObject.LinkReference.name + "\"";

			int selectionCount = m_LinkContainer.SelectionCount;
			if (selectionCount == 0)
			{
			}
			else if (selectionCount == 1)
			{
				menu.AddItem(m_MenuPingLink, false, PingSelectedLink);
				menu.AddItem(m_MenuSetAsSelection, false, SetAsSelection);
				menu.AddItem(m_MenuAddToSelection, false, AddToSelection);
			
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
				menu.AddItem(m_MenuSetAsSelectionPlural, false, SetAsSelection);
				menu.AddItem(m_MenuAddToSelectionPlural, false, AddToSelection);

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

			//TODO: show this when links state has changed, too
			//TODO: make sure the scene is still loaded?
			if (SceneStateMonitor.Instance.GetSceneState(m_SceneId).IsDirty)
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
			//TODO: detect links to objects not saved in the scene, warn the user
			m_Window.SerializationControlInstance.SaveHierarchyLinks(m_SceneId);

			m_ControlTitle.text = m_TitleSuffix;
			m_HierarchyLinksChanged = false;
		}

		private bool ValidateSceneView()
		{
			System.Collections.ArrayList sceneViews = SceneView.sceneViews;
			return sceneViews != null && sceneViews.Count > 0;
		}

		private void OnHierarchyLinksChanged()
		{
			//TODO
			//m_ControlTitle.text = sceneName + m_TitleSuffix + '*';
			m_HierarchyLinksChanged = true;
		}
	}
}

