using UnityEditor;
using UnityEngine;
using System.Collections.Generic;


namespace JumpTo
{
	public class GuiHierarchyJumpLinkView : GuiJumpLinkViewBase<HierarchyJumpLink>
	{
		private GUIContent m_MenuFrameLink;
		private GUIContent m_MenuFrameLinkPlural;
		private GUIContent m_MenuSaveLinks;
		private string m_TitleText;	//TEMP
		
		
		public override void OnWindowEnable(EditorWindow window)
		{
			base.OnWindowEnable(window);

			m_ControlTitle = new GUIContent(ResLoad.Instance.GetText(ResId.LabelHierarchyLinks));
			m_MenuFrameLink = new GUIContent(ResLoad.Instance.GetText(ResId.MenuContextFrameLink));
			m_MenuFrameLinkPlural = new GUIContent(ResLoad.Instance.GetText(ResId.MenuContextFrameLinkPlural));
			m_MenuSaveLinks = new GUIContent(ResLoad.Instance.GetText(ResId.MenuContextSaveLinks));

			//TEMP
			m_TitleText = m_ControlTitle.text;
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
			m_MenuPingLink.text = ResLoad.Instance.GetText(ResId.MenuContextPingLink) + " \"" + m_LinkContainer.ActiveSelectedObject.LinkReference.name + "\"";

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

				menu.AddItem(m_MenuRemoveLink, false, RemoveSelectedAndFlag);
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

				menu.AddItem(m_MenuRemoveLinkPlural, false, RemoveSelectedAndFlag);
			}

			menu.ShowAsContext();
		}

		protected override void ShowTitleContextMenu()
		{
			GenericMenu menu = null;

			if (EditorApplication.currentScene != string.Empty &&
				JumpLinks.Instance.HierarchyLinksChanged)
			{
				menu = new GenericMenu();
				menu.AddItem(m_MenuSaveLinks, false, SaveLinks);
			}

			if (m_LinkContainer.Links.Count > 0)
			{
				if (menu == null)
					menu = new GenericMenu();

				menu.AddItem(m_MenuRemoveAll, false, RemoveAllAndFlag);
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
			SerializationControl.Instance.SaveHierarchyLinks(EditorApplication.currentScene);

			JumpLinks.Instance.HierarchyLinksChanged = false;
		}

		private bool ValidateSceneView()
		{
			return SceneView.lastActiveSceneView != null || SceneView.currentDrawingSceneView != null;
		}

		//TEMP
		private void RemoveSelectedAndFlag()
		{
			JumpLinks.Instance.HierarchyLinksChanged = true;
			RemoveSelected();
		}

		//TEMP
		private void RemoveAllAndFlag()
		{
			JumpLinks.Instance.HierarchyLinksChanged = true;
			RemoveAll();
		}

		//TEMP
		protected override void OnGui()
		{
			if (JumpLinks.Instance.HierarchyLinksChanged)
				m_ControlTitle.text = m_TitleText + '*';
			else
				m_ControlTitle.text = m_TitleText;

			base.OnGui();
		}
	}
}

