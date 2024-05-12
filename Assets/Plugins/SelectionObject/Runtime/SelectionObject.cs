using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public static class SelectionObject {

    private static RenderTexture _renderTexture;
    private static Material _material;
    private static int frameCount;

    public static void FindObjectOfScreenPosition(Vector3 mousePosition) {
        // TryInitIdRenderTexture();
    }

    private static void TryInitIdRenderTexture() {
        if (_renderTexture == null) {
            _renderTexture = new RenderTexture(Screen.width, Screen.height, 0, GraphicsFormat.R32_SInt) {
                name = "InstanceIDTexture"
            };
            // _renderTexture = CreateRenderTexture();
        }

        if (_material == null) {
            var shader = Resources.Load<Shader>("DrawInstanceID");
            _material = CoreUtils.CreateEngineMaterial(shader);
        }


        // if (Time.frameCount != frameCount) {
        //     ReRenderRenderTexture();
        //     frameCount = Time.frameCount;
        // }
    }

    private static void ReRenderRenderTexture() {
        RenderTexture previousActive = RenderTexture.active;
        RenderTexture.active = _renderTexture;
        GL.Clear(true, true, Color.clear);
        RenderTexture.active = previousActive;

        Graphics.SetRenderTarget(_renderTexture);
        var renderers = GameObject.FindObjectsOfType<Renderer>();
        foreach (var renderer in renderers) {
            if ((renderer.gameObject.layer & LayerMask.NameToLayer("Selection")) > 0) {
                if (renderer is MeshRenderer) {
                    var meshFilter = renderer.GetComponent<MeshFilter>();
                    if (meshFilter != null) {
                        var mesh = meshFilter.sharedMesh;
                        _material.SetInt("_InstanceID", renderer.gameObject.GetInstanceID());
                        // Graphics.DrawMesh(mesh, renderer.gameObject.transform.localToWorldMatrix, _material, 0);
                        Graphics.DrawMesh(mesh, renderer.gameObject.transform.localToWorldMatrix, _material, 0);
                    }
                }
            }
        }
    }

    private static RenderTexture CreateRenderTexture() {
        return new RenderTexture(Screen.width, Screen.height, 0, GraphicsFormat.R32_SInt);
    }
}