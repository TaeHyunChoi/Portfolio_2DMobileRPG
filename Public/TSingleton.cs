using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TSingleton<T> : MonoBehaviour where T : TSingleton<T>
{
    private static volatile T _uniqueInstace;
    private static volatile GameObject _uniqueObject;

    public static T _instance
    {
        get
        {
            if (_uniqueInstace == null)
            {
                lock (typeof(T))
                {
                    if (_uniqueInstace == null && _uniqueObject == null)
                    {
                        _uniqueObject = new GameObject(typeof(T).Name, typeof(T));
                        _uniqueInstace = _uniqueObject.GetComponent<T>();
                        _uniqueInstace.Init();
                    }
                }
            }
            return _uniqueInstace;
        }
    }
    public virtual void Init()
    {
        GameObject other = GameObject.Find(gameObject.name);
        if (other != this.gameObject)
        { 
            Destroy(other);
        }
        DontDestroyOnLoad(gameObject);
    }
}