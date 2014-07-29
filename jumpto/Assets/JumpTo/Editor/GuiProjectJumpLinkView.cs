using UnityEditor;
using UnityEngine;
using System.Collections.Generic;


namespace JumpTo
{
	public class GuiProjectJumpLinkView : GuiJumpLinkViewBase<ProjectJumpLink>
	{
		protected GUIContent m_MenuOpenLink;
		protected GUIContent m_MenuOpenLinkPlural;


		public override void OnWindowEnable(EditorWindow window)
		{
			base.OnWindowEnable(window);

			m_ControlTitle = new GUIContent(ResLoad.Instance.GetText(ResId.LabelProjectLinks));
			m_MenuOpenLink = new GUIContent(ResLoad.Instance.GetText(ResId.MenuContextOpenLink));
			m_MenuOpenLinkPlural = new GUIContent(ResLoad.Instance.GetText(ResId.MenuContextOpenLinkPlural));
		}

		protected override void ShowContextMenu()
		{
			GenericMenu menu = new GenericMenu();

			//NOTE: a space followed by an underscore (" _") will cause all text following that
			//		to appear right-justified and all caps in a GenericMenu. the name is being
			//		parsed for hotkeys, and " _" indicates 'no modifiers' in the hotkey string.
			//		See: http://docs.unity3d.com/ScriptReference/MenuItem.html
			m_MenuPingLink.text = ResLoad.Instance.GetText(ResId.MenuContextPingLink) + " \"" + m_LinkContainer.ActiveSelectedObject.LinkLabelContent.text + "\"";

			int selectionCount = m_LinkContainer.SelectionCount;
			if (selectionCount == 0)
			{
			}
			else if (selectionCount == 1)
			{
				menu.AddItem(m_MenuPingLink, false, PingSelectedLink);
				menu.AddItem(m_MenuSetAsSelection, false, SetAsSelection);
				menu.AddItem(m_MenuAddToSelection, false, AddToSelection);
				menu.AddItem(m_MenuOpenLink, false, OpenAssets);
				menu.AddSeparator(string.Empty);
				menu.AddItem(m_MenuRemoveLink, false, RemoveSelected);
				//TODO: remove all but selected
				//TODO: invert selection
			}
			else if (selectionCount > 1)
			{
				menu.AddItem(m_MenuPingLink, false, PingSelectedLink);
				menu.AddItem(m_MenuSetAsSelectionPlural, false, SetAsSelection);
				menu.AddItem(m_MenuAddToSelectionPlural, false, AddToSelection);
				menu.AddItem(m_MenuOpenLinkPlural, false, OpenAssets);
				menu.AddSeparator(string.Empty);
				menu.AddItem(m_MenuRemoveLinkPlural, false, RemoveSelected);
				//TODO: remove all but selected
				//TODO: invert selection
			}

			menu.ShowAsContext();
		}

		protected override void OnDoubleClick()
		{
			OpenAssets();
		}

		private void OpenAssets()
		{
			ProjectJumpLink activeSelection = m_LinkContainer.ActiveSelectedObject;
			if (activeSelection != null)
				AssetDatabase.OpenAsset(activeSelection.LinkReference);
		}
	}
}
