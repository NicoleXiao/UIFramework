using UnityEngine;
using UnityEngine.UI;


namespace UIFramework
{
    public class MaskManager : UIBase
    {
        public Image m_image;
        private UIInfo m_clickCloseUI = UIPath.None;
        public UIInfo clickCloseUI
        {
            get
            {
                return m_clickCloseUI;
            }
            set
            {
                m_clickCloseUI = value;
            }
        }

        public void SetTransparent()
        {
            m_image.color = new Color(0, 0, 0, 0);
        }

        public void SetAlpha()
        {
            m_image.color = new Color(0, 0, 0, 0.5f);
        }
    }
}