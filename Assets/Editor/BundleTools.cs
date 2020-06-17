using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.Utilities;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using System;
using System.IO;
using System.Linq;
using UnityEngine.U2D;
using UnityEditor.U2D;

namespace UIFramework
{
    public static class BundleTools
    {
        private const string BundleGroupConfigPath = "Assets/BundleSetting/BundleGroups.asset";

        private const string SettingPath = "Assets/AddressableAssetsData/AddressableAssetSettings.asset";


        [MenuItem("BundleTools/GenSpriteAtlas")]
        public static void GenSpriteAtlas()
        {
            var addressGroupConfig = AssetDatabase.LoadAssetAtPath<BundleGroups>(BundleGroupConfigPath);
            if (addressGroupConfig)
            {
                var atlasItems = addressGroupConfig.SpriteAtlasItems;
                EditorUtility.DisplayProgressBar("图集", "生成图集中", 0);
                List<string> spriteAtlasKeys = new List<string>();
                foreach (var item in atlasItems)
                {
                    var assets = item.GetAllAssets();
                    foreach (var path in assets)
                    {
                        var atlasFileName = path + ".spriteatlas";
                        if (Directory.GetDirectories(path).Length == 0)
                        {
                            if (!File.Exists(atlasFileName))
                            {
                                var obj = AssetDatabase.LoadMainAssetAtPath(path);
                                if (obj)
                                {
                                    SpriteAtlas spriteAtlas = new SpriteAtlas();
                                    spriteAtlas.Add(new UnityEngine.Object[] { obj });
                                    AssetDatabase.CreateAsset(spriteAtlas, atlasFileName);
                                }
                            }
                        }

                        var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasFileName);
                        if (atlas)
                        {
                            if (!spriteAtlasKeys.Contains(atlasFileName))
                            {
                                spriteAtlasKeys.Add(atlasFileName);
                            }
                            else
                            {
                                Debug.LogError("重复的图集名称，解决办法：文件夹名字不重复，即使在不同的文件夹！");
                            }

                            atlas.SetIncludeInBuild(false);
                            SpriteAtlasPackingSettings settings = SpriteAtlasExtensions.GetPackingSettings(atlas);
                            settings.enableTightPacking = false;
                            settings.enableRotation = false;
                            settings.padding = 2;
                            SpriteAtlasExtensions.SetPackingSettings(atlas, settings);

                        }
                    }
                }
                EditorUtility.DisplayProgressBar("图集", "Packing sprite atlas", 0.7f);
                SpriteAtlasUtility.PackAllAtlases(EditorUserBuildSettings.activeBuildTarget);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorUtility.ClearProgressBar();

            }
        }

