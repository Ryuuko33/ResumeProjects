using UnityEngine;
using Psychoflow.Util;

namespace Psychoflow.SSWaterReflection2D {
	internal struct SSWaterReflectionVariance : System.IEquatable<SSWaterReflectionVariance> {
		public bool useUnscaledTime;
		public bool useDisplacementCompositeTexture;
		public bool enablePerpectiveCorrection;
		public bool enableVertexDisplacement;
		public bool useVoronoiNoiseDisplacement;
		public FoamBlendMode foamBlendMode;
#if SUPPORTS_PSYCHOFLOW_BLEND_MODES
		public int spriteColorOverlayBlendMode;
#endif

		public override bool Equals(object obj) {
			if (this.GetType() != obj.GetType()) {
				return false;
			}

			return this.Equals((SSWaterReflectionVariance)obj);
		}

#if SUPPORTS_PSYCHOFLOW_BLEND_MODES
		public SSRWater2DVariance(bool useUnscaledTime,
			bool useDisplacementCompositeTexture, bool enablePerpectiveCorrection,
			bool enableVertexDisplacement, bool useVoronoiNoiseDisplacement,
			FoamBlendMode foamBlendMode,
			int spriteColorOverlayBlendMode) {
			this.useUnscaledTime = useUnscaledTime;
			this.useDisplacementCompositeTexture = useDisplacementCompositeTexture;
			this.enablePerpectiveCorrection = enablePerpectiveCorrection;
			this.enableVertexDisplacement = enableVertexDisplacement;
			this.useVoronoiNoiseDisplacement = useVoronoiNoiseDisplacement;
			this.foamBlendMode = foamBlendMode;
			this.spriteColorOverlayBlendMode = spriteColorOverlayBlendMode;
		}

		public override string ToString() {
			return string.Format("{0}-{1}{2}{3}{4}-foam:{5}-spriteColorOverlay:{6}",
				useUnscaledTime ? "UnscaledTime" : "ScaledTime",
				useDisplacementCompositeTexture ? "T" : "F",
				enablePerpectiveCorrection ? "T" : "F",
				enableVertexDisplacement ? "T" : "F",
				useVoronoiNoiseDisplacement ? "T" : "F",
				foamBlendMode.ToString(),
				spriteColorOverlayBlendMode
				);
		}

		public bool Equals(SSRWater2DVariance other) {
			return (this.useUnscaledTime == other.useUnscaledTime &&
				this.useDisplacementCompositeTexture == other.useDisplacementCompositeTexture &&
				this.enablePerpectiveCorrection == other.enablePerpectiveCorrection &&
				this.enableVertexDisplacement == other.enableVertexDisplacement &&
				this.useVoronoiNoiseDisplacement == other.useVoronoiNoiseDisplacement &&
				this.foamBlendMode == other.foamBlendMode &&
				this.spriteColorOverlayBlendMode == other.spriteColorOverlayBlendMode
				);
		}

		public override int GetHashCode() {
			return HashCode.Combine(
					this.useUnscaledTime,
					this.useDisplacementCompositeTexture,
					this.enablePerpectiveCorrection,
					this.enableVertexDisplacement,
					this.useVoronoiNoiseDisplacement,
					this.foamBlendMode,
					this.spriteColorOverlayBlendMode
					);
		}
#else
		public SSWaterReflectionVariance(bool useUnscaledTime,
			bool useDisplacementCompositeTexture, bool enablePerpectiveCorrection,
			bool enableVertexDisplacement, bool useVoronoiNoiseDisplacement,
			FoamBlendMode foamBlendMode) {
			this.useUnscaledTime = useUnscaledTime;
			this.useDisplacementCompositeTexture = useDisplacementCompositeTexture;
			this.enablePerpectiveCorrection = enablePerpectiveCorrection;
			this.enableVertexDisplacement = enableVertexDisplacement;
			this.useVoronoiNoiseDisplacement = useVoronoiNoiseDisplacement;
			this.foamBlendMode = foamBlendMode;
		}

