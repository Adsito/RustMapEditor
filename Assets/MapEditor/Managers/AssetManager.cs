using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

public static class AssetManager
{
	public static class Callbacks
    {
		public delegate void AssetManagerCallback();

		/// <summary>Called after Rust Asset Bundles are loaded into the editor. </summary>
		public static event AssetManagerCallback BundlesLoaded;

		/// <summary>Called after Rust Asset Bundles are unloaded from the editor. </summary>
		public static event AssetManagerCallback BundlesDisposed;

		public static void OnBundlesLoaded() => BundlesLoaded?.Invoke();
		public static void OnBundlesDisposed() => BundlesDisposed?.Invoke();
	}

	public static string BundlePath { get; private set; }

	public static GameManifest Manifest { get; private set; }

	public const string ManifestPath = "assets/manifest.asset";
	public const string AssetDumpPath = "AssetDump.txt";
	public const string MaterialsListPath = "MaterialsList.txt";

	public static Dictionary<uint, string> IDLookup { get; private set; } = new Dictionary<uint, string>();
	public static Dictionary<string, uint> PathLookup { get; private set; } = new Dictionary<string, uint>();
	public static Dictionary<string, AssetBundle> BundleLookup { get; private set; } = new Dictionary<string, AssetBundle>();

	public static Dictionary<string, AssetBundle> BundleCache { get; private set; } = new Dictionary<string, AssetBundle>(System.StringComparer.OrdinalIgnoreCase);
	public static Dictionary<string, Object> AssetCache { get; private set; } = new Dictionary<string, Object>();
	public static Dictionary<string, Texture2D> PreviewCache { get; private set; } = new Dictionary<string, Texture2D>();

	public static List<string> AssetPaths { get; private set; } = new List<string>();

	public static bool IsInitialised { get; private set; }

	/// <summary>Loads the prefabs from the Rust prefab bundle.</summary>
	/// <param name="bundlesRoot">The file path to the Rust bundles file.</param>
	public static void Initialise(string bundlesRoot)
	{
		if (!Coroutines.IsInitialising && !IsInitialised)
			EditorCoroutineUtility.StartCoroutineOwnerless(Coroutines.Initialise(bundlesRoot));
		if (IsInitialised)
			Debug.Log("Bundles already loaded.");
	}

	public static void Dispose()
	{
		if (!Coroutines.IsInitialising && IsInitialised)
			EditorCoroutineUtility.StartCoroutineOwnerless(Coroutines.Dispose());
	}

	private static T GetAsset<T>(string filePath) where T : Object
	{
		if (!BundleLookup.TryGetValue(filePath, out AssetBundle bundle))
			return null;

		return bundle.LoadAsset<T>(filePath);
	}

    public static T LoadAsset<T>(string filePath) where T : Object
	{
		var asset = default(T);

		if (AssetCache.ContainsKey(filePath))
			asset = AssetCache[filePath] as T;
		else
		{
			asset = GetAsset<T>(filePath);
			if (asset != null)
				AssetCache.Add(filePath, asset);
		}
		return asset;
	}

	public static GameObject LoadPrefab(string filePath)
    {
        if (AssetCache.ContainsKey(filePath))
            return AssetCache[filePath] as GameObject;

        else
        {
            GameObject val = GetAsset<GameObject>(filePath);
            if (val != null)
            {
				PrefabManager.Setup(val, filePath);
				AssetCache.Add(filePath, val);
				PrefabManager.Callbacks.OnPrefabLoaded(val);
				return val;
            }
            Debug.LogWarning("Prefab not loaded from bundle: " + filePath);
            return PrefabManager.DefaultPrefab;
        }
    }

	public static Texture2D GetPreview(string filePath)
    {
		if (PreviewCache.TryGetValue(filePath, out Texture2D preview))
			return preview;
        else
        {
			var prefab = LoadPrefab(filePath);
			if (prefab.name == "DefaultPrefab")
				return AssetPreview.GetAssetPreview(prefab);

			prefab.SetActive(true);
			var tex = AssetPreview.GetAssetPreview(prefab) ?? new Texture2D(60, 60);
			PreviewCache.Add(filePath, tex);
			prefab.SetActive(false);
			return tex;
        }
    }

