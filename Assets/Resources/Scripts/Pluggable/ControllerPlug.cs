using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerPlug : MonoBehaviour
{
    // ��ɵ�
    private List<BasePluggable> plugs;
    // �켱���� �ʿ��� ��ɵ�
    private List<BasePluggable> overridePlugs;

    // ���� ��� �ڵ��
    private int currentPlugs;
    // �⺻ ��� �ڵ��
    private int defaultPlugs;
    // ��� ��� �ڵ��
    private int plugsLocked;

    // ĳ��
    public Transform playerCamera;
    private Animator playerAnimator;
    private Rigidbody playerRigidbody;
    private Transform playerTransform;
    private ObitCamera cameraScript;

    // horizontal axis
    private float h;
    // vertical axis
    private float v;

    // ī�޶� ���ϵ��� ������ �� ȸ�� �ϴ� �ӵ�
    public float lerpTurn = 0.05f;
    // �޸��� ����� ī�޶� �þ߰��� ���� �Ǿ��� �� ��ϵǾ���?
    private bool flagChangeFOV;
    // �޸��� �þ߰�
    public float runFOV = 100;
    // �������� ���ߴ� ����
    private Vector3 dirLast;
    // �޸��� ���ΰ� ? 
    private bool flagRun;
    // �ִϸ��̼� h�� ��
    private float hFloat;
    // �ִϸ��̼� v�� ��
    private float vFloat;
    // �� ���� �پ� �ִ°�?
    private int flagOnGround;
    // ������ �浹üũ�� ���� �浹ü ����
    private Vector3 colliderGround;

    public float getHorizontal { get => h; }
    public float getVertical { get => v; }
    public ObitCamera getCameraScript { get => cameraScript; }
    public Rigidbody getRigidbody { get => playerRigidbody; }
    public Animator getAnimator { get => playerAnimator; }
    // ���� � �÷��װ� ���� �ִ°�?
    public int getDefaultPlugs { get => defaultPlugs; }


    private void Awake()
    {
        playerTransform = transform;
        plugs = new List<BasePluggable>();
        overridePlugs = new List<BasePluggable>();
        playerAnimator = GetComponent<Animator>();
        cameraScript = playerCamera.GetComponent<ObitCamera>();
        playerRigidbody = GetComponent<Rigidbody>();
        colliderGround = GetComponent<Collider>().bounds.extents;

        // �ִϸ�����
        hFloat = Animator.StringToHash("H");
        vFloat = Animator.StringToHash("V");
        flagOnGround = Animator.StringToHash("Grounded");
    }

    // �÷��̾ �̵����ΰ�?
    // �츮�� �̵��ϴ� �߿� �÷������� ���̱� ���ؼ��̴�.
    public bool getFlagMoving()
    {
        //return (h != 0.0f) || (v != 0.0f);
        return (Mathf.Abs(h) > Mathf.Epsilon) || (Mathf.Abs(v) < Mathf.Epsilon);
    }

    // �������� ������ �̵��ϰ� �ִ°�?
    public bool getFlagHorizontalMoving()
    {
        return Mathf.Abs(h) > Mathf.Epsilon;
    }

    public bool getFlagRun()
    {
        foreach (BasePluggable basePluggable in plugs)
        {
            if (basePluggable.flagAllowRun == false)
            {
                return false;
            }
        }

        foreach (BasePluggable overridePluggable in overridePlugs)
        {
            if (!overridePluggable.flagAllowRun)
            {
                return false;
            }
        }

        return true;
    }

    // �޸��°� �����Ѱ�?
    public bool getFlagReadyRunning()
    {
        return flagRun && getFlagMoving() && getFlagRun();
    }

    // Ȥ�� �� ���� �ִ°�?
    public bool getFlagGrounded()
    {
        Ray ray = new Ray(playerTransform.position + Vector3.up * 2 * colliderGround.x, Vector3.down);
        return Physics.SphereCast(ray, colliderGround.x, colliderGround.x + 0.2f);
    }

    private void Update()
    {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        playerAnimator.SetFloat("H", h, 0.1f, Time.deltaTime);
        playerAnimator.SetFloat("V", v, 0.1f, Time.deltaTime);

        flagRun = Input.GetKey(KeyCode.Space);

        if (getFlagReadyRunning())
        {
            flagChangeFOV = true;
            cameraScript.setFOV(runFOV);
        }
        else if (flagChangeFOV)
        {
            cameraScript.resetFOV();
            flagChangeFOV = false;
        }
    }

    public void resetPosition()
    {
        if (dirLast != Vector3.zero)
        {
            dirLast.y = 0f;
            Quaternion targetRotation = Quaternion.LookRotation(dirLast);
            Quaternion newRotation = Quaternion.Slerp(playerRigidbody.rotation, targetRotation, lerpTurn);
            playerRigidbody.MoveRotation(newRotation);
        }
    }

    private void FixedUpdate()
    {
        bool flagAnyPlayActive = false;

        if (plugsLocked > 0 || overridePlugs.Count == 0)
        {
            foreach (BasePluggable basePluggable in plugs)
            {
                if (basePluggable.isActiveAndEnabled && currentPlugs == basePluggable.getPlugsCode)
                {
                    flagAnyPlayActive = true;
                    basePluggable.childFixedUpdate();
                }
            }
        }
        else
        {
            foreach (BasePluggable basePluggable in overridePlugs)
            {
                basePluggable.childFixedUpdate();
            }
        }

        if (!flagAnyPlayActive && overridePlugs.Count == 0)
        {
            playerRigidbody.useGravity = true;
            resetPosition();
        }
    }

    private void LateUpdate()
    {
        if (plugsLocked > 0 || overridePlugs.Count == 0)
        {
            foreach (BasePluggable basePluggable in plugs)
            {
                if (basePluggable.isActiveAndEnabled && currentPlugs == basePluggable.getPlugsCode)
                {
                    basePluggable.childLateUpdate();
                }
            }
        }
        else
        {
            foreach (BasePluggable basePluggable in overridePlugs)
            {
                basePluggable.childLateUpdate();
            }
        }
    }

    public void AddPlugs(BasePluggable basePluggable)
    {
        plugs.Add(basePluggable);
    }

    public void regDefaultPlugs(int plugCode)
    {
        defaultPlugs = plugCode;
        currentPlugs = plugCode;
    }

    public void regPlugs(int plugCode)
    {
        if (currentPlugs == defaultPlugs)
        {
            currentPlugs = plugCode;
        }
    }

    public void UnRegPlugs(int plugCode)
    {
        if (currentPlugs == plugCode)
        {
            currentPlugs = defaultPlugs;
        }
    }

    public bool OverrideWithPlugs(BasePluggable basePluggable)
    {
        if (!overridePlugs.Contains(basePluggable))
        {
            if (overridePlugs.Count == 0)
            {
                foreach (BasePluggable pluggable in plugs)
                {
                    if (pluggable.isActiveAndEnabled && currentPlugs == pluggable.getPlugsCode)
                    {
                        pluggable.OnOverride();
                        break;
                    }
                }
            }
            overridePlugs.Add(basePluggable);
            return true;
        }
        return false;
    }

    public bool UnOverridingPlugs(BasePluggable pluggable)
    {
        if (overridePlugs.Contains(pluggable))
        {
            overridePlugs.Remove(pluggable);
            return true;
        }
        return false;
    }

    public bool getOverriding(BasePluggable basePluggable = null)
    {
        if (basePluggable == null)
        {
            return overridePlugs.Count > 0;
        }
        return overridePlugs.Contains(basePluggable);
    }

    // ���� �÷����ΰ�?
    public bool getFlagCurrentPlugs(int plugCode)
    {
        return currentPlugs == plugCode;
    }

    public bool getLockStatus(int plugCode = 0)
    {
        return (plugsLocked != 0 && plugsLocked != plugCode);
    }

    public void LockPlugs(int plugCode)
    {
        if (plugsLocked == 0)
        {
            plugsLocked = plugCode;
        }
    }

    public void UnLockPlugs(int plugCode)
    {
        if(plugsLocked == plugCode)
        {
            plugsLocked = 0;
        }
    }

    public Vector3 getDirLast()
    {
        return dirLast;
    }

    public void setDirLast(Vector3 direction)
    {
        dirLast = direction;    
    }
}



