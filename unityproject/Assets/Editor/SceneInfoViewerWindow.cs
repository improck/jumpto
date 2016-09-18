using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Reflection;


public class SceneInfoViewerWindow : EditorWindow
{
	[SerializeField] private Vector2 m_ScrollPos = Vector2.zero;

	private Scene[] m_Scenes;
	

	private void OnEnable()
	{
		EditorApplication.hierarchyWindowChanged += OnHierarchyWindowChanged;
		RefreshScenes();
	}

	private void OnDisable()
	{
		EditorApplication.hierarchyWindowChanged -= OnHierarchyWindowChanged;
		m_Scenes = null;
	}

	private void RefreshScenes()
	{
		m_Scenes = new Scene[EditorSceneManager.sceneCount];
		for (int i = 0; i < m_Scenes.Length; i++)
		{
			m_Scenes[i] = EditorSceneManager.GetSceneAt(i);
		}
	}

	private void OnHierarchyWindowChanged()
	{
		RefreshScenes();
		Repaint();
	}

	private void OnGUI()
	{
		GUILayout.BeginHorizontal();
		{
			GUILayout.BeginVertical();
			{
				EditorGUILayout.LabelField("Scene Count:", EditorSceneManager.sceneCount.ToString());
				EditorGUILayout.LabelField("Loaded Scene Count:", EditorSceneManager.loadedSceneCount.ToString());
			}
			GUILayout.EndVertical();

			if (GUILayout.Button("Refresh", GUILayout.Width(80.0f)))
			{
				OnEnable();
			}
		}
		GUILayout.EndHorizontal();

		GUILayout.Space(8.0f);

		m_ScrollPos = GUILayout.BeginScrollView(m_ScrollPos);
		{
			foreach (Scene scene in m_Scenes)
			{
				GUILayout.Box("", GUILayout.Height(4.0f), GUILayout.ExpandWidth(true));

				EditorGUILayout.LabelField("Name:", scene.name);
				EditorGUILayout.LabelField("Handle:", scene.GetHashCode().ToString());
				EditorGUILayout.LabelField("Path:", scene.path);
				EditorGUILayout.LabelField("Root Count:", scene.rootCount.ToString());
				EditorGUILayout.LabelField("Dirty:", scene.isDirty.ToString());
				EditorGUILayout.LabelField("Loaded:", scene.isLoaded.ToString());
				EditorGUILayout.LabelField("Build Index:", scene.buildIndex.ToString());

				GUILayout.Space(8.0f);
			}
		}
		GUILayout.EndScrollView();
	}

	[MenuItem("Window/Scene Info")]
	public static void SceneInfoViewer_Menu_Window()
	{
		SceneInfoViewerWindow window = EditorWindow.GetWindow<SceneInfoViewerWindow>("Scene Info");
		window.Show();
	}
}
