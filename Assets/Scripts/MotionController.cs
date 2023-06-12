using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MotionController : MonoBehaviour
{
    GameplayInputActions inputAction = null;

    [SerializeField, Range(0.0f, 10.0f)]
    float playerVelocity = 1.0f;

    [SerializeField, Range(0.1f, 5.0f)]
    float jumpHeight = 1.0f;

    bool isJumping = false;

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
        
    }

    // Update is called once per frame
    void Update()
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
            StartCoroutine(JumpPlayerCoroutine());
        }

        MoveLtOrRt(deltaX);
    }

    private void MoveLtOrRt(float deltaX)
    {
        transform.position = new Vector2(transform.position.x + deltaX, transform.position.y);
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
