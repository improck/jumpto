using UnityEditor;
using UnityEngine;
using System.IO;
using System.Reflection;
using SceneStateDetection;


namespace JumpTo
{
	//TODO: all auto-saving of hierarchy links is disabled. not enough info from unity editor to make it work.
	public class SerializationControl
	{
		private string m_SaveDirectory = string.Empty;
		private string m_HierarchySaveDirectory = string.Empty;
		private string[] m_HierarchyLinkPaths = null;
		private JumpToEditorWindow m_Window = null;


		private const string ProjectLinksSaveFile = "projectlinks";
		private const string SettingsSaveFile = "settings";
		private const string SaveFileExtension = ".jumpto";


		public void Initialize(JumpToEditorWindow window)
		{
			m_Window = window;

			SceneStateControl.OnSceneWillSave += OnSceneWillSave;
			//SceneStateControl.OnSceneWillLoad += OnSceneWillLoad;
			//SceneStateControl.OnSceneSaved += OnSceneSaved;
			SceneStateControl.OnSceneLoaded += OnSceneLoaded;
		}

		public void Uninitialize()
		{
			SceneStateControl.OnSceneWillSave -= OnSceneWillSave;
			//SceneStateControl.OnSceneWillLoad -= OnSceneWillLoad;
			//SceneStateControl.OnSceneSaved -= OnSceneSaved;
			SceneStateControl.OnSceneLoaded -= OnSceneLoaded;
		}

		private void OnSceneWillSave(string sceneAssetPath)
		{
			if (CreateSaveDirectories())
			{
				SaveSettings();
				SaveProjectLinks();

				//TODO: objects that are new in the scene have not been
				//		assigned a localidinfile at this point, so they
				//		will not save.
				//GetHierarchyLinkPaths();
			}
		}

		//TODO: wipes out an existing file or just saves nothing
		//	this happens when the scene has changed, the user loads
		//	a new scene and chooses save from the popup. this only
		//	gets called after the delayCall, so the scene is already
		//	unloaded.
		//	can't just save hi links from OnSceneWillSave because
		//	there may not yet be an asset file. need to temp store
		//	the hi links data that will be saved in OnSceneWillSave
		//	then actually write it in OnSceneSaved.
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
		//		//TODO: this is the troublesome one. scene objects have already been destroyed by this point
		//		SaveHierarchyLinks(EditorApplication.currentScene);
		//	}
		//}

		private void OnSceneLoaded(string sceneAssetPath)
		{
			SetSaveDirectoryPaths();

			if (sceneAssetPath != string.Empty)
				LoadHierarchyLinks();
		}

		//private void OnWindowOpen()
		//{
		//	//Debug.Log("window open");
		//	EditorApplication.delayCall += WaitForWindowOpenComplete;
		//}

		//private void WaitForWindowOpenComplete()
		//{
		//	SetSaveDirectoryPaths();

		//	LoadSettings();
		//	LoadProjectLinks();
		//	//LoadHierarchyLinks();
		//}

		public void OnWindowEnable()
		{
			SetSaveDirectoryPaths();

			LoadSettings();

			if (m_Window.JumpLinksInstance.GetJumpLinkContainer<ProjectJumpLink>().Links.Count == 0)
			{
				LoadProjectLinks();
			}

			if (m_Window.JumpLinksInstance.GetJumpLinkContainer<HierarchyJumpLink>().Links.Count == 0)
			{
				EditorApplication.delayCall += 
					delegate()
					{
						LoadHierarchyLinks();
						m_Window.Repaint();
					};
			}
		}

		public void OnWindowDisable()
		{
			if (CreateSaveDirectories())
			{
				SaveSettings();
				SaveProjectLinks();
			}
		}

		public void OnWindowClose()
		{
			if (CreateSaveDirectories())
			{
				SaveSettings();
				SaveProjectLinks();

				//if (EditorApplication.currentScene != string.Empty)
				//{
				//	GetHierarchyLinkPaths();
				//	SaveHierarchyLinks(EditorApplication.currentScene);
				//}
			}
		}

		private void SaveProjectLinks()
		{
			string filePath = m_SaveDirectory + ProjectLinksSaveFile + SaveFileExtension;
			Object[] linkReferences = m_Window.JumpLinksInstance.GetJumpLinkContainer<ProjectJumpLink>().AllLinkReferences;
			if (linkReferences != null)
			{
				//TODO: error handling
				using (StreamWriter streamWriter = new StreamWriter(filePath))
				{
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
				}
			}
			else
			{
				DeleteSaveFile(filePath);
			}
		}

