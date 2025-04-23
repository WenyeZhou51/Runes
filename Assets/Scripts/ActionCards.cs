using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "New Action Card", menuName = "Action Card")]
public class ActionCards : Cards
{
    
    public GameObject bullet;
    private GameObject instance;
    public virtual void Use(Vector2 useLoc, Quaternion useDir) {
        instance = Instantiate(bullet, useLoc, useDir);
        
    }

    public GameObject GetBullet() { return this.bullet; }
    public GameObject GetInstance() { return this.instance; }
}
