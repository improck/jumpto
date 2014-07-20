using UnityEditor;
using UnityEngine;
using System.Collections.Generic;


namespace JumpTo
{
	public class GuiProjectJumpLinkView : GuiJumpLinkViewBase<ProjectJumpLink>
	{
		protected GUIContent m_MenuOpenLink;


		public override void OnWindowEnable(EditorWindow window)
		{
			base.OnWindowEnable(window);

			m_ControlTitle = new GUIContent(ResLoad.Instance.GetText(ResId.LabelProjectLinks));
			m_MenuOpenLink = new GUIContent(ResLoad.Instance.GetText(ResId.MenuContextOpenLink));
		}

		protected override void ShowContextMenu()
		{
			GenericMenu menu = new GenericMenu();
			//TODO: use pluralized text for multiple selection
			menu.AddItem(m_MenuPingLink, false, PingSelectedLink);
			menu.AddItem(m_MenuSetAsSelection, false, SetAsSelection);
			menu.AddItem(m_MenuAddToSelection, false, AddToSelection);
			menu.AddItem(m_MenuOpenLink, false, OpenAssets);
			menu.AddSeparator(string.Empty);
			menu.AddItem(m_MenuRemoveLink, false, RemoveSelected);

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
