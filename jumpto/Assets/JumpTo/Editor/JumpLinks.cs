using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;


namespace JumpTo
{
	[System.Serializable]
	public enum LinkReferenceType
	{
		Asset = 0,
		GameObject = 0,
		Model = 1,
		ModelInstance = 1,
		Prefab = 2,
		PrefabInstance = 2,
		PrefabInstanceBroken = 3
	}

	
	public abstract class JumpLink : ScriptableObject
	{
		[SerializeField] protected UnityEngine.Object m_LinkReference;
		[SerializeField] protected GUIContent m_LinkLabelContent = new GUIContent();
		[SerializeField] protected LinkReferenceType m_ReferenceType = LinkReferenceType.Asset;
		[SerializeField] protected bool m_Selected = false;


		public UnityEngine.Object LinkReference { get { return m_LinkReference; } set { m_LinkReference = value; } }
		public GUIContent LinkLabelContent { get { return m_LinkLabelContent; } set { m_LinkLabelContent = value; } }
		public LinkReferenceType ReferenceType { get { return m_ReferenceType; } set { m_ReferenceType = value; } }
		public RectRef Area { get; set; }
		public bool Selected { get { return m_Selected; } set { m_Selected = value; } }


		public JumpLink()
		{
			Area = new RectRef();
		}
	}


	public class ProjectJumpLink : JumpLink
	{
	}


	public class HierarchyJumpLink : JumpLink
	{
		[SerializeField] protected bool m_Active = true;


		public bool Active { get { return m_Active; } set { m_Active = value; } }
	}


	public abstract class JumpLinkContainer<T> : ScriptableObject where T : JumpLink
	{
		[SerializeField] protected List<T> m_Links = new List<T>();
		[SerializeField] protected int m_ActiveSelection = -1;


		public List<T> Links { get { return m_Links; } }
		public int ActiveSelection { get { return m_ActiveSelection; } set { m_ActiveSelection = Mathf.Clamp(value, -1, m_Links.Count - 1); } }
		public T ActiveSelectedObject { get { return m_ActiveSelection > -1 ? m_Links[m_ActiveSelection] : null; } }
		
		public T this[int index]
		{
			get
			{
				if (index < 0 || index >= m_Links.Count)
					throw new System.IndexOutOfRangeException(typeof(T) + ": Link index out of range (" + index + "/" + m_Links.Count + ")");

				return m_Links[index];
			}
		}

		public bool HasSelection
		{
			get
			{
				for (int i = 0; i < m_Links.Count; i++)
				{
					if (m_Links[i].Selected)
						return true;
				}

				return false;
			}
		}

		public int SelectionCount
		{
			get
			{
				int count = 0;
				for (int i = 0; i < m_Links.Count; i++)
				{
					if (m_Links[i].Selected)
						count++;
				}

				return count;
			}
		}

		public T[] SelectedLinks
		{
			get
			{
				if (m_Links.Count == 0)
					return null;
				else
				{
					List<T> selection = new List<T>();
					for (int i = 0; i < m_Links.Count; i++)
					{
						if (m_Links[i].Selected)
							selection.Add(m_Links[i]);
					}

					return selection.ToArray();
				}
			}
		}

		public Object[] SelectedLinkReferences
		{
			get
			{
				if (m_Links.Count == 0)
					return null;
				else
				{
					List<Object> selection = new List<Object>();
					for (int i = 0; i < m_Links.Count; i++)
					{
						if (m_Links[i].Selected)
							selection.Add(m_Links[i].LinkReference);
					}

					return selection.ToArray();
				}
			}
		}


		public abstract void AddLink(UnityEngine.Object linkReference, PrefabType prefabType);
		protected abstract void UpdateLinkInfo(T link, PrefabType prefabType);


		public void RemoveLink(int index)
		{
			if (index < 0 || index > m_Links.Count - 1)
				return;

			DestroyImmediate(m_Links[index]);
			m_Links.RemoveAt(index);

			for (int i = index; i < m_Links.Count; i++)
			{
				m_Links[i].Area.y = i * GraphicAssets.LinkHeight;
			}
		}

		public void RemoveSelected()
		{
			for (int i = m_Links.Count - 1; i >= 0; i--)
			{
				if (m_Links[i].Selected)
				{
					DestroyImmediate(m_Links[i]);
					m_Links.RemoveAt(i);
				}
			}

			m_ActiveSelection = -1;

			RefreshLinksY();
		}

