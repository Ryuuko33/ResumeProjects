#if UNITY_EDITOR
using UnityEditor;

using UnityEngine;

namespace Psychoflow.SSWaterReflection2D {
	//[CreateAssetMenu(menuName = "ScriptableObjects/Create New SSWR2DPrefs")]
	/// <summary>
	/// The preferences of SSWaterReflection2D for UnityEditor only.
	/// </summary>
    public class SSWR2DPrefs : ScriptableObject {
		#region Singleton
		private static SSWR2DPrefs s_Instance;
		public static SSWR2DPrefs Instance {
			get {
				if (s_Instance) {
					return s_Instance;
				}
				s_Instance = FindInstanceInAssetDatabase();
				return s_Instance;
			}
		}

		private static SSWR2DPrefs FindInstanceInAssetDatabase() {
			var ids = AssetDatabase.FindAssets("SSWR2DPrefs t:SSWR2DPrefs");
			if (ids.Length == 1) {
				var prefsInstance = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(ids[0]));

				return (SSWR2DPrefs)prefsInstance;
			} else {
				Debug.LogError("Couldn't find a SSWR2DPrefs! Please reimport package!");
				return null;
			}
		}
		#endregion

		[Tooltip("Show reflection reference line which color is RED.")]
		public bool gizmoShowReflectionLine = true;
		[Tooltip("Show foam edge threshold line which color is YELLOW.")]
		public bool gizmoShowFoamEdgeThreshold = true;
		[Tooltip("Show perspective correction length as a line which color is GREEN.")]
		public bool gizmoShowPerspectiveMaxLength = true;
	}
}
#endif