using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;


namespace ImpRock.JumpTo.Editor
{
	//TODO: all auto-saving of hierarchy links is disabled. not enough info from unity editor to make it work.
	internal sealed class SerializationControl
	{
		private static System.Version s_Version = null;

		public System.Version Version { get { return s_Version; } }

		private string m_SaveDirectory = string.Empty;
		private string m_HierarchySaveDirectory = string.Empty;
		private JumpToEditorWindow m_Window = null;


		private const string ProjectLinksSaveFile = "projectlinks";
		private const string SettingsSaveFile = "settings";
		private const string SaveFileExtension = ".jumpto";


		static SerializationControl()
		{
			s_Version = typeof(SerializationControl).Assembly.GetName().Version;
		}


		public void Initialize(JumpToEditorWindow window)
		{
			m_Window = window;

			SceneSaveDetector.OnSceneWillSave += OnSceneWillSave;
			SceneSaveDetector.OnSceneDeleted += OnSceneDeleted;
			//SceneStateMonitor.OnSceneWillLoad += OnSceneWillLoad;
			//SceneStateMonitor.OnSceneSaved += OnSceneSaved;
			//SceneStateMonitor.OnSceneLoaded += OnSceneLoaded;
			SceneStateMonitor.OnSceneOpened += OnSceneOpened;

			foreach (SceneState sceneState in SceneStateMonitor.Instance.GetSceneStates())
			{
				sceneState.OnIsLoadedChange += OnSceneLoadedChange;
				sceneState.OnClose += OnSceneClosed;
			}
		}

		public void Uninitialize()
		{
			SceneSaveDetector.OnSceneWillSave -= OnSceneWillSave;
			SceneSaveDetector.OnSceneDeleted -= OnSceneDeleted;
			//SceneStateMonitor.OnSceneWillLoad -= OnSceneWillLoad;
			//SceneStateMonitor.OnSceneSaved -= OnSceneSaved;
			//SceneStateMonitor.OnSceneLoaded -= OnSceneLoaded;
			SceneStateMonitor.OnSceneOpened -= OnSceneOpened;

			foreach (SceneState sceneState in SceneStateMonitor.Instance.GetSceneStates())
			{
				sceneState.OnIsLoadedChange -= OnSceneLoadedChange;
				sceneState.OnClose -= OnSceneClosed;
			}
		}

		/// <summary>
		/// Called from SceneSaveDetector
		/// </summary>
		/// <param name="sceneAssetPath"></param>
		private void OnSceneWillSave(string sceneAssetPath)
		{
			if (CreateSaveDirectories())
			{
				SaveProjectLinks();

				//NOTE: can't save hierarchy links
				//objects that are new in the scene have not been
				//	assigned a localidinfile at this point, so they
				//	will not save.
			}
		}

		/// <summary>
		/// Called from SceneSaveDetector
		/// </summary>
		/// <param name="sceneAssetPath"></param>
		private void OnSceneDeleted(string sceneAssetPath)
		{
			//TODO: delete the associated hierarchy links file IF it is not being viewed
			//string sceneGuid = AssetDatabase.AssetPathToGUID(sceneAssetPath);
			//string filePath = m_HierarchySaveDirectory + sceneGuid + SaveFileExtension;
		}

		//NOTE: wipes out an existing file or just saves nothing
		//	this happens when the scene has changed, the user loads
		//	a new scene and chooses save from the popup. this only
		//	gets called after the delayCall, so the scene is already
		//	unloaded. can't just save hi links from OnSceneWillSave
		//	because there may not yet be an asset file.
		//private void OnSceneSaved(string sceneAssetPath)
		//{
		//	if (CreateSaveDirectories())
		//	{
		//		SaveHierarchyLinks(sceneAssetPath);
		//	}
		//}

		//private void OnSceneWillLoad()
		//{
		//	if (CreateSaveDirectories())
		//	{
		//		//NOTE: this is the troublesome one. scene objects have already been destroyed by this point
		//		SaveHierarchyLinks(EditorApplication.currentScene);
		//	}
		//}

		private void OnSceneOpened(SceneState sceneState)
		{
			sceneState.OnIsLoadedChange += OnSceneLoadedChange;
			sceneState.OnClose += OnSceneClosed;

			if (sceneState.IsLoaded)
			{
				SetSaveDirectoryPaths();
				LoadHierarchyLinks(sceneState.SceneId, sceneState.Scene);
			}
		}
		