		public void SaveHierarchyLinks(string sceneAssetPath)
		{
			//TODO: move this when/if unity leaves hooks for scene save events
			GetHierarchyLinkPaths();

			string sceneGuid = AssetDatabase.AssetPathToGUID(sceneAssetPath);
			string filePath = m_HierarchySaveDirectory + sceneGuid + SaveFileExtension;
			if (m_HierarchyLinkPaths != null)
			{
				//TODO: error handling
				using (StreamWriter streamWriter = new StreamWriter(filePath))
				{
					for (int i = 0; i < m_HierarchyLinkPaths.Length; i++)
					{
						if (m_HierarchyLinkPaths[i] != null &&
							m_HierarchyLinkPaths[i].Length > 0)
							streamWriter.WriteLine(m_HierarchyLinkPaths[i]);
					}
				}
			}
			else
			{
				DeleteSaveFile(filePath);
			}
		}

		private void SaveSettings()
		{
			string filePath = m_SaveDirectory + SettingsSaveFile + SaveFileExtension;
			using (StreamWriter streamWriter = new StreamWriter(filePath))
			{
				JumpToSettings settings = m_Window.JumpToSettingsInstance;

				streamWriter.WriteLine(settings.Visibility);
				streamWriter.WriteLine(settings.ProjectFirst);
				streamWriter.WriteLine(settings.Vertical);
				streamWriter.WriteLine(settings.DividerPosition);
			}
		}
		
		private void LoadProjectLinks()
		{
			string filePath = m_SaveDirectory + ProjectLinksSaveFile + SaveFileExtension;
			if (!File.Exists(filePath))
				return;

			JumpLinkContainer<ProjectJumpLink> links = m_Window.JumpLinksInstance.GetJumpLinkContainer<ProjectJumpLink>();
			links.RemoveAll();

			//TODO: error handling
			using (StreamReader streamReader = new StreamReader(filePath))
			{
				int instanceId;
				string line;
				string path;
				while (!streamReader.EndOfStream)
				{
					JumpLinks jumpLinks = m_Window.JumpLinksInstance;

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
		}

		private void LoadHierarchyLinks()
		{
			HierarchyJumpLinkContainer links = m_Window.JumpLinksInstance.HierarchyLinks;
			links.RemoveAll();

			string currentScene = EditorApplication.currentScene;

			if (string.IsNullOrEmpty(currentScene))
				return;

			string sceneGuid = AssetDatabase.AssetPathToGUID(currentScene);
			string filePath = m_HierarchySaveDirectory + sceneGuid + SaveFileExtension;
			
			if (!File.Exists(filePath))
				return;

			//TODO: error handling
			using (StreamReader streamReader = new StreamReader(filePath))
			{
				PropertyInfo inspectorModeProperty = typeof(SerializedObject).GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);

				string line;
				string transPath;
				string objName;
				int localId = 0;
				int searchId = 0;
				int pipeLoc = 0;
				GameObject obj;
				SerializedObject serializedObject;
				while (!streamReader.EndOfStream)
				{
					JumpLinks jumpLinks = m_Window.JumpLinksInstance;

					line = streamReader.ReadLine();
					if (line == null || line.Length == 0)
						continue;

					pipeLoc = line.LastIndexOf('|');
					transPath = line.Substring(0, pipeLoc);
					localId = int.Parse(line.Substring(pipeLoc + 1));
					
					obj = GameObject.Find(transPath);
					if (obj != null)
					{
						serializedObject = new SerializedObject(obj);
						serializedObject.SetInspectorMode(InspectorMode.Debug);
						searchId = serializedObject.GetLocalIdInFile();

						if (searchId == localId)
						{
							jumpLinks.CreateOnlyHierarchyJumpLink(obj);
						}
						else if (obj.transform.parent != null)
						{
							objName = obj.name;
							foreach (Transform trans in obj.transform.parent)
							{
								if (trans.name == objName && trans.gameObject != obj)
								{
									serializedObject = new SerializedObject(trans.gameObject);
									serializedObject.SetInspectorMode(InspectorMode.Debug);
									searchId = serializedObject.GetLocalIdInFile();
									if (searchId == localId)
									{
										jumpLinks.CreateOnlyHierarchyJumpLink(trans.gameObject);
										break;
									}
								}
							}
						}
						else
						{
							objName = obj.name;

							HierarchyProperty hierarchyProperty = new HierarchyProperty(HierarchyType.GameObjects);
							int[] expanded = new int[0];
							while (hierarchyProperty.Next(expanded))
							{
								if (hierarchyProperty.name == objName && hierarchyProperty.pptrValue != obj)
								{
									serializedObject = new SerializedObject(hierarchyProperty.pptrValue);
									serializedObject.SetInspectorMode(InspectorMode.Debug);
									searchId = serializedObject.GetLocalIdInFile();
									if (searchId == localId)
									{
										jumpLinks.CreateOnlyHierarchyJumpLink(hierarchyProperty.pptrValue);
										break;
									}
								}
							}
						}
					}
				}
			}
		}

		private void LoadSettings()
		{
			string filePath = m_SaveDirectory + SettingsSaveFile + SaveFileExtension;
			if (!File.Exists(filePath))
				return;

			using (StreamReader streamReader = new StreamReader(filePath))
			{
				JumpToSettings settings = m_Window.JumpToSettingsInstance;
				settings.Visibility= (JumpToSettings.VisibleList)System.Enum.Parse(typeof(JumpToSettings.VisibleList), streamReader.ReadLine());
				settings.ProjectFirst = bool.Parse(streamReader.ReadLine());
				settings.Vertical = bool.Parse(streamReader.ReadLine());
				settings.DividerPosition = float.Parse(streamReader.ReadLine());
			}
		}

		private void SetSaveDirectoryPaths()
		{
			m_SaveDirectory = Path.GetFullPath(Application.dataPath) + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "JumpTo" + Path.DirectorySeparatorChar;
			m_HierarchySaveDirectory = m_SaveDirectory + "HierarchyLinks" + Path.DirectorySeparatorChar;
		}

		private bool CreateSaveDirectories()
		{
			//TODO: pull logerror statements from the resource loader

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
					Debug.LogError("JumpTo: The save folder path exceeds the max path length allowed by your system " + m_SaveDirectory);
				}
				catch (IOException)
				{
					created = false;
					Debug.LogError("JumpTo: File exists at path " + m_SaveDirectory + ". Please rename it and save again.");
				}
				catch (System.UnauthorizedAccessException)
				{
					created = false;
					Debug.LogError("JumpTo: The system reports that the current user is not authorized to create " + m_SaveDirectory);
				}
				catch (System.ArgumentException)
				{
					created = false;
					Debug.LogError("JumpTo: The save folder contains invalid characters " + m_SaveDirectory);
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
					Debug.LogError("JumpTo: The save folder path exceeds the max path length allowed by your system " + m_HierarchySaveDirectory);
				}
				catch (IOException)
				{
					created = false;
					Debug.LogError("JumpTo: File exists at path " + m_HierarchySaveDirectory + ". Please rename it and save again.");
				}
				catch (System.UnauthorizedAccessException)
				{
					created = false;
					Debug.LogError("JumpTo: The system reports that the current user is not authorized to create " + m_HierarchySaveDirectory);
				}
				catch (System.ArgumentException)
				{
					created = false;
					Debug.LogError("JumpTo: The save folder contains invalid characters " + m_HierarchySaveDirectory);
				}
			}

