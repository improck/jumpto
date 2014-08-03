using UnityEngine;


namespace JumpTo
{
	public static class JumpToUtility
	{
		public static string GetTransformPath(this Transform transform)
		{
			string path = string.Empty;
			while (transform != null)
			{
				path = "/" + transform.name + path;
				transform = transform.parent;
			}

			return path;
		}
	}
}
