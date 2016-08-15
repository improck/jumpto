using UnityEditor;
using UnityEngine;
using System.Collections.Generic;


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
		public GUIStyle ToolbarStyle { get; private set; }
		public GUIStyle ToolbarButtonStyle { get; private set; }
		public GUIStyle ToolbarPopupStyle { get; private set; }
		public GUIStyle DragDropInsertionStyle { get; private set; }
		//public GUIStyle DividerHorizontalStyle { get; private set; }
		//public GUIStyle DividerVerticalStyle { get; private set; }

		//***** COLORS *****

		public Color[] LinkTextColors { get; private set; }
		public Color[] SelectedLinkTextColors { get; private set; }

		//***** CONSTANTS *****

		public readonly Color DisabledColorModifier = new Color(0.0f, 0.0f, 0.0f, 0.4f);
		public const float LinkHeight = 16.0f;
		public const float LinkViewTitleBarHeight = 20.0f;


		public void InitGuiStyle()
		{
			GUISkin editorSkin = null;
			if (EditorGUIUtility.isProSkin)
				editorSkin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene);
			else
				editorSkin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);

			LinkViewTitleStyle = new GUIStyle(editorSkin.GetStyle("IN BigTitle"));
			LinkViewTitleStyle.name = "JumpTo Title";
			LinkViewTitleStyle.alignment = TextAnchor.MiddleLeft;

			LinkLabelStyle = new GUIStyle(editorSkin.GetStyle("PR Label"));
			LinkLabelStyle.name = "Link Label Style";
			LinkLabelStyle.padding.left = 8;

			ToolbarStyle = new GUIStyle(editorSkin.GetStyle("Toolbar"));
			ToolbarPopupStyle = new GUIStyle(editorSkin.GetStyle("ToolbarPopup"));
			ToolbarButtonStyle = new GUIStyle(editorSkin.GetStyle("toolbarbutton"));

			DragDropInsertionStyle = new GUIStyle(editorSkin.GetStyle("PR Insertion"));
			DragDropInsertionStyle.imagePosition = ImagePosition.ImageOnly;
			DragDropInsertionStyle.contentOffset = new Vector2(0.0f, -16.0f);

			//DividerHorizontalStyle = new GUIStyle();
			//DividerHorizontalStyle.name = "JumpTo Divider H";
			//DividerHorizontalStyle.normal.background = ToolbarStyle.normal.background;
			//DividerHorizontalStyle.border = new RectOffset(0, 0, 2, 2);

			//DividerVerticalStyle = new GUIStyle();
			//DividerVerticalStyle.name = "JumpTo Divider V";
			//DividerVerticalStyle.normal.background = JumpToResources.Instance.GetImage(ResId.ImageDividerVertical);
			//DividerVerticalStyle.border = new RectOffset(0, 0, 2, 2);
		}

		public void InitAssets()
		{
			IconPrefabNormal = EditorGUIUtility.FindTexture("PrefabNormal Icon");
			IconPrefabModel = EditorGUIUtility.FindTexture("PrefabModel Icon");
			IconGameObject = EditorGUIUtility.FindTexture("GameObject Icon");
			IconProjectView = EditorGUIUtility.FindTexture("Project");
			IconHierarchyView = EditorGUIUtility.FindTexture("UnityEditor.HierarchyWindow");
			
			if (EditorGUIUtility.isProSkin)
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
