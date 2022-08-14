using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class AimShootPlug : BasePluggable
{

    public Texture2D imgCrossHair;
    public float lerpAimTime = 0.15f;
    public Vector3 aimPivotOffset = new Vector3(0.5f, 1.2f, 0f);
    public Vector3 aimCamOffset = new Vector3(0.0f, .4f, -0.7f);

    private int flagAim;
    private bool flagAimming;
    private int cornerAimming;
    private bool aimCorner;

    // 애니메이션 ik
    private Vector3 initRootRotation;
    private Vector3 initHipRotation;
    private Vector3 initSpineRotation;
    private Transform playerTransform;

    private void Start()
    {
        playerTransform = transform;

        flagAim = Animator.StringToHash("Aim");
        cornerAimming = Animator.StringToHash("Corner");

        Transform hip = controllerPlug.getAnimator.GetBoneTransform(HumanBodyBones.Hips);
        initRootRotation = (hip.parent == transform) ? Vector3.zero : hip.parent.localEulerAngles;
        initHipRotation = hip.localEulerAngles;
        initSpineRotation = controllerPlug.getAnimator.GetBoneTransform(HumanBodyBones.Spine).localEulerAngles;
    }

    void Rotation()
    {
        Vector3 forward = controllerPlug.playerCamera.TransformDirection(Vector3.forward);
        forward.y = 0f;
        forward = forward.normalized;

        Quaternion targetRotation = Quaternion.Euler(0f, controllerPlug.getCameraScript.getHorizontal, 0.0f);

        float minSpd = Quaternion.Angle(playerTransform.rotation, targetRotation) * lerpAimTime;

        if (aimCorner)
        {
            playerTransform.rotation = Quaternion.LookRotation(-controllerPlug.getDirLast());
            targetRotation *= Quaternion.Euler(initRootRotation);
            targetRotation *= Quaternion.Euler(initHipRotation);
            targetRotation *= Quaternion.Euler(initSpineRotation);

            Transform spine = controllerPlug.getAnimator.GetBoneTransform(HumanBodyBones.Spine);
            spine.rotation = targetRotation;
        }
        else
        {
            controllerPlug.setDirLast(forward);
            playerTransform.rotation = Quaternion.Slerp(playerTransform.rotation, targetRotation, minSpd * Time.deltaTime);
        }
    }

    void aimManager()
    {
        Rotation();
    }

    private IEnumerator ToggleAimOn()
    {
        yield return new WaitForSeconds(0.05f);

        if (controllerPlug.getLockStatus(plugsCode) | controllerPlug.getOverriding(this))
        {
            yield return false;
        }
        else
        {
            flagAimming = true;

            int signal = 1;
            if (aimCorner)
            {
                signal = (int)Mathf.Sign(controllerPlug.getHorizontal);
            }


            aimCamOffset.x = Mathf.Abs(aimCamOffset.x) * signal;
            aimPivotOffset.x = Mathf.Abs(aimPivotOffset.x) * signal;

            yield return new WaitForSeconds(0.1f);
            controllerPlug.getAnimator.SetFloat(spdFloat, 0.0f);
            controllerPlug.OverrideWithPlugs(this);
        }
    }

    private IEnumerator ToggleAimOff()
    {
        flagAimming = false;
        yield return new WaitForSeconds(0.3f);
        controllerPlug.getCameraScript.resetAimOffset();
        controllerPlug.getCameraScript.resetMaxVAngle();
        yield return new WaitForSeconds(0.1f);

        controllerPlug.UnOverridingPlugs(this);
    }

    public override void childFixedUpdate()
    {
        if (flagAimming)
        {
            controllerPlug.getCameraScript.setPosTargetOffset(aimCamOffset, aimPivotOffset);
        }
    }

    public override void childLateUpdate()
    {
        aimManager();
    }

    private void Update()
    {
        aimCorner = controllerPlug.getAnimator.GetBool(cornerAimming);
        if (Input.GetAxisRaw("Fire2") != 0 && !flagAimming)
        {
            StartCoroutine(ToggleAimOn());
        }
        else if (flagAimming && Input.GetAxisRaw("Fire2") == 0)
        {
            StartCoroutine(ToggleAimOff());
        }

        getFlagRun = !flagAimming;

        if (flagAimming && Input.GetButtonDown("Fire3") && !aimCorner)
        {
            aimCamOffset.x = aimCamOffset.x * (-1);
            aimPivotOffset.x = aimPivotOffset.x * (-1);
        }
        controllerPlug.getAnimator.SetBool(flagAim, flagAimming);
    }

    private void OnGUI()
    {
        if (imgCrossHair != null)
        {
            float length = controllerPlug.getCameraScript.getCurrentPivotMagnitude(aimPivotOffset);
            if (length < 0.05f)
            {
                GUI.DrawTexture(new Rect(Screen.width * 0.5f - (imgCrossHair.width * 0.5f), Screen.height * 0.5f - (imgCrossHair.height * 0.5f), imgCrossHair.width, imgCrossHair.height), imgCrossHair);   
            }
        }
    }
}
