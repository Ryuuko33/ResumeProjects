using UnityEngine;
using Psychoflow.Util;

namespace Psychoflow.SSWaterReflection2D {
	/// <summary>
	/// Sprite Self Reflection is a way to reflect the sprite renderer itself vertically. 
	/// It's quite useful for Top-down games and Retro RPG games.
	/// </summary>
	[ExecuteAlways]
	public class SpriteSelfWaterReflection : MonoBehaviour, ISSWaterReflection2DRenderer {


		#region Serialize Fields
		[Tooltip("The reflection provider that provides the reflection parameters.")]
		[SerializeField] private SSWaterReflectionProvider m_ReflectionProvider = null;
		[Tooltip("The offset distance of reflection reference plane(line). It's the fixed distance from the origin of the object.")]
		[SerializeField] [Range(-50f, 50f)] private float m_RefelectionReferenceOffsetY = 0f;
		[Tooltip("Does lock reflection reference offset Y?")]
		[SerializeField] private bool m_LockReflectionReferenceY = false;

		[Tooltip("The depth offset of the reflection. It's used to determine the render order.")]
		[SerializeField] private float m_ReflectionDepthOffset = 0f;
		[Tooltip("The way of determining Sorting Layer and Order In Layer of the reflection's SpriteRenderer")]
		[SerializeField] private ReflectionSortingOrders m_ReflectionSortingOrder = ReflectionSortingOrders.OrderInLayerOffset;
		[Tooltip("The offset of OrderInLayer from the original SpriteRenderer that will be reflected.")]
		[SerializeField] private int m_OrderInLayerOffset = 0;
		[Tooltip("The custom specific Sorting Layer for reflection renderer.")]
		[SerializeField][SortingLayerIntProperty] private int m_CustomSortingLayerID = 0;
		[Tooltip("The custom specific Order In Layer for reflection renderer.")]
		[SerializeField] private int m_CustomOrderInLayer = 0;
		[Tooltip("The custom Sprite Mask Interaction for reflection renderer.")]
		[SerializeField] private SpriteMaskInteraction m_ReflectionMaskInteraction = SpriteMaskInteraction.None;

		[Tooltip("The dirty check flags that determines what should check every update")]
		[SerializeField] private PropertyCheckingFlag m_PropertyCheckingFlag = PropertyCheckingFlag.Transform_Enabled;

#if UNITY_EDITOR
		[Header("Debug")]
		[Tooltip("When uncheck, the reflection instance will show in hierarchy for debug. Only works in Unity Editor.")]
		[SerializeField] private bool m_HideReflectionInstanceInHierarchy = true;
#endif
		#endregion

