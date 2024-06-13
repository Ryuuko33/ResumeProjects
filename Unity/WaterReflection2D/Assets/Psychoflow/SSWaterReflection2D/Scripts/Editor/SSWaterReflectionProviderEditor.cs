using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;

namespace Psychoflow.SSWaterReflection2D {
	[CustomEditor(typeof(SSWaterReflectionProvider))]
	[CanEditMultipleObjects]
    internal class SSWaterReflectionProviderEditor : UnityEditor.Editor {
		private SerializedProperty m_UseUnscaledTime;
		private SerializedProperty m_ReferenceLocalY;
		private SerializedProperty m_ScreenBoundFadeOut;
		private SerializedProperty m_WaterScale;

		private SerializedProperty m_DisplacementMode;

		private SerializedProperty m_DisplacementTexture;
		private SerializedProperty m_DisplacementSpeed;
		private SerializedProperty m_UseSecondDisplacementTexture;
		private SerializedProperty m_DisplacementSecondTexture;
		private SerializedProperty m_DisplacementSecondSpeedOffset;

		private SerializedProperty m_ReflectionTint;
		private SerializedProperty m_ReflectionDisplacementAmount;
		private SerializedProperty m_ReflectionFoamWeight;

		private SerializedProperty m_RefractionTint;
		private SerializedProperty m_RefractionDisplacementAmount;
		private SerializedProperty m_RefractionFoamWeight;

		private SerializedProperty m_FoamColor;
		private SerializedProperty m_FoamBlendMode;
		private SerializedProperty m_FoamThreshold;
		private SerializedProperty m_FoamEdgeThreshold;

		private SerializedProperty m_EnablePerspectiveCorrection;
		private SerializedProperty m_PerspectiveCorrectionStrength;
		private SerializedProperty m_PerspectiveCorrectionContrast;
		private SerializedProperty m_PerspectiveCorrectionMaxLength;
		private SerializedProperty m_PerspectiveReflectionTiltY;

		private SerializedProperty m_VoronoiAngleOffset;
		private SerializedProperty m_VoronoiAngleSpeed;
		private SerializedProperty m_VoronoiCellDensity;
		private SerializedProperty m_VoronoiPowerValue;

		private SerializedObject m_SSWR2DPrefsObject;
		private SerializedProperty gizmoShowReflectionLine;
		private SerializedProperty gizmoShowFoamEdgeThreshold;
		private SerializedProperty gizmoShowPerspectiveMaxLength;