		private void OnSceneLoadedChange(SceneState sceneState, bool oldIsLoaded)
		{
			SetSaveDirectoryPaths();

			if (sceneState.IsLoaded)
			{
				LoadHierarchyLinks(sceneState.SceneId, sceneState.Scene);
			}
		}

		private void OnSceneClosed(SceneState sceneState)
		{
			sceneState.OnClose -= OnSceneClosed;
			sceneState.OnIsLoadedChange -= OnSceneLoadedChange;
		}

		//private void OnWindowOpen()
		//{
		//	//Debug.Log("window open");
		//	EditorApplication.delayCall += WaitForWindowOpenComplete;
		//}

		//private void WaitForWindowOpenComplete()
		//{
		//	SetSaveDirectoryPaths();
		
		//	LoadProjectLinks();
		//	//LoadHierarchyLinks();
		//}

		public void OnWindowEnable()
		{
			SetSaveDirectoryPaths();
			
			if (m_Window.JumpLinksInstance.ProjectLinks.Links.Count == 0)
			{
				LoadProjectLinks();
			}

			foreach (SceneState sceneState in SceneStateMonitor.Instance.GetSceneStates())
			{
				HierarchyJumpLinkContainer links =
					m_Window.JumpLinksInstance.GetHierarchyJumpLinkContainer(sceneState.SceneId);

				if ((links == null || links.Links.Count == 0) && sceneState.IsLoaded)
					LoadHierarchyLinks(sceneState.SceneId, sceneState.Scene);
			}
		}

		public void OnWindowDisable()
		{
			if (CreateSaveDirectories())
			{
				SaveProjectLinks();
			}
		}

		public void OnWindowClose()
		{
			if (CreateSaveDirectories())
			{
				SaveProjectLinks();

				//if (EditorApplication.currentScene != string.Empty)
				//{
				//	GetHierarchyLinkPaths();
				//	SaveHierarchyLinks(EditorApplication.currentScene);
				//}
			}
		}

		public bool SaveProjectLinks()
		{
			bool success = true;

			string filePath = m_SaveDirectory + ProjectLinksSaveFile + SaveFileExtension;
			Object[] linkReferences = m_Window.JumpLinksInstance.ProjectLinks.AllLinkReferences;
			if (linkReferences != null)
			{
				using (StreamWriter streamWriter = new StreamWriter(filePath))
				{
					try
					{
						streamWriter.WriteLine(Version.ToString());

						int instanceId;
						string line;
						for (int i = 0; i < linkReferences.Length; i++)
						{
							instanceId = linkReferences[i].GetInstanceID();
							line = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(instanceId));
							if (AssetDatabase.IsSubAsset(instanceId))
								line += "|" + instanceId;

							streamWriter.WriteLine(line);
						}

						streamWriter.Close();
					}
					catch (System.Exception ex)
					{
						streamWriter.Close();
						success = false;
						Debug.LogError(JumpToResources.Instance.GetText(ResId.LogStatements[0]) + "\n" + ex.Message);
					}
				}
			}
			else
			{
				DeleteSaveFile(filePath);
			}

			return success;
		}

		public bool SaveHierarchyLinks(int sceneId)
		{
			bool success = true;

			string[] linkPaths = GetHierarchyLinkPaths(sceneId);

			string sceneAssetPath = SceneStateMonitor.Instance.GetSceneState(sceneId).Path;
			string sceneGuid = AssetDatabase.AssetPathToGUID(sceneAssetPath);
			string filePath = m_HierarchySaveDirectory + sceneGuid + SaveFileExtension;
			if (linkPaths != null)
			{
				using (StreamWriter streamWriter = new StreamWriter(filePath))
				{
					try
					{
						streamWriter.WriteLine(Version.ToString());

						for (int i = 0; i < linkPaths.Length; i++)
						{
							if (linkPaths[i] != null &&
								linkPaths[i].Length > 0)
								streamWriter.WriteLine(linkPaths[i]);
						}

						streamWriter.Close();
					}
					catch (System.Exception ex)
					{
						streamWriter.Close();
						success = false;
						Debug.LogError(JumpToResources.Instance.GetText(ResId.LogStatements[1]) + "\n" + ex.Message);
					}
				}
			}
			else
			{
				DeleteSaveFile(filePath);
			}

			return success;
		}
		
