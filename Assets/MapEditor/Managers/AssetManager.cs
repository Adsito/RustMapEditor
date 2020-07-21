using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class AssetManager
{
	public static string BundlePath { get; private set; }

	public static GameManifest Manifest { get; private set; }

	public const string ManifestPath = "assets/manifest.asset";
	public const string AssetDumpPath = "AssetDump.txt";
	public const string MaterialsListPath = "MaterialsList.txt";

	public static AssetBundleManifest AssetManifest { get; private set; }

	public static Dictionary<uint, string> IDLookup { get; private set; } = new Dictionary<uint, string>();
	public static Dictionary<string, uint> PathLookup { get; private set; } = new Dictionary<string, uint>();

	public static Dictionary<string, AssetBundle> Bundles { get; private set; } = new Dictionary<string, AssetBundle>(System.StringComparer.OrdinalIgnoreCase);
	public static Dictionary<string, AssetBundle> AssetPaths { get; private set; } = new Dictionary<string, AssetBundle>(System.StringComparer.OrdinalIgnoreCase);
	public static Dictionary<string, Object> Cache { get; private set; } = new Dictionary<string, Object>();

	public static List<string> ManifestStrings { get => IsInitialised ? GetManifestStrings() : new List<string>(); private set => ManifestStrings = value; }

	public static bool IsInitialised { get; private set; }

	/// <summary>Loads the prefabs from the Rust prefab bundle.</summary>
	/// <param name="bundlesRoot">The file path to the Rust bundles file.</param>
	public static void Initialise(string bundlesRoot)
	{
		if (!IsInitialised)
		{
			if (!Directory.Exists(SettingsManager.RustDirectory))
            {
				Debug.LogError("Directory does not exist: " + bundlesRoot);
				return;
			}

			if (!SettingsManager.RustDirectory.EndsWith("Rust"))
            {
				Debug.LogError("Not a valid Rust install directory: " + SettingsManager.RustDirectory);
				return;
            }

			ProgressBarManager.Display("Loading Bundles", "Loading Root Bundle", 0.1f);
			BundlePath = bundlesRoot;
			var rootBundle = AssetBundle.LoadFromFile(bundlesRoot);
			if (rootBundle == null)
			{
				Debug.LogError("Couldn't load root AssetBundle - " + bundlesRoot);
				return;
			}

			var manifestList = rootBundle.LoadAllAssets<AssetBundleManifest>();
			if (manifestList.Length != 1)
			{
				Debug.LogError("Couldn't find AssetBundleManifest - " + manifestList.Length);
				return;
			}
			AssetManifest = manifestList[0];

			var bundles = AssetManifest.GetAllAssetBundles();
			ProgressBarManager.SetProgressIncrement(0.9f / bundles.Length);
			for (int i = 0; i < bundles.Length; i++)
			{
				ProgressBarManager.DisplayIncremental("Loading Bundles", "Loading: " + bundles[i]);
				var bundlePath = Path.GetDirectoryName(BundlePath) + Path.DirectorySeparatorChar + bundles[i];
				var asset = AssetBundle.LoadFromFile(bundlePath);
				if (asset == null)
				{
					Debug.LogError("Couldn't load AssetBundle - " + bundlePath);
					return;
				}
				Bundles.Add(bundles[i], asset);
				foreach (var filename in asset.GetAllAssetNames())
					AssetPaths.Add(filename, asset);
                foreach (var filename in asset.GetAllScenePaths())
					AssetPaths.Add(filename, asset);
			}

			Manifest = GetAsset<GameManifest>(ManifestPath);
			if (Manifest == null)
			{
				Debug.LogError("Couldn't load GameManifest.");
				Dispose();
				return;
			}

			for (uint index = 0; index < Manifest.pooledStrings.Length; ++index)
			{
				IDLookup.Add(Manifest.pooledStrings[index].hash, Manifest.pooledStrings[index].str);
				PathLookup.Add(Manifest.pooledStrings[index].str, Manifest.pooledStrings[index].hash);
			}

			AssetDump();
			rootBundle.Unload(true);
			FixMaterials();
			IsInitialised = true;
			
			ProgressBarManager.Clear();
			PrefabManager.ReplaceWithLoaded(PrefabManager.PrefabParent.GetComponentsInChildren<PrefabDataHolder>());
		}
		else
			Debug.Log("Bundle already loaded.");
	}

	private static T GetAsset<T>(string filePath) where T : Object
	{
		if (!AssetPaths.TryGetValue(filePath, out AssetBundle bundle))
			return null;

		return bundle.LoadAsset<T>(filePath);
	}

	public static T LoadAsset<T>(string filePath) where T : Object
	{
		var asset = default(T);

		if (Cache.ContainsKey(filePath))
			asset = Cache[filePath] as T;
		else
		{
			asset = GetAsset<T>(filePath);
			if (asset != null)
				Cache.Add(filePath, asset);
		}
		return asset;
	}

	public static GameObject LoadPrefab(string filePath)
	{
		if (Cache.ContainsKey(filePath))
			return Cache[filePath] as GameObject;

		else
		{
			GameObject val = GetAsset<GameObject>(filePath);
			if (val != null)
			{
				PrefabManager.Process(val, filePath);
				Cache.Add(filePath, val);
				return val;
			}
			Debug.LogWarning("Prefab not loaded from bundle: " + filePath);
			return PrefabManager.DefaultPrefab;
		}
	}

	public static void Dispose()
	{
		PrefabManager.ReplaceWithDefault(PrefabManager.PrefabParent.GetComponentsInChildren<PrefabDataHolder>());
		foreach (var item in Bundles)
			item.Value.Unload(true);

		AssetPaths.Clear();
		Bundles.Clear();
		Cache.Clear();
		IDLookup.Clear();
		PathLookup.Clear();
		IsInitialised = false;
	}

	public static List<string> GetManifestStrings()
	{
		if (Manifest == null)
			return null;

		List<string> manifestStrings = new List<string>();
		foreach (var item in Manifest.pooledStrings)
			manifestStrings.Add(item.str);

		return manifestStrings;
	}

	/// <summary>Dumps every asset found in the Rust content bundle to a text file.</summary>
	public static void AssetDump()
	{
		using (StreamWriter streamWriter = new StreamWriter(AssetDumpPath, false))
			foreach (var item in AssetPaths.Keys)
				streamWriter.WriteLine(item + " : " + ToID(item));
	}

	public static string ToPath(uint i)
	{
		if ((int)i == 0)
			return i.ToString();
		if (IDLookup.TryGetValue(i, out string str))
			return str;
		return i.ToString();
	}

	public static uint ToID(string str)
	{
		if (string.IsNullOrEmpty(str))
			return 0;
		if (PathLookup.TryGetValue(str, out uint num))
			return num;
		return 0;
	}

	/// <summary>Sets materials set in the MaterialsList.txt to the shader supplied. Used for certain prefabs which appear black due to the Rust shaders not having
	/// all of the dependencies needed to function properly in the project.</summary>
	public static void FixMaterials()
	{
		Shader std = Shader.Find("Standard");
		Shader spc = Shader.Find("Standard (Specular setup)");

		if (File.Exists(MaterialsListPath))
        {
            foreach (var item in File.ReadAllLines(MaterialsListPath))
            {
				var lineSplit = item.Split(':');
				lineSplit[0] = lineSplit[0].Trim(' '); // Shader Name
				lineSplit[1] = lineSplit[1].Trim(' '); // Material Path
				switch (lineSplit[0])
				{
					case "Standard":
						UpdateShader(LoadAsset<Material>(lineSplit[1]), std);
						break;
					case "Specular":
						UpdateShader(LoadAsset<Material>(lineSplit[1]), spc);
						break;
				}
			}
		}
		LoadAsset<Material>(@"assets/content/nature/overgrowth/models/materials/overgrowth.mat").DisableKeyword("_TINTENABLED_ON"); // Fix for overgrowth materials.
	}

	/// <summary>Updates the material with the new shaders properties.</summary>
	private static void UpdateShader(Material mat, Shader shader)
    {
		mat.shader = shader;
		switch (mat.GetFloat("_Mode"))
		{
			case 0f:
				mat.SetOverrideTag("RenderType", "");
				mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
				mat.SetInt("_ZWrite", 1);
				mat.DisableKeyword("_ALPHATEST_ON");
				mat.DisableKeyword("_ALPHABLEND_ON");
				mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
				mat.renderQueue = -1;
				break;
			case 1f:
				mat.SetOverrideTag("RenderType", "TransparentCutout");
				mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
				mat.SetInt("_ZWrite", 1);
				mat.EnableKeyword("_ALPHATEST_ON");
				mat.DisableKeyword("_ALPHABLEND_ON");
				mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
				mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
				break;
			case 2f:
				mat.SetOverrideTag("RenderType", "Transparent");
				mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
				mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				mat.SetInt("_ZWrite", 0);
				mat.DisableKeyword("_ALPHATEST_ON");
				mat.EnableKeyword("_ALPHABLEND_ON");
				mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
				mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
				break;
			case 3f:
				mat.SetOverrideTag("RenderType", "Transparent");
				mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				mat.SetInt("_ZWrite", 0);
				mat.DisableKeyword("_ALPHATEST_ON");
				mat.DisableKeyword("_ALPHABLEND_ON");
				mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
				mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
				break;
		}
    }
}