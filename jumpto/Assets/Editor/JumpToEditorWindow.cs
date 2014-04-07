using UnityEditor;
using UnityEngine;
using System.Collections.Generic;


//TODO: make items clickable (what happens when clicked?)
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
	public class LinkedObject
	{
		public UnityEngine.Object LinkReference;
		public GUIContent LinkLabelContent = new GUIContent();
		public Color LinkColor = Color.black;
		public System.Type LinkType = null;
	}


	[SerializeField]
	private List<LinkedObject> m_ProjectReferences = new List<LinkedObject>();

	[SerializeField]
	private List<LinkedObject> m_HierarchyReferences = new List<LinkedObject>();

	private bool m_ProjectLinksUnfolded = true;
	private bool m_HierarchyLinksUnfolded = true;

	private Texture2D m_IconPrefabNormal;
	private Texture2D m_IconPrefabModel;
	private Texture2D m_IconGameObject;

	private GUIStyle  m_LinkLabelStyle;

	private Color ColorViolet = new Color(0.6f, 0.27f, 0.67f, 1.0f);


	void OnEnable()
	{
		m_IconPrefabNormal = EditorGUIUtility.FindTexture("PrefabNormal Icon");
		m_IconPrefabModel = EditorGUIUtility.FindTexture("PrefabModel Icon");
		m_IconGameObject = EditorGUIUtility.FindTexture("GameObject Icon");
	}

	void OnGUI()
	{
		//NOTE: it's ridiculous that I have to do this here.
		if (m_LinkLabelStyle == null)
		{
			m_LinkLabelStyle = new GUIStyle(GUI.skin.label);
			m_LinkLabelStyle.padding.left = 17;
		}

		m_ProjectLinksUnfolded = EditorGUILayout.Foldout(m_ProjectLinksUnfolded, "Project References");
		if (m_ProjectLinksUnfolded)
		{
			for (int i = 0; i < m_ProjectReferences.Count; i++)
			{
				GUILayout.BeginHorizontal();
				m_LinkLabelStyle.normal.textColor = m_ProjectReferences[i].LinkColor;
				GUILayout.Space(17.0f);
				GUILayout.Label(m_ProjectReferences[i].LinkLabelContent, m_LinkLabelStyle, GUILayout.Height(16.0f));
				GUILayout.EndHorizontal();
			}
		}

		EditorGUILayout.Separator();

		m_HierarchyLinksUnfolded = EditorGUILayout.Foldout(m_HierarchyLinksUnfolded, "Hierarchy References");
		if (m_HierarchyLinksUnfolded)
		{
			for (int i = 0; i < m_HierarchyReferences.Count; i++)
			{
				GUILayout.BeginHorizontal();
				m_LinkLabelStyle.normal.textColor = m_HierarchyReferences[i].LinkColor;
				GUILayout.Space(17.0f);
				GUILayout.Label(m_HierarchyReferences[i].LinkLabelContent, m_LinkLabelStyle, GUILayout.Height(16.0f));
				GUILayout.EndHorizontal();
			}
		}
	}

	void OnFocus()
	{
		CheckHierarchyLinks();
		CheckProjectLinks();
	}

	//***** Only called if focused *****
	void OnHierarchyChange()
	{
		Debug.Log("Hierarchy changed");
		CheckHierarchyLinks();
		Repaint();
	}

	void OnProjectChange()
	{
		CheckProjectLinks();
		Repaint();
	}
	//**********************************

	private void CreateJumpLink(UnityEngine.Object linkObject)
	{
		PrefabType prefabType = PrefabType.None;
		if (linkObject is GameObject)
		{
			prefabType = PrefabUtility.GetPrefabType(linkObject);
			if (prefabType == PrefabType.None ||
				prefabType == PrefabType.PrefabInstance ||
				prefabType == PrefabType.ModelPrefabInstance ||
				prefabType == PrefabType.DisconnectedPrefabInstance ||
				prefabType == PrefabType.DisconnectedModelPrefabInstance)
			{
				AddHierarchyLink(linkObject, prefabType);
			}
			else
			{
				AddProjectLink(linkObject, prefabType);
			}
		}
		else
		{
			AddProjectLink(linkObject, prefabType);
		}
	}

	private void CreateMultipleJumpLinks(UnityEngine.Object[] linkObjects)
	{
		for (int i = 0; i < linkObjects.Length; i++)
		{
			CreateJumpLink(linkObjects[i]);
		}
	}

	private void AddHierarchyLink(UnityEngine.Object linkObject, PrefabType prefabType)
	{
		//basically, if no linked object in the list has a reference to the passed object
		if (!m_HierarchyReferences.Exists(linked => linked.LinkReference == linkObject))
		{
			LinkedObject link = new LinkedObject();
			link.LinkReference = linkObject;
			link.LinkType = linkObject.GetType();
			GUIContent linkContent = EditorGUIUtility.ObjectContent(linkObject, link.LinkType);
			link.LinkLabelContent.text = linkContent.text != string.Empty ? linkContent.text : "[Unnamed]";
			link.LinkLabelContent.image = linkContent.image;
			
			if (linkObject is GameObject)
			{
				if (prefabType == PrefabType.PrefabInstance)
				{
					if (link.LinkLabelContent.image == null)
						link.LinkLabelContent.image = m_IconPrefabNormal;

					link.LinkColor = Color.blue;
				}
				else if (prefabType == PrefabType.ModelPrefabInstance)
				{
					if (link.LinkLabelContent.image == null)
						link.LinkLabelContent.image = m_IconPrefabModel;
					link.LinkColor = ColorViolet;
				}
				else if (prefabType == PrefabType.DisconnectedPrefabInstance)
				{
					if (link.LinkLabelContent.image == null)
						link.LinkLabelContent.image = m_IconPrefabNormal;
					
					link.LinkColor = Color.red;
				}
				else if (prefabType == PrefabType.DisconnectedModelPrefabInstance)
				{
					if (link.LinkLabelContent.image == null)
						link.LinkLabelContent.image = m_IconPrefabModel;

					link.LinkColor = Color.red;
				}
				else if (link.LinkLabelContent.image == null)
				{
					link.LinkLabelContent.image = m_IconGameObject;
					//color = black
				}

				Transform linkTransform = (linkObject as GameObject).transform;
				link.LinkLabelContent.tooltip = GetTransformPath(linkTransform);
			}

			m_HierarchyReferences.Add(link);
		}
	}

	private void AddProjectLink(UnityEngine.Object linkObject, PrefabType prefabType)
	{
		//basically, if no linked object in the list has a reference to the passed object
		if (!m_ProjectReferences.Exists(linked => linked.LinkReference == linkObject))
		{
			LinkedObject link = new LinkedObject();
			link.LinkReference = linkObject;
			link.LinkType = linkObject.GetType();

			GUIContent linkContent = EditorGUIUtility.ObjectContent(linkObject, link.LinkType);
			link.LinkLabelContent.image = linkContent.image;
			link.LinkLabelContent.tooltip = AssetDatabase.GetAssetPath(linkObject);

			if (linkContent.text == string.Empty)
			{
				if (linkObject.name != string.Empty)
				{
					link.LinkLabelContent.text = linkObject.name;
				}
				else
				{
					string assetName = AssetDatabase.GetAssetPath(linkObject.GetInstanceID());
					int slash = assetName.LastIndexOf('/');
					int dot = assetName.LastIndexOf('.');
					link.LinkLabelContent.text = assetName.Substring(slash + 1, dot - slash - 1);
				}
			}
			else
			{
				link.LinkLabelContent.text = linkContent.text;
			}

			if (linkObject is GameObject)
			{
				if (prefabType == PrefabType.Prefab)
				{
					if (link.LinkLabelContent.image == null)
						link.LinkLabelContent.image = m_IconPrefabNormal;

					link.LinkColor = Color.blue;
				}
				else if (prefabType == PrefabType.ModelPrefab)
				{
					if (link.LinkLabelContent.image == null)
						link.LinkLabelContent.image = m_IconPrefabModel;
					link.LinkColor = ColorViolet;
				}
				else if (link.LinkLabelContent.image == null)
				{
					link.LinkLabelContent.image = m_IconGameObject;
					//color = black
				}
			}

			m_ProjectReferences.Add(link);
		}
	}

	private void CheckHierarchyLinks()
	{
		//TODO: how?
	}

	private void CheckProjectLinks()
	{
		//TODO: how?
	}

	private string GetTransformPath(Transform transform)
	{
		string path = string.Empty;
		while (transform != null)
		{
			path = "/" + transform.name + path;
			transform = transform.parent;
		}

		return path;
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
