using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemy : Enemy
{
    public LayerMask layerMask;
    // Start is called before the first frame update


    private void Start()
    {
        layerMask = LayerMask.GetMask("Wall");
    }
    protected override bool AttackCondition()
    {
        return base.AttackCondition() && hasLineOfSight();
    }
    private bool hasLineOfSight() {
        RaycastHit2D hitInfo = Physics2D.Raycast(this.transform.position, (player.transform.position - this.transform.position),layerMask);
        if (hitInfo.collider != null){
            if (hitInfo.collider.gameObject.tag == "Player")
            {
                return true;
            }
        }
        return false;
        
    }
}
