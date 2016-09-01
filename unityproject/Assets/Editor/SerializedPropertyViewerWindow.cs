using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;


public class SerializedPropertyViewerWindow : EditorWindow
{
	[MenuItem("Window/Serialized Property Viewer")]
	public static void SerializedPropertyViewer_Menu_Window()
	{
		SerializedPropertyViewerWindow window = EditorWindow.GetWindow<SerializedPropertyViewerWindow>(true, "Serialized Properties");

		window.OnSelectionChange();

		window.ShowUtility();
	}

	public static void ShowAndInspectObject(Object reference)
	{
		SerializedPropertyViewerWindow window = EditorWindow.GetWindow<SerializedPropertyViewerWindow>(true, "Serialized Properties");

		window.PushAndInspectObject(reference);

		window.ShowUtility();
	}


	private class SerializedPropertyInfo
	{
		public string Info;
		public Object Reference;
	}


	private Object m_Inspected = null;
	private Vector2 m_ScrollViewPos = Vector2.zero;
	private bool m_Debug = true;
	private bool m_ShortenStrings = true;
	private bool m_IsPrefabInstance = false;
	private List<SerializedPropertyInfo> m_Properties = new List<SerializedPropertyInfo>();
	private List<Object> m_ObjectStack = new List<Object>();
	private PropertyInfo m_InspectorModeInfo = null;
	private int m_StackIndex = 0;

	
	private void OnEnable()
	{
		m_InspectorModeInfo = typeof(SerializedObject).GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);
	}

	private void OnGUI()
	{
		if (m_Inspected != null)
		{
			GUILayout.Label(m_Inspected.name);
			GUILayout.BeginHorizontal();
			{
				bool debug = GUILayout.Toggle(m_Debug, "Debug", GUILayout.ExpandWidth(false));
				if (debug != m_Debug)
				{
					m_Debug = debug;
					Refresh();
				}

				bool shortenStrings = GUILayout.Toggle(m_ShortenStrings, "ShortenStrings", GUILayout.ExpandWidth(false));
				if (shortenStrings != m_ShortenStrings)
				{
					m_ShortenStrings = shortenStrings;
					Refresh();
				}
			}
			GUILayout.EndHorizontal();

			if (m_IsPrefabInstance)
			{
				if (GUILayout.Button("Inspect Prefab Object", GUILayout.Width(150.0f)))
				{
					Object prefabObject = PrefabUtility.GetPrefabObject(m_Inspected);
					m_ObjectStack.Add(prefabObject);
					Inspect(prefabObject);
				}
			}

			if (m_ObjectStack.Count > 0)
			{
				GUILayout.BeginHorizontal();
				{
					if (m_StackIndex > 0)
					{
						if (GUILayout.Button("Back", GUILayout.Width(60.0f)))
						{
							m_StackIndex--;
							Inspect(m_ObjectStack[m_StackIndex]);
						}
					}
					else
					{
						GUILayout.Space(68.0f);
					}

					GUILayout.Space(10.0f);
					GUILayout.Label(m_StackIndex + "/" + (m_ObjectStack.Count - 1), GUILayout.ExpandWidth(false));
					GUILayout.Space(10.0f);

					if (m_StackIndex < m_ObjectStack.Count - 1)
					{
						if (GUILayout.Button("Forward", GUILayout.Width(60.0f)))
						{
							m_StackIndex++;
							Inspect(m_ObjectStack[m_StackIndex]);
						}
					}
				}
				GUILayout.EndHorizontal();
			}

			GUILayout.Space(10.0f);

			m_ScrollViewPos = GUILayout.BeginScrollView(m_ScrollViewPos);
			{
				for (int i = 0; i < m_Properties.Count; i++)
				{
					if (m_Properties[i].Reference == null)
					{
						GUILayout.Label(m_Properties[i].Info);
					}
					else
					{
						GUILayout.BeginHorizontal();
						{
							GUILayout.Label(m_Properties[i].Info);
							if (GUILayout.Button("Inspect", GUILayout.Width(60.0f)))
							{
								m_StackIndex++;

								if (m_ObjectStack.Count > 0 && m_StackIndex < m_ObjectStack.Count - 1)
								{
									m_ObjectStack.RemoveRange(m_StackIndex, m_ObjectStack.Count - m_StackIndex);
								}

								m_ObjectStack.Add(m_Properties[i].Reference);
								Inspect(m_Properties[i].Reference);
							}
						}
						GUILayout.EndHorizontal();
					}
				}
			}
			GUILayout.EndScrollView();
		}   //if inspected not null
		else
		{
			GUILayout.Label("Select something to inspect");
		}
	}

	private void OnSelectionChange()
	{
		m_StackIndex = 0;
		m_ObjectStack.Clear();
		
		m_ObjectStack.Add(Selection.activeObject);

		Inspect(Selection.activeObject);
	}

	private void Inspect(Object toInspect)
	{
		m_Inspected = toInspect;

		if (m_Inspected != null && m_Inspected.GetType() == typeof(GameObject))
		{
			PrefabType prefabType = PrefabUtility.GetPrefabType(m_Inspected);
			m_IsPrefabInstance = prefabType == PrefabType.ModelPrefabInstance || prefabType == PrefabType.PrefabInstance;
		}
		else
		{
			m_IsPrefabInstance = false;
		}

		Refresh();
	}

	private void PushAndInspectObject(Object toInspect)
	{
		if (toInspect == null)
			return;

		m_StackIndex++;

		if (m_ObjectStack.Count > 0 && m_StackIndex < m_ObjectStack.Count - 1)
		{
			m_ObjectStack.RemoveRange(m_StackIndex, m_ObjectStack.Count - m_StackIndex);
		}

		m_ObjectStack.Add(toInspect);
		Inspect(toInspect);
	}

	private void Refresh()
	{
		//clear list
		m_Properties.Clear();

		if (m_Inspected == null)
			return;

		SerializedObject serializedObject = new SerializedObject(m_Inspected);

		if (m_Debug)
			m_InspectorModeInfo.SetValue(serializedObject, InspectorMode.Debug, null);
		else
			m_InspectorModeInfo.SetValue(serializedObject, InspectorMode.Normal, null);

		SerializedProperty property = serializedObject.GetIterator();
		if (property != null)
		{
			SerializedPropertyInfo propertyInfo;
			while (property.Next(true))
			{
				propertyInfo = new SerializedPropertyInfo();
				propertyInfo.Reference = null;
				m_Properties.Add(propertyInfo);

				if (property.propertyType == SerializedPropertyType.Integer)
				{
					propertyInfo.Info = property.propertyPath + " (" + property.propertyType + "/" + property.intValue + ")";
				}
				else if (property.propertyType == SerializedPropertyType.String)
				{
					string stringValue = property.stringValue;
					propertyInfo.Info = property.propertyPath + " (" + property.propertyType + "/" + property.stringValue + ")";

					if (m_ShortenStrings)
					{
						//skip the "array" of characters + the size
						for (int i = 0; i <= stringValue.Length + 1; i++)
						{
							property.Next(true);
						}
					}
				}
				else if (property.propertyType == SerializedPropertyType.ObjectReference)
				{
					if (property.objectReferenceValue != null)
						propertyInfo.Info = property.propertyPath + " (" + property.propertyType + "/" + property.objectReferenceValue.GetType() + "/" + property.objectReferenceValue.name + ")";
					else
						propertyInfo.Info = property.propertyPath + " (" + property.propertyType + "/null)";

					propertyInfo.Reference = property.objectReferenceValue;
				}
				else if (property.propertyType == SerializedPropertyType.Generic)
				{
					propertyInfo.Info = property.propertyPath + " (" + property.propertyType + "/" + property.type + ")";
				}
				else if (property.propertyType == SerializedPropertyType.Boolean)
				{
					propertyInfo.Info = property.propertyPath + " (" + property.propertyType + "/" + property.boolValue + ")";
				}
				else
				{
					propertyInfo.Info = property.propertyPath + " (" + property.propertyType + ")";
				}
			}
		}

		Repaint();
	}
}
