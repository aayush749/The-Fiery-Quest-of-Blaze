using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MotionController : MonoBehaviour
{
    GameplayInputActions inputAction = null;

    [SerializeField, Range(0.0f, 10.0f)]
    float playerVelocity = 1.0f;

    //[SerializeField, Range(0.1f, 500.0f)]
    float jumpHeight = 2.0f;

    bool isJumping = false;

    private Animator animator = null;

    [SerializeField]
    float minXLocationConstraint = 0.0f;

    [SerializeField]
    float maxXLocationConstraint = 0.0f;

    [SerializeField]
    Transform groundIndicator = null;

    [SerializeField]
    LayerMask groundLayerMask;

    [SerializeField]
    private float groundCheckRadius;

    [SerializeField]
    private Vector2 leftWallCheckOffset, rightWallCheckOffset;
    
    Rigidbody2D rb2d = null;

    private void Awake()
    {
        inputAction = new GameplayInputActions();
    }

    private void OnEnable()
    {
        inputAction.Enable();
    }

    private void OnDisable()
    {
        inputAction.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        rb2d = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void FixedUpdate()
    {
        HandleMovementInput();
    }

    private void HandleMovementInput()
    {
        float rtMotionStrngth = inputAction.Gameplay.MoveRight.ReadValue<float>();
        float ltMotionStrngth = inputAction.Gameplay.MoveLeft.ReadValue<float>();

        bool jump = inputAction.Gameplay.Jump.ReadValue<float>() == 1.0f ? true : false;


        var deltaX = (rtMotionStrngth - ltMotionStrngth) * playerVelocity * Time.deltaTime;
        
        if (!isJumping && jump)
        {
            //StartCoroutine(JumpPlayerCoroutine());
            rb2d.velocity = new Vector2(rb2d.velocity.x, jumpHeight);
        }

        MoveLtOrRt(deltaX);
    }

    private void MoveLtOrRt(float deltaX)
    {
        deltaX *= 10.0f;

        var position = groundIndicator;
        bool touchesWall = Physics2D.OverlapCircle((Vector2)position.transform.position + leftWallCheckOffset, groundCheckRadius, groundLayerMask)
                           ||Physics2D.OverlapCircle((Vector2) position.transform.position + rightWallCheckOffset, groundCheckRadius, groundLayerMask);

        var ltCircleCenter = (Vector2) position.transform.position + leftWallCheckOffset;
        var rtCircleCenter = (Vector2) position.transform.position + rightWallCheckOffset;

        MyDebug.DrawCircle(new Vector3(ltCircleCenter.x, ltCircleCenter.y, 0.0f), groundCheckRadius, 1000, Color.red);
        MyDebug.DrawCircle(new Vector3(rtCircleCenter.x, rtCircleCenter.y, 0.0f), groundCheckRadius, 1000, Color.red);

        if (touchesWall)
        {
            animator.SetBool("IsWalking", false);
            return;
        }

        // update animator state
        if (deltaX != 0.0f)
        {
            animator.SetBool("IsWalking", true);
            if (deltaX < 0.0f)
            {
                // change the scale to -ve of original, to show moving leftwards
                if (transform.localScale.x > 0.0f)
                {
                    transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
                }
            }
            else
            {
                // change the scale to +ve of original, to show moving rightwards
                if (transform.localScale.x < 0.0f)
                {
                    transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                }
            }

            // change the actual position (respecting the location constraints)
            var newXPos = Mathf.Clamp(transform.position.x + deltaX, minXLocationConstraint, maxXLocationConstraint);
            transform.position = new Vector2(newXPos, transform.position.y);
        }
        else
            animator.SetBool("IsWalking", false);

        Debug.LogFormat($"Velocity: {playerVelocity}, deltaX: {deltaX * 10.0f}");
        animator.SetFloat("WalkingSpeed", playerVelocity * Mathf.Abs(deltaX * 10.0f));
    }

    private IEnumerator JumpPlayerCoroutine()
    {
        isJumping = true;

        var initialHt = transform.position.y;

        // make the player reach an elevated height
        var deltaHeight = 0.1f;

        while (transform.position.y < initialHt + jumpHeight)
        {
            transform.position = new Vector2(transform.position.x, transform.position.y + deltaHeight);
            deltaHeight += 0.1f;

            yield return null;
        }

        deltaHeight = 0.1f;
        // bring the player back to the initial height
        while (transform.position.y > initialHt)
        {
            transform.position = new Vector2(transform.position.x, transform.position.y - deltaHeight);
            deltaHeight += 0.1f;
            yield return null;
        }

        isJumping = false;
    }
}
