#ifndef INCLUDE_WATER_CG
#define INCLUDE_WATER_CG

#include "UnityCG.cginc"

struct VertexInputs
{
    float4 positionOS : POSITION;
    float2 uv : TEXCOORD0;
    half4 color : COLOR;
};

struct Varyings
{
    float2 uv : TEXCOORD0;
    float4 positionCS : SV_POSITION;
    half4 color : Color;

    float4 positionWS : TEXCOORD1;
    float2 screenUV : TEXCOORD2;
    float2 displacementUV : TEXCOORD3;
    #ifdef USE_COMPOSITE_TEX
    float2 displacementCompositeUV : TEXCOORD4;
    #endif
};

CBUFFER_START(UnityPerDrawSprite)
// Sprite Renderer parameters.
float _EnableExternalAlpha;
half2 _Flip;
half4 _RendererColor;
CBUFFER_END

sampler2D _MainTex;
float4 _MainTex_ST;
float4 _MainTex_TexelSize;

sampler2D _AlphaTex;

float _WaterScale;

// Displacement parameters
sampler2D _DisplacementTex;
float4 _DisplacementTex_ST;
float _DisplacementSpeed;

#ifdef USE_COMPOSITE_TEX
sampler2D _DisplacementCompositeTex;
float4 _DisplacementCompositeTex_ST;
float _DisplacementCompositeSpeedOffset;
#endif

#ifdef VERTEX_DISPLACEMENT
    sampler2D _VertexDisplacementTex;
    float4 _VertexDisplacementTex_ST;
    float _VertexDisplacementAmount;
    float _VertexDisplacementSpeed;
#endif

#ifdef PERSPECTIVE_CORRECTION
    float _PerspectiveReflectionTiltY;
    float _PerspectiveCorrectionStrength;
    float _PerspectiveCorrectionContrast;
    float _PerspectiveCorrectionMaxLength;
#endif

///////////
// Global
///////////
static const float _WorldUVScalar = 0.1;

float4 _UnscaledTime;

// Unpack displacment texture from [0, 1] to [-1, 1]
float2 UnpackDisplacementValue(float2 color) {
    return color * 2 - 1.0;
}

inline float4 UnityFlipSprite(in float3 pos, in half2 flip) {
    return float4(pos.xy * flip, pos.z, 1.0);
}

half4 SampleSpriteTexture (float2 uv) {
    half4 color = tex2D (_MainTex, uv);

#if ETC1_EXTERNAL_ALPHA
    half4 alpha = tex2D (_AlphaTex, uv);
    color.a = lerp (color.a, alpha.r, _EnableExternalAlpha);
#endif

    return color;
}

////////////////////////////////
///   Voronoi Noise
///////////////////////////////
#ifdef VORONOI_NOISE_DISPLACEMENT

float _VoronoiAngleOffset;
float _VoronoiAngleSpeed;
float _VoronoiCellDensity;
float _VoronoiPower;

inline float2 unity_voronoi_noise_randomVector (float2 UV, float offset) {
    float2x2 m = float2x2(15.27, 47.63, 99.41, 89.98);
    UV = frac(sin(mul(UV, m)) * 46839.32);
    return float2(sin(UV.y*+offset)*0.5+0.5, cos(UV.x*offset)*0.5+0.5);
}

float Unity_Voronoi_float(float2 UV, float AngleOffset, float CellDensity) {
    float2 g = floor(UV * CellDensity);
    float2 f = frac(UV * CellDensity);
    float t = 8.0;
    float3 res = float3(8.0, 0.0, 0.0);
    float Out = 0.0;

    for(int y=-1; y<=1; y++)
    {
        for(int x=-1; x<=1; x++)
        {
            float2 lattice = float2(x,y);
            float2 offset = unity_voronoi_noise_randomVector(lattice + g, AngleOffset);
            float d = distance(lattice + offset, f);
            if(d < res.x)
            {
                res = float3(d, offset.x, offset.y);
                Out = res.x;
            }
        }
    }
    return saturate(Out);
}

float2 SampleVoronoiNoise(float2 uv) {
    return Unity_Voronoi_float(uv, _VoronoiAngleOffset + _VoronoiAngleSpeed * _Time[1], _VoronoiCellDensity);
}

float2 VoronoiDisplacement(float2 uv) {
    // Sample Voronoi Noise.
    float voronoiFloat = Unity_Voronoi_float(uv, _VoronoiAngleOffset + _VoronoiAngleSpeed * _Time[1], _VoronoiCellDensity);
    voronoiFloat = pow(voronoiFloat, _VoronoiPower);
    voronoiFloat = UnpackDisplacementValue(voronoiFloat);
    return float2(voronoiFloat, voronoiFloat);
}
#endif

