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
			IconPrefabNormal = EditorGUIUtility.FindTexture("PrefabNormal Icon");
			IconPrefabModel = EditorGUIUtility.FindTexture("PrefabModel Icon");
			IconGameObject = EditorGUIUtility.FindTexture("GameObject Icon");

			IconBackground = new Texture2D(4, 4, TextureFormat.RGB24, false);
			Color[] whiteTex = new Color[16];
			for (int i = 0; i < 16; i++)
				whiteTex[i] = Color.white;
			IconBackground.SetPixels(whiteTex);
			IconBackground.Apply();

			ColorViolet = new Color(0.6f, 0.27f, 0.67f, 1.0f);
		}

		public void Cleanup()
		{
			Object.DestroyImmediate(IconBackground);
		}
		#endregion

		//***** IMAGES *****

		public Texture2D IconPrefabNormal { get; private set; }
		public Texture2D IconPrefabModel { get; private set; }
		public Texture2D IconGameObject { get; private set; }
		public Texture2D IconBackground { get; private set; }

		//***** GUI STYLES *****

		public GUIStyle LinkLabelStyle { get; private set; }

		//***** CONSTANTS *****

		public readonly Color ColorViolet = new Color(0.6f, 0.27f, 0.67f, 1.0f);
		public const float LinkHeight = 19.0f;


		public void InitGuiStyle()
		{
			LinkLabelStyle = new GUIStyle(GUI.skin.label);
			LinkLabelStyle.padding.left = 17;
		}
	}
}
