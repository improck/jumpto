﻿using UnityEditor;
using UnityEngine;


namespace ImpRock.JumpTo.Editor
{
	//TODO: may not need to set prefab icons manually anymore
	//TODO: unity's selected item colors have changed; try to match
	//TODO: there are now "on" variants of many icons
	//TODO: there are now "d_*" prefixed versions of many icons; EditorGUIUtility.Load() seems to prefer that one
	internal sealed class GraphicAssets
	{
		#region Singleton
		private static GraphicAssets s_Instance = null;

		public static GraphicAssets Instance { get { if (s_Instance == null) { s_Instance = new GraphicAssets(); s_Instance.InitAssets(); } return s_Instance; } }

		
		public static void DestroyInstance()
		{
			if (s_Instance != null)
			{
				s_Instance = null;
			}
		}
		#endregion

		//***** IMAGES *****

		public Texture2D IconPrefabNormal { get; private set; }
		public Texture2D IconPrefabModel { get; private set; }
		public Texture2D IconGameObject { get; private set; }
		public Texture2D IconProjectView { get; private set; }
		public Texture2D IconHierarchyView { get; private set; }
		
		//***** GUI STYLES *****
		
		public GUIStyle LinkViewTitleStyle { get; private set; }
		public GUIStyle LinkLabelStyle { get; private set; }
		public GUIStyle DragDropInsertionStyle { get; private set; }
		public GUIStyle FoldoutStyle { get; private set; }

		//***** COLORS *****

		public Color[] LinkTextColors { get; private set; }
		public Color[] SelectedLinkTextColors { get; private set; }

		//***** CONSTANTS *****

		public readonly Color DisabledColorModifier = new(0.0f, 0.0f, 0.0f, 0.4f);
		public const float LinkHeight = 16.0f;
		public const float LinkViewTitleBarHeight = 18.0f;
		public const bool ForceProSkin = false;


		public void InitGuiStyle()
		{
			GUISkin editorSkin;
			if (EditorGUIUtility.isProSkin || ForceProSkin)
				editorSkin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene);
			else
				editorSkin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);

			LinkViewTitleStyle = new GUIStyle(editorSkin.GetStyle("ProjectBrowserTopBarBg"))
			{
				name = "Link View Title",
				alignment = TextAnchor.MiddleLeft,
				clipping = TextClipping.Clip,
				contentOffset = new Vector2(0.0f, -1.0f),
				font = null,
				fontSize = 0,
				imagePosition = ImagePosition.TextOnly
			};

			RectOffset padding = LinkViewTitleStyle.padding;
			padding.top = 0;
			padding.bottom = 0;
			padding.left = 32;
			padding.right = 32;

			LinkViewTitleStyle.normal.textColor = LinkTextColors[0];

			LinkLabelStyle = new GUIStyle(editorSkin.GetStyle("PR Label"))
			{
				name = "Link Label Style"
			};

			padding = LinkLabelStyle.padding;
			padding.left = 8;
			
			FoldoutStyle = new GUIStyle(editorSkin.GetStyle("Foldout"));

			DragDropInsertionStyle = new GUIStyle(editorSkin.GetStyle("PR Insertion"))
			{
				imagePosition = ImagePosition.ImageOnly,
				contentOffset = new Vector2(0.0f, -16.0f)
			};
		}

		public void InitAssets()
		{
			IconPrefabNormal = EditorGUIUtility.Load("Prefab On Icon") as Texture2D;
			IconPrefabModel = EditorGUIUtility.Load("PrefabModel Icon") as Texture2D;
			IconGameObject = EditorGUIUtility.Load("GameObject Icon") as Texture2D;
			IconProjectView = EditorGUIUtility.Load("Project") as Texture2D;
			IconHierarchyView = EditorGUIUtility.Load("d_SceneAsset Icon") as Texture2D;

			if (EditorGUIUtility.isProSkin || ForceProSkin)
			{
				LinkTextColors = new Color[]
				{
					new(0.7f, 0.7f, 0.7f, 1.0f),	//normal
					new(0.84f, 0.6f, 0.92f, 1.0f),	//model
					new(0.298f, 0.5f, 0.85f, 1.0f),	//prefab
					new(0.7f, 0.4f, 0.4f, 1.0f)		//broken prefab
				};
			}
			else
			{
				LinkTextColors = new Color[]
				{
					Color.black,					//normal
					new(0.6f, 0.0f, 0.8f, 1.0f),	//model
					new(0.0f, 0.3f, 0.6f, 1.0f),	//prefab
					new(0.4f, 0.0f, 0.0f, 1.0f)		//broken prefab
				};
			}

			SelectedLinkTextColors = new Color[]
			{
				Color.white,						//normal
				new(0.92f, 0.8f, 1.0f, 1.0f),		//model
				new(0.7f, 0.75f, 1.0f, 1.0f),		//prefab
				new(1.0f, 0.7f, 0.7f, 1.0f)			//broken prefab
			};
		}
	}
}
