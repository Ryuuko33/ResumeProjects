//#define DONT_NEED_TO_UPDATE_UNSCALED_TIME
using System.Collections.Generic;
using UnityEngine;
using Psychoflow.Util;

namespace Psychoflow.SSWaterReflection2D {
	/// <summary>
	/// The water parameter provider for <see cref="SSWaterReflectionRenderer"/> and <see cref="SpriteSelfWaterReflection"/>.
	/// </summary>
	[ExecuteAlways]
	public class SSWaterReflectionProvider : MonoBehaviour {
		#region Serialized Fields
		//[Header("General")]
		[Tooltip("When true, use the unscaled delta time to simulate the Particle System. Otherwise, use the scaled delta time.")]
		[SerializeField] private bool m_UseUnscaledTime = false;
		[Tooltip("The position Y of reflection plane in *Local* space. It's usually 0 that the reflection plane is just right on this object position.")]
		[SerializeField] private float m_ReferenceLocalY = 0f;
		[Tooltip("The fading range that reflection is close to screen boundary. (Range in [0, 1]) The higher value, the larger range of fading reflection. 0 means no fading.")]
		[SerializeField] [Range(0, 1)] private float m_ScreenBoundFadeOut = 0.1f;
		[Tooltip("The scale of water reflection. It affects displacement and foam size.")]
		[SerializeField] [Range(0.1f, 10f)] private float m_WaterScale = 1f;
		[Tooltip("Displacement mode determines the shape of reflection displacement and foam.")]
		[SerializeField] DisplacementModes m_DisplacementMode = DisplacementModes.Texture;
		//[Header("Displacement Texture")]
		[Tooltip("The texture that is only used when DisplacementMode ==  DisplacementModes.Texture")]
		[SerializeField] private Texture m_DisplacementTexture = null;
		[Tooltip("The speed of displacement moving horizontally.")]
		[SerializeField] [Range(-10f, 10f)] private float m_DisplacementSpeed = 0.1f;
		[Tooltip("When true, the reflection displacement will be based on not only DisplacementTexture but also DisplacementSecondTexture.")]
		[SerializeField] private bool m_UseSecondDisplacementTexture = false;
		[Tooltip("The second displacement texture.")]
		[SerializeField] private Texture m_DisplacementSecondTexture = null;
		[Tooltip("The speed offset of second displacement texture. It's an offset value from DisplacementSpeed to make the water look more complicated.")]
		[SerializeField] [Range(-5f, 5f)] private float m_DisplacementSecondSpeedOffset = 0.05f;
		//[Header("Voroni Displacement(Experiential)")]
		[Tooltip("Offset value for points")]
		[SerializeField] private float m_VoronoiAngleOffset = 1f;
		[Tooltip("The speed of changing voronoi angle.")]
		[SerializeField] private float m_VoronoiAngleSpeed = 1f;
		[Tooltip("Density of cells generated.")]
		[SerializeField] private float m_VoronoiCellDensity = 14f;
		[Tooltip("Power value is a factor of mathamatic power function in computing voronoi noise. It affects the fading level of vornoi cell. It can be understood as the inner size of cell.")]
		[SerializeField] [LogRange(0.1f, 10f, 10)] private float m_VoronoiPowerValue = 1.4f;
		//[Header("Water Reflection")]
		[Tooltip("The color tint for reflection.")]
		[SerializeField] private Color m_ReflectionTint = new Color(0.7f, 0.8f, 1, 0.75f);
		[Tooltip("The amount(strength) of reflection displacement. Higher value makes more distorted result.")]
		[SerializeField] [Range(0, 1f)] private float m_ReflectionDisplacementAmount = 0.1f;
		[Tooltip("The weight of foam in reflection. Weight is a multplier of the foam opacity.")]
		[SerializeField] [Range(0, 1f)] private float m_ReflectionFoamWeight = 1.0f;
		//[Header("Water Refraction")]
		[Tooltip("The color tint for refraction.")]
		[SerializeField] private Color m_RefractionTint = new Color(0.7f, 0.8f, 1, 1);
		[Tooltip("The amount(strength) of refraction displacement. Higher value makes more distorted result.")]
		[SerializeField] [Range(0, 1f)] private float m_RefractionDisplacementAmount = 0.1f;
		[Tooltip("The weight of foam in refraction. Weight is a multplier of the foam opacity.")]
		[SerializeField] [Range(0, 1f)] private float m_RefractionFoamWeight = 0.5f;
		//[Header("Foam")]
		[Tooltip("Foam color")]
		[SerializeField] private Color m_FoamColor = new Color(1, 1, 1, 0.3f);
		[Tooltip("The blend mode of foam that blends with reflection and refraction.")]
		[SerializeField] private FoamBlendMode m_FoamBlendMode = FoamBlendMode.Normal;
		[Tooltip("The threshold determines how much displacement would make foam. Range in [0, 1].")]
		[SerializeField] [Range(0, 1f)] private float m_FoamThreshold = 0.5f;
		[Tooltip("Edge Threshold is used to simulate the situation that there is more foam closing shore than further.")]
		[SerializeField] [Range(0, 30f)] private float m_FoamEdgeThreshold = 2f;
		//[Header("Perspective Correction")]
		[Tooltip("Perspective correction is used to fake this 2D sprite as 3D plane lying on the XZ-plane and facing Y-axis in perspective view. It would makes the water look different in different camera position.")]
		[SerializeField] private bool m_EnablePerspectiveCorrection = true;
		[Tooltip("How much the perspective correction is. Higher value gets stronger bevel.")]
		[SerializeField] [Range(0, 1f)] private float m_PerspectiveCorrectionStrength = 0.5f;
		[Tooltip("How much difference of correction strength between the closest(reflection baseline) and the furthest (PerspectiveCorrectionMaxLength).")]
		[SerializeField] [Range(0, 1f)] private float m_PerspectiveCorrectionContrast = 0.5f;
		[Tooltip("The maximum distance of perspective correction.")]
		[SerializeField] [Range(0, 30f)] private float m_PerspectiveCorrectionMaxLength = 3f;
		[Tooltip("Tilt is used to fake not only 3D plane lying on XZ-plane but also the plane that does not go to the screen side, which means you are able to see the object inside the water in screen.")]
		[SerializeField] [LogRange(0.1f, 10f, 10)] private float m_PerspectiveReflectionTiltY = 1f;
		
