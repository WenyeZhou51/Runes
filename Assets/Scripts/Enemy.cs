using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class Enemy : MonoBehaviour
{
    public GameObject PopUp;
    public int health;
    public enum State { 
        wandering,
        chasing,
        attacking,
        fleeing
    
    }
    public State curState;
    public float aggroRadius;
    public float deaggroRadius;
    public float attackingRadius;
    public float stopAttackRadius;
    public float fleeingThreshold;
    public float stopFleeingThreshold;
    public GameObject player;
    private AIPath aiPath;
    public Vector2 dest;
    private bool moving;
    

    private void Awake()
    {
        player = GameObject.FindWithTag("Player");
        aiPath = this.GetComponent<AIPath>();
        curState = State.wandering;
        moving = false;
    }


    protected virtual void Update()
    {
        onStateUpdate(curState);
        manageStateTransition();
    }


    protected virtual void manageStateTransition() {
        float distance = Vector2.Distance(player.transform.position, this.transform.position);
        switch (curState)
        {
            case State.wandering:
                if (distance < aggroRadius) {
                    curState = State.chasing;
                }
                if (distance < fleeingThreshold) {
                    curState = State.fleeing;
                }
                break;
            case State.chasing:
                if (distance > deaggroRadius)
                {
                    curState = State.wandering;
                }
                if (distance < attackingRadius)
                {
                    curState = State.attacking;
                }
                if (distance < fleeingThreshold)
                {
                    curState = State.fleeing;
                }
                break;
            case State.attacking:
                if (distance > stopAttackRadius)
                {
                    curState = State.chasing;
                }
                if (distance < fleeingThreshold)
                {
                    curState = State.fleeing;
                }
                break;
            case State.fleeing:
                if (distance > stopFleeingThreshold) {
                    if (distance < attackingRadius)
                    {
                        curState = State.attacking;
                    }
                    else if (distance < aggroRadius)
                    {
                        curState = State.chasing;
                    }
                    else {
                        curState = State.wandering;
                    }
                }
                break;


        }
    }
    private bool checkValid(Vector3 dest) {
        NNInfo nNInfo = AstarPath.active.GetNearest(dest);
        return nNInfo.node != null && nNInfo.node.Walkable;
    }

    protected virtual void onStateUpdate(State state)
    {
        switch (curState)
        {
            case State.wandering:
                if (!moving) {

                    moving = true;
                    Vector2 randDir = new Vector2(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)).normalized;
                    dest = transform.position + new Vector3 (randDir.x* Random.Range(0.5f, 2), randDir.y * Random.Range(0.5f, 2),0);
                    if (checkValid(dest))
                    {
                        aiPath.destination = dest;
                    }
                    else {
                        aiPath.destination = transform.position;
                    }
                    
                }
                if (aiPath.reachedEndOfPath) {
                    moving = false;
                }
                

                break;
            case State.chasing:
                dest = player.transform.position;
                aiPath.destination = dest;
                break;
            case State.attacking:
                break;
            case State.fleeing:
                break;

        }
    }




    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("PlayerBullet")) {
            takeDamage(collision.gameObject.GetComponent<Bullet>().damage);
        }
        
    }

    private void takeDamage(int damage) {
        health -= damage;
        GameObject popUp = Instantiate(PopUp);
        popUp.GetComponent<DamagePopUp>().damageNum = damage;
        popUp.GetComponent<RectTransform>().localPosition = this.transform.position+0.5f*Vector3.up;
        if (health <= 0) { Destroy(this.gameObject); }
    }

}
