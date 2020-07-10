using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace UIFramework
{
    /// <summary>
    /// 游戏初始化
    /// </summary>
    public class GameInit : MonoBehaviour
    {

        private void Start()
        {
            StartCoroutine(BeforeInit());
        }



        public IEnumerator BeforeInit()
        {
            CSpriteAtlasManager.GetInstance().Initialize();
            var op = Addressables.InitializeAsync();
            op.Completed += (s) =>
            {
                AddressableMgr.GetInstance().Initialize();
            };
            yield return op;
            yield return CSpriteAtlasManager.instance.LoadAllSpriteAtlas(AddressableMgr.instance.spriteAtlasPrimaryKeys);
            SceneManager.LoadScene("UI");
        }
        
    }
}
