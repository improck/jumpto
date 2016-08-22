﻿using UnityEditor;
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
		private string m_SaveDirectory = string.Empty;
		private string m_HierarchySaveDirectory = string.Empty;
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

			//TODO: make it work with multi-scene
			//if (sceneAssetPath != string.Empty)
			//	LoadHierarchyLinks();
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
			
			if (m_Window.JumpLinksInstance.ProjectLinks.Links.Count == 0)
			{
				LoadProjectLinks();
			}

			//TODO: see if this can use the SceneStateMonitor instead of doing this in place
			EditorApplication.delayCall +=
				delegate ()
				{
					int sceneCount = EditorSceneManager.sceneCount;
					for (int i = 0; i < sceneCount; i++)
					{
						Scene scene = EditorSceneManager.GetSceneAt(i);
						if (scene.isLoaded)
							LoadHierarchyLinks(scene.GetHashCode(), scene);
					}

					m_Window.Repaint();
				};
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

		private void SaveProjectLinks()
		{
			string filePath = m_SaveDirectory + ProjectLinksSaveFile + SaveFileExtension;
			Object[] linkReferences = m_Window.JumpLinksInstance.ProjectLinks.AllLinkReferences;
			if (linkReferences != null)
			{
				using (StreamWriter streamWriter = new StreamWriter(filePath))
				{
					try
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
					catch (System.Exception ex)
					{
						Debug.LogError("JumpTo Error: Unable to save project links; error when writing to file\n" + ex.Message);
					}
				}
			}
			else
			{
				DeleteSaveFile(filePath);
			}
		}

		public void SaveHierarchyLinks(int sceneId)
		{
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
						for (int i = 0; i < linkPaths.Length; i++)
						{
							if (linkPaths[i] != null &&
								linkPaths[i].Length > 0)
								streamWriter.WriteLine(linkPaths[i]);
						}
					}
					catch (System.Exception ex)
					{
						Debug.LogError("JumpTo Error: Unable to save hierarchy links; error when writing to file\n" + ex.Message);
					}
				}
			}
			else
			{
				DeleteSaveFile(filePath);
			}
		}
		
		private void LoadProjectLinks()
		{
			string filePath = m_SaveDirectory + ProjectLinksSaveFile + SaveFileExtension;
			if (!File.Exists(filePath))
				return;

			JumpLinkContainer<ProjectJumpLink> links = m_Window.JumpLinksInstance.ProjectLinks;
			links.RemoveAll();

			using (StreamReader streamReader = new StreamReader(filePath))
			{
				try
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
				catch (System.Exception ex)
				{
					Debug.LogError("JumpTo Error: Unable to load project links; error when reading from file\n" + ex.Message);
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
						//int rootOrder = 0;
						char[] delimiterPipe = new char[] { '|' };
						char[] delimeterForwardSlash = new char[] { '/' };
						string[] lineSegments;
						//string[] rootOrders;
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
										//TODO: the path is built with invalid assumptions
										//		does not work for nested prefabs
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
					}	//if root objects exist
				}   //try
				catch (System.Exception ex)
				{
					Debug.LogError("JumpTo Error: Unable to load hierarchy links; error when reading from file\n" + ex.ToString());
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

					string paths = "|" +
							JumpToUtility.GetRootOrderPath(linkReferenceTransform) + "|" +
							JumpToUtility.GetTransformPath(linkReferenceTransform);

					PrefabType prefabType = PrefabUtility.GetPrefabType(linkReferenceObject);
					
					if (prefabType == PrefabType.ModelPrefabInstance ||
						prefabType == PrefabType.PrefabInstance)
					{
						linkReferenceObject = PrefabUtility.GetPrefabObject(linkReferenceObject);
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
	}
}

