using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class ColorConverter {
    public static Color32 ToColor(int value) {
        Span<byte> bytes = stackalloc byte[4];
        BitConverter.TryWriteBytes(bytes, value + 1);
        return new Color32(bytes[0], bytes[1], bytes[2], bytes[3]);
    }

    public static int ToInt(Color32 value) {
        ReadOnlySpan<byte> bytes = stackalloc byte[] { value.r, value.g, value.b, value.a };
        return BitConverter.ToInt32(bytes) - 1;
    }
}

public static class ColoredInstanceID {
    private static Dictionary<Color, int> dic = new Dictionary<Color, int>();

    public static Color GetColorDebug(int instanceId) {
        var color = ColorConverter.ToColor((instanceId * 10000).GetHashCode());
        dic[color] = instanceId;
        return color;
    }

    public static int GetIDDebug(Color color) {
        return dic.GetValueOrDefault(color, 0);
    }
}

public class SelectionObjectRenderPass : ScriptableRenderPass {
    private RenderTexture _renderTexture;
    private Material _material;
    private RenderTexture _pixelRenderTexture;
    private Texture2D _texture;

    public SelectionObjectRenderPass() {
        _renderTexture = new RenderTexture(Screen.width, Screen.height, 0, GraphicsFormat.R8G8B8A8_SRGB) {
            name = "InstanceIDTexture"
        };
        var shader = Resources.Load<Shader>("DrawInstanceID");
        _material = CoreUtils.CreateEngineMaterial(shader);

        _pixelRenderTexture = new RenderTexture(1, 1, 0, GraphicsFormat.R8G8B8A8_SRGB) {
            name = "pixelRenderTexture"
        };

        // _pixelRenderTexture = new RenderTexture(Screen.width, Screen.height, 0, GraphicsFormat.R8G8B8A8_SRGB) {
        //     name = "pixelRenderTexture"
        // };

        _texture = new Texture2D(_pixelRenderTexture.width, _pixelRenderTexture.height,
            GraphicsFormat.R8G8B8A8_SRGB, TextureCreationFlags.None);

        renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
        if (renderingData.cameraData.cameraType != CameraType.Game) {
            return;
        }

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
                        var color = ColoredInstanceID.GetColorDebug(renderer.gameObject.GetInstanceID());
                        Debug.Log("Draw color" + color);
                        cmd.SetGlobalColor("_InstanceID", color);
                        cmd.DrawRenderer(renderer, _material);
                    }
                }
            }
        }

        // cmd.Blit(_renderTexture, _pixelRenderTexture, 
        //     new Vector2(_pixelRenderTexture.width, _pixelRenderTexture.height), 
        //     Input.mousePosition);

        // cmd.Blit(_renderTexture, _pixelRenderTexture,
        //     new Vector2(_pixelRenderTexture.width, _pixelRenderTexture.height),
        //     new Vector2(300, 300));

        cmd.CopyTexture(
            _renderTexture,
            0,
            0,
            Math.Clamp((int)Input.mousePosition.x, 0, Screen.width - 1),
            Math.Clamp((int)Input.mousePosition.y, 0, Screen.height - 1),
            _pixelRenderTexture.width,
            _pixelRenderTexture.height,
            _pixelRenderTexture,
            0,
            0,
            0, 0
        );

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);

        RenderTexture.active = _pixelRenderTexture;
        _texture.ReadPixels(new Rect(0, 0, _pixelRenderTexture.width, _pixelRenderTexture.height), 0, 0);
        _texture.Apply();
        var pickedColor = _texture.GetPixel(0, 0);
        Debug.Log("readback color" + pickedColor);
        RenderTexture.active = null;

        SelectionObject.InstanceID = ColoredInstanceID.GetIDDebug(pickedColor);
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