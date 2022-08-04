using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleMonobehaviour<T> : MonoBehaviour where T : Component
{
    private static T monoInstance;
    public static T Instance => monoInstance;

    public static T SingleMonoInstance()
    {
        if (monoInstance == null)
        {
            monoInstance = (T)FindObjectOfType(typeof(T));

            if (monoInstance == null)
            {
                GameObject _newGameObject = new GameObject(typeof(T).Name, typeof(T));
                monoInstance = _newGameObject.GetComponent<T>();
            }
        }
        return monoInstance;
    }

    protected virtual void Awake()
    {
        monoInstance = this as T;
        if(Application.isPlaying == true)
        {
            Object retObj = gameObject;
            if(transform.parent != null && transform.root != null)
            {
                retObj = transform.root.gameObject;
            }

            DontDestroyOnLoad(retObj);
        }
    }
}