		#region Public Properties
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
				CheckWaterPlaneChanged();
			}
		}

		/// <summary>
		/// The offset distance of reflection reference plane(line). It's the fixed distance from the origin of the object.
		/// </summary>
		public float RefelectionReferenceOffsetY {
			get {
				return m_RefelectionReferenceOffsetY;
			}
			set {
				m_RefelectionReferenceOffsetY = value;
				UpdateReflectionRenderersPosition();
			}
		}

		/// <summary>
		/// Does lock reflection reference offset Y?
		/// If locked, the reflection reference Y is *NOT* affected by the position of object itself and <see cref="RefelectionReferenceOffsetY"/> anymore.
		/// On the other hand, the reference Y is fixed.
		/// <br/>
		/// It'll be useful for floaty foreground obejcts in 2D perspective platformer game.
		/// </summary>
		public bool LockReflectionReferenceY {
			get => m_LockReflectionReferenceY;
			set {
				if (m_LockReflectionReferenceY == value) { return; }
				if (value) {
					m_LockedWorldPositionY = ReflectionAnchor.y;
				}
				m_LockReflectionReferenceY = value;
			}
		}
		[SerializeField] private float m_LockedWorldPositionY;

		/// <summary>
		/// The depth offset of the reflection. It's used to determine the render order.
		/// <br/>
		/// For example, the reflected renderer is at (0, 0, 1f) and ReflectionDepthOffset is 0.001f.
		/// The reflection renderer will be at (0, 0, 1.001f) which is further than the reflected from regular 2d camera.
		/// So it'll be under the reflected which is a regular case.
		/// </summary>
		public float ReflectionDepthOffset {
			get {
				return m_ReflectionDepthOffset;
			}
			set {
				if (m_ReflectionDepthOffset == value) { return; }
				m_ReflectionDepthOffset = value;
				UpdateReflectionRenderersPosition();
			}
		}

		/// <summary>
		/// The way of determining Sorting Layer and Order In Layer of the reflection's <see cref="SpriteRenderer"/>.
		/// </summary>
		public ReflectionSortingOrders ReflectionSortingOrder {
			get {
				return m_ReflectionSortingOrder;
			}
			set {
				if (m_ReflectionSortingOrder == value) { return; }
				m_ReflectionSortingOrder = value;
				UpdateReflectionRendererSortingOrder();
			}
		}

		/// <summary>
		/// The offset of OrderInLayer from the original <see cref="SpriteRenderer"/> that will be reflected.
		/// It works only if <see cref="ReflectionSortingOrder"/> == <see cref="ReflectionSortingOrders.OrderInLayerOffset"/>.
		/// <br/>
		/// E.g. If the reflected <see cref="SpriteRenderer"/> has SortingLayer(3) and OrderInLayer(10) and OrderInLayerOffset(5), 
		/// the reflection renderer has SortingLayer(3) and OrderInLayer(15).
		/// </summary>
		public int OrderInLayerOffset {
			get {
				return m_OrderInLayerOffset;
			}
			set {
				if (m_OrderInLayerOffset == value) { return; }
				m_OrderInLayerOffset = value;
				UpdateReflectionRendererSortingOrder();
			}
		}

		/// <summary>
		/// The custom specific <see cref="Renderer.sortingLayerID"/> to reflection renderer.
		/// It works only if <see cref="ReflectionSortingOrder"/> == <see cref="ReflectionSortingOrders.CustomSortingOrder"/>.
		/// </summary>
		public int CustomSortingLayerID {
			get {
				return m_CustomSortingLayerID;
			}
			set {
				if (m_CustomSortingLayerID == value) { return; }
				m_CustomSortingLayerID = value;
				UpdateReflectionRendererSortingOrder();
			}
		}

		/// <summary>
		/// The custom specific OrderInLayer (<see cref="Renderer.sortingOrder"/>) to reflection renderer.
		/// It works only if <see cref="ReflectionSortingOrder"/> == <see cref="ReflectionSortingOrders.CustomSortingOrder"/>.
		/// </summary>
		public int CustomOrderInLayer {
			get {
				return m_CustomOrderInLayer;
			}
			set {
				if (m_CustomOrderInLayer == value) { return; }
				m_CustomOrderInLayer = value;
				UpdateReflectionRendererSortingOrder();
			}
		}

		/// <summary>
		/// The <see cref="SpriteMaskInteraction"/> of reflection's <see cref="SpriteRenderer"/>.
		/// Reflection renderer doesn't follow the <see cref="SpriteMaskInteraction"/> value of the original reflected sprite renderer, 
		/// it always be this value whenever.
		/// </summary>
		public SpriteMaskInteraction ReflectionMaskInteraction {
			get {
				return m_ReflectionMaskInteraction;
			}
			set {
				if (m_ReflectionMaskInteraction == value) { return; }
				m_ReflectionMaskInteraction = value;
				UpdateReflectionRenderersMaskInteraction();
			}
		}

		/// <summary>
		/// The dirty check flags that determines what should check every update.
		/// </summary>
		public PropertyCheckingFlag PropertyCheckingFlag {
			get {
				return m_PropertyCheckingFlag;
			}
			set {
				if (m_PropertyCheckingFlag == value) { return; }
				m_PropertyCheckingFlag = value;
			}
		}

		private SpriteRenderer[] m_Renderers;
		/// <summary>
		/// Cached reflection renderers.
		/// </summary>
		private SpriteRenderer[] _Renderers {
			get {
				if (m_Renderers == null) {
					SetupOwnRenderers();
				}
				return m_Renderers;
			}
		}

		/// <summary>
		/// The bounds of all reflected sprite renderers.
		/// It will recalculate the bound every time you get the property.
		/// </summary>
		private Bounds Bounds {
			get {
				Bounds bounds = new Bounds(this.transform.position, Vector3.zero);
				for (int i = 0; i < _Renderers.Length; i++) {
					bounds.Encapsulate(_Renderers[i].bounds);
				}

				return bounds;
			}
		}

		private Vector2 ReflectionAnchor {
			get {
				Vector2 position = (Vector2)this.transform.position;
				if (LockReflectionReferenceY) {
					// 直接套用已經鎖定的y值
					position.y = m_LockedWorldPositionY;
				} else {
					position += new Vector2(0, RefelectionReferenceOffsetY).GetScale(this.transform.lossyScale);
				}
				return position;
			}
		}
		#endregion

		private struct ReflectionObject {
			public GameObject gameObject;
			public SpriteRenderer renderer;
			public MaterialPropertyBlock materialPropertyBlock;
			public SpriteRenderer originalSpriteRenderer;

			public ReflectionObject(SpriteRenderer renderer, SpriteRenderer originalSpriteRenderer) {
				this.renderer = renderer;
				this.gameObject = renderer.gameObject;
				this.materialPropertyBlock = new MaterialPropertyBlock();
				this.originalSpriteRenderer = originalSpriteRenderer;
			}

			public MaterialPropertyBlock StartMaterialPropertyBlock() {
				if (this.materialPropertyBlock == null) {
					this.materialPropertyBlock = new MaterialPropertyBlock();
				}
				originalSpriteRenderer.GetPropertyBlock(this.materialPropertyBlock);
				return this.materialPropertyBlock;
			}

			public void EndMaterialPropertyBlock() {
				renderer.SetPropertyBlock(this.materialPropertyBlock);
			}

			public void UpdateRendererAsOriginal() {
				CopySpriteRenderer(originalSpriteRenderer, renderer);
			}
		}

		private ReflectionObject[] m_ReflectionObjects;

		private SSWaterReflectionProvider m_LastReflectionProvider = null;
		private float m_LastOffsetY = 0f;
		private bool m_ForceInitRenderersLately = false;

		#region LateUpdate checks dirty.
		void LateUpdate() {
			CheckDirty();
		}

		/// <summary>
		/// To check the parameter is dirty and force to update the materials and renderer stuffs.
		/// </summary>
		void CheckDirty() {
			if (m_ForceInitRenderersLately) {
				m_ForceInitRenderersLately = false;
				ClearAndInitReflectionRenderers();
				return;
			}

			CheckWaterPlaneChanged(); // Always check water plane reference.

			if ((m_PropertyCheckingFlag & PropertyCheckingFlag.Transform) != 0) {
				if (CheckPositionChanged()) {
					UpdateReflectionRenderersPosition();
				}
			}
			bool shouldCheckSpriteRendererEnabled = (m_PropertyCheckingFlag & PropertyCheckingFlag.SpriteRendererEnabled) != 0;
			bool shouldCheckSpriteRendererSortingOrders = (m_PropertyCheckingFlag & PropertyCheckingFlag.SpriteRendererSortingOrders) != 0;
			bool shouldCheckSpriteRendererOtherProperties = (m_PropertyCheckingFlag & PropertyCheckingFlag.SpriteRendererOtherProperties) != 0;
			CheckSpriteRendererPropertiesChanged(shouldCheckSpriteRendererEnabled, shouldCheckSpriteRendererSortingOrders, shouldCheckSpriteRendererOtherProperties);
		}

		#region Check dirty methods
		private void CheckWaterPlaneChanged() {
			if (m_LastReflectionProvider != m_ReflectionProvider) {
				if (m_LastReflectionProvider) {
					m_LastReflectionProvider.RemoveWaterRenderer(this);
				}
				if (m_ReflectionProvider) {
					m_ReflectionProvider.RegisterWaterRenderer(this);
				}
				m_LastReflectionProvider = m_ReflectionProvider;
				// Update materials.
				UpdateReflecitionRenderersMaterial();
			}
		}

		/// <summary>
		/// Check if the sprite renderer changed properties, and up to date reflection renderer if it did.
		/// </summary>
		/// <param name="checkEnabled">Should check <see cref="Behaviour.enabled"/> and <see cref="GameObject.activeInHierarchy"/> to update reflection renderer's enabled?</param>
		/// <param name="checkSortingOrders">Should check <see cref="Renderer.sortingOrder"/> and <see cref="Renderer.sortingLayerID"/> to update reflection renderer's?</param>
		/// <param name="checkOtherProperties">Should check other all properties of <see cref="SpriteRenderer"/> to update reflection renderer's?</param>
		public void CheckSpriteRendererPropertiesChanged(bool checkEnabled, bool checkSortingOrders, bool checkOtherProperties) {
			if (m_Renderers == null || m_Renderers.Length <= 0) { 
				return;
			}
			// CustomSortingOrder doesn't need to check original sorting order.
			if (checkSortingOrders && ReflectionSortingOrder == ReflectionSortingOrders.CustomSortingOrder) {
				checkSortingOrders = false;
			}
			if (!checkEnabled && !checkSortingOrders && !checkOtherProperties) {
				// Don't check anything.
				return;
			}

			// Trace all renderers.
			for (int i = 0; i < m_Renderers.Length; i++) {
				if (!m_Renderers[i]) {
					if (m_ReflectionObjects[i].gameObject) {
						Destroy(m_ReflectionObjects[i].gameObject);
					}
					continue;
				}
				SpriteRenderer originalSpriteRenderer = m_ReflectionObjects[i].originalSpriteRenderer;
				SpriteRenderer reflectionInstance = m_ReflectionObjects[i].renderer;
				if (checkEnabled) {
					bool enabled = originalSpriteRenderer.enabled && originalSpriteRenderer.gameObject.activeInHierarchy;
					if (reflectionInstance.enabled != enabled) {
						reflectionInstance.enabled = enabled;
					}
				}

				if (checkSortingOrders) {
                    if (reflectionInstance.sortingLayerID != originalSpriteRenderer.sortingLayerID) {
						reflectionInstance.sortingLayerID = originalSpriteRenderer.sortingLayerID;
					}
					if (reflectionInstance.sortingOrder != originalSpriteRenderer.sortingOrder) {
						// Only when using OrderInLayerOffset, we check the sorting orders.
						reflectionInstance.sortingOrder = originalSpriteRenderer.sortingOrder + OrderInLayerOffset;
					}
				}

				if (checkOtherProperties) {
					CopySpriteRendererLazy(originalSpriteRenderer, reflectionInstance);
				}
			}
		}

		private bool CheckPositionChanged() {
			bool hasChanged = false;
			if (this.transform.hasChanged) {
				this.transform.hasChanged = false;
				hasChanged = true;
			}
			if (this.m_LastOffsetY != this.RefelectionReferenceOffsetY) {
				this.m_LastOffsetY = this.RefelectionReferenceOffsetY;
				hasChanged = true;
			}

			// Trace all renderers transform.
			for (int i = 0; i < m_Renderers.Length; i++) {
				if (!m_Renderers[i]) {
					if (m_ReflectionObjects[i].gameObject) {
						Destroy(m_ReflectionObjects[i].gameObject);
					}
					continue;
				}
				if (m_Renderers[i].transform.hasChanged) {
					m_Renderers[i].transform.hasChanged = false;
					hasChanged = true;
				}
			}
			return hasChanged;
		}
		#endregion
		#endregion


		// Called by m_ReflectionProvider.
		void ISSWaterReflection2DRenderer.OnParameterChanged() {
			UpdateReflecitionRenderersMaterial();
		}


		private void OnEnable() {
			//GameScheduler.Instance.OnGameLateUpdate.Add(OnLateUpdate);
			if (m_ReflectionProvider) {
				m_ReflectionProvider.RegisterWaterRenderer(this);
			}
			ClearAndInitReflectionRenderers();
		}

		private void OnDisable() {
			// 這部分Unity不會清要自己清
			if (m_ReflectionProvider) {
				m_ReflectionProvider.RemoveWaterRenderer(this);
			}
			ClearRelfectionRenderers();
		}

