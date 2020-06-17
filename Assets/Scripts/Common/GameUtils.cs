using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUtils
{
    // BOOL
    public static void PlayerSetBool(string key, bool value)
    {
        PlayerPrefs.SetInt(key, value ? 1 : 0);
    }
    public static bool PlayerGetBool(string key, bool defaultValue)
    {
        int result = PlayerPrefs.GetInt(key, defaultValue ? 1 : 0);
        return result != 0;
    }

    public static void PlayerSetFloat(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
    }

    public static float PlayerGetFloat(string key, float defaultValue = 0f)
    {
        return PlayerPrefs.GetFloat(key, defaultValue);
    }
}
