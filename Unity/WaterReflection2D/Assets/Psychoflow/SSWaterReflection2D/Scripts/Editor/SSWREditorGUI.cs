using System.Collections;
using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

namespace Psychoflow.SSWaterReflection2D {
	internal class SSWREditorGUI {
		private static bool s_GeneralFoldout = true;
		private static bool s_DisplacementFoldout = true;
		private static bool s_ReflectionFoldout = true;
		private static bool s_RefractionFoldout = true;
		private static bool s_FoamFoldout = true;
		private static bool s_PerspectiveCorrectionFoldout = true;
		private static bool s_DebugPropertiesFoldout = true;

		private static GUIStyle s_FoldoutHeaderStyle;
		private static GUIStyle FoldoutHeaderStyle {
			get {
				if (s_FoldoutHeaderStyle == null) {
					s_FoldoutHeaderStyle = new GUIStyle(EditorStyles.foldoutHeader);
					s_FoldoutHeaderStyle.fontSize = (int)(s_FoldoutHeaderStyle.fontSize * 1.2f);
					s_FoldoutHeaderStyle.fixedHeight = s_FoldoutHeaderStyle.lineHeight * 1.5f;
				}
				return s_FoldoutHeaderStyle;
			}
		}

		public static bool BeginGeneralFoldout() {
			s_GeneralFoldout = BeginFoldoutGroup(s_GeneralFoldout, "General");
			return s_GeneralFoldout;
		}
		public static void EndGeneralFoldout() {
			EndFoldoutGroup(s_GeneralFoldout);
		}
		public static bool BeginDisplacementFoldout() {
			s_DisplacementFoldout = BeginFoldoutGroup(s_DisplacementFoldout, "Displacement");
			return s_DisplacementFoldout;
		}
		public static void EndDisplacementFoldout() {
			EndFoldoutGroup(s_DisplacementFoldout);
		}
		public static bool BeginReflectionFoldout() {
			s_ReflectionFoldout = BeginFoldoutGroup(s_ReflectionFoldout, "Reflection");
			return s_ReflectionFoldout;
		}
		public static void EndReflectionFoldout() {
			EndFoldoutGroup(s_ReflectionFoldout);
		}
		public static bool BeginRefractionFoldout() {
			s_RefractionFoldout = BeginFoldoutGroup(s_RefractionFoldout, "Refraction");
			return s_RefractionFoldout;
		}
		public static void EndRefractionFoldout() {
			EndFoldoutGroup(s_RefractionFoldout);
		}
		public static bool BeginFoamFoldout() {
			s_FoamFoldout = BeginFoldoutGroup(s_FoamFoldout, "Foam");
			return s_FoamFoldout;
		}
		public static void EndFoamFoldout() {
			EndFoldoutGroup(s_FoamFoldout);
		}
		public static bool BeginPerspectiveCorrectionFoldout() {
			s_PerspectiveCorrectionFoldout = BeginFoldoutGroup(s_PerspectiveCorrectionFoldout, "Perspective Correction");
			return s_PerspectiveCorrectionFoldout;
		}
		public static void EndPerspectiveCorrectionFoldout() {
			EndFoldoutGroup(s_PerspectiveCorrectionFoldout);
		}
		public static bool BeginGlobalDebugFoldout() {
			s_DebugPropertiesFoldout = BeginFoldoutGroup(s_DebugPropertiesFoldout, "Debug (Global)");
			return s_DebugPropertiesFoldout;
		}
		public static void EndGlobalDebugFoldout() {
			EndFoldoutGroup(s_DebugPropertiesFoldout);
		}


		public static bool BeginFoldoutGroup(bool foldoutBoolean, string header) {
			EditorGUIHelper.BeginBackgroundColorGroup(new Color(0.7f, 0.7f, 0.7f, 1f));
			foldoutBoolean = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutBoolean, header, FoldoutHeaderStyle);
			EditorGUIHelper.EndBackgroundColorGroup();
			if (foldoutBoolean) {
				EditorGUILayout.BeginVertical(GUI.skin.box);
				EditorGUI.indentLevel += 1;
			}
			return foldoutBoolean;
		}