//////////////////////////////
/// Perspective Correction
//////////////////////////////
#ifdef PERSPECTIVE_CORRECTION
#define COMPUTE_PERSPECTIVE_CORRECTION(varyingInput, point2ReferenceY) PerspectiveCorrectionWithContrastAndWorldUV(varyingInput.screenUV.x, saturate(point2ReferenceY / _PerspectiveCorrectionMaxLength), _PerspectiveCorrectionStrength, _PerspectiveCorrectionContrast, varyingInput.positionWS.z)
#else
#define COMPUTE_PERSPECTIVE_CORRECTION(varyingInput, point2ReferenceY) 0
#endif

#ifdef PERSPECTIVE_CORRECTION
// Ref: https://forums.tigsource.com/index.php?topic=40539.msg1104986#msg1104986
inline float2 PerspectiveCorrection(in float xNormalized, in float yNormalized, in float strength, in float powerValue) {
    // the value must be [0, 1), otherwise it will get inverse motion.
    float perspectiveStrength = strength * 0.9 * pow(yNormalized, powerValue);
    return float2((0.5 - xNormalized) * perspectiveStrength, 0.0);
}
inline float2 PerspectiveCorrectionWithContrast(in float xNormalized, in float yNormalized, in float strength, in float contrast) {
    // power function makes contrast larger.
    // [1/16, 16]
    float power = pow(2, lerp(-4, 4, contrast));
    return PerspectiveCorrection(xNormalized, yNormalized, strength, power);
}

inline float2 PerspectiveCorrectionWithContrastAndWorldUV(in float xNormalized, in float yNormalized, in float strength, in float contrast, in float depthWS) {
    float worldUVScale = _WorldUVScalar;
    // Perspective camera
    if (unity_OrthoParams.w == 0.0){
        float linearDepth = depthWS - _WorldSpaceCameraPos.z;
        // m11 = 1 / tan( fov/2 * Deg2Rad )
        // https://www.scratchapixel.com/lessons/3d-basic-rendering/perspective-and-orthographic-projection-matrix/building-basic-perspective-projection-matrix
        float yScalar = linearDepth / unity_CameraProjection._m11;
        float xScalar = yScalar * _ScreenParams.x / _ScreenParams.y;
        worldUVScale *= 2.0 * abs(xScalar);
    } else {
        worldUVScale *= abs(unity_OrthoParams.x);
    }
    
    
    return PerspectiveCorrectionWithContrast(xNormalized, yNormalized, strength, contrast) * worldUVScale;
}
#endif

//////////////////////////////
/// Displacement
//////////////////////////////
void GetDisplcementOffsetAndSamplingUV(in Varyings IN, in float point2ReferenceY, out float2 displcementOffset, out float2 samplingDisplacementUV) {
    half2 perspectiveCorrection = COMPUTE_PERSPECTIVE_CORRECTION(IN, point2ReferenceY);

#ifdef USE_UNSCALED_TIME
    float time = _UnscaledTime[1];
#else
    float time = _Time[1];
#endif
    half2 moveOffset1 = float2(time * _DisplacementSpeed, 0.0);
    samplingDisplacementUV = IN.displacementUV + (moveOffset1 + perspectiveCorrection) / _WaterScale;

    #ifdef VORONOI_NOISE_DISPLACEMENT
        displcementOffset = VoronoiDisplacement(samplingDisplacementUV);
    #else
        // Sample displacement texture.
        #ifdef USE_COMPOSITE_TEX
            // Sample two textures and combine.
            float compositeSpeed = (_DisplacementSpeed + _DisplacementCompositeSpeedOffset * sign(_DisplacementSpeed));
            half2 moveOffset2 = float2(time * compositeSpeed, 0.0);
            float2 sampleDisplacementCompositeUV = IN.displacementCompositeUV + (moveOffset2 + perspectiveCorrection) / _WaterScale;
            displcementOffset = (tex2D(_DisplacementTex, samplingDisplacementUV).xy + tex2D(_DisplacementCompositeTex, sampleDisplacementCompositeUV).xy) * 0.5;
        #else
            displcementOffset = tex2D(_DisplacementTex, samplingDisplacementUV).xy;
        #endif
        displcementOffset = UnpackDisplacementValue(displcementOffset);
    #endif
}

///////////////////
///    Foam
///////////////////
#ifdef VORONOI_NOISE_DISPLACEMENT
#define COMPUTE_FOAM(result, foamColor, foamThreshold, edgeFoamThreshold, displcementOffset, point2ReferenceY, samplingDisplacementUV) \
FoamVoronoi(result, foamColor, foamThreshold, samplingDisplacementUV);
#else
#define COMPUTE_FOAM(result, foamColor, foamThreshold, edgeFoamThreshold, displcementOffset, point2ReferenceY, samplingDisplacementUV) \
Foam(result, foamColor, foamThreshold, edgeFoamThreshold, displcementOffset, point2ReferenceY);
#endif

