using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Experimental;


namespace ImpRock.JumpTo.Editor
{
	internal sealed class GraphicAssets
	{
		#region Singleton
		private static GraphicAssets s_Instance = null;

		public static GraphicAssets Instance { get { if (s_Instance == null) { s_Instance = new GraphicAssets(); } return s_Instance; } }

		
		private GraphicAssets()
		{
			InitAssets();
		}

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

		public readonly Color DisabledColorModifier = new Color(0.0f, 0.0f, 0.0f, 0.4f);
		public const float LinkHeight = 16.0f;
		public const float LinkViewTitleBarHeight = 18.0f;
		public const bool ForceProSkin = false;


		public void InitGuiStyle()
		{
			GUISkin editorSkin = null;
			if (EditorGUIUtility.isProSkin || ForceProSkin)
				editorSkin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene);
			else
				editorSkin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
			
			LinkViewTitleStyle = new GUIStyle(editorSkin.GetStyle("ProjectBrowserTopBarBg"));
			LinkViewTitleStyle.name = "Link View Title";
			LinkViewTitleStyle.alignment = TextAnchor.MiddleLeft;
			LinkViewTitleStyle.clipping = TextClipping.Clip;
			LinkViewTitleStyle.padding.top = 0;
			LinkViewTitleStyle.padding.bottom = 0;
			LinkViewTitleStyle.padding.left = 32;
			LinkViewTitleStyle.padding.right = 32;
			LinkViewTitleStyle.contentOffset = new Vector2(0.0f, -1.0f);
			LinkViewTitleStyle.font = null;
			LinkViewTitleStyle.fontSize = 0;
			LinkViewTitleStyle.imagePosition = ImagePosition.TextOnly;
			LinkViewTitleStyle.normal.textColor = LinkTextColors[0];

			LinkLabelStyle = new GUIStyle(editorSkin.GetStyle("PR Label"));
			LinkLabelStyle.name = "Link Label Style";
			LinkLabelStyle.padding.left = 8;
			
			FoldoutStyle = new GUIStyle(editorSkin.GetStyle("Foldout"));

			DragDropInsertionStyle = new GUIStyle(editorSkin.GetStyle("PR Insertion"));
			DragDropInsertionStyle.imagePosition = ImagePosition.ImageOnly;
			DragDropInsertionStyle.contentOffset = new Vector2(0.0f, -16.0f);
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
					new Color(0.7f, 0.7f, 0.7f, 1.0f),		//normal
					new Color(0.84f, 0.6f, 0.92f, 1.0f),	//model
					new Color(0.298f, 0.5f, 0.85f, 1.0f),	//prefab
					new Color(0.7f, 0.4f, 0.4f, 1.0f)		//broken prefab
				};
			}
			else
			{
				LinkTextColors = new Color[]
				{
					Color.black,							//normal
					new Color(0.6f, 0.0f, 0.8f, 1.0f),		//model
					new Color(0.0f, 0.3f, 0.6f, 1.0f),		//prefab
					new Color(0.4f, 0.0f, 0.0f, 1.0f)		//broken prefab
				};											
			}

			SelectedLinkTextColors = new Color[]
			{
				Color.white,								//normal
				new Color(0.92f, 0.8f, 1.0f, 1.0f),			//model
				new Color(0.7f, 0.75f, 1.0f, 1.0f),			//prefab
				new Color(1.0f, 0.7f, 0.7f, 1.0f)			//broken prefab
			};												
		}
	}
}
