using System.Collections;
using UnityEngine.UI;
using UIFramework;

public class MessageUI : UIBase
{
    public Text m_title;
    public Text m_messageInfo;

    protected override IEnumerator BeforeShow(params object[] param)
    {
        if (param != null && param.Length > 0)
        {
            m_messageInfo.text = param[0].ToString();
            if (param.Length >= 2)
            {
                var title = param[1].ToString();
                if (!string.IsNullOrEmpty(title))
                {
                    m_title.text = title;
                }
            }
        }
        return base.BeforeShow();
    }

    protected override void Show()
    {
        CloseSelfEvent += Close;
    }

    private void Close(UIInfo ui)
    {
        DeSpawnUI();
        m_messageInfo.text = "";
    }
}