	/// <summary>Dumps every asset found in the Rust content bundle to a text file.</summary>
	public static void AssetDump()
	{
		using (StreamWriter streamWriter = new StreamWriter(AssetDumpPath, false))
			foreach (var item in BundleLookup.Keys)
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

	private static class Coroutines
    {
		public static bool IsInitialising { get; private set; }

		public static IEnumerator Initialise(string bundlesRoot)
		{
			IsInitialising = true;
			ProgressManager.RemoveProgressBars("Asset Bundles");

			int progressID = Progress.Start("Load Asset Bundles", null, Progress.Options.Sticky);
			int bundleID = Progress.Start("Bundles", null, Progress.Options.Sticky, progressID);
			int materialID = Progress.Start("Materials", null, Progress.Options.Sticky, progressID);
			int prefabID = Progress.Start("Replace Prefabs", null, Progress.Options.Sticky, progressID);
			Progress.Report(bundleID, 0f);
			Progress.Report(materialID, 0f);
			Progress.Report(prefabID, 0f);

			yield return EditorCoroutineUtility.StartCoroutineOwnerless(LoadBundles(bundlesRoot, (progressID, bundleID, materialID)));
			if (!IsInitialising)
            {
				Progress.Finish(bundleID, Progress.Status.Failed);
				Progress.Finish(materialID, Progress.Status.Failed);
				Progress.Finish(prefabID, Progress.Status.Failed);
				yield break;
			}
			yield return EditorCoroutineUtility.StartCoroutineOwnerless(SetBundleReferences((progressID, bundleID)));
			yield return EditorCoroutineUtility.StartCoroutineOwnerless(SetMaterials(materialID));

			IsInitialised = true; IsInitialising = false;
			Callbacks.OnBundlesLoaded();
			PrefabManager.ReplaceWithLoaded(PrefabManager.CurrentMapPrefabs, prefabID);
		}

		public static IEnumerator Dispose() 
		{
			IsInitialising = true;
			ProgressManager.RemoveProgressBars("Unload Asset Bundles");

			int progressID = Progress.Start("Unload Asset Bundles", null, Progress.Options.Sticky);
			int bundleID = Progress.Start("Bundles", null, Progress.Options.Sticky, progressID);
			int prefabID = Progress.Start("Prefabs", null, Progress.Options.Sticky, progressID);
			Progress.Report(bundleID, 0f);
			Progress.Report(prefabID, 0f);
			PrefabManager.ReplaceWithDefault(PrefabManager.CurrentMapPrefabs, prefabID);

			while (PrefabManager.IsChangingPrefabs)
				yield return null;

			for (int i = 0; i < BundleCache.Count; i++)
            {
				Progress.Report(bundleID, (float)i / BundleCache.Count, "Unloading: " + BundleCache.ElementAt(i).Key);
				BundleCache.ElementAt(i).Value.Unload(true);
				yield return null;
            }
			
			int bundleCount = BundleCache.Count;
			BundleLookup.Clear();
			BundleCache.Clear();
			AssetCache.Clear();

			Progress.Report(bundleID, 0.99f, "Unloaded: " + bundleCount + " bundles.");
			Progress.Finish(bundleID, Progress.Status.Succeeded);
			IsInitialised = false; IsInitialising = false;
		}

		public static IEnumerator LoadBundles(string bundleRoot, (int progress, int bundle, int material) ID)
        {
			if (!Directory.Exists(SettingsManager.RustDirectory))
			{
				Debug.LogError("Directory does not exist: " + bundleRoot);
				IsInitialising = false;
				yield break;
			}

			if (!SettingsManager.RustDirectory.EndsWith("Rust") && !SettingsManager.RustDirectory.EndsWith("RustStaging"))
			{
				Debug.LogError("Not a valid Rust install directory: " + SettingsManager.RustDirectory);
				IsInitialising = false;
				yield break;
			}

			var rootBundle = AssetBundle.LoadFromFile(bundleRoot);
			if (rootBundle == null)
			{
				Debug.LogError("Couldn't load root AssetBundle - " + bundleRoot);
				IsInitialising = false;
				yield break;
			}

			var manifestList = rootBundle.LoadAllAssets<AssetBundleManifest>();
			if (manifestList.Length != 1)
			{
				Debug.LogError("Couldn't find AssetBundleManifest - " + manifestList.Length);
				IsInitialising = false;
				yield break;
			}

			var assetManifest = manifestList[0];
			var bundles = assetManifest.GetAllAssetBundles();

			for (int i = 0; i < bundles.Length; i++)
			{
				Progress.Report(ID.bundle, (float)i / bundles.Length, "Loading: " + bundles[i]);
				var bundlePath = Path.GetDirectoryName(bundleRoot) + Path.DirectorySeparatorChar + bundles[i];

				var asset = AssetBundle.LoadFromFileAsync(bundlePath);
				while (!asset.isDone)
					yield return null;

				if (asset == null)
				{
					Debug.LogError("Couldn't load AssetBundle - " + bundlePath);
					IsInitialising = false;
					yield break;
				}
				BundleCache.Add(bundles[i], asset.assetBundle);
			}
			rootBundle.Unload(true);
		}

		public static IEnumerator SetBundleReferences((int parent, int bundle) ID)
		{
			var sw = new System.Diagnostics.Stopwatch();
			sw.Start();

			foreach (var asset in BundleCache.Values)
			{
				foreach (var filename in asset.GetAllAssetNames())
				{
					BundleLookup.Add(filename, asset);
					if (sw.Elapsed.TotalMilliseconds >= 0.5f)
					{
						yield return null;
						sw.Restart();    
					}
				}
				foreach (var filename in asset.GetAllScenePaths())
				{
					BundleLookup.Add(filename, asset);
					if (sw.Elapsed.TotalMilliseconds >= 0.5f)
					{
						yield return null;
						sw.Restart();
					}
				}
			}

			Progress.Report(ID.bundle, 0.99f, "Loaded " + BundleCache.Count + " bundles.");
			Progress.Finish(ID.bundle, Progress.Status.Succeeded);

			Manifest = GetAsset<GameManifest>(ManifestPath);
			if (Manifest == null)
			{
				Debug.LogError("Couldn't load GameManifest.");
				Dispose();
				Progress.Finish(ID.parent, Progress.Status.Failed);
				yield break;
			}

			var setLookups = Task.Run(() =>
			{
				for (uint i = 0; i < Manifest.pooledStrings.Length; ++i)
				{
					IDLookup.Add(Manifest.pooledStrings[i].hash, Manifest.pooledStrings[i].str);
					PathLookup.Add(Manifest.pooledStrings[i].str, Manifest.pooledStrings[i].hash);
					if (ToID(Manifest.pooledStrings[i].str) != 0)
						AssetPaths.Add(Manifest.pooledStrings[i].str);
				}
				AssetDump();
			});
			while (!setLookups.IsCompleted)
            {
				if (sw.Elapsed.TotalMilliseconds >= 0.1f)
                {
					yield return null;
					sw.Restart();
				}
			}
		}

		public static IEnumerator SetMaterials(int materialID)
		{
			if (File.Exists(MaterialsListPath))
			{
				Shader std = Shader.Find("Standard");
				Shader spc = Shader.Find("Standard (Specular setup)");
				string[] materials = File.ReadAllLines(MaterialsListPath);
				for (int i = 0; i < materials.Length; i++)
				{
					var lineSplit = materials[i].Split(':');
					lineSplit[0] = lineSplit[0].Trim(' '); // Shader Name
					lineSplit[1] = lineSplit[1].Trim(' '); // Material Path
					Progress.Report(materialID, (float)i / materials.Length, "Setting: " + lineSplit[1]);
					switch (lineSplit[0])
					{
						case "Standard":
							EditorCoroutineUtility.StartCoroutineOwnerless(UpdateShader(LoadAsset<Material>(lineSplit[1]), std));
							break;
						case "Specular":
							EditorCoroutineUtility.StartCoroutineOwnerless(UpdateShader(LoadAsset<Material>(lineSplit[1]), spc));
							break;
						case "Foliage":
							LoadAsset<Material>(lineSplit[1]).DisableKeyword("_TINTENABLED_ON");
							break;
					}
					yield return null;
				}
				Progress.Report(materialID, 0.99f, "Set " + materials.Length + " materials.");
				Progress.Finish(materialID, Progress.Status.Succeeded);
			}
		}

		public static IEnumerator UpdateShader(Material mat, Shader shader)
		{
			mat.shader = shader;
			yield return null;
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
					SetKeyword(mat, "_NORMALMAP", mat.GetTexture("_BumpMap") || mat.GetTexture("_DetailNormalMap"));
					if (mat.HasProperty("_SPECGLOSSMAP"))
						SetKeyword(mat, "_SPECGLOSSMAP", mat.GetTexture("_SpecGlossMap"));
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
					mat.EnableKeyword("_NORMALMAP");
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
					mat.EnableKeyword("_NORMALMAP");
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
					mat.EnableKeyword("_NORMALMAP");
					mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
					break;
			}
		}

		static void SetKeyword(Material mat, string keyword, bool state)
		{
			if (state)
				mat.EnableKeyword(keyword);
			else
				mat.DisableKeyword(keyword);
		}
	}
}