		private void LoadProjectLinks()
		{
			string filePath = m_SaveDirectory + ProjectLinksSaveFile + SaveFileExtension;
			if (!File.Exists(filePath))
				return;

			ProjectJumpLinkContainer links = m_Window.JumpLinksInstance.ProjectLinks;
			links.RemoveAll();

			using (StreamReader streamReader = new StreamReader(filePath))
			{
				try
				{
					if (streamReader.EndOfStream)
						return;
					
					string fileVersion = streamReader.ReadLine();

					System.Action<StreamReader> loader = FindProjectLinkLoader(fileVersion);

					if (loader != null)
					{
						loader(streamReader);
					}
					else
					{
						Debug.LogError(JumpToResources.Instance.GetText(ResId.LogStatements[2]));
					}
				}
				catch (System.Exception ex)
				{
					Debug.LogError(JumpToResources.Instance.GetText(ResId.LogStatements[3]) + "\n" + ex.Message);
				}
			}
		}

		private void LoadHierarchyLinks(int sceneId, Scene scene)
		{
			string sceneGuid = AssetDatabase.AssetPathToGUID(scene.path);
			string filePath = m_HierarchySaveDirectory + sceneGuid + SaveFileExtension;

			if (!File.Exists(filePath))
				return;

			HierarchyJumpLinkContainer links = m_Window.JumpLinksInstance.AddHierarchyJumpLinkContainer(sceneId);
			links.RemoveAll();

			using (StreamReader streamReader = new StreamReader(filePath))
			{
				try
				{
					if (streamReader.EndOfStream)
						return;

					string fileVersion = streamReader.ReadLine();

					System.Action<StreamReader, Scene> loader = FindHierarchyLinkLoader(fileVersion);

					if (loader != null)
					{
						loader(streamReader, scene);
					}
					else
					{
						Debug.LogError(JumpToResources.Instance.GetText(ResId.LogStatements[4]));
					}
				}
				catch (System.Exception ex)
				{
					string logFormat = JumpToResources.Instance.GetText(ResId.LogStatements[5]);
					Debug.LogError(string.Format(logFormat, scene.name) + "\n" + ex.Message);
				}
			}
		}
		
		private void SetSaveDirectoryPaths()
		{
			m_SaveDirectory = Path.GetFullPath(Application.dataPath) + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "JumpTo" + Path.DirectorySeparatorChar;
			m_HierarchySaveDirectory = m_SaveDirectory + "HierarchyLinks" + Path.DirectorySeparatorChar;
		}

		private bool CreateSaveDirectories()
		{
			bool created = true;

			SetSaveDirectoryPaths();

			if (!Directory.Exists(m_SaveDirectory))
			{
				try
				{
					Directory.CreateDirectory(m_SaveDirectory);
				}
				catch (PathTooLongException)
				{
					created = false;
					string logFormat = JumpToResources.Instance.GetText(ResId.LogStatements[6]);
					Debug.LogError(string.Format(logFormat, m_SaveDirectory));
				}
				catch (IOException)
				{
					created = false;
					string logFormat = JumpToResources.Instance.GetText(ResId.LogStatements[7]);
					Debug.LogError(string.Format(logFormat, m_SaveDirectory));
				}
				catch (System.UnauthorizedAccessException)
				{
					created = false;
					string logFormat = JumpToResources.Instance.GetText(ResId.LogStatements[8]);
					Debug.LogError(string.Format(logFormat, m_SaveDirectory));
				}
				catch (System.ArgumentException)
				{
					created = false;
					string logFormat = JumpToResources.Instance.GetText(ResId.LogStatements[9]);
					Debug.LogError(string.Format(logFormat, m_SaveDirectory));
				}
			}

			if (created && !Directory.Exists(m_HierarchySaveDirectory))
			{
				try
				{
					Directory.CreateDirectory(m_HierarchySaveDirectory);
				}
				catch (PathTooLongException)
				{
					created = false;
					string logFormat = JumpToResources.Instance.GetText(ResId.LogStatements[10]);
					Debug.LogError(string.Format(logFormat, m_HierarchySaveDirectory));
				}
				catch (IOException)
				{
					created = false;
					string logFormat = JumpToResources.Instance.GetText(ResId.LogStatements[11]);
					Debug.LogError(string.Format(logFormat, m_HierarchySaveDirectory));
				}
				catch (System.UnauthorizedAccessException)
				{
					created = false;
					string logFormat = JumpToResources.Instance.GetText(ResId.LogStatements[12]);
					Debug.LogError(string.Format(logFormat, m_HierarchySaveDirectory));
				}
				catch (System.ArgumentException)
				{
					created = false;
					string logFormat = JumpToResources.Instance.GetText(ResId.LogStatements[13]);
					Debug.LogError(string.Format(logFormat, m_HierarchySaveDirectory));
				}
			}

			return created;
		}

