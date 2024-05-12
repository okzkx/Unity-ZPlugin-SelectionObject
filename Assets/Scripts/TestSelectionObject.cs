using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSelectionObject : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update() {
        SelectionObject.FindObjectOfScreenPosition(Input.mousePosition);
        
        
    }
}