		public void RemoveNonSelected()
		{
			T active = ActiveSelectedObject;

			for (int i = m_Links.Count - 1; i >= 0; i--)
			{
				if (!m_Links[i].Selected)
				{
					DestroyImmediate(m_Links[i]);
					m_Links.RemoveAt(i);
				}
			}

			m_ActiveSelection = m_Links.IndexOf(active);

			RefreshLinksY();
		}

		public void RemoveAll()
		{
			m_Links.Clear();
			m_ActiveSelection = -1;
		}

		public void MoveLink(int from, int to)
		{
			//can't move a link to itself
			if (from == to)
				return;

			//get the link, then remove it from the list
			T link = m_Links[from];
			m_Links.RemoveAt(from);

			//find the range of links that will shift
			//	within the list as a result of the move
			int min = 0;
			int max = 0;
			if (to > from)
			{
				to--;

				min = from;
				max = to + 1;
			}
			else
			{
				min = to;
				max = from + 1;
			}

			//re-insert the link
			m_Links.Insert(to, link);

			//adjust the y-positions of the affected links
			//	instead of all of the links
			for (; min < max; min++)
			{
				m_Links[min].Area.y = min * GraphicAssets.LinkHeight;
			}
		}

		public void MoveSelected(int to)
		{
			//get the indices of the selected links
			List<int> selection = new List<int>();
			for (int i = 0; i < m_Links.Count; i++)
			{
				if (m_Links[i].Selected)
				{
					//early return if trying to move the
					//	selection into itself
					if (i == to)
						return;

					selection.Add(i);
				}
			}

			//grab an array of the selected object before removal
			T[] selectionObjects = SelectedLinks;

			//remove the selected links from the list
			int toAdjusted = to;
			for (int i = selection.Count - 1; i >= 0; i--)
			{
				m_Links.RemoveAt(selection[i]);

				//modify the insertion index for selected links
				//	above it
				if (selection[i] < to)
					toAdjusted--;
			}

			//re-insert the selection
			m_Links.InsertRange(toAdjusted, selectionObjects);

			//fix all of the y-positions
			RefreshLinksY();
		}

		public void LinkSelectionSet(int index)
		{
			for (int i = 0; i < m_Links.Count; i++)
			{
				m_Links[i].Selected = i == index;
			}

			m_ActiveSelection = Mathf.Clamp(index, -1, m_Links.Count - 1);
		}

		public void LinkSelectionSetRange(int from, int to)
		{
			if (from > to)
			{
				int temp = to;
				to = from;
				from = temp;
			}

			for (int i = 0; i < from; i++)
			{
				m_Links[i].Selected = false;
			}

			for (int i = from; i <= to; i++)
			{
				m_Links[i].Selected = true;
			}

			for (int i = to + 1; i < m_Links.Count; i++)
			{
				m_Links[i].Selected = false;
			}
		}

		public void LinkSelectionInvert()
		{
			for (int i = 0; i < m_Links.Count; i++)
			{
				m_Links[i].Selected = !m_Links[i].Selected;
			}
		}

		public void LinkSelectionAdd(int index)
		{
			if (index >= 0 && index < m_Links.Count)
			{
				m_Links[index].Selected = true;
				m_ActiveSelection = index;
			}
		}

		public void LinkSelectionRemove(int index)
		{
			if (index >= 0 && index < m_Links.Count)
			{
				m_Links[index].Selected = false;
				m_ActiveSelection = index;
			}
		}

		public void LinkSelectionClear()
		{
			for (int i = 0; i < m_Links.Count; i++)
			{
				m_Links[i].Selected = false;
			}

			m_ActiveSelection = -1;
		}

		public void RefreshLinks()
		{
			for (int i = m_Links.Count - 1; i >= 0; i--)
			{
				if (m_Links[i].LinkReference == null)
				{
					DestroyImmediate(m_Links[i]);
					m_Links.RemoveAt(i);
				}
				else
				{
					UpdateLinkInfo(m_Links[i], PrefabUtility.GetPrefabType(m_Links[i].LinkReference));
				}
			}

			RefreshLinksY();
		}

