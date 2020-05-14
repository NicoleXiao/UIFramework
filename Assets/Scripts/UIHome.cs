using System.Collections;
using System.Collections.Generic;
using UIFramework;
using UnityEngine;

public class UIHome : UIBase
{
    protected override void Show()
    {
        base.Show();
    }

    protected override IEnumerator AfterShow()
    {
        UIManager.GetInstance().CloseAllButCurUIByLayer(Info);
        return base.AfterShow();
    }

    public void OnClickEquipBtn()
    {

    }

    public void OnClickSignBtn()
    {

    }

    public void OnClickMatchBtn()
    {

    }
}
