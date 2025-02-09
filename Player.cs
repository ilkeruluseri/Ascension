using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour
{

    public float maxJumpHeight = 4;
    public float minJumpHeight = 1;
    public float timeToJumpApex = .4f;
    float accelerationTimeAirborne = .2f;
    float accelerationTimeGrounded = .1f;
    float moveSpeed = 10;
    float maxSpedUpMoveSpeed = 15;


    public Vector2 wallJumpClimb;
    public Vector2 wallJumpOff;
    public Vector2 wallLeap;

    public float wallSlideSpeedMax = 3;
    public float wallStickTime = .25f;
    float timeToWallUnstick;

    public float gravityMultiplier = 2f;
    float gravity;
    float maxJumpVelocity;
    float minJumpVelocity;
    Vector3 velocity;
    float velocityXSmoothing;

    Controller2D controller;
    Animator animator;
    SpriteRenderer spriteRenderer;
    AudioManager audioManager;
    public ParticleSystem dustRun;
    public ParticleSystem dustJump;
    public ParticleSystem dustSlide;
    public ParticleSystem dustWallJumpRight;
    public ParticleSystem dustWallJumpLeft;
    public TrailRenderer airTrail;

    Vector2 directionalInput;
    bool wallSliding;
    int wallDirX;

    private Vector2 lastPosition;
    private Vector2 currentVelocity;
    float momentumDecayRate = 1f;
    private bool isOnPlatform = false;

    //Animation stuff
    bool isMoving;
    bool isJumping;
    bool isIdle;

    [Header("Sound")]
    [SerializeField] private List<AudioClip> footSteps;

    AudioSource audioSource;

    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<Controller2D>();
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioManager = GetComponent<AudioManager>();

        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
        lastPosition = transform.position;

        audioManager.StopAll();
        audioManager.Play("music" + SceneManager.GetActiveScene().name);

    }

    void Update()
    {
        CalculateVelocity();
        HandleWallSliding();

        controller.Move(velocity * Time.deltaTime, directionalInput);

        if (controller.collisions.above || controller.collisions.below)
        {
            if (controller.collisions.slidingDownMaxSlope)
            {
                velocity.y += controller.collisions.slopeNormal.y * -gravity * Time.deltaTime;
            }
            else
            {
                velocity.y = 0;
            }
        }

        // Calculate the current velocity based on the change in position
        currentVelocity = (Vector2)transform.position - lastPosition;
        currentVelocity /= Time.deltaTime;

        // Update lastPosition to the current position for the next frame
        lastPosition = transform.position;

        //Debug.Log(currentVelocity);
        

        HandleAnimations();
    }

    public void SetDirectionalInput(Vector2 input)
    {
        directionalInput = input;
    }

    public void OnJumpInputDown()
    {
        if (wallSliding)
        {
            if (wallDirX == directionalInput.x)
            {
                velocity.x = -wallDirX * wallJumpClimb.x;
                velocity.y = wallJumpClimb.y;
            }
            else if (directionalInput.x == 0)
            {
                velocity.x = -wallDirX * wallJumpOff.x;
                velocity.y = wallJumpOff.y;
            }
            else
            {
                velocity.x = -wallDirX * wallLeap.x;
                velocity.y = wallLeap.y;
                Debug.Log("Leaped with x: " + velocity.x + " y: " + velocity.y);
            }
        }
        if (controller.collisions.below)
        {
            if (controller.collisions.slidingDownMaxSlope)
            {
                if (directionalInput.x != -Mathf.Sign(controller.collisions.slopeNormal.x))
                { // not jumping against max slope
                    velocity.y = maxJumpVelocity * controller.collisions.slopeNormal.y;
                    velocity.x = maxJumpVelocity * controller.collisions.slopeNormal.x;
                }
            }
            else
            {
                velocity.y = maxJumpVelocity + currentVelocity.y;
                RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 5f, LayerMask.GetMask("Platform"));
                if (hit)
                {
                    Debug.Log("Platform hit");
                    velocity.x = currentVelocity.x;
                }
                else
                {
                    if (Mathf.Abs(currentVelocity.x) < maxSpedUpMoveSpeed)
                    {
                        if (Mathf.Sign(currentVelocity.x) == 1)
                        {
                            velocity.x += 1f;
                        }
                        else
                        {
                            velocity.x -= 1f;
                        }
                    }
                }
                //Debug.Log("Jumped with x: " + velocity.x + " y: " + velocity.y);
            }
        }
        if (!isJumping)
        {
            audioManager.Play("Jump");

            if (controller.collisions.left)
            {
                dustWallJumpLeft.Play();
            }
            else if (controller.collisions.right)
            {
                dustWallJumpRight.Play();
            }
            else
            {
                dustJump.Play();
            }
            
        }
        isJumping = true;
    }

    public void OnJumpInputUp()
    {
        if (velocity.y > minJumpVelocity)
        {
            velocity.y = minJumpVelocity;
        }
    }

    float wallSlideTimer = 0f;
    void HandleWallSliding()
    {
        wallDirX = (controller.collisions.left) ? -1 : 1;
        wallSliding = false;
        if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0)
        {
            wallSliding = true;
            if (!audioManager.IsPlaying("Slide") && wallSlideTimer > 0.2f)
            {
                audioManager.Play("Slide");
            }
            else
            {
                wallSlideTimer += Time.deltaTime;
            }

            if (velocity.y < -wallSlideSpeedMax)
            {
                velocity.y = -wallSlideSpeedMax;
            }

            if (timeToWallUnstick > 0)
            {
                velocityXSmoothing = 0;
                velocity.x = 0;

                if (directionalInput.x != wallDirX && directionalInput.x != 0)
                {
                    timeToWallUnstick -= Time.deltaTime;
                }
                else
                {
                    timeToWallUnstick = wallStickTime;
                }
            }
            else
            {
                timeToWallUnstick = wallStickTime;
            }

        }
        else
        {
            if (audioManager.IsPlaying("Slide"))
            {
                audioManager.Stop("Slide");
                wallSlideTimer = 0f;
            }
        }

    }

    void CalculateVelocity()
    {
        float targetVelocityX = directionalInput.x * moveSpeed;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1f, LayerMask.GetMask("Obstacles"));
        if (directionalInput.x == 0 && hit) // If not trying to move, slow down much faster, just not when on moving platforms
        {
            momentumDecayRate = 240f; 
        }
        else if (hit)
        {
            momentumDecayRate = 1f;
        }
        else
        {
            momentumDecayRate = 0f;
        }
        if (Mathf.Abs(currentVelocity.x) > moveSpeed && Mathf.Sign(directionalInput.x) == Mathf.Sign(currentVelocity.x))
        {
            if (Mathf.Sign(directionalInput.x) == 1)
            {
                velocity.x -= momentumDecayRate * Time.deltaTime;
            }
            else
            {
                velocity.x += momentumDecayRate * Time.deltaTime;
            }
        }
        else if (Mathf.Abs(currentVelocity.x) > moveSpeed && directionalInput == Vector2.zero)
        {
            if (Mathf.Sign(currentVelocity.x) == 1)
            {
                velocity.x -= momentumDecayRate * Time.deltaTime;
            }
            else
            {
                velocity.x += momentumDecayRate * Time.deltaTime;
            }
        }
        else
        {
            velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
        }
        velocity.y = Mathf.Clamp(gravityMultiplier * gravity * Time.deltaTime + velocity.y, -50f, float.PositiveInfinity); //
    }

    public void SetIsOnPlatform(bool val)
    {
        isOnPlatform = val;
    }

    public Vector2 GetCurrentVelocity()
    {
        return currentVelocity;
    }

    int particleDir = 1;
    private void HandleAnimations()
    {
        if (isJumping && controller.collisions.below || controller.collisions.right || controller.collisions.left) // isJumping set on jumpButtownDown
        {
            isJumping = false;
        }

        if (Mathf.Abs(directionalInput.x) > 0 && !isJumping && !wallSliding)
        {
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }
        if (!isMoving && !isJumping && !wallSliding)
        {
            isIdle = true;
        }
        else
        {
            isIdle = false;
        }

        if (!wallSliding)
        {
            if (directionalInput.x >= 0)
            {
                spriteRenderer.flipX = false;
            }
            else
            {
                spriteRenderer.flipX = true;
            }
        }
        else
        {
            if (controller.collisions.left)
            {
                spriteRenderer.flipX = true;
            }
            else
            {
                spriteRenderer.flipX = false;
            }
        }
        

        animator.SetBool("isMoving", isMoving);
        animator.SetBool("isJumping", isJumping);
        animator.SetBool("isWallSliding", wallSliding);
        animator.SetBool("isIdle", isIdle);

        wallDirX = (controller.collisions.left) ? -1 : 1;

        if (controller.collisions.below)
        {
            PlayDustRun();
            airTrail.emitting = false;
        }
        else if (!controller.collisions.right && !controller.collisions.left)
        {
            PlayAirTrail();
        }
        else
        {
            if (airTrail.emitting)
            {
                airTrail.emitting = false;
            }
        }
        
        if ((Mathf.Abs(currentVelocity.x) < 12f || !controller.collisions.below) && dustRun.isPlaying)
        {
            dustRun.Stop();
        }

        if (wallSliding && !dustSlide.isPlaying)
        {
            if (wallDirX == -1) // wall on left
            {
                if (particleDir == 1) // particle on right
                {
                    dustSlide.transform.RotateAround(transform.position, Vector3.back, 180f);
                    dustSlide.transform.RotateAround(transform.position, Vector3.right, 180f);
                    particleDir = -1;
                }
            }
            else // wall on right
            {
                if (particleDir == -1) // particle on left
                {
                    dustSlide.transform.RotateAround(transform.position, Vector3.back, 180f);
                    dustSlide.transform.RotateAround(transform.position, Vector3.right, 180f);
                    particleDir = 1;
                }
            }
            dustSlide.Play();
        }
        else
        {
            if (dustSlide.isPlaying && !wallSliding)
            {
                dustSlide.Stop();
            }
        }
    }

    public Vector2 GetDirectionalInput()
    {
        return directionalInput;
    }

    private void PlayFootStep()
    {
        AudioClip clip = footSteps[Random.Range(0, footSteps.Count)];

        audioSource.clip = clip;
        audioSource.pitch = Random.Range(0.8f, 1.2f);
        audioSource.volume = Random.Range(0.5f, 0.7f);

        audioSource.Play();
    }

    int runDustDir = -1; // Dust going left
    public void PlayDustRun()
    {
        if (!dustRun.isPlaying)
        {
            if (Mathf.Abs(currentVelocity.x) > 12f)
            {
                if (directionalInput.x >= 0) // going right
                {
                    if (runDustDir == 1) // dust going right
                    {
                        dustRun.transform.RotateAround(dustRun.transform.position, Vector3.up, 180);
                    }
                }
                else // going left
                {
                    if (runDustDir == -1) // dust going left
                    {
                        dustRun.transform.RotateAround(dustRun.transform.position, Vector3.up, 180);
                    }
                }

                dustRun.Play();
            }
        }
    }
    public void PlayAirTrail()
    {
        if (!airTrail.emitting)
        {
            Debug.Log("X: " + Mathf.Abs(currentVelocity.x) + " Y: " + Mathf.Abs(currentVelocity.y));
            if (Mathf.Abs(currentVelocity.x) > 20f || Mathf.Abs(currentVelocity.y) > 50f)
            {
                airTrail.emitting = true;
            }
            else
            {
                airTrail.emitting = false;
            }
        }
    }

    int CalculateRotation(int currentDir, int desiredDir)
    {
        if (currentDir == desiredDir)
        {
            return 0;
        }
        if (currentDir == 1 && desiredDir == -1) // Turned right, want left
        {
            return 180; // Rotate 180 degrees
        }
        if (currentDir == -1 && desiredDir == 1) // Turned left, want right
        {
            return 180; // Rotate 180 degrees
        }

        // Rotate 90 degrees clockwise or counter-clockwise
        return (desiredDir - currentDir) * 90;
    }
}