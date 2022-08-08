using System.Collections;
using System.Collections.Generic;
using UnityObject = UnityEngine.Object;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static UnityObject Load(string path)
    {
        return Resources.Load(path);
    }

    public static GameObject LoadAndInstantiate(string path)
    {
        UnityObject obj = Load(path);
        if (obj == null)
        {
            return null;
        }
        //return (GameObject)GameObject.Instantiate(obj);
        return GameObject.Instantiate(obj) as GameObject;
    }
}
