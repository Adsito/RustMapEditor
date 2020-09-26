using System;
using System.Collections.Generic;
using ProtoBuf;

public class CustomPrefab
{
    [ProtoContract]
    public class Base
    {
        [ProtoMember(1)] public string Name;
        [ProtoMember(2)] public string Path;
        [ProtoMember(3)] public string Author;
        [ProtoMember(4)] public int Hash;
        [ProtoMember(5)] public int Version;
        [ProtoMember(6)] public List<PrefabData> Prefabs;
    }

    [ProtoContract, Serializable]
    public class PrefabData
    {
        [ProtoMember(1)] public WorldSerialization.PrefabData Prefab;
        [ProtoMember(2)] public List<PrefabData> Children;
    }

    public int InstanceID { get; private set; }

    public Base Data { get; set; }
}