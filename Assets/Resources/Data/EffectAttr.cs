using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectAttrType
{
    None = -1, // �⺻�� ������ �� �Ǿ� �ִ�.
    Normal, // ����
    Attack,
    Damage,
    Etc
}

public class EffectAttr
{
    public int code = 0;
    public EffectAttrType effectAttrType = EffectAttrType.None;

    public GameObject effectObj = null;

    public string effectObjName = string.Empty;
    public string effectObjPath = string.Empty;
    public string effectObjFullPath = string.Empty;

    public EffectAttr() { }

    // ���� �ε� ���
    public void effectPreLoad()
    {
        this.effectObjFullPath = effectObjPath + effectObjName;
        if (this.effectObjFullPath != string.Empty && this.effectObj == null)
        {
            //this.effectObj = (GameObject)ResourceManager.Load(effectObjFullPath);
            this.effectObj = ResourceManager.Load(effectObjFullPath) as GameObject;
        }
    }

    // �����ε� �� ����Ʈ �����ִ� ���
    public void delectEffect()
    {
        if (this.effectObj != null)
        {
            this.effectObj = null;
        }
    }

    public GameObject Instantiate(Vector3 _pos)
    {
        if(this.effectObj == null)
        {
            this.effectPreLoad();
        }

        if (this.effectObj != null)
        {
            GameObject retEffectObj = GameObject.Instantiate(effectObj, _pos, Quaternion.identity);
            return retEffectObj;
        }

        return null;
    }
}
