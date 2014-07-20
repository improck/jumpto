using UnityEditor;
using UnityEngine;
using JumpTo;


//xTODO: make items clickable
//xTODO: project and hierarchy foldouts
//xTODO: replace GUILayout.Label with m_LinkLabelStyle.Draw()
//xTODO: ability to remove items
//xTODO: context menu
//xTODO: drag-n-drop to window
//xTODO: double-click behavior
//xTODO: serialize links
//xTODO: create a toolbar
//xTODO: drag-n-drop from project window
//xTODO: drag-n-drop from hierarchy window
//xTODO: multiple selection
//TODO: update on scene change?
//TODO: update on project change?
//TODO: serialize to a file
//TODO: clear all links from gui
//TODO: move to a dll assembly
//TODO: assembly resource text, multiple languages
//TODO: load images from assembly resources
//TODO: comment all of this code


public class JumpToEditorWindow : EditorWindow
{
	[System.NonSerialized] private bool m_Initialized = false;

	[SerializeField] private JumpLinks m_JumpLinks;
	[SerializeField] private JumpToSettings m_Settings;
	[SerializeField] private GuiToolbar m_Toolbar;
	[SerializeField] private GuiJumpLinkListView m_View;

	[System.NonSerialized] private RectRef m_Position = new RectRef();


	void OnEnable()
	{
		m_JumpLinks = JumpLinks.Instance;

		if (m_JumpLinks == null)
		{
			m_JumpLinks = JumpLinks.Create();
		}

		m_Settings = JumpToSettings.Instance;

		if (m_Settings == null)
		{
			m_Settings = JumpToSettings.Create();
		}

		if (m_Toolbar == null)
		{
			m_Toolbar = GuiBase.Create<GuiToolbar>();
		}

		if (m_View == null)
		{
			m_View = GuiBase.Create<GuiJumpLinkListView>();
		}

		if (m_Settings.Visibility == JumpToSettings.VisibleList.ProjectAndHierarchy)
		{
			if (m_Settings.Vertical)
			{
				this.minSize = new Vector2(120.0f, GuiJumpLinkListView.DividerMin * 2.0f);
			}
			else
			{
				this.minSize = new Vector2(240.0f, GuiJumpLinkListView.DividerMin);
			}
		}
		else
		{
			this.minSize = new Vector2(120.0f, GuiJumpLinkListView.DividerMin);
		}

		m_JumpLinks.CheckHierarchyLinks();
		m_JumpLinks.CheckProjectLinks();

		m_Toolbar.OnWindowEnable(this);
		m_View.OnWindowEnable(this);
	}

	void OnDisable()
	{
		m_View.OnWindowDisable(this);
	}

	void OnDestroy()
	{
		m_View.OnWindowClose(this);

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

		//position.x & y are the position of the window in Unity, i think
		//	maybe it's the window position on the desktop
		//	either way it wasn't the value i expected, so i force (0, 0)
		m_Position.Set(0.0f, 0.0f, position.width, 18.0f);
		m_Toolbar.Draw(m_Position);

		m_Position.y = m_Position.height;
		m_Position.height = position.height - m_Position.height;
		m_View.Draw(m_Position);
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


	private void CreateMultipleJumpLinks(UnityEngine.Object[] linkReferences)
	{
		#region Removed Filtering
		//NOTE: already fully tested, but decided against it due
		//		to a potentially negative user experience
		//switch (JumpToSettings.Instance.Visibility)
		//{
		//case JumpToSettings.VisibleList.ProjectAndHierarchy:
		//	{
		//		for (int i = 0; i < linkReferences.Length; i++)
		//		{
		//			m_JumpLinks.CreateJumpLink(linkReferences[i]);
		//		}
		//	}
		//	break;
		//case JumpToSettings.VisibleList.HierarchyOnly:
		//	{
		//		for (int i = 0; i < linkReferences.Length; i++)
		//		{
		//			m_JumpLinks.CreateOnlyHierarchyJumpLink(linkReferences[i]);
		//		}
		//	}
		//	break;
		//case JumpToSettings.VisibleList.ProjectOnly:
		//	{
		//		for (int i = 0; i < linkReferences.Length; i++)
		//		{
		//			m_JumpLinks.CreateOnlyProjectJumpLink(linkReferences[i]);
		//		}
		//	}
		//	break;
		//}
		#endregion

		for (int i = 0; i < linkReferences.Length; i++)
		{
			m_JumpLinks.CreateJumpLink(linkReferences[i]);
		}

		Repaint();
	}

	
	
	[MenuItem("Tools/Jump To")]
	public static void JumpTo_InitMainMenu()
	{
		JumpToEditorWindow window = EditorWindow.GetWindow<JumpToEditorWindow>("Jump To");
		window.Show();
	}

	[MenuItem("Assets/Create/Jump Link", true)]
	public static bool JumpTo_AssetsCreateJumpLink_Validate()
	{
		Object[] selected = Selection.objects;
		return JumpLinks.Instance != null && selected != null && selected.Length > 0;
	}

	[MenuItem("Assets/Create/Jump Link", false)]
	public static void JumpTo_AssetsCreateJumpLink()
	{
		Object[] selected = Selection.objects;
		JumpToEditorWindow window = EditorWindow.GetWindow<JumpToEditorWindow>("Jump To");
		window.CreateMultipleJumpLinks(selected);
	}

	[MenuItem("GameObject/Create Other/Jump Link", true)]
	public static bool JumpTo_GameObjectCreateOtherJumpLink_Validate()
	{
		Object[] selected = Selection.objects;
		return JumpLinks.Instance != null && selected != null && selected.Length > 0;
	}

	[MenuItem("GameObject/Create Other/Jump Link", false)]
	public static void JumpTo_GameObjectCreateOtherJumpLink()
	{
		Object[] selected = Selection.objects;
		JumpToEditorWindow window = EditorWindow.GetWindow<JumpToEditorWindow>("Jump To");
		window.CreateMultipleJumpLinks(selected);
	}
}
