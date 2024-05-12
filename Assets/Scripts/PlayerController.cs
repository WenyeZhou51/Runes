using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 moveDir ;
    private Vector2 rollDir ;
    private Vector2 lastMoveDir;    
    [SerializeField] private float moveSpeed = 350;
    private float rollSpeed;
    [SerializeField] private float maxRollSpeed = 1500;
    [SerializeField] private float rollDelay = 0.7f;
    private float nextRoll;
    [SerializeField] private Camera cam;
    private Vector2 camPos;
    private Vector2 lookDir;
    public Weapon defaultWeaponOne;
    private float health;
    private float healthMax = 100f;
    public float mana;
    private float manaRegen = 50f;
    private float manaMax = 100f;
    public GameObject healthBar;
    public GameObject manaBar;
    private UnityEngine.UI.Slider hpSlider;
    private UnityEngine.UI.Slider manaSlider;
    private Animator animator;
    private bool attacking = false;
    private Transform firepoint;

    private enum State { 
        Normal,
        Rolling,
    }
    private State state;
    private void Awake() { 
        //Debug.Log("player initialized");
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        firepoint = transform.Find("FirePoint");
        state = State.Normal;
        Weapons = new List<Weapon>();        
        Weapons.Add(defaultWeaponOne);
        mana = manaMax;
        health = healthMax;
        hpSlider = healthBar.GetComponent<UnityEngine.UI.Slider>();
        manaSlider = manaBar.GetComponent<UnityEngine.UI.Slider>();
        hpSlider.maxValue = healthMax;
        hpSlider.value = health;
        manaSlider.maxValue = manaMax;
        manaSlider.value = mana;
       

        
    }
    private void Update()
    {
        handleMovementInput();
        TrackCurWeapon();
        if (Input.GetMouseButton(0))
        {
            Attack(firepoint.transform.position, firepoint.transform.rotation);
            if (state != State.Rolling) {
                attacking = true;
            }
            
        }
        else {
            attacking = false;
        }
        animator.SetBool("attacking", attacking);
        if (mana < manaMax)
        {
            mana += manaRegen * Time.deltaTime;
        }
        else {
            mana = manaMax;
        }
        manaSlider.value = mana;




    }


    private List<Weapon> Weapons;
    private Weapon curWeapon;

    public List<Weapon> getWeapons() {
        return this.Weapons;
    }
    public void Attack(Vector3 position,Quaternion rotation,bool instant = false) {

        if (firepoint.transform.position != position) { Debug.Log("called by onImpact"); }
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
        lookDir = (camPos - rb.position).normalized;
        animator.SetFloat("lookDirX", lookDir.x);
        animator.SetFloat("lookDirY", lookDir.y);
        firepoint.transform.position = this.transform.position + new Vector3(lookDir.x,lookDir.y,0);
        float angle = Mathf.Atan2(lookDir.y,lookDir.x)* Mathf.Rad2Deg;
        firepoint.rotation = Quaternion.Euler(0, 0, angle);
        //firepoint.rotation = Quaternion.LookRotation(lookDir,Vector3.up);
        //rb.rotation = angle;



    }



    

    private void handleMovementInput() {
        animator.SetInteger("state", (int)state);
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
                animator.SetFloat("horizontal_dir", moveX);
                animator.SetFloat("vertical_dir", moveY);
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
                if (!attacking)
                {
                    rb.velocity = moveDir * moveSpeed * Time.deltaTime;
                }
                else {
                    rb.velocity = new Vector2(0, 0);
                }
                break;
            case State.Rolling:
                rb.velocity = rollDir * rollSpeed * Time.deltaTime;
                break;
        }
        
    }



}
