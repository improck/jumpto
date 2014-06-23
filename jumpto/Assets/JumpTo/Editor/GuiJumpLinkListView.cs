﻿using UnityEditor;
using UnityEngine;


namespace JumpTo
{
	public class GuiJumpLinkListView : GuiBase
	{
		[SerializeField] private float m_Divider = 0.5f;
		[SerializeField] private GuiProjectJumpLinkView m_ProjectView;
		[SerializeField] private GuiHierarchyJumpLinkView m_HierarchyView;

		private RectRef m_DrawRect = new RectRef();
		private Rect m_DividerRect;

		private const float DividerHalfThickness = 3.0f;
		public const float DividerMin = 74.0f;

		private readonly Vector2 IconSize = new Vector2(16.0f, 16.0f);
		private readonly int DividerHash = "divider".GetHashCode();


		public override void OnWindowEnable(EditorWindow window)
		{
			if (m_ProjectView == null)
			{
				m_ProjectView = GuiBase.Create<GuiProjectJumpLinkView>();
			}

			if (m_HierarchyView == null)
			{
				m_HierarchyView = GuiBase.Create<GuiHierarchyJumpLinkView>();
			}

			m_ProjectView.OnWindowEnable(window);
			m_HierarchyView.OnWindowEnable(window);
		}

		protected override void OnGui()
		{
			Vector2 iconSizeBak = EditorGUIUtility.GetIconSize();
			EditorGUIUtility.SetIconSize(IconSize);

			Color bgColorBak = GUI.backgroundColor;

			switch (JumpToSettings.Instance.Visibility)
			{
			case JumpToSettings.VisibleList.ProjectAndHierarchy:
				{
					float adjWidth = m_Size.x;
					float adjHeight = m_Size.y;

					//draw the top/left box
					if (JumpToSettings.Instance.Vertical)
					{
						adjHeight = Mathf.Floor(m_Size.y * m_Divider);
						m_DrawRect.Set(0.0f, 0.0f, m_Size.x, adjHeight - DividerHalfThickness);

						m_DividerRect.x = 0.0f;
						m_DividerRect.y = m_DrawRect.height;
						m_DividerRect.width = m_Size.x;
						m_DividerRect.height = DividerHalfThickness * 2.0f;
					}
					else
					{
						adjWidth = Mathf.Floor(m_Size.x * m_Divider);
						m_DrawRect.Set(0.0f, 0.0f, adjWidth - DividerHalfThickness, m_Size.y);

						m_DividerRect.x = m_DrawRect.width;;
						m_DividerRect.y = 0.0f;
						m_DividerRect.width = DividerHalfThickness * 2.0f;
						m_DividerRect.height = m_Size.y;
					}

					if (JumpToSettings.Instance.ProjectFirst)
						m_ProjectView.Draw(m_DrawRect);
					else
						m_HierarchyView.Draw(m_DrawRect);
					
					//TODO: draw a divider

					//draw the bottom/right box
					if (JumpToSettings.Instance.Vertical)
					{
						m_DrawRect.Set(0.0f, adjHeight + DividerHalfThickness, m_Size.x, (m_Size.y - adjHeight) - DividerHalfThickness);
					}
					else
					{
						m_DrawRect.Set(adjWidth + DividerHalfThickness, 0.0f, (m_Size.x - adjWidth) - DividerHalfThickness, m_Size.y);
					}

					if (JumpToSettings.Instance.ProjectFirst)
						m_HierarchyView.Draw(m_DrawRect);
					else
						m_ProjectView.Draw(m_DrawRect);

					OnDividerGui();
				}
				break;
			case JumpToSettings.VisibleList.ProjectOnly:
				{
					m_DrawRect.Set(0.0f, 0.0f, m_Size.x, m_Size.y);

					m_ProjectView.Draw(m_DrawRect);
				}
				break;
			case JumpToSettings.VisibleList.HierarchyOnly:
				{
					m_DrawRect.Set(0.0f, 0.0f, m_Size.x, m_Size.y);

					m_HierarchyView.Draw(m_DrawRect);
				}
				break;
			}

			GUI.backgroundColor = bgColorBak;

			EditorGUIUtility.SetIconSize(iconSizeBak);
		}

		private void OnDividerGui()
		{
			int controlId = GUIUtility.GetControlID(DividerHash, FocusType. Passive, m_DividerRect);

			if (JumpToSettings.Instance.Vertical)
				EditorGUIUtility.AddCursorRect(m_DividerRect, MouseCursor.SplitResizeUpDown, controlId);
			else
				EditorGUIUtility.AddCursorRect(m_DividerRect, MouseCursor.SplitResizeLeftRight, controlId);

			Event current = Event.current;
			switch (current.GetTypeForControl(controlId))
			{
			case EventType.MouseDown:
				{
					if (current.button == 0 && m_DividerRect.Contains(current.mousePosition))
					{
						GUIUtility.hotControl = controlId;
						current.Use();
					}
				}
				break;
			case EventType.MouseUp:
				{
					if (GUIUtility.hotControl == controlId)
					{
						GUIUtility.hotControl = 0;
						current.Use();
					}
				}
				break;
			case EventType.MouseDrag:
				{
					if (GUIUtility.hotControl == controlId && current.button == 0)
					{
						if (JumpToSettings.Instance.Vertical)
						{
							m_Divider = Mathf.Clamp(current.mousePosition.y, DividerMin, m_Size.y - DividerMin) / m_Size.y;
						}
						else
						{
							m_Divider = Mathf.Clamp(current.mousePosition.x, DividerMin, m_Size.x - DividerMin) / m_Size.x;
						}

						current.Use();
					}
				}
				break;
			}
		}
	}
}