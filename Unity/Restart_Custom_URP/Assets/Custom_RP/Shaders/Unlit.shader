Shader "Custom RP/Unlit"
{
    Properties
    {
    	_BaseMap("Texture", 2D) = "write" {}
    	_BaseColor("Color", Color) = (1.0, 1.0, 1.0, 1.0)
    	
    	_Cutoff("Alpha CutOff", Range(0.0, 1.0)) = 0.5
        [Toggle(_CLIPPING)] _Clipping ("Alpha Clipping", Float) = 0
    	
    	[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend", Float) = 1
    	[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend", Float) = 0
    	[Enum(Off, 0, On, 1)] _ZWrite ("Z Write", Float) = 1
    }
    SubShader
    {
        Pass
        {
        	Blend [_SrcBlend][_DstBlend]
        	Zwrite [_ZWrite]
        	
			HLSLPROGRAM
			#pragma shader_feature _CLIPPING
			#pragma multi_compile_instancing
			#pragma vertex UnlitPassVertex
			#pragma fragment UnlitPassFragment
			#include "UnlitPass.hlsl"
			ENDHLSL
        }
    }
	
	CustomEditor "CustomShaderGUI"
}
