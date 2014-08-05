using UnityEditor;
using UnityEngine;
using System.IO;
using SceneStateDetection;


namespace JumpTo
{
	public class SerializationControl
	{
		#region Singleton
		private static SerializationControl s_Instance = null;

		public static SerializationControl Instance { get { if (s_Instance == null) { s_Instance = new SerializationControl(); } return s_Instance; } }

		public static void DestroyInstance()
		{
			if (s_Instance != null)
			{
				s_Instance.CleanUp();
				s_Instance = null;
			}
		}
		#endregion

		private string m_SaveDirectory = string.Empty;
		private string m_HierarchySaveDirectory = string.Empty;


		private const string ProjectLinksSaveFile = "projectlinks";
		private const string SettingsSaveFile = "settings";
		private const string SaveFileExtension = ".jumpto";


		public SerializationControl()
		{
			SceneStateControl.OnSceneSaved += OnSceneSaved;
			SceneStateControl.OnSceneLoaded += OnSceneLoaded;

			JumpToEditorWindow.OnWillEnable += OnWindowEnable;
			JumpToEditorWindow.OnWillClose += OnWindowClose;
		}

		private void CleanUp()
		{
			SceneStateControl.OnSceneSaved -= OnSceneSaved;
			SceneStateControl.OnSceneLoaded -= OnSceneLoaded;

			JumpToEditorWindow.OnWillEnable -= OnWindowEnable;
			JumpToEditorWindow.OnWillClose -= OnWindowClose;
		}

		private void OnSceneSaved()
		{
			if (CreateSaveDirectories())
			{
				SaveSettings();
				SaveProjectLinks();
				SaveHierarchyLinks();
			}
		}

		private void OnSceneLoaded()
		{
			SetSaveDirectoryPaths();

			LoadHierarchyLinks();
		}

		private void OnWindowEnable()
		{
			SetSaveDirectoryPaths();

			LoadSettings();
			LoadProjectLinks();
		}

		private void OnWindowDisable()
		{
		}

		private void OnWindowClose()
		{
			if (CreateSaveDirectories())
			{
				SaveSettings();
				SaveProjectLinks();
				SaveHierarchyLinks();
			}
		}

		private void SaveProjectLinks()
		{
			string filePath = m_SaveDirectory + ProjectLinksSaveFile + SaveFileExtension;
			Object[] linkReferences = JumpLinks.Instance.GetJumpLinkContainer<ProjectJumpLink>().AllLinkReferences;
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

		private void SaveHierarchyLinks()
		{
			string currentScene = EditorApplication.currentScene;
			string sceneGuid = AssetDatabase.AssetPathToGUID(currentScene);
			string filePath = m_HierarchySaveDirectory + sceneGuid + SaveFileExtension;
			Object[] linkReferences = JumpLinks.Instance.HierarchyLinks.AllLinkReferences;
			if (linkReferences != null)
			{
				//TODO: error handling
				using (StreamWriter streamWriter = new StreamWriter(filePath))
				{
					//int instanceId;
					string line;
					for (int i = 0; i < linkReferences.Length; i++)
					{
						//NOTE: can't use instanceId because they change each time the scene is loaded!
						//instanceId = linkReferences[i].GetInstanceID();
						//line = instanceId.ToString();

						line = (linkReferences[i] as GameObject).transform.GetTransformPath();

						streamWriter.WriteLine(line);
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
				JumpToSettings settings = JumpToSettings.Instance;

				streamWriter.WriteLine(settings.Visibility);
				streamWriter.WriteLine(settings.ProjectFirst);
				streamWriter.WriteLine(settings.Vertical);
			}
		}
		
		private void LoadProjectLinks()
		{
			string filePath = m_SaveDirectory + ProjectLinksSaveFile + SaveFileExtension;
			if (!File.Exists(filePath))
				return;

			JumpLinkContainer<ProjectJumpLink> links = JumpLinks.Instance.GetJumpLinkContainer<ProjectJumpLink>();
			links.RemoveAll();

			//TODO: error handling
			using (StreamReader streamReader = new StreamReader(filePath))
			{
				int instanceId;
				string line;
				string path;
				while (!streamReader.EndOfStream)
				{
					JumpLinks jumpLinks = JumpLinks.Instance;

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
			SetSaveDirectoryPaths();

			HierarchyJumpLinkContainer links = JumpLinks.Instance.HierarchyLinks;
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
				string line;
				while (!streamReader.EndOfStream)
				{
					JumpLinks jumpLinks = JumpLinks.Instance;

					line = streamReader.ReadLine();
					//NOTE: can't use instanceId because they change each time the scene is loaded!
					//Object obj = EditorUtility.InstanceIDToObject(int.Parse(line));
					//if (obj != null && obj is GameObject)
					//{
					//	jumpLinks.CreateOnlyHierarchyJumpLink(obj);
					//}

					GameObject obj = GameObject.Find(line);
					if (obj != null && obj is GameObject)
					{
						jumpLinks.CreateOnlyHierarchyJumpLink(obj);
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
				JumpToSettings settings = JumpToSettings.Instance;
				settings.Visibility= (JumpToSettings.VisibleList)System.Enum.Parse(typeof(JumpToSettings.VisibleList), streamReader.ReadLine());
				settings.ProjectFirst = bool.Parse(streamReader.ReadLine());
				settings.Vertical = bool.Parse(streamReader.ReadLine());
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
	}
}
