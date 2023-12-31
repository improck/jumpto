﻿using UnityEditor;
using UnityEngine;


namespace ImpRock.JumpTo.Editor
{
	[System.Serializable]
	internal sealed class HierarchyJumpLinkContainerFaketionary : Faketionary<int, HierarchyJumpLinkContainer> { }


	internal sealed class HierarchyJumpLinkContainer : JumpLinkContainer<HierarchyJumpLink>
	{
		[SerializeField] bool m_HasLinksToUnsavedInstances = false;


		public bool HasLinksToUnsavedInstances { get { return m_HasLinksToUnsavedInstances; } }


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

				RaiseOnLinksChanged();
			}
		}

		public override void RefreshLinks()
		{
			m_HasLinksToUnsavedInstances = false;

			base.RefreshLinks();
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
				link.LinkLabelContent.tooltip = JumpToUtility.GetTransformPath(linkTransform);
				
				if (prefabType == PrefabType.PrefabInstance || prefabType == PrefabType.ModelPrefabInstance)
				{
					linkReference = PrefabUtility.GetPrefabObject(linkReference);
				}

				SerializedObject serializedObject = new SerializedObject(linkReference);
				serializedObject.SetInspectorMode(InspectorMode.Debug);
				if (serializedObject.GetLocalIdInFile() == 0 && prefabType != PrefabType.MissingPrefabInstance)
				{
					m_HasLinksToUnsavedInstances = true;
				}
			}
		}
	}
}
