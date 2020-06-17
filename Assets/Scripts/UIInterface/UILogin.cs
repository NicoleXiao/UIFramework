using UIFramework;
using UnityEngine;
using UnityEngine.UI;

public class UILogin : UIBase
{
    public InputField m_userName;
    public InputField m_password;


    protected override void Show()
    {
        base.Show();
        ReadLoginInfo();
    }

    public void OnClickConfirmBtn()
    {
        if (string.IsNullOrEmpty(m_userName.text))
        {
            UIManager.GetInstance().ShowMessage("请输入账号名字");
            return;
        }

        if (string.IsNullOrEmpty(m_password.text))
        {
            UIManager.GetInstance().ShowMessage("请输入密码");
            return;
        }
        SaveLoginInfo();
        UIManager.GetInstance().ShowUI(UIPath.HomeUI);

    }


    public void SaveLoginInfo()
    {
        PlayerPrefs.SetString("username", m_userName.text);
        PlayerPrefs.SetString("pwd", m_password.text);
    }

    public void ReadLoginInfo()
    {
        m_userName.text = PlayerPrefs.GetString("username");
        m_password.text = PlayerPrefs.GetString("pwd");
    }

}

