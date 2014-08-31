using UnityEditor;
using UnityEngine;
using JumpTo;
using SceneStateDetection;


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
//xTODO: clear all links from gui
//xTODO: update on scene change
//xTODO: update on project change
//xTODO: change the view control look-and-feel
//xTODO: detect a scene load
//xTODO: serialize to a file
//TODO: draw a grab bar as the divider, horizontal and vertical
//TODO: left-click hamburger menu for list view control
//TODO: move to a dll assembly
//TODO: assembly resource text, multiple languages
//TODO: load images from assembly resources
//TODO: find the minimum editorwindow width
//TODO: comment all of this code


public class JumpToEditorWindow : EditorWindow
{
	[SerializeField] private JumpLinks m_JumpLinks;
	[SerializeField] private JumpToSettings m_Settings;
	[SerializeField] private GuiToolbar m_Toolbar;
	[SerializeField] private GuiJumpLinkListView m_View;
	[SerializeField] private SceneStateControl m_SceneState;
	[SerializeField] private bool m_FirstOpen = true;

	[System.NonSerialized] private bool m_Initialized = false;
	[System.NonSerialized] private RectRef m_Position = new RectRef();
	[System.NonSerialized] private double m_LastHierarchyRefreshTime = 0.0f;


	public static event EditorApplication.CallbackFunction OnWindowOpen;
	public static event EditorApplication.CallbackFunction OnWillEnable;
	public static event EditorApplication.CallbackFunction OnWillDisable;
	public static event EditorApplication.CallbackFunction OnWillClose;


	//called when window is first open
	//called before deserialization due to a compile
	//public JumpToEditorWindow()
	//{
	//	SerializationControl.CreateInstance();

	//	if (OnWindowOpen != null)
	//		OnWindowOpen();
	//}

	//called when window is first open
	//called after deserialization due to a compile
	void OnEnable()
	{
		Debug.Log("OnEnable()");
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

		if (m_SceneState == null)
		{
			m_SceneState = SceneStateControl.Create();
		}

		SceneLoadDetector.EnsureExistence();

		SerializationControl.CreateInstance();

		m_JumpLinks.RefreshProjectLinks();
		m_JumpLinks.RefreshHierarchyLinks();
		m_LastHierarchyRefreshTime = EditorApplication.timeSinceStartup;

		m_Toolbar.OnWindowEnable(this);
		m_View.OnWindowEnable(this);

		EditorApplication.projectWindowChanged += OnProjectWindowChange;
		EditorApplication.hierarchyWindowChanged += OnHierarchyWindowChange;
		SceneStateControl.OnSceneLoaded += OnSceneLoaded;

		//NOTE: this DOES NOT WORK because closing unity will serialize the
		//		jumpto window if it's open. that means that when unity is
		//		restarted, the m_FirstOpen flag gets deserialized to equal
		//		FALSE.
		//Debug.Log("first open = " + m_FirstOpen);
		//if (m_FirstOpen)
		//{
		//	m_FirstOpen = false;
		//
		//	if (OnWindowOpen != null)
		//		OnWindowOpen();
		//}

		if (OnWillEnable != null)
			OnWillEnable();
	}
	
	//called before window closes
	//called before serialization due to a compile
	void OnDisable()
	{
		if (OnWillDisable != null)
			OnWillDisable();

		SceneLoadDetector.TemporarilyDestroyInstance();

		m_View.OnWindowDisable(this);

		EditorApplication.projectWindowChanged -= OnProjectWindowChange;
		EditorApplication.hierarchyWindowChanged -= OnHierarchyWindowChange;
		SceneStateControl.OnSceneLoaded -= OnSceneLoaded;

		Debug.Log("OnDisable()");
	}

	void OnLostFocus()
	{
		Debug.Log("OnLostFocus()");
	}

	//NOT called when unity editor is closed
	//called when window is closed
	void OnDestroy()
	{
		Debug.Log("OnDestroy()");

		if (OnWillClose != null)
			OnWillClose();

		//NOTE: this doesn't get called on editor close!
		//using (System.IO.StreamWriter streamWriter = new System.IO.StreamWriter(@"r:\testfile.txt"))
		//{
		//	streamWriter.WriteLine("called from OnDestroy()");
		//}

		m_View.OnWindowClose(this);

		//m_FirstOpen = true;

		GraphicAssets.Instance.Cleanup();
		GraphicAssets.DestroyInstance();

		SceneLoadDetector.PermanentlyDestroyInstance();

		SerializationControl.DestroyInstance();
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
		m_Position.Set(0.0f, 0.0f, position.width, 17.0f);
		m_Toolbar.Draw(m_Position);

		m_Position.y = m_Position.height;
		m_Position.height = position.height - m_Position.height;
		m_View.Draw(m_Position);
	}

	void OnFocus()
	{
		Debug.Log("OnFocus()");

		//apparently OnFocus() gets called before OnEnable()!
		if (m_JumpLinks != null)
		{
			m_JumpLinks.RefreshHierarchyLinks();
			m_JumpLinks.RefreshProjectLinks();
		}
	}

	//***** Only called if window is visible *****
	//void OnHierarchyChange()
	//{
	//	Debug.Log("Hierarchy changed");
	//	m_JumpLinks.RefreshHierarchyLinks();
	//	Repaint();
	//}

	//void OnProjectChange()
	//{
	//	Debug.Log("Project changed");
	//	m_JumpLinks.RefreshProjectLinks();
	//	Repaint();
	//}

	//void OnDidOpenScene()
	//{
	//	Debug.Log("OnDidOpenScene(): " + EditorApplication.currentScene);
	//}
	//********************************************

	private void OnProjectWindowChange()
	{
		Debug.Log("Project Window Changed");
		m_JumpLinks.RefreshProjectLinks();
		Repaint();
	}

	private void OnHierarchyWindowChange()
	{
		if (EditorApplication.isPlaying)
			return;

		if (EditorApplication.timeSinceStartup - m_LastHierarchyRefreshTime >= 0.2f)
		{
			m_LastHierarchyRefreshTime = EditorApplication.timeSinceStartup;

			Debug.Log("Hierarchy Window Changed");
			m_JumpLinks.RefreshHierarchyLinks();
			Repaint();
		}
	}

	private void OnSceneLoaded(string sceneAssetPath)
	{
		Repaint();
	}


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


	public static JumpToEditorWindow GetOrCreateWindow()
	{
		return EditorWindow.GetWindow<JumpToEditorWindow>("Jump To");
	}

	public static void RepaintOpenWindows()
	{
		JumpToEditorWindow[] windows = Resources.FindObjectsOfTypeAll<JumpToEditorWindow>();
		if (windows != null && windows.Length > 0)
		{
			for (int i = 0; i < windows.Length; i++)
			{
				windows[i].Repaint();
			}
		}
	}
	
	[MenuItem("Tools/Jump To")]
	public static void JumpTo_InitMainMenu()
	{
		JumpToEditorWindow window = GetOrCreateWindow();
		window.Show();
	}

	[MenuItem("Assets/Create/Jump Link", true)]
	public static bool JumpTo_AssetsCreateJumpLink_Validate()
	{
		JumpToEditorWindow[] windows = Resources.FindObjectsOfTypeAll<JumpToEditorWindow>();

		Object[] selected = Selection.objects;
		return windows.Length > 0 && JumpLinks.Instance != null && selected != null && selected.Length > 0;
	}

	[MenuItem("Assets/Create/Jump Link", false)]
	public static void JumpTo_AssetsCreateJumpLink()
	{
		Object[] selected = Selection.objects;
		JumpToEditorWindow window = GetOrCreateWindow();
		window.CreateMultipleJumpLinks(selected);
	}

	[MenuItem("GameObject/Create Other/Jump Link", true)]
	public static bool JumpTo_GameObjectCreateOtherJumpLink_Validate()
	{
		JumpToEditorWindow[] windows = Resources.FindObjectsOfTypeAll<JumpToEditorWindow>();

		Object[] selected = Selection.objects;
		return windows.Length > 0 && JumpLinks.Instance != null && selected != null && selected.Length > 0;
	}

	[MenuItem("GameObject/Create Other/Jump Link", false)]
	public static void JumpTo_GameObjectCreateOtherJumpLink()
	{
		Object[] selected = Selection.objects;
		JumpToEditorWindow window = GetOrCreateWindow();
		window.CreateMultipleJumpLinks(selected);
	}
}
