using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectObjManager : SingleMonobehaviour<EffectObjManager>
{

    private Transform effectObj = null;

    private void Start()
    {
        if(effectObj == null)
        {
            effectObj = new GameObject("EffectObj").transform;
            effectObj.SetParent(transform);
        }
    }

    public GameObject EffectInstantate(int idx, Vector3 pos)
    {
        EffectAttr attr = DataXMLManager.EffectData().getAttr(idx);
        GameObject effectInstance = attr.Instantiate(pos);
        effectInstance.SetActive(true);
        return effectInstance;
    }
}