		#endregion

		/// <summary>
		/// When true, use the unscaled delta time to simulate the Particle System. Otherwise, use the scaled delta time.
		/// This is useful for playing effects whilst the game is paused and <see cref="Time.timeScale"/> is set to zero.
		/// <br/>
		/// > [!TIP]
		/// > There is a tiny optimization way:
		/// > You can **UNCOMMENT** the SSWaterReflectionProvider.cs:Line 1 to define the DONT_NEED_TO_UPDATE_UNSCALED_TIME macro
		/// > if you don't need unscaled time feature or you already or you already assign _UnscaledTime by yourself.
		/// </summary>
		public bool UseUnscaledTime {
			get => m_UseUnscaledTime;
			set {
				if (m_UseUnscaledTime == value) {
					return;
				}
				m_UseUnscaledTime = value;
				NotifyRendererOnParameterChanged();
			}
		}

		/// <summary>
		/// The position Y of reflection plane in *Local* space. It's usually 0 that the reflection plane is just right on this object position.
		/// </summary>
		public float ReferenceLocalY { 
			get => m_ReferenceLocalY; 
			set {
				if (m_ReferenceLocalY == value) {
					return;
				}
				m_ReferenceLocalY = value;
				NotifyRendererOnParameterChanged();
			} 
		}

		/// <summary>
		/// The position Y of reflection plane in *World* space. It's a readonly property that resulted by <see cref="ReferenceLocalY"/>.
		/// </summary>
		public float ReferenceWorldY {
			get {
				Vector3 referencePoint = transform.TransformPoint(new Vector3(0, ReferenceLocalY, 0));
				return referencePoint.y;
			}
		}

		/// <summary>
		/// The fading range that reflection is close to screen boundary. (Range in [0, 1])
		/// The higher value, the larger range of fading reflection. 0 means no fading.
		/// </summary>
		public float ScreenBoundFadeOut { 
			get => m_ScreenBoundFadeOut; 
			set {
				if (m_ScreenBoundFadeOut == value) {
					return;
				}
				m_ScreenBoundFadeOut = Mathf.Clamp01(value);
				NotifyRendererOnParameterChanged();
			}
		}

