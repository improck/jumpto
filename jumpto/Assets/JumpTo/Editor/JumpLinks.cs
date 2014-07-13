using UnityEditor;
using UnityEngine;
using System.Collections.Generic;


namespace JumpTo
{
	[System.Serializable]
	public class ProjectJumpLink : ScriptableObject
	{
		[SerializeField] private UnityEngine.Object m_LinkReference;
		[SerializeField] private GUIContent m_LinkLabelContent = new GUIContent();
		[SerializeField] private Color m_LinkColor = Color.black;


		public UnityEngine.Object LinkReference { get { return m_LinkReference; } set { m_LinkReference = value; } }
		public GUIContent LinkLabelContent { get { return m_LinkLabelContent; } set { m_LinkLabelContent = value; } }
		public Color LinkColor { get { return m_LinkColor; } set { m_LinkColor = value; } }
		public RectRef Area { get; set; }


		public ProjectJumpLink()
		{
			Area = new RectRef();
		}

		void OnSerialize()
		{
		}

		void OnDeserialize()
		{
		}
	}


	[System.Serializable]
	public class HierarchyJumpLink : ScriptableObject
	{
		[SerializeField] private UnityEngine.Object m_LinkReference;
		[SerializeField] private GUIContent m_LinkLabelContent = new GUIContent();
		[SerializeField] private Color m_LinkColor = Color.black;


		public UnityEngine.Object LinkReference { get { return m_LinkReference; } set { m_LinkReference = value; } }
		public GUIContent LinkLabelContent { get { return m_LinkLabelContent; } set { m_LinkLabelContent = value; } }
		public Color LinkColor { get { return m_LinkColor; } set { m_LinkColor = value; } }
		public RectRef Area { get; set; }


		public HierarchyJumpLink()
		{
			Area = new RectRef();
		}

		void OnSerialize()
		{
		}

		void OnDeserialize()
		{
		}
	}

	
	[System.Serializable]
	public class JumpLinks : ScriptableObject
	{
		#region Pseudo-Singleton
		private static JumpLinks s_Instance = null;

		public static JumpLinks Instance { get { return s_Instance; } }

		public static JumpLinks Create()
		{
			JumpLinks instance = ScriptableObject.CreateInstance<JumpLinks>();
			instance.hideFlags = HideFlags.HideAndDontSave;

			return instance;
		}


		protected JumpLinks() { s_Instance = this; }
		#endregion


		[SerializeField] private List<ProjectJumpLink> m_ProjectLinks = new List<ProjectJumpLink>();
		[SerializeField] private List<HierarchyJumpLink> m_HierarchyLinks = new List<HierarchyJumpLink>();


