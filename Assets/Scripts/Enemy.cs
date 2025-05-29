using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using BulletHell;
using static Unity.Burst.Intrinsics.X86.Avx;
using System.Data.SqlTypes;

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
    public LayerMask seeThroughLayers;
    public GameObject player;
    private PathMeasure pathMeasure;
    private AIPath aiPath;
    private Seeker seeker;
    public Vector2 dest;
    private bool moving;
    public float distance;
    public float timer;
    private float interval = 0.1f;
    public bool dying = false;
    private ProjectileEmitterAdvanced emitter;
    

    private void Awake()
    {
        player = GameObject.FindWithTag("Player");
        aiPath = this.GetComponent<AIPath>();
        seeker = GetComponent<Seeker>();
        emitter = GetComponentInChildren<ProjectileEmitterAdvanced>();
        pathMeasure = GetComponentInChildren<PathMeasure>();
        curState = State.wandering;
        moving = false;

    }
    private void Start()
    {
        //ProjectileManager.Instance.AddEmitter(GetComponent<ProjectileEmitterAdvanced>(), 1000);
    }


    protected virtual void Update() {

        timer += Time.deltaTime;

        if (timer >= interval)
        {
            manageStateTransition();
            timer = 0f;
        }
        onStateUpdate(curState);
        
    }

   
    protected virtual void manageStateTransition() {
        distance = pathMeasure.getDistance();
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
                if (AttackCondition())
                {
                    curState = State.attacking;
                }
                if (distance < fleeingThreshold)
                {
                    curState = State.fleeing;
                }
                break;
            case State.attacking:
                if (!AttackCondition())
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

    protected virtual bool AttackCondition() {
        return distance < attackingRadius;

    }
    private bool checkValid(Vector3 dest) {
        NNInfo nNInfo = AstarPath.active.GetNearest(dest);
        return nNInfo.node != null && nNInfo.node.Walkable;
    }

    //private bool canSeePlayer() { 
    //    RaycastHit2D = 
    //}
    protected void onStateUpdate(State state)
    {
        switch (curState)
        {
            case State.wandering:
                wanderBehavior();
                break;
            case State.chasing:
                chaseBehavior();
                break; 
            case State.attacking:
                attackBehavior();
                break;
            case State.fleeing:
                fleeBehavior();
                break;

        }
    }
    protected virtual void wanderBehavior() {
        if (!moving)
        {

            moving = true;
            Vector2 randDir = new Vector2(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)).normalized;
            dest = transform.position + new Vector3(randDir.x * Random.Range(0.5f, 2), randDir.y * Random.Range(0.5f, 2), 0);
            if (checkValid(dest))
            {
                aiPath.destination = dest;

            }
            else
            {
                aiPath.destination = transform.position;
            }

        }
        if (aiPath.reachedEndOfPath)
        {

            moving = false;
        }
    }
    protected virtual void chaseBehavior() {
        moving = false;
        dest = player.transform.position;
        aiPath.destination = dest;
    }

    protected virtual void attackBehavior() { }
    protected virtual void fleeBehavior()
    {


    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("PlayerBullet")) {
            takeDamage(collision.gameObject.GetComponent<Bullet>().damage);
        }
        
    }

    public void takeDamage(int damage) {
        if (!dying)
        {
            health -= damage;
            GameObject popUp = Instantiate(PopUp);
            popUp.GetComponent<DamagePopUp>().damageNum = damage;
            popUp.GetComponent<RectTransform>().localPosition = this.transform.position + 0.5f * Vector3.up;
            if (health <= 0) { die(); }
        }
        else {
            Debug.Log("already dying");
        }

    }

    private void die() {
        dying = true;
        emitter.AutoFire = false;
        MonoBehaviour[] comps = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour c in comps)
        {
            if (!(c is ProjectileEmitterAdvanced)) {
                c.enabled = false;
            }
           
        }
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }

        Renderer[] renderers = GetComponents<Renderer>();
        foreach (Renderer rend in renderers)
        {
            rend.enabled = false;
        }
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = false; // Disables the Rigidbody2D simulation
        }
        Destroy(gameObject,5f);

    }

}
