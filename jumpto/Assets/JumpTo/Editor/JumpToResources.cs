using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Resources;


namespace JumpTo
{
	public static class ResId
	{
		public static readonly int LabelProjectLinks = "label_projectLinks".GetHashCode();
		public static readonly int LabelHierarchyLinks = "label_hierarchyLinks".GetHashCode();

		public static readonly int TooltipProjectFirst = "tooltip_projectFirst".GetHashCode();
		public static readonly int TooltipHierarchyFirst = "tooltip_hierarchyFirst".GetHashCode();
		public static readonly int TooltipVertical = "tooltip_vertical".GetHashCode();
		public static readonly int TooltipHorizontal = "tooltip_horizontal".GetHashCode();

		public static readonly int MenuProjectView = "menu_projectView".GetHashCode();
		public static readonly int MenuHierarchyView = "menu_hierarchyView".GetHashCode();
		public static readonly int MenuBothView = "menu_bothView".GetHashCode();
		public static readonly int MenuContextPingLink = "menu_contextPingLink".GetHashCode();
		public static readonly int MenuContextSetAsSelection = "menu_contextSetAsSelection".GetHashCode();
		public static readonly int MenuContextAddToSelection = "menu_contextAddToSelection".GetHashCode();
		public static readonly int MenuContextFrameLink = "menu_contextFrameLink".GetHashCode();
		public static readonly int MenuContextOpenLink = "menu_contextOpenLink".GetHashCode();
		public static readonly int MenuContextRemoveLink = "menu_contextRemoveLink".GetHashCode();
	}

	public class ResLoad
	{
		#region Singleton
		private static ResLoad s_Instance = null;

		public static ResLoad Instance { get { if (s_Instance == null) { s_Instance = new ResLoad(); } return s_Instance; } }


		private ResLoad()
		{
			LoadResources();
		}

		public void Cleanup()
		{
		}
		#endregion


		public string GetText(int textId)
		{
			return m_TextResources[textId];
		}

		public Texture2D GetImage(int imageId)
		{
			return m_ImageResources[imageId];
		}


		private Dictionary<int, string> m_TextResources = new Dictionary<int, string>();
		private Dictionary<int, Texture2D> m_ImageResources = new Dictionary<int, Texture2D>();


		private void LoadResources()
		{
			//text resources
			switch (Application.systemLanguage)
			{
			case SystemLanguage.English:
				LoadText("jumptolang_en.txt");
				break;
			}

			//TODO: load images
		}

		private void LoadText(string fileName)
		{
			//ResourceManager rm = new ResourceManager(fileName, this.GetType().Assembly);

			//ResourceReader res = new ResourceReader(this.GetType().Assembly.GetManifestResourceStream("JumpTo." + fileName));
			//IDictionaryEnumerator textEntry = res.GetEnumerator();

			//m_TextResources.Clear();
			//while (textEntry.MoveNext())
			//{
			//	m_TextResources.Add(textEntry.Key.GetHashCode(), (string)textEntry.Value);
			//}

			string[] resNames = this.GetType().Assembly.GetManifestResourceNames();
			if (resNames == null || resNames.Length == 0)
			{
				LoadDefaultText();
			}
			else
			{
				for (int i = 0; i < resNames.Length; i++)
				{
					Debug.Log(resNames[i]);
				}
			}
		}

		private void LoadDefaultText()
		{
			m_TextResources.Add(ResId.LabelProjectLinks, "Project Links");
			m_TextResources.Add(ResId.LabelHierarchyLinks, "Hierarchy Links");

			m_TextResources.Add(ResId.TooltipProjectFirst, "Project First");
			m_TextResources.Add(ResId.TooltipHierarchyFirst, "Hierarchy First");
			m_TextResources.Add(ResId.TooltipVertical, "Vertical Orientation");
			m_TextResources.Add(ResId.TooltipHorizontal, "Horizontal Orientation");

			m_TextResources.Add(ResId.MenuProjectView, "Project");
			m_TextResources.Add(ResId.MenuHierarchyView, "Hierarchy");
			m_TextResources.Add(ResId.MenuBothView, "Both");
			m_TextResources.Add(ResId.MenuContextPingLink, "Ping Link");
			m_TextResources.Add(ResId.MenuContextSetAsSelection, "Set Link as Selection");
			m_TextResources.Add(ResId.MenuContextAddToSelection, "Add Link to Selection");
			m_TextResources.Add(ResId.MenuContextFrameLink, "Frame Link in Scene");
			m_TextResources.Add(ResId.MenuContextOpenLink, "Open Asset");
			m_TextResources.Add(ResId.MenuContextRemoveLink, "Remove Link");
		}
	}
}
