// Decompiled with JetBrains decompiler
// Type: GameManifest
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FFBD04B0-3818-45ED-99BC-2B8616F3C2C4
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Rust\RustClient_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class GameManifest : ScriptableObject
{
    internal static Dictionary<string, string> guidToPath = new Dictionary<string, string>();
    internal static Dictionary<string, UnityEngine.Object> guidToObject = new Dictionary<string, UnityEngine.Object>();
    public GameManifest.PooledString[] pooledStrings;
    public GameManifest.MeshColliderInfo[] meshColliders;
    public GameManifest.PrefabProperties[] prefabProperties;
    public GameManifest.EffectCategory[] effectCategories;
    public string[] skinnables;
    public string[] entities;
    internal static GameManifest loadedManifest;

    public static GameManifest Current
    {
        get
        {
            if ((UnityEngine.Object)GameManifest.loadedManifest != (UnityEngine.Object)null)
                return GameManifest.loadedManifest;
            GameManifest.Load();
            return GameManifest.loadedManifest;
        }
    }
    public static void Load(GameManifest loadedManifest)
    {
        if ((UnityEngine.Object)GameManifest.loadedManifest != (UnityEngine.Object)null)
            return;
        GameManifest.loadedManifest = loadedManifest;
        //Debug.Log(loadedManifest.prefabProperties.Length);
        foreach (GameManifest.PrefabProperties prefabProperty in GameManifest.loadedManifest.prefabProperties)
            GameManifest.guidToPath.Add(prefabProperty.guid, prefabProperty.name);
        //DebugE.Log((object)GameManifest.GetMetadataStatus(), StackTraceLogType.None);
    }
    public static void Load()
    {
        if ((UnityEngine.Object)GameManifest.loadedManifest != (UnityEngine.Object)null)
            return;
        GameManifest.loadedManifest = FileSystem.Load<GameManifest>("Assets/manifest.asset", true);
        foreach (GameManifest.PrefabProperties prefabProperty in GameManifest.loadedManifest.prefabProperties)
            GameManifest.guidToPath.Add(prefabProperty.guid, prefabProperty.name);
        //DebugE.Log((object)GameManifest.GetMetadataStatus(), StackTraceLogType.None);
    }
/*
    public static void LoadAssets()
    {
        if (Skinnable.All != null)
            return;
        Skinnable.All = GameManifest.LoadSkinnableAssets();
        DebugEx.Log((object)GameManifest.GetAssetStatus(), StackTraceLogType.None);
    }

    private static Skinnable[] LoadSkinnableAssets()
    {
        string[] skinnables = GameManifest.loadedManifest.skinnables;
        Skinnable[] skinnableArray = new Skinnable[skinnables.Length];
        for (int index = 0; index < skinnables.Length; ++index)
            skinnableArray[index] = FileSystem.Load<Skinnable>(skinnables[index], true);
        return skinnableArray;
    }*/

    internal static Dictionary<string, string[]> LoadEffectDictionary()
    {
        GameManifest.EffectCategory[] effectCategories = GameManifest.loadedManifest.effectCategories;
        Dictionary<string, string[]> dictionary = new Dictionary<string, string[]>();
        foreach (GameManifest.EffectCategory effectCategory in effectCategories)
            dictionary.Add(effectCategory.folder, effectCategory.prefabs.ToArray());
        return dictionary;
    }

    internal static string GUIDToPath(string guid)
    {
        if (string.IsNullOrEmpty(guid))
        {
            Debug.LogError((object)"GUIDToPath: guid is empty");
            return string.Empty;
        }
        GameManifest.Load();
        string str;
        if (GameManifest.guidToPath.TryGetValue(guid, out str))
            return str;
        Debug.LogWarning((object)("GUIDToPath: no path found for guid " + guid));
        return string.Empty;
    }

    internal static UnityEngine.Object GUIDToObject(string guid)
    {
        UnityEngine.Object object1 = (UnityEngine.Object)null;
        if (GameManifest.guidToObject.TryGetValue(guid, out object1))
            return object1;
        string path = GameManifest.GUIDToPath(guid);
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogWarning((object)("Missing file for guid " + guid));
            GameManifest.guidToObject.Add(guid, (UnityEngine.Object)null);
            return (UnityEngine.Object)null;
        }
        UnityEngine.Object object2 = FileSystem.Load<UnityEngine.Object>(path, true);
        GameManifest.guidToObject.Add(guid, object2);
        return object2;
    }

    private static string GetMetadataStatus()
    {
        StringBuilder stringBuilder = new StringBuilder();
        if ((UnityEngine.Object)GameManifest.loadedManifest != (UnityEngine.Object)null)
        {
            stringBuilder.Append("Manifest Metadata Loaded");
            stringBuilder.AppendLine();
            stringBuilder.Append("\t");
            stringBuilder.Append(GameManifest.loadedManifest.pooledStrings.Length.ToString());
            stringBuilder.Append(" pooled strings");
            stringBuilder.AppendLine();
            stringBuilder.Append("\t");
            stringBuilder.Append(GameManifest.loadedManifest.meshColliders.Length.ToString());
            stringBuilder.Append(" mesh colliders");
            stringBuilder.AppendLine();
            stringBuilder.Append("\t");
            stringBuilder.Append(GameManifest.loadedManifest.prefabProperties.Length.ToString());
            stringBuilder.Append(" prefab properties");
            stringBuilder.AppendLine();
            stringBuilder.Append("\t");
            stringBuilder.Append(GameManifest.loadedManifest.effectCategories.Length.ToString());
            stringBuilder.Append(" effect categories");
            stringBuilder.AppendLine();
            stringBuilder.Append("\t");
            stringBuilder.Append(GameManifest.loadedManifest.entities.Length.ToString());
            stringBuilder.Append(" entity names");
            stringBuilder.AppendLine();
            stringBuilder.Append("\t");
            stringBuilder.Append(GameManifest.loadedManifest.skinnables.Length.ToString());
            stringBuilder.Append(" skinnable names");
        }
        else
            stringBuilder.Append("Manifest Metadata Missing");
        return stringBuilder.ToString();
    }

    private static string GetAssetStatus()
    {
        StringBuilder stringBuilder = new StringBuilder();
        if ((UnityEngine.Object)GameManifest.loadedManifest != (UnityEngine.Object)null)
        {
            stringBuilder.Append("Manifest Assets Loaded");
            stringBuilder.AppendLine();
            stringBuilder.Append("\t");
            //stringBuilder.Append(Skinnable.All == null ? "0" : Skinnable.All.Length.ToString());
            stringBuilder.Append(" skinnable objects");
        }
        else
            stringBuilder.Append("Manifest Assets Missing");
        return stringBuilder.ToString();
    }

    [Serializable]
    public struct PooledString
    {
        [HideInInspector]
        public string str;
        public uint hash;
    }

    [Serializable]
    public class MeshColliderInfo
    {
        [HideInInspector]
        public string name;
        public uint hash;
        public PhysicMaterial physicMaterial;
    }

    [Serializable]
    public class PrefabProperties
    {
        [HideInInspector]
        public string name;
        public string guid;
        public uint hash;
        public bool needsWarmup;
    }

    [Serializable]
    public class EffectCategory
    {
        [HideInInspector]
        public string folder;
        public List<string> prefabs;
    }
}