#if UNITY_EDITOR
		private void Reset() {
			if (!this.m_ReflectionProvider) {
				this.AssignReflectionProviderAsClosest();
			}
			SetReferenceOffsetYAsBoundBottom();
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
#endif

		/// <summary>
		/// Clear and find the children renderers and creating reflection renderers.
		/// This method can be used to make the reflection renderer up-to-date, it's like a Full Refresh method.
		/// </summary>
		public void ClearAndInitReflectionRenderers() {
			SetupOwnRenderers();
			CreateReflectionRenderers();
		}

		/// <summary>
		/// It's identical with <see cref="ClearAndInitReflectionRenderers"/> but it is not immediate. It will do it in the next LateUpdate().
		/// </summary>
		public void ClearAndInitReflectionRenderersLately() {
			m_ForceInitRenderersLately = true;
		}


		void SetupOwnRenderers() {
			ClearRelfectionRenderers();
			m_Renderers = this.GetComponentsInChildren<SpriteRenderer>();
		}

		void ClearRelfectionRenderers() {
			if (m_ReflectionObjects != null) {
				foreach (var reflectionObject in m_ReflectionObjects) {
					if (reflectionObject.gameObject) {
						if (Application.isPlaying) {
							Destroy(reflectionObject.gameObject);
						} else {
							DestroyImmediate(reflectionObject.gameObject);
						}
					}
				}
				m_ReflectionObjects = null;
			}
		}

		void CreateReflectionRenderers() {
			m_ReflectionObjects = new ReflectionObject[m_Renderers.Length];
			for (int i = 0; i < m_Renderers.Length; i++) {
				GameObject refObject = m_Renderers[i].gameObject;
				GameObject newObject = new GameObject($"{refObject.name} - SelfReflection (Don't Save)");
				newObject.transform.position = refObject.transform.position;
				newObject.transform.rotation = refObject.transform.rotation;
				newObject.transform.localScale = refObject.transform.lossyScale;
#if UNITY_EDITOR
				if (m_HideReflectionInstanceInHierarchy) {
					newObject.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor | HideFlags.HideInHierarchy | HideFlags.HideInInspector;
				} else {
					newObject.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
				}
#else
				newObject.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor | HideFlags.HideInHierarchy | HideFlags.HideInInspector;
#endif
				SpriteRenderer spriteRenderer = newObject.AddComponent<SpriteRenderer>();
				CopySpriteRenderer(m_Renderers[i], spriteRenderer);
				m_ReflectionObjects[i] = new ReflectionObject(spriteRenderer, m_Renderers[i]);

				ValidateMaterial(m_ReflectionObjects[i]);
			}
			UpdateReflectionRenderersPosition();
			UpdateReflectionRenderersMaskInteraction();
			UpdateReflectionRendererSortingOrder();
		}

		/// <summary>
		/// Make the reflection renderer's transform up-to-date.
		/// It's not only about position, but also rotation and scale.
		/// </summary>
		public void UpdateReflectionRenderersPosition() {
			if (m_ReflectionObjects != null) {
				for (int i = 0; i < m_ReflectionObjects.Length; i++) {
					if (!m_Renderers[i]) {
						if (m_ReflectionObjects[i].gameObject) {
							Destroy(m_ReflectionObjects[i].gameObject);
						}
						continue;
					}
					Vector3 reflectionPosition = ReflectPositionByY(m_Renderers[i].transform.position, this.ReflectionAnchor);
					reflectionPosition.z += m_ReflectionDepthOffset;
					m_ReflectionObjects[i].gameObject.transform.position = reflectionPosition;
					Vector3 scale = m_Renderers[i].transform.lossyScale;
					scale.x *= -1;
					m_ReflectionObjects[i].gameObject.transform.localScale = scale;
					Vector3 eulerAngle = m_Renderers[i].transform.eulerAngles;
					eulerAngle.z = (180f - eulerAngle.z);
					m_ReflectionObjects[i].gameObject.transform.eulerAngles = eulerAngle;
				}
			}
		}

		/// <summary>
		/// Make the reflection renderer's <see cref="SpriteRenderer.maskInteraction"/> up-to-date.
		/// </summary>
		public void UpdateReflectionRenderersMaskInteraction() {
			if (m_ReflectionObjects != null) {
				for (int i = 0; i < m_ReflectionObjects.Length; i++) {
					m_ReflectionObjects[i].renderer.maskInteraction = this.ReflectionMaskInteraction;
				}
			}
		}

		/// <summary>
		/// Make the reflection renderer's <see cref="Renderer.sortingLayerID"/> and <see cref="Renderer.sortingOrder"/> up-to-date.
		/// </summary>
		public void UpdateReflectionRendererSortingOrder() {
			if (m_ReflectionObjects != null) {
                switch (this.ReflectionSortingOrder) {
                    case ReflectionSortingOrders.OrderInLayerOffset:
						for (int i = 0; i < m_ReflectionObjects.Length; i++) {
							m_ReflectionObjects[i].renderer.sortingLayerID =
								m_ReflectionObjects[i].originalSpriteRenderer.sortingLayerID;
							m_ReflectionObjects[i].renderer.sortingOrder =
								m_ReflectionObjects[i].originalSpriteRenderer.sortingOrder + this.OrderInLayerOffset;
						}
						break;
                    case ReflectionSortingOrders.CustomSortingOrder:
						for (int i = 0; i < m_ReflectionObjects.Length; i++) {
							m_ReflectionObjects[i].renderer.sortingLayerID = this.CustomSortingLayerID;
							m_ReflectionObjects[i].renderer.sortingOrder = this.CustomOrderInLayer;
						}
						break;
                    default:
                        break;
                }
			}
		}

		void UpdateReflecitionRenderersMaterial() {
			if (m_ReflectionObjects != null) {
				for (int i = 0; i < m_ReflectionObjects.Length; i++) {
					m_ReflectionObjects[i].UpdateRendererAsOriginal();
					ValidateMaterial(m_ReflectionObjects[i]);
				}
			}
		}

		private void ValidateMaterial(in ReflectionObject reflectionObject) {
			SpriteRenderer renderer = reflectionObject.renderer;
			if (!m_ReflectionProvider) {
				// Show pink borken.
				renderer.sharedMaterial = SharedWater2DMaterialFactory.GetErrorSpriteMaterial();
				return;
			}
#if SUPPORTS_PSYCHOFLOW_BLEND_MODES
			Material targetMaterial = reflectionObject.originalSpriteRenderer.sharedMaterial;
			// 如果是mask則直接採用，也不需要水參數
			if (targetMaterial == BlendModes.SpriteDepthMask.MaskOffMaterial ||
				targetMaterial == BlendModes.SpriteDepthMask.MaskOnMaterial) {
				renderer.sharedMaterial = targetMaterial;
				return;
			}
			int colorOverlayEnumValue = targetMaterial.GetEnumValueByKeywordArray(BlendModes.SharedBlendMaterials.SHADER_KEYWORD_COLOR_OVERLAY_ENUM);
			renderer.sharedMaterial = SharedWater2DMaterialFactory.GetSpriteRefleciotnWaterMaterial(
				m_ReflectionProvider.UseUnscaledTime,
				m_ReflectionProvider.UseCompositeDisplacement,
				m_ReflectionProvider.EnablePerspectiveCorrection,
				false,
				m_ReflectionProvider.UseVoronoiNoiseDisplacement,
				m_ReflectionProvider.FoamBlendMode,
				colorOverlayEnumValue
				);
#else
			renderer.sharedMaterial = SharedWater2DMaterialFactory.GetSpriteSelfReflectionWaterMaterial(
				m_ReflectionProvider.UseUnscaledTime,
				m_ReflectionProvider.UseSecondDisplacementTexture,
				m_ReflectionProvider.EnablePerspectiveCorrection,
				false,
				m_ReflectionProvider.UseVoronoiNoiseDisplacement,
				m_ReflectionProvider.FoamBlendMode
				);
#endif
			// Use material property block to make variance.
			var materialBlock = reflectionObject.StartMaterialPropertyBlock();
			// Setup uv scale for correcting the uv distortion.
			Vector2 scale = GetSpriteScale(renderer);
			materialBlock.SetVector("_SpriteUVScale", scale);

			// Setup other water parameters.
			materialBlock.SetFloat(ShaderPropertyIDs.ReferenceWorldY, m_ReflectionProvider.ReferenceWorldY);
			materialBlock.SetFloat(ShaderPropertyIDs.WaterScale, m_ReflectionProvider.WaterScale);
			if (m_ReflectionProvider.DisplacementTexture) {
				materialBlock.SetTexture(ShaderPropertyIDs.DisplacementTex, m_ReflectionProvider.DisplacementTexture);
			}
			materialBlock.SetFloat(ShaderPropertyIDs.DisplacementSpeed, m_ReflectionProvider.DisplacementSpeed);

			if (m_ReflectionProvider.UseSecondDisplacementTexture && m_ReflectionProvider.DisplacementSecondTexture) {
				materialBlock.SetTexture(ShaderPropertyIDs.DisplacementCompositeTex, m_ReflectionProvider.DisplacementSecondTexture);
				materialBlock.SetFloat(ShaderPropertyIDs.DisplacementCompositeSpeedOffset, m_ReflectionProvider.DisplacementSecondSpeedOffset);
			}

			materialBlock.SetColor(ShaderPropertyIDs.ReflectionTint, m_ReflectionProvider.ReflectionTint);
			materialBlock.SetFloat(ShaderPropertyIDs.ReflectionDisplacementAmount, m_ReflectionProvider.ReflectionDisplacementAmount);
			materialBlock.SetFloat(ShaderPropertyIDs.ReflectionFoamWeight, m_ReflectionProvider.ReflectionFoamWeight);

			materialBlock.SetColor(ShaderPropertyIDs.FoamColor, m_ReflectionProvider.FoamColor);
			materialBlock.SetFloat(ShaderPropertyIDs.FoamThreshold, m_ReflectionProvider.FoamThreshold);
			materialBlock.SetFloat(ShaderPropertyIDs.FoamEdgeThreshold, m_ReflectionProvider.FoamEdgeThreshold);

			if (m_ReflectionProvider.EnablePerspectiveCorrection) {
				materialBlock.SetFloat(ShaderPropertyIDs.PerspectiveCorrectionStrength, m_ReflectionProvider.PerspectiveCorrectionStrength);
				materialBlock.SetFloat(ShaderPropertyIDs.PerspectiveCorrectionContrast, m_ReflectionProvider.PerspectiveCorrectionContrast);
				materialBlock.SetFloat(ShaderPropertyIDs.PerspectiveCorrectionMaxLength, m_ReflectionProvider.PerspectiveCorrectionMaxLength);
				materialBlock.SetFloat(ShaderPropertyIDs.PerspectiveReflectionTiltY, m_ReflectionProvider.PerspectiveReflectionTiltY);
			}

			// [TODO] Vertex displacement.
			// ...

			if (m_ReflectionProvider.UseVoronoiNoiseDisplacement) {
				materialBlock.SetFloat(ShaderPropertyIDs.VoronoiAngleOffset, m_ReflectionProvider.VoronoiAngleOffset);
				materialBlock.SetFloat(ShaderPropertyIDs.VoronoiAngleSpeed, m_ReflectionProvider.VoronoiAngleSpeed);
				materialBlock.SetFloat(ShaderPropertyIDs.VoronoiCellDensity, m_ReflectionProvider.VoronoiCellDensity);
				materialBlock.SetFloat(ShaderPropertyIDs.VoronoiPower, m_ReflectionProvider.VoronoiPowerValue);
			}
			reflectionObject.EndMaterialPropertyBlock();
		}

		/// <summary>
		/// Let <see cref="RefelectionReferenceOffsetY"/> as the bound bottom of reflected renderers.
		/// </summary>
		public void SetReferenceOffsetYAsBoundBottom() {
			var bound = this.Bounds;
			Vector2 centerBottom = bound.center - new Vector3(0f, bound.extents.y, 0);
			Vector2 localCenterBottom = (centerBottom - (Vector2)this.transform.position);
			// inverse scale.
			localCenterBottom = new Vector2(localCenterBottom.x / transform.lossyScale.x, localCenterBottom.y / transform.lossyScale.y);
			this.RefelectionReferenceOffsetY = localCenterBottom.y;
		}

		private static Vector2 GetSpriteScale(SpriteRenderer renderer) {
			if (!renderer || !renderer.sprite) {
				return Vector2.zero;
			}
			Vector2 textureSize = new Vector2(renderer.sprite.texture.width, renderer.sprite.texture.height);
			Vector2 objectScale = renderer.transform.lossyScale;

			return Vector2.one / objectScale / textureSize * renderer.sprite.pixelsPerUnit;
		}

		private static Vector3 ReflectPositionByY(in Vector3 position, in Vector3 anchor) {
			Vector3 dir = position - anchor;
			dir = Vector3.Reflect(dir, Vector3.up);
			return anchor + dir;
		}

		/// <summary>
		/// Copy all properties apart from enabled of SpriteRenderer.
		/// </summary>
		/// <param name="src"></param>
		/// <param name="dst"></param>
		private static void CopySpriteRenderer(SpriteRenderer src, SpriteRenderer dst) {
			if (src == null || dst == null) { return; }

			dst.sprite = src.sprite;
			dst.flipX = src.flipX;
			dst.flipY = src.flipY;
			dst.drawMode = src.drawMode;
			dst.tileMode = src.tileMode;
			dst.color = src.color;
			dst.adaptiveModeThreshold = src.adaptiveModeThreshold;
			dst.spriteSortPoint = src.spriteSortPoint;
			//dst.sortingLayerID = src.sortingLayerID;
			//dst.sortingOrder = src.sortingOrder;
		}

		/// <summary>
		/// Copy all properties apart from enabled of SpriteRenderer.
		/// The method won't call setter of property that is already same.
		/// </summary>
		/// <param name="src"></param>
		/// <param name="dst"></param>
		private static void CopySpriteRendererLazy(SpriteRenderer src, SpriteRenderer dst) {
			if (src == null || dst == null) { return; }

			if (dst.sprite != src.sprite)
				dst.sprite = src.sprite;
			if (dst.flipX != src.flipX)
				dst.flipX = src.flipX;
			if (dst.flipY != src.flipY)
				dst.flipY = src.flipY;
			if (dst.drawMode != src.drawMode)
				dst.drawMode = src.drawMode;
			if (dst.tileMode != src.tileMode)
				dst.tileMode = src.tileMode;
			if (dst.color != src.color)
				dst.color = src.color;
			if (dst.adaptiveModeThreshold != src.adaptiveModeThreshold)
				dst.adaptiveModeThreshold = src.adaptiveModeThreshold;
			if (dst.spriteSortPoint != src.spriteSortPoint)
				dst.spriteSortPoint = src.spriteSortPoint;
			//if (dst.sortingLayerID != src.sortingLayerID)
			//	dst.sortingLayerID = src.sortingLayerID;
			//if (dst.sortingOrder != src.sortingOrder)
			//	dst.sortingOrder = src.sortingOrder;
		}

#if UNITY_EDITOR
		private void OnDrawGizmosSelected() {
			if (SSWR2DPrefs.Instance.gizmoShowReflectionLine) {
				Vector2 reflectionAnchor = this.ReflectionAnchor;
				Vector2 extent = new Vector2(Bounds.extents.x, 0);
				Gizmos.color = Color.red;
				Gizmos.DrawLine(reflectionAnchor - extent, reflectionAnchor + extent);
			}
		}
#endif
	}
}