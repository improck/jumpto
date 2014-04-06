using UnityEditor;
using UnityEngine;
using System.Collections.Generic;


//TODO: make items clickable (what happens when clicked?)
//TODO: draw hierarchy list correclty
//TODO: project and hierarchy foldouts
//TODO: ability to remove items
//TODO: context menu
//TODO: drag-n-drop to/from window
//TODO: serialize links
//TODO: update on scene change?
//TODO: update on project change?


public class JumpToEditorWindow : EditorWindow
{
	[SerializeField]
	private List<UnityEngine.Object> m_ProjectReferences = new List<Object>();

	[SerializeField]
	private List<UnityEngine.Object> m_HierarchyReferences = new List<Object>();
	//private SerializedObject m_SerializedObject;

	private Texture2D m_IconPrefabNormal;
	private Texture2D m_IconPrefabModel;


	void OnEnable()
	{
		m_IconPrefabNormal = EditorGUIUtility.FindTexture("PrefabNormal Icon");
		m_IconPrefabModel = EditorGUIUtility.FindTexture("PrefabModel Icon");
	}

	void OnGUI()
	{
		GUIContent objContent;
		GUILayout.Label("Project References");
		for (int i = 0; i < m_ProjectReferences.Count; i++)
		{
			//EditorGUILayout.ObjectField(m_ProjectReferences[i], m_ProjectReferences[i].GetType(), true);
			objContent = EditorGUIUtility.ObjectContent(m_ProjectReferences[i], m_ProjectReferences[i].GetType());
			if (m_ProjectReferences[i] is GameObject)
			{
				if (objContent.image != null)
				{
					Debug.Log(AssetDatabase.GetAssetPath(objContent.image.GetInstanceID()));
				}
				else
				{
					PrefabType prefabType = PrefabUtility.GetPrefabType(m_ProjectReferences[i]);
					if (prefabType == PrefabType.Prefab)
						objContent.image = m_IconPrefabNormal;
					else if (prefabType == PrefabType.ModelPrefab)
						objContent.image = m_IconPrefabModel;
				}
			}

			if (objContent.text == string.Empty)
			{
				if (m_ProjectReferences[i].name != string.Empty)
				{
					objContent.text = m_ProjectReferences[i].name;
				}
				else
				{
					string assetName = AssetDatabase.GetAssetPath(m_ProjectReferences[i].GetInstanceID());
					int slash = assetName.LastIndexOf('/');
					int dot = assetName.LastIndexOf('.');
					objContent.text = assetName.Substring(slash + 1, dot - slash - 1);
				}
			}

			GUILayout.BeginHorizontal();
			GUILayout.Label(objContent, GUILayout.Height(16.0f));
			GUILayout.EndHorizontal();
		}

		EditorGUILayout.Separator();

		GUILayout.Label("Hierarchy References");
		for (int i = 0; i < m_HierarchyReferences.Count; i++)
		{
			EditorGUILayout.ObjectField(m_HierarchyReferences[i], m_HierarchyReferences[i].GetType(), true);
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
				AddHierarchyLink(linkObject);
			}
			else
			{
				AddProjectLink(linkObject);
			}
		}
		else
		{
			AddProjectLink(linkObject);
		}
	}

	private void CreateMultipleJumpLinks(UnityEngine.Object[] linkObjects)
	{
		for (int i = 0; i < linkObjects.Length; i++)
		{
			CreateJumpLink(linkObjects[i]);
		}
	}

	private void AddHierarchyLink(UnityEngine.Object linkObject)
	{
		if (!m_HierarchyReferences.Contains(linkObject))
			m_HierarchyReferences.Add(linkObject);
	}

	private void AddProjectLink(UnityEngine.Object linkObject)
	{
		if (!m_ProjectReferences.Contains(linkObject))
			m_ProjectReferences.Add(linkObject);
	}

	private void CheckHierarchyLinks()
	{
		//TODO: how?
	}

	private void CheckProjectLinks()
	{
		//TODO: how?
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
