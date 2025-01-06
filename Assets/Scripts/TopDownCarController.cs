using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopDownCarController : MonoBehaviour
{
    [Header("Car Base Stats")]
    public const float BASE_MAX_SPEED = 89.8f;
    public const float BASE_ACCELERATION = 30.0f;
    public const float BASE_TURN = 3.5f;
    public const float BASE_DRIFT = 0.95f;

    [Header("Car Current Stats")]
    public float driftFactor = 0.95f;
    public float accelerationFactor = 30.0f;
    public float turnFactor = 3.5f;
    public float maxSpeed = 89.8f;

    [Header("Sprites")]
    public SpriteRenderer carSpriteRenderer;
    public SpriteRenderer carShadowRenderer;

    [Header("Jumping")]
    public AnimationCurve jumpCurve;
    public ParticleSystem landingParticleSystem;

    //Local variables
    float accelerationInput = 0;
    float steeringInput = 0;

    float rotationAngle = 0;

    float velocityVsUp = 0;

    bool isJumping = false;

    //Components
    Rigidbody2D carRigidbody2D;
    Collider2D carCollider;
    CarSFXHandler carSfxHandler;
    CarSkinLoader carSkinLoader;

    //Awake is called when the script instance is being loaded.
    void Awake()
    {
        carRigidbody2D = GetComponent<Rigidbody2D>();
        carCollider = GetComponentInChildren<Collider2D>();
        carSfxHandler = GetComponent<CarSFXHandler>();

        // Buscar el CarSkinLoader existente o crear uno nuevo
        carSkinLoader = GetComponent<CarSkinLoader>();
        if (carSkinLoader == null)
        {
            carSkinLoader = gameObject.AddComponent<CarSkinLoader>();
        }
        
        // Asignar las referencias
        carSkinLoader.carSpriteRenderer = carSpriteRenderer;
        carSkinLoader.carShadowRenderer = carShadowRenderer;
    }

    void Start()
    {
        rotationAngle = transform.rotation.eulerAngles.z;
    }

    //Frame-rate independent for physics calculations.
    void FixedUpdate()
    {
        ApplyEngineForce();

        KillOrthogonalVelocity();

        ApplySteering();
    }

    void ApplyEngineForce()
    {
        //Don't let the player brake while in the air, but we still allow some drag so it can be slowed slightly. 
        if (isJumping && accelerationInput < 0)
            accelerationInput = 0;

        //Apply drag if there is no accelerationInput so the car stops when the player lets go of the accelerator
        if (accelerationInput == 0)
            carRigidbody2D.linearDamping = Mathf.Lerp(carRigidbody2D.linearDamping, 3.0f, Time.fixedDeltaTime * 3);
        else carRigidbody2D.linearDamping = 0;

        //Caculate how much "forward" we are going in terms of the direction of our velocity
        velocityVsUp = Vector2.Dot(transform.up, carRigidbody2D.linearVelocity);

        //Limit so we cannot go faster than the max speed in the "forward" direction
        if (velocityVsUp > maxSpeed && accelerationInput > 0)
            return;

        //Limit so we cannot go faster than the 50% of max speed in the "reverse" direction
        if (velocityVsUp < -maxSpeed * 0.5f && accelerationInput < 0)
            return;

        //Limit so we cannot go faster in any direction while accelerating
        if (carRigidbody2D.linearVelocity.sqrMagnitude > maxSpeed * maxSpeed && accelerationInput > 0 && !isJumping)
            return;

        //Create a force for the engine
        Vector2 engineForceVector = transform.up * accelerationInput * accelerationFactor;

        //Apply force and pushes the car forward
        carRigidbody2D.AddForce(engineForceVector, ForceMode2D.Force);
    }

    void ApplySteering()
    {
        //Limit the cars ability to turn when moving slowly
        float minSpeedBeforeAllowTurningFactor = (carRigidbody2D.linearVelocity.magnitude / 2);
        minSpeedBeforeAllowTurningFactor = Mathf.Clamp01(minSpeedBeforeAllowTurningFactor);

        //Update the rotation angle based on input
        rotationAngle -= steeringInput * turnFactor * minSpeedBeforeAllowTurningFactor;

        //Apply steering by rotating the car object
        carRigidbody2D.MoveRotation(rotationAngle);
    }

    void KillOrthogonalVelocity()
    {
        //Get forward and right velocity of the car
        Vector2 forwardVelocity = transform.up * Vector2.Dot(carRigidbody2D.linearVelocity, transform.up);
        Vector2 rightVelocity = transform.right * Vector2.Dot(carRigidbody2D.linearVelocity, transform.right);

        //Kill the orthogonal velocity (side velocity) based on how much the car should drift. 
        carRigidbody2D.linearVelocity = forwardVelocity + rightVelocity * driftFactor;
    }

    float GetLateralVelocity()
    {
        //Returns how how fast the car is moving sideways. 
        return Vector2.Dot(transform.right, carRigidbody2D.linearVelocity);
    }

    public bool IsTireScreeching(out float lateralVelocity, out bool isBraking)
    {
        lateralVelocity = GetLateralVelocity();
        isBraking = false;

        if (isJumping)
            return false;

        //Check if we are moving forward and if the player is hitting the brakes. In that case the tires should screech.
        if (accelerationInput < 0 && velocityVsUp > 0)
        {
            isBraking = true;
            return true;
        }

        //If we have a lot of side movement then the tires should be screeching
        if (Mathf.Abs(GetLateralVelocity()) > 4.0f)
            return true;

        return false;
    }

    public void SetInputVector(Vector2 inputVector)
    {
        steeringInput = inputVector.x;
        accelerationInput = inputVector.y;
    }

    public float GetVelocityMagnitude()
    {
        return carRigidbody2D.linearVelocity.magnitude;
    }

    public void Jump(float jumpHeightScale, float jumpPushScale, int carColliderLayerBeforeJump)
    {
        if (!isJumping)
            StartCoroutine(JumpCo(jumpHeightScale, jumpPushScale, carColliderLayerBeforeJump));
    }

    private IEnumerator JumpCo(float jumpHeightScale, float jumpPushScale, int carColliderLayerBeforeJump)
    {
        isJumping = true;

        float jumpStartTime = Time.time;
        float jumpDuration = carRigidbody2D.linearVelocity.magnitude * 0.05f;

        jumpHeightScale = jumpHeightScale * carRigidbody2D.linearVelocity.magnitude * 0.05f;
        jumpHeightScale = Mathf.Clamp(jumpHeightScale, 0.0f, 1.0f);

        //Change the layer of the car, as we have jumped we are now flying
        carCollider.gameObject.layer = LayerMask.NameToLayer("ObjectFlying");

        carSfxHandler.PlayJumpSfx();

        //Change sorting layer to flying
        carSpriteRenderer.sortingLayerName = "Flying";
        carShadowRenderer.sortingLayerName = "Flying";

        //Push the object forward as we passed a jump
        carRigidbody2D.AddForce(carRigidbody2D.linearVelocity.normalized * jumpPushScale * 10, ForceMode2D.Impulse);

        while (isJumping)
        {
            //Percentage 0 - 1.0 of where we are in the jumping process
            float jumpCompletedPercentage = (Time.time - jumpStartTime) / jumpDuration;
            jumpCompletedPercentage = Mathf.Clamp01(jumpCompletedPercentage);

            //Take the base scale of 1 and add how much we should increase the scale with. 
            carSpriteRenderer.transform.localScale = Vector3.one + Vector3.one * jumpCurve.Evaluate(jumpCompletedPercentage) * jumpHeightScale;

            //Change the shadow scale also but make it a bit smaller. In the real world this should be the opposite, the higher an object gets the larger its shadow gets but this looks better in my opinion
            carShadowRenderer.transform.localScale = carSpriteRenderer.transform.localScale * 0.75f;

            //Offset the shadow a bit. This is not 100% correct either but works good enough in our game. 
            carShadowRenderer.transform.localPosition = new Vector3(1, -1, 0.0f) * 3 * jumpCurve.Evaluate(jumpCompletedPercentage) * jumpHeightScale;

            //When we reach 100% we are done
            if (jumpCompletedPercentage == 1.0f)
                break;

            yield return null;
        }

        //Disable the car collider so we can perform an overlapped check 
        carCollider.enabled = false;

        //Do not check for collisions with triggers
        ContactFilter2D contactFilter2D = new ContactFilter2D();
        contactFilter2D.useTriggers = false;

        Collider2D[] hitResults = new Collider2D[2];

        int numberOfHitObjects = Physics2D.OverlapCircle(transform.position, 1.5f, contactFilter2D, hitResults);

        //Enable the car collider again so we can detect things with the trigger. 
        carCollider.enabled = true;

        //Check if landing is ok or not, if we hit zero objects then it is ok
        if (numberOfHitObjects != 0)
        {
            //Something is below the car so we need to jump again
            isJumping = false;

            //add a small jump and push the car forward a bit. 
            Jump(0.2f, 0.6f, carColliderLayerBeforeJump);
        }
        else
        {
            //Handle landing, scale back the object
            carSpriteRenderer.transform.localScale = Vector3.one;

            //reset the shadows position and scale
            carShadowRenderer.transform.localPosition = Vector3.zero;
            carShadowRenderer.transform.localScale = carSpriteRenderer.transform.localScale;

            //We are safe to land, so enable change the collision layer back to what it was before we jumped
            carCollider.gameObject.layer = carColliderLayerBeforeJump;


            //Change sorting layer to regular layer
            carSpriteRenderer.sortingLayerName = "Default";
            carShadowRenderer.sortingLayerName = "Default";

            //Play the landing particle system if it is a bigger jump
            if (jumpHeightScale > 0.2f)
            {
                landingParticleSystem.Play();

                carSfxHandler.PlayLandingSfx();
            }

            //Change state
            isJumping = false;
        }
    }

    public bool IsJumping()
    {
        return isJumping;
    }

    //Detect Jump trigger
    void OnTriggerEnter2D(Collider2D collider2d)
    {
        if (collider2d.CompareTag("Jump"))
        {
            //Get the jump data from the jump
            JumpData jumpData = collider2d.GetComponent<JumpData>();
            Jump(jumpData.jumpHeightScale, jumpData.jumpPushScale, carCollider.gameObject.layer);
        }
    }

    public void UpdateStatsWithPercentages(float speedPercentage, float accelerationPercentage, float handlingPercentage)
    {
        // Asegurar que los porcentajes estén entre 0 y 100
        speedPercentage = Mathf.Clamp(speedPercentage, 0f, 100f);
        accelerationPercentage = Mathf.Clamp(accelerationPercentage, 0f, 100f);
        handlingPercentage = Mathf.Clamp(handlingPercentage, 0f, 100f);

        // Convertir porcentajes a multiplicadores (0-1)
        float speedMultiplier = speedPercentage / 100f;
        float accelerationMultiplier = accelerationPercentage / 100f;
        float handlingMultiplier = handlingPercentage / 100f;

        // Aplicar los multiplicadores a las estadísticas base
        maxSpeed = BASE_MAX_SPEED * speedMultiplier;
        accelerationFactor = BASE_ACCELERATION * accelerationMultiplier;
        turnFactor = BASE_TURN * handlingMultiplier;
        driftFactor = BASE_DRIFT * handlingMultiplier;

        Debug.Log($"Stats Updated - Speed: {speedPercentage}%, Acceleration: {accelerationPercentage}%, Handling: {handlingPercentage}%");
    }
}