using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PerObjectMaterialProperties : MonoBehaviour
{
    // private static int baseColorId = Shader.PropertyToID("_BaseColor");
    // private static int baseMapId = Shader.PropertyToID("_BaseMap");
    // private static int cutoffId = Shader.PropertyToID("_Cutoff");
    // metallicId = Shader.

    private static int
        baseColorId = Shader.PropertyToID("_BaseColor"),
        baseMapId = Shader.PropertyToID("_BaseMap"),
        cutoffId = Shader.PropertyToID("_Cutoff"),
        metallicId = Shader.PropertyToID("_Metallic"),
        smoothnessId = Shader.PropertyToID("_Smoothness"); 

    [SerializeField] 
    private Color baseColor = Color.white;
    [SerializeField] 
    private Texture2D baseMap;

    [SerializeField, Range(0f, 1f)] private float
        cutoff = 0.5f,
        metallic = 0f,
        smoothness = 0.5f;

    private static MaterialPropertyBlock block;

    // 在脚本被加载或者监视面板中数值发生变化时被调用
    private void OnValidate()
    {
        if (block == null)
        {
            block = new MaterialPropertyBlock();
        }
        
        block.SetColor(baseColorId, baseColor);
        block.SetFloat(cutoffId, cutoff);
        block.SetFloat(metallicId, metallic);
        block.SetFloat(smoothnessId, smoothness);
        if (baseMap != null)
        {
            block.SetTexture(baseMapId, baseMap);
        }
        GetComponent<Renderer>().SetPropertyBlock(block);
    }

}
