using System;
using System.Collections.Generic;
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
}