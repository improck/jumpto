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
//xTODO: move to a dll assembly
//xTODO: draw a grab bar as the divider, horizontal AND vertical
//xTODO: left-click hamburger menu for list view control
//xTODO: assembly resource text, multiple languages
//xTODO: load images from assembly resources
//xTODO: save the divider placement to settings
//xTODO: make some indicator of hierarchy links save state
//xTODO: bug! hamburger icon is null when window is open on startup
//xTODO: check out the drag-n-drop bug on jordan's mac
//xTODO: find the minimum editorwindow size
//TODO: comment all of this code


internal sealed class JumpToEditorWindow : EditorWindow
{
	private static int s_InstanceCount = 0;


	[SerializeField] private JumpLinks m_JumpLinks;
	[SerializeField] private JumpToSettings m_Settings;
	[SerializeField] private GuiToolbar m_Toolbar;
	[SerializeField] private GuiJumpLinkListView m_View;
	[SerializeField] private SceneStateControl m_SceneState;
	//[SerializeField] private bool m_FirstOpen = true;

	[System.NonSerialized] private bool m_Initialized = false;
	[System.NonSerialized] private RectRef m_Position = new RectRef();
	[System.NonSerialized] private double m_LastHierarchyRefreshTime = 0.0f;
	[System.NonSerialized] private SerializationControl m_SerializationControl = null;

	private EditorWindowCachedTitleContentWrapper m_CachedTitleContent = null;


	public JumpLinks JumpLinksInstance { get { return m_JumpLinks; } }
	public JumpToSettings JumpToSettingsInstance { get { return m_Settings; } }
	public SerializationControl SerializationControlInstance { get { return m_SerializationControl; } }


	//NOTE: nobody's using these right now
	//public static event EditorApplication.CallbackFunction OnWindowOpen;
	//public static event EditorApplication.CallbackFunction OnWillEnable;
	//public static event EditorApplication.CallbackFunction OnWillDisable;
	//public static event EditorApplication.CallbackFunction OnWillClose;


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
		s_InstanceCount++;

		//this.title = GetInstanceID().ToString();
		//Debug.Log("OnEnable(): " + GetInstanceID() + " " + s_InstanceCount);

		JumpToResources.Instance.LoadResources();
		GraphicAssets.Instance.InitAssets();

		if (m_JumpLinks == null)
		{
			m_JumpLinks = JumpLinks.Create();
		}

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

		if (m_SceneState == null)
		{
			m_SceneState = SceneStateControl.Create();
		}

		EditorApplication.delayCall += OnPostEnable;

		if (m_SerializationControl == null)
		{
			m_SerializationControl = new SerializationControl();
			m_SerializationControl.Initialize(this);
		}

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

		//NOTE: nobody's using it right now
		//if (OnWillEnable != null)
		//	OnWillEnable();

		m_SerializationControl.OnWindowEnable();

		RefreshMinSize();

		m_JumpLinks.RefreshProjectLinks();
		m_JumpLinks.RefreshHierarchyLinks();
		m_LastHierarchyRefreshTime = EditorApplication.timeSinceStartup;

		m_Toolbar.OnWindowEnable(this);
		m_View.OnWindowEnable(this);

