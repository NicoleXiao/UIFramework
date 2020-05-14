using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;


namespace UIFramework
{
    /// <summary>
    /// 普通界面加载
    /// </summary>
    public class UICommonLoad : UILoadBase
    {
        /// <summary>
        /// 成功加载了UI的事件
        /// loader为空就是从对象池中取出来的
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="data"></param>
        /// <param name="loader"></param>
        /// <param name="showEvent"></param>
        protected override void PushUISuccessful(LoadUIData data)
        {
            ManagerUIShowEvent(data);
        }

        /// <summary>
        ///关闭普通界面事件
        /// </summary>
        /// <param name="ui"></param>
        public void ClickCloseCommonUI(UIInfo info)
        {
            CloseCurrentUI(info);
        }

        #region UI界面显示和隐藏事件处理
        /// <summary>
        /// 当ui显示的时候(包括出生或者重新显示)需要处理的事件
        /// 比如是否要添加Mask，ComFull类型的ui是全屏，需要隐藏在此ui之前的所有全屏ui，ComPopup 是独立弹窗不受影响
        /// </summary>
        /// <param name="ui"></param>
        private void ManagerUIShowEvent(LoadUIData data)
        {
            if (data.info.uiType == UIType.ComFull)
            {
                bool startHide = false;
                if (m_UIStack.Count >= 2)
                {
                    for (int i = m_UIStack.Count - 1; i >= 0; i--)
                    {
                        var ui = m_UIStack[i];
                        if (ui.info.uiType != UIType.ComPopup)
                        {
                            if (ui.info.Equals(data.info))
                            {
                                startHide = true;
                                continue;
                            }
                            if (startHide)
                            {
                                ui.uiBase.HideUI();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 关闭当前UI事件处理
        /// </summary>
        /// <param name="info"></param>
        /// <param name="isCloseCurrent"></param>
        private void ManagerCloseCurUIEvent(UIInfo info)
        {
            if (m_UIStack.Count > 0)
            {
                int removeIndex = -1;
                for (int i = m_UIStack.Count - 1; i >= 0; i--)
                {
                    if (m_UIStack[i].info.Equals(info))
                    {
                        removeIndex = i;
                        m_UIStack[i].uiBase.DeSpawnUI();
                        m_UIStack.RemoveAt(i);
                    }
                    if (i < removeIndex && !m_UIStack[i].uiBase.IsShowing && m_UIStack[i].info.uiType != UIType.ComFull)
                    {
                        m_UIStack[i].uiBase.ResumeUI();
                    }
                    if (i < removeIndex && m_UIStack[i].info.uiType == UIType.ComFull)
                    {
                        break;
                    }
                }
                //刷新sortOrder
                if (removeIndex != -1)
                {
                    for (int j = removeIndex; j < m_UIStack.Count; j++)
                    {
                        m_UIStack[j].uiBase.SetSortOrder(GetSortOrderByIndex(j, UILayer.CommonLayer));
                    }
                }
            }
        }

        /// <summary>
        /// 关闭除了当前UI的所有UI事件处理
        /// </summary>
        private void ManagerCloseAllButCurrentEvent(UIInfo info)
        {
            if (m_UIStack.Count > 0)
            {
                for (int i = m_UIStack.Count - 1; i >= 0; i--)
                {
                    if (!m_UIStack[i].info.Equals(info))
                    {
                        m_UIStack[i].uiBase.DeSpawnUI();
                        m_UIStack.RemoveAt(i);
                    }
                }
                //关闭到最后剩一个UI，刷新下层级,因为加载时间的问题，会出现uibase还没有加载出来的问题
                if (m_UIStack.Count == 1)
                {
                    if (m_UIStack[0].uiBase != null)
                    {
                        m_UIStack[0].uiBase.SetSortOrder(GetSortOrderByIndex(0, UILayer.CommonLayer));
                    }
                    else
                    {
                        var data = m_UIStack[0];
                        data.sortOrder = GetSortOrderByIndex(0, UILayer.CommonLayer);
                        m_UIStack[0] = data;
                    }
                }
            }
        }
        #endregion

        public override void MaskClickClose(UIInfo info)
        {
            for (int i = m_UIStack.Count - 1; i >= 0; i--)
            {
                if (m_UIStack[i].info.Equals(info))
                {
                    ManagerCloseCurUIEvent(m_UIStack[i].info);
                    break;
                }
            }
        }

        public override void CloseCurrentUI(UIInfo info)
        {
            ManagerCloseCurUIEvent(info);
        }

        public override void CloseAllUIButCurrent(UIInfo info)
        {
            ManagerCloseAllButCurrentEvent(info);
        }

        public override void CloseAllUIButCurByLayer(UIInfo info)
        {
            ManagerCloseAllButCurrentEvent(info);
        }

    }
}
