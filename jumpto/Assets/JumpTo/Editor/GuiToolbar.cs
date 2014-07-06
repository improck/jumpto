using UnityEditor;
using UnityEngine;
using System.Collections;


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
			//NOTE: the toolbar style has, by default, an offset of (0, -1) and a fixed height of 18
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

	//***** OLD ONGUI *****
	//switch (Event.current.type)
	//{
	//case EventType.Layout:
	//	{
	//		//NOTE: for all native toolbars, space to first button is 6.0f
	//		m_DrawRect.Set(6.0f, 0.0f, 40.0f, m_Size.y);
	//		m_FirstStateButton.Layout(m_DrawRect);

	//		m_DrawRect.x += m_DrawRect.width;
	//		m_OrientationButton.Layout(m_DrawRect);
	//	}
	//	break;
	//case EventType.MouseDown:
	//	{
	//		if (m_FirstStateButton.HitTest(Event.current.mousePosition))
	//		{
	//			m_FirstStateButton.OnMouseDown(Event.current.mousePosition);
	//			Event.current.Use();
	//		}
	//		else if (m_OrientationButton.HitTest(Event.current.mousePosition))
	//		{
	//			m_OrientationButton.OnMouseDown(Event.current.mousePosition);
	//			Event.current.Use();
	//		}
	//	}
	//	break;
	//case EventType.MouseUp:
	//	{
	//		m_FirstStateButton.OnMouseUp(Event.current.mousePosition);
	//		m_OrientationButton.OnMouseUp(Event.current.mousePosition);
	//	}
	//	break;
	//case EventType.MouseMove:
	//	{
	//		m_FirstStateButton.OnMouseMove(Event.current.mousePosition);
	//		m_OrientationButton.OnMouseMove(Event.current.mousePosition);
	//	}
	//	break;
	//case EventType.Repaint:
	//	{
	//		m_DrawRect.Set(0.0f, 0.0f, m_Size.x, m_Size.y);
	//		GraphicAssets.Instance.ToolbarStyle.Draw(m_DrawRect, false, false, false, false);

	//		m_DrawRect.x = 130.0f;
	//		m_DrawRect.width = 40.0f;

	//		//GraphicAssets.Instance.ToolbarButtonStyle.Draw(m_DrawRect, new GUIContent("test"), false, true, false, true);

	//		m_FirstStateButton.Draw();
	//		m_OrientationButton.Draw();
	//	}
	//	break;
	//}
	//*********************

	//public class ToolbarButton
	//{
	//	protected bool m_Hovered = false;
	//	protected bool m_Active = false;
	//	protected bool m_On = false;
	//	protected bool m_Focused = false;
	//	protected bool m_WasActive = false;

	//	protected Rect m_DrawRect;


	//	public GUIContent Content { get; set; }


	//	public virtual void Init()
	//	{
	//		Content = new GUIContent();
	//	}

	//	public virtual void Layout(RectRef drawRect)
	//	{
	//		m_DrawRect = drawRect;
	//	}

	//	public virtual void Draw()
	//	{
	//		//TODO: why aren't the active textures rendering?
	//		GraphicAssets.Instance.ToolbarButtonStyle.Draw(m_DrawRect, Content, m_Hovered, m_Active, m_On, m_Focused);
	//	}

	//	public bool HitTest(Vector2 mousePosition)
	//	{
	//		return m_DrawRect.Contains(mousePosition);
	//	}

	//	public virtual void OnMouseDown(Vector2 mousePosition)
	//	{
	//		Debug.Log("Clicked: " + Content.tooltip);
	//		m_WasActive = m_Active = true;
	//	}

	//	public virtual void OnMouseUp(Vector2 mousePosition)
	//	{
	//		m_WasActive = m_Active = false;
	//	}

	//	public virtual void OnMouseMove(Vector2 mousePosition)
	//	{
	//		if (m_WasActive)
	//		{
	//			m_Active = m_DrawRect.Contains(mousePosition);
	//		}
	//	}
	//}

	//public class ToolbarToggleButton : ToolbarButton
	//{
	//	public override void OnMouseUp(Vector2 mousePosition)
	//	{
	//		base.OnMouseUp(mousePosition);
	//		if (m_DrawRect.Contains(mousePosition))
	//			m_On = !m_On;
	//	}
	//}
