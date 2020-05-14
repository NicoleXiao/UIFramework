using System;
using UnityEngine;

public static  class ObjectExtension 
{
    public static void TryInvoke<T>(this Action<T> action, T param)
    {
        if (action == null) return;
        try
        {
            action(param);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    public static string GetAssetName(this string assetPath)
    {
        if (string.IsNullOrEmpty(assetPath))
        {
            UnityEngine.Debug.LogError("GetAssetName()-->the asset path is empty!");
            return string.Empty;
        }
        var index1 = assetPath.LastIndexOf('/');
        var index2 = assetPath.LastIndexOf('.');
        return assetPath.Substring(index1 + 1, index2 - index1 - 1);
    }

    public static GameObject InstantiateSafe(this GameObject target, Transform parent = null)
    {
        if (target == null) throw new ArgumentNullException("InstantiateSafe failured");
        GameObject go = UnityEngine.Object.Instantiate(target, parent);
        go.transform.localScale = Vector3.one;
        go.transform.localPosition = Vector3.zero;
        return go;
    }

    public static void CustomSetActive(this GameObject target, bool active)
    {
        if (target != null && target.activeSelf != active)
        {
            target.SetActive(active);
        }
    }

    public static T GetOrAddComponent<T>(this GameObject target) where T : Component
    {
        if (target)
        {
            T t = target.GetComponent<T>();
            if (t == null)
            {
                t = target.AddComponent<T>();
            }
            return t;
        }
        return null;
    }

    public static void SetLayerRecursively(this GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            child.gameObject.SetLayerRecursively(layer);
        }
    }

    public static void DestroySafe(this GameObject obj, float delay = 0f)
    {
        if (Application.isEditor)
        {
            UnityEngine.Object.DestroyImmediate(obj);
        }
        else
        {
            UnityEngine.Object.Destroy(obj, delay);
        }
    }

    public static float GetParticleSystemDuration(this GameObject obj)
    {
        if (!obj) return 0;
        var root = obj.GetComponent<ParticleSystem>();
        float max = root ? root.main.duration : 0;
        var childs = obj.GetComponentsInChildren<ParticleSystem>();
        foreach (var child in childs)
        {
            float time = child.main.duration + child.main.startDelayMultiplier;
            if (child.main.duration + child.main.startDelayMultiplier > max && (!child.main.loop))
            {
                max = time;
            }
        }
        return max;
    }

    /// <summary>
    /// 播放粒子特效
    /// </summary>
    /// <param name="obj"></param>
    /// <returns>最长时间,-1 looping</returns>
    public static void PlayEffect(this GameObject obj)
    {
        if (!obj) return;
        var particles = obj.GetComponentsInChildren<ParticleSystem>();
        foreach (var pt in particles)
        {
            pt.Play();
        }
    }

    public static T FindInParents<T>(this GameObject go) where T : Component
    {
        if (go == null) return null;
        var comp = go.GetComponent<T>();

        if (comp != null)
            return comp;

        var t = go.transform.parent;
        while (t != null && comp == null)
        {
            comp = t.gameObject.GetComponent<T>();
            t = t.parent;
        }
        return comp;
    }

}
