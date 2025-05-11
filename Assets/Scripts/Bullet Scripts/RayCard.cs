using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ray", menuName = "Action Card/Ray")]
public class RayCard : ActionCards
{
    public override void Use(Vector2 useLoc, Quaternion useDir)
    {
        // Create a single ray projectile that will bounce
        GameObject rayInstance = Instantiate(bullet, useLoc, useDir);
        
        // Use reflection to set the private instance field in the base class
        var instanceField = typeof(ActionCards).GetField("instance", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (instanceField != null)
        {
            instanceField.SetValue(this, rayInstance);
        }
    }
} 