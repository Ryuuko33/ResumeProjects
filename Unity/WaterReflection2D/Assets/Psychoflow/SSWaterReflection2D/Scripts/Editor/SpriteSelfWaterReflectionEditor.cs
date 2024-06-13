using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;

namespace Psychoflow.SSWaterReflection2D {
	[CustomEditor(typeof(SpriteSelfWaterReflection))]
	[CanEditMultipleObjects]
	internal class SpriteSelfWaterReflectionEditor : UnityEditor.Editor {
		private static readonly Color RefreshReflectionInstanceButtonColor = new Color(47f / 255f, 214f / 255f, 0f / 255f, 1f);
		private static readonly Color RefreshReflectionInstanceTextColor = new Color(1f, 1f, 1f, 1f);

		private SpriteSelfWaterReflection[] m_BindTargets;

		private SerializedProperty m_ReflectionProvider;
		private SerializedProperty m_RefelectionReferenceOffsetY;
		private SerializedProperty m_ReflectionDepthOffset;
		private SerializedProperty m_ReflectionSortingOrder;
		private SerializedProperty m_OrderInLayerOffset;
		private SerializedProperty m_CustomSortingLayerID;
		private SerializedProperty m_CustomOrderInLayer;
		private SerializedProperty m_ReflectionMaskInteraction;
		private SerializedProperty m_PropertyCheckingFlag;

		private SerializedProperty m_HideReflectionInstanceInHierarchy;

		private SerializedObject m_SSWR2DPrefsObject;
		private SerializedProperty gizmoShowReflectionLine;

		// OnEnable is called when you focus on this inspector.
		protected void OnEnable() {
			m_BindTargets = targets.Select(x => x as SpriteSelfWaterReflection).ToArray();
			m_ReflectionProvider = serializedObject.FindProperty(nameof(m_ReflectionProvider));
			m_RefelectionReferenceOffsetY = serializedObject.FindProperty(nameof(m_RefelectionReferenceOffsetY));
			m_ReflectionDepthOffset = serializedObject.FindProperty(nameof(m_ReflectionDepthOffset));
			m_ReflectionSortingOrder = serializedObject.FindProperty(nameof(m_ReflectionSortingOrder));
			m_OrderInLayerOffset = serializedObject.FindProperty(nameof(m_OrderInLayerOffset));
			m_CustomSortingLayerID = serializedObject.FindProperty(nameof(m_CustomSortingLayerID));
			m_CustomOrderInLayer = serializedObject.FindProperty(nameof(m_CustomOrderInLayer));
			m_ReflectionMaskInteraction = serializedObject.FindProperty(nameof(m_ReflectionMaskInteraction));
			m_PropertyCheckingFlag = serializedObject.FindProperty(nameof(m_PropertyCheckingFlag));

			m_HideReflectionInstanceInHierarchy = serializedObject.FindProperty(nameof(m_HideReflectionInstanceInHierarchy));
			GlobalDebugPropertiesInit();

			SSWREditorGUI.LoadFoldoutBooleansFromEditorPrefs();

			Undo.undoRedoPerformed += UndoRedoPerformed;
		}
		private void OnDisable() {
			SSWREditorGUI.SaveFoldoutBooleansToEditorPrefs();
			Undo.undoRedoPerformed -= UndoRedoPerformed;
		}