		public override string ToString() {
			return string.Format("{0}-{1}{2}{3}{4}-{5}",
				useUnscaledTime ? "UnscaledTime" : "ScaledTime",
				useDisplacementCompositeTexture ? "T" : "F",
				enablePerpectiveCorrection ? "T" : "F",
				enableVertexDisplacement ? "T" : "F",
				useVoronoiNoiseDisplacement ? "T" : "F",
				foamBlendMode.ToString()
				);
		}

		public bool Equals(SSWaterReflectionVariance other) {
			return (this.useUnscaledTime == other.useUnscaledTime &&
				this.useDisplacementCompositeTexture == other.useDisplacementCompositeTexture &&
				this.enablePerpectiveCorrection == other.enablePerpectiveCorrection &&
				this.enableVertexDisplacement == other.enableVertexDisplacement &&
				this.useVoronoiNoiseDisplacement == other.useVoronoiNoiseDisplacement &&
				this.foamBlendMode == other.foamBlendMode
				);
		}

		public override int GetHashCode() {
			return Util.HashCode.Combine(
					this.useUnscaledTime,
					this.useDisplacementCompositeTexture,
					this.enablePerpectiveCorrection,
					this.enableVertexDisplacement,
					this.useVoronoiNoiseDisplacement,
					this.foamBlendMode
					);
		}
#endif
	}
	internal class SharedWater2DMaterialFactory {
#region Singleton
		private static SharedWater2DMaterialFactory s_Instance;
		public static SharedWater2DMaterialFactory Instance {
			get {
				if (s_Instance == null) {
					s_Instance = new SharedWater2DMaterialFactory();
				}
				return s_Instance;
			}
		}
#endregion

#region Shader IDs & Enums
		public static readonly int SHADER_ID_USE_UNSCALED_TIME = Shader.PropertyToID("_USE_UNSCALED_TIME");
		public static readonly int SHADER_ID_USE_COMPOSITE_TEX = Shader.PropertyToID("_USE_COMPOSITE_TEX");
		public static readonly int SHADER_ID_PERSPECTIVE_CORRECTION = Shader.PropertyToID("_PERSPECTIVE_CORRECTION");
		public static readonly int SHADER_ID_VERTEX_DISPLACEMENT = Shader.PropertyToID("_VERTEX_DISPLACEMENT");
		public static readonly int SHADER_ID_VORONOI_NOISE_DISPLACEMENT = Shader.PropertyToID("_VORONOI_NOISE_DISPLACEMENT");
		public static readonly int SHADER_ID_FOAM_BLEND = Shader.PropertyToID("FOAM_BLEND");

		public const string SHADER_KEYWORD_USE_UNSCALED_TIME = "USE_UNSCALED_TIME";
		public const string SHADER_KEYWORD_USE_COMPOSITE_TEX = "USE_COMPOSITE_TEX";
		public const string SHADER_KEYWORD_PERSPECTIVE_CORRECTION = "PERSPECTIVE_CORRECTION";
		public const string SHADER_KEYWORD_VERTEX_DISPLACEMENT = "VERTEX_DISPLACEMENT";
		public const string SHADER_KEYWORD_VORONOI_NOISE_DISPLACEMENT = "VORONOI_NOISE_DISPLACEMENT";

		private static readonly string[] SHADER_KEYWORD_FOAM_BLEND_MODES = new string[] {
			"FOAM_BLEND_NORMAL",
			"FOAM_BLEND_ADD",
			"FOAM_BLEND_MULTIPLY",
			"FOAM_BLEND_SOFTLIGHT",
		};
#endregion

