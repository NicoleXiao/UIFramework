using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
            yield return CSpriteAtlasManager.instance.LoadAllSpriteAtlas(AddressableMgr.instance.spriteAtlasPrimaryKeys);
            SceneManager.LoadScene("UI");
        }
        
    }
}
