using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 moveDir ;
    private Vector2 rollDir ;
    private Vector2 lastMoveDir;    [SerializeField] private float moveSpeed = 350;
    private float rollSpeed;
    [SerializeField] private float maxRollSpeed = 1500;
    [SerializeField] private float rollDelay = 0.7f;
    private float nextRoll;
    [SerializeField] private Camera cam;
    private Vector2 camPos;
    private Vector2 lookDir;
    public Weapon defaultWeaponOne;
    public float health;
    public float healthMax = 100f;
    public float mana;
    public float manaRegen = 1f;
    public float manaMax = 100f;

    private enum State { 
        Normal,
        Rolling,
    }
    private State state;
    private void Awake() { 
        //Debug.Log("player initialized");
        rb = GetComponent<Rigidbody2D>();
        state = State.Normal;
        Weapons = new List<Weapon>();        
        Weapons.Add(defaultWeaponOne);
        mana = manaMax;
        health = healthMax;

        
    }
    private void Update()
    {
        handleMovementInput();
        TrackCurWeapon();
        if (Input.GetMouseButton(0)) {
            Attack(this.transform.position,this.transform.rotation);
        }
        if (mana < manaMax)
        {
            mana += manaRegen * Time.deltaTime;
        }
        else {
            mana = manaMax;
        }





    }


    private List<Weapon> Weapons;
    private Weapon curWeapon;

    public List<Weapon> getWeapons() {
        return this.Weapons;
    }
    public void Attack(Vector3 position,Quaternion rotation,bool instant = false) {

        if (this.transform.position != position) { Debug.Log("called by onImpact"); }
        curWeapon.fireWeapon(position,rotation, instant);
    }


    private void TrackCurWeapon()
    {
        curWeapon = Weapons[0];
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            curWeapon = Weapons[0];
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            curWeapon = Weapons[1];
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            curWeapon = Weapons[2];
        }
    }

    private void FixedUpdate()
    {
        handleMovement();
        lookDir = camPos - rb.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
        rb.rotation = angle;



    }



    

    private void handleMovementInput() {
        switch (state)
        {
            case State.Normal:
                float moveX = 0f;
                float moveY = 0f;

                if (Input.GetKey(KeyCode.W))
                {
                    moveY = 1f;
                }
                if (Input.GetKey(KeyCode.S))
                {
                    moveY = -1f;
                }
                if (Input.GetKey(KeyCode.A))
                {
                    moveX = -1f;
                }
                if (Input.GetKey(KeyCode.D))
                {
                    moveX = 1f;
                }
                moveDir = new Vector3(moveX, moveY).normalized;
                if (moveX != 0 || moveY != 0)
                {
                    lastMoveDir = moveDir;
                }
                if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time >= nextRoll)
                {
                    rollDir = lastMoveDir;
                    state = State.Rolling;
                    rollSpeed = maxRollSpeed;
                    nextRoll = Time.time + rollDelay;
                }
                break;
            case State.Rolling:
                float rollSpeedDrop = 5f;
                rollSpeed -= rollSpeed * rollSpeedDrop * Time.deltaTime;
                float rollSpeedMin = 300f;
                if (rollSpeed < rollSpeedMin)
                {
                    state = State.Normal;
                }
                break;
        }

        camPos = cam.ScreenToWorldPoint(Input.mousePosition);

    }

    

    private void handleMovement()
    {
        switch (state)
        {
            case State.Normal:
                rb.velocity = moveDir * moveSpeed * Time.deltaTime;
                break;
            case State.Rolling:
                rb.velocity = rollDir * rollSpeed * Time.deltaTime;
                break;
        }

        
    }

}