		public void RefreshLinksY()
		{
			for (int i = 0; i < m_Links.Count; i++)
			{
				m_Links[i].Area.y = i * GraphicAssets.LinkHeight;
			}
		}

		public int LinkHitTest(Vector2 position)
		{
			int linkCount = m_Links.Count;
			if (linkCount > 0)
			{
				if (linkCount == 1)
				{
					if (m_Links[0].Area.RectInternal.Contains(position))
						return 0;
				}
				else
				{
					int indexMin = 0;
					int indexMax = linkCount - 1;
					int index = 0;
					RectRef rect = null;

					while (indexMax >= indexMin)
					{
						index = (indexMin + indexMax) >> 1;

						rect = m_Links[index].Area;

						//if below the link rect
						if (rect.yMax < position.y)
						{
							indexMin = index + 1;
						}
						//if above the link rect
						else if (rect.yMin > position.y)
						{
							indexMax = index - 1;
						}
						//if within the link rect
						else if (position.x >= rect.xMin && position.x <= rect.xMax)
						{
							return index;
						}
						//not within any link rect
						else
						{
							break;
						}
					}
				}
			}

			return -1;
		}

		public bool SelectionHitTest(Vector2 position)
		{
			for (int i = 0; i < m_Links.Count; i++)
			{
				if (m_Links[i].Selected && m_Links[i].Area.RectInternal.Contains(position))
					return true;
			}

			return false;
		}
	}


	public class ProjectJumpLinkContainer : JumpLinkContainer<ProjectJumpLink>
	{
		public override void AddLink(UnityEngine.Object linkReference, PrefabType prefabType)
		{
			//basically, if no linked object in the list has a reference to the passed object
			if (!m_Links.Exists(linked => linked.LinkReference == linkReference))
			{
				ProjectJumpLink link = ScriptableObject.CreateInstance<ProjectJumpLink>();
				link.hideFlags = HideFlags.HideAndDontSave;
				link.LinkReference = linkReference;

				UpdateLinkInfo(link, prefabType);

				link.Area.Set(0.0f, m_Links.Count * GraphicAssets.LinkHeight, 100.0f, GraphicAssets.LinkHeight);

				m_Links.Add(link);
			}
		}

		protected override void UpdateLinkInfo(ProjectJumpLink link, PrefabType prefabType)
		{
			UnityEngine.Object linkReference = link.LinkReference;
			GUIContent linkContent = EditorGUIUtility.ObjectContent(linkReference, linkReference.GetType());
			link.LinkLabelContent.image = linkContent.image;
			link.LinkLabelContent.tooltip = AssetDatabase.GetAssetPath(linkReference);

			//empty prefabs have no content text
			if (linkContent.text == string.Empty)
			{
				//try to get the name from the link reference itself
				if (linkReference.name != string.Empty)
				{
					link.LinkLabelContent.text = linkReference.name;
				}
				//otherwise pull the object name straight from the filename
				else
				{
					string assetName = AssetDatabase.GetAssetPath(linkReference.GetInstanceID());
					int slash = assetName.LastIndexOf('/');
					int dot = assetName.LastIndexOf('.');
					link.LinkLabelContent.text = assetName.Substring(slash + 1, dot - slash - 1);
				}
			}
			else
			{
				link.LinkLabelContent.text = linkContent.text;
			}

			if (linkReference is GameObject)
			{
				GraphicAssets graphicAssets = GraphicAssets.Instance;

				if (prefabType == PrefabType.Prefab)
				{
					link.ReferenceType = LinkReferenceType.Prefab;

					if (link.LinkLabelContent.image == null)
						link.LinkLabelContent.image = graphicAssets.IconPrefabNormal;
				}
				else if (prefabType == PrefabType.ModelPrefab)
				{
					link.ReferenceType = LinkReferenceType.Model;

					if (link.LinkLabelContent.image == null)
						link.LinkLabelContent.image = graphicAssets.IconPrefabModel;
				}
				else
				{
					link.ReferenceType = LinkReferenceType.Asset;

					if (link.LinkLabelContent.image == null)
						link.LinkLabelContent.image = graphicAssets.IconGameObject;
				}
			}
		}

