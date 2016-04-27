using UnityEditor;
using UnityEngine;
using System.Collections.Generic;


namespace JumpTo
{
	internal abstract class JumpLinkContainer<T> : ScriptableObject where T : JumpLink
	{
		[SerializeField] protected List<T> m_Links = new List<T>();
		[SerializeField] protected int m_ActiveSelection = -1;


		public List<T> Links { get { return m_Links; } }
		public int ActiveSelection { get { return m_ActiveSelection; } set { m_ActiveSelection = Mathf.Clamp(value, -1, m_Links.Count - 1); } }
		public T ActiveSelectedObject { get { return m_ActiveSelection > -1 ? m_Links[m_ActiveSelection] : null; } }

		public T this[int index]
		{
			get
			{
				if (index < 0 || index >= m_Links.Count)
					throw new System.IndexOutOfRangeException(typeof(T) + ": Link index out of range (" + index + "/" + m_Links.Count + ")");

				return m_Links[index];
			}
		}

		public bool HasSelection
		{
			get
			{
				for (int i = 0; i < m_Links.Count; i++)
				{
					if (m_Links[i].Selected)
						return true;
				}

				return false;
			}
		}

		public int SelectionCount
		{
			get
			{
				int count = 0;
				for (int i = 0; i < m_Links.Count; i++)
				{
					if (m_Links[i].Selected)
						count++;
				}

				return count;
			}
		}

		public T[] SelectedLinks
		{
			get
			{
				if (m_Links.Count == 0)
					return null;
				else
				{
					List<T> selection = new List<T>();
					for (int i = 0; i < m_Links.Count; i++)
					{
						if (m_Links[i].Selected)
							selection.Add(m_Links[i]);
					}

					return selection.ToArray();
				}
			}
		}

		public Object[] SelectedLinkReferences
		{
			get
			{
				if (m_Links.Count == 0)
					return null;
				else
				{
					List<Object> selection = new List<Object>();
					for (int i = 0; i < m_Links.Count; i++)
					{
						if (m_Links[i].Selected)
							selection.Add(m_Links[i].LinkReference);
					}

					return selection.ToArray();
				}
			}
		}

		public Object[] AllLinkReferences
		{
			get
			{
				if (m_Links.Count == 0)
					return null;
				else
				{
					Object[] linkRefs = new Object[m_Links.Count];
					for (int i = 0; i < m_Links.Count; i++)
					{
						linkRefs[i] = m_Links[i].LinkReference;
					}

					return linkRefs;
				}
			}
		}


		public event System.Action OnLinksChanged;


		public abstract void AddLink(UnityEngine.Object linkReference, PrefabType prefabType);
		protected abstract void UpdateLinkInfo(T link, PrefabType prefabType);


		public void RemoveLink(int index)
		{
			if (index < 0 || index > m_Links.Count - 1)
				return;

			Object.DestroyImmediate(m_Links[index]);
			m_Links.RemoveAt(index);

			for (int i = index; i < m_Links.Count; i++)
			{
				m_Links[i].Area.y = i * GraphicAssets.LinkHeight;
			}

			if (OnLinksChanged != null)
				OnLinksChanged();
		}

		public void RemoveSelected()
		{
			for (int i = m_Links.Count - 1; i >= 0; i--)
			{
				if (m_Links[i].Selected)
				{
					Object.DestroyImmediate(m_Links[i]);
					m_Links.RemoveAt(i);
				}
			}

			m_ActiveSelection = -1;

			RefreshLinksY();

			if (OnLinksChanged != null)
				OnLinksChanged();
		}

		public void RemoveNonSelected()
		{
			T active = ActiveSelectedObject;

			for (int i = m_Links.Count - 1; i >= 0; i--)
			{
				if (!m_Links[i].Selected)
				{
					Object.DestroyImmediate(m_Links[i]);
					m_Links.RemoveAt(i);
				}
			}

			m_ActiveSelection = m_Links.IndexOf(active);

			RefreshLinksY();

			if (OnLinksChanged != null)
				OnLinksChanged();
		}

		public void RemoveAll()
		{
			m_Links.Clear();
			m_ActiveSelection = -1;

			if (OnLinksChanged != null)
				OnLinksChanged();
		}

		public void MoveLink(int from, int to)
		{
			//can't move a link to itself
			if (from == to)
				return;

			//get the link, then remove it from the list
			T link = m_Links[from];
			m_Links.RemoveAt(from);

			//find the range of links that will shift
			//	within the list as a result of the move
			int min = 0;
			int max = 0;
			if (to > from)
			{
				to--;

				min = from;
				max = to + 1;
			}
			else
			{
				min = to;
				max = from + 1;
			}

			//re-insert the link
			m_Links.Insert(to, link);

			//adjust the y-positions of the affected links
			//	instead of all of the links
			for (; min < max; min++)
			{
				m_Links[min].Area.y = min * GraphicAssets.LinkHeight;
			}

			if (OnLinksChanged != null)
				OnLinksChanged();
		}

