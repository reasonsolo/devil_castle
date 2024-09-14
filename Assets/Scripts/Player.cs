using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.MPE;
using UnityEngine;
using UnityEngine.Timeline;

public class Player : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Vector2 velo = new Vector2(5f, 12f);
    [SerializeField] private Animator anim;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundDistance;
    [Header("Dash")]
    [SerializeField] private float dashSpeedUp;
    [SerializeField] private float dashDuration;
    [SerializeField] private float dashCooldown;
    [Header("Attack")]
    [SerializeField] private float attackSpeed;
    [SerializeField] private float comboDelay = 1f;
    private float comboTimerWindow = 0;
    private bool doCombo = false;
    private bool isAttacking = false;
    private int comboCounter = 0;
    private static int maxCombo = 3;

    private float dashTime = 0;
    private float dashCD = 0;


    private float lastDirect = 1;
    private bool isJumping = false;
    private bool isJumping2 = false;
    private bool isGrounded = true;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        groundDistance = GetComponent<CapsuleCollider2D>().size.y / 2;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateStatus();
        React();
        Anime();
    }

    private void UpdateStatus() {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundDistance, groundLayer);
        isJumping = !isGrounded;
        if (!isJumping) {
            isJumping2 = false;
        }
        if (dashTime > 0) {
            dashTime -= Time.deltaTime;
            dashCD = dashCooldown;
        } else {
            if (dashCD > 0)
            {
                dashCD -= Time.deltaTime;
            }
        }
        if (comboTimerWindow > 0) {
            comboTimerWindow = math.max(comboTimerWindow - Time.deltaTime, 0);
        } else {
            comboCounter = 1;
        }
    }

    private void React()
    {
        int horizontalMove = (int)Input.GetAxisRaw("Horizontal");
        if (
        Attack(Input.GetButtonDown("Fire1"))
        || Jump(Input.GetButtonDown("Jump"))
        || Dash(Input.GetKeyDown(KeyCode.LeftShift))
        || Run(horizontalMove))
        {
            return;
        }
    }

    private bool Attack(bool control) {
        if (!control || !isGrounded || dashTime > 0) {
            return isAttacking;
        }
        doCombo = isAttacking;
        if (!doCombo) {
            comboCounter = 1;
        }
        isAttacking = true;
        comboTimerWindow = comboDelay;
        StopMoving();
        return isAttacking;
    }

    private bool Dash(bool control) {
        if (control && dashTime <= 0 && dashCD <= 0 && rb.velocity.x != 0) {
            dashTime = dashDuration;
            Move(new Vector2(rb.velocity.x * dashSpeedUp, 0));
        }
        return dashTime > 0;
    }

    private bool Run(int direct) {
        if (!isGrounded) {
            return false;
        }
        return Move(new Vector2(velo.x * direct, rb.velocity.y));
    }

    private bool StopMoving() {
        return Move(new Vector2(0, 0));
    }

    private bool Move(Vector2 v)
    {
        int direct = (int)math.sign(v.x);
        if (direct * lastDirect < 0)
        {
            Flip();
        }
        rb.velocity = v;
        if (direct != 0)
        {
            lastDirect = direct;
        }
        return rb.velocity.x != 0;
    }

    private bool Jump(bool control)
    {
        if (control) {
            if (isGrounded) {
                rb.velocity = new Vector2(rb.velocity.x, velo.y);
                isJumping = true;
            } else if (isJumping && !isJumping2) {
                rb.velocity = new Vector2(rb.velocity.x, velo.y);
                isJumping2 = true;
            }
            return true;
        }
        return false;
    }

    private void Anime()
    {
        anim.SetBool("isRunning", rb.velocity.x != 0);
        anim.SetBool("isJumping", isJumping);
        anim.SetBool("isJumping2", isJumping2);
        anim.SetFloat("yVelo", rb.velocity.y);
        anim.SetFloat("xVelo", rb.velocity.x);
        anim.SetBool("isDashing", dashTime > 0);
        anim.SetFloat("dashTime", dashTime);
        anim.SetBool("isAttacking", isAttacking);
        anim.SetInteger("comboCounter", comboCounter);
        anim.SetBool("doCombo", doCombo);
    }

    public void AttackOver() {
        Debug.Log("attack over " + comboTimerWindow + " combo " + doCombo + ": " + comboCounter);
        isAttacking = doCombo;
        if (doCombo && comboCounter < maxCombo) {
            comboCounter ++;
            comboTimerWindow = comboDelay;
        }
        if (comboCounter > maxCombo) {
            comboCounter = 1;
        }
        doCombo = false;
    }

    private void Flip()
    {
        transform.Rotate(0, 180, 0);
    }
}
