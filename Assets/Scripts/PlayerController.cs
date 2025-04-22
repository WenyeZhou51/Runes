using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using JetBrains.Annotations;

#if UNITY_EDITOR
using Edgar.Unity.Editor;
#endif


public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 moveDir ;
    private Vector2 rollDir ;
    private Vector2 lastMoveDir;    
    [SerializeField] private float moveSpeed;
    private float rollSpeed;
    [SerializeField] private float maxRollSpeed = 1400;
    [SerializeField] private float rollDelay = 0.7f;
    private float nextRoll;
    [SerializeField] private Camera cam;
    private Vector2 camPos;
    private Vector2 lookDir;
    public Weapon defaultWeaponOne;
    private float health;
    public float mana;
    public GameObject healthBar;
    public GameObject manaBar;
    public GameObject playerTakeDamagePopUp;
    private UnityEngine.UI.Slider hpSlider;
    private UnityEngine.UI.Slider manaSlider;
    private Animator animator;
    private bool attacking = false;
    private Transform firepoint;
    private GameObject currentHoveredObject = null;
    public LayerMask interactableLayer;
    public bool isPaused = false;
    public float hitPause = 0.25f;
    public bool invincible = false;



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
        if (MyGameManager.Instance.PlayerPosition != null) {
            this.transform.position = (Vector3)MyGameManager.Instance.PlayerPosition;
            MyGameManager.Instance.PlayerPosition = null;
        }

        moveSpeed = MyGameManager.Instance.MoveSpeed;
        health = MyGameManager.Instance.Health;
        mana = MyGameManager.Instance.Mana;
        hpSlider = healthBar.GetComponent<UnityEngine.UI.Slider>();
        manaSlider = manaBar.GetComponent<UnityEngine.UI.Slider>();
        hpSlider.maxValue = MyGameManager.Instance.MaxHealth;
        hpSlider.value = health;
        manaSlider.maxValue = MyGameManager.Instance.MaxMana;
        manaSlider.value = mana;
        
        // Initialize movement direction vector to prevent null issues
        moveDir = Vector2.zero;
        lastMoveDir = Vector2.right; // Default direction
    }
    private void Update()
    {
        bool dialoguePlaying = false;
        // Check if DialogueManager exists and get dialogue state
        if (DialogueManager.GetInstance() != null) {
            dialoguePlaying = DialogueManager.GetInstance().dialoguePlaying;
        }
        
        if (!dialoguePlaying) {
            handleMovementInput();
            DetectMouseOver();
            TrackCurWeapon();
            if (Input.GetKeyDown(KeyCode.P))
            {
                TogglePause();
            }
            if (Input.GetKeyDown(KeyCode.Q))
            {
                Application.Quit();
            }
            if (Input.GetMouseButton(0))
            {
                Attack(firepoint.transform.position, firepoint.transform.rotation);
                if (state != State.Rolling)
                {
                    attacking = true;
                }


            }
            else
            {
                attacking = false;
            }
            animator.SetBool("attacking", attacking);
            if (mana < MyGameManager.Instance.MaxMana)
            {
                mana += MyGameManager.Instance.ManaRegen * Time.deltaTime;
                MyGameManager.Instance.Mana = mana;
            }
            else
            {
                mana = MyGameManager.Instance.MaxMana;
                MyGameManager.Instance.Mana = mana;
            }
            manaSlider.value = mana;
        }
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



    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;
            Debug.Log("Game Paused");
        }
        else
        {
            Time.timeScale = 1f;
            Debug.Log("Game Resumed");
        }
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
                invincible = true;
                float rollSpeedDrop = 5f;
                rollSpeed -= rollSpeed * rollSpeedDrop * Time.deltaTime;
                float rollSpeedMin = 300f;
                if (rollSpeed < rollSpeedMin)
                {
                    state = State.Normal;
                    invincible = false;
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

    void DetectMouseOver()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPosition, Vector2.zero, Mathf.Infinity, interactableLayer);


        
        if (hit.collider != null)
        {
            if (hit.collider.gameObject.GetComponent<GenericGlyph>() != null)
            {

                currentHoveredObject = hit.collider.gameObject;
                currentHoveredObject.GetComponent<GenericGlyph>().HandleMouseOver();
                if (Input.GetMouseButtonDown(1))
                {
                    DescriptionManager.Instance.currentGlyph = currentHoveredObject.GetComponent<GenericGlyph>();
                    DescriptionManager.Instance.ShowInputField();

                }
            }
            else
            {
                if (currentHoveredObject != null)
                {
                    currentHoveredObject.GetComponent<GenericGlyph>().HandleMouseExit();
                    currentHoveredObject = null;
                }
            }
        }
        else
        {
            if (currentHoveredObject != null)
            {
                currentHoveredObject.GetComponent<GenericGlyph>().HandleMouseExit();
                currentHoveredObject = null;
            }
        }
        
       
    }

    public void TakeDamage(int damage) {
        if (invincible || DialogueManager.GetInstance().dialoguePlaying) { return;  }
        Debug.Log("taking damage");
        health -= damage;
        MyGameManager.Instance.Health = health;
        StartCoroutine(HitPause(hitPause));
        GameObject popUp = Instantiate(playerTakeDamagePopUp);
        popUp.GetComponent<PlayerTakeDamagePopUp>().damageNum = damage;
        popUp.GetComponent<RectTransform>().position = this.transform.position+0.5f * Vector3.up;
        hpSlider.value = health;
        if (health <= 0) { playerDeath(); } 
    }

    private IEnumerator HitPause( float hitPause ) {
        Time.timeScale = 0.5f;
        invincible = true;
        yield return new WaitForSecondsRealtime(hitPause);
        invincible = false;
        Time.timeScale = 1f;
        
        // Ensure movement is reset after hit pause
        if (state == State.Normal) {
            moveDir = Vector2.zero;
        }
    }

    private void playerDeath() {
        Destroy(gameObject);
        SceneManagerScript.Instance.restart();
        Time.timeScale = 1f;
    }


}
