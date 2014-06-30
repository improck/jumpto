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

		public float ProjectMaxY
		{
			get
			{
				if (m_ProjectLinks.Count > 0)
				{
					return m_ProjectLinks[m_ProjectLinks.Count - 1].Area.y + GraphicAssets.LinkHeight;
				}

				return 0.0f;
			}
		}

		public float HierarchyMaxY
		{
			get
			{
				if (m_HierarchyLinks.Count > 0)
				{
					return m_HierarchyLinks[m_HierarchyLinks.Count - 1].Area.y + GraphicAssets.LinkHeight;
				}

				return 0.0f;
			}
		}


		//void OnEnable()
		//{
		//	s_Instance = this;
		//}

		public void CreateJumpLink(UnityEngine.Object linkObject)
		{
			PrefabType prefabType = PrefabType.None;
			if (linkObject is GameObject)
			{
				prefabType = PrefabUtility.GetPrefabType(linkObject);
				if (prefabType == PrefabType.None ||
					prefabType == PrefabType.PrefabInstance ||
					prefabType == PrefabType.ModelPrefabInstance ||
					prefabType == PrefabType.DisconnectedPrefabInstance ||
					prefabType == PrefabType.DisconnectedModelPrefabInstance ||
					prefabType == PrefabType.MissingPrefabInstance)
				{
					AddHierarchyLink(linkObject, prefabType);
				}
				else
				{
					AddProjectLink(linkObject, prefabType);
				}
			}
			else
			{
				AddProjectLink(linkObject, prefabType);
			}
		}

		public void RemoveProjectLink(int index)
		{
			if (index < 0 || index > m_ProjectLinks.Count - 1)
				return;

			m_ProjectLinks.RemoveAt(index);
		}

		public void RemoveHierarchyLink(int index)
		{
			if (index < 0 || index > m_HierarchyLinks.Count - 1)
				return;

			m_HierarchyLinks.RemoveAt(index);
		}

		private void AddProjectLink(UnityEngine.Object linkObject, PrefabType prefabType)
		{
			//basically, if no linked object in the list has a reference to the passed object
			if (!m_ProjectLinks.Exists(linked => linked.LinkReference == linkObject))
			{
				ProjectJumpLink link = ScriptableObject.CreateInstance<ProjectJumpLink>();
				link.hideFlags = HideFlags.HideAndDontSave;
				link.LinkReference = linkObject;

				GUIContent linkContent = EditorGUIUtility.ObjectContent(linkObject, linkObject.GetType());
				link.LinkLabelContent.image = linkContent.image;
				link.LinkLabelContent.tooltip = AssetDatabase.GetAssetPath(linkObject);

				if (linkContent.text == string.Empty)
				{
					if (linkObject.name != string.Empty)
					{
						link.LinkLabelContent.text = linkObject.name;
					}
					else
					{
						string assetName = AssetDatabase.GetAssetPath(linkObject.GetInstanceID());
						int slash = assetName.LastIndexOf('/');
						int dot = assetName.LastIndexOf('.');
						link.LinkLabelContent.text = assetName.Substring(slash + 1, dot - slash - 1);
					}
				}
				else
				{
					link.LinkLabelContent.text = linkContent.text;
				}

				if (linkObject is GameObject)
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

				m_ProjectLinks.Add(link);

				//TODO: position it at the bottom of the list
				if (m_ProjectLinks.Count > 1)
				{
					Rect lastArea = m_ProjectLinks[m_ProjectLinks.Count - 1].Area;
					Rect linkArea = link.Area;
					linkArea.x = lastArea.x;
					linkArea.y = lastArea.y + GraphicAssets.LinkHeight;
					linkArea.width = lastArea.width;
					linkArea.height = lastArea.height;
					link.Area.Set(linkArea);
				}
			}
		}

		private void AddHierarchyLink(UnityEngine.Object linkObject, PrefabType prefabType)
		{
			//basically, if no linked object in the list has a reference to the passed object
			if (!m_HierarchyLinks.Exists(linked => linked.LinkReference == linkObject))
			{
				HierarchyJumpLink link = ScriptableObject.CreateInstance<HierarchyJumpLink>();
				link.hideFlags = HideFlags.HideAndDontSave;
				link.LinkReference = linkObject;

				GUIContent linkContent = EditorGUIUtility.ObjectContent(linkObject, linkObject.GetType());
				link.LinkLabelContent.text = linkContent.text != string.Empty ? linkContent.text : "[Unnamed]";
				link.LinkLabelContent.image = linkContent.image;

				if (linkObject is GameObject)
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

					Transform linkTransform = (linkObject as GameObject).transform;
					link.LinkLabelContent.tooltip = GetTransformPath(linkTransform);
				}

				m_HierarchyLinks.Add(link);

				//TODO: position it at the bottom of the list
				if (m_HierarchyLinks.Count > 1)
				{
					Rect lastArea = m_HierarchyLinks[m_HierarchyLinks.Count - 1].Area;
					Rect linkArea = link.Area;
					linkArea.x = lastArea.x;
					linkArea.y = lastArea.y + GraphicAssets.LinkHeight;
					linkArea.width = lastArea.width;
					linkArea.height = lastArea.height;
					link.Area.Set(linkArea);
				}
			}
		}
		
		public ProjectJumpLink ProjectLinkHitTest(Vector2 position)
		{
			//TODO: binary search of list sorted by y-position

			//int linkCount = m_ProjectLinks.Count;
			//if (linkCount > 0)
			//{
			//	if (linkCount == 1)
			//	{
			//		RectRef rect = m_ProjectLinks[0].Area;
			//		if (rect.yMin == position.y ||
			//				(rect.yMin > position.y && rect.yMax < position.y))
			//		{
			//			if (m_ProjectLinks[0].Visible)
			//				return m_ProjectLinks[0];
			//			else
			//				return null;
			//		}
			//	}
			//	else
			//	{
			//		int indexMin = 0;
			//		int indexMax = linkCount - 1;
			//		int index = 0;
			//		RectRef rect = null;

			//		while (indexMax >= indexMin)
			//		{
			//			index = (indexMin + indexMax) >> 1;

			//			rect = m_ProjectLinks[index].Area;
						
			//			if (rect.yMax < position.y)
			//			{
			//				indexMin = index + 1;
			//			}
			//			else if (rect.yMin > position.y)
			//			{
			//				indexMax = index - 1;
			//			}
			//			else if (rect.yMin == position.y ||
			//				(rect.yMin > position.y && rect.yMax < position.y))
			//			{
			//				if (m_ProjectLinks[index].Visible)
			//					return m_ProjectLinks[index];
			//				else
			//					return null;
			//			}
			//		}
			//	}
			//}

			return null;
		}

		public HierarchyJumpLink HierarchyLinkHitTest(Vector2 position)
		{
			//TODO: binary search of list sorted by y-position

			return null;
		}

		public void CheckHierarchyLinks()
		{
			//TODO: how?
		}

		public void CheckProjectLinks()
		{
			//TODO: how?
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
