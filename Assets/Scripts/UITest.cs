using UIFramework;
using UnityEngine;


public class UITest : MonoBehaviour
{
    void Start()
    {
        UIManager.GetInstance().ShowUI(UIPath.LoginUI);
    }


}
