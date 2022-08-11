using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 이동 점프
/// 충돌
/// </summary>
public class MovePlug : BasePluggable
{
    // 걷기
    public float spdWalk = 0.15f;
    // 뛰기
    public float spdRun = 1.0f;
    // 부스터 달리기
    public float spdBooster = 2.0f;
    // 뎀프
    public float spdDampTime = 0.1f;

    // 현재 점프 중인가?   
    private bool flagJumpping;

    // 애니메이터 체크 점프 / 땅에 있는지
    private int ckGrounded;
    private int ckJump;

    // 점프 할 높이
    public float jumpHeight = 1.5f;
    // 점프 시 관성
    public float jumpFrontForce = 10f;
    // 마우스 휠로 속도 조절
    public float spdMouse, spdSeeker;

    // 충돌 체크
    private bool flagColliding;
    private CapsuleCollider capsuleCollider;

    // 캐릭터 캐싱
    private Transform playerTransform;

    private void Start()
    {
        playerTransform = transform;
        capsuleCollider = GetComponent<CapsuleCollider>();
        ckJump = Animator.StringToHash("Jump");
        ckGrounded = Animator.StringToHash("Grounded");
        controllerPlug.getAnimator.SetBool(ckGrounded, true);

        // 콘센트에 플러그를 등록
        controllerPlug.AddPlugs(this);
        // 콘센트에 기본 플러그를 등록
        controllerPlug.regDefaultPlugs(this.plugsCode);
        // 현재 스피드는 달리기 속도로 설정
        spdSeeker = spdRun;
    }

    Vector3 rotatingMove(float horizontal, float vertical)
    {
        Vector3 forward = controllerPlug.playerCamera.TransformDirection(Vector3.forward);
        forward.y = 0;
        forward = forward.normalized;

        Vector3 right = new Vector3(forward.z, 0f, -forward.x);

        Vector3 targetDirection = Vector3.zero;

        targetDirection = forward * vertical + right * horizontal;

        if (controllerPlug.getFlagMoving() && targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            Quaternion newRotation = Quaternion.Slerp(controllerPlug.getRigidbody.rotation, targetRotation, controllerPlug.lerpTurn);

            controllerPlug.getRigidbody.MoveRotation(newRotation);
            controllerPlug.setDirLast(targetDirection);
        }

        if (!(Mathf.Abs(horizontal) > 0.9f || Mathf.Abs(vertical) > 0.9f))
        {
            controllerPlug.resetPosition();
        }
        return targetDirection;
    }

    private void deleteVerticalVelocity()
    {
        Vector3 horizontalVelocity = controllerPlug.getRigidbody.velocity;
        horizontalVelocity.y = 0f;
        controllerPlug.getRigidbody.velocity = horizontalVelocity;
    }

    void moveManager(float horizontal, float vertical)
    {
        if (controllerPlug.getFlagGrounded())
        {
            controllerPlug.getRigidbody.useGravity = true;
        }
        else if (!controllerPlug.getAnimator.GetBool(ckJump) && controllerPlug.getRigidbody.velocity.y > 0)
        {
            deleteVerticalVelocity();
        }

        rotatingMove(horizontal, vertical);
        Vector2 dir = new Vector2(horizontal, vertical);
        spdMouse = Vector2.ClampMagnitude(dir, 1f).magnitude;
        spdSeeker += Input.GetAxis("Mouse ScrollWheel");
        spdSeeker = Mathf.Clamp(spdSeeker, spdWalk, spdRun);
        spdMouse *= spdSeeker;
        if (controllerPlug.getFlagReadyRunning())
        {
            spdMouse = spdBooster;
        }
        controllerPlug.getAnimator.SetFloat("Speed", spdMouse, spdDampTime, Time.deltaTime);
    }

    private void OnCollisionStay(Collision collision)
    {
        flagColliding = true;

        if (controllerPlug.getFlagCurrentPlugs(getPlugsCode) && collision.GetContact(0).normal.y <= 0.1f)
        {
            float vel = controllerPlug.getAnimator.velocity.magnitude;
            Vector3 targetMove = Vector3.ProjectOnPlane(playerTransform.forward, collision.GetContact(0).normal).normalized * vel;
            controllerPlug.getRigidbody.AddForce(targetMove, ForceMode.VelocityChange);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        flagColliding = false;
    }

    void jumpManager()
    {
        if (flagJumpping && !controllerPlug.getAnimator.GetBool(ckJump) && controllerPlug.getFlagGrounded())
        {
            controllerPlug.LockPlugs(plugsCode);
            controllerPlug.getAnimator.SetBool(ckJump, true);
            if (controllerPlug.getAnimator.GetFloat("Speed") > 0.1f)
            {
                capsuleCollider.material.dynamicFriction = 0f;
                capsuleCollider.material.staticFriction = 0f;

                deleteVerticalVelocity();

                float velocity = 2f * Mathf.Abs(Physics.gravity.y) * jumpHeight;
                velocity = Mathf.Sqrt(velocity);
                controllerPlug.getRigidbody.AddForce(Vector3.up * velocity, ForceMode.VelocityChange);
            }
        }
        else if (controllerPlug.getAnimator.GetBool(ckJump))
        {
            if (!controllerPlug.getFlagGrounded() && !flagColliding && controllerPlug.getLockStatus())
            {
                controllerPlug.getRigidbody.AddForce(playerTransform.forward * jumpFrontForce * Physics.gravity.magnitude * spdBooster, ForceMode.Acceleration);
            }

            if (controllerPlug.getRigidbody.velocity.y < 0f && controllerPlug.getFlagGrounded())
            {
                controllerPlug.getAnimator.SetBool(ckGrounded, true);

                capsuleCollider.material.dynamicFriction = 0.6f;
                capsuleCollider.material.staticFriction = 0.5f;

                flagJumpping = false;

                controllerPlug.getAnimator.SetBool(ckJump, false);

                controllerPlug.UnLockPlugs(this.plugsCode);
            }
        }
    }

    private void Update()
    {
        if (!flagJumpping && Input.GetButtonDown("Jump") && controllerPlug.getFlagCurrentPlugs(plugsCode)&& !controllerPlug.getOverriding())
        {
            flagJumpping = true;
        }
    }

    public override void childFixedUpdate()
    {
        moveManager(controllerPlug.getHorizontal, controllerPlug.getVertical);
        jumpManager();
    }
}