		// OnEnable is called when you focus on this inspector.
		protected void OnEnable() {
			m_UseUnscaledTime = serializedObject.FindProperty(nameof(m_UseUnscaledTime));
			m_ReferenceLocalY = serializedObject.FindProperty(nameof(m_ReferenceLocalY));
			m_ScreenBoundFadeOut = serializedObject.FindProperty(nameof(m_ScreenBoundFadeOut));
			m_WaterScale = serializedObject.FindProperty(nameof(m_WaterScale));

			m_DisplacementMode = serializedObject.FindProperty(nameof(m_DisplacementMode));

			m_DisplacementTexture = serializedObject.FindProperty(nameof(m_DisplacementTexture));
			m_DisplacementSpeed = serializedObject.FindProperty(nameof(m_DisplacementSpeed));
			m_UseSecondDisplacementTexture = serializedObject.FindProperty(nameof(m_UseSecondDisplacementTexture));
			m_DisplacementSecondTexture = serializedObject.FindProperty(nameof(m_DisplacementSecondTexture));
			m_DisplacementSecondSpeedOffset = serializedObject.FindProperty(nameof(m_DisplacementSecondSpeedOffset));

			m_VoronoiAngleOffset = serializedObject.FindProperty(nameof(m_VoronoiAngleOffset));
			m_VoronoiAngleSpeed = serializedObject.FindProperty(nameof(m_VoronoiAngleSpeed));
			m_VoronoiCellDensity = serializedObject.FindProperty(nameof(m_VoronoiCellDensity));
			m_VoronoiPowerValue = serializedObject.FindProperty(nameof(m_VoronoiPowerValue));

			m_ReflectionTint = serializedObject.FindProperty(nameof(m_ReflectionTint));
			m_ReflectionDisplacementAmount = serializedObject.FindProperty(nameof(m_ReflectionDisplacementAmount));
			m_ReflectionFoamWeight = serializedObject.FindProperty(nameof(m_ReflectionFoamWeight));

			m_RefractionTint = serializedObject.FindProperty(nameof(m_RefractionTint));
			m_RefractionDisplacementAmount = serializedObject.FindProperty(nameof(m_RefractionDisplacementAmount));
			m_RefractionFoamWeight = serializedObject.FindProperty(nameof(m_RefractionFoamWeight));

			m_FoamColor = serializedObject.FindProperty(nameof(m_FoamColor));
			m_FoamBlendMode = serializedObject.FindProperty(nameof(m_FoamBlendMode));
			m_FoamThreshold = serializedObject.FindProperty(nameof(m_FoamThreshold));
			m_FoamEdgeThreshold = serializedObject.FindProperty(nameof(m_FoamEdgeThreshold));

			m_EnablePerspectiveCorrection = serializedObject.FindProperty(nameof(m_EnablePerspectiveCorrection));
			m_PerspectiveCorrectionStrength = serializedObject.FindProperty(nameof(m_PerspectiveCorrectionStrength));
			m_PerspectiveCorrectionContrast = serializedObject.FindProperty(nameof(m_PerspectiveCorrectionContrast));
			m_PerspectiveCorrectionMaxLength = serializedObject.FindProperty(nameof(m_PerspectiveCorrectionMaxLength));
			m_PerspectiveReflectionTiltY = serializedObject.FindProperty(nameof(m_PerspectiveReflectionTiltY));

			GlobalDebugPropertiesInit();

			SSWREditorGUI.LoadFoldoutBooleansFromEditorPrefs();
		}
		private void OnDisable() {
			SSWREditorGUI.SaveFoldoutBooleansToEditorPrefs();
		}

		// OnInspectorGUI is called once per inspector update.
		public override void OnInspectorGUI() {
			//base.OnInspectorGUI();
			serializedObject.Update();
			if (SSWREditorGUI.BeginGeneralFoldout()) {
				EditorGUILayout.PropertyField(m_UseUnscaledTime);
				EditorGUILayout.PropertyField(m_ReferenceLocalY);
				EditorGUILayout.PropertyField(m_ScreenBoundFadeOut);
				EditorGUILayout.PropertyField(m_WaterScale);
			}
			SSWREditorGUI.EndGeneralFoldout();


			if (SSWREditorGUI.BeginDisplacementFoldout()) {
				EditorGUILayout.PropertyField(m_DisplacementMode);
				if (!m_DisplacementMode.hasMultipleDifferentValues) {
					switch ((DisplacementModes)m_DisplacementMode.intValue) {
						case DisplacementModes.Texture:
							DisplacementTextureGUI();
							break;
						case DisplacementModes.Voronoi:
							VoronoiDisplacementGUI();
							break;
						default:
							break;
					}
				}
				
			}
			SSWREditorGUI.EndDisplacementFoldout();

			if (SSWREditorGUI.BeginReflectionFoldout()) {
				EditorGUILayout.PropertyField(m_ReflectionTint);
				EditorGUILayout.PropertyField(m_ReflectionDisplacementAmount);
				EditorGUILayout.PropertyField(m_ReflectionFoamWeight);
			}
			SSWREditorGUI.EndReflectionFoldout();

			if (SSWREditorGUI.BeginRefractionFoldout()) {
					EditorGUILayout.PropertyField(m_RefractionTint);
				EditorGUILayout.PropertyField(m_RefractionDisplacementAmount);
				EditorGUILayout.PropertyField(m_RefractionFoamWeight);
			}
			SSWREditorGUI.EndRefractionFoldout();

			if (SSWREditorGUI.BeginFoamFoldout()) {
					EditorGUILayout.PropertyField(m_FoamColor);
				EditorGUILayout.PropertyField(m_FoamBlendMode);
				EditorGUILayout.PropertyField(m_FoamThreshold);
				EditorGUILayout.PropertyField(m_FoamEdgeThreshold);
			}
			SSWREditorGUI.EndFoamFoldout();

			if (SSWREditorGUI.BeginPerspectiveCorrectionFoldout()) {
				EditorGUILayout.PropertyField(m_EnablePerspectiveCorrection);

				bool disableGroup = m_EnablePerspectiveCorrection.hasMultipleDifferentValues || !m_EnablePerspectiveCorrection.boolValue;
				EditorGUI.BeginDisabledGroup(disableGroup);
				EditorGUILayout.PropertyField(m_PerspectiveCorrectionStrength, new GUIContent("Strength"));
				EditorGUILayout.PropertyField(m_PerspectiveCorrectionContrast, new GUIContent("Contrast"));
				EditorGUILayout.PropertyField(m_PerspectiveCorrectionMaxLength, new GUIContent("Max Length"));
				EditorGUILayout.PropertyField(m_PerspectiveReflectionTiltY, new GUIContent("Tilt Y"));
				EditorGUI.EndDisabledGroup();
			}
			SSWREditorGUI.EndPerspectiveCorrectionFoldout();

			GlobalDebugPropertiesGUI();

			serializedObject.ApplyModifiedProperties();
		}

