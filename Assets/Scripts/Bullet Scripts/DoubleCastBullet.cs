using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleCastBullet : Bullet
{
    // This is just a dummy bullet for visual representation
    // The actual effect is handled by the DoubleCastCard script
    
    protected override void Awake()
    {
        base.Awake();
        // Destroy instantly as this is just a visual cue
        Destroy(gameObject, 0.1f);
    }
} 