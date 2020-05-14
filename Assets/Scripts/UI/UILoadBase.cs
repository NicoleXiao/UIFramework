using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace UIFramework
{
    /// <summary>
    /// UI加载的基类
    /// </summary>
    public class UILoadBase : MonoBehaviour
    {
        /**每个ui sortOrder的差值*/
        protected int m_layerSpace=100;
        /**开始层级初始值*/
        protected int m_startLayer = 100;
        /**存放加载的ui信息*/
        protected List<LoadUIData> m_UIStack = new List<LoadUIData>();

        /// <summary>
        /// 根据当前在UIStack的位置获取相应显示层级
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected int GetSortOrderByIndex(int index, UILayer layer)
        {
            if (index <= 0)
            {
                index = 0;
            }
            return m_layerSpace * index + 1 + m_startLayer + ((int)layer - 1) * 1000;
        }

        /// <summary>
        ///关闭界面事件
        /// </summary>
        /// <param name="ui"></param>
        public void ClickCloseUI(UIInfo info)
        {
            CloseCurrentUI(info);
        }

        protected int GetUIStackIndex(UIInfo info)
        {
            for (int i = m_UIStack.Count - 1; i >= 0; i--)
            {
                if (m_UIStack[i].info.Equals(info))
                {
                    return i;
                }
            }
            return 0;
        }

        /// <summary>
        /// 获取当前最高层级数
        /// </summary>
        /// <returns></returns>
        public int GetTopUILayer(UILayer layer)
        {
            return GetSortOrderByIndex(GetCurLayerTotalIndex(layer)-1, layer); ;
        }

        private int GetCurLayerTotalIndex(UILayer layer)
        {
            int index = 0;
            for (int i = 0; i < m_UIStack.Count; i++)
            {
                UIBase ui = m_UIStack[i].uiBase;
                if (ui != null && ui.Layer == layer)
                {
                    index++;
                }
            }
            return index;
        }


        #region Mask相关
        /// <summary>
        /// 获取需要Mask的UI信息
        /// </summary>
        /// <returns></returns>
        public LoadUIData GetNeedMaskUIInfo()
        {
            for (int i = m_UIStack.Count - 1; i >= 0; i--)
            {
                if (m_UIStack[i].info.maskType != UIMaskType.None)
                {
                    return m_UIStack[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Mask点击关闭事件处理
        /// </summary>
        /// <param name="ui"></param>
        public virtual void MaskClickClose(UIInfo info)
        {

        }
        #endregion

        #region 外部调用关闭UI接口

        public void CloseAllUI()
        {
            for (int i = 0; i < m_UIStack.Count; i++)
            {
                LoadUIData data = m_UIStack[i];
                if (data.uiBase != null)
                {
                    data.uiBase.DeSpawnUI();
                }
            }
            m_UIStack.Clear();
        }

        public virtual void CloseAllUIButCurByLayer(UIInfo info)
        {
        }


        /// <summary>
        /// 关闭除当前UI的所有UI,如果path是空的则关闭所有的
        /// </summary>
        public virtual void CloseAllUIButCurrent(UIInfo info)
        {
        }

        /// <summary>
        /// 关闭指定UI
        /// </summary>
        /// <param name="name"></param>
        public virtual void CloseCurrentUI(UIInfo info)
        {

        }
        #endregion

        public void PushUIToStack(LoadUIData data)
        {
            var ui = data.uiBase;
            ui.UICanvas.sortingOrder = GetSortOrderByIndex(GetCurLayerTotalIndex(data.layer),data.layer);
            ui.CloseSelfEvent += ClickCloseUI;
            data.uiBase.Layer = data.layer;
            m_UIStack.Add(data);
        }

        public void LoadComplete(LoadUIData data)
        {
            PushUISuccessful(data);
        }

        /// <summary>
        /// Push UI成功之后的事件
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="info"></param>
        /// <param name="loader"></param>
        /// <param name="showEvent"></param>
        /// <param name="userData"></param>
        protected virtual void PushUISuccessful(LoadUIData data)
        {

        }

    }
}
