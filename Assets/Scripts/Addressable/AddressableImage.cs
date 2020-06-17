using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace UIFramework
{
    /// <summary>
    /// 基于Addressable加载的Image组件
    /// </summary>
    public class AddressableImage : Image
    {
        public bool m_isNativeSize;
        private string m_loadPath;
        private AsyncOperationHandle<Sprite>? m_Handle;

        public void SetSprite(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            if (m_Handle != null && m_loadPath == path)
            {
                return;
            }
            if (m_Handle.HasValue)
            {
                m_Handle.Value.Completed -= Completed;
                Addressables.Release(m_Handle.Value);
                m_Handle = null;
            }
            m_Handle = Addressables.LoadAssetAsync<Sprite>(path);
            m_Handle.Value.Completed += Completed;
        }

        private void Completed(AsyncOperationHandle<Sprite> sp)
        {
            if (sp.Status == AsyncOperationStatus.Succeeded)
            {
                sprite = sp.Result;
                if (m_isNativeSize)
                {
                    SetNativeSize();
                }
                enabled = true;
            }
        }

        public void Release()
        {
            m_loadPath = string.Empty;
            if (m_Handle.HasValue)
            {
                m_Handle.Value.Completed -= Completed;
                Addressables.Release(m_Handle.Value);
                m_Handle = null;
            }
            sprite = null;
            enabled = false;
        }
    }
}
