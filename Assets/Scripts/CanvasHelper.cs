using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class CanvasHelper : MonoBehaviour
{
    private Canvas canvas;
    private Transform canvasTransform;
    // Start is called before the first frame update
    void Start()
    {
        canvas = GetComponent<Canvas>();
        canvasTransform = canvas.transform;
        canvasTransform.localScale = Vector3.one;
    }

    // Update is called once per frame
    void Update()
    {
        canvasTransform.localScale = Vector3.one;
    }
}
