using UnityEditor;
using UnityEngine;


namespace ImpRock.JumpTo.Editor
{
	internal sealed class GuiToolbar : GuiBase
	{
		//private Rect m_DrawRect;

		//private GUIContent m_FirstStateContent = new GUIContent();
		//private GUIContent m_OrientationContent = new GUIContent();

		//private int m_SelectedView = 0;
		//private GUIContent[] m_ViewContent = new GUIContent[3];

		//private JumpToEditorWindow m_Window;


		//public override void OnWindowEnable(EditorWindow window)
		//{
		//	m_Window = window as JumpToEditorWindow;

		//	m_ViewContent[0] = new GUIContent(JumpToResources.Instance.GetText(ResId.MenuProjectView));
		//	m_ViewContent[1] = new GUIContent(JumpToResources.Instance.GetText(ResId.MenuHierarchyView));
		//	m_ViewContent[2] = new GUIContent(JumpToResources.Instance.GetText(ResId.MenuBothView));

		//	RefreshFirstStateButton();
		//	RefreshOrientationButton();
		//	RefreshVisibilityPopup();
		//}

		protected override void OnGui()
		{
		//	//NOTE: the toolbar style has, by default, a fixed height of 18.
		//	//		this must be taken into account when drawing a toolbar
		//	GUIStyle style = GraphicAssets.Instance.ToolbarStyle;
		//	if (style == null)
		//		Debug.Log("style is null");
		//	m_DrawRect.Set(0.0f, style.contentOffset.y, m_Size.x, style.fixedHeight);
		//	GUI.Box(m_DrawRect, GUIContent.none, style);

		//	//draw first state button
		//	m_DrawRect.x = 6.0f;
		//	m_DrawRect.width = 26.0f;
		//	style = GraphicAssets.Instance.ToolbarButtonStyle;
		//	if (GUI.Button(m_DrawRect, m_FirstStateContent, style))
		//	{
		//		m_Window.JumpToSettingsInstance.ProjectFirst = !m_Window.JumpToSettingsInstance.ProjectFirst;
		//		RefreshFirstStateButton();
		//	}

		//	//draw orientation button
		//	m_DrawRect.x += m_DrawRect.width;
		//	m_DrawRect.width = 24.0f;
		//	if (GUI.Button(m_DrawRect, m_OrientationContent, style))
		//	{
		//		m_Window.JumpToSettingsInstance.Vertical = !m_Window.JumpToSettingsInstance.Vertical;
		//		RefreshOrientationButton();
		//		m_Window.RefreshMinSize();
		//	}

		//	//m_DrawRect.x += m_DrawRect.width;
		//	//if (GUI.Button(m_DrawRect, "Save", style))
		//	//{
		//	//}

		//	//m_DrawRect.x += m_DrawRect.width;
		//	//if (GUI.Button(m_DrawRect, "Load", style))
		//	//{
		//	//	m_SelectedView = (int)m_Window.JumpToSettingsInstance.Visibility;
		//	//}

		//	//draw visibility popup
		//	style = GraphicAssets.Instance.ToolbarPopupStyle;
		//	m_DrawRect.width = 70.0f;
		//	m_DrawRect.x = m_Size.x - (m_DrawRect.width + 6.0f);
		//	m_SelectedView = EditorGUI.Popup(m_DrawRect, m_SelectedView, m_ViewContent, style);
		//	if (GUI.changed)
		//	{
		//		m_Window.JumpToSettingsInstance.Visibility = (JumpToSettings.VisibleList)m_SelectedView;
		//		m_Window.RefreshMinSize();
		//	}
		}

		//private void RefreshFirstStateButton()
		//{
		//	if (m_Window.JumpToSettingsInstance.ProjectFirst)
		//	{
		//		m_FirstStateContent.tooltip = JumpToResources.Instance.GetText(ResId.TooltipProjectFirst);
		//		m_FirstStateContent.image = GraphicAssets.Instance.IconProjectView;
		//	}
		//	else
		//	{
		//		m_FirstStateContent.tooltip = JumpToResources.Instance.GetText(ResId.TooltipHierarchyFirst);
		//		m_FirstStateContent.image = GraphicAssets.Instance.IconHierarchyView;
		//	}
		//}

		//private void RefreshOrientationButton()
		//{
		//	if (m_Window.JumpToSettingsInstance.Vertical)
		//	{
		//		m_OrientationContent.tooltip = JumpToResources.Instance.GetText(ResId.TooltipVertical);
		//		m_OrientationContent.image = JumpToResources.Instance.GetImage(ResId.ImageVerticalView);
		//	}
		//	else
		//	{
		//		m_OrientationContent.tooltip = JumpToResources.Instance.GetText(ResId.TooltipHorizontal);
		//		m_OrientationContent.image = JumpToResources.Instance.GetImage(ResId.ImageHorizontalView);
		//	}
		//}

		//private void RefreshVisibilityPopup()
		//{
		//	m_SelectedView = (int)m_Window.JumpToSettingsInstance.Visibility;
		//}
	}
}
