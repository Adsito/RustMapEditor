using System;
using System.Collections.Generic;

[Serializable]
public class AssetSceneManifest
{
    public List<SceneEntry> Scenes;
}

[Serializable]
public class SceneEntry
{
    public string Name;
    public bool AutoLoad;
    public bool CanUnload;
    public List<string> IncludedAssets;
}
