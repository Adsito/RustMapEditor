using System;
using System.Collections.Generic;

public class StringPool
{
    public static Dictionary<uint, string> toString;
    private static Dictionary<string, uint> toNumber;
    private static bool initialized;
    public static uint closest;

    private static void Init()
    {
        if (StringPool.initialized)
            return;
        StringPool.toString = new Dictionary<uint, string>();
        StringPool.toNumber = new Dictionary<string, uint>((IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase);
        GameManifest gameManifest = BundleManager.Manifest;
        if (gameManifest == null) return;
        for (uint index = 0; (long)index < (long)gameManifest.pooledStrings.Length; ++index)
        {
            StringPool.toString.Add(gameManifest.pooledStrings[index].hash, gameManifest.pooledStrings[index].str);
            StringPool.toNumber.Add(gameManifest.pooledStrings[index].str, gameManifest.pooledStrings[index].hash);
        }
        StringPool.initialized = true;
        StringPool.closest = StringPool.Get("closest");
    }

    public static string Get(uint i)
    {
        if ((int)i == 0)
            return string.Empty;
        StringPool.Init();
        string str;
        if (StringPool.toString.TryGetValue(i, out str))
            return str;
        return string.Empty;
    }

    public static uint Get(string str)
    {
        if (string.IsNullOrEmpty(str))
            return 0;
        StringPool.Init();
        uint num;
        if (StringPool.toNumber.TryGetValue(str, out num))
            return num;
        return 0;
    }

    public static uint Add(string str)
    {
        uint key = 0;
        if (!StringPool.toNumber.TryGetValue(str, out key))
        {
            StringPool.toString.Add(key, str);
            StringPool.toNumber.Add(str, key);
        }
        return key;
    }
}
