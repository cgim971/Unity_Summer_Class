using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 1. 카메라로부터 위치 오프셋 피봇 오프셋을 설정
// 2. 충돌체크 ; 이중체크
// 캐릭터로부터 카메라 사이
// 카메라로부터 캐릭터 사이
// 
// 3. Recoil
// 
// 4. FOV

[RequireComponent(typeof(Camera))]
public class ObitCamera : MonoBehaviour
{

    public Transform characterPlayer;

    public Vector3 pivotOffset = new Vector3(0f, 1f, 0f);
    public Vector3 camOffset = new Vector3(0.4f, 0.5f, -2.0f);

    // 마우스 이동 속도
    public float smooth = 10f;
    // 마우스 부위 별 이동 속도
    public float aimingMouseSpeedH = 6.0f;
    public float aimingMouseSpeedV = 6.0f;
    // 볼 수 있는 각도
    public float angleMaxV = 30.0f;
    public float angleMinV = -60.0f;

    // 총 반동
    public float angleBounceRecoil = 5.0f;

    private float angleHorizontal = 0.0f;
    private float angleVertical = 0.0f;

    private Transform transformCamera;

    private Camera fovCamera;

    // 플레이어로부터 카메라까지 벡터
    private Vector3 posRealCamera;

    // 카메라와 플레이어 사이 거리
    private float posDistanceRealCamera;

    private Vector3 lerpPivotOffset;
    private Vector3 lerpCamOffset;
    private Vector3 targetPivotOffset;
    private Vector3 targetCamOffset;

    private float lerpDefaultFOV;
    private float lerpTargetFOV;

    private float maxVerticalAngleTarget;
    private float angleRecoil = 0f;

    public float getHorizontal
    {
        get
        {
            return angleHorizontal;
        }
    }

    private void Awake()
    {
        transformCamera = transform;
        fovCamera = transformCamera.GetComponent<Camera>();

        transformCamera.position = characterPlayer.position + Quaternion.identity * pivotOffset + Quaternion.identity * camOffset;
        transformCamera.rotation = Quaternion.identity;

        posRealCamera = transformCamera.position - characterPlayer.position;
        posDistanceRealCamera = posRealCamera.magnitude - 0.5f;

        lerpPivotOffset = pivotOffset;
        lerpCamOffset = camOffset;

        lerpDefaultFOV = fovCamera.fieldOfView;
        angleHorizontal = characterPlayer.eulerAngles.y;

        // 리셋 3종
        // aim
        // fov
        // angle
        resetAimOffset();
        resetFOV();
        resetMaxVAngle();
    }

    public void resetAimOffset()
    {
        targetPivotOffset = pivotOffset;
        targetCamOffset = camOffset;
    }

    public void resetFOV()
    {
        this.lerpTargetFOV = lerpDefaultFOV;
    }

    public void resetMaxVAngle()
    {
        maxVerticalAngleTarget = angleMaxV;
    }

    public void recoilBounceAngleV(float val)
    {
        angleRecoil = val;
    }

    public void setPosTargetOffset(Vector3 newPivotOffset, Vector3 newCamOffset)
    {
        targetPivotOffset = newPivotOffset;
        targetCamOffset = newCamOffset;
    }

    public void setFOV(float _val)
    {
        this.lerpTargetFOV = _val;
    }

    bool ckViewingPos(Vector3 ckPos, float playerHeight)
    {
        Vector3 target = characterPlayer.position + (Vector3.up * playerHeight);

        if (Physics.SphereCast(ckPos, 0.2f, target - ckPos, out RaycastHit hit, posDistanceRealCamera))
        {
            if (hit.transform != characterPlayer && !hit.transform.GetComponent<Collider>().isTrigger)
            {
                return false;
            }
        }

        return true;
    }
    bool ckViewingPosR(Vector3 ckPos, float playerHeight, float maxDistance)
    {
        Vector3 origin = characterPlayer.position + (Vector3.up * playerHeight);

        if (Physics.SphereCast(ckPos, 0.2f, ckPos - origin, out RaycastHit hit, maxDistance))
        {
            if (hit.transform != characterPlayer && hit.transform != transform && !hit.transform.GetComponent<Collider>().isTrigger)
            {
                return false;
            }
        }

        return true;
    }

    bool ckDoubleViewingPos(Vector3 ckPos, float offset)
    {
        float playerFocusHeight = characterPlayer.GetComponent<CapsuleCollider>().height * 0.75f;
        return ckViewingPos(ckPos, playerFocusHeight) && ckViewingPosR(ckPos, playerFocusHeight, offset);
    }

    private void Update()
    {
        angleHorizontal += Mathf.Clamp(Input.GetAxis("Mouse X"), -1f, 1f) * aimingMouseSpeedH;
        angleVertical += Mathf.Clamp(Input.GetAxis("Mouse Y"), -1f, 1f) * aimingMouseSpeedV;

        angleVertical = Mathf.Clamp(angleVertical, angleMinV, maxVerticalAngleTarget);

        angleVertical = Mathf.LerpAngle(angleVertical, angleVertical + angleRecoil, 10 * Time.deltaTime);

        Quaternion camRotationY = Quaternion.Euler(0f, angleHorizontal, 0f);
        Quaternion aimRotation = Quaternion.Euler(-angleVertical, angleHorizontal, 0f);
        transformCamera.rotation = aimRotation;

        fovCamera.fieldOfView = Mathf.Lerp(fovCamera.fieldOfView, lerpTargetFOV, Time.deltaTime);
        Vector3 posBaseTemp = characterPlayer.position + camRotationY * targetPivotOffset;
        Vector3 noCollisionOffset = targetCamOffset;
    }



}
