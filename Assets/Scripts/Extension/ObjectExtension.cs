using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace UIFramework
{
    public static class ObjectExtension
    {
        public static string GetAssetName(this string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                UnityEngine.Debug.LogError("GetAssetName()-->the asset path is empty!");
                return string.Empty;
            }
            var index1 = assetPath.LastIndexOf('/');
            var index2 = assetPath.LastIndexOf('.');
            return assetPath.Substring(index1 + 1, index2 - index1 - 1);
        }

#if UNITY_EDITOR
        public static string[] FindAssets(string[] folders, string filter, string exincludeName)
        {
            List<string> list = new List<string>();
            var assets = AssetDatabase.FindAssets(filter, folders);
            foreach (var guid in assets)
            {
                list.Add(AssetDatabase.GUIDToAssetPath(guid).ToLower());
            }
            string[] exinclude = null;
            if (!string.IsNullOrEmpty(exincludeName))
            {
                exinclude = exincludeName.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            }
            if (exinclude != null && exinclude.Length > 0)
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    string assetPath = list[i];
                    for (int j = 0; j < exinclude.Length; j++)
                    {
                        if (assetPath.Contains(exinclude[j]))
                        {
                            list.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
            return list.ToArray();
        }
#endif
    }
}
