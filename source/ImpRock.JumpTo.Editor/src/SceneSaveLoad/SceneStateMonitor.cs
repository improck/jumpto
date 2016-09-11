using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;


namespace ImpRock.JumpTo.Editor
{
	internal sealed class SceneState
	{
		public int SceneId = 0;
		public string Name = string.Empty;
		public string Path = string.Empty;
		public int RootCount = 0;
		public bool IsDirty = false;
		public bool IsLoaded = false;
		public Scene Scene;
		
		
		public bool DiffName { get { return Name != Scene.name; } }
		public bool DiffPath { get { return Path != Scene.path; } }
		public bool DiffRootCount { get { return RootCount != Scene.rootCount; } }
		public bool DiffIsDirty { get { return IsDirty != Scene.isDirty; } }
		public bool DiffIsLoaded { get { return IsLoaded != Scene.isLoaded; } }


		public event System.Action<SceneState, string> OnNameChange;
		public event System.Action<SceneState, string> OnPathChange;
		public event System.Action<SceneState, int> OnRootCountChange;
		public event System.Action<SceneState, bool> OnIsDirtyChange;
		public event System.Action<SceneState, bool> OnIsLoadedChange;
		public event System.Action<SceneState> OnClose;
		

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
				OnNameChange(this, Scene.name);
			if (OnPathChange != null && DiffPath)
				OnPathChange(this, Scene.path);
			if (OnRootCountChange != null && DiffRootCount)
				OnRootCountChange(this, Scene.rootCount);
			if (OnIsDirtyChange != null && DiffIsDirty)
				OnIsDirtyChange(this, Scene.isDirty);
			if (OnIsLoadedChange != null && DiffIsLoaded)
				OnIsLoadedChange(this, Scene.isLoaded);
				
			SceneId = Scene.GetHashCode();
			Name = Scene.name;
			Path = Scene.path;
			RootCount = Scene.rootCount;
			IsDirty = Scene.isDirty;
			IsLoaded = Scene.isLoaded;
		}

		public void SceneClosed()
		{
			OnClose?.Invoke(this);
			Debug.Log("SceneStateMonitor: scene closed " + Name);
		}

		public override string ToString()
		{
			return SceneId + " " + Name + " " + IsLoaded;
		}
	}

	
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
		[SerializeField] private bool m_HierarchyChanged = false;

		
		private Dictionary<int, SceneState> m_SceneStates = new Dictionary<int, SceneState>();


		public static event System.Action<int, int> OnSceneCountChanged;
		public static event System.Action<int, int> OnLoadedSceneCountChanged;
		public static event System.Action<SceneState> OnSceneOpened;
		public static event System.Action<string> OnSceneWillSave;
		public static event System.Action<string> OnSceneSaved;


		private static void Initialize() { }


		//***** Merged from SceneStateControl *****

		/// <summary>
		/// Called from SceneSaveDetector
		/// </summary>
		/// <param name="sceneAssetPath">The relative path to the scene being saved</param>
		public static void SceneWillSave(string sceneAssetPath)
		{
			if (s_Instance == null)
				return;
			
			SceneLoadDetector.TemporarilyDestroyInstance(true);

			if (OnSceneWillSave != null)
				OnSceneWillSave(sceneAssetPath);

			EditorApplication.delayCall +=
				delegate ()
				{
					if (OnSceneSaved != null)
						OnSceneSaved(sceneAssetPath);
				};
		}

		/// <summary>
		/// Called from SceneLoadDetector
		/// </summary>
		public static void SceneDataIsUnloading()
		{
			if (s_Instance == null)
				return;
			
			s_Instance.m_HierarchyChanged = false;
			EditorApplication.hierarchyWindowChanged += DetectRecompile;
		}

		/// <summary>
		/// Called from SceneLoadDetector
		/// </summary>
		public static void SceneDataWillLoad()
		{
			if (s_Instance == null)
				return;
			
			EditorApplication.delayCall +=
				delegate ()
				{
					//if the hierarchy changed prior to the delayed scene load
					//	then the scene load was the result of a scene asset being
					//	opened or a new scene being created. else, it was triggered
					//	by an assembly reload which deserializes the scene but does
					//	NOT call the hierarchyWindowChanged event
					if (s_Instance.m_HierarchyChanged)
					{
						Debug.Log("SceneWillLoad: Hierarchy has changed");

						s_Instance.m_HierarchyChanged = false;
					}
					else
					{
						Debug.Log("SceneWillLoad: Hierarchy has NOT changed");
					}

					EditorApplication.hierarchyWindowChanged -= DetectRecompile;
				};
		}

		private static void DetectRecompile()
		{
			if (s_Instance == null)
				return;

			s_Instance.m_HierarchyChanged = true;
			EditorApplication.hierarchyWindowChanged -= DetectRecompile;
		}
		//*****************************************


		public SceneState GetSceneState(int sceneId)
		{
			SceneState sceneState = null;
			m_SceneStates.TryGetValue(sceneId, out sceneState);

			return sceneState;
		}

		public SceneState[] GetSceneStates()
		{
			SceneState[] sceneStates = new SceneState[m_SceneStates.Count];
			m_SceneStates.Values.CopyTo(sceneStates, 0);
			return sceneStates;
		}

		public void InitializeSceneStateData()
		{
			EditorApplication.hierarchyWindowChanged += OnHierarchyWindowChanged;

			m_SceneCount = EditorSceneManager.sceneCount;
			m_LoadedSceneCount = EditorSceneManager.loadedSceneCount;

			for (int i = 0; i < m_SceneCount; i++)
			{
				Scene scene = EditorSceneManager.GetSceneAt(i);
				m_SceneStates[scene.GetHashCode()] = new SceneState(scene);
			}
		}

		private void OnHierarchyWindowChanged()
		{
			int currentSceneCount = EditorSceneManager.sceneCount;

			//TODO: there has got to be a more efficient way to do this!

			//find newly opened scenes
			bool sceneStateChanged = false;
			int[] currentSceneIds = new int[currentSceneCount];
			for (int i = 0; i < currentSceneCount; i++)
			{
				Scene scene = EditorSceneManager.GetSceneAt(i);
				currentSceneIds[i] = scene.GetHashCode();
				if (!m_SceneStates.ContainsKey(currentSceneIds[i]))
				{
					sceneStateChanged = true;
					SceneState sceneState = new SceneState(scene);
					m_SceneStates[currentSceneIds[i]] = sceneState;
					OnSceneOpened?.Invoke(sceneState);
				}
			}

			//find newly closed scenes
			Dictionary<int, SceneState> currentSceneStates = new Dictionary<int, SceneState>();
			foreach (KeyValuePair<int, SceneState> sceneState in m_SceneStates)
			{
				if (!ArrayUtility.Contains(currentSceneIds, sceneState.Key))
				{
					sceneStateChanged = true;
					sceneState.Value.SceneClosed();
				}
				else
				{
					currentSceneStates.Add(sceneState.Key, sceneState.Value);
				}
			}

			if (sceneStateChanged)
			{
				m_SceneStates = currentSceneStates;
			}

			if (m_SceneCount != currentSceneCount)
			{
				OnSceneCountChanged?.Invoke(m_SceneCount, currentSceneCount);

				m_SceneCount = currentSceneCount;
			}

			currentSceneCount = EditorSceneManager.loadedSceneCount;
			if (m_LoadedSceneCount != currentSceneCount)
			{
				OnLoadedSceneCountChanged?.Invoke(m_LoadedSceneCount, currentSceneCount);
				
				m_LoadedSceneCount = currentSceneCount;
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