		private bool DeleteSaveFile(string filePath)
		{
			bool deleted = true;

			if (File.Exists(filePath))
			{
				try
				{
					File.Delete(filePath);
				}
				catch (PathTooLongException)
				{
					deleted = false;
					string logFormat = JumpToResources.Instance.GetText(ResId.LogStatements[14]);
					Debug.LogError(string.Format(logFormat, filePath));
				}
				catch (IOException)
				{
					deleted = false;
					string logFormat = JumpToResources.Instance.GetText(ResId.LogStatements[15]);
					Debug.LogError(string.Format(logFormat, filePath));
				}
				catch (System.UnauthorizedAccessException)
				{
					deleted = false;
					string logFormat = JumpToResources.Instance.GetText(ResId.LogStatements[16]);
					Debug.LogError(string.Format(logFormat, filePath));
				}
				catch (System.ArgumentException)
				{
					deleted = false;
					string logFormat = JumpToResources.Instance.GetText(ResId.LogStatements[17]);
					Debug.LogError(string.Format(logFormat, filePath));
				}
			}

			return deleted;
		}

		private string[] GetHierarchyLinkPaths(int sceneId)
		{
			Object[] linkReferences = m_Window.JumpLinksInstance.HierarchyLinks[sceneId].AllLinkReferences;
			if (linkReferences != null)
			{
				SerializedObject serializedObject;
				int localId = 0;
				Object linkReferenceObject = null;
				Transform linkReferenceTransform = null;
				string[] linkPaths = new string[linkReferences.Length];
				for (int i = 0; i < linkReferences.Length; i++)
				{
					linkReferenceObject = linkReferences[i];
					linkReferenceTransform = (linkReferenceObject as GameObject).transform;

					string paths = "|";

					PrefabType prefabType = PrefabUtility.GetPrefabType(linkReferenceObject);

					if (prefabType == PrefabType.ModelPrefabInstance ||
						prefabType == PrefabType.PrefabInstance)
					{
						linkReferenceObject = PrefabUtility.GetPrefabObject(linkReferenceObject);

						//we only want the path up to the prefab instance's root
						GameObject prefabRoot = PrefabUtility.FindPrefabRoot(linkReferenceTransform.gameObject);
						paths += JumpToUtility.GetRootOrderPath(linkReferenceTransform, prefabRoot.transform) + "|" +
							JumpToUtility.GetTransformPath(linkReferenceTransform, prefabRoot.transform);
					}
					else
					{
						paths += JumpToUtility.GetRootOrderPath(linkReferenceTransform) + "|" +
							JumpToUtility.GetTransformPath(linkReferenceTransform);
					}

					serializedObject = new SerializedObject(linkReferenceObject);
					serializedObject.SetInspectorMode(InspectorMode.Debug);

					localId = serializedObject.GetLocalIdInFile();
					linkPaths[i] = (int)prefabType + "|" + localId.ToString() + paths;
				}

				return linkPaths;
			}

			return null;
		}

		private System.Action<StreamReader> FindProjectLinkLoader(string fileVersion)
		{
			System.Version version = new System.Version(fileVersion);

			if (version.Major == 2)
			{
				return ProjectLinkLoader_2_x_x_x;
			}

			return null;
		}

		private System.Action<StreamReader, Scene> FindHierarchyLinkLoader(string fileVersion)
		{
			System.Version version = new System.Version(fileVersion);

			if (version.Major == 2)
			{
				return HierarchyLinkLoader_2_x_x_x;
			}

			return null;
		}

		private void ProjectLinkLoader_2_x_x_x(StreamReader streamReader)
		{
			//Line format
			//	assetguid
			//	assetguid|instanceId
			//
			//	assetguid = GUID of asset in the DB
			//	instanceId = instance ID of child within the asset

			JumpLinks jumpLinks = m_Window.JumpLinksInstance;

			int instanceId;
			string line;
			string path;
			while (!streamReader.EndOfStream)
			{
				line = streamReader.ReadLine();
				if (line.Length == 32)
				{
					path = AssetDatabase.GUIDToAssetPath(line);
					if (!string.IsNullOrEmpty(path))
					{
						Object obj = AssetDatabase.LoadMainAssetAtPath(path);
						if (obj != null)
							jumpLinks.CreateOnlyProjectJumpLink(obj);
					}
				}
				else if (line.Length > 33 && line[32] == '|')
				{
					instanceId = int.Parse(line.Substring(33));
					path = AssetDatabase.GUIDToAssetPath(line.Substring(0, 32));
					if (!string.IsNullOrEmpty(path))
					{
						Object[] objs = AssetDatabase.LoadAllAssetsAtPath(path);
						if (objs != null)
						{
							for (int j = 0; j < objs.Length; j++)
							{
								if (objs[j].GetInstanceID() == instanceId)
									jumpLinks.CreateOnlyProjectJumpLink(objs[j]);
							}
						}
					}
				}
			}
		}