		public void Save()
		{
			if (m_Links.Count > 0)
			{
				using (StreamWriter streamWriter = new StreamWriter(Application.dataPath + "\\..\\projectlinks.jumpto"))
				{
					int instanceId;
					string line;
					for (int i = 0; i < m_Links.Count; i++)
					{
						instanceId = m_Links[i].LinkReference.GetInstanceID();
						line = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(instanceId));
						if (AssetDatabase.IsSubAsset(instanceId))
							line += "|" + instanceId;

						streamWriter.WriteLine(line);
					}
				}
			}
		}

		public void Load()
		{
			string linksFilePath = Application.dataPath + "\\..\\projectlinks.jumpto";
			if (!File.Exists(linksFilePath))
				return;

			using (StreamReader streamReader = new StreamReader(linksFilePath))
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
	}


	public class HierarchyJumpLinkContainer : JumpLinkContainer<HierarchyJumpLink>
	{
		public override void AddLink(UnityEngine.Object linkReference, PrefabType prefabType)
		{
			//basically, if no linked object in the list has a reference to the passed object
			if (!m_Links.Exists(linked => linked.LinkReference == linkReference))
			{
				HierarchyJumpLink link = ScriptableObject.CreateInstance<HierarchyJumpLink>();
				link.hideFlags = HideFlags.HideAndDontSave;
				link.LinkReference = linkReference;

				UpdateLinkInfo(link, prefabType);

				link.Area.Set(0.0f, m_Links.Count * GraphicAssets.LinkHeight, 100.0f, GraphicAssets.LinkHeight);

				m_Links.Add(link);
			}
		}

		protected override void UpdateLinkInfo(HierarchyJumpLink link, PrefabType prefabType)
		{
			UnityEngine.Object linkReference = link.LinkReference;

			GUIContent linkContent = EditorGUIUtility.ObjectContent(linkReference, linkReference.GetType());
			link.LinkLabelContent.text = linkContent.text != string.Empty ? linkContent.text : "[Unnamed]";
			link.LinkLabelContent.image = linkContent.image;

			if (linkReference is GameObject)
			{
				GraphicAssets graphicAssets = GraphicAssets.Instance;

				link.Active = (link.LinkReference as GameObject).activeInHierarchy;

				if (prefabType == PrefabType.PrefabInstance)
				{
					link.ReferenceType = LinkReferenceType.PrefabInstance;

					if (link.LinkLabelContent.image == null)
						link.LinkLabelContent.image = graphicAssets.IconPrefabNormal;
				}
				else if (prefabType == PrefabType.ModelPrefabInstance)
				{
					link.ReferenceType = LinkReferenceType.ModelInstance;

					if (link.LinkLabelContent.image == null)
						link.LinkLabelContent.image = graphicAssets.IconPrefabModel;
				}
				else if (prefabType == PrefabType.DisconnectedPrefabInstance ||
					prefabType == PrefabType.DisconnectedModelPrefabInstance ||
					prefabType == PrefabType.MissingPrefabInstance)
				{
					link.ReferenceType = LinkReferenceType.PrefabInstanceBroken;

					if (link.LinkLabelContent.image == null)
						link.LinkLabelContent.image = graphicAssets.IconPrefabNormal;
				}
				else
				{
					link.ReferenceType = LinkReferenceType.GameObject;

					if (link.LinkLabelContent.image == null)
						link.LinkLabelContent.image = graphicAssets.IconGameObject;
				}

				Transform linkTransform = (linkReference as GameObject).transform;
				link.LinkLabelContent.tooltip = GetTransformPath(linkTransform);
			}
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
	}

	
	public class JumpLinks : ScriptableObject
	{
		#region Pseudo-Singleton
		private static JumpLinks s_Instance = null;

		public static JumpLinks Instance { get { return s_Instance; } }

		public static JumpLinks Create()
		{
			JumpLinks instance = ScriptableObject.CreateInstance<JumpLinks>();
			instance.hideFlags = HideFlags.HideAndDontSave;

			instance.m_ProjectLinkContainer = ScriptableObject.CreateInstance<ProjectJumpLinkContainer>();
			instance.m_ProjectLinkContainer.hideFlags = HideFlags.HideAndDontSave;
			instance.m_HierarchyLinkContainer = ScriptableObject.CreateInstance<HierarchyJumpLinkContainer>();
			instance.m_HierarchyLinkContainer.hideFlags = HideFlags.HideAndDontSave;

			return instance;
		}


