using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIFramework
{
    /// <summary>
    /// 弹窗ui加载
    /// </summary>
    public class UIPopupLoad : UILoadBase
    {
        public override void MaskClickClose(UIInfo info)
        {
            int removeIndex = 0;
            for (int i = m_UIStack.Count - 1; i >= 0; i--)
            {
                var ui = m_UIStack[i];
                if (ui.info.Equals(info))
                {
                    if (ui.uiBase != null)
                    {
                        ui.uiBase.DeSpawnUI();
                    }
                    removeIndex = i;
                    m_UIStack.RemoveAt(i);
                    break;
                }
            }
            for (int j = removeIndex; j < m_UIStack.Count; j++)
            {
                m_UIStack[j].uiBase.SetSortOrder(GetSortOrderByIndex(j, m_UIStack[j].layer));
            }
        }

        public override void CloseAllUIButCurByLayer(UIInfo info)
        {
            UILayer layer = GetUILayer(info);
            for (int i = m_UIStack.Count - 1; i >= 0; i--)
            {
                var ui = m_UIStack[i];
                if (!ui.info.Equals(info) && ui.uiBase.Layer == layer)
                {
                    ui.uiBase.DeSpawnUI();
                    m_UIStack.RemoveAt(i);
                }
                else
                {
                    ui.uiBase.SetSortOrder(GetSortOrderByIndex(0, ui.uiBase.Layer));
                }
            }
        }

        private UILayer GetUILayer(UIInfo info)
        {
            for (int i = m_UIStack.Count - 1; i >= 0; i--)
            {
                var ui = m_UIStack[i];
                if (ui.info.Equals(info))
                {
                    return ui.uiBase.Layer;
                }
            }
            return UILayer.None;
        }

        public override void CloseAllUIButCurrent(UIInfo info)
        {
            for (int i = m_UIStack.Count - 1; i >= 0; i--)
            {
                var ui = m_UIStack[i];
                if (!ui.info.Equals(info))
                {
                    ui.uiBase.DeSpawnUI();
                    m_UIStack.RemoveAt(i);
                }
                else
                {
                    ui.uiBase.SetSortOrder(GetSortOrderByIndex(0, ui.uiBase.Layer));
                }
            }
        }


        public override void CloseCurrentUI(UIInfo info)
        {
            for (int i = m_UIStack.Count - 1; i >= 0; i--)
            {
                var ui = m_UIStack[i];
                if (ui.info.Equals(info))
                {
                    ui.uiBase.DeSpawnUI();
                    m_UIStack.RemoveAt(i);
                    break;
                }
            }
        }
    }
}
