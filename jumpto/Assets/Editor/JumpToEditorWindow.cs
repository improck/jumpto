using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using JumpTo;


//xTODO: make items clickable
//xTODO: project and hierarchy foldouts
//TODO: replace GUILayout.Label with m_LinkLabelStyle.Draw()
//TODO: ability to remove items
//TODO: context menu
//TODO: drag-n-drop to/from window
//TODO: serialize links
//TODO: update on scene change?
//TODO: update on project change?


public class JumpToEditorWindow : EditorWindow
{
	private bool m_Initialized = false;

	[SerializeField] private bool m_ProjectLinksUnfolded = true;
	[SerializeField] private bool m_HierarchyLinksUnfolded = true;

	[SerializeField] private JumpLinks m_JumpLinks;


	void OnEnable()
	{
		//m_JumpLinks = JumpLinks.Instance;
		//m_IconBackground = EditorGUIUtility.FindTexture("me_trans_head_l");
	}

	void OnDestroy()
	{
		//Object.DestroyImmediate(m_IconBackground);
	}

	private void Init()
	{
		m_Initialized = true;

		JumpTo.GraphicAssets.Instance.InitGuiStyle();
	}

	void OnGUI()
	{
		//NOTE: it's ridiculous that I have to do this here.
		if (!m_Initialized)
			Init();

		Vector2 iconSizeBak = EditorGUIUtility.GetIconSize();
		EditorGUIUtility.SetIconSize(GraphicAssets.Instance.IconSize);

		Color bgColor = GUI.backgroundColor;

		m_ProjectLinksUnfolded = EditorGUILayout.Foldout(m_ProjectLinksUnfolded, "Project References");
		if (m_ProjectLinksUnfolded)
		{
			for (int i = 0; i < m_JumpLinks.ProjectLinks.Count; i++)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Space(17.0f);

				//m_LinkLabelStyle.normal.textColor = m_ProjectReferences[i].LinkColor;
				//if (m_ProjectReferences[i] == m_SelectedObject)
				//{
				//	m_LinkLabelStyle.normal.background = m_IconBackground;
				//	GUI.backgroundColor = Color.cyan;
				//}
				//else
				//{
				//	m_LinkLabelStyle.normal.background = null;
				//	GUI.backgroundColor = bgColor;
				//}

				GUILayout.Label(m_JumpLinks.ProjectLinks[i].LinkLabelContent, GraphicAssets.Instance.LinkLabelStyle, GUILayout.Height(JumpLinks.LinkHeight));
				m_JumpLinks.ProjectLinks[i].Area = GUILayoutUtility.GetLastRect();

				//m_ProjectReferences[i].Area.x = 16.0f;
				//m_ProjectReferences[i].Area.y = 16.0f * i + 16.0f;
				//m_ProjectReferences[i].Area.width = position.width;
				//m_ProjectReferences[i].Area.height = 16.0f;
				//m_LinkLabelStyle.Draw(m_ProjectReferences[i].Area,
				//	m_ProjectReferences[i].LinkLabelContent,
				//	false, false,
				//	m_ProjectReferences[i] == m_SelectedObject,
				//	m_ProjectReferences[i] == m_SelectedObject);

				GUILayout.EndHorizontal();
			}
		}

		EditorGUILayout.Separator();

