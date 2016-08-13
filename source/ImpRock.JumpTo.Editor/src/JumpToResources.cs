using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Resources;


namespace ImpRock.JumpTo.Editor
{
	internal static class ResId
	{
		public static readonly int LabelProjectLinks = "label_projectLinks".GetHashCode();
		public static readonly int LabelHierarchyLinks = "label_hierarchyLinks".GetHashCode();

		//public static readonly int TooltipProjectFirst = "tooltip_projectFirst".GetHashCode();
		//public static readonly int TooltipHierarchyFirst = "tooltip_hierarchyFirst".GetHashCode();
		//public static readonly int TooltipVertical = "tooltip_vertical".GetHashCode();
		//public static readonly int TooltipHorizontal = "tooltip_horizontal".GetHashCode();

		//public static readonly int MenuProjectView = "menu_projectView".GetHashCode();
		//public static readonly int MenuHierarchyView = "menu_hierarchyView".GetHashCode();
		//public static readonly int MenuBothView = "menu_bothView".GetHashCode();

		public static readonly int MenuContextPingLink = "menu_contextPingLink".GetHashCode();
		public static readonly int MenuContextSetAsSelection = "menu_contextSetAsSelection".GetHashCode();
		public static readonly int MenuContextAddToSelection = "menu_contextAddToSelection".GetHashCode();
		public static readonly int MenuContextFrameLink = "menu_contextFrameLink".GetHashCode();
		public static readonly int MenuContextOpenLink = "menu_contextOpenLink".GetHashCode();
		public static readonly int MenuContextRemoveLink = "menu_contextRemoveLink".GetHashCode();
		public static readonly int MenuContextRemoveAll = "menu_contextRemoveAll".GetHashCode();
		
		public static readonly int MenuContextSetAsSelectionPlural = "menu_contextSetAsSelectionPlural".GetHashCode();
		public static readonly int MenuContextAddToSelectionPlural = "menu_contextAddToSelectionPlural".GetHashCode();
		public static readonly int MenuContextFrameLinkPlural = "menu_contextFrameLinkPlural".GetHashCode();
		public static readonly int MenuContextOpenLinkPlural = "menu_contextOpenLinkPlural".GetHashCode();
		public static readonly int MenuContextRemoveLinkPlural = "menu_contextRemoveLinkPlural".GetHashCode();
		public static readonly int MenuContextSaveLinks = "menu_contextSaveLinks".GetHashCode();

		public static readonly int DialogRemoveAllTitle = "dialog_removeAllTitle".GetHashCode();
		public static readonly int DialogRemoveAllMessage = "dialog_removeAllMessage".GetHashCode();
		public static readonly int DialogYes = "dialog_yes".GetHashCode();
		public static readonly int DialogNo = "dialog_no".GetHashCode();

		//public static readonly int ImageDividerVertical;
		//public static readonly int ImageHorizontalView;
		//public static readonly int ImageVerticalView;
		public static readonly int ImageHamburger;
		public static readonly int ImageTabIcon = "tabicon.png".GetHashCode();

		static ResId()
		{
			if (EditorGUIUtility.isProSkin)
			{
				//ImageDividerVertical = "divider_v_pro.png".GetHashCode();
				//ImageHorizontalView = "horizontal_pro.png".GetHashCode();
				//ImageVerticalView = "vertical_pro.png".GetHashCode();
				ImageHamburger = "hamburger_pro.png".GetHashCode();
			}
			else
			{
				//ImageDividerVertical = "divider_v.png".GetHashCode();
				//ImageHorizontalView = "horizontal.png".GetHashCode();
				//ImageVerticalView = "vertical.png".GetHashCode();
				ImageHamburger = "hamburger.png".GetHashCode();
			}
		}
	}

	internal sealed class JumpToResources
	{
		#region Singleton
		private static JumpToResources s_Instance = null;

		public static JumpToResources Instance { get { if (s_Instance == null) { s_Instance = new JumpToResources(); } return s_Instance; } }


		private JumpToResources() { }

		public static void DestroyInstance()
		{
			if (s_Instance != null)
			{
				s_Instance.CleanUp();
				s_Instance = null;
			}
		}
		#endregion


		private Dictionary<int, string> m_TextResources = new Dictionary<int, string>();
		private Dictionary<int, Texture2D> m_ImageResources = new Dictionary<int, Texture2D>();


		public string GetText(int textId)
		{
			return m_TextResources[textId];
		}

		public Texture2D GetImage(int imageId)
		{
			return m_ImageResources[imageId];
		}

