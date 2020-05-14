using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.AddressableAssets;

namespace UIFramework
{
    //定义ui的层级
    public enum UILayer
    {
        None = 0,
        CommonLayer = 1,
        TipsLayer = 2,
        SystemPopupLayer = 3,
    }

    //ui的加载信息
    public class LoadUIData : IEquatable<LoadUIData>
    {
        public UIBase uiBase;
        public UIInfo info;
        public UILayer layer;
        public int sortOrder;
        public bool isLoading;

        public override bool Equals(object obj)
        {
            return obj is LoadUIData data &&
                   info.Equals(data.info);
        }

        public bool Equals(LoadUIData other)
        {
            return other != null &&
              info.Equals(other.info) &&
              EqualityComparer<UIBase>.Default.Equals(uiBase, other.uiBase) &&
              sortOrder == other.sortOrder &&
              isLoading == other.isLoading;
        }

        public override int GetHashCode()
        {
            var hashCode = -183342971;
            hashCode = hashCode * -1521134295 + EqualityComparer<UIInfo>.Default.GetHashCode(info);
            hashCode = hashCode * -1521134295 + EqualityComparer<UIBase>.Default.GetHashCode(uiBase);
            hashCode = hashCode * -1521134295 + sortOrder.GetHashCode();
            hashCode = hashCode * -1521134295 + isLoading.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(LoadUIData left, LoadUIData right)
        {
            return EqualityComparer<LoadUIData>.Default.Equals(left, right);
        }

        public static bool operator !=(LoadUIData left, LoadUIData right)
        {
            return !(left == right);
        }
    }

    public class UIManager : MonoSingleton<UIManager>
    {
        /**父节点*/
        private Transform m_root;

        /**全局唯一的mask*/
        private MaskManager m_mask = null;

        private UICommonLoad m_commonLoad;
        private UIPopupLoad m_popupLoad;

        /**全局栈 用于存放所有加载出来的ui信息*/
        private List<LoadUIData> m_AllUIStack = new List<LoadUIData>();

        protected override void Init()
        {
            base.Init();
            if (m_root == null)
            {
                m_root = GameObject.Find("UIRoot").transform;
            }
            m_commonLoad = InitPopupLoad<UICommonLoad>("CommonUILoad") ;
            m_popupLoad = InitPopupLoad<UIPopupLoad>("PopupUILoad");
        }

        private T InitPopupLoad<T>(string name) where T : UILoadBase
        {
            T load = new GameObject(name).AddComponent<T>();
            load.transform.SetParent(this.transform);
            return load;
        }

        /// <summary>
        /// 游戏里面通用的消息提示弹窗
        /// </summary>
        public void ShowMessage(string message, string messageTitle = "")
        {
            List<string> messageData = new List<string>();
            if (!string.IsNullOrEmpty(message))
            {
                messageData.Add(message);
            }
            if (!string.IsNullOrEmpty(messageTitle))
            {
                messageData.Add(messageTitle);
            }
            ShowUI(UIPath.MessageUI, null, messageData.ToArray());
        }

        public void ShowUI(UIInfo info, Action<UIBase> showEvent = null, params object[] userData)
        {
            if (!CanStartLoadUI(ref info, showEvent, userData))
            {
                return;
            }
            StartCoroutine(CoLoadUI(info,showEvent,userData));
        }