		private void HierarchyLinkLoader_2_x_x_x(StreamReader streamReader, Scene scene)
		{
			//Line format
			//	prefabtype|localId|rootorderpath|transformpath
			//
			//	prefabtype = PrefabType int
			//	localId = LocalIdentfierInFile OR prefab object ID
			//	rootorderpath = child index path to the object from root OR from prefab root
			//	transformpath = name path to the object from root OR from prefab root

			JumpLinks jumpLinks = m_Window.JumpLinksInstance;

			//"unordered" because it's not guaranteed that these objects
			//	are in the same order as they are in the scene
			GameObject[] unorderedRootObjects = scene.GetRootGameObjects();

			if (unorderedRootObjects.Length > 0)
			{
				GameObject[] rootObjects = new GameObject[unorderedRootObjects.Length];

				//put the root objects in order
				SerializedObject so = null;
				for (int i = 0; i < rootObjects.Length; i++)
				{
					so = new SerializedObject(unorderedRootObjects[i].transform);
					rootObjects[so.FindProperty("m_RootOrder").intValue] = unorderedRootObjects[i];
				}

				Dictionary<int, GameObject> localIdToGameObjects = new Dictionary<int, GameObject>();
				Dictionary<int, GameObject> localIdToPrefabs = new Dictionary<int, GameObject>();
				JumpToUtility.GetAllLocalIds(rootObjects, localIdToGameObjects, localIdToPrefabs);

				string line;
				string transformPath;
				int prefabTypeId = 0;
				int localId = 0;
				char[] delimiterPipe = new char[] { '|' };
				char[] delimeterForwardSlash = new char[] { '/' };
				string[] lineSegments;
				string[] transformNames;
				while (!streamReader.EndOfStream)
				{
					line = streamReader.ReadLine();
					if (line == null || line.Length == 0)
						continue;

					lineSegments = line.Split(delimiterPipe, System.StringSplitOptions.None);

					if (lineSegments.Length == 0)
						continue;

					if (!int.TryParse(lineSegments[0], out prefabTypeId))
						continue;

					if (!int.TryParse(lineSegments[1], out localId))
						continue;

					//the localId should NEVER be zero
					if (localId == 0)
						continue;

					//TODO: this "could" generate an exception
					PrefabType prefabType = (PrefabType)prefabTypeId;

					//try to find the object based solely on its localId
					if (prefabType != PrefabType.ModelPrefabInstance &&
						prefabType != PrefabType.PrefabInstance)
					{
						GameObject gameObject = null;
						if (localIdToGameObjects.TryGetValue(localId, out gameObject))
							jumpLinks.CreateOnlyHierarchyJumpLink(gameObject);

						//TODO: what if it's not found?
						continue;
					}
					else
					{
						//NOTE: searching for children within prefabs is not reliable because they are not currently 
						//		uniquely addressed within a scene. if a prefab is renamed and moved after its link is
						//		saved, it may not be correctly relinked on load. if a prefab is moved within a scene,
						//		it may not be correctly relinked on load. blame Unity for this.

						//get the root node for the prefab instance
						GameObject gameObject = null;
						if (localIdToPrefabs.TryGetValue(localId, out gameObject))
						{
							//get names of the path nodes
							transformNames = lineSegments[3].Split(delimeterForwardSlash, System.StringSplitOptions.RemoveEmptyEntries);

							//check for corrupt path
							if (transformNames.Length == 0)
								continue;

							if (transformNames.Length == 1)
							{
								jumpLinks.CreateOnlyHierarchyJumpLink(gameObject);
							}
							else
							{
								transformPath = transformNames[1];
								for (int i = 2; i < transformNames.Length; i++)
								{
									transformPath += "/" + transformNames[i];
								}

								Transform transform = gameObject.transform.Find(transformPath);
								if (transform != null)
								{
									jumpLinks.CreateOnlyHierarchyJumpLink(transform.gameObject);
								}
							}
						}
					}
				}   //while ! end of stream

				localIdToGameObjects.Clear();
				localIdToPrefabs.Clear();
			}   //if root objects exist
		}   //HierarchyLinkLoader_2000
	}
}

