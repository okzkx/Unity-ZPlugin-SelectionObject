using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TestSelectionObject : MonoBehaviour {
    public Material selectedMaterial;

    private List<Material> sharedMaterials = new List<Material>();

    void Update() {
        var renderers = GameObject.FindObjectsOfType<Renderer>();
        foreach (var renderer in renderers) {
            if ((renderer.gameObject.layer & LayerMask.NameToLayer("Selection")) > 0) {
                renderer.GetSharedMaterials(sharedMaterials);
                if (renderer.gameObject.GetInstanceID() == SelectionObject.InstanceID) {
                    if (!sharedMaterials.Contains(selectedMaterial)) {
                        sharedMaterials.Add(selectedMaterial);
                        renderer.sharedMaterials = sharedMaterials.ToArray();
                    }
                }
                else {
                    if (sharedMaterials.Contains(selectedMaterial)) {
                        sharedMaterials.Remove(selectedMaterial);
                        renderer.sharedMaterials = sharedMaterials.ToArray();
                    }
                }
            }
        }
    }
}