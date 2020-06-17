using System.Collections;
using System.Collections.Generic;
using UIFramework;
using UnityEngine;

public class UIHome : UIBase
{
    public AddressableImage m_image;
    protected override void Show()
    {
        base.Show();
        //atlas name[sprite name]
        string path = $"atlas/common.spriteatlas[ui_comprop_close]";
        m_image.SetSprite(path);
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