        [MenuItem("BundleTools/AssetBundle/SetAddressableGroupName")]
        public static void SetAddressbaleGroupName()
        {
            try
            {
                var asset = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>(SettingPath);
                var addressGroupCfg = AssetDatabase.LoadAssetAtPath<BundleGroups>(BundleGroupConfigPath);
                var groups = addressGroupCfg.BundleItems;
                EditorUtility.DisplayCancelableProgressBar("Addressable 资源处理中", "清理Groups..", 0.1f);
                for (int i = asset.groups.Count - 1; i >= 0; i--)
                {
                    var assetGroup = asset.groups[i];
                    if (assetGroup == null)
                    {
                        asset.groups.RemoveAt(i);
                        continue;
                    }

                    if (assetGroup.name.Contains("Builde In Data"))
                    {
                        continue;
                    }

                    if (assetGroup.IsDefaultGroup())
                    {
                        continue;
                    }

                    if (assetGroup.entries.Count <= 0 && IsDefaultGroup(assetGroup))
                    {
                        asset.groups.RemoveAt(i);
                        asset.RemoveGroup(assetGroup);
                    }
                }

                EditorUtility.DisplayCancelableProgressBar("Addressable 资源处理中", "清理Groups..", 0.2f);

                for (int j = 0; j < groups.Count; j++)
                {
                    var item = groups[j];

                    if (string.IsNullOrEmpty(item.groupName))
                    {
                        throw new ArgumentNullException("groupName");
                    }
                    EditorUtility.DisplayCancelableProgressBar("Addressable 资源处理中", item.groupName, (float)j / groups.Count);
                    var assets = item.GetAllAssets();
                    var addressGroup = asset.FindGroup(item.groupName);
                    if (!addressGroup)
                    {
                        var groupSchmeTemple = (asset.GroupTemplateObjects[0] as AddressableAssetGroupTemplate);
                        addressGroup = asset.CreateGroup(item.groupName, false, false, false, null, groupSchmeTemple.GetTypes());
                        groupSchmeTemple.ApplyToAddressableAssetGroup(addressGroup);
                    }

                    if (addressGroup)
                    {
                        var bundleAssetGroup = addressGroup.GetSchema<BundledAssetGroupSchema>();
                        if (!bundleAssetGroup)
                        {
                            bundleAssetGroup = addressGroup.AddSchema<BundledAssetGroupSchema>(false);
                        }

                        bundleAssetGroup.UseAssetBundleCrc = false;
                        bundleAssetGroup.BundleNaming = item.namingStyle;
                        bundleAssetGroup.BundleMode = item.bundleMode;


                        var contentUpdateScnema = addressGroup.GetSchema<ContentUpdateGroupSchema>();
                        if (!contentUpdateScnema)
                        {
                            contentUpdateScnema = addressGroup.AddSchema<ContentUpdateGroupSchema>(false);
                        }
                        contentUpdateScnema.StaticContent = true;
                    }
                    foreach (var assetPath in assets)
                    {
                        var guid = AssetDatabase.AssetPathToGUID(assetPath);
                        var entry = asset.CreateOrMoveEntry(guid, addressGroup, false, false);
                        entry.SetAddress(GetSimpleAddresName(entry.address), false);
                    }
                }

                for (int i = 0; i < asset.groups.Count; i++)
                {
                    var item = asset.groups[i];
                    if (!IsDefaultGroup(item))
                    {
                        foreach (var entry in item.entries.ToArray())
                        {
                            string filePath = Path.Combine(Application.dataPath, "BundleResource", entry.address);
                            if (string.IsNullOrEmpty(entry.AssetPath))
                            {
                                item.RemoveAssetEntry(entry);
                                Debug.Log("AssetRemove:" + filePath);
                            }

                            if (File.Exists(filePath))
                            {
                                entry.SetLabel("dlc-content", false);
                                entry.SetAddress(GetSimpleAddresName(entry.address), false);
                            }
                            else
                            {
                                Debug.Log(filePath);
                                item.RemoveAssetEntry(entry);
                            }
                        }
                    }
                }

                AssetDatabase.SaveAssets();
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static string GetSimpleAddresName(string addressName)
        {
            return addressName.ToLower().Replace("assets/bundleresource/", "").ToLower();
        }

        private static bool IsDefaultGroup(AddressableAssetGroup group)
        {
            if (group == null) return false;
            if (group.IsDefaultGroup() || group.GetSchema<PlayerDataGroupSchema>())
            {
                return true;
            }

            return false;
        }

        [MenuItem("BundleTools/AssetBundle/Remove All Bundle Name")]
        public static void RemoveBundleNameTools()
        {
            var bundleNames = AssetDatabase.GetAllAssetBundleNames();
            bundleNames.ForEach((s) =>
            {
                var assets = AssetDatabase.GetAssetPathsFromAssetBundle(s);
                assets.ForEach((a) =>
                {
                    var import = AssetImporter.GetAtPath(a);
                    import.assetBundleName = "";
                    if (!string.IsNullOrWhiteSpace(import.assetBundleVariant))
                    {
                        import.assetBundleVariant = "";
                    }
                });

            });
            AssetDatabase.RemoveUnusedAssetBundleNames();
        }
    }
}