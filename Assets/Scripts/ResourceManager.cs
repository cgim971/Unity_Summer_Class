using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static Object Load(string path)
    {
        return Resources.Load(path);
    }

    public static GameObject LoadAndInstantiate(string path)
    {
        Object obj = Load(path);
        if (obj == null)
        {
            return null;
        }
        return (GameObject)GameObject.Instantiate(obj);
    }
}