		/// <summary>
		/// The scale of water reflection. It affects displacement and foam size.
		/// </summary>
		public float WaterScale { 
			get => m_WaterScale; 
			set {
				if (m_WaterScale == value) {
					return;
				}
				m_WaterScale = value;
				NotifyRendererOnParameterChanged();
			}
		}

		/// <summary>
		/// Displacement mode determines the shape of reflection displacement and foam.
		/// </summary>
		public DisplacementModes DisplacementMode {
			get => m_DisplacementMode;
			set {
				if (m_DisplacementMode == value) { return; }
				m_DisplacementMode = value;
				NotifyRendererOnParameterChanged();
			}
		}

		/// <summary>
		/// The speed of displacement moving horizontally.
		/// </summary>
		public float DisplacementSpeed {
			get => m_DisplacementSpeed;
			set {
				if (m_DisplacementSpeed == value) {
					return;
				}
				m_DisplacementSpeed = value;
				NotifyRendererOnParameterChanged();
			}
		}

		/// <summary>
		/// The texture that is only used when <see cref="DisplacementMode"/> == <see cref="DisplacementModes.Texture"/>.
		/// <br/>
		/// > [!TIP]
		/// > The package includes 9 displacement textures in Textures folder!
		/// </summary>
		public Texture DisplacementTexture { 
			get => m_DisplacementTexture; 
			set {
				if (m_DisplacementTexture == value) {
					return;
				}
				m_DisplacementTexture = value;
				NotifyRendererOnParameterChanged();
			}
		}

		/// <summary>
		/// When true, the reflection displacement will be based on not only <see cref="DisplacementTexture"/> but also <see cref="DisplacementSecondTexture"/>.
		/// <br/>
		/// And you are able to have second texture with different speed by <see cref="DisplacementSecondSpeedOffset"/> so that the water will look more complicated.
		/// <br/>
		/// It's only used when <see cref="DisplacementMode"/> == <see cref="DisplacementModes.Texture"/>.
		/// </summary>
		public bool UseSecondDisplacementTexture { 
			get => m_UseSecondDisplacementTexture; 
			set {
                if (m_UseSecondDisplacementTexture == value) {
                    return;
                }
                m_UseSecondDisplacementTexture = value;
				NotifyRendererOnParameterChanged();
			}
		}

		/// <summary>
		/// The second displacement texture.
		/// <br/>
		/// It's only used when <see cref="DisplacementMode"/> == <see cref="DisplacementModes.Texture"/> and <see cref="UseSecondDisplacementTexture"/> == true.
		/// <br/>
		/// > [!TIP]
		/// > The package includes 9 displacement textures in Textures folder!
		/// </summary>
		public Texture DisplacementSecondTexture { 
			get => m_DisplacementSecondTexture; 
			set {
                if (m_DisplacementSecondTexture == value) {
                    return;
                }
                m_DisplacementSecondTexture = value;
				NotifyRendererOnParameterChanged();
			}
		}

		/// <summary>
		/// The speed offset of second displacement texture. It's an offset value from <see cref="DisplacementSpeed"/> to make the water look more complicated.
		/// <br/>
		/// For example, DisplacementSpeed = 1f and DisplacementSecondSpeedOffset = 0.5f. The displacement speed of second texture is 1.5f.
		/// <br/>
		/// It's only used when <see cref="DisplacementMode"/> == <see cref="DisplacementModes.Texture"/> and <see cref="UseSecondDisplacementTexture"/> == true.
		/// </summary>
		public float DisplacementSecondSpeedOffset {
			get => m_DisplacementSecondSpeedOffset;
			set {
				if (m_DisplacementSecondSpeedOffset == value) {
					return;
				}
				m_DisplacementSecondSpeedOffset = value;
				NotifyRendererOnParameterChanged();
			}
		}

		#region Voronoi 
		/// <summary>
		/// Is current displacement mode <see cref="DisplacementModes.Voronoi"/>?
		/// </summary>
		public bool UseVoronoiNoiseDisplacement {
			get => this.DisplacementMode == DisplacementModes.Voronoi;
		}

		/// <summary>
		/// Offset value for points
		/// </summary>
		public float VoronoiAngleOffset {
			get => m_VoronoiAngleOffset;
			set {
				if (m_VoronoiAngleOffset == value) {
					return;
				}
				m_VoronoiAngleOffset = value;
				NotifyRendererOnParameterChanged();
			}
		}

