using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace UIFramework
{

    public class UIBase : MonoBehaviour
    {
        [SerializeField]
        private Button m_backBtn;

        public bool IsInit { get; private set; }

        public UILayer Layer { get; set; }

        public UIInfo Info { get; private set; }

        public bool IsShowing { get { return m_status == UIStatus.Showing; } }

        private UIStatus m_status;

        public delegate void BackBtnDelegate(UIInfo ui);

        private event BackBtnDelegate m_CloseSelfEvent;
        //关闭此UI(其他UI不关闭)的事件 
        public event BackBtnDelegate CloseSelfEvent
        {
            add
            {
                m_CloseSelfEvent += value;
            }
            remove
            {
                m_CloseSelfEvent -= value;
            }
        }

        /// <summary>
        /// 基于3D模型  UI有两个摄像机
        /// 1.UICamera 层级在3D模型之上的。
        /// 2.UIBGCamera 层级在3D模型之下的(一般都是背景)
        /// </summary>
        public static string[] Camera = new string[]
         {
           "UICamera",
           "UIBGCamera",
         };
        [ValueDropdown("Camera")]
        public string m_Camera = Camera[0];

        private Canvas canvas;
        public Canvas UICanvas
        {
            get
            {
                if (canvas == null)
                {
                    canvas = this.GetComponent<Canvas>();
                }
                return canvas;
            }
        }

        private GraphicRaycaster m_raycaster;
        public GraphicRaycaster Raycaster
        {
            get
            {
                if (m_raycaster == null)
                {
                    m_raycaster = this.GetComponent<GraphicRaycaster>();
                }
                return m_raycaster;
            }
        }

        public void SetSortOrder(int order)
        {
            UICanvas.sortingOrder = order;
        }

        public void InitUI(UIInfo info)
        {
            SetCamera();
            this.Info = info;
            UICanvas.planeDistance = 0;
            if (m_backBtn != null)
            {
                m_backBtn.onClick.AddListener(() =>
                {
                    OnClickCloseBtn();
                });
            }
            IsInit = true;
        }

        private void SetCamera()
        {
            if (string.IsNullOrEmpty(m_Camera))
            {
                Debug.LogError("请设置Camera摄像机类型！！！！1");
                return;
            }
            UICanvas.worldCamera = GameObject.Find(m_Camera).GetComponent<Camera>();
        }

        /// <summary>
        /// 显示UI
        /// </summary>
        public IEnumerator ShowUI(params object[] userData)
        {
            if (userData != null && userData.Length > 0)
            {
                yield return BeforeShow(userData);
                yield return InnerShow(userData);
            }
            else
            {
                yield return BeforeShow();
                yield return InnerShow();
            }
        }

        /// <summary>
        /// 用于处理打开一个ui以后的事件，如关闭其他所有ui
        /// </summary>
        /// <returns></returns>
        public IEnumerator LoadCompleteUI(params object[] userData)
        {
            if (userData != null && userData.Length > 0)
            {
                yield return AfterShow(userData);
            }
            else
            {
                yield return AfterShow();
            }
        }

        private IEnumerator InnerShow(params object[] param)
        {
            m_status = UIStatus.Showing;
            gameObject.CustomSetActive(true);
            Raycaster.enabled = true;
            if (param != null && param.Length > 0)
            {
                Show(param);
            }
            else
            {
                Show();
            }

            yield break;
        }

        /// <summary>
        /// 从隐藏状态重新显示UI
        /// </summary>
        public void ResumeUI()
        {
            gameObject.CustomSetActive(true);
            Raycaster.enabled = true;
            Resume();
        }


        /// <summary>
        /// 隐藏UI
        /// </summary>
        public void HideUI()
        {
            m_status = UIStatus.Hide;
            Raycaster.enabled = false;
            gameObject.CustomSetActive(false);
            Hide();
        }

        /// <summary>
        /// 回收UI
        /// </summary>
        public void DeSpawnUI()
        {
            m_status = UIStatus.Despawn;
            m_CloseSelfEvent = null;
            Raycaster.enabled = false;
            DeSpawn();
            PoolManager.instance.DeSpawn<GameObject>(AssetType.Prefab, this.gameObject);
        }



        /// <summary>
        /// 点击关闭按钮
        /// </summary>
        public void OnClickCloseBtn()
        {
            ClickClose();
            if (m_CloseSelfEvent != null)
            {
                m_CloseSelfEvent(Info);
            }
        }


        #region 用于子类的继承
        protected virtual IEnumerator BeforeShow()
        {
            yield break;
        }

        protected virtual IEnumerator BeforeShow(params object[] param)
        {
            yield break;
        }

        protected virtual void Show()
        {

        }

        protected virtual void Show(params object[] param)
        {

        }

        protected virtual IEnumerator AfterShow()
        {
            yield break;
        }

        protected virtual IEnumerator AfterShow(params object[] param)
        {
            yield break;
        }


        protected virtual void ClickClose()
        {

        }

        /// <summary>
        /// 重新显示
        /// </summary>
        protected virtual void Resume()
        {

        }

        /// <summary>
        /// 隐藏
        /// </summary>
        protected virtual void Hide()
        {

        }

        /// <summary>
        /// 回收
        /// </summary>
        protected virtual void DeSpawn()
        {

        }

        /// <summary>
        /// 摧毁
        /// </summary>
        protected virtual void Destroy()
        {

        }
        #endregion

    }

}