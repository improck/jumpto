using UnityEditor;
using UnityEngine;
using System.Collections.Generic;


namespace JumpTo
{
	public class GraphicAssets
	{
		#region Singleton
		private static GraphicAssets s_Instance = null;

		public static GraphicAssets Instance { get { if (s_Instance == null) { s_Instance = new GraphicAssets(); } return s_Instance; } }


		private GraphicAssets()
		{
			InitAssets();
		}
		#endregion

		//***** IMAGES *****

		public Texture2D IconPrefabNormal { get; private set; }
		public Texture2D IconPrefabModel { get; private set; }
		public Texture2D IconGameObject { get; private set; }
		
		//***** GUI STYLES *****

		public GUIStyle LinkViewTitleStyle { get; private set; }
		public GUIStyle LinkLabelStyle { get; private set; }
		public GUIStyle LinkBoxStyle { get; private set; }
		public GUIStyle ToolbarStyle { get; private set; }
		public GUIStyle ToolbarButtonStyle { get; private set; }
		public GUIStyle ToolbarPopupStyle { get; private set; }
		public GUIStyle DragDropInsertionStyle { get; private set; }
		public GUIStyle DividerStyle { get; private set; }

		//***** COLORS *****

		public Color[] LinkTextColors { get; private set; }
		public Color[] SelectedLinkTextColors { get; private set; }

		//***** CONSTANTS *****

		public readonly Color DisabledColorModifier = new Color(0.0f, 0.0f, 0.0f, 0.4f);
		public const float LinkHeight = 16.0f;


		private Texture2D m_Outline;


		public void InitGuiStyle()
		{
			GUISkin editorSkin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);

			LinkViewTitleStyle = new GUIStyle(editorSkin.GetStyle("IN BigTitle"));
			LinkViewTitleStyle.name = "JumpTo Title";
			LinkViewTitleStyle.alignment = TextAnchor.MiddleLeft;

			LinkLabelStyle = new GUIStyle(editorSkin.GetStyle("PR Label"));
			LinkLabelStyle.name = "Link Label Style";
			LinkLabelStyle.padding.left = 8;

			LinkBoxStyle = new GUIStyle(GUI.skin.box);
			LinkBoxStyle.fontStyle = FontStyle.Bold;
			LinkBoxStyle.alignment = TextAnchor.UpperLeft;
			LinkBoxStyle.normal.background = m_Outline;
			Vector2 contentOffset = LinkBoxStyle.contentOffset;
			contentOffset.x = 2.0f;
			LinkBoxStyle.contentOffset = contentOffset;

			ToolbarStyle = new GUIStyle(editorSkin.GetStyle("Toolbar"));
			ToolbarButtonStyle = new GUIStyle(editorSkin.GetStyle("toolbarbutton"));
			ToolbarPopupStyle = new GUIStyle(editorSkin.GetStyle("ToolbarPopup"));
			DragDropInsertionStyle = new GUIStyle(editorSkin.GetStyle("PR Insertion"));
			DragDropInsertionStyle.imagePosition = ImagePosition.ImageOnly;
			DragDropInsertionStyle.contentOffset = new Vector2(0.0f, -16.0f);

			DividerStyle = new GUIStyle();
			DividerStyle.name = "JumpTo Divider";
			DividerStyle.normal.background = ToolbarStyle.normal.background;
			DividerStyle.border = new RectOffset(0, 0, 2, 2);
		}

		public void Cleanup()
		{
			Object.DestroyImmediate(m_Outline);
			s_Instance = null;
		}

		private void InitAssets()
		{
			IconPrefabNormal = EditorGUIUtility.FindTexture("PrefabNormal Icon");
			IconPrefabModel = EditorGUIUtility.FindTexture("PrefabModel Icon");
			IconGameObject = EditorGUIUtility.FindTexture("GameObject Icon");
			
			//TODO: load this from an embedded image
			m_Outline = new Texture2D(32, 32, TextureFormat.RGBA32, false);
			Color[] outline = new Color[32 * 32];
			for (int i = 0; i < 32; i++)
			{
				outline[i] = Color.black;
				outline[i * 32] = Color.black;
				outline[i * 32 + 31] = Color.black;
				outline[32 * 31 + i] = Color.black;
			}
			m_Outline.SetPixels(outline);
			m_Outline.Apply();
			m_Outline.hideFlags = HideFlags.HideAndDontSave;

			if (EditorGUIUtility.isProSkin)
			{
				LinkTextColors = new Color[]
				{
					new Color(0.7f, 0.7f, 0.7f, 1.0f),		//normal
					new Color(0.84f, 0.6f, 0.92f, 1.0f),	//model
					new Color(0.3f, 0.5f, 0.85f, 1.0f),		//prefab
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
