using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PhysicalCC : MonoBehaviour
{
    private IEnumerator dampingCor;

    [Header("Ground Check")]
    public bool isGround;

    public float groundAngle;
    public Vector3 groundNormal { get; private set; }

    [Header("Movement")]
    public bool ProjectMoveOnGround;

    public Vector3 moveInput;
    private Vector3 moveVelocity;

    [Header("Slope and inertia")]
    public float slopeLimit = 45;

    public float inertiaDampingTime = 0.1f;
    public float slopeStartForce = 3f;
    public float slopeAcceleration = 3f;
    public Vector3 inertiaVelocity;

    [Header("interaction with the platform")]
    public bool platformAction;

    public Vector3 platformVelocity;

    [Header("Collision")]
    public bool applyCollision = true;

    public float pushForce = 55f;

    // PROPERTIES: ----------------------------------------------------------------------------

    public CharacterController CharacterController { get; private set; }

    // FIELDS: --------------------------------------------------------------------------------

    private const string Platform = "Platform";

    // GAME ENGINE METHODS: -------------------------------------------------------------------

    private void Start()
    {
        CharacterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        GroundCheck();

        moveVelocity = ProjectMoveOnGround ? Vector3.ProjectOnPlane(moveInput, groundNormal) : moveInput;

        if (isGround)
        {
            if (groundAngle < slopeLimit && inertiaVelocity != Vector3.zero)
                InertiaDamping();
        }

        GravityUpdate();

        Vector3 moveDirection = (moveVelocity + inertiaVelocity + platformVelocity);

        CharacterController.Move((moveDirection) * Time.deltaTime);
    }

    private void GravityUpdate()
    {
        if (isGround && groundAngle > slopeLimit)
        {
            inertiaVelocity +=
                Vector3.ProjectOnPlane(
                    groundNormal.normalized + (Vector3.down * (groundAngle / 30)).normalized *
                    Mathf.Pow(slopeStartForce, slopeAcceleration), groundNormal) * Time.deltaTime;
        }
        else if (!isGround) inertiaVelocity.y -= Mathf.Pow(3f, 3) * Time.deltaTime;
    }

    private void InertiaDamping()
    {
        Vector3 currentVelocity = Vector3.zero;

        //inertia braking when the force of movement is applied
        float resistanceAngle = Vector3.Angle(Vector3.ProjectOnPlane(inertiaVelocity, groundNormal),
            Vector3.ProjectOnPlane(moveVelocity, groundNormal));

        resistanceAngle = resistanceAngle == 0 ? 90 : resistanceAngle;

        inertiaVelocity = (inertiaVelocity + moveVelocity).magnitude <= 0.1f
            ? Vector3.zero
            : Vector3.SmoothDamp(inertiaVelocity, Vector3.zero, ref currentVelocity,
                inertiaDampingTime / (3 / (180 / resistanceAngle)));
    }

    private void GroundCheck()
    {
        if (Physics.SphereCast(transform.position, CharacterController.radius, Vector3.down, out RaycastHit hit,
                CharacterController.height / 2 - CharacterController.radius + 0.01f))
        {
            isGround = true;
            groundAngle = Vector3.Angle(Vector3.up, hit.normal);
            groundNormal = hit.normal;

            if (hit.transform.CompareTag(Platform))
            {
                platformVelocity = hit.collider.attachedRigidbody == null | !platformAction
                    ? Vector3.zero
                    : hit.collider.attachedRigidbody.velocity;
            }

            if (Physics.BoxCast(transform.position, new Vector3(CharacterController.radius / 2.5f, CharacterController.radius / 3f, CharacterController.radius / 2.5f),
                    Vector3.down, out RaycastHit helpHit, transform.rotation, CharacterController.height / 2 - CharacterController.radius / 2))
            {
                groundAngle = Vector3.Angle(Vector3.up, helpHit.normal);
            }
        }
        else
        {
            platformVelocity = Vector3.zero;
            isGround = false;
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!applyCollision) return;

        Rigidbody body = hit.collider.attachedRigidbody;

        // check rigidbody
        if (body == null || body.isKinematic) return;

        Vector3 pushDir = hit.point - (hit.point + hit.moveDirection.normalized);

        // Apply the push
        body.AddForce(pushDir * pushForce, ForceMode.Force);
    }
}