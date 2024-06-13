using UnityEngine;

namespace Psychoflow.Util {

	public static class Vector2Extensions {
		// From 2D GameKit.Helper
		/// <summary>
		/// Rotate a vector2 by counter-clockwise.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="degrees"></param>
		/// <returns></returns>
		public static Vector2 Rotate(this Vector2 v, float degrees) {
			if (degrees == 0f) { return v; }

			float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
			float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

			float tx = v.x;
			float ty = v.y;
			v.x = (cos * tx) - (sin * ty);
			v.y = (sin * tx) + (cos * ty);
			return v;
		}

		/// <summary>
		/// Get the multiple of two vectors component-wise.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="scalar">Scale</param>
		/// <returns>The multiple of two vector2 component-wise.</returns>
		public static Vector2 GetScale(this Vector2 v, in Vector2 scalar) {
			return new Vector2(v.x * scalar.x, v.y * scalar.y);
		}
	}

	public static class MaterialExtensions {
		public static void SetBooleanParameter(this Material mat, int shaderPropertyID, string shaderKeyword, bool value) {
			if (value) {
				mat.SetFloat(shaderPropertyID, 1f);
				mat.EnableKeyword(shaderKeyword);
			} else {
				mat.SetFloat(shaderPropertyID, 0f);
				mat.DisableKeyword(shaderKeyword);
			}
		}

		public static void SetEnumKeyword(this Material mat, int shaderPropertyID, string[] keywords, int value) {
			mat.SetFloat(shaderPropertyID, value);
			for (int i = 0; i < keywords.Length; i++) {
				string keyword = keywords[i];
				if (i == value) {
					mat.EnableKeyword(keyword);
				} else {
					mat.DisableKeyword(keyword);
				}
			}
		}
	}
}