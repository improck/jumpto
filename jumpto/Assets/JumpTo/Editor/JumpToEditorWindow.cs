﻿using UnityEditor;
using UnityEngine;
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
	[System.NonSerialized] private bool m_Initialized = false;

	[SerializeField] private JumpLinks m_JumpLinks;
	[SerializeField] private GuiJumpLinkView m_View;


	void OnEnable()
	{
		m_JumpLinks = JumpLinks.Instance;

		if (m_JumpLinks == null)
		{
			m_JumpLinks = ScriptableObject.CreateInstance<JumpLinks>();
			m_JumpLinks.hideFlags = HideFlags.HideAndDontSave;
		}

		if (m_View == null)
		{
			m_View = ScriptableObject.CreateInstance<GuiJumpLinkListView>();
			m_View.hideFlags = HideFlags.HideAndDontSave;
		}

		//m_IconBackground = EditorGUIUtility.FindTexture("me_trans_head_l");
	}

	void OnDisable()
	{
		GraphicAssets.Instance.Cleanup();
	}

	void OnDestroy()
	{
		//Object.DestroyImmediate(m_IconBackground);

		GraphicAssets.Instance.Cleanup();
	}

	private void Init()
	{
		m_Initialized = true;

		GraphicAssets.Instance.InitGuiStyle();
	}

	void OnGUI()
	{
		//NOTE: it's ridiculous that I have to do this here.
		if (!m_Initialized)
			Init();

		m_View.OnGui();
		
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
		//apparently OnFocus() gets called before OnEnable()!
		if (m_JumpLinks != null)
		{
			m_JumpLinks.CheckHierarchyLinks();
			m_JumpLinks.CheckProjectLinks();
		}
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