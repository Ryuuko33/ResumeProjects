using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;

namespace Psychoflow.SSWaterReflection2D {
	[CustomEditor(typeof(SSWaterReflectionRenderer))]
	[CanEditMultipleObjects]
	internal class SSWaterReflectionRendererEditor : UnityEditor.Editor {
		private SSWaterReflectionRenderer[] m_BindTargets;

		private SerializedProperty m_ReflectionProvider;
		private SerializedProperty m_EnableSSReflection;


		private SerializedObject m_SSWR2DPrefsObject;
		private SerializedProperty gizmoShowReflectionLine;
		private SerializedProperty gizmoShowFoamEdgeThreshold;
		private SerializedProperty gizmoShowPerspectiveMaxLength;

		// OnEnable is called when you focus on this inspector.
		protected void OnEnable() {
			m_BindTargets = targets.Select(x => x as SSWaterReflectionRenderer).ToArray();
			m_ReflectionProvider = serializedObject.FindProperty(nameof(m_ReflectionProvider));
			m_EnableSSReflection = serializedObject.FindProperty(nameof(m_EnableSSReflection));

			GlobalDebugPropertiesInit();
			SSWREditorGUI.LoadFoldoutBooleansFromEditorPrefs();
		}
		private void OnDisable() {
			SSWREditorGUI.SaveFoldoutBooleansToEditorPrefs();
		}

		private bool AnyTargetProviderIsNull() {
			foreach (var item in m_BindTargets) {
				if (item.ReflectionProvider == null) {
					return true;
				}
			}
			return false;
		}
		// OnInspectorGUI is called once per inspector update.
		public override void OnInspectorGUI() {
			serializedObject.Update();
			if (SSWREditorGUI.BeginGeneralFoldout()) {

				EditorGUILayout.PropertyField(m_ReflectionProvider);
				if (AnyTargetProviderIsNull()) {
					if (EditorGUIHelper.Button("Find Closest Provider", EditorGUIHelper.EditorButtonSize.Small)) {
						foreach (var m_Target in m_BindTargets) {
							// Only for null
							if (m_Target.ReflectionProvider == null) {
								Undo.RecordObject(m_Target, "Find Closest Provider");
								m_Target.AssignReflectionProviderAsClosest();
								PrefabUtility.RecordPrefabInstancePropertyModifications(m_Target);
							}
						}
					}
				}
				EditorGUILayout.PropertyField(m_EnableSSReflection);
				
				serializedObject.ApplyModifiedProperties();
			}
			SSWREditorGUI.EndGeneralFoldout();

			// Debug options.
			GlobalDebugPropertiesGUI();
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