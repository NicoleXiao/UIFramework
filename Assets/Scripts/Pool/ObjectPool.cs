using System.Collections.Generic;
using UnityEngine;

namespace UIFramework
{
    /// <summary>
    /// 资源类型
    /// </summary>
    public enum AssetType
    {
        Prefab = 1,
        Sprite = 2,
        AudioClip = 3,
    }

    public class ObjectPool<T> where T : UnityEngine.Object
    {
        public string name { get; set; }
        /** 每次自动清理几个游戏对象。 */
        public int cullMaxPerPass = 5;
        /** 每过多久执行一遍自动清理，单位是秒。从上一次清理过后开始计时 */
        public float cullDelay = 30f;
        /** 物体多久没有使用将会被清除 */
        public float cullIdleTime = 30f;
        /** 清理闲置对象协程是否启动 */
        private bool cullingActive = false;

        private float currentTime = 0f;
        /**这里应该是abName+assetName的HasCode 的值*/
        private Dictionary<int, List<PoolItem<T>>> m_spawned = new Dictionary<int, List<PoolItem<T>>>();

        private List<PoolItem<T>> m_poolItems = new List<PoolItem<T>>();

        /// <summary>
        /// 将物体加入对象池中
        /// </summary>
        /// <param name="abName"></param>
        /// <param name="assetName"></param>
        /// <param name="obj"></param>
        /// <param name="use">是否被使用</param>
        /// <param name="isLoadObj">是否为Addressable加载物体,有些放入对象池的物体是克隆的</param>
        /// <returns></returns>
        public T AddItemToPool(string abName, string assetName, T obj, bool isLoadObj, bool use)
        {
            PoolItem<T> item = new PoolItem<T>();
            item.obj = obj;
            item.abName = abName;
            item.poolState = use ? PoolState.Use : PoolState.Idle;
            item.useTime = Time.realtimeSinceStartup;
            item.isLoadObj = isLoadObj;
            int key = GetHashCode(abName, assetName);
            if (!m_spawned.ContainsKey(key))
            {
                m_spawned.Add(key, new List<PoolItem<T>>() { item });
            }
            m_poolItems.Add(item);
            SetParent(obj);
            return item.obj;
        }

        /// <summary>
        /// 获取HashCode
        /// </summary>
        /// <param name="abName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        private int GetHashCode(string abName, string assetName)
        {
            return (abName + assetName).GetHashCode();
        }

        /// <summary>
        /// 对象池里面没有可用的物体，就克隆一个出来
        /// </summary>
        /// <param name="abName"></param>
        /// <param name="assetName"></param>
        /// <param name="Obj"></param>
        private T CreateUseItem(string abName, string assetName, T obj)
        {
            T prefab = UnityEngine.Object.Instantiate(obj);
            return AddItemToPool(abName, assetName, prefab, false, true);
        }

        /// <summary>
        /// 是否存在指定类型物体
        /// </summary>
        /// <param name="abName"></param>
        /// <param name="assetName"></param>
        /// <param name="assetType"></param>
        /// <returns></returns>
        public bool IsPoolExitObj(string abName, string assetName, AssetType assetType)
        {
            int key = GetHashCode(abName, assetName);
            List<PoolItem<T>> list = new List<PoolItem<T>>();
            if (m_spawned.TryGetValue(key, out list))
            {
                if (list != null && list.Count > 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 从对象池中获取物体
        /// 1.先从列表中查找没有被使用的物体，如果有就直接取出来
        /// 2.如果没有可使用的物体，就克隆一个出来使用
        /// </summary>
        /// <param name="abName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public T GetFromPool(string abName, string assetName)
        {
            T obj = null;
            int key = GetHashCode(abName, assetName);
            List<PoolItem<T>> list = new List<PoolItem<T>>();
            if (m_spawned.TryGetValue(key, out list))
            {
                if (list != null && list.Count > 0)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        PoolItem<T> item = list[i];
                        if (item.poolState == PoolState.Idle)
                        {
                            item.poolState = PoolState.Use;
                            item.useTime = Time.realtimeSinceStartup;
                            obj = item.obj;
                            break;
                        }
                    }
                    if (obj == null)
                    {
                        obj = CreateUseItem(abName, assetName, list[0].obj);
                    }
                }
            }
            return obj;
        }

        /// <summary>
        /// 回收对象
        /// </summary>
        /// <param name="name"></param>
        /// <param name="obj"></param>
        public void Recycle(T obj)
        {
            for (int i = 0; i < m_poolItems.Count; i++)
            {
                if (m_poolItems[i].obj.Equals(obj))
                {
                    m_poolItems[i].poolState = PoolState.Idle;
                    break;
                }
            }
        }


        /// <summary>
        /// 对应类型物体放在对应类型节点下
        /// </summary>
        /// <param name="obj"></param>
        private void SetParent(T obj)
        {
            if (obj.GetType() == typeof(GameObject))
            {
                (obj as GameObject).transform.SetParent(PoolManager.instance.GetParent(AssetType.Prefab));
            }
        }


        #region 没有检测过的功能，暂时屏蔽掉
        /// <summary>
        /// 用于自动释放对象检测
        /// </summary>
        //public bool CanReleaseGameObject()
        //{
        //    currentTime += Time.deltaTime;
        //    if (currentTime > cullDelay && !cullingActive)
        //    {
        //        currentTime = 0;
        //        return true;
        //    }
        //    return false;
        //}

        /// <summary>
        /// 自动释放闲置的物体
        /// </summary>
        /// <returns></returns>
        //    public IEnumerator CullDespawned()
        //    {
        //        Debug.Log("开始清理");
        //        cullingActive = true;
        //        int releaseCount = 0;
        //        foreach (List<PoolItem<T>> items in m_spawned.Values)
        //        {
        //            for (int i = 0; i < items.Count; i++)
        //            {
        //                var poolItem = items[i];
        //                if (poolItem.poolState == PoolState.Idle)
        //                {
        //                    float seconds = Time.realtimeSinceStartup - poolItem.useTime;
        //                    if (seconds > cullIdleTime)
        //                    {
        //                        //如果是从内存加载出来的，要释放掉
        //                        if (poolItem.isLoadObj)
        //                        {

        //                        }
        //                        else  //是克隆的直接摧毁
        //                        {
        //#if UNITY_EDITOR
        //                            UnityEngine.Object.DestroyImmediate(poolItem.obj);
        //#else
        //                            UnityEngine.Object.Destroy(items[i].obj);
        //#endif
        //                        }
        //                        items.RemoveAt(i);
        //                        releaseCount++;
        //                    }
        //                    if (releaseCount >= cullMaxPerPass)
        //                    {
        //                        releaseCount = 0;
        //                        yield return new WaitForSeconds(cullDelay);
        //                    }
        //                }
        //            }
        //            cullingActive = false;
        //        }
        //    }
        #endregion
    }

    public enum PoolState
    {
        Use,
        Idle
    }

    public class PoolItem<T>
    {

        public T obj;
        /**是否为加载出来的对象*/
        public bool isLoadObj;
        /**物体的ab包名字*/
        public string abName;
        /**物体最近一次使用的时间*/
        public float useTime;
        /**物体状态(闲置/使用)*/
        public PoolState poolState { get; set; }

    }
}