		protected JumpLinks() { s_Instance = this; }
		#endregion


		[SerializeField] private ProjectJumpLinkContainer m_ProjectLinkContainer;
		[SerializeField] private HierarchyJumpLinkContainer m_HierarchyLinkContainer;


		public static void Save()
		{
			s_Instance.m_ProjectLinkContainer.Save();

			//TODO: save the hierarchy links for the loaded scene
			//NOTE: what if the current scene isn't saved yet?
		}

		public static void Load()
		{
			s_Instance.m_ProjectLinkContainer.Load();

			//TODO: load the hierarchy links for the loaded scene
			//NOTE: what if the current scene isn't saved yet?
		}


		public JumpLinkContainer<T> GetJumpLinkContainer<T>() where T : JumpLink
		{
			if (typeof(T) == typeof(ProjectJumpLink))
				return m_ProjectLinkContainer as JumpLinkContainer<T>;
			else
				return m_HierarchyLinkContainer as JumpLinkContainer<T>;
		}

		public static bool WouldBeProjectLink(UnityEngine.Object linkReference)
		{
			if (!(linkReference is GameObject))
			{
				return true;
			}

			PrefabType prefabType = PrefabUtility.GetPrefabType(linkReference);
			return prefabType == PrefabType.ModelPrefab || prefabType == PrefabType.Prefab;
		}

		public static bool WouldBeHierarchyLink(UnityEngine.Object linkReference)
		{
			PrefabType prefabType = PrefabUtility.GetPrefabType(linkReference);
			return linkReference is GameObject &&
				(prefabType == PrefabType.None ||
				   prefabType == PrefabType.PrefabInstance ||
				   prefabType == PrefabType.ModelPrefabInstance ||
				   prefabType == PrefabType.DisconnectedPrefabInstance ||
				   prefabType == PrefabType.DisconnectedModelPrefabInstance ||
				   prefabType == PrefabType.MissingPrefabInstance);
		}


		public void CreateJumpLink(UnityEngine.Object linkReference)
		{
			if (linkReference is GameObject)
			{
				PrefabType prefabType = PrefabUtility.GetPrefabType(linkReference);
				if (prefabType == PrefabType.None ||
					prefabType == PrefabType.PrefabInstance ||
					prefabType == PrefabType.ModelPrefabInstance ||
					prefabType == PrefabType.DisconnectedPrefabInstance ||
					prefabType == PrefabType.DisconnectedModelPrefabInstance ||
					prefabType == PrefabType.MissingPrefabInstance)
				{
					m_HierarchyLinkContainer.AddLink(linkReference, prefabType);
				}
				else
				{
					m_ProjectLinkContainer.AddLink(linkReference, prefabType);
				}
			}
			else if (!(linkReference is Component))
			{
				m_ProjectLinkContainer.AddLink(linkReference, PrefabType.None);
			}
		}

		public void CreateOnlyProjectJumpLink(UnityEngine.Object linkReference)
		{
			if (linkReference is Component)
				return;

			PrefabType prefabType = PrefabUtility.GetPrefabType(linkReference);
			if (!(linkReference is GameObject) ||
				prefabType == PrefabType.ModelPrefab ||
				prefabType == PrefabType.Prefab)
			{
				m_ProjectLinkContainer.AddLink(linkReference, prefabType);
			}
		}

		public void CreateOnlyHierarchyJumpLink(UnityEngine.Object linkReference)
		{
			if (linkReference is Component)
				return;

			PrefabType prefabType = PrefabUtility.GetPrefabType(linkReference);
			if (linkReference is GameObject &&
				(prefabType == PrefabType.None ||
				   prefabType == PrefabType.PrefabInstance ||
				   prefabType == PrefabType.ModelPrefabInstance ||
				   prefabType == PrefabType.DisconnectedPrefabInstance ||
				   prefabType == PrefabType.DisconnectedModelPrefabInstance ||
				   prefabType == PrefabType.MissingPrefabInstance))
			{
				m_HierarchyLinkContainer.AddLink(linkReference, prefabType);
			}
		}

		public void RefreshHierarchyLinks()
		{
			m_HierarchyLinkContainer.RefreshLinks();
		}

		public void RefreshProjectLinks()
		{
			m_ProjectLinkContainer.RefreshLinks();
		}
	}
}