		public SharedWater2DMaterialFactory() {
			m_SharedSSWRelfectionMaterials = new SharedSSWRelfectionMaterials();
			m_SharedSpriteReflectionWaterMaterials = new SharedSpriteSelfReflectionMaterials();
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void StaticInit() {
			Instance.Clear();
		}

		public void Clear() {
			m_SharedSSWRelfectionMaterials.ClearAllInstances();
			m_SharedSpriteReflectionWaterMaterials.ClearAllInstances();
		}

		private static Material s_ErrorSpriteMaterial = null;
		public static Material GetErrorSpriteMaterial() {
			if (s_ErrorSpriteMaterial == null || s_ErrorSpriteMaterial.Equals(null)) {
				s_ErrorSpriteMaterial = new Material(Shader.Find("Hidden/ErrorSpriteShader"));
			}
			return s_ErrorSpriteMaterial;
		}

#if SUPPORTS_PSYCHOFLOW_BLEND_MODES
		public static Material GetSSRWater2DMaterial(bool useUnscaledTime,
										bool useDisplacementCompositeTexture, bool enablePerpectiveCorrection,
										bool enableVertexDisplacement, bool useVoronoiNoiseDisplacement,
										FoamBlendMode foamBlendMode) {
			return Instance.m_SharedSSRWater2DMaterials.Get(
				new SSRWater2DVariance(useUnscaledTime,
				useDisplacementCompositeTexture, enablePerpectiveCorrection,
				enableVertexDisplacement, useVoronoiNoiseDisplacement,
				foamBlendMode, 
				0)
				);
		}

		public static Material GetSpriteRefleciotnWaterMaterial(bool useUnscaledTime,
										bool useDisplacementCompositeTexture, bool enablePerpectiveCorrection,
										bool enableVertexDisplacement, bool useVoronoiNoiseDisplacement,
										FoamBlendMode foamBlendMode,
										int spriteColorOverlayBlendMode) {
			return Instance.m_SharedSpriteReflectionWaterMaterials.Get(
				new SSRWater2DVariance(useUnscaledTime, useDisplacementCompositeTexture, enablePerpectiveCorrection,
				enableVertexDisplacement, useVoronoiNoiseDisplacement,
				foamBlendMode,
				spriteColorOverlayBlendMode)
				);
		}
#else
		public static Material GetSSRWater2DMaterial(bool useUnscaledTime,
										bool useDisplacementCompositeTexture, bool enablePerpectiveCorrection,
										bool enableVertexDisplacement, bool useVoronoiNoiseDisplacement,
										FoamBlendMode foamBlendMode) {
			return Instance.m_SharedSSWRelfectionMaterials.Get(
				new SSWaterReflectionVariance(useUnscaledTime, 
				useDisplacementCompositeTexture, enablePerpectiveCorrection,
				enableVertexDisplacement, useVoronoiNoiseDisplacement, 
				foamBlendMode)
				);
		}

		public static Material GetSpriteSelfReflectionWaterMaterial(bool useUnscaledTime,
										bool useDisplacementCompositeTexture, bool enablePerpectiveCorrection,
										bool enableVertexDisplacement, bool useVoronoiNoiseDisplacement,
										FoamBlendMode foamBlendMode) {
			return Instance.m_SharedSpriteReflectionWaterMaterials.Get(
				new SSWaterReflectionVariance(useUnscaledTime, useDisplacementCompositeTexture, enablePerpectiveCorrection,
				enableVertexDisplacement, useVoronoiNoiseDisplacement,
				foamBlendMode)
				);
		}
#endif

		private SharedSSWRelfectionMaterials m_SharedSSWRelfectionMaterials;
		private SharedSpriteSelfReflectionMaterials m_SharedSpriteReflectionWaterMaterials;

		private class SharedSSWRelfectionMaterials : SharedResources<SSWaterReflectionVariance, Material> {
#region Static Resources
			private static Shader s_SSWaterReflectionShader;
			private static Shader SSWaterReflectionShader {
				get {
					if (s_SSWaterReflectionShader == null || s_SSWaterReflectionShader.Equals(null)) {
						s_SSWaterReflectionShader = Shader.Find("SSWaterReflection2D/SSWaterReflection");
					}
					return s_SSWaterReflectionShader;
				}
			}
#endregion
			public SharedSSWRelfectionMaterials() : base() { }
			public SharedSSWRelfectionMaterials(params SSWaterReflectionVariance[] keys) : base(keys) { }
			protected override Material CreateInstance(SSWaterReflectionVariance keyValue) {
				Material mat = new Material(SSWaterReflectionShader) {
					name = string.Format("SSWaterReflection-({0})", keyValue.ToString()),
					hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild
				};
				mat.SetBooleanParameter(SHADER_ID_USE_UNSCALED_TIME, SHADER_KEYWORD_USE_UNSCALED_TIME, keyValue.useUnscaledTime);
				mat.SetBooleanParameter(SHADER_ID_USE_COMPOSITE_TEX, SHADER_KEYWORD_USE_COMPOSITE_TEX, keyValue.useDisplacementCompositeTexture);
				mat.SetBooleanParameter(SHADER_ID_PERSPECTIVE_CORRECTION, SHADER_KEYWORD_PERSPECTIVE_CORRECTION, keyValue.enablePerpectiveCorrection);
				mat.SetBooleanParameter(SHADER_ID_VERTEX_DISPLACEMENT, SHADER_KEYWORD_VERTEX_DISPLACEMENT, keyValue.enableVertexDisplacement);
				mat.SetBooleanParameter(SHADER_ID_VORONOI_NOISE_DISPLACEMENT, SHADER_KEYWORD_VORONOI_NOISE_DISPLACEMENT, keyValue.useVoronoiNoiseDisplacement);
				mat.SetEnumKeyword(SHADER_ID_FOAM_BLEND, SHADER_KEYWORD_FOAM_BLEND_MODES, (int)keyValue.foamBlendMode);
				return mat;
			}
		}

		private class SharedSpriteSelfReflectionMaterials : SharedResources<SSWaterReflectionVariance, Material> {
#region Static Resources
			private static Shader s_SpriteReflectionWaterShader;
			private static Shader SpriteReflectionWaterShader {
				get {
					if (s_SpriteReflectionWaterShader == null || s_SpriteReflectionWaterShader.Equals(null)) {
						s_SpriteReflectionWaterShader = Shader.Find("SSWaterReflection2D/Sprite-ReflectionWater");
					}
					return s_SpriteReflectionWaterShader;
				}
			}
#endregion
			public SharedSpriteSelfReflectionMaterials() : base() { }
			public SharedSpriteSelfReflectionMaterials(params SSWaterReflectionVariance[] keys) : base(keys) { }
			protected override Material CreateInstance(SSWaterReflectionVariance keyValue) {
				Material mat = new Material(SpriteReflectionWaterShader) {
					name = string.Format("Sprite-ReflectionWater-({0})", keyValue.ToString()),
					hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild
				};
				mat.SetBooleanParameter(SHADER_ID_USE_UNSCALED_TIME, SHADER_KEYWORD_USE_UNSCALED_TIME, keyValue.useUnscaledTime);
				mat.SetBooleanParameter(SHADER_ID_USE_COMPOSITE_TEX, SHADER_KEYWORD_USE_COMPOSITE_TEX, keyValue.useDisplacementCompositeTexture);
				mat.SetBooleanParameter(SHADER_ID_PERSPECTIVE_CORRECTION, SHADER_KEYWORD_PERSPECTIVE_CORRECTION, keyValue.enablePerpectiveCorrection);
				mat.SetBooleanParameter(SHADER_ID_VERTEX_DISPLACEMENT, SHADER_KEYWORD_VERTEX_DISPLACEMENT, keyValue.enableVertexDisplacement);
				mat.SetBooleanParameter(SHADER_ID_VORONOI_NOISE_DISPLACEMENT, SHADER_KEYWORD_VORONOI_NOISE_DISPLACEMENT, keyValue.useVoronoiNoiseDisplacement);
				mat.SetEnumKeyword(SHADER_ID_FOAM_BLEND, SHADER_KEYWORD_FOAM_BLEND_MODES, (int)keyValue.foamBlendMode);
#if SUPPORTS_PSYCHOFLOW_BLEND_MODES
				mat.SetEnumKeyword(BlendModes.SharedBlendMaterials.SHADER_ID_COLOR_OVERLAY_ENUM, BlendModes.SharedBlendMaterials.SHADER_KEYWORD_COLOR_OVERLAY_ENUM, keyValue.spriteColorOverlayBlendMode);
#endif
				return mat;
			}
		}
	}
}