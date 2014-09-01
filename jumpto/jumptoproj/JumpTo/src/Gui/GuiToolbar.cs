using UnityEditor;
using UnityEngine;


namespace JumpTo
{
	public class GuiToolbar : GuiBase
	{
		private RectRef m_DrawRect = new RectRef();

		private GUIContent m_FirstStateContent = new GUIContent();
		private GUIContent m_OrientationContent = new GUIContent();

		private int m_SelectedView = 0;
		private GUIContent[] m_ViewContent = new GUIContent[3];


		public override void OnWindowEnable(EditorWindow window)
		{
			m_ViewContent[0] = new GUIContent(ResLoad.Instance.GetText(ResId.MenuProjectView));
			m_ViewContent[1] = new GUIContent(ResLoad.Instance.GetText(ResId.MenuHierarchyView));
			m_ViewContent[2] = new GUIContent(ResLoad.Instance.GetText(ResId.MenuBothView));

			RefreshFirstStateButton();
			RefreshOrientationButton();
			RefreshVisibilityPopup();
		}

		protected override void OnGui()
		{
			//NOTE: the toolbar style has, by default, a fixed height of 18.
			//		this must be taken into account when drawing a toolbar
			GUIStyle style = GraphicAssets.Instance.ToolbarStyle;
			m_DrawRect.Set(0.0f, style.contentOffset.y, m_Size.x, style.fixedHeight);
			GUI.Box(m_DrawRect, GUIContent.none, style);

			//draw first state button
			m_DrawRect.x = 6.0f;
			m_DrawRect.width = 26.0f;
			style = GraphicAssets.Instance.ToolbarButtonStyle;
			if (GUI.Button(m_DrawRect, m_FirstStateContent, style))
			{
				JumpToSettings.Instance.ProjectFirst = !JumpToSettings.Instance.ProjectFirst;
				RefreshFirstStateButton();
			}

			//draw orientation button
			m_DrawRect.x += m_DrawRect.width;
			m_DrawRect.width = 24.0f;
			if (GUI.Button(m_DrawRect, m_OrientationContent, style))
			{
				JumpToSettings.Instance.Vertical = !JumpToSettings.Instance.Vertical;
				RefreshOrientationButton();
			}

			//m_DrawRect.x += m_DrawRect.width;
			//if (GUI.Button(m_DrawRect, "Save", style))
			//{
			//}

			//m_DrawRect.x += m_DrawRect.width;
			//if (GUI.Button(m_DrawRect, "Load", style))
			//{
			//	m_SelectedView = (int)JumpToSettings.Instance.Visibility;
			//}

			//draw visibility popup
			style = GraphicAssets.Instance.ToolbarPopupStyle;
			m_DrawRect.width = 70.0f;
			m_DrawRect.x = m_Size.x - (m_DrawRect.width + 6.0f);
			m_SelectedView = EditorGUI.Popup(m_DrawRect, m_SelectedView, m_ViewContent, style);
			if (GUI.changed)
			{
				JumpToSettings.Instance.Visibility = (JumpToSettings.VisibleList)m_SelectedView;
			}
		}

		private void RefreshFirstStateButton()
		{
			if (JumpToSettings.Instance.ProjectFirst)
			{
				m_FirstStateContent.tooltip = ResLoad.Instance.GetText(ResId.TooltipProjectFirst);
				m_FirstStateContent.image = GraphicAssets.Instance.IconProjectView;
			}
			else
			{
				m_FirstStateContent.tooltip = ResLoad.Instance.GetText(ResId.TooltipHierarchyFirst);
				m_FirstStateContent.image = GraphicAssets.Instance.IconHierarchyView;
			}
		}

		private void RefreshOrientationButton()
		{
			if (JumpToSettings.Instance.Vertical)
			{
				m_OrientationContent.tooltip = ResLoad.Instance.GetText(ResId.TooltipVertical);
				m_OrientationContent.image = ResLoad.Instance.GetImage(ResId.ImageVerticalView);
			}
			else
			{
				m_OrientationContent.tooltip = ResLoad.Instance.GetText(ResId.TooltipHorizontal);
				m_OrientationContent.image = ResLoad.Instance.GetImage(ResId.ImageHorizontalView);
			}
		}

		private void RefreshVisibilityPopup()
		{
			m_SelectedView = (int)JumpToSettings.Instance.Visibility;
		}
	}
}
