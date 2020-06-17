using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIFramework
{

    public enum UIStatus
    {
        Showing,
        Hide,
        Despawn
    }

    public enum UIType
    {
        None = 0,
        ComFull = 1,         //普通全屏界面
        ComPopup = 2,       //普通弹窗
        Tips = 3,          //提示信息UI
        SystemPopup = 4,  //最高弹窗UI
    }

    public enum UIMaskType
    {
        None = 0,                    //不需要遮罩
        OnlyMask = 1,               //黑色遮罩,无点击关闭UI效果
        MaskClickClose = 2,         //黑色遮罩，有点击关闭UI效果
        TransparentMask = 3,       //透明遮罩，无点击关闭UI效果
        TransparentClickMask = 4,  //透明遮罩，有点击关闭UI效果
    }

    public class UIInfo : IEquatable<UIInfo>
    {
        public bool isClosed;         //是否已经关闭了(异步加载UI的同时，有关闭此UI的指令)
        public UIType uiType;         //加载UI的类型
        public UIMaskType maskType;   //遮罩类型
        public string loadPath;       //加载的路径
        public object[] userDatas;    //UI需要的数据
        public IEnumerator iterator;  //当前ui开启的协程

        public UIInfo() { }

        public UIInfo(string path, UIType type, UIMaskType maskState = UIMaskType.None)
        {
            loadPath = "Prefabs/" + path;
            uiType = type;
            maskType = maskState;
            userDatas = null;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public bool Equals(UIInfo other)
        {
            return !ReferenceEquals(other, null) && loadPath == other.loadPath &&
                   uiType == other.uiType &&
                   maskType == other.maskType &&
                   EqualityComparer<object[]>.Default.Equals(userDatas, other.userDatas);
        }

        public override int GetHashCode()
        {
            var hashCode = -795526335;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(loadPath);
            hashCode = hashCode * -1521134295 + uiType.GetHashCode();
            hashCode = hashCode * -1521134295 + maskType.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<object[]>.Default.GetHashCode(userDatas);
            return hashCode;
        }

        public static bool operator ==(UIInfo info1, UIInfo info2)
        {
            return !ReferenceEquals(info1, null) && info1.Equals(info2);
        }

        public static bool operator !=(UIInfo info1, UIInfo info2)
        {
            return !(info1 == info2);
        }
    }

    public class UIPath
    {
        public static UIInfo None = new UIInfo(); 

        public static UIInfo MaskUI = new UIInfo("MaskUI.prefab", UIType.ComFull);

        public static UIInfo LoginUI = new UIInfo("LoginUI.prefab", UIType.ComFull);

        public static UIInfo MessageUI = new UIInfo("MessageUI.prefab", UIType.Tips,UIMaskType.MaskClickClose);

        public static UIInfo HomeUI = new UIInfo("HomeUI.prefab",UIType.ComFull);

    }

}