		/// <summary>
		/// The speed of changing voronoi angle.
		/// </summary>
		public float VoronoiAngleSpeed {
			get => m_VoronoiAngleSpeed;
			set {
				if (m_VoronoiAngleSpeed == value) {
					return;
				}
				m_VoronoiAngleSpeed = value;
				NotifyRendererOnParameterChanged();
			}
		}

		/// <summary>
		/// Density of cells generated.
		/// </summary>
		public float VoronoiCellDensity {
			get => m_VoronoiCellDensity;
			set {
				if (m_VoronoiCellDensity == value) {
					return;
				}
				m_VoronoiCellDensity = value;
				NotifyRendererOnParameterChanged();
			}
		}

		/// <summary>
		/// Power value is a factor of mathamatic power function in computing voronoi noise.
		/// It affects the fading level of vornoi cell. It can be understood as the inner size of cell.
		/// <br/>
		/// The higher value, more area of 0% strength. The lower value, more area of 100% strength.
		/// </summary>
		public float VoronoiPowerValue {
			get => m_VoronoiPowerValue;
			set {
				if (m_VoronoiPowerValue == value) {
					return;
				}
				m_VoronoiPowerValue = value;
				NotifyRendererOnParameterChanged();
			}
		}
		#endregion

		/// <summary>
		/// The color tint for reflection.
		/// </summary>
		public Color ReflectionTint { 
			get => m_ReflectionTint; 
			set {
				if (m_ReflectionTint == value) {
					return;
				}
				m_ReflectionTint = value;
				NotifyRendererOnParameterChanged();
			} 
		}

		/// <summary>
		/// The amount(strength) of reflection displacement. Higher value makes more distorted result.
		/// </summary>
		public float ReflectionDisplacementAmount { 
			get => m_ReflectionDisplacementAmount; 
			set {
                if (m_ReflectionDisplacementAmount == value) {
                    return;
                }
                m_ReflectionDisplacementAmount = value;
				NotifyRendererOnParameterChanged();
            } 
		}

		/// <summary>
		/// The weight of foam in reflection. Weight is a multplier of the foam opacity.
		/// Default is 1f which shows 100% foam in reflection.
		/// <br/>
		/// 
		/// > [!WARNING] Weight is **not the certain value of foam opacity**. 
		/// <br/>
		/// > It's a **multiplier** of opacity.
		/// <br/>
		/// > If the <see cref="FoamColor"/>.a is 0.5f and reflection weight is 1f, the foam opacity is still 0.5f.
		/// </summary>
		public float ReflectionFoamWeight { 
			get => m_ReflectionFoamWeight; 
			set {
                if (m_ReflectionFoamWeight == value) {
                    return;
                }
                m_ReflectionFoamWeight = value;
                NotifyRendererOnParameterChanged();
            }
		}

		/// <summary>
		/// The color tint for refraction.
		/// </summary>
		public Color RefractionTint { 
			get => m_RefractionTint; 
			set {
                if (m_RefractionTint == value) {
                    return;
                }
                m_RefractionTint = value;
                NotifyRendererOnParameterChanged();
            } 
		}

		/// <summary>
		/// The amount(strength) of refraction displacement. Higher value makes more distorted result.
		/// </summary>
		public float RefractionDisplacementAmount { 
			get => m_RefractionDisplacementAmount;
			set {
                if (m_RefractionDisplacementAmount == value) {
                    return;
                }
                m_RefractionDisplacementAmount = value;
                NotifyRendererOnParameterChanged();
            }
		}

		/// <summary>
		/// The weight of foam in refraction. Weight is a multplier of the foam opacity.
		/// Default is 1f which shows 100% foam in refraction.
		/// <br/>
		/// > [!WARNING] Weight is **not the certain value of foam opacity**. 
		/// <br/>
		/// > It's a **multiplier** of opacity.
		/// <br/>
		/// > If the <see cref="FoamColor"/>.a is 0.5f and refraction weight is 1f, the foam opacity is still 0.5f.
		/// </summary>
		public float RefractionFoamWeight { 
			get => m_RefractionFoamWeight; 
			set {
                if (m_RefractionFoamWeight == value) {
                    return;
                }
                m_RefractionFoamWeight = value;
                NotifyRendererOnParameterChanged();
            } 
		}