        private IEnumerator CoLoadUI(UIInfo info, Action<UIBase> showEvent = null, params object[] userDatas)
        {
            LoadUIData data = PrePushToAllStack(info, userDatas);
            //先从对象池里面加载物体，没有找到对应的物体就加载一个，加载出来要放进对象池中保存起来
            GameObject obj = PoolManager.instance.GetObject<GameObject>(info.loadPath, info.loadPath.GetAssetName(), AssetType.Prefab);
            if (obj == null)
            {
                //动态加载
                var handle = Addressables.InstantiateAsync(info.loadPath);
                yield return handle;
                if (handle.Result != null)
                {
                    obj = handle.Result;
                    //加载完了放入对象池
                    PoolManager.instance.AddItemToPool(info.loadPath, info.loadPath.GetAssetName(), obj, AssetType.Prefab, true);
                }
            }
            data.isLoading = false;

            if (obj != null)
            {
                obj.transform.SetParent(m_root);
                UIBase ui = obj.GetOrAddComponent<UIBase>();
                if (!ui.IsInit)
                {
                    ui.InitUI(info);
                }
                ui.CloseSelfEvent += OnClickCloseEvent;
                data.uiBase = ui;
                PushUIDataToAllStack(data);
                PushUIToSelfStack(data);
                if (data.info.maskType != UIMaskType.None)
                {
                    ManagerMask();
                }

                IEnumerator iterator = ui.ShowUI(info.userDatas);
                info.iterator = iterator;
                yield return iterator;

                showEvent?.TryInvoke(ui);

                LoadUIComplete(data);

                IEnumerator iterator2 = ui.LoadCompleteUI(info.userDatas);
                info.iterator = iterator2;
                yield return iterator2;
                info.iterator = null;
            }
            else
            {
                Debug.LogError("Load " + data.info.loadPath + " fail!!! ");
            }
        }

