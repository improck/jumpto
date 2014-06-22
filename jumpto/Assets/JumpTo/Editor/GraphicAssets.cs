﻿using UnityEditor;
using UnityEngine;


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

			ColorViolet = new Color(0.6f, 0.27f, 0.67f, 1.0f);
		}
		#endregion

		//***** IMAGES *****

		public Texture2D IconPrefabNormal { get; private set; }
		public Texture2D IconPrefabModel { get; private set; }
		public Texture2D IconGameObject { get; private set; }

		//***** GUI STYLES *****

		public GUIStyle LinkLabelStyle { get; private set; }
		public GUIStyle LinkBoxStyle { get; private set; }

		//***** CONSTANTS *****

		public readonly Color ColorViolet = new Color(0.6f, 0.27f, 0.67f, 1.0f);
		public const float LinkHeight = 19.0f;


		private Texture2D m_Outline;


		public void InitGuiStyle()
		{
			GUISkin editorSkin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
			GUIStyle hierarchyLabelStyle = editorSkin.GetStyle("Hi Label");
			LinkLabelStyle = new GUIStyle(hierarchyLabelStyle);
			LinkLabelStyle.name = "Link Label Style";
			LinkLabelStyle.padding.left = 8;

			LinkBoxStyle = new GUIStyle(GUI.skin.box);
			LinkBoxStyle.fontStyle = FontStyle.Bold;
			LinkBoxStyle.normal.background = m_Outline;
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

			//see ctor for definition of ColorViolet
		}
	}
}
