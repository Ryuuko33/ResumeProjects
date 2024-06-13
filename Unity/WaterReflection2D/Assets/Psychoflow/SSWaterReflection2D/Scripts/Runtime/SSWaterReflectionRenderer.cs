using UnityEngine;

namespace Psychoflow.SSWaterReflection2D {
	/// <summary>
	/// Screen Space Reflection(SSR) is a way to reflect things based on screen texture which is much more efficient than the way by another reflection camera. 
	/// It's very useful in 2D Platformer game like Kingdom. And it can be used on both orthographic and perspective view.
	/// </summary>
	[ExecuteAlways]
	[RequireComponent(typeof(SpriteRenderer))]
    public class SSWaterReflectionRenderer : MonoBehaviour, ISSWaterReflection2DRenderer {
		#region Unity Serialized Fields
		[Tooltip("The reflection provider that provides the reflection parameters.")]
		[SerializeField] private SSWaterReflectionProvider m_ReflectionProvider = null;
		[Tooltip("Enable screen space reflection or not.")]
		[SerializeField] private bool m_EnableSSReflection = true;
		#endregion

		/// <summary>
		/// The reflection provider that provides the reflection parameters.
		/// </summary>
		public SSWaterReflectionProvider ReflectionProvider {
			get {
				return m_ReflectionProvider;
			}
			set {
				if (m_ReflectionProvider == value) {
					return;
				}
				m_LastReflectionProvider = m_ReflectionProvider;
				m_ReflectionProvider = value;
				CheckReflectionProviderChanged();
			}
		}

		/// <summary>
		/// Enable screen space reflection or not.
		/// <br/>
		/// If it's off, it'll only show the foam of water, which is useful when you only want SpriteSelf reflection and still can see foam.
		/// </summary>
		public bool EnableSSReflection {
			get => m_EnableSSReflection;
			set {
				if (m_EnableSSReflection == value) {
					return;
				}
				m_EnableSSReflection = value;
				UpdateRendererMaterial();
			}
		}

		private SpriteRenderer m_SpriteRenderer;
		private SpriteRenderer _SpriteRenderer {
			get {
				if (!m_SpriteRenderer) {
					m_SpriteRenderer = GetComponent<SpriteRenderer>();
				}
				return m_SpriteRenderer;
			}
		}

		private MaterialPropertyBlock m_MaterialPropertyBlock;
		private MaterialPropertyBlock _MaterialPropertyBlock {
			get {
				if (m_MaterialPropertyBlock == null || m_MaterialPropertyBlock.Equals(null)) {
					m_MaterialPropertyBlock = new MaterialPropertyBlock();
				}
				return m_MaterialPropertyBlock;
			}
		}

		private SSWaterReflectionProvider m_LastReflectionProvider;
		private bool m_LastEnableSSReflection;

		private void OnEnable() {
			UpdateRendererMaterial();
			if (!m_ReflectionProvider) {
				return;
			}
			m_ReflectionProvider.RegisterWaterRenderer(this);
		}

		private void OnDisable() {
			if (!m_ReflectionProvider) {
				return;
			}
			m_ReflectionProvider.RemoveWaterRenderer(this);
		}

		void LateUpdate() {
			CheckDirty();
		}

		/// <summary>
		/// To check the parameter is dirty and force to update the materials and renderer stuffs.
		/// </summary>
		void CheckDirty() {
			CheckReflectionProviderChanged();
			CheckEnableSSReflectionChanged();
		}

		void CheckReflectionProviderChanged() {
			if (m_LastReflectionProvider != m_ReflectionProvider) {
				if (m_LastReflectionProvider) {
					m_LastReflectionProvider.RemoveWaterRenderer(this);
				}
				if (m_ReflectionProvider) {
					m_ReflectionProvider.RegisterWaterRenderer(this);
				}

				m_LastReflectionProvider = m_ReflectionProvider;
				// Update.
				UpdateRendererMaterial();
			}
		}

		void CheckEnableSSReflectionChanged() {
			if (m_LastEnableSSReflection != m_EnableSSReflection) {
				UpdateRendererMaterial();
			}
		}

		// Called by m_ReflectionProvider.
		void ISSWaterReflection2DRenderer.OnParameterChanged() {
			UpdateRendererMaterial();
		}