        /// <summary>
        /// 是否可以加载UI
        /// </summary>
        /// <param name="uiInfo"></param>
        /// <param name="showEvent"></param>
        /// <param name="userData"></param>
        /// <returns></returns>
        private bool CanStartLoadUI(ref UIInfo uiInfo, Action<UIBase> showEvent = null, params object[] userDatas)
        {
            for (int i = 0; i < m_AllUIStack.Count; i++)
            {
                if (m_AllUIStack[i].info.Equals(uiInfo))
                {
                    if (m_AllUIStack[i].isLoading)
                    {
                        Debug.Log(uiInfo.loadPath + " is Loading！！！！");
                        uiInfo.userDatas = userDatas;
                        return false;
                    }
                    else if (m_AllUIStack[i].uiBase != null)
                    {
                        //这种情况可能就是关闭的时候没有关掉，或者一直都是打开状态没有关闭过，直接手动关闭掉然后再加载一次。
                        CloseCurUI(uiInfo);
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 先把加载信息放入全局的堆栈里面
        /// </summary>
        private LoadUIData PrePushToAllStack(UIInfo info, params object[] userDatas)
        {
            LoadUIData data = new LoadUIData();
            data.info = info;
            data.isLoading = true;
            data.info.isClosed = false;
            data.layer = GetLayer(data.info.uiType);
            info.userDatas = userDatas;
            m_AllUIStack.Add(data);
            return data;
        }


        /// <summary>
        /// 在加载之前Push过加载数据，数据只包含加载路径和加载状态
        /// 加载成功之后，找到对应的加载数据替换成全新的加载数据。
        /// </summary>
        /// <param name="data"></param>
        private void PushUIDataToAllStack(LoadUIData data)
        {
            for (int i = m_AllUIStack.Count - 1; i >= 0; i--)
            {
                if (m_AllUIStack[i].info.Equals(data.info))
                {
                    m_AllUIStack[i] = data;
                }
            }
        }

        private void PushUIToSelfStack(LoadUIData data)
        {
            var type = data.info.uiType;
            if (type == UIType.ComFull || type == UIType.ComPopup)
            {
                m_commonLoad.PushUIToStack(data);
            }
            else
            {
                m_popupLoad.PushUIToStack(data);
            }
        }

        private void LoadUIComplete(LoadUIData data)
        {
            var type = data.info.uiType;
            if (type == UIType.ComFull || type == UIType.ComPopup)
            {
                m_commonLoad.LoadComplete(data);
            }
            else
            {
                m_popupLoad.LoadComplete(data);
            }
        }


        public UILayer GetLayer(UIType type)
        {
            switch (type)
            {
                case UIType.ComFull:
                case UIType.ComPopup:
                    return UILayer.CommonLayer;
                case UIType.Tips:
                    return UILayer.TipsLayer;
                case UIType.SystemPopup:
                    return UILayer.SystemPopupLayer;
            }
            Debug.Log($"没有找到对应类型 ：{type} 的Layer");
            return UILayer.None;
        }

        #region Mask

        private void ManagerMask()
        {
            Debug.Log("ManagerMask");
            LoadUIData maskData = null;
            for (int i = 0; i < m_AllUIStack.Count; i++)
            {
                LoadUIData data = m_AllUIStack[i];
                if (data.uiBase != null && !data.isLoading && data.info.maskType != UIMaskType.None)
                {
                    if (maskData == null)
                    {
                        maskData = data;
                    }
                    else
                    {
                        if (maskData.layer <= data.layer)
                        {
                            maskData = data;
                        }
                    }
                }
            }
            if (maskData != null)
            {
                StartCoroutine(ManagerMaskShow(maskData));
            }
            else
            {
                HideMask();
            }
        }

        IEnumerator ManagerMaskShow(LoadUIData loadInfo)
        {
            if (!m_mask)
            {
                yield return LoadMask();
                if (!m_mask) yield break;
            }
            
            m_mask.CloseSelfEvent -= MaskClickEvent;
            switch (loadInfo.info.maskType)
            {
                case UIMaskType.OnlyMask:
                    m_mask.clickCloseUI = UIPath.None;
                    m_mask.SetAlpha(); ;
                    break;
                case UIMaskType.MaskClickClose:
                    m_mask.clickCloseUI = loadInfo.info;
                    m_mask.CloseSelfEvent += MaskClickEvent;
                    m_mask.SetAlpha() ;
                    break;
                case UIMaskType.TransparentMask:
                    m_mask.clickCloseUI = UIPath.None;
                    m_mask.SetTransparent();
                    break;
                case UIMaskType.TransparentClickMask:
                    m_mask.clickCloseUI = loadInfo.info;
                    m_mask.CloseSelfEvent += MaskClickEvent;
                    m_mask.SetTransparent();
                    break;
            }
            m_mask.UICanvas.sortingOrder = loadInfo.uiBase.UICanvas.sortingOrder - 1;
            m_mask.ResumeUI();
        }


        /// <summary>
        /// Mask是特殊的加载,不会放在UIStack里面,也不会放在对象池里面,只有显示和隐藏
        /// </summary>
        /// <param name="sortingOrder"></param>
        /// <param name="isClick"></param>
        private IEnumerator LoadMask()
        {
            var handle = Addressables.InstantiateAsync(UIPath.MaskUI.loadPath);
            yield return handle;
            if (handle.Result != null)
            {
                GameObject obj = handle.Result;
                obj.transform.SetParent(m_root);
                m_mask = obj.GetOrAddComponent<MaskManager>();
                m_mask.InitUI(UIPath.MaskUI);
                yield return m_mask.ShowUI();
            }
            else
            {
                Debug.LogError("load file failur:" + UIPath.MaskUI.loadPath);
            }
        }

        private void HideMask()
        {
            if (m_mask != null)
            {
                m_mask.CloseSelfEvent -= MaskClickEvent;
                m_mask.HideUI();
            }
        }

        private void MaskClickEvent(UIInfo info)
        {
            CloseCurUI(m_mask.clickCloseUI);
        }

        #endregion

        private LoadUIData GetLoadInfo(UIInfo uiInfo)
        {
            for (int i = 0; i < m_AllUIStack.Count; i++)
            {
                if (m_AllUIStack[i].info.Equals(uiInfo))
                {
                    return m_AllUIStack[i];
                }
            }
            return null;
        }



        #region UI的关闭事件

        /// <summary>
        /// ui点击关闭按钮需要处理的
        /// </summary>
        private void OnClickCloseEvent(UIInfo info)
        {
            RemoveCurUIFromAllUIStack(info);
            ManagerMask();
        }

        /// <summary>
        ///当某个ui关闭的时候，从m_AllUIStack中移除对应UI
        /// </summary>
        /// <param name="info"></param>
        private void RemoveCurUIFromAllUIStack(UIInfo info)
        {
            for (int i = m_AllUIStack.Count - 1; i >= 0; i--)
            {
                if (m_AllUIStack[i].info.Equals(info))
                {
                    LoadUIData loadData = m_AllUIStack[i];
                    m_AllUIStack.RemoveAt(i);
                    CallAfterCloseUI(loadData);
                    break;
                }
            }
        }

        
        private bool IsExitUIBase(LoadUIData data)
        {
            if (data.uiBase == null)
            {
                Debug.Log(data.info.loadPath.GetAssetName() + " uibase is null !!!!");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 关闭某个指定的UI
        /// </summary>
        /// <param name="info"></param>
        public void CloseCurUI(UIInfo info)
        {
            for (int i = m_AllUIStack.Count - 1; i >= 0; i--)
            {
                var data = m_AllUIStack[i];
                if (data.info.Equals(info) && !data.info.isClosed)
                {
                    data.info.isClosed = true;
                    if (IsExitUIBase(data))
                    {
                        m_AllUIStack[i].uiBase.OnClickCloseBtn();
                    }
                    else
                    {
                        Debug.Log("加载中关闭：" + info.loadPath + " " + data.isLoading);
                        RemoveCurUIFromAllUIStack(info);
                    }
                    CallAfterCloseUI(data);
                    break;
                }
            }
        }

        /// <summary>
        /// 关闭当前ui同Layer层级下的UI
        /// </summary>
        /// <param name="info"></param>
        public void CloseAllButCurUIByLayer(UIInfo info)
        {
            for (int i = m_AllUIStack.Count - 1; i >= 0; i--)
            {
                var data = m_AllUIStack[i];
                if (data.info.Equals(info))
                {
                    if (data.layer == UILayer.CommonLayer)
                    {
                        m_commonLoad.CloseAllUIButCurByLayer(info);
                    }
                    else
                    {
                        m_popupLoad.CloseAllUIButCurByLayer(info);
                    }
                    RemoveUIByLayerBurCurUI(data);
                    break;
                }
            }
        }

        /// <summary>
        /// 关闭此ui之外的所有ui
        /// </summary>
        public void CloseAllButCurUI(UIInfo info)
        {
            for (int i = m_AllUIStack.Count - 1; i >= 0; i--)
            {
                var data = m_AllUIStack[i];
                if (!data.info.Equals(info))
                {
                    if (IsExitUIBase(data))
                    {
                        data.uiBase.DeSpawnUI();
                    }
                    m_AllUIStack.RemoveAt(i);
                }
            }
            m_commonLoad.CloseAllUIButCurrent(info);
            m_popupLoad.CloseAllUIButCurrent(info);
        }

        /// <summary>
        /// 关闭所有的ui
        /// </summary>
        public void CloseAllUI()
        {
            m_commonLoad.CloseAllUI();
            m_popupLoad.CloseAllUI();
            m_AllUIStack.Clear();
        }

        private void RemoveUIByLayerBurCurUI(LoadUIData curData)
        {
            for (int i = m_AllUIStack.Count - 1; i >= 0; i--)
            {
                var data = m_AllUIStack[i];
                if (IsExitUIBase(data) && !data.Equals(curData) && data.uiBase.Layer == curData.layer)
                {
                    data.uiBase.DeSpawnUI();
                    m_AllUIStack.RemoveAt(i);
                }
            }
            ManagerMask();
        }

        /// <summary>
        /// 在关闭某个ui的时候，需要停止这个ui正在进行的协程
        /// </summary>
        /// <param name="data"></param>
        public void CallAfterCloseUI(LoadUIData data)
        {
            if (data.info.iterator != null)
            {
                StopCoroutine(data.info.iterator);
            }
        }
        #endregion
    }
}