// �߻�ȭ Ŭ������ ����� ������
// �ڽĵ��� �ϳ��� Ŭ������ ����ȯ�Ͽ� �ű⿡ �÷��� ������ �� �� �ִ�.
public abstract class BasePluggable : MonoBehaviour
{
    // �ӵ� ���� : �Ȱ� �޸��°� ����
    protected int spdFloat;
    protected ControllerPlug controllerPlug;

    // �����ڵ�
    protected int plugsCode;
    // �� �� �� �־�?
    protected bool getFlagRun;

    private void Awake()
    {
        // ĳ��
        this.controllerPlug = GetComponent<ControllerPlug>();
        getFlagRun = true;
        // �ؽ��ڵ�� ���� �ڵ常��� (Reflection Object GetType())
        plugsCode = this.GetType().GetHashCode();
    }

    // �÷��� ���� �ڵ� ��������
    public int getPlugsCode { get => plugsCode; }
    // �� �޷��� �ɱ�?
    public bool flagAllowRun { get => getFlagRun; }

    // ���� �ִ� �÷��׵� ������Ʈ ����
    public virtual void childLateUpdate() { }
    public virtual void childFixedUpdate() { }

    // �� ����� ���� �߿��ϴ� ���� �ؾ� ��
    public virtual void OnOverride() { }
}
