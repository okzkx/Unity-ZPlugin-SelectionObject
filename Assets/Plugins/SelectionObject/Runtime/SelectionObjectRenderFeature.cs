using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SelectionObjectRenderPass : ScriptableRenderPass {
    private static RenderTexture _renderTexture;
    private static Material _material;


    public SelectionObjectRenderPass() {
        _renderTexture = new RenderTexture(Screen.width, Screen.height, 0, GraphicsFormat.R8G8B8A8_SInt) {
            name = "InstanceIDTexture"
        };
        var shader = Resources.Load<Shader>("DrawInstanceID");
        _material = CoreUtils.CreateEngineMaterial(shader);
        renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
        var cmd = CommandBufferPool.Get("SelectionObject Render Pass");
        cmd.SetRenderTarget(_renderTexture, depthAttachment);
        cmd.ClearRenderTarget(RTClearFlags.Color, Color.clear, 1, 0);

        var renderers = GameObject.FindObjectsOfType<Renderer>();
        foreach (var renderer in renderers) {
            if ((renderer.gameObject.layer & LayerMask.NameToLayer("Selection")) > 0) {
                if (renderer is MeshRenderer) {
                    var meshFilter = renderer.GetComponent<MeshFilter>();
                    if (meshFilter != null) {
                        // var mesh = meshFilter.sharedMesh;
                        // _material.SetInt("_InstanceID", renderer.gameObject.GetInstanceID());
                        // Graphics.DrawMesh(mesh, renderer.gameObject.transform.localToWorldMatrix, _material, 0);
                        // Graphics.DrawMesh(mesh, renderer.gameObject.transform.localToWorldMatrix, _material, 0);
                        //  new MaterialPropertyBlock();
                        // renderer.SetPropertyBlock();
                        cmd.SetGlobalInt("_InstanceID", renderer.gameObject.GetInstanceID());
                        cmd.DrawRenderer(renderer, _material);
                    }
                }
            }
        }
        
        // if (PickBufferManager.instance != null && pickBufferMaterial != null) {
        //     var objs = PickBufferManager.instance.objs;
        //
        //     for (int i = 0; i < objs.Count; i++) {
        //         var obj = objs[i];
        //         var color = PickBufferManager.instance.IDToColor(i);
        //         var renderer = obj.GetComponent<MeshRenderer>();
        //         cmd.SetGlobalColor("_Color", color);
        //         cmd.DrawRenderer(renderer, pickBufferMaterial, 0, 0);
        //     }
        // }

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
}

public class SelectionObjectRenderFeature : ScriptableRendererFeature {
    private static SelectionObjectRenderPass _pass;

    public override void Create() {
        _pass = new SelectionObjectRenderPass();
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
        renderer.EnqueuePass(_pass);
    }
}