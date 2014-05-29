using UnityEditor;
using UnityEngine;
using System.Collections;


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

			IconSize = new Vector2(16.0f, 16.0f);
			ColorViolet = new Color(0.6f, 0.27f, 0.67f, 1.0f);
		}

		public void DestroyInstance()
		{
			Object.DestroyImmediate(IconBackground);
		}
		#endregion


		public Texture2D IconPrefabNormal { get; private set; }
		public Texture2D IconPrefabModel { get; private set; }
		public Texture2D IconGameObject { get; private set; }
		public Texture2D IconBackground { get; private set; }

		public GUIStyle LinkLabelStyle { get; private set; }

		public Vector2 IconSize { get; private set; }
		public Color ColorViolet { get; private set; }


		public void InitGuiStyle()
		{
			LinkLabelStyle = new GUIStyle(GUI.skin.label);
			LinkLabelStyle.padding.left = 17;
		}
	}
}
