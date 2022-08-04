using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataXMLManager : MonoBehaviour
{
    private static EffectXMLData xmlData = null;

    private void Start()
    {
        if (xmlData == null)
        {
            xmlData = ScriptableObject.CreateInstance<EffectXMLData>();
            xmlData.LoadData();
        }
    }

    public static EffectXMLData EffectData()
    {
        if (xmlData == null)
        {
            xmlData = ScriptableObject.CreateInstance<EffectXMLData>();
            xmlData.LoadData();
        }
        return xmlData;
    }
}
