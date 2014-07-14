using UnityEditor;
using UnityEngine;
using System.Collections.Generic;


namespace JumpTo
{
	public class GuiHierarchyJumpLinkView : GuiJumpLinkViewBase<HierarchyJumpLink>
	{
		private GUIContent m_MenuFrameLink;
		
		
		public override void OnWindowEnable(EditorWindow window)
		{
			base.OnWindowEnable(window);

			m_ControlTitle = new GUIContent(ResLoad.Instance.GetText(ResId.LabelHierarchyLinks));
			m_MenuFrameLink = new GUIContent(ResLoad.Instance.GetText(ResId.MenuContextFrameLink));
		}

		protected override void ShowContextMenu()
		{
			GenericMenu menu = new GenericMenu();
			//TODO: use pluralized text for multiple selection
			menu.AddItem(m_MenuPingLink, false, PingSelectedLink);
			menu.AddItem(m_MenuSetAsSelection, false, SetAsSelection);
			menu.AddItem(m_MenuAddToSelection, false, AddToSelection);
			
			if (ValidateSceneView())
				menu.AddItem(m_MenuFrameLink, false, FrameLink);
			else
				menu.AddDisabledItem(m_MenuFrameLink);

			menu.AddSeparator(string.Empty);

			menu.AddItem(m_MenuRemoveLink, false, RemoveSelected);

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
				Object[] selection = Selection.objects;
				Object[] tempSelection = { m_LinkContainer.Links[m_Selected].LinkReference };
				Selection.objects = tempSelection;
				sceneView.FrameSelected();
				Selection.objects = selection;
			}
		}

		private bool ValidateSceneView()
		{
			return SceneView.lastActiveSceneView != null || SceneView.currentDrawingSceneView != null;
		}
	}
}

