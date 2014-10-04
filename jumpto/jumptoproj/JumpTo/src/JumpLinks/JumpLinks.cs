﻿using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;


namespace JumpTo
{
	[System.Serializable]
	internal enum LinkReferenceType
	{
		Asset = 0,
		GameObject = 0,
		Model = 1,
		ModelInstance = 1,
		Prefab = 2,
		PrefabInstance = 2,
		PrefabInstanceBroken = 3
	}

	
	internal abstract class JumpLink : ScriptableObject
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


	internal sealed class ProjectJumpLink : JumpLink
	{
	}


	internal sealed class HierarchyJumpLink : JumpLink
	{
		[SerializeField] private bool m_Active = true;


		public bool Active { get { return m_Active; } set { m_Active = value; } }
	}


	internal sealed class JumpLinks : EditorScriptableObject<JumpLinks>
	{
		[SerializeField] private ProjectJumpLinkContainer m_ProjectLinkContainer;
		[SerializeField] private HierarchyJumpLinkContainer m_HierarchyLinkContainer;
		

		public ProjectJumpLinkContainer ProjectLinks { get { return m_ProjectLinkContainer; } }
		public HierarchyJumpLinkContainer HierarchyLinks { get { return m_HierarchyLinkContainer; } }


		protected override void Initialize()
		{
			m_ProjectLinkContainer = ScriptableObject.CreateInstance<ProjectJumpLinkContainer>();
			m_ProjectLinkContainer.hideFlags = HideFlags.HideAndDontSave;
			m_HierarchyLinkContainer = ScriptableObject.CreateInstance<HierarchyJumpLinkContainer>();
			m_HierarchyLinkContainer.hideFlags = HideFlags.HideAndDontSave;
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
