using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public static class SelectionObject {
    public static int InstanceID { get; set; }

    public static Renderer SelectedRenderer() {
        var renderers = GameObject.FindObjectsOfType<Renderer>();
        foreach (var renderer in renderers) {
            if ((renderer.gameObject.layer & LayerMask.NameToLayer("Selection")) > 0) {
                if (renderer.gameObject.GetInstanceID() == InstanceID) {
                    return renderer;
                }
            }
        }

        return null;
    }
}