#ifndef SUPPORTS_PSYCHOFLOW_BLEND_MODES
// Copy from BlendModeFunctions.cginc
// It is very similar to the Photoshop soft light mode, for dark blend colors it is identical, for bright ones it differs a bit. It does not share the disadvantage of the Photoshop soft light mode.
half3 SoftLightPegtop(in half3 src, in half3 dst) {
	// http://www.pegtop.net/delphi/articles/blendmodes/softlight.htm
	// Formula#1: (1 - a) * ab + a * (1 - (1 - a) * (1 - b))
	//return (1 - dst) * dst * src + dst * (1 - (1 - dst) * (1 - src)); 
	// https://en.wikipedia.org/wiki/Blend_modes#Soft_Light #pegtop
	// Formula#2: (1 - 2b)a^2 + 2ba
	return (1 - 2 * src) * dst * dst + 2 * dst * src;
}
#endif

// Blend two colors.
// destination alpha is always 1.
inline half3 Blend(in half3 src, in half3 dst, in half srcAlpha) {
    #if FOAM_BLEND_NORMAL
    return saturate(src * srcAlpha + dst * (1.0 - srcAlpha));
    #elif FOAM_BLEND_ADD
    return saturate(src * srcAlpha + dst);
    #elif FOAM_BLEND_MULTIPLY
    return lerp(dst, saturate(src * dst), srcAlpha);
    #elif FOAM_BLEND_SOFTLIGHT
    return lerp(dst, saturate(SoftLightPegtop(src, dst)), srcAlpha);
    #endif
    return half3(1.0, 0, 1.0); // pink - the broken color.
}

void Foam(inout half3 result, in half4 foamColor, in float foamThreshold, in float edgeFoamThreshold, in float2 displcementOffset, in float point2ReferenceY) {
    // Threshold of foam by sampling displacement texture.
    // The foam occurs when the amount of displacement is too large.
    // Edge foam threshold makes foam happened easily when the uv.y is in the edge threshold.
    float2 weight = abs(displcementOffset);
    float edgeWeight = saturate(point2ReferenceY / max(edgeFoamThreshold, 0.001));
    if ((weight.x > foamThreshold && weight.y > foamThreshold)
        || (edgeWeight < weight.x)) {
        result.rgb = Blend(foamColor.rgb, result.rgb, foamColor.a);
    }
}

#ifdef VORONOI_NOISE_DISPLACEMENT
void FoamVoronoi(inout half3 result, in half4 foamColor, in float foamThreshold, in float2 samplingDisplacementUV) {
     // threshold.
    float voronoiFloat = SampleVoronoiNoise(samplingDisplacementUV);
    voronoiFloat = pow(voronoiFloat, _VoronoiPower);
    float weight = max(0,  voronoiFloat - foamThreshold);
    result.rgb = Blend(foamColor.rgb, result.rgb, foamColor.a * weight);
}
#endif

Varyings Vertex(VertexInputs IN)
{
    Varyings OUT;

    OUT.color = IN.color * _RendererColor;

    // Sprite Flip
    IN.positionOS = UnityFlipSprite(IN.positionOS, _Flip);
    
    //vertex displacement
    #ifdef VERTEX_DISPLACEMENT
        float2 vpUV = TRANSFORM_TEX(IN.uv, _VertexDisplacementTex);
        IN.positionOS.xy += (2 * tex2Dlod(_VertexDisplacementTex, float4(vpUV + _Time[1] * _VertexDisplacementSpeed, 0, 0)).rg - 1) * _VertexDisplacementAmount;
    #endif

    // transform to clip space
    OUT.positionCS = UnityObjectToClipPos(IN.positionOS);
    OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);

    // transform to world space.
    OUT.positionWS = mul(unity_ObjectToWorld, IN.positionOS);

    // transform to screen uv [0, 1]
    float4 screenClipUV = ComputeGrabScreenPos(OUT.positionCS); // xy in [0, w]
    OUT.screenUV = screenClipUV.xy / screenClipUV.w; // xy in [0, 1]

    float2 worldUV = (1.0 / _WaterScale * _WorldUVScalar) * OUT.positionWS.xy;
    OUT.displacementUV = TRANSFORM_TEX(worldUV, _DisplacementTex);
    #ifdef USE_COMPOSITE_TEX
    OUT.displacementCompositeUV = TRANSFORM_TEX(worldUV, _DisplacementCompositeTex);
    #endif

    #ifdef PIXELSNAP_ON
    OUT.positionCS = UnityPixelSnap(OUT.positionCS);
    #endif

    return OUT;
}

#endif
