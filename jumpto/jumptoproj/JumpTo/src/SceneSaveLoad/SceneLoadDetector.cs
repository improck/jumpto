using UnityEditor;
using UnityEngine;


namespace SceneStateDetection
{
	/// <summary>
	/// As it says, the SceneLoadDetector detects scene loads. Or, more succinctly,
	/// it detects scene unloads. Whenever a ScriptableObject is instantiated, it is
	/// associated with the scene 'root' unless its hideFlags include the DontSave flag
	/// If the object is referenced by a scene object (GameObject, MonoBehavior, etc.),
	/// then it gets serialized with the rest of the scene data. If not, it will be
	/// cleaned up as a leaked object when the scene is saved. This triggers the usual
	/// chain of destruction for a Unity.Object based instance. It is first disabled,
	/// which triggers OnDisable(), then it is destroyed, which triggers the OnDestroy().
	/// Thus, by leaving a ScriptableObject associated with the scene root but not
	/// referenced by any scene object, we have a way to generate an event when a scene
	/// is unloaded and when a new scene is loaded.
	/// </summary>
	public class SceneLoadDetector : ScriptableObject
	{
		private static SceneLoadDetector s_Instance;
		private bool m_KeepAlive = true;


		/// <summary>
		/// Call this from your editor's OnEnable(). Creating an instance of the
		/// object will associate with the scene 'root', which causes its OnDestroy()
		/// to be called when the scene is being unloaded. DO NOT instantiate this
		/// object yourself because there must always be only one.
		/// </summary>
		public static void EnsureExistence()
		{
			//wait one editor frame, then check for an existing instance
			//	EditorApplication.delayCall is not documented, but is very
			//	useful. Any methods subscribed will be called on the next
			//	'frame' of the Unity Editor. That is, on the next update.
			//	the method is unsubscribed just before being called, so it
			//	is only called once.
			EditorApplication.delayCall += WaitToCreate;
		}

		/// <summary>
		/// Call this from your editor's OnDisable(). The instance must not exist
		/// in the scene when the assemblies reload or when a scene gets saved. If
		/// the instance is not destroyed, Unity will treat it as a leaked object.
		/// Calling TemporarilyDestroyInstance() will destroy the instance immediately,
		/// which triggers the OnDestroy(), which then schedules a new instance to be
		/// instantiated during the next editor update.
		/// </summary>
		public static void TemporarilyDestroyInstance()
		{
			if (s_Instance != null)
			{
				//NOTE: as soon as OnDestroy() gets called, a new instance
				//		is scheduled to be created in the next editor frame
				DestroyImmediate(s_Instance);
				s_Instance = null;
			}
		}

		/// <summary>
		/// Call this from your editor's OnDestroy(). Immediately destroys the instance
		/// of this object, but doesn't cause a reinstantiation on the next Editor update.
		/// </summary>
		public static void PermanentlyDestroyInstance()
		{
			if (s_Instance != null)
			{
				//tell OnDestroy() not to create a new instance in the next
				// Editor frame
				s_Instance.m_KeepAlive = false;

				DestroyImmediate(s_Instance);
				s_Instance = null;
			}
		}

		/// <summary>
		/// Gets called during the next Editor update after a call to EnsureExistence().
		/// Looks for an instance of this object. If one is not found, it creates one
		/// and signals that a scene has been loaded.
		/// </summary>
		private static void WaitToCreate()
		{
			if (Resources.FindObjectsOfTypeAll<SceneLoadDetector>().Length == 0)
			{
				s_Instance = CreateInstance<SceneLoadDetector>();
				s_Instance.hideFlags = HideFlags.HideInHierarchy;

				SceneStateControl.SceneWillLoad();
			}
		}

		void OnDisable()
		{
			if (m_KeepAlive)
				SceneStateControl.SceneIsUnloading();
		}

		/// <summary>
		/// Gets called just before the object is destroyed, which happens when a scene
		/// is being unloaded. Causes a new instance to be created after the next scene
		/// loads.
		/// </summary>
		void OnDestroy()
		{
			if (m_KeepAlive)
			{
				EnsureExistence();
			}
		}
	}
}
