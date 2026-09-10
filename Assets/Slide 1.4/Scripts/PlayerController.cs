using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float runSpeed = 10f;
    [SerializeField] float jumpSpeed = 8f;
    [SerializeField] float climbSpeed = 5f;
    [SerializeField] Vector2 deathKick = new Vector2 (5f, 5f);
    [SerializeField] GameObject bullet;
    [SerializeField] Transform gun;
    
    Vector2 moveInput;
    Rigidbody2D myRigidbody;
    Animator myAnimator;
    CapsuleCollider2D myBodyCollider;
    BoxCollider2D myFeetCollider;
    float gravityScaleAtStart;

    bool isAlive = true;

    void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        myBodyCollider = GetComponent<CapsuleCollider2D>();
        myFeetCollider = GetComponent<BoxCollider2D>();
        if (myRigidbody != null)
        {
            gravityScaleAtStart = myRigidbody.gravityScale;
        }
    }

    void Update()
    {
        if (!isAlive) { return; }

        // Đọc input ngang (A/D hoặc mũi tên trái/phải)
        float x = 0f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x = -1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x = 1f;
        moveInput.x = x;

        // Đọc input dọc (W/S hoặc mũi tên lên/xuống) - dùng cho leo thang
        float y = 0f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) y = -1f;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) y = 1f;
        moveInput.y = y;

        Run();
        FlipSprite();
        ClimbLadder();
        Jump();
        Die();
    }

    void Jump()
    {
        bool jumpPressed = Keyboard.current.spaceKey.wasPressedThisFrame
                         || Keyboard.current.wKey.wasPressedThisFrame
                         || Keyboard.current.upArrowKey.wasPressedThisFrame;

        if (jumpPressed)
        {
            if (myFeetCollider != null && myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Ground")))
            {
                myRigidbody.linearVelocity = new Vector2(myRigidbody.linearVelocity.x, jumpSpeed);
            }
        }
    }

    void Run()
    {
        Vector2 playerVelocity = new Vector2(moveInput.x * runSpeed, myRigidbody.linearVelocity.y);
        myRigidbody.linearVelocity = playerVelocity;

        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidbody.linearVelocity.x) > Mathf.Epsilon;
        if (myAnimator != null)
        {
            myAnimator.SetBool("isRunning", playerHasHorizontalSpeed);
        }
    }

    void FlipSprite()
    {
        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidbody.linearVelocity.x) > Mathf.Epsilon;

        if (playerHasHorizontalSpeed)
        {
            transform.localScale = new Vector2(Mathf.Sign(myRigidbody.linearVelocity.x), 1f);
        }
    }

    void ClimbLadder()
    {
        if (myFeetCollider == null || !myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Climbing"))) 
        { 
            if (myRigidbody != null) myRigidbody.gravityScale = gravityScaleAtStart;
            if (myAnimator != null) myAnimator.SetBool("isClimbing", false);
            return;
        }
        
        Vector2 climbVelocity = new Vector2(myRigidbody.linearVelocity.x, moveInput.y * climbSpeed);
        myRigidbody.linearVelocity = climbVelocity;
        myRigidbody.gravityScale = 0f;

        bool playerHasVerticalSpeed = Mathf.Abs(myRigidbody.linearVelocity.y) > Mathf.Epsilon;
        if (myAnimator != null)
        {
            myAnimator.SetBool("isClimbing", playerHasVerticalSpeed);
        }
    }

    void Die()
    {
        if (myBodyCollider != null && myBodyCollider.IsTouchingLayers(LayerMask.GetMask("Enemies", "Hazards")))
        {
            isAlive = false;
            if (myAnimator != null) myAnimator.SetTrigger("Dying");
            if (myRigidbody != null) myRigidbody.linearVelocity = deathKick;
            // FindObjectOfType<GameSession>().ProcessPlayerDeath();
        }
    }
}