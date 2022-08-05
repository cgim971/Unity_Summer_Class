using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataXMLManager : MonoBehaviour
{
    private static EffectXMLData effectXmlData = null;
    private static SoundXMLData soundXmlData = null;

    private void Start()
    {
        if (effectXmlData == null)
        {
            effectXmlData = ScriptableObject.CreateInstance<EffectXMLData>();
            effectXmlData.LoadData();
        }
    }

    public static EffectXMLData EffectData()
    {
        if (effectXmlData == null)
        {
            effectXmlData = ScriptableObject.CreateInstance<EffectXMLData>();
            effectXmlData.LoadData();
        }
        return effectXmlData;
    }

    public static SoundXMLData SoundData()
    {
        if (soundXmlData == null)
        {
            soundXmlData = ScriptableObject.CreateInstance<SoundXMLData>();
            soundXmlData.LoadData();
        }
        return soundXmlData;
    }
}