			return created;
		}

		private bool DeleteSaveFile(string filePath)
		{
			//TODO: pull logerror statements from the resource loader

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
					Debug.LogError("JumpTo: The file path exceeds the max path length allowed by your system " + filePath);
				}
				catch (IOException)
				{
					deleted = false;
					Debug.LogError("JumpTo: File " + filePath + " is in use. Unable to delete.");
				}
				catch (System.UnauthorizedAccessException)
				{
					deleted = false;
					Debug.LogError("JumpTo: The system reports that the current user is not authorized to delete " + filePath);
				}
				catch (System.ArgumentException)
				{
					deleted = false;
					Debug.LogError("JumpTo: The file path contains invalid characters " + filePath);
				}
			}

			return deleted;
		}

		private void GetHierarchyLinkPaths()
		{
			m_HierarchyLinkPaths = null;

			Object[] linkReferences = m_Window.JumpLinksInstance.HierarchyLinks.AllLinkReferences;
			if (linkReferences != null)
			{
				SerializedObject serializedObject;
				int localId = 0;
				m_HierarchyLinkPaths = new string[linkReferences.Length];
				for (int i = 0; i < linkReferences.Length; i++)
				{
					//NOTE: can't use instanceId because they change each time the scene is loaded!
					//		instead we have to store the transform path. it's not very reliable,
					//		though, so we also get the "local id in file."
					//instanceId = linkReferences[i].GetInstanceID();

					serializedObject = new SerializedObject(linkReferences[i]);
					serializedObject.SetInspectorMode(InspectorMode.Debug);

					//skip objects that haven't been saved to the scene
					//TODO: should this even be necessary in the future?
					localId = serializedObject.GetLocalIdInFile();
					if (localId == 0)
						continue;
					
					m_HierarchyLinkPaths[i] =
						JumpToUtility.GetTransformPath((linkReferences[i] as GameObject).transform)
							+ "|" + serializedObject.GetLocalIdInFile().ToString();
				}
			}
		}
	}
}