		private void UndoRedoPerformed() {
			foreach (var m_Target in m_BindTargets) {
				m_Target.ClearAndInitReflectionRenderersLately();
			}
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
				EditorGUILayout.PropertyField(m_RefelectionReferenceOffsetY);
				if (GUILayout.Button("Set As Bound Bottom")) {
					foreach (var m_Target in m_BindTargets) {
						Undo.RecordObject(m_Target, "Set ReferenceOffset As Bound Bottom");
						m_Target.SetReferenceOffsetYAsBoundBottom();
						PrefabUtility.RecordPrefabInstancePropertyModifications(m_Target);
						UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
					}
				}

				// [TODO]: This doesn't handle with multiple selection well..
				bool lockReflectionBoundary = EditorGUILayout.Toggle("Lock Reflection Boundary", m_BindTargets[0].LockReflectionReferenceY);
				if (lockReflectionBoundary != m_BindTargets[0].LockReflectionReferenceY) {
					foreach (var m_Target in m_BindTargets) {
						Undo.RecordObject(m_Target, "Modify Lock Reflection Boundary");
						m_Target.LockReflectionReferenceY = lockReflectionBoundary;
						PrefabUtility.RecordPrefabInstancePropertyModifications(m_Target);
					}
					UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
				}

				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(m_ReflectionDepthOffset);
				bool reflectionDepthOffsetDirty = EditorGUI.EndChangeCheck();

				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(m_ReflectionSortingOrder);
				if (!m_ReflectionSortingOrder.hasMultipleDifferentValues) {
					EditorGUIHelper.PushIndentLevel();
					switch ((ReflectionSortingOrders)m_ReflectionSortingOrder.intValue) {
						case ReflectionSortingOrders.OrderInLayerOffset:
							EditorGUILayout.PropertyField(m_OrderInLayerOffset);
							break;
						case ReflectionSortingOrders.CustomSortingOrder:
							EditorGUILayout.PropertyField(m_CustomSortingLayerID);
							EditorGUILayout.PropertyField(m_CustomOrderInLayer);
							break;
						default:
							break;
					}
					EditorGUIHelper.PopIndentLevel();
				}
				bool reflectionSortingOrderDirty = EditorGUI.EndChangeCheck();


				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(m_ReflectionMaskInteraction);
				bool maskInteractionDirty = EditorGUI.EndChangeCheck();

				EditorGUILayout.PropertyField(m_PropertyCheckingFlag);

				if (EditorGUIHelper.Button("Refresh Relflection Instances",
					EditorGUIHelper.EditorButtonSize.Large, false,
					RefreshReflectionInstanceButtonColor, RefreshReflectionInstanceTextColor)) {
					foreach (var m_Target in m_BindTargets) {
						m_Target.ClearAndInitReflectionRenderers();
					}
					UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
				}

				// Debug options.
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(m_HideReflectionInstanceInHierarchy);
				bool hideInHierarchyDirty = EditorGUI.EndChangeCheck();

				serializedObject.ApplyModifiedProperties();

				if (maskInteractionDirty) {
					foreach (var m_Target in m_BindTargets) {
						m_Target.UpdateReflectionRenderersMaskInteraction();
					}
				}
				if (reflectionDepthOffsetDirty) {
					foreach (var m_Target in m_BindTargets) {
						m_Target.UpdateReflectionRenderersPosition();
					}
				}
				if (reflectionSortingOrderDirty) {
					foreach (var m_Target in m_BindTargets) {
						m_Target.UpdateReflectionRendererSortingOrder();
					}
				}
				if (hideInHierarchyDirty) {
					foreach (var m_Target in m_BindTargets) {
						m_Target.ClearAndInitReflectionRenderersLately();
					}
				}
			}
			SSWREditorGUI.EndGeneralFoldout();


			GlobalDebugPropertiesGUI();
		}

		private void GlobalDebugPropertiesInit() {
			m_SSWR2DPrefsObject = new SerializedObject(SSWR2DPrefs.Instance);
			gizmoShowReflectionLine = m_SSWR2DPrefsObject.FindProperty(nameof(gizmoShowReflectionLine));
		}
		private void GlobalDebugPropertiesGUI() {
			if (SSWREditorGUI.BeginGlobalDebugFoldout()) {
				m_SSWR2DPrefsObject.Update();
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(gizmoShowReflectionLine);
				if (EditorGUI.EndChangeCheck()) {
					m_SSWR2DPrefsObject.ApplyModifiedProperties();
				}
			}
			SSWREditorGUI.EndGlobalDebugFoldout();
		}
	}
}