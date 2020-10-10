using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using LZ4;
using ProtoBuf;
using UnityEngine;

[ProtoContract, Serializable]
public class CustomPrefab
{
    [ProtoMember(1)] public string Name;
    [ProtoMember(2)] public string Path;
    [ProtoMember(3)] public string Author;
    [ProtoMember(4)] public string Hash;
    [ProtoMember(5)] public int Version;
    [ProtoMember(6)] public List<PrefabData> Prefabs;

    [ProtoContract]
    public class PrefabData
    {
        [ProtoMember(1)] public WorldSerialization.PrefabData Prefab;
        [ProtoMember(2)] public List<PrefabData> Children;
    }

    public static void Create(PrefabDataHolder[] prefabs)
    {
        if (prefabs.Length == 0)
            return;

        var obj = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/CustomPrefab"), PrefabManager.PrefabParent, false);
        obj.transform.position = prefabs[0].transform.position;
        var holder = obj.GetComponent<CustomPrefabHolder>();
        holder.Setup(prefabs);
        foreach (var item in prefabs)
            item.transform.SetParent(obj.transform);
    }

    public static void Load()
    {
        foreach (var item in Directory.GetFiles("Prefabs", "*.prefab", SearchOption.AllDirectories))
        {
            try
            {
                using (var fileStream = new FileStream(item, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (var compressionStream = new LZ4Stream(fileStream, LZ4StreamMode.Decompress))
                    {
                        var prefab = Serializer.Deserialize<CustomPrefab>(compressionStream);
                        if (!PrefabManager.CustomPrefabs.ContainsKey(item))
                            PrefabManager.CustomPrefabs.Add(item, prefab);
                    }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
    }

    public static void Save(CustomPrefab prefab)
    {
        try
        {
            prefab.Hash = GenerateHash(prefab);
            using (var fileStream = new FileStream(prefab.Path, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var compressionStream = new LZ4Stream(fileStream, LZ4StreamMode.Compress))
                    Serializer.Serialize(compressionStream, prefab);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    public static string GenerateHash(CustomPrefab prefab)
    {
        var checksum = new CheckSum();
		foreach (var item in prefab.Prefabs)
            HashPrefab(item, checksum);

        return checksum.MD5();
    }

	private static void HashPrefab(PrefabData prefab, CheckSum checkSum)
    {
        checkSum.Add(prefab.Prefab.id);
		checkSum.Add(prefab.Prefab.position.x, 3);
		checkSum.Add(prefab.Prefab.position.y, 3);
		checkSum.Add(prefab.Prefab.position.z, 3);
		checkSum.Add(prefab.Prefab.scale.x, 3);
		checkSum.Add(prefab.Prefab.scale.y, 3);
		checkSum.Add(prefab.Prefab.scale.z, 3);
		checkSum.Add(prefab.Prefab.rotation.x, 3);
		checkSum.Add(prefab.Prefab.rotation.y, 3);
		checkSum.Add(prefab.Prefab.rotation.z, 3);
		foreach (var item in prefab.Children)
			HashPrefab(item, checkSum);
    }

    private class CheckSum
    {
        private List<byte> values = new List<byte>();

        public void Add(float f, int bytes)
        {
            var v = new Union32();
            v.f = f;
            if (bytes >= 4) values.Add(v.b1);
            if (bytes >= 3) values.Add(v.b2);
            if (bytes >= 2) values.Add(v.b3);
            if (bytes >= 1) values.Add(v.b4);
        }

        public void Add(uint u)
        {
            var v = new Union32();
            v.u = u;
            values.Add(v.b1);
            values.Add(v.b2);
            values.Add(v.b3);
            values.Add(v.b4);
        }

        public string MD5()
        {
            var hashFunc = new MD5CryptoServiceProvider();
            var hashBytes = hashFunc.ComputeHash(values.ToArray());
            return BytesToString(hashBytes);
        }

        private string BytesToString(byte[] bytes)
        {
            var sb = new StringBuilder();

            for (int x = 0; x < bytes.Length; x++)
                sb.Append(bytes[x].ToString("x2"));

            return sb.ToString();
        }
    }
}