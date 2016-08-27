﻿using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;


namespace ImpRock.JumpTo.Editor
{
	internal sealed class SceneState
	{
		public int SceneId;
		public string Name;
		public string Path;
		public int RootCount;
		public bool IsDirty;
		public bool IsLoaded;
		public Scene Scene;
		
		
		public bool DiffName { get { return Name != Scene.name; } }
		public bool DiffPath { get { return Path != Scene.path; } }
		public bool DiffRootCount { get { return RootCount != Scene.rootCount; } }
		public bool DiffIsDirty { get { return IsDirty != Scene.isDirty; } }
		public bool DiffIsLoaded { get { return IsLoaded != Scene.isLoaded; } }


		public event System.Action<int, string> OnNameChange;
		public event System.Action<int, string> OnPathChange;
		public event System.Action<int, int> OnRootCountChange;
		public event System.Action<int, bool> OnIsDirtyChange;
		public event System.Action<int, bool> OnIsLoadedChange;
		public event System.Action<int> OnClose;
		

		public SceneState(Scene scene)
		{
			Scene = scene;
			SceneId = Scene.GetHashCode();
			Name = scene.name;
			Path = scene.path;
			RootCount = scene.rootCount;
			IsDirty = scene.isDirty;
			IsLoaded = scene.isLoaded;
		}

		public void UpdateInfo()
		{
			if (OnNameChange != null && DiffName)
				OnNameChange(SceneId, Scene.name);
			if (OnPathChange != null && DiffPath)
				OnPathChange(SceneId, Scene.path);
			if (OnRootCountChange != null && DiffRootCount)
				OnRootCountChange(SceneId, Scene.rootCount);
			if (OnIsDirtyChange != null && DiffIsDirty)
				OnIsDirtyChange(SceneId, Scene.isDirty);
			if (OnIsLoadedChange != null && DiffIsLoaded)
				OnIsLoadedChange(SceneId, Scene.isLoaded);
				
			SceneId = Scene.GetHashCode();
			Name = Scene.name;
			Path = Scene.path;
			RootCount = Scene.rootCount;
			IsDirty = Scene.isDirty;
			IsLoaded = Scene.isLoaded;
		}

		public void SceneClosed()
		{
			OnClose?.Invoke(SceneId);
			Debug.Log("SceneStateMonitor: scene closed " + Name);
		}
	}


	[InitializeOnLoad]
	[System.Serializable]
	internal sealed class SceneStateMonitor
	{
		#region Pseudo-Singleton
		private static SceneStateMonitor s_Instance = null;

		public static SceneStateMonitor Instance { get { return s_Instance; } }

		public static SceneStateMonitor Create()
		{
			if (s_Instance == null)
				s_Instance = new SceneStateMonitor();

			return s_Instance;
		}


		private SceneStateMonitor() { s_Instance = this; Initialize(); }
		#endregion


		[SerializeField] private int m_SceneCount = 0;
		[SerializeField] private int m_LoadedSceneCount = 0;

		
		private Dictionary<int, SceneState> m_SceneStates = new Dictionary<int, SceneState>();


		public static event System.Action<int, int> OnSceneCountChanged;
		public static event System.Action<int, int> OnLoadedSceneCountChanged;
		public static event System.Action<SceneState> OnSceneOpen;


		private static void Initialize()
		{
			s_Instance.InternalInitialize();
		}


		public SceneState GetSceneState(int sceneId)
		{
			SceneState sceneState = null;
			m_SceneStates.TryGetValue(sceneId, out sceneState);

			return sceneState;
		}
		
		private void InternalInitialize()
		{
			EditorApplication.delayCall +=
				delegate ()
				{
					m_SceneCount = EditorSceneManager.sceneCount;
					m_LoadedSceneCount = EditorSceneManager.loadedSceneCount;
					
					for (int i = 0; i < m_SceneCount; i++)
					{
						Scene scene = EditorSceneManager.GetSceneAt(i);
						m_SceneStates[scene.GetHashCode()] = new SceneState(scene);
					}

					EditorApplication.hierarchyWindowChanged += OnHierarchyWindowChanged;
				};
		}

		private void OnHierarchyWindowChanged()
		{
			int sceneCount = EditorSceneManager.sceneCount;
			if (m_SceneCount != sceneCount)
			{
				OnSceneCountChanged?.Invoke(m_SceneCount, sceneCount);

				m_SceneCount = sceneCount;
				
				//find newly opened scenes
				int[] allSceneIds = new int[m_SceneCount];
				for (int i = 0; i < m_SceneCount; i++)
				{
					Scene scene = EditorSceneManager.GetSceneAt(i);
					allSceneIds[i] = scene.GetHashCode();
					if (!m_SceneStates.ContainsKey(allSceneIds[i]))
					{
						SceneState sceneState = new SceneState(scene);
						m_SceneStates[allSceneIds[i]] = sceneState;
						OnSceneOpen?.Invoke(sceneState);
						Debug.Log("SceneStateMonitor: scene open " + sceneState.Name);
					}
				}

				//find newly closed scenes & make a new dictionary
				Dictionary<int, SceneState> sceneStates = new Dictionary<int, SceneState>();
				foreach (KeyValuePair<int, SceneState> sceneStatePair in m_SceneStates)
				{
					if (System.Array.IndexOf(allSceneIds, sceneStatePair.Key) > -1)
					{
						sceneStates.Add(sceneStatePair.Key, sceneStatePair.Value);
					}
					else
					{
						sceneStatePair.Value.SceneClosed();
					}
				}

				m_SceneStates = sceneStates;
			}

			sceneCount = EditorSceneManager.loadedSceneCount;
			if (m_LoadedSceneCount != sceneCount)
			{
				OnLoadedSceneCountChanged?.Invoke(m_LoadedSceneCount, sceneCount);

				m_LoadedSceneCount = sceneCount;
			}

			UpdateSceneData();
		}
		
		private void UpdateSceneData()
		{
			foreach (KeyValuePair<int, SceneState> sceneStatePair in m_SceneStates)
			{
				sceneStatePair.Value.UpdateInfo();
			}
		}
	}
}