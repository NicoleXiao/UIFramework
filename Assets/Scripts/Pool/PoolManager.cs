using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIFramework
{
    public class PoolManager : MonoSingleton<PoolManager>
    {

        private bool isInitialized = false;

        /**对象池根节点，不同类型的对象池都放在这个下面**/
        private static string POOL_ROOT = "PoolRoot";

        /**对象池集合**/
        private Dictionary<string, object> mapPools = new Dictionary<string, object>();

        /**存放对象池物体的节点**/
        Dictionary<string, Transform> mapNodes = new Dictionary<string, Transform>();

        public Transform GetParent(AssetType obj)
        {
            return mapNodes[obj.ToString()];
        }

        protected override void Init()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (isInitialized) return;
            var root = GameObject.Find(POOL_ROOT);
            if (!root)
            {
                root = new GameObject(POOL_ROOT);
                root.AddComponent<DontDestroyOnload>();
            }
            var types = Enum.GetNames(typeof(AssetType));
            foreach (var type in types)
            {
                var trans = new GameObject(type.ToString()).transform;
                trans.SetParent(root.transform);
                mapNodes[type] = trans;
            }
            isInitialized = true;
        }

        /// <summary>
        /// 获取对象池
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetType"></param>
        /// <returns></returns>
        public ObjectPool<T> GetPool<T>(AssetType assetType) where T : UnityEngine.Object
        {
            var type = typeof(T);
            var key = Enum.GetName(typeof(AssetType), assetType);
            ObjectPool<T> pool = null;
            if (!mapPools.ContainsKey(key))
            {
                pool = new ObjectPool<T>();
                mapPools.Add(key, pool);
            }
            else
            {
                pool = mapPools[key] as ObjectPool<T>;
            }
            return pool;
        }

        /// <summary>
        /// 是否存在某个类型物体
        /// </summary>
        /// <returns></returns>
        public bool isExitObject<T>(string assetPath, string assetName, AssetType assetType) where T : UnityEngine.Object
        {
            return GetPool<T>(assetType).IsPoolExitObj(assetPath, assetName, assetType);
        }

        /// <summary>
        /// 获取物体
        /// </summary>
        /// <returns></returns>
        public T GetObject<T>(string assetPath, string assetName, AssetType assetType) where T : UnityEngine.Object
        {
            return GetPool<T>(assetType).GetFromPool(assetPath, assetName);
        }

        /// <summary>
        /// 放入对象池
        /// </summary>
        public void AddItemToPool<T>(string assetPath, string assetName, T obj, AssetType assetType, bool isUse) where T : UnityEngine.Object
        {
            ObjectPool<T> pool = GetPool<T>(assetType);
            pool.AddItemToPool(assetPath, assetName, obj, true, isUse);
        }

        /// <summary>
        /// 回收物体
        /// </summary>
        public void DeSpawn<T>(AssetType assetType, T obj) where T : UnityEngine.Object
        {

            if (assetType == AssetType.Prefab)
            {
                GameObject g = obj as GameObject;
                g.CustomSetActive(false);
                g.transform.SetParent(GameObject.Find(Enum.GetName(typeof(AssetType), assetType)).transform);
            }
            GetPool<T>(assetType).Recycle(obj);
        }
    }

}