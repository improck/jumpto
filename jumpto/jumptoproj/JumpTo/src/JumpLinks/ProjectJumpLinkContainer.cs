using UnityEditor;
using UnityEngine;


namespace JumpTo
{
	internal sealed class ProjectJumpLinkContainer : JumpLinkContainer<ProjectJumpLink>
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
	}
}
