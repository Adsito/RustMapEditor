using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class AssetBundleBackend : FileSystemBackend, System.IDisposable
{
	private AssetBundle rootBundle;
	private AssetBundleManifest manifest;
	private Dictionary<string, AssetBundle> bundles = new Dictionary<string, AssetBundle>(System.StringComparer.OrdinalIgnoreCase);
	private Dictionary<string, AssetBundle> files = new Dictionary<string, AssetBundle>(System.StringComparer.OrdinalIgnoreCase);
	private string assetPath;

	public AssetBundleBackend(string assetRoot)
	{
        
		isError = false;
		assetPath = Path.GetDirectoryName(assetRoot) + Path.DirectorySeparatorChar.ToString();

		rootBundle = AssetBundle.LoadFromFile(assetRoot);
		if (rootBundle == null)
		{
			LoadError("Couldn't load root AssetBundle - " + assetRoot);
			return;
		}

		var manifestList = rootBundle.LoadAllAssets<AssetBundleManifest>();
        Debug.Log(manifestList.Length);
		if (manifestList.Length != 1)
		{
			LoadError("Couldn't find AssetBundleManifest - " + manifestList.Length);
			return;
		}

		manifest = manifestList[0];

		foreach (var ab in manifest.GetAllAssetBundles())
		{
			LoadBundle(ab);
            Debug.Log("Getting asset bundles");
			if (isError) return;
		}

		BuildFileIndex();
	}

	private void LoadBundle(string bundleName)
	{
		if (bundles.ContainsKey(bundleName)) return;

		var fileLocation = assetPath + bundleName;
		var asset = AssetBundle.LoadFromFile(fileLocation);

		if (asset == null)
		{
			LoadError("Couldn't load AssetBundle - " + fileLocation);
			return;
		}

		bundles.Add(bundleName, asset);
	}

	private void BuildFileIndex()
	{
		files.Clear();

		foreach (var bundle in bundles)
		{
            Debug.Log("Build file index");
			if (bundle.Key.StartsWith("content", System.StringComparison.InvariantCultureIgnoreCase))
				continue;

			foreach (var filename in bundle.Value.GetAllAssetNames())
			{
				files.Add(filename, bundle.Value);
			}
		}
	}

	public void Dispose()
	{
		manifest = null;

		foreach (var bundle in bundles)
		{
			bundle.Value.Unload(false);
			Object.DestroyImmediate(bundle.Value);
		}
		bundles.Clear();

		if (rootBundle)
		{
			rootBundle.Unload(false);
			Object.DestroyImmediate(rootBundle);
			rootBundle = null;
		}
	}

	protected override T LoadAsset<T>(string filePath)
	{
		AssetBundle bundle = null;
        
		if (!files.TryGetValue(filePath, out bundle))
		{
			return null;
		}

		return bundle.LoadAsset<T>(filePath);
	}

	protected override string[] LoadAssetList(string folder, string search)
	{
		var list = new List<string>();

		foreach (var file in files.Where(x => x.Key.StartsWith(folder, System.StringComparison.InvariantCultureIgnoreCase)))
		{
			if (!string.IsNullOrEmpty(search) && file.Key.IndexOf(search, System.StringComparison.InvariantCultureIgnoreCase) == -1)
				continue;

			list.Add(file.Key);
		}

		return list.ToArray();
	}
}