		/// <summary>
		/// Update the material of sprite renderer.
		/// </summary>
		public void UpdateRendererMaterial() {
			if (!m_ReflectionProvider) {
				// Show pink borken.
				_SpriteRenderer.sharedMaterial = SharedWater2DMaterialFactory.GetErrorSpriteMaterial();
				return; 
			}
			// Setup shared material.
			_SpriteRenderer.sharedMaterial = SharedWater2DMaterialFactory.GetSSRWater2DMaterial(
				m_ReflectionProvider.UseUnscaledTime,
				m_ReflectionProvider.UseSecondDisplacementTexture,
				m_ReflectionProvider.EnablePerspectiveCorrection,
				false,
				m_ReflectionProvider.UseVoronoiNoiseDisplacement,
				m_ReflectionProvider.FoamBlendMode
				);
			// Use material property block to make variance.
			_SpriteRenderer.GetPropertyBlock(_MaterialPropertyBlock);

			_MaterialPropertyBlock.SetFloat(ShaderPropertyIDs.ReferenceWorldY, m_ReflectionProvider.ReferenceWorldY);
			_MaterialPropertyBlock.SetFloat(ShaderPropertyIDs.ScreenBoundFadeOut, m_ReflectionProvider.ScreenBoundFadeOut);
			_MaterialPropertyBlock.SetFloat(ShaderPropertyIDs.WaterScale, m_ReflectionProvider.WaterScale);
			if (m_ReflectionProvider.DisplacementTexture) {
				_MaterialPropertyBlock.SetTexture(ShaderPropertyIDs.DisplacementTex, m_ReflectionProvider.DisplacementTexture);
			}
			_MaterialPropertyBlock.SetFloat(ShaderPropertyIDs.DisplacementSpeed, m_ReflectionProvider.DisplacementSpeed);

			if (m_ReflectionProvider.UseSecondDisplacementTexture && m_ReflectionProvider.DisplacementSecondTexture) {
				_MaterialPropertyBlock.SetTexture(ShaderPropertyIDs.DisplacementCompositeTex, m_ReflectionProvider.DisplacementSecondTexture);
				_MaterialPropertyBlock.SetFloat(ShaderPropertyIDs.DisplacementCompositeSpeedOffset, m_ReflectionProvider.DisplacementSecondSpeedOffset);
			}

			Color reflectionTint = m_ReflectionProvider.ReflectionTint;
			if (!EnableSSReflection) {
				reflectionTint.a = 0f; // Don't reflect anything.
			}
			_MaterialPropertyBlock.SetColor(ShaderPropertyIDs.ReflectionTint, reflectionTint);
			_MaterialPropertyBlock.SetFloat(ShaderPropertyIDs.ReflectionDisplacementAmount, m_ReflectionProvider.ReflectionDisplacementAmount);
			_MaterialPropertyBlock.SetFloat(ShaderPropertyIDs.ReflectionFoamWeight, m_ReflectionProvider.ReflectionFoamWeight);

			_MaterialPropertyBlock.SetColor(ShaderPropertyIDs.RefractionTint, m_ReflectionProvider.RefractionTint);
			_MaterialPropertyBlock.SetFloat(ShaderPropertyIDs.RefractionDisplacementAmount, m_ReflectionProvider.RefractionDisplacementAmount);
			_MaterialPropertyBlock.SetFloat(ShaderPropertyIDs.RefractionFoamWeight, m_ReflectionProvider.RefractionFoamWeight);

			_MaterialPropertyBlock.SetColor(ShaderPropertyIDs.FoamColor, m_ReflectionProvider.FoamColor);
			_MaterialPropertyBlock.SetFloat(ShaderPropertyIDs.FoamThreshold, m_ReflectionProvider.FoamThreshold);
			_MaterialPropertyBlock.SetFloat(ShaderPropertyIDs.FoamEdgeThreshold, m_ReflectionProvider.FoamEdgeThreshold);

			if (m_ReflectionProvider.EnablePerspectiveCorrection) {
				_MaterialPropertyBlock.SetFloat(ShaderPropertyIDs.PerspectiveCorrectionStrength, m_ReflectionProvider.PerspectiveCorrectionStrength);
				_MaterialPropertyBlock.SetFloat(ShaderPropertyIDs.PerspectiveCorrectionContrast, m_ReflectionProvider.PerspectiveCorrectionContrast);
				_MaterialPropertyBlock.SetFloat(ShaderPropertyIDs.PerspectiveCorrectionMaxLength, m_ReflectionProvider.PerspectiveCorrectionMaxLength);
				_MaterialPropertyBlock.SetFloat(ShaderPropertyIDs.PerspectiveReflectionTiltY, m_ReflectionProvider.PerspectiveReflectionTiltY);
			}

			if (m_ReflectionProvider.UseVoronoiNoiseDisplacement) {
				_MaterialPropertyBlock.SetFloat(ShaderPropertyIDs.VoronoiAngleOffset, m_ReflectionProvider.VoronoiAngleOffset);
				_MaterialPropertyBlock.SetFloat(ShaderPropertyIDs.VoronoiAngleSpeed, m_ReflectionProvider.VoronoiAngleSpeed);
				_MaterialPropertyBlock.SetFloat(ShaderPropertyIDs.VoronoiCellDensity, m_ReflectionProvider.VoronoiCellDensity);
				_MaterialPropertyBlock.SetFloat(ShaderPropertyIDs.VoronoiPower, m_ReflectionProvider.VoronoiPowerValue);
			}

			_SpriteRenderer.SetPropertyBlock(_MaterialPropertyBlock);

			// Clear dirty flag.
			m_LastEnableSSReflection = m_EnableSSReflection;
		}

#if UNITY_EDITOR
		private void Reset() {
			if (!this.m_ReflectionProvider) {
				this.AssignReflectionProviderAsClosest();
			}
		}

