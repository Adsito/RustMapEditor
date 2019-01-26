// Decompiled with JetBrains decompiler
// Type: FileSystem
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FFBD04B0-3818-45ED-99BC-2B8616F3C2C4
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Rust\RustClient_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

public static class FileSystem
{
    public static IFileSystem iface = (IFileSystem)null;
    public static Dictionary<string, UnityEngine.Object> cache = new Dictionary<string, UnityEngine.Object>();

    public static GameObject[] LoadPrefabs(string folder)
    {
        if (!folder.EndsWith("/"))
            Debug.LogWarning((object)("FileSystem.LoadPrefabs - folder should end in '/' - " + folder));
        if (!folder.StartsWith("assets/"))
            Debug.LogWarning((object)("FileSystem.LoadPrefabs - should start with assets/ - " + folder));
        return FileSystem.LoadAll<GameObject>(folder, ".prefab");
    }

    public static GameObject LoadPrefab(string filePath)
    {
        if (!filePath.StartsWith("assets/", StringComparison.CurrentCultureIgnoreCase))
            Debug.LogWarning((object)("FileSystem.LoadPrefab - should start with assets/ - " + filePath));
        return FileSystem.Load<GameObject>(filePath, true);
    }

    public static string[] FindAll(string folder, string search = "")
    {
        return FileSystem.iface.FindAll(folder, search);
    }

    public static T[] LoadAll<T>(string folder, string search = "") where T : UnityEngine.Object
    {
        List<T> objList = new List<T>();
        foreach (string filePath in FileSystem.FindAll(folder, search))
        {
            T obj = FileSystem.Load<T>(filePath, true);
            if ((UnityEngine.Object)obj != (UnityEngine.Object)null)
                objList.Add(obj);
        }
        return objList.ToArray();
    }

    public static T Load<T>(string filePath, bool bComplain = true) where T : UnityEngine.Object
    {
        filePath = filePath.ToLower();
        T obj1 = (T)null;
        T obj2;
        if (FileSystem.cache.ContainsKey(filePath))
        {
            obj2 = FileSystem.cache[filePath] as T;
        }
        else
        {
            
            obj2 = FileSystem.iface.Load<T>(filePath, bComplain);
            if ((UnityEngine.Object)obj2 != (UnityEngine.Object)null)
                FileSystem.cache.Add(filePath, (UnityEngine.Object)obj2);
        }
        return obj2;
    }

    public static FileSystem.Operation LoadAsync(string filePath)
    {
        filePath = filePath.ToLower();
        return !FileSystem.cache.ContainsKey(filePath) ? new FileSystem.Operation(filePath, FileSystem.iface.LoadAsync(filePath)) : new FileSystem.Operation(filePath, (AsyncOperation)null);
    }

    public struct Operation
    {
        private string path;
        private AsyncOperation request;

        public Operation(string path, AsyncOperation request)
        {
            this.path = path;
            this.request = request;
        }

        public bool isDone
        {
            get
            {
                if (this.request != null)
                    return this.request.isDone;
                return true;
            }
        }

        public float progress
        {
            get
            {
                if (this.request != null)
                    return this.request.progress;
                return 1f;
            }
        }

        public static implicit operator AsyncOperation(FileSystem.Operation op)
        {
            return op.request;
        }

        public T Load<T>() where T : UnityEngine.Object
        {
            T obj = (T)null;
            if (!this.isDone)
                return obj;
            if (FileSystem.cache.ContainsKey(this.path))
            {
                obj = FileSystem.cache[this.path] as T;
            }
            else
            {
                AssetBundleRequest request = this.request as AssetBundleRequest;
                if (request != null && request.asset != (UnityEngine.Object)null)
                {
                    FileSystem.cache.Add(this.path, request.asset);
                    obj = request.asset as T;
                }
            }
            return obj;
        }
    }
}
