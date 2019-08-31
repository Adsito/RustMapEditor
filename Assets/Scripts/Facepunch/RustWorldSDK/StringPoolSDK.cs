// Decompiled with JetBrains decompiler
// Type: StringPool
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FFBD04B0-3818-45ED-99BC-2B8616F3C2C4
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Rust\RustClient_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

public class StringPoolSDK
{
    public static Dictionary<uint, string> toString;
    private static Dictionary<string, uint> toNumber;
    private static bool initialized;
    public static uint closest;

    private static void Init()
    {
        if (StringPoolSDK.initialized)
            return;
        StringPoolSDK.toString = new Dictionary<uint, string>();
        StringPoolSDK.toNumber = new Dictionary<string, uint>((IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase);
        GameManifestSDK gameManifest = FileSystemSDK.Load<GameManifestSDK>("Assets/manifest.asset", true);
        for (uint index = 0; (long)index < (long)gameManifest.pooledStrings.Length; ++index)
        {
            StringPoolSDK.toString.Add(gameManifest.pooledStrings[index].hash, gameManifest.pooledStrings[index].str);
            StringPoolSDK.toNumber.Add(gameManifest.pooledStrings[index].str, gameManifest.pooledStrings[index].hash);
        }
        StringPoolSDK.initialized = true;
        StringPoolSDK.closest = StringPoolSDK.Get("closest");
    }

    public static string Get(uint i)
    {
        if ((int)i == 0)
            return string.Empty;
        StringPoolSDK.Init();
        string str;
        if (StringPoolSDK.toString.TryGetValue(i, out str))
            return str;
        Debug.LogWarning((object)("StringPool.GetString - no string for ID" + (object)i));
        return string.Empty;
    }

    public static uint Get(string str)
    {
        if (string.IsNullOrEmpty(str))
            return 0;
        StringPoolSDK.Init();
        uint num;
        if (StringPoolSDK.toNumber.TryGetValue(str, out num))
            return num;
        Debug.LogWarning((object)("StringPool.GetNumber - no number for string " + str));
        return 0;
    }

    public static uint Add(string str)
    {
        uint key = 0;
        if (!StringPoolSDK.toNumber.TryGetValue(str, out key))
        {
            //key = str.ManifestHash();
            StringPoolSDK.toString.Add(key, str);
            StringPoolSDK.toNumber.Add(str, key);
        }
        return key;
    }
}
