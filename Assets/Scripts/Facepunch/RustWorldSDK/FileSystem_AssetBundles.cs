// Decompiled with JetBrains decompiler
// Type: FileSystem_AssetBundles
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FFBD04B0-3818-45ED-99BC-2B8616F3C2C4
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Rust\RustClient_Data\Managed\Assembly-CSharp.dll


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

public class FileSystem_AssetBundles : IFileSystem
{
    public static string loadingError = string.Empty;
    private Dictionary<string, AssetBundle> bundles = new Dictionary<string, AssetBundle>((IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, AssetBundle> files = new Dictionary<string, AssetBundle>((IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase);
    public static bool isError;
    private AssetBundle rootBundle;
    private AssetBundleManifest manifest;
    private string assetPath;

    public FileSystem_AssetBundles(string assetRoot)
    {
        FileSystem_AssetBundles.isError = false;
        this.assetPath = Path.GetDirectoryName(assetRoot) + (object)Path.DirectorySeparatorChar;
        this.rootBundle = AssetBundle.LoadFromFile(assetRoot);
        if ((UnityEngine.Object)this.rootBundle == (UnityEngine.Object)null)
        {
            this.LoadError("Couldn't load root AssetBundle - " + assetRoot);
        }
        else
        {
            AssetBundleManifest[] assetBundleManifestArray = this.rootBundle.LoadAllAssets<AssetBundleManifest>();
            if (assetBundleManifestArray.Length != 1)
            {
                this.LoadError("Couldn't find AssetBundleManifest - " + (object)assetBundleManifestArray.Length);
            }
            else
            {
                this.manifest = assetBundleManifestArray[0];
                foreach (string allAssetBundle in this.manifest.GetAllAssetBundles())
                {
                    this.LoadBundle(allAssetBundle);
                    if (FileSystem_AssetBundles.isError)
                        return;
                }
                this.BuildFileIndex();
            }
        }
    }

    public void UnloadBundles()
    {
        this.manifest = (AssetBundleManifest)null;
        foreach (KeyValuePair<string, AssetBundle> bundle in this.bundles)
        {
            bundle.Value.Unload(false);
            UnityEngine.Object.DestroyImmediate((UnityEngine.Object)bundle.Value);
        }
        this.bundles.Clear();
        if (!(bool)((UnityEngine.Object)this.rootBundle))
            return;
        this.rootBundle.Unload(false);
        UnityEngine.Object.DestroyImmediate((UnityEngine.Object)this.rootBundle);
        this.rootBundle = (AssetBundle)null;
    }

    private void LoadError(string err)
    {
        UnityEngine.Debug.LogError((object)err);
        FileSystem_AssetBundles.loadingError = err;
        FileSystem_AssetBundles.isError = true;
    }

    private void LoadBundle(string bundleName)
    {
        if (this.bundles.ContainsKey(bundleName))
            return;
        string path = this.assetPath + bundleName;
        AssetBundle assetBundle = AssetBundle.LoadFromFile(path);
        if ((UnityEngine.Object)assetBundle == (UnityEngine.Object)null)
            this.LoadError("Couldn't load AssetBundle - " + path);
        else
            this.bundles.Add(bundleName, assetBundle);
    }

    private void BuildFileIndex()
    {
        this.files.Clear();
        foreach (KeyValuePair<string, AssetBundle> bundle in this.bundles)
        {
            if (!bundle.Key.StartsWith("content", StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (string allAssetName in bundle.Value.GetAllAssetNames())
                    this.files.Add(allAssetName, bundle.Value);
            }
        }
    }

    public T[] LoadAll<T>(string folder, string search) where T : UnityEngine.Object
    {
        List<T> objList = new List<T>();
        foreach (KeyValuePair<string, AssetBundle> keyValuePair in this.files.Where<KeyValuePair<string, AssetBundle>>((Func<KeyValuePair<string, AssetBundle>, bool>)(x => x.Key.StartsWith(folder, StringComparison.InvariantCultureIgnoreCase))))
        {
            if (string.IsNullOrEmpty(search) || keyValuePair.Key.Contains(search))
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                //if (FileConVar.debug)
                //    File.AppendAllText("filesystem_debug.csv", string.Format("LoadAll,{0}\n", (object)keyValuePair.Key));
                T obj = keyValuePair.Value.LoadAsset<T>(keyValuePair.Key);
                //if (FileConVar.time)
                //    File.AppendAllText("filesystem.csv", string.Format("LoadAll,{0},{1}\n", (object)keyValuePair.Key, (object)stopwatch.Elapsed.TotalMilliseconds));
                if (!((UnityEngine.Object)obj == (UnityEngine.Object)null))
                    objList.Add(obj);
            }
        }
        return objList.ToArray();
    }

    public T Load<T>(string filePath, bool bComplain = true) where T : UnityEngine.Object
    {
        AssetBundle assetBundle = (AssetBundle)null;
        if (!this.files.TryGetValue(filePath, out assetBundle))
        {
            UnityEngine.Debug.LogWarning((object)("[BUNDLE] Not found: " + filePath));
            return (T)null;
        }
        Stopwatch stopwatch = Stopwatch.StartNew();
        //if (FileConVar.debug)
        //    File.AppendAllText("filesystem_debug.csv", string.Format("Load,{0}\n", (object)filePath));
        T obj = assetBundle.LoadAsset<T>(filePath);
        //if (FileConVar.time)
        //    File.AppendAllText("filesystem.csv", string.Format("Load,{0},{1}\n", (object)filePath, (object)stopwatch.Elapsed.TotalMilliseconds));
        if ((UnityEngine.Object)obj == (UnityEngine.Object)null && bComplain)
            UnityEngine.Debug.LogWarning((object)("[BUNDLE] Not found in bundle: " + filePath));
        return obj;
    }

    public AsyncOperation LoadAsync(string filePath)
    {
        AssetBundle assetBundle = (AssetBundle)null;
        if (!this.files.TryGetValue(filePath, out assetBundle))
        {
            UnityEngine.Debug.LogWarning((object)("[BUNDLE] Not found: " + filePath));
            return (AsyncOperation)null;
        }
        AssetBundleRequest assetBundleRequest = assetBundle.LoadAssetAsync<UnityEngine.Object>(filePath);
        if (assetBundleRequest == null)
            UnityEngine.Debug.LogWarning((object)("[BUNDLE] Not found in bundle: " + filePath));
        return (AsyncOperation)assetBundleRequest;
    }

    public string[] FindAll(string folder, string search)
    {
        List<string> stringList = new List<string>();
        foreach (KeyValuePair<string, AssetBundle> keyValuePair in this.files.Where<KeyValuePair<string, AssetBundle>>((Func<KeyValuePair<string, AssetBundle>, bool>)(x => x.Key.StartsWith(folder, StringComparison.InvariantCultureIgnoreCase))))
        {
            if (string.IsNullOrEmpty(search) || keyValuePair.Key.Contains(search))
                stringList.Add(keyValuePair.Key);
        }
        return stringList.ToArray();
    }
}