		m_HierarchyLinksUnfolded = EditorGUILayout.Foldout(m_HierarchyLinksUnfolded, "Hierarchy References");
		if (m_HierarchyLinksUnfolded)
		{
			for (int i = 0; i < m_JumpLinks.HierarchyLinks.Count; i++)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Space(17.0f);

				//m_LinkLabelStyle.normal.textColor = m_HierarchyReferences[i].LinkColor;
				//if (m_HierarchyReferences[i] == m_SelectedObject)
				//{
				//	m_LinkLabelStyle.normal.background = m_IconBackground;
				//	GUI.backgroundColor = Color.cyan;
				//}
				//else
				//{
				//	m_LinkLabelStyle.normal.background = null;
				//	GUI.backgroundColor = bgColor;
				//}

				GUILayout.Label(m_JumpLinks.HierarchyLinks[i].LinkLabelContent, GraphicAssets.Instance.LinkLabelStyle, GUILayout.Height(JumpLinks.LinkHeight));
				m_JumpLinks.HierarchyLinks[i].Area = GUILayoutUtility.GetLastRect();

				GUILayout.EndHorizontal();
			}
		}

		GUI.backgroundColor = bgColor;
		EditorGUIUtility.SetIconSize(iconSizeBak);

		//Event currentEvent = Event.current;
		//switch (currentEvent.type)
		//{
		//case EventType.MouseDown:
		//	{
		//		m_MouseDownObject = null;
		//		Vector2 mousePos = currentEvent.mousePosition;
		//		if (m_ProjectLinksUnfolded)
		//		{
		//			for (int i = 0; i < m_ProjectReferences.Count; i++)
		//			{
		//				if (m_ProjectReferences[i].Area.Contains(mousePos))
		//				{
		//					m_MouseDownObject = m_ProjectReferences[i];
		//					break;
		//				}
		//			}
		//		}

		//		if (m_MouseDownObject == null && m_HierarchyLinksUnfolded)
		//		{
		//			for (int i = 0; i < m_HierarchyReferences.Count; i++)
		//			{
		//				if (m_HierarchyReferences[i].Area.Contains(mousePos))
		//				{
		//					m_MouseDownObject = m_HierarchyReferences[i];
		//					break;
		//				}
		//			}
		//		}
		//	}
		//	break;
		//case EventType.MouseUp:
		//	{
		//		m_SelectedObject = null;
		//		Vector2 mousePos = currentEvent.mousePosition;
		//		if (m_ProjectLinksUnfolded)
		//		{
		//			for (int i = 0; i < m_ProjectReferences.Count; i++)
		//			{
		//				if (m_ProjectReferences[i].Area.Contains(mousePos) &&
		//					m_ProjectReferences[i] == m_MouseDownObject)
		//				{
		//					m_SelectedObject = m_ProjectReferences[i];
		//					m_MouseDownObject = null;
		//					Repaint();
		//					break;
		//				}
		//			}
		//		}

		//		if (m_SelectedObject == null && m_HierarchyLinksUnfolded)
		//		{
		//			for (int i = 0; i < m_HierarchyReferences.Count; i++)
		//			{
		//				if (m_HierarchyReferences[i].Area.Contains(mousePos) &&
		//					m_HierarchyReferences[i] == m_MouseDownObject)
		//				{
		//					m_SelectedObject = m_HierarchyReferences[i];
		//					m_MouseDownObject = null;
		//					Repaint();
		//					break;
		//				}
		//			}
		//		}

		//		if (m_SelectedObject != null)
		//		{
		//			Debug.Log("Selected " + m_SelectedObject.LinkLabelContent.text);
		//		}
		//	}
		//	break;
		////case EventType.Layout:
		////	{
				
		////	}
		////	break;
		//}
	}

	void OnFocus()
	{
		m_JumpLinks = JumpLinks.Instance;
		m_JumpLinks.CheckHierarchyLinks();
		m_JumpLinks.CheckProjectLinks();
	}

	//***** Only called if focused *****
	void OnHierarchyChange()
	{
		Debug.Log("Hierarchy changed");
		m_JumpLinks.CheckHierarchyLinks();
		Repaint();
	}

	void OnProjectChange()
	{
		Debug.Log("Project changed");
		m_JumpLinks.CheckProjectLinks();
		Repaint();
	}
	//**********************************


	private void CreateMultipleJumpLinks(UnityEngine.Object[] linkObjects)
	{
		for (int i = 0; i < linkObjects.Length; i++)
		{
			m_JumpLinks.CreateJumpLink(linkObjects[i]);
		}
	}

	
	
	[MenuItem("Tools/Jump To")]
	public static void JumpTo_InitMainMenu()
	{
		JumpToEditorWindow window = EditorWindow.GetWindow<JumpToEditorWindow>("Jump To");
		//window.m_SerializedObject = new SerializedObject(window);
		window.Show();
	}

	[MenuItem("Assets/Create/Jump Link", true)]
	public static bool JumpTo_CreateJumpLink_Validate()
	{
		Object[] selected = Selection.objects;
		return selected != null && selected.Length > 0;
	}

	[MenuItem("Assets/Create/Jump Link", false)]
	public static void JumpTo_CreateJumpLink()
	{
		Object[] selected = Selection.objects;
		JumpToEditorWindow window = EditorWindow.GetWindow<JumpToEditorWindow>("Jump To");
		window.CreateMultipleJumpLinks(selected);
	}
}
