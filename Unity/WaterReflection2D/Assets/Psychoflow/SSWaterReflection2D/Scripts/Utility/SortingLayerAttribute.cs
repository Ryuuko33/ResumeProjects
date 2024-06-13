using System;
using UnityEngine;
#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
#endif

namespace Psychoflow.Util {

	// From: https://think24code.wordpress.com/2014/11/14/unity3d-sorting-layer-property-drawer/
	/// <summary>
	/// The attribute for drawing sorting layer in inspector with int property.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class SortingLayerIntPropertyAttribute : PropertyAttribute {

	}

#if UNITY_EDITOR
	/**
	 * Sorting layer inspector drawer.
	 */
	[CustomPropertyDrawer(typeof(SortingLayerIntPropertyAttribute))]
	internal class SortingLayerIntPropertyDrawer : PropertyDrawer {

		/**
		 * Is called to draw a property.
		 *
		 * @param position Rectangle on the screen to use for the property GUI.
		 * @param property The SerializedProperty to make the custom GUI for.
		 * @param label The label of the property.
		 */
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			if (property.propertyType != SerializedPropertyType.Integer) {
				// Integer is expected. Everything else is ignored.
				return;
			}
			EditorGUI.LabelField(position, label);

			float offset = EditorGUIUtility.labelWidth - (EditorGUI.indentLevel * 15) + 2;
			position.x += offset;
			position.width -= offset;

			string[] sortingLayerNames = SortingLayerHelper.GetSortingLayerNames();
			int[] sortingLayerIDs = SortingLayerHelper.GetSortingLayerIDs();

			int sortingLayerIndex = Mathf.Max(0, Array.IndexOf<int>(sortingLayerIDs, property.intValue));
			sortingLayerIndex = EditorGUI.Popup(position, sortingLayerIndex, sortingLayerNames);
			property.intValue = sortingLayerIDs[sortingLayerIndex];
		}
	}

	public static class SortingLayerHelper {
		/**
		 * Retrives list of sorting layer names.
		 *
		 * @return List of sorting layer names.
		 */
		public static string[] GetSortingLayerNames() {
			System.Type internalEditorUtilityType = typeof(InternalEditorUtility);
			PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
			return (string[])sortingLayersProperty.GetValue(null, new object[0]);
		}

		/**
		 * Retrives list of sorting layer identifiers.
		 *
		 * @return List of sorting layer identifiers.
		 */
		public static int[] GetSortingLayerIDs() {
			System.Type internalEditorUtilityType = typeof(InternalEditorUtility);
			PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerUniqueIDs", BindingFlags.Static | BindingFlags.NonPublic);
			return (int[])sortingLayersProperty.GetValue(null, new object[0]);
		}
	}
#endif
}