		private void DisplacementTextureGUI() {
			EditorGUILayout.PropertyField(m_DisplacementTexture);
			EditorGUILayout.PropertyField(m_DisplacementSpeed);
			EditorGUILayout.PropertyField(m_UseSecondDisplacementTexture, new GUIContent("Use Second Texture"));

			bool disableGroup = m_UseSecondDisplacementTexture.hasMultipleDifferentValues || !m_UseSecondDisplacementTexture.boolValue;
			EditorGUI.BeginDisabledGroup(disableGroup);
			EditorGUILayout.PropertyField(m_DisplacementSecondTexture, new GUIContent("Second Texture"));
			EditorGUILayout.PropertyField(m_DisplacementSecondSpeedOffset, new GUIContent("Second Speed Offset"));
			EditorGUI.EndDisabledGroup();
		}

		private void VoronoiDisplacementGUI() {
			EditorGUILayout.PropertyField(m_DisplacementSpeed);
			EditorGUILayout.PropertyField(m_VoronoiAngleOffset);
			EditorGUILayout.PropertyField(m_VoronoiAngleSpeed);
			EditorGUILayout.PropertyField(m_VoronoiCellDensity);
			EditorGUILayout.PropertyField(m_VoronoiPowerValue);
		}

		private void GlobalDebugPropertiesInit() {
			m_SSWR2DPrefsObject = new SerializedObject(SSWR2DPrefs.Instance);
			gizmoShowReflectionLine = m_SSWR2DPrefsObject.FindProperty(nameof(gizmoShowReflectionLine));
			gizmoShowFoamEdgeThreshold = m_SSWR2DPrefsObject.FindProperty(nameof(gizmoShowFoamEdgeThreshold));
			gizmoShowPerspectiveMaxLength = m_SSWR2DPrefsObject.FindProperty(nameof(gizmoShowPerspectiveMaxLength));
		}
		private void GlobalDebugPropertiesGUI() {
			if (SSWREditorGUI.BeginGlobalDebugFoldout()) {
				m_SSWR2DPrefsObject.Update();
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(gizmoShowReflectionLine);
				EditorGUILayout.PropertyField(gizmoShowFoamEdgeThreshold);
				EditorGUILayout.PropertyField(gizmoShowPerspectiveMaxLength);
				if (EditorGUI.EndChangeCheck()) {
					m_SSWR2DPrefsObject.ApplyModifiedProperties();
				}
			}
			SSWREditorGUI.EndGlobalDebugFoldout();
		}

		
	}
}