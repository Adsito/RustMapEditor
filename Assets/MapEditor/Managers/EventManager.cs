using System;

public static class EventManager
{
    public delegate void AssetManagerCallback();

    /// <summary>Called after Rust Asset Bundles are loaded into the editor. </summary>
    public static event AssetManagerCallback BundlesLoaded;

    /// <summary>Called after Rust Asset Bundles are unloaded from the editor. </summary>
    public static event AssetManagerCallback BundlesDisposed;

    public static void OnBundlesLoaded() => BundlesLoaded?.Invoke();

    public static void OnBundlesDisposed() => BundlesDisposed?.Invoke();
}