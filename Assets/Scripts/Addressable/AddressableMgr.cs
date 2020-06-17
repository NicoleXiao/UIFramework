using Sirenix.Utilities;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.U2D;

namespace UIFramework
{
    public class AddressableMgr : Singleton<AddressableMgr>
    {

        public List<object> primaryKeys { get; } = new List<object>();

        public List<string> spriteAtlasPrimaryKeys = new List<string>();

        public Dictionary<string, string> spriteAtlasKeys { get; } = new Dictionary<string, string>();

        private IList<IResourceLocation> m_objectLocations { get; } = new List<IResourceLocation>();

        private IList<IResourceLocation> m_spriteAtlasLocation { get; } = new List<IResourceLocation>();

        public bool hasDlcContent { get; private set; } = false;


        public override void Init()
        {
            var locators = Addressables.ResourceLocators;
            foreach (var locator in locators)
            {
                var keys = locator.Keys;
                IList<IResourceLocation> _objLocations = null;
                IList<IResourceLocation> _spriteAtlasLocations = null;

                foreach (var key in keys)
                {
                    if (locator.Locate(key, typeof(Object), out _objLocations))
                    {
                        if (_objLocations != null && _objLocations.Count > 0)
                        {
                            m_objectLocations.AddRange(_objLocations);
                        }
                    }

                    if (locator.Locate(key, typeof(SpriteAtlas), out _spriteAtlasLocations))
                    {
                        if (_spriteAtlasLocations != null && _spriteAtlasLocations.Count > 0)
                        {
                            m_spriteAtlasLocation.AddRange(_spriteAtlasLocations);
                        }
                    }
                    if (!hasDlcContent && key is string && key.ToString() == "dlc-content")
                    {
                        hasDlcContent = true;
                    }
                }
            }

            foreach (var location in m_objectLocations)
            {
                if (!primaryKeys.Contains(location.PrimaryKey))
                {
                    primaryKeys.Add(location.PrimaryKey);
                }
            }

            foreach (var spriteAtlasLoc in m_spriteAtlasLocation)
            {
                var primaryKey = spriteAtlasLoc.PrimaryKey;
                var key = Path.GetFileNameWithoutExtension(primaryKey);
                if (!spriteAtlasKeys.ContainsKey(key))
                {
                    spriteAtlasKeys.Add(key, primaryKey);
                }

                if (!spriteAtlasPrimaryKeys.Contains(primaryKey))
                {
                    spriteAtlasPrimaryKeys.Add(primaryKey);
                }
            }
        }

        public string GetSpriteAltasPrimaryKey(string tag)
        {
            string primary = string.Empty;
            spriteAtlasKeys.TryGetValue(tag, out primary);
            return primary;
        }
    }
}
