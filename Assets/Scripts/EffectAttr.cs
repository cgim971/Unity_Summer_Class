using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectAttrType
{
    None = -1,
    Normal,
}

public class EffectAttr : MonoBehaviour
{
    public int code = 0;
    public EffectAttrType effectAttrType = EffectAttrType.Normal;
    public GameObject effectObj = null;
    public string effectObjName = string.Empty;
    public string effectObjPath = string.Empty;
    public string effectObjFullPath = string.Empty;

    public EffectAttr() { }

    public void effectPreLoad()
    {
        this.effectObjFullPath = effectObjPath + effectObjName;
        if (this.effectObjFullPath != string.Empty && this.effectObj == null)
        {
            this.effectObj = (GameObject)ResourceManager.Load(effectObjFullPath);
        }
    }

    public void delectEffect()
    {
        if (this.effectObj != null)
        {
            this.effectObj = null;
        }
    }

    public GameObject Instantiate(Vector3 _pos)
    {
        if (this.effectObj != null)
        {
            GameObject retEffectObj = GameObject.Instantiate(effectObj, _pos, Quaternion.identity);
            return retEffectObj;
        }
        else
        {
            this.effectPreLoad();
        }

        return null;
    }
}
