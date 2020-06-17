using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressableResLoader : UIFramework.AddressableResLoader
{

}

namespace UIFramework
{
    public class AddressableResLoader : BaseResLoader
    {
        private static Dictionary<string, List<AsyncOperationHandle>> m_AsyncLoadingHandles = new Dictionary<string, List<AsyncOperationHandle>>();

        private static List<AsyncOperationHandle> m_Handles = new List<AsyncOperationHandle>();


        private static void AddHandle(string assetKey, AsyncOperationHandle handle)
        {
            assetKey = assetKey.ToLower();
            List<AsyncOperationHandle> operationHandles;
            if (!m_AsyncLoadingHandles.TryGetValue(assetKey, out operationHandles))
            {
                operationHandles = new List<AsyncOperationHandle>();
                m_AsyncLoadingHandles.Add(assetKey, operationHandles);
            }

            handle.Destroyed += (o) => { m_Handles.RemoveAll(s => s.Equals(o)); };
            m_Handles.Add(handle);

            operationHandles.Add(handle);
        }

        private static bool RemoveHandle(string assetKey, AsyncOperationHandle handle)
        {
            assetKey = assetKey.ToLower();
            List<AsyncOperationHandle> operationHandles;
            if (m_AsyncLoadingHandles.TryGetValue(assetKey, out operationHandles))
            {
                int index = operationHandles.IndexOf(handle);
                if (index >= 0)
                {
                    operationHandles.RemoveAt(index);
                    return true;
                }
            }

            return false;
        }

        public static AsyncOperationHandle<GameObject> InstantiateAsync(string assetKey, bool checkLoading = true, Action<string, GameObject> action = null)
        {
            assetKey = assetKey.ToLower();
            if (checkLoading && m_AsyncLoadingHandles.ContainsKey(assetKey) && m_AsyncLoadingHandles[assetKey].Count > 0)
            {
                var loading = m_AsyncLoadingHandles[assetKey][0].Convert<GameObject>();
                if (action != null)
                {
                    Action<AsyncOperationHandle<GameObject>> a = (h) => { if (action != null) action.Invoke(assetKey, h.Result); };
                    loading.Completed += a;
                }

                return loading;
            }
            else
            {
                var handle = Addressables.InstantiateAsync(assetKey);
                AddHandle(assetKey, handle);
                handle.Completed += (h) =>
                {
                    if (h.Result)
                    {
                        h.Result.transform.SetParent(PoolManager.GetInstance().GetParent(AssetType.Prefab));
                    }
                    RemoveHandle(assetKey, h);
                    try
                    {
                        if (action != null)
                        {
                            action.Invoke(assetKey, h.Result);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                };

                return handle;
            }
        }

        public static AsyncOperationHandle<T> LoadAssetAsync<T>(string assetKey, bool checkLoading = true, Action<string, bool, T> action = null)
        {
            assetKey = assetKey.ToLower();
            if (checkLoading && m_AsyncLoadingHandles.ContainsKey(assetKey) && m_AsyncLoadingHandles[assetKey].Count > 0)
            {
                var loading = m_AsyncLoadingHandles[assetKey][0].Convert<T>();
                if (action != null)
                {
                    Action<AsyncOperationHandle<T>> a = (h) => { if (action != null) action.Invoke(assetKey, h.Result != null && h.OperationException == null, h.Result); };
                    loading.Completed += a;
                }

                return loading;
            }
            else
            {
                var handle = Addressables.LoadAssetAsync<T>(assetKey);
                AddHandle(assetKey, handle);
                Action<AsyncOperationHandle<T>> a = (h) =>
                {
                    RemoveHandle(assetKey, h);
                    try
                    {
                        if (action != null)
                        {
                            action.Invoke(assetKey, h.Result != null && h.OperationException == null, h.Result);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                };
                handle.Completed += a;
                return handle;
            }
        }


        public static void ReleaseHandle(AsyncOperationHandle? handle)
        {
            try
            {
                if (handle.HasValue)
                {
                    bool isLoading = false;
                    string assetKey = string.Empty;
                    int indexOfHandle = -1;
                    foreach (var item in m_AsyncLoadingHandles)
                    {
                        indexOfHandle = item.Value.IndexOf(handle.Value);
                        if (indexOfHandle != -1)
                        {
                            assetKey = item.Key;
                            isLoading = true;
                            break;
                        }
                    }

                    if (isLoading)
                    {
                        m_AsyncLoadingHandles[assetKey].RemoveAt(indexOfHandle);
                        Debug.Log($"{nameof(AddressableResLoader)}.ReleaseHandle()  -->the handle that is loading was be released! {assetKey}");
                    }

                    if (handle.Equals(null))
                    {
                        Debug.LogException(new ArgumentException(nameof(AsyncOperationHandle)));
                    }
                    else
                    {
                        if (m_Handles.Contains(handle.Value))
                        {
                            Addressables.Release(handle.Value);
                        }
                        else
                        {
                            Debug.LogException(new Exception("the handle have been released!"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public static void Release(Object obj)
        {
            Addressables.Release(obj);
        }

    }
}