		public void LoadResources()
		{
			//text resources
			//LoadDefaultText();
			switch (Application.systemLanguage)
			{
			case SystemLanguage.English:
				LoadText("jumptolang_en.txt");
				break;
			default:
				LoadText("jumptolang_en.txt");
				break;
			}

			//image resources
			LoadImage("tabicon.png");

			if (EditorGUIUtility.isProSkin)
			{
				//pro skin
				LoadImage("divider_v_pro.png");
				LoadImage("horizontal_pro.png");
				LoadImage("vertical_pro.png");
				LoadImage("hamburger_pro.png");
			}
			else
			{
				//free skin
				LoadImage("divider_v.png");
				LoadImage("horizontal.png");
				LoadImage("vertical.png");
				LoadImage("hamburger.png");
			}
		}

		private void LoadText(string fileName)
		{
			using (Stream resStream = this.GetType().Assembly.GetManifestResourceStream("ImpRock.JumpTo.Editor.res.lang." + fileName))
			{
				using (StreamReader reader = new StreamReader(resStream))
				{
					int idHash = 0;
					int equalsPos = 0;
					const string equals = " = ";
					string line = string.Empty;
					string id = string.Empty;
					string text = string.Empty;

					while (!reader.EndOfStream)
					{
						line = reader.ReadLine();
						if (line == null || line.Length == 0 || line[0] == ';')
							continue;

						equalsPos = line.IndexOf(equals);
						id = line.Substring(0, equalsPos);
						text = line.Substring(equalsPos + 3);

						idHash = id.GetHashCode();
						
						if (m_TextResources.ContainsKey(idHash))
						{
							m_TextResources[idHash] = text;
						}
						else
						{
							m_TextResources.Add(idHash, text);
						}
					}
				}
			}
		}

		private void LoadImage(string fileName)
		{
			int fileNameHash = fileName.GetHashCode();
			if (!m_ImageResources.ContainsKey(fileNameHash))
			{
				using (Stream resStream = this.GetType().Assembly.GetManifestResourceStream("ImpRock.JumpTo.Editor.res.image." + fileName))
				{
					byte[] fileBytes = new byte[resStream.Length];
					resStream.Read(fileBytes, 0, fileBytes.Length);

					Texture2D texture = new Texture2D(4, 4, TextureFormat.ARGB32, false);
					texture.LoadImage(fileBytes);
					texture.hideFlags = HideFlags.HideAndDontSave;
					texture.wrapMode = TextureWrapMode.Clamp;
					texture.anisoLevel = 1;
					texture.Apply();

					m_ImageResources.Add(fileNameHash, texture);
				}
			}
		}

		//private void LoadDefaultText()
		//{
		//	m_TextResources.Add(ResId.LabelProjectLinks, "Project Links");
		//	m_TextResources.Add(ResId.LabelHierarchyLinks, "Hierarchy Links");

		//	m_TextResources.Add(ResId.TooltipProjectFirst, "Project First");
		//	m_TextResources.Add(ResId.TooltipHierarchyFirst, "Hierarchy First");
		//	m_TextResources.Add(ResId.TooltipVertical, "Vertical Orientation");
		//	m_TextResources.Add(ResId.TooltipHorizontal, "Horizontal Orientation");

		//	m_TextResources.Add(ResId.MenuProjectView, "Project");
		//	m_TextResources.Add(ResId.MenuHierarchyView, "Hierarchy");
		//	m_TextResources.Add(ResId.MenuBothView, "Both");
		//	m_TextResources.Add(ResId.MenuContextPingLink, "Ping");
		//	m_TextResources.Add(ResId.MenuContextSetAsSelection, "Set This as Selection");
		//	m_TextResources.Add(ResId.MenuContextAddToSelection, "Add This to Selection");
		//	m_TextResources.Add(ResId.MenuContextFrameLink, "Frame This in Scene");
		//	m_TextResources.Add(ResId.MenuContextOpenLink, "Open This");
		//	m_TextResources.Add(ResId.MenuContextRemoveLink, "Remove This");
		//	m_TextResources.Add(ResId.MenuContextRemoveAll, "Remove All");
		//	m_TextResources.Add(ResId.MenuContextSetAsSelectionPlural, "Set These as Selection");
		//	m_TextResources.Add(ResId.MenuContextAddToSelectionPlural, "Add These to Selection");
		//	m_TextResources.Add(ResId.MenuContextFrameLinkPlural, "Frame These in Scene");
		//	m_TextResources.Add(ResId.MenuContextOpenLinkPlural, "Open These");
		//	m_TextResources.Add(ResId.MenuContextRemoveLinkPlural, "Remove These");
		//	m_TextResources.Add(ResId.MenuContextSaveLinks, "Save Links");

		//	m_TextResources.Add(ResId.DialogRemoveAllTitle, "Confirm Remove All");
		//	m_TextResources.Add(ResId.DialogRemoveAllMessage, "Are you sure you want to remove all links from the list?");
		//	m_TextResources.Add(ResId.DialogYes, "Yes");
		//	m_TextResources.Add(ResId.DialogNo, "No");
		//}

		private void CleanUp()
		{
			//unload images
			foreach (KeyValuePair<int, Texture2D> image in m_ImageResources)
			{
				Object.DestroyImmediate(image.Value);
			}
		}
	}
}
