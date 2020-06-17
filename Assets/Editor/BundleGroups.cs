using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using UnityEngine.Serialization;

namespace UIFramework
{

    [Serializable]
    public class BundleGroupItem
    {
        [Tooltip("AddressGroupName")]
        public string groupName;

        [FormerlySerializedAs("directorPath")]
        [Tooltip("资源目录")]
        public string directoryPath;

        [Tooltip("目录说明")]
        public string desc;

        [Tooltip("资源过滤类型")]
        public string fileExtensionFilters;

        [Tooltip("忽略文件名")]
        public string excludeFileNames;

        [Tooltip("AssetBundle 打包模式")]
        public BundledAssetGroupSchema.BundlePackingMode bundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;

        [Tooltip("AssetBundle 名字格式")]
        public BundledAssetGroupSchema.BundleNamingStyle namingStyle = BundledAssetGroupSchema.BundleNamingStyle.NoHash;

        public string[] GetAllAssets()
        {
            return ObjectExtension.FindAssets(new string[] { directoryPath }, fileExtensionFilters, excludeFileNames);
        }
    }


    [Serializable]
    public class UIAtlasGroup
    {
        [Tooltip("存放图片的根目录")]
        public string rootDirectory;

        [Tooltip("资源过滤类型")]
        public string fileExtensionFilters;

        [Tooltip("忽略文件名")]
        public string excludeFileNames;

        public string[] GetAllAssets()
        {
            return ObjectExtension.FindAssets(new string[] { rootDirectory }, fileExtensionFilters, excludeFileNames);
        }
    }


    [CreateAssetMenu(fileName = "BundleGroups", menuName = "Bundles/BundleGroups.Asset")]
    public class BundleGroups : ScriptableObject
    {
        [TableList]
        public List<UIAtlasGroup> SpriteAtlasItems;

        [TableList]
        public List<BundleGroupItem> BundleItems;
    }
}
