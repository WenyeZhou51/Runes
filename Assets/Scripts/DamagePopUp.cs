using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamagePopUp : MonoBehaviour
{
    private TextMeshPro text;
    public int damageNum;
    private float creationTime;
    private float duration;
    // Start is called before the first frame update
    private void Awake()
    {
        text = GetComponent<TextMeshPro>();
        creationTime = Time.time;
        duration = 1f;
    }

    // Update is called once per frame
    private void Start()
    {
        text.SetText(damageNum.ToString());
    }
    void Update()
    {
        float speed = 0.7f;
        transform.position += speed*Vector3.up*Time.deltaTime;
        text.alpha -= 1.5f*Time.deltaTime;
        if (Time.time - creationTime > duration) { 
            Destroy(this.gameObject);
        }
    }
}