		public List<ProjectJumpLink> ProjectLinks { get { return m_ProjectLinks; } }
		public List<HierarchyJumpLink> HierarchyLinks { get { return m_HierarchyLinks; } }


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
					AddHierarchyLink(linkReference, prefabType);
				}
				else
				{
					AddProjectLink(linkReference, prefabType);
				}
			}
			else if (!(linkReference is Component))
			{
				AddProjectLink(linkReference, PrefabType.None);
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
				AddProjectLink(linkReference, prefabType);
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
				AddHierarchyLink(linkReference, prefabType);
			}
		}

		public void RemoveProjectLink(int index)
		{
			if (index < 0 || index > m_ProjectLinks.Count - 1)
				return;

			m_ProjectLinks.RemoveAt(index);

			for (int i = index; i < m_ProjectLinks.Count; i++)
			{
				m_ProjectLinks[i].Area.y = i * GraphicAssets.LinkHeight;
			}
		}

		public void RemoveHierarchyLink(int index)
		{
			if (index < 0 || index > m_HierarchyLinks.Count - 1)
				return;

			m_HierarchyLinks.RemoveAt(index);

			for (int i = index; i < m_HierarchyLinks.Count; i++)
			{
				m_HierarchyLinks[i].Area.y = i * GraphicAssets.LinkHeight;
			}
		}

		public void MoveProjectLink(int from, int to)
		{
			if (from == to)
				return;

			ProjectJumpLink link = m_ProjectLinks[from];
			m_ProjectLinks.RemoveAt(from);

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

			m_ProjectLinks.Insert(to, link);

			for (; min < max; min++)
			{
				m_ProjectLinks[min].Area.y = min * GraphicAssets.LinkHeight;
			}
		}

		public void MoveHierarchyLink(int from, int to)
		{
			if (from == to)
				return;

			HierarchyJumpLink link = m_HierarchyLinks[from];
			m_HierarchyLinks.RemoveAt(from);

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

			m_HierarchyLinks.Insert(to, link);

			for (; min < max; min++)
			{
				m_HierarchyLinks[min].Area.y = min * GraphicAssets.LinkHeight;
			}
		}

		public void RefreshLinksY()
		{
			for (int i = 0; i < m_ProjectLinks.Count; i++)
			{
				m_ProjectLinks[i].Area.y = i * GraphicAssets.LinkHeight;
			}

			for (int i = 0; i < m_HierarchyLinks.Count; i++)
			{
				m_HierarchyLinks[i].Area.y = i * GraphicAssets.LinkHeight;
			}
		}

		private void AddProjectLink(UnityEngine.Object linkReference, PrefabType prefabType)
		{
			//basically, if no linked object in the list has a reference to the passed object
			if (!m_ProjectLinks.Exists(linked => linked.LinkReference == linkReference))
			{
				ProjectJumpLink link = ScriptableObject.CreateInstance<ProjectJumpLink>();
				link.hideFlags = HideFlags.HideAndDontSave;
				link.LinkReference = linkReference;

				GUIContent linkContent = EditorGUIUtility.ObjectContent(linkReference, linkReference.GetType());
				link.LinkLabelContent.image = linkContent.image;
				link.LinkLabelContent.tooltip = AssetDatabase.GetAssetPath(linkReference);

				if (linkContent.text == string.Empty)
				{
					if (linkReference.name != string.Empty)
					{
						link.LinkLabelContent.text = linkReference.name;
					}
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
						if (link.LinkLabelContent.image == null)
							link.LinkLabelContent.image = graphicAssets.IconPrefabNormal;

						link.LinkColor = Color.blue;
					}
					else if (prefabType == PrefabType.ModelPrefab)
					{
						if (link.LinkLabelContent.image == null)
							link.LinkLabelContent.image = graphicAssets.IconPrefabModel;
						link.LinkColor = graphicAssets.ColorViolet;
					}
					else if (link.LinkLabelContent.image == null)
					{
						link.LinkLabelContent.image = graphicAssets.IconGameObject;
						//color = black
					}
				}

				link.Area.Set(0.0f, m_ProjectLinks.Count * GraphicAssets.LinkHeight, 100.0f, GraphicAssets.LinkHeight);

				m_ProjectLinks.Add(link);
			}
		}

		private void AddHierarchyLink(UnityEngine.Object linkReference, PrefabType prefabType)
		{
			//basically, if no linked object in the list has a reference to the passed object
			if (!m_HierarchyLinks.Exists(linked => linked.LinkReference == linkReference))
			{
				HierarchyJumpLink link = ScriptableObject.CreateInstance<HierarchyJumpLink>();
				link.hideFlags = HideFlags.HideAndDontSave;
				link.LinkReference = linkReference;

				GUIContent linkContent = EditorGUIUtility.ObjectContent(linkReference, linkReference.GetType());
				link.LinkLabelContent.text = linkContent.text != string.Empty ? linkContent.text : "[Unnamed]";
				link.LinkLabelContent.image = linkContent.image;

				if (linkReference is GameObject)
				{
					GraphicAssets graphicAssets = GraphicAssets.Instance;

					if (prefabType == PrefabType.PrefabInstance)
					{
						if (link.LinkLabelContent.image == null)
							link.LinkLabelContent.image = graphicAssets.IconPrefabNormal;

						link.LinkColor = Color.blue;
					}
					else if (prefabType == PrefabType.ModelPrefabInstance)
					{
						if (link.LinkLabelContent.image == null)
							link.LinkLabelContent.image = graphicAssets.IconPrefabModel;
						link.LinkColor = graphicAssets.ColorViolet;
					}
					else if (prefabType == PrefabType.DisconnectedPrefabInstance ||
						prefabType == PrefabType.DisconnectedModelPrefabInstance ||
						prefabType == PrefabType.MissingPrefabInstance)
					{
						if (link.LinkLabelContent.image == null)
							link.LinkLabelContent.image = graphicAssets.IconPrefabNormal;

						link.LinkColor = Color.red;
					}
					//else if (prefabType == PrefabType.DisconnectedModelPrefabInstance)
					//{
					//	if (link.LinkLabelContent.image == null)
					//		link.LinkLabelContent.image = graphicAssets.IconPrefabModel;

					//	link.LinkColor = Color.red;
					//}
					else if (link.LinkLabelContent.image == null)
					{
						link.LinkLabelContent.image = graphicAssets.IconGameObject;
						//color = black
					}

					Transform linkTransform = (linkReference as GameObject).transform;
					link.LinkLabelContent.tooltip = GetTransformPath(linkTransform);
				}

				link.Area.Set(0.0f, m_HierarchyLinks.Count * GraphicAssets.LinkHeight, 100.0f, GraphicAssets.LinkHeight);

				m_HierarchyLinks.Add(link);
			}
		}
		
		public int ProjectLinkHitTest(Vector2 position)
		{
			int linkCount = m_ProjectLinks.Count;
			if (linkCount > 0)
			{
				if (linkCount == 1)
				{
					if (m_ProjectLinks[0].Area.RectInternal.Contains(position))
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

						rect = m_ProjectLinks[index].Area;

						if (rect.yMax < position.y)
						{
							indexMin = index + 1;
						}
						else if (rect.yMin > position.y)
						{
							indexMax = index - 1;
						}
						else if (position.x > rect.xMin && position.x < rect.xMax)
						{
							return index;
						}
					}
				}
			}

			return -1;
		}

		public int HierarchyLinkHitTest(Vector2 position)
		{
			int linkCount = m_HierarchyLinks.Count;
			if (linkCount > 0)
			{
				if (linkCount == 1)
				{
					if (m_HierarchyLinks[0].Area.RectInternal.Contains(position))
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

						rect = m_HierarchyLinks[index].Area;

						if (rect.yMax < position.y)
						{
							indexMin = index + 1;
						}
						else if (rect.yMin > position.y)
						{
							indexMax = index - 1;
						}
						else if (position.x > rect.xMin && position.x < rect.xMax)
						{
							return index;
						}
					}
				}
			}

			return -1;
		}

		public void CheckHierarchyLinks()
		{
			for (int i = 0; i < m_HierarchyLinks.Count; i++)
			{
				//TODO: how?

				//NOTE: seems to be causing a crash
				//m_HierarchyLinks[i].Area.Set(0.0f, i * GraphicAssets.LinkHeight, 100.0f, GraphicAssets.LinkHeight);
			}
		}

		public void CheckProjectLinks()
		{
			for (int i = 0; i < m_ProjectLinks.Count; i++)
			{
				//TODO: how?

				//m_ProjectLinks[i].Area.Set(0.0f, i * GraphicAssets.LinkHeight, 100.0f, GraphicAssets.LinkHeight);
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
}