		/// <summary>
		/// Foam color
		/// </summary>
		public Color FoamColor { 
			get => m_FoamColor; 
			set {
                if (m_FoamColor == value) {
                    return;
                }
                m_FoamColor = value;
                NotifyRendererOnParameterChanged();
            } 
		}

		/// <summary>
		/// The blend mode of foam that blends with reflection and refraction.
		/// <br/>
		/// It works like the layer blend mode in Photoshop.
		/// </summary>
		public FoamBlendMode FoamBlendMode {
			get => m_FoamBlendMode;
			set {
				if (m_FoamBlendMode == value) {
					return;
				}
				m_FoamBlendMode = value;
				NotifyRendererOnParameterChanged();
			}
		}

		/// <summary>
		/// The threshold determines how much displacement would make foam. Range in [0, 1].
		/// <br/>
		/// Higher threshold makes less foam.
		/// </summary>
		public float FoamThreshold { 
			get => m_FoamThreshold; 
			set {
                if (m_FoamThreshold == value) {
                    return;
                }
                m_FoamThreshold = value;
                NotifyRendererOnParameterChanged();
            } 
		}

		/// <summary>
		/// Edge Threshold is used to simulate the situation that there is more foam closing shore than further.
		/// <br/>
		/// It is the **distance** from reflection baseline. So higher value results larger range of edge foam.
		/// </summary>
		public float FoamEdgeThreshold { 
			get => m_FoamEdgeThreshold; 
			set {
                if (m_FoamEdgeThreshold == value) {
                    return;
                }
                m_FoamEdgeThreshold = value;
                NotifyRendererOnParameterChanged();
            } 
		}

		/// <summary>
		/// Perspective correction is used to fake this 2D sprite as 3D plane lying on the XZ-plane and facing Y-axis in perspective view.
		/// It would makes the water look different in different camera position.
		/// <br/>
		/// The basic idea is from [Kingdom implementation](https://forums.tigsource.com/index.php?topic=40539.20).
		/// </summary>
		public bool EnablePerspectiveCorrection { 
			get => m_EnablePerspectiveCorrection; 
			set {
                if (m_EnablePerspectiveCorrection == value) {
                    return;
                }
                m_EnablePerspectiveCorrection = value;
                NotifyRendererOnParameterChanged();
            }
		}
		/// <summary>
		/// How much the perspective correction is. Higher value gets stronger bevel.
		/// <br/>
		/// The value actually determines the **max strength** of correction at the max length(<see cref="PerspectiveCorrectionMaxLength"/>).
		/// </summary>
		public float PerspectiveCorrectionStrength { 
			get => m_PerspectiveCorrectionStrength; 
			set {
                if (m_PerspectiveCorrectionStrength == value) {
                    return;
                }
                m_PerspectiveCorrectionStrength = value;
                NotifyRendererOnParameterChanged();
            }
		}

		/// <summary>
		/// How much difference of correction strength between the closest(reflection baseline) and the furthest(<see cref="PerspectiveCorrectionMaxLength"/>).
		/// It can be understood as **slope** or **gradient** of perspective correction.
		/// </summary>
		public float PerspectiveCorrectionContrast { 
			get => m_PerspectiveCorrectionContrast; 
			set {
                if (m_PerspectiveCorrectionContrast == value) {
                    return;
                }
                m_PerspectiveCorrectionContrast = value;
                NotifyRendererOnParameterChanged();
            }
		}

		/// <summary>
		/// The maximum distance of perspective correction.
		/// The further gets same correction strength as the max length gets.
		/// </summary>
		public float PerspectiveCorrectionMaxLength {
			get => m_PerspectiveCorrectionMaxLength; 
			set {
                if (m_PerspectiveCorrectionMaxLength == value) {
                    return;
                }
                m_PerspectiveCorrectionMaxLength = value;
                NotifyRendererOnParameterChanged();
            }
		}

		/// <summary>
		/// Tilt is used to fake not only 3D plane lying on XZ-plane but also the plane that does not go to the screen side,
		/// which means you are able to see the object inside the water in screen.
		/// Therefore, the reflection is not the perfect 1:1 mapping when this value does not equals 1f.
		/// </summary>
		public float PerspectiveReflectionTiltY { 
			get => m_PerspectiveReflectionTiltY; 
			set {
                if (m_PerspectiveReflectionTiltY == value) {
                    return;
                }
                m_PerspectiveReflectionTiltY = value;
                NotifyRendererOnParameterChanged();
            }
		}

