// Decompiled with JetBrains decompiler
// Type: IFileSystem
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FFBD04B0-3818-45ED-99BC-2B8616F3C2C4
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Rust\RustClient_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public interface IFileSystem
{
    T Load<T>(string filePath, bool bComplain = true) where T : Object;

    string[] FindAll(string folder, string search);

    AsyncOperation LoadAsync(string filePath);
}
