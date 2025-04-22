using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurrentRotate : MonoBehaviour
{
    // Start is called before the first frame update
    public float rotateSpeed;

    // Update is called once per frame
    void FixedUpdate()
    {
        float rotation = rotateSpeed * Time.deltaTime;
        this.transform.Rotate(0, 0, rotation);
    }
}
