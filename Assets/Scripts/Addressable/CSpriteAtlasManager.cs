using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;

namespace UIFramework
{
    public class CSpriteAtlasManager : Singleton<CSpriteAtlasManager>
    {
        private Dictionary<string, SpriteAtlas> m_spriteAtlasCaches = new Dictionary<string, SpriteAtlas>();

        private Dictionary<string, AsyncOperationHandle<SpriteAtlas>> m_spriteAtlasHandle = new Dictionary<string, AsyncOperationHandle<SpriteAtlas>>();

        private int m_requestedCount = 0;
        public void Initialize()
        {
            Debug.Log("CSpriteAtlasManager Initialize");
        }

        public override void Init()
        {
            SpriteAtlasManager.atlasRequested += AtlasRequested;
            SpriteAtlasManager.atlasRegistered += AtlasRegistered;
        }

        public override void UnInit()
        {
            SpriteAtlasManager.atlasRequested -= AtlasRequested;
            SpriteAtlasManager.atlasRegistered -= AtlasRegistered;
        }

        private void AtlasRequested(string tag, Action<SpriteAtlas> callback)
        {
            Debug.Log("Atlas Requested");
            if (m_spriteAtlasCaches.ContainsKey(tag))
            {
                callback?.Invoke(m_spriteAtlasCaches[tag]);
                return;
            }
            if (Application.isEditor)
            {
#if UNITY_EDITOR
                var guids = AssetDatabase.FindAssets("t:spriteatlas");
                foreach (var guid in guids)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    var fileName = Path.GetFileNameWithoutExtension(assetPath);
                    if (fileName == tag)
                    {
                        var spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(assetPath);
                        if (spriteAtlas != null)
                        {
                            callback?.Invoke(spriteAtlas);
                        }
                    }
                }
#endif
            }
            else
            {
                Debug.Log("Atlas Requested  ："  +  tag);
                var primaryKey = AddressableMgr.instance.GetSpriteAltasPrimaryKey(tag);
                if (!string.IsNullOrEmpty(primaryKey))
                {
                    Debug.Log("Atlas Requested 2");
                    var op = Addressables.LoadAssetAsync<SpriteAtlas>(primaryKey);
                    op.Completed += (s) => { callback?.Invoke(s.Result); };
                }
            }
        }

        private void AtlasRegistered(SpriteAtlas spriteAtlas)
        {
            Debug.Log("Atlas Registered");
            if (spriteAtlas)
            {
                var tag = spriteAtlas.tag;
                if (!m_spriteAtlasCaches.ContainsKey(tag))
                {
                    m_spriteAtlasCaches.Add(tag, spriteAtlas);
                }
            }
        }

        public IEnumerator LoadAllSpriteAtlas(List<string> spriteAltas)
        {
            Debug.Log("LoadAllSpriteAtlas");
            if (spriteAltas == null || spriteAltas.Count <= 0)
            {
                yield break;
            }

            foreach (var sa in spriteAltas)
            {
                var fileName = Path.GetFileNameWithoutExtension(sa);
                if (!m_spriteAtlasHandle.ContainsKey(fileName))
                {
                    var handle = Addressables.LoadAssetAsync<SpriteAtlas>(sa);
                    handle.Completed += OnSpriteAtlasLoaded;
                    m_spriteAtlasHandle.Add(sa, handle);
                    m_requestedCount++;
                }
            }

            yield return new WaitUntil(() =>
            {
                while (m_requestedCount > 0)
                {
                    return false;
                }
                return true;
            });
        }

        private void OnSpriteAtlasLoaded(AsyncOperationHandle<SpriteAtlas> spriteAtlas)
        {
            m_requestedCount--;

            string primaryKey = string.Empty;

            foreach (var handle in m_spriteAtlasHandle)
            {
                if (handle.Value.Equals(spriteAtlas))
                {
                    primaryKey = handle.Key;
                    break;
                }
            }

            if (spriteAtlas.Status == AsyncOperationStatus.Succeeded)
            {
                var tag = spriteAtlas.Result.tag;
                if (!m_spriteAtlasCaches.ContainsKey(tag))
                {
                    m_spriteAtlasCaches.Add(tag, spriteAtlas.Result);
                }
            }
        }
    }
}
