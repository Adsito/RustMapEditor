// GameManifest
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class GameManifest : ScriptableObject
{
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

		public bool pool;
	}

	[Serializable]
	public class EffectCategory
	{
		[HideInInspector]
		public string folder;

		public List<string> prefabs;
	}

	public PooledString[] pooledStrings;

	public MeshColliderInfo[] meshColliders;

	public PrefabProperties[] prefabProperties;

	public EffectCategory[] effectCategories;

	public string[] skinnables;

	public string[] entities;

	internal static GameManifest loadedManifest;

	internal static Dictionary<string, string> guidToPath = new Dictionary<string, string>();

	internal static Dictionary<string, UnityEngine.Object> guidToObject = new Dictionary<string, UnityEngine.Object>();

	public static GameManifest Current
	{
		get
		{
			if (loadedManifest != null)
			{
				return loadedManifest;
			}
			Load();
			return loadedManifest;
		}
	}

	public static void Load()
	{
		if (!(loadedManifest != null))
		{
			loadedManifest = FileSystem.Load<GameManifest>("Assets/manifest.asset");
			PrefabProperties[] array = loadedManifest.prefabProperties;
			foreach (PrefabProperties prefabProperties in array)
			{
				guidToPath.Add(prefabProperties.guid, prefabProperties.name);
			}
		}
	}
    /*
	public static void LoadAssets()
	{
		if (Skinnable.All == null)
		{
			Skinnable.All = LoadSkinnableAssets();
			DebugEx.Log(GetAssetStatus(), StackTraceLogType.None);
		}
	}

	private static Skinnable[] LoadSkinnableAssets()
	{
		string[] array = loadedManifest.skinnables;
		Skinnable[] array2 = new Skinnable[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array2[i] = FileSystem.Load<Skinnable>(array[i]);
		}
		return array2;
	}*/

	internal static Dictionary<string, string[]> LoadEffectDictionary()
	{
		EffectCategory[] array = loadedManifest.effectCategories;
		Dictionary<string, string[]> dictionary = new Dictionary<string, string[]>();
		EffectCategory[] array2 = array;
		foreach (EffectCategory effectCategory in array2)
		{
			dictionary.Add(effectCategory.folder, effectCategory.prefabs.ToArray());
		}
		return dictionary;
	}

	internal static string GUIDToPath(string guid)
	{
		if (string.IsNullOrEmpty(guid))
		{
			Debug.LogError("GUIDToPath: guid is empty");
			return string.Empty;
		}
		Load();
		if (guidToPath.TryGetValue(guid, out string value))
		{
			return value;
		}
		Debug.LogWarning("GUIDToPath: no path found for guid " + guid);
		return string.Empty;
	}

	internal static UnityEngine.Object GUIDToObject(string guid)
	{
		UnityEngine.Object value = null;
		if (guidToObject.TryGetValue(guid, out value))
		{
			return value;
		}
		string text = GUIDToPath(guid);
		if (string.IsNullOrEmpty(text))
		{
			Debug.LogWarning("Missing file for guid " + guid);
			guidToObject.Add(guid, null);
			return null;
		}
		UnityEngine.Object @object = FileSystem.Load<UnityEngine.Object>(text);
		guidToObject.Add(guid, @object);
		return @object;
	}

	private static string GetMetadataStatus()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (loadedManifest != null)
		{
			stringBuilder.Append("Manifest Metadata Loaded");
			stringBuilder.AppendLine();
			stringBuilder.Append("\t");
			stringBuilder.Append(loadedManifest.pooledStrings.Length.ToString());
			stringBuilder.Append(" pooled strings");
			stringBuilder.AppendLine();
			stringBuilder.Append("\t");
			stringBuilder.Append(loadedManifest.meshColliders.Length.ToString());
			stringBuilder.Append(" mesh colliders");
			stringBuilder.AppendLine();
			stringBuilder.Append("\t");
			stringBuilder.Append(loadedManifest.prefabProperties.Length.ToString());
			stringBuilder.Append(" prefab properties");
			stringBuilder.AppendLine();
			stringBuilder.Append("\t");
			stringBuilder.Append(loadedManifest.effectCategories.Length.ToString());
			stringBuilder.Append(" effect categories");
			stringBuilder.AppendLine();
			stringBuilder.Append("\t");
			stringBuilder.Append(loadedManifest.entities.Length.ToString());
			stringBuilder.Append(" entity names");
			stringBuilder.AppendLine();
			stringBuilder.Append("\t");
			stringBuilder.Append(loadedManifest.skinnables.Length.ToString());
			stringBuilder.Append(" skinnable names");
		}
		else
		{
			stringBuilder.Append("Manifest Metadata Missing");
		}
		return stringBuilder.ToString();
	}

	private static string GetAssetStatus()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (loadedManifest != null)
		{
			stringBuilder.Append("Manifest Assets Loaded");
			stringBuilder.AppendLine();
			stringBuilder.Append("\t");
			//stringBuilder.Append((Skinnable.All != null) ? Skinnable.All.Length.ToString() : "0");
			stringBuilder.Append(" skinnable objects");
		}
		else
		{
			stringBuilder.Append("Manifest Assets Missing");
		}
		return stringBuilder.ToString();
	}
}
