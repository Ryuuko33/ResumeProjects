// Made by PeDev 2020

Shader "SSWaterReflection2D/SSWaterReflection"
{
	Properties
	{
		_MainTex("Alpha Texture", 2D) = "white" {}
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0

		[Toggle(USE_UNSCALED_TIME)]
		_USE_UNSCALED_TIME("Use unscaled time", Float) = 0
		_ReferenceWorldY("Reference Y in World Space", Float) = 0
        _ScreenBoundFadeOut("Bound Fade Out", Range(0, 1)) = 0.1
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
		_ReflectionDisplacementAmount("Reflection Displacement Amount (world space unit)", Range(0, 1.0)) = 0.2
		_ReflectionFoamWeight("Reflection Foam Weight", Range(0, 1)) = 1

		[Header(Water Refraction)]
		[Space]
		_RefractionTint("Refraction Tint(Support Transparent)", Color) = (0.7, 0.8, 1, 0.75)
		_RefractionDisplacementAmount("Refraction Displacement Amount (world space unit)", Range(0, 1.0)) = 0.2
		_RefractionFoamWeight("Refraction Foam Weight", Range(0, 1)) = 0.5


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
		[ShowIf(_PERSPECTIVE_CORRECTION)]_PerspectiveCorrectionMaxLength("Perspective Correction Length", Range(0, 10)) = 3
        [ShowIf(_PERSPECTIVE_CORRECTION)]_PerspectiveReflectionTiltY("Perspective Tilt Y", Range(0.1, 30)) = 1.0

		[Header(Vertex Displacement)]
		[Space]
		[Toggle(VERTEX_DISPLACEMENT)]
		_VERTEX_DISPLACEMENT("Vertex Displacement", Float) = 0
		[ShowIf(_VERTEX_DISPLACEMENT)]_VertexDisplacementTex("Vertex Displacement Texture", 2D) = "white" {}
		[ShowIf(_VERTEX_DISPLACEMENT)]_VertexDisplacementAmount("Vertex Displacement Amount", Range(0, 1)) = 0.015
		[ShowIf(_VERTEX_DISPLACEMENT)]_VertexDisplacementSpeed("Vertex Displacement Speed", Range(0, 1)) = 0.025

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
		Tags { 
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
		 }
		LOD 100
        Blend One OneMinusSrcAlpha

		ZTest LEqual
		ZWrite Off
		Cull Off

		GrabPass {}

		Pass
		{
			CGPROGRAM
			#pragma vertex Vertex
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog

			#pragma multi_compile_local _ USE_UNSCALED_TIME
			#pragma multi_compile_local _ USE_COMPOSITE_TEX
			#pragma multi_compile_local _ PERSPECTIVE_CORRECTION
			#pragma shader_feature VORONOI_NOISE_DISPLACEMENT
			#pragma shader_feature VERTEX_DISPLACEMENT
			#pragma multi_compile_local FOAM_BLEND_NORMAL FOAM_BLEND_ADD FOAM_BLEND_MULTIPLY FOAM_BLEND_SOFTLIGHT

			#include "WaterCG.cginc"

			sampler2D _GrabTexture;
			float4 _GrabTexture_TexelSize;

			float _ReferenceWorldY;
            float _ScreenBoundFadeOut;

			float _ReflectionDisplacementAmount;
			float _ReflectionFoamWeight;
			half4 _ReflectionTint;

			half4 _RefractionTint;
			float _RefractionDisplacementAmount;
			float _RefractionFoamWeight;

			half4 _FoamColor;
			float _FoamThreshold;
			float _FoamEdgeThreshold;

			float2 ComputeDisplacementSS(float4 positionWS, float2 positionSS, float2 displacementWS) {
				// Get scalar from world coordinate to screen coordinate.
				float4 newPositionWS = positionWS + float4(displacementWS, 0, 0);
				float4 newPositionCS = mul(UNITY_MATRIX_VP, newPositionWS);
				float4 newPositionSS = ComputeGrabScreenPos(newPositionCS);
				newPositionSS.xy = newPositionSS.xy / newPositionSS.w;
				return newPositionSS.xy - positionSS;
			}

			half4 frag(Varyings IN) : SV_Target {
				// The object's world position = unity_ObjectToWorld._m03_m13_m23
				// You can imagine that world position = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).
				float4 referenceWS = float4(unity_ObjectToWorld._m03_m13_m23, 1);
				referenceWS.y = _ReferenceWorldY;
				float4 referenceCS = mul(UNITY_MATRIX_VP, referenceWS);
				float4 referenceScreenPos = ComputeGrabScreenPos(referenceCS); // result xy in [0, w]
				float yReferenceSS = referenceScreenPos.y / referenceScreenPos.w; // divided w to [0, 1]

				// Compute the vector from this world point to reference Y.
				float point2ReferenceY = _ReferenceWorldY - IN.positionWS.y;

				// Displacement Offsets and Sampling UV
				float2 displcementOffset, samplingDisplacementUV;
				GetDisplcementOffsetAndSamplingUV(IN, point2ReferenceY, displcementOffset, samplingDisplacementUV);

				// Relfection Y.
				float refelctionOffset = (yReferenceSS - IN.screenUV.y);
				#ifdef PERSPECTIVE_CORRECTION
					refelctionOffset *= _PerspectiveReflectionTiltY;
				#endif
				float uvY = yReferenceSS + refelctionOffset;
				float2 reflectionUV = float2(IN.screenUV.x, uvY);

				// Displace reflection UV
				reflectionUV += ComputeDisplacementSS(IN.positionWS, IN.screenUV, displcementOffset * _ReflectionDisplacementAmount);
				half3 reflectionColor = saturate(tex2D(_GrabTexture, reflectionUV)) * _ReflectionTint.rgb;

				// Refraction
				float2 refractionUV = IN.screenUV;
				// Displace UV
				refractionUV += ComputeDisplacementSS(IN.positionWS, IN.screenUV, displcementOffset * _RefractionDisplacementAmount);
				half3 refractionColor = saturate(tex2D(_GrabTexture, refractionUV)) * _RefractionTint.rgb;

				// Fade out if Closing the bound of screen.
				float reflectionClosingBoundY = max(0, min(1.0 - reflectionUV.y, reflectionUV.y));
				float reflectionBoundFactor = saturate(reflectionClosingBoundY / max(_ScreenBoundFadeOut, 0.001));
				float reflectionWeight = _ReflectionTint.a * reflectionBoundFactor;

				// Blending reflection and refraction.
				half4 result;
				result.rgb = lerp(refractionColor, reflectionColor, reflectionWeight);

				// Foam thresholding
				float foamWeight = lerp(_RefractionFoamWeight, _ReflectionFoamWeight, reflectionBoundFactor);
				float foamThreshold = lerp(1.0, _FoamThreshold, foamWeight);
				float edgeFoamThreshold = lerp(0.0, _FoamEdgeThreshold, foamWeight);
				
				COMPUTE_FOAM(result.rgb, _FoamColor, foamThreshold, edgeFoamThreshold, displcementOffset, point2ReferenceY, samplingDisplacementUV);

				// Alpha blending.
				// sample the alpha texture
				half alpha = tex2D(_MainTex, IN.uv).a;

				result.rgb *= alpha;
				result.a = alpha;
				
				return result;
			}
			ENDCG
		}
	}
}
