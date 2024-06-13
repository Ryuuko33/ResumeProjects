using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// RenderPipelineAsset继承自 ScriptableObject类，这个类一般用于配置和保存数据，其实例可以被Unity保存为资源文件
[CreateAssetMenu(menuName = "Rendering/Custom Render Pipeline")]
public class CustomRenderPipelineAsset : RenderPipelineAsset
{

    [SerializeField] 
    private bool 
        useDynamicBatching = true, 
        useGPUInstancing = true, 
        useSRPBatcher = true;

    // 阴影设置属性
    [SerializeField] 
    private ShadowSettings shadows = default;
    
    // 创建渲染管线
    protected override RenderPipeline CreatePipeline()
    {
        return new CustomRenderPipeline(
            useDynamicBatching, useGPUInstancing, useSRPBatcher, shadows
            );
    }
}
