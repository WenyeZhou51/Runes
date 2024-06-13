using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GlyphPopUp : MonoBehaviour
{
    private TextMeshPro text;
    MeshRenderer meshRenderer;
    public string description;
    private float creationTime;
    private float duration;
    public bool fading;
    // Start is called before the first frame update
    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        text = GetComponent<TextMeshPro>();
        creationTime = Time.time;
        duration = 1f;
        fading = false;
    }

    // Update is called once per frame
    private void Start()
    {
        text.SetText(description);
        meshRenderer.sortingOrder = 10;
    }
    void Update()
    {

        if (!fading)
        {
            creationTime = Time.time;
        }
        else {
            text.alpha -= Time.deltaTime;
        }
        if (Time.time - creationTime > duration)
        {
            Destroy(this.gameObject);
        }
    }
}
