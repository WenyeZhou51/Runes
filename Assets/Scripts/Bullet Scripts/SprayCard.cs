using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[CreateAssetMenu(fileName = "Spray", menuName = "Action Card/Spray")]
public class SprayCard : ActionCards
{
    public int projectileCount = 15;
    private float coneAngle = 90f; // 90 degree cone
    private GameObject lastInstance; // Keep track of the last instance for GetInstance()
    
    // Use reflection to set the private field in the base class
    private FieldInfo instanceField;
    
    private void OnEnable()
    {
        // Get the private field info using reflection
        instanceField = typeof(ActionCards).GetField("instance", 
            BindingFlags.NonPublic | BindingFlags.Instance);
    }
    
    public override void Use(Vector2 useLoc, Quaternion useDir)
    {
        // Extract the euler angle from the rotation
        float baseAngle = useDir.eulerAngles.z;
        
        // Spawn multiple projectiles in a cone pattern
        for (int i = 0; i < projectileCount; i++)
        {
            // Calculate random angle within cone range
            float spreadAngle = Random.Range(-coneAngle/2, coneAngle/2);
            float finalAngle = baseAngle + spreadAngle;
            
            // Create quaternion for this bullet
            Quaternion bulletRotation = Quaternion.Euler(0, 0, finalAngle);
            
            // Instantiate the bullet with the spread rotation
            lastInstance = Instantiate(bullet, useLoc, bulletRotation);
            
            // Use reflection to set the private field in the base class
            if (instanceField != null && i == 0) // Only set for the first bullet
            {
                instanceField.SetValue(this, lastInstance);
            }
        }
    }
    
    // Override GetInstance to return our last created instance
    public new GameObject GetInstance()
    {
        return lastInstance ?? base.GetInstance();
    }
} 