		private HashSet<ISSWaterReflection2DRenderer> m_Water2DRenderers = new HashSet<ISSWaterReflection2DRenderer>();

		internal void RegisterWaterRenderer(ISSWaterReflection2DRenderer renderer) {
			m_Water2DRenderers.Add(renderer);
		}

		internal void RemoveWaterRenderer(ISSWaterReflection2DRenderer renderer) {
			m_Water2DRenderers.Remove(renderer);
		}

		internal void ClearWaterRenderers() {
			m_Water2DRenderers.Clear();
		}

		private void LateUpdate() {
			if (transform.hasChanged) {
				transform.hasChanged = false;
				NotifyRendererOnParameterChanged();
			}
#if !DONT_NEED_TO_UPDATE_UNSCALED_TIME
			UpdateShaderGlobalProperties();
#endif
		}

		private void OnValidate() {
			NotifyRendererOnParameterChanged();
		}

		private void NotifyRendererOnParameterChanged() {
			foreach (var water2DRenderer in m_Water2DRenderers) {
				water2DRenderer.OnParameterChanged();
			}
		}

		#region Static setup shader global properties
#if !DONT_NEED_TO_UPDATE_UNSCALED_TIME
		private static int s_ShaderUnscaledTimeLastFrameNumber = -1;
		private static void UpdateShaderGlobalProperties() {
			if (s_ShaderUnscaledTimeLastFrameNumber == Time.frameCount) {
				return;
			}
			float unscaledTime = Time.unscaledTime;
			Shader.SetGlobalVector(ShaderPropertyIDs.UnscaledTime, new Vector4(unscaledTime / 20f, unscaledTime, unscaledTime * 2, unscaledTime * 3));
			s_ShaderUnscaledTimeLastFrameNumber = Time.frameCount;
		}
#endif
		#endregion

		#region EDITOR
#if UNITY_EDITOR
		internal const float GizmoCubeSize = 0.4f;
		internal void DrawGizmoCubeAsReflectionLine() {
			const float IconSize = 0.4f;
			Gizmos.color = Color.red;
			Gizmos.matrix = Matrix4x4.TRS(this.transform.position, Quaternion.AngleAxis(45f, Vector3.forward), Vector3.one);
			Gizmos.DrawCube(Vector2.zero, new Vector3(IconSize, IconSize, IconSize));
			Gizmos.matrix = Matrix4x4.identity;
		}

		private void OnDrawGizmosSelected() {
			const float LineLength = 1000f;
			if (SSWR2DPrefs.Instance.gizmoShowReflectionLine) {
				DrawGizmoCubeAsReflectionLine();
				Vector3 referencePoint = new Vector3(transform.position.x, ReferenceWorldY, transform.position.z);
				Vector3 point1 = referencePoint - new Vector3(LineLength * 0.5f, 0f, 0f);
				Vector3 point2 = referencePoint + new Vector3(LineLength * 0.5f, 0f, 0f);
				Gizmos.color = Color.red;
				Gizmos.DrawLine(point1, point2);
			}

			if (SSWR2DPrefs.Instance.gizmoShowFoamEdgeThreshold) {
				Gizmos.color = Color.yellow;
				Vector3 foamEdgePoint = new Vector3(transform.position.x, ReferenceWorldY, transform.position.z) - new Vector3(0, FoamEdgeThreshold, 0);
				Vector3 point1 = foamEdgePoint - new Vector3(LineLength * 0.5f, 0f, 0f);
				Vector3 point2 = foamEdgePoint + new Vector3(LineLength * 0.5f, 0f, 0f);
				Gizmos.DrawLine(point1, point2);
			}

			if (SSWR2DPrefs.Instance.gizmoShowPerspectiveMaxLength) {
				Gizmos.color = Color.green;
				Vector3 perspectivePoint = new Vector3(transform.position.x, ReferenceWorldY, transform.position.z) - new Vector3(0, PerspectiveCorrectionMaxLength, 0);
				Vector3 point1 = perspectivePoint - new Vector3(LineLength * 0.5f, 0f, 0f);
				Vector3 point2 = perspectivePoint + new Vector3(LineLength * 0.5f, 0f, 0f);
				Gizmos.DrawLine(point1, point2);
			}
		}
#endif
		#endregion
	}
}