Shader "SSWaterReflection2D/Sprite-ReflectionWater"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0

		[PerRendererData] _SpriteUVScale("_SpriteUVScale", Vector) = (0, 0, 0, 0)

		[Toggle(USE_UNSCALED_TIME)]
		_USE_UNSCALED_TIME("Use unscaled time", Float) = 0
        _ReferenceWorldY("Reference Y in World Space", Float) = 0
		_WaterScale("Water Scale", Range(0.1, 10)) = 1

		_DisplacementTex("Displacement Texture", 2D) = "white" {}
		_DisplacementSpeed("Displacement Speed", Range(-10, 10)) = 0.1

		[Toggle(USE_COMPOSITE_TEX)] 
		_USE_COMPOSITE_TEX("Use Composite Texture", Float) = 1
		[ShowIf(_USE_COMPOSITE_TEX)]_DisplacementCompositeTex("Displacement Composite Texture", 2D) = "white" {}
		[ShowIf(_USE_COMPOSITE_TEX)]_DisplacementCompositeSpeedOffset("Displacement Composite Speed Offset", Range(-5, 5)) = 0.05


		[Header(Water Reflection)]
		[Space]
		_ReflectionTint("Reflection Tint(Support Transparent)", Color) = (0.7, 0.8, 1, 0.75)
		_ReflectionDisplacementAmount("Reflection Displacement Amount", Range(0, 1.0)) = 0.2
		_ReflectionFoamWeight("Reflection Foam Weight", Range(0, 1)) = 1

		[Header(Foam)]
		[Space]
		_FoamColor("Foam Color", Color) = (1.0, 1.0, 1.0, 0.3)
		[KeywordEnum(Normal, Add, Multiply, SoftLight)] FOAM_BLEND ("Foam Blend Mode", Float) = 0
		_FoamThreshold("Foam Threshold", Range(0, 1)) = 0.5
		_FoamEdgeThreshold("Foam Edge Threshold(Local Space Unit)", Float) = 0.1

		[Header(Perspective Correction)]
		[Space]
		[Toggle(PERSPECTIVE_CORRECTION)]
		_PERSPECTIVE_CORRECTION("Perspective Correction", Float) = 0
		[ShowIf(_PERSPECTIVE_CORRECTION)]_PerspectiveCorrectionStrength("Perspective Correction Strength", Range(0, 1.0)) = 0.5
		[ShowIf(_PERSPECTIVE_CORRECTION)]_PerspectiveCorrectionContrast("Perspective Correction Contrast", Range(0, 1.0)) = 0.5
		[ShowIf(_PERSPECTIVE_CORRECTION)]_PerspectiveCorrectionMaxLength("Perspective Correction Length", Range(0, 30)) = 3
        [ShowIf(_PERSPECTIVE_CORRECTION)]_PerspectiveReflectionTiltY("Perspective Tilt Y", Range(0.1, 10)) = 1.0

		[Header(Voronoi Noise Displacement)]
		[Space]
		[Toggle(VORONOI_NOISE_DISPLACEMENT)]
		_VORONOI_NOISE_DISPLACEMENT("Use Voronoi Noise Displacement", Float) = 0
		[ShowIf(_VORONOI_NOISE_DISPLACEMENT)]_VoronoiAngleOffset("Voronoi Angle Offset", Float) = 1
		[ShowIf(_VORONOI_NOISE_DISPLACEMENT)]_VoronoiAngleSpeed("Voronoi Angle Speed", Float) = 1
		[ShowIf(_VORONOI_NOISE_DISPLACEMENT)]_VoronoiCellDensity("Voronoi Cell Density", Float) = 14
		[ShowIf(_VORONOI_NOISE_DISPLACEMENT)]_VoronoiPower("Voronoi Power Value", Float) = 1.4
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
            "DisableBatching"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            #pragma target 2.0
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA

			#pragma multi_compile_local _ USE_UNSCALED_TIME
            #pragma multi_compile_local _ USE_COMPOSITE_TEX
			#pragma multi_compile_local _ PERSPECTIVE_CORRECTION
			#pragma shader_feature VORONOI_NOISE_DISPLACEMENT
			#pragma multi_compile_local FOAM_BLEND_NORMAL FOAM_BLEND_ADD FOAM_BLEND_MULTIPLY FOAM_BLEND_SOFTLIGHT

			//#define SUPPORTS_PSYCHOFLOW_BLEND_MODES
			// Supports the color overlay feature in Psychoflow.BlendModes
			#ifdef SUPPORTS_PSYCHOFLOW_BLEND_MODES
			#pragma multi_compile_local COLOR_OVERLAY_NONE COLOR_OVERLAY_NORMAL COLOR_OVERLAY_ADD COLOR_OVERLAY_MULTIPLY COLOR_OVERLAY_SOFTLIGHT
			#include "Assets/Psychoflow/BlendModes/Shaders/Includes/SamplingMethodCG.cginc"
			#include "Assets/Psychoflow/BlendModes/Shaders/Includes/SpriteColorOverlayCG.cginc"
			#endif

            #include "WaterCG.cginc"

            float _ReferenceWorldY;

			float4 _SpriteUVScale;

			float _ReflectionDisplacementAmount;
			float _ReflectionFoamWeight;
			half4 _ReflectionTint;

			half4 _FoamColor;
			float _FoamThreshold;
			float _FoamEdgeThreshold;

            half4 Fragment(Varyings IN) : SV_Target {
                // Compute uv Y via water reference Y.
				float point2ReferenceY = _ReferenceWorldY - IN.positionWS.y;

				// Displacement Offsets and Sampling UV
				float2 displcementOffset, samplingDisplacementUV;
				GetDisplcementOffsetAndSamplingUV(IN, point2ReferenceY, displcementOffset, samplingDisplacementUV);

				float2 reflectionUV = IN.uv + _ReflectionDisplacementAmount * displcementOffset * _SpriteUVScale;
				// Discard the out of sprite uv color.
				if (reflectionUV.x < 0 || reflectionUV.x > 1 || reflectionUV.y < 0 || reflectionUV.y > 1) {
					discard;
				}

#ifdef SUPPORTS_PSYCHOFLOW_BLEND_MODES
				half4 mainTexColor = SampleTexture(_MainTex, reflectionUV, _MainTex_TexelSize) * IN.color;
#else
				half4 mainTexColor = tex2D(_MainTex, reflectionUV) * IN.color;
#endif
#if ETC1_EXTERNAL_ALPHA
				half4 alpha = tex2D(_AlphaTex, reflectionUV);
				mainTexColor.a = lerp(mainTexColor.a, alpha.r, _EnableExternalAlpha);
#endif

				#ifdef SUPPORTS_PSYCHOFLOW_BLEND_MODES
				// Sprite Effects: Color Overlay
				#ifndef COLOR_OVERLAY_NONE
				mainTexColor.rgb = BlendColorOverlay(COLOR_OVERLAY_PARAMS(_OverlayTint.rgb, mainTexColor.rgb, _OverlayTint.a, IN.positionWS));
				#endif
				#endif

                half4 result = mainTexColor * _ReflectionTint;

				// Foam thresholding
				float foamThreshold = lerp(1.0, _FoamThreshold, _ReflectionFoamWeight);
				float edgeFoamThreshold = lerp(0.0, _FoamEdgeThreshold, _ReflectionFoamWeight);

				COMPUTE_FOAM(result.rgb, _FoamColor, foamThreshold, edgeFoamThreshold, displcementOffset, point2ReferenceY, samplingDisplacementUV);

                // Alpha blending.
                result.rgb *= result.a;
                return result;
            }
        ENDCG
        }
    }
}
