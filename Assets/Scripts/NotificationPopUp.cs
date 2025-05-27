using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NotificationPopUp : MonoBehaviour
{
    private TextMeshPro text;
    public string message;
    private float creationTime;
    private float duration;
    
    private void Awake()
    {
        text = GetComponent<TextMeshPro>();
        creationTime = Time.time;
        duration = 2f;
    }

    private void Start()
    {
        text.SetText(message);
        text.color = Color.yellow;
    }
    
    void Update()
    {
        float speed = 0.5f;
        transform.position += speed * Vector3.up * Time.deltaTime;
        text.alpha -= 0.75f * Time.deltaTime;
        if (Time.time - creationTime > duration) { 
            Destroy(this.gameObject);
        }
    }
} 