		public static bool BeginFoldoutGroup(bool foldoutBoolean, string header, Color backgroundColor, Color? boxColor) {
			EditorGUIHelper.BeginBackgroundColorGroup(backgroundColor);
			foldoutBoolean = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutBoolean, header, FoldoutHeaderStyle);
			EditorGUIHelper.EndBackgroundColorGroup();
			if (foldoutBoolean) {
				if (boxColor.HasValue) {
					EditorGUIHelper.BeginBackgroundColorGroup(boxColor.Value);
				}
				EditorGUILayout.BeginVertical(GUI.skin.box);
				if (boxColor.HasValue) {
					EditorGUIHelper.EndBackgroundColorGroup();
				}
				EditorGUI.indentLevel += 1;
			}
			return foldoutBoolean;
		}

		public static void EndFoldoutGroup(bool foldoutBoolean) {
			if (foldoutBoolean) {
				EditorGUI.indentLevel -= 1;
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.EndFoldoutHeaderGroup();
		}

		public static void LoadFoldoutBooleansFromEditorPrefs() {
			if (EditorPrefs.HasKey(GeneralFoldout_PrefKey)) {
				s_GeneralFoldout = EditorPrefs.GetBool(GeneralFoldout_PrefKey);
			}
			if (EditorPrefs.HasKey(DisplacementFoldout_PrefKey)) {
				s_DisplacementFoldout = EditorPrefs.GetBool(DisplacementFoldout_PrefKey);
			}
			if (EditorPrefs.HasKey(ReflectionFoldout_PrefKey)) {
				s_ReflectionFoldout = EditorPrefs.GetBool(ReflectionFoldout_PrefKey);
			}
			if (EditorPrefs.HasKey(RefractionFoldout_PrefKey)) {
				s_RefractionFoldout = EditorPrefs.GetBool(RefractionFoldout_PrefKey);
			}
			if (EditorPrefs.HasKey(FoamFoldout_PrefKey)) {
				s_FoamFoldout = EditorPrefs.GetBool(FoamFoldout_PrefKey);
			}
			if (EditorPrefs.HasKey(PerspectiveCorrectionFoldout_PrefKey)) {
				s_PerspectiveCorrectionFoldout = EditorPrefs.GetBool(PerspectiveCorrectionFoldout_PrefKey);
			}
			if (EditorPrefs.HasKey(DebugPropertiesFoldout_PrefKey)) {
				s_DebugPropertiesFoldout = EditorPrefs.GetBool(DebugPropertiesFoldout_PrefKey);
			}
		}

		public static void SaveFoldoutBooleansToEditorPrefs() {
			EditorPrefs.SetBool(GeneralFoldout_PrefKey, s_GeneralFoldout);
			EditorPrefs.SetBool(DisplacementFoldout_PrefKey, s_DisplacementFoldout);
			EditorPrefs.SetBool(ReflectionFoldout_PrefKey, s_ReflectionFoldout);
			EditorPrefs.SetBool(RefractionFoldout_PrefKey, s_RefractionFoldout);
			EditorPrefs.SetBool(FoamFoldout_PrefKey, s_FoamFoldout);
			EditorPrefs.SetBool(PerspectiveCorrectionFoldout_PrefKey, s_PerspectiveCorrectionFoldout);
			EditorPrefs.SetBool(DebugPropertiesFoldout_PrefKey, s_DebugPropertiesFoldout);
		}

		private const string FoldoutBooleanPrefPrefix = "SSWR2DProvider_";
		private const string GeneralFoldout_PrefKey = FoldoutBooleanPrefPrefix + nameof(s_GeneralFoldout);
		private const string DisplacementFoldout_PrefKey = FoldoutBooleanPrefPrefix + nameof(s_DisplacementFoldout);
		private const string ReflectionFoldout_PrefKey = FoldoutBooleanPrefPrefix + nameof(s_ReflectionFoldout);
		private const string RefractionFoldout_PrefKey = FoldoutBooleanPrefPrefix + nameof(s_RefractionFoldout);
		private const string FoamFoldout_PrefKey = FoldoutBooleanPrefPrefix + nameof(s_FoamFoldout);
		private const string PerspectiveCorrectionFoldout_PrefKey = FoldoutBooleanPrefPrefix + nameof(s_PerspectiveCorrectionFoldout);
		private const string DebugPropertiesFoldout_PrefKey = FoldoutBooleanPrefPrefix + nameof(s_DebugPropertiesFoldout);
	}
}