		public void MoveSelected(int to)
		{
			//get the indices of the selected links
			List<int> selection = new List<int>();
			for (int i = 0; i < m_Links.Count; i++)
			{
				if (m_Links[i].Selected)
				{
					//early return if trying to move the
					//	selection into itself
					if (i == to)
						return;

					selection.Add(i);
				}
			}

			//grab an array of the selected object before removal
			T[] selectionObjects = SelectedLinks;

			//remove the selected links from the list
			int toAdjusted = to;
			for (int i = selection.Count - 1; i >= 0; i--)
			{
				m_Links.RemoveAt(selection[i]);

				//modify the insertion index for selected links
				//	above it
				if (selection[i] < to)
					toAdjusted--;
			}

			//re-insert the selection
			m_Links.InsertRange(toAdjusted, selectionObjects);

			//fix all of the y-positions
			RefreshLinksY();

			if (OnLinksChanged != null)
				OnLinksChanged();
		}

		public void LinkSelectionSet(int index)
		{
			for (int i = 0; i < m_Links.Count; i++)
			{
				m_Links[i].Selected = i == index;
			}

			m_ActiveSelection = Mathf.Clamp(index, -1, m_Links.Count - 1);
		}

		public void LinkSelectionSetRange(int from, int to)
		{
			if (from > to)
			{
				int temp = to;
				to = from;
				from = temp;
			}

			for (int i = 0; i < from; i++)
			{
				m_Links[i].Selected = false;
			}

			for (int i = from; i <= to; i++)
			{
				m_Links[i].Selected = true;
			}

			for (int i = to + 1; i < m_Links.Count; i++)
			{
				m_Links[i].Selected = false;
			}
		}

		public void LinkSelectionInvert()
		{
			for (int i = 0; i < m_Links.Count; i++)
			{
				m_Links[i].Selected = !m_Links[i].Selected;
			}
		}

		public void LinkSelectionAdd(int index)
		{
			if (index >= 0 && index < m_Links.Count)
			{
				m_Links[index].Selected = true;
				m_ActiveSelection = index;
			}
		}

		public void LinkSelectionRemove(int index)
		{
			if (index >= 0 && index < m_Links.Count)
			{
				m_Links[index].Selected = false;
				m_ActiveSelection = index;
			}
		}

		public void LinkSelectionClear()
		{
			for (int i = 0; i < m_Links.Count; i++)
			{
				m_Links[i].Selected = false;
			}

			m_ActiveSelection = -1;
		}

		public void RefreshLinks()
		{
			for (int i = m_Links.Count - 1; i >= 0; i--)
			{
				if (m_Links[i].LinkReference == null)
				{
					Object.DestroyImmediate(m_Links[i]);
					m_Links.RemoveAt(i);
				}
				else
				{
					UpdateLinkInfo(m_Links[i], PrefabUtility.GetPrefabType(m_Links[i].LinkReference));
				}
			}

			RefreshLinksY();
		}

		public void RefreshLinksY()
		{
			for (int i = 0; i < m_Links.Count; i++)
			{
				m_Links[i].Area.y = i * GraphicAssets.LinkHeight;
			}
		}

		public int LinkHitTest(Vector2 position)
		{
			int linkCount = m_Links.Count;
			if (linkCount > 0)
			{
				if (linkCount == 1)
				{
					if (m_Links[0].Area.RectInternal.Contains(position))
						return 0;
				}
				else
				{
					int indexMin = 0;
					int indexMax = linkCount - 1;
					int index = 0;
					RectRef rect = null;

					while (indexMax >= indexMin)
					{
						index = (indexMin + indexMax) >> 1;

						rect = m_Links[index].Area;

						//if below the link rect
						if (rect.yMax < position.y)
						{
							indexMin = index + 1;
						}
						//if above the link rect
						else if (rect.yMin > position.y)
						{
							indexMax = index - 1;
						}
						//if within the link rect
						else if (position.x >= rect.xMin && position.x <= rect.xMax)
						{
							return index;
						}
						//not within any link rect
						else
						{
							break;
						}
					}
				}
			}

			return -1;
		}

		public bool SelectionHitTest(Vector2 position)
		{
			for (int i = 0; i < m_Links.Count; i++)
			{
				if (m_Links[i].Selected && m_Links[i].Area.RectInternal.Contains(position))
					return true;
			}

			return false;
		}

		protected void RaiseOnLinksChanged()
		{
			if (OnLinksChanged != null)
				OnLinksChanged();
		}
	}
}
