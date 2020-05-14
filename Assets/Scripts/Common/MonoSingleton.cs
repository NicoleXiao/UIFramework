using System;
using UnityEngine;
using Object = UnityEngine.Object;


[AutoSingleton(true)]
public class MonoSingleton<T> : MonoBehaviour where T : Component
{
    private static T _instance;

    private static bool _destroyed;

    public static T instance
    {
        get
        {
            return GetInstance();
        }
    }

    protected bool m_IsApplicationQuit = false;

    public static T GetInstance()
    {
        if (_instance == null && !_destroyed)
        {
            Type typeFromHandle = typeof(T);
            _instance = (T)FindObjectOfType(typeFromHandle);
            if (_instance == null)
            {
                object[] customAttributes = typeFromHandle.GetCustomAttributes(typeof(AutoSingletonAttribute), true);
                if (customAttributes.Length > 0 && !((AutoSingletonAttribute)customAttributes[0]).bAutoCreate)
                {
                    return default(T);
                }
                GameObject val = new GameObject(typeof(T).Name);
                _instance = val.AddComponent<T>();
                GameObject val2 = GameObject.Find("BootObj");
                if (val2 != null)
                {
                    val.transform.SetParent(val2.transform);
                }
            }
        }
        return _instance;
    }

    public static void DestroyInstance()
    {
        
        if (_instance != null)
        {
            Object.Destroy(_instance.gameObject);
        }
        _destroyed = true;
        _instance = null;
        
        
    }

    public static void ClearDestroy()
    {
        DestroyInstance();
        _destroyed = false;
    }

    protected virtual void Awake()
    {
        if (_instance != null && _instance.gameObject != this.gameObject)
        {
            if (Application.isPlaying)
            {
                Destroy(this.gameObject);
            }
            else
            {
                DestroyImmediate(this.gameObject);
            }
        }
        else if (_instance == null)
        {
            _instance = this.GetComponent<T>();
        }
        if (transform.parent == null)
        {
            DontDestroyOnLoad(this.gameObject);
        }
        Init();
    }

    protected virtual void OnDestroy()
    {
        if (_instance != null && _instance.gameObject == this.gameObject)
        {
            _instance = null;
        }
    }

    public static bool HasInstance()
    {
        return _instance != null;
    }

    protected virtual void Init()
    {
    }

    private void OnApplicationQuit() {
        m_IsApplicationQuit = true;
    }
}