		m_CachedTitleContent = new EditorWindowCachedTitleContentWrapper(this);
		GUIContent titleContent = m_CachedTitleContent.TitleContent;
		titleContent.text = "JumpTo";
		titleContent.image = JumpToResources.Instance.GetImage(ResId.ImageTabIcon);
		//JumpToUtility.SetWindowTitleContent(this, JumpToResources.Instance.GetImage(ResId.ImageTabIcon), "JumpTo");
		//m_CachedWindowTitleContent = JumpToUtility.GetWindowTitleContent(this);
		//m_CachedWindowTitleContent.text = "JumpTo";
		//m_CachedWindowTitleContent.image = JumpToResources.Instance.GetImage(ResId.ImageTabIcon);
	}

	void OnPostEnable()
	{
		if (!EditorApplication.isPlayingOrWillChangePlaymode)
			SceneLoadDetector.EnsureExistence();
		else
		{
			EditorApplication.playmodeStateChanged += OnPlayModeStateChanged;
		}
	}
	
	//called before window closes
	//called before serialization due to a compile
	void OnDisable()
	{
		//NOTE: nobody's using it right now
		//if (OnWillDisable != null)
		//	OnWillDisable();

		m_SerializationControl.OnWindowDisable();

		SceneLoadDetector.TemporarilyDestroyInstance();

		m_View.OnWindowDisable(this);

		EditorApplication.projectWindowChanged -= OnProjectWindowChange;
		EditorApplication.hierarchyWindowChanged -= OnHierarchyWindowChange;
		SceneStateControl.OnSceneLoaded -= OnSceneLoaded;

		m_Initialized = false;

		//Debug.Log("OnDisable(): " + GetInstanceID() + " " + s_InstanceCount);
	}

	//void OnLostFocus()
	//{
	//	Debug.Log("OnLostFocus(): " + GetInstanceID() + " " + s_InstanceCount);
	//}

	//NOT called when unity editor is closed
	//called when window is closed
	void OnDestroy()
	{
		s_InstanceCount--;
		//Debug.Log("OnDestroy(): " + GetInstanceID() + " " + s_InstanceCount);

		//NOTE: nobody's using it right now
		//if (OnWillClose != null)
		//	OnWillClose();

		m_SerializationControl.OnWindowClose();

		//NOTE: this doesn't get called on editor close!
		//using (System.IO.StreamWriter streamWriter = new System.IO.StreamWriter(@"r:\testfile.txt"))
		//{
		//	streamWriter.WriteLine("called from OnDestroy()");
		//}

		m_View.OnWindowClose(this);

		//m_FirstOpen = true;

		SceneLoadDetector.PermanentlyDestroyInstance();

		m_SerializationControl.Uninitialize();
		//GraphicAssets.DestroyInstance();
		//ResLoad.DestroyInstance();
	}

	private void Init()
	{
		m_Initialized = true;

		GraphicAssets.Instance.InitGuiStyle();
	}

	void OnGUI()
	{
		m_CachedTitleContent.TitleContent.image = JumpToResources.Instance.GetImage(ResId.ImageTabIcon);

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

	//apparently OnFocus() gets called before OnEnable()!
	//void OnFocus()
	//{
	//	Debug.Log("OnFocus(): " + GetInstanceID());
	//}

	void OnBecameVisible()
	{
		//Debug.Log("OnBecameVisible()");

		if (m_JumpLinks != null)
		{
			m_JumpLinks.RefreshHierarchyLinks();
			m_JumpLinks.RefreshProjectLinks();
		}
	}

	//void ModifierKeysChanged()
	//{
	//	Debug.Log("ModifierKeysChanged()");
	//}

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
		//Debug.Log("Project Window Changed");
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

			//Debug.Log("Hierarchy Window Changed");
			m_JumpLinks.RefreshHierarchyLinks();
			Repaint();
		}
	}

	private void OnSceneLoaded(string sceneAssetPath)
	{
		Repaint();
	}

	private void OnPlayModeStateChanged()
	{
		//attempting to detect stops
		if (!EditorApplication.isPlaying &&
			!EditorApplication.isPaused)
		{
			SceneLoadDetector.EnsureExistence();
			EditorApplication.playmodeStateChanged -= OnPlayModeStateChanged;
		}
	}

	public void RefreshMinSize()
	{
		//TODO: need to notify the parent HostView's ContainerWindow of a minSize change
		//	See ContainerWindow.OnResize()
		if (m_Settings.Visibility == JumpToSettings.VisibleList.ProjectAndHierarchy)
		{
			if (m_Settings.Vertical)
			{
				this.minSize = new Vector2(139.0f, GuiJumpLinkListView.DividerMin * 2.0f);
			}
			else
			{
				this.minSize = new Vector2(GuiJumpLinkListView.DividerMin * 2.0f, GuiJumpLinkListView.DividerMin);
			}
		}
		else
		{
			this.minSize = new Vector2(139.0f, GuiJumpLinkListView.DividerMin);
		}

		//NOTE: if docked, this makes the window pop out to a new container
		//	See this.set_position(), and this.CreateNewWindowForEditorWindow()
		//Rect pos = position;

		//if (minSize.x > pos.width)
		//{
		//	pos.width = minSize.x;
		//}

		//if (minSize.y > pos.height)
		//{
		//	pos.height = minSize.y;
		//}

		//position = pos;

		Repaint();
	}


	private void CreateMultipleJumpLinks(UnityEngine.Object[] linkReferences)
	{
		#region Removed Filtering
		//NOTE: already fully tested, but decided against it due
		//		to a potentially negative user experience
		//switch (m_Window.JumpToSettingsInstance.Visibility)
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
		return EditorWindow.GetWindow<JumpToEditorWindow>("JumpTo");
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
	
	[MenuItem("Window/JumpTo")]
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
		return windows.Length > 0 && windows[0].m_JumpLinks != null && selected != null && selected.Length > 0;
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
		return windows.Length > 0 && windows[0].m_JumpLinks != null && selected != null && selected.Length > 0;
	}

	[MenuItem("GameObject/Create Other/Jump Link", false)]
	public static void JumpTo_GameObjectCreateOtherJumpLink()
	{
		Object[] selected = Selection.objects;
		JumpToEditorWindow window = GetOrCreateWindow();
		window.CreateMultipleJumpLinks(selected);
	}
}