		/// <summary>
		/// [Editor Only] Find the closest <see cref="SSWaterReflectionProvider"/> and assign <see cref="SSWaterReflectionRenderer.ReflectionProvider"/> by it.
		/// </summary>
		public void AssignReflectionProviderAsClosest() {
			var providers = FindObjectsOfType<SSWaterReflectionProvider>();
			float closestDist = float.MaxValue;

            foreach (var provider in providers) {
				float dist = Vector3.Distance(provider.transform.position, this.transform.position);
				if (dist < closestDist) {
					closestDist = dist;
					m_ReflectionProvider = provider;
				}
            }
		}

		private void OnDrawGizmosSelected() {
			if (!m_ReflectionProvider) {
				return;
			}
			Bounds bound = _SpriteRenderer.bounds;
			if (SSWR2DPrefs.Instance.gizmoShowReflectionLine) {
				m_ReflectionProvider.DrawGizmoCubeAsReflectionLine();
				Vector3 referencePoint = new Vector3(transform.position.x, m_ReflectionProvider.ReferenceWorldY, transform.position.z);
				Vector3 point1 = referencePoint - new Vector3(bound.size.x * 0.5f, 0f, 0f);
				Vector3 point2 = referencePoint + new Vector3(bound.size.x * 0.5f, 0f, 0f);
				Gizmos.color = Color.red;
				Gizmos.DrawLine(point1, point2);
			}

			if (SSWR2DPrefs.Instance.gizmoShowFoamEdgeThreshold) {
				Gizmos.color = Color.yellow;
				Vector3 foamEdgePoint = new Vector3(transform.position.x, m_ReflectionProvider.ReferenceWorldY, transform.position.z) - new Vector3(0, m_ReflectionProvider.FoamEdgeThreshold, 0);
				Vector3 point1 = foamEdgePoint - new Vector3(bound.size.x * 0.5f, 0f, 0f);
				Vector3 point2 = foamEdgePoint + new Vector3(bound.size.x * 0.5f, 0f, 0f);
				Gizmos.DrawLine(point1, point2);
			}

			if (SSWR2DPrefs.Instance.gizmoShowPerspectiveMaxLength) {
				Gizmos.color = Color.green;
				Vector3 perspectivePoint = new Vector3(transform.position.x, m_ReflectionProvider.ReferenceWorldY, transform.position.z) - new Vector3(0, m_ReflectionProvider.PerspectiveCorrectionMaxLength, 0);
				Vector3 point1 = perspectivePoint - new Vector3(bound.size.x * 0.5f, 0f, 0f);
				Vector3 point2 = perspectivePoint + new Vector3(bound.size.x * 0.5f, 0f, 0f);
				Gizmos.DrawLine(point1, point2);
			}
		}
#endif
	}
}