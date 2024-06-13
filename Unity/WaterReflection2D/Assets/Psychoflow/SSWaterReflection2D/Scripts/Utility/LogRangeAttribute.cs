using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Psychoflow.Util {
	// Refer: https://forum.unity.com/threads/logarithmic-slider.290600/
	/// <summary>
	/// LogRange attribute makes float slider that value is on the log(x) curve in inspector. 
	/// It is be useful for the scale type value since 0.1x and 10x are the same distance from 1x.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class LogRangeAttribute : PropertyAttribute {
		public readonly float min = 1e-3f;
		public readonly float max = 1e3f;
		public readonly float power = 2;
		public LogRangeAttribute(float min, float max, float power) {
			if (min <= 0) {
				min = 1e-4f;
			}
			this.min = min;
			this.max = max;
			this.power = power;
		}
	}


#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(LogRangeAttribute))]
	internal class LogRangeDrawer : PropertyDrawer {
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			LogRangeAttribute attribute = (LogRangeAttribute) this.attribute;
			if (property.propertyType != SerializedPropertyType.Float) {
				EditorGUI.LabelField(position, label.text, "Use LogarithmicRange with float.");
				return;
			}

			Slider(position, property, attribute.min, attribute.max, attribute.power, label);
		}

		public static void Slider(
			Rect position, SerializedProperty property,
			float leftValue, float rightValue, float power, GUIContent label) {
			label = EditorGUI.BeginProperty(position, label, property);
			EditorGUI.BeginChangeCheck();
			float num = PowerSlider(position, label, property.floatValue, leftValue, rightValue, power);

			if (EditorGUI.EndChangeCheck())
				property.floatValue = num;
			EditorGUI.EndProperty();
		}

		public static float PowerSlider(Rect position, GUIContent label, float value, float leftValue, float rightValue, float power) {
			var editorGuiType = typeof(EditorGUI);
			var methodInfo = editorGuiType.GetMethod(
				"PowerSlider",
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static,
				null,
				new[] {typeof(Rect), typeof(GUIContent), typeof(float), typeof(float), typeof(float), typeof(float)},
				null
				);

			if (methodInfo != null) {
				return (float)methodInfo.Invoke(null, new object[] { position, label, value, leftValue, rightValue, power });
			}
			return leftValue;
		}
	}
#endif
}