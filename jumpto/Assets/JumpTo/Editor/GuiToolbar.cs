﻿using UnityEditor;
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
			m_DrawRect.width = 40.0f;
			style = GraphicAssets.Instance.ToolbarButtonStyle;
			if (GUI.Button(m_DrawRect, m_FirstStateContent, style))
			{
				JumpToSettings.Instance.ProjectFirst = !JumpToSettings.Instance.ProjectFirst;
				RefreshFirstStateButton();
			}

			//draw orientation button
			m_DrawRect.x += m_DrawRect.width;
			if (GUI.Button(m_DrawRect, m_OrientationContent, style))
			{
				JumpToSettings.Instance.Vertical = !JumpToSettings.Instance.Vertical;
				RefreshOrientationButton();
			}

			m_DrawRect.x += m_DrawRect.width;
			if (GUI.Button(m_DrawRect, "Save", style))
			{
				JumpToSettings.Save();
				//JumpLinks.Save();
			}

			m_DrawRect.x += m_DrawRect.width;
			if (GUI.Button(m_DrawRect, "Load", style))
			{
				JumpToSettings.Load();
				m_SelectedView = (int)JumpToSettings.Instance.Visibility;

				//SerializationControl.Instance.LoadHierarchyLinks();
			}

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
			}
			else
			{
				m_FirstStateContent.tooltip = ResLoad.Instance.GetText(ResId.TooltipHierarchyFirst);
			}
		}

		private void RefreshOrientationButton()
		{
			if (JumpToSettings.Instance.Vertical)
			{
				m_OrientationContent.tooltip = ResLoad.Instance.GetText(ResId.TooltipVertical);
			}
			else
			{
				m_OrientationContent.tooltip = ResLoad.Instance.GetText(ResId.TooltipHorizontal);
			}
		}

		private void RefreshVisibilityPopup()
		{
			m_SelectedView = (int)JumpToSettings.Instance.Visibility;
		}
	}
}
