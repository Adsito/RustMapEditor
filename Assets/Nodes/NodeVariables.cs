using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace NodeVariables
{
    [System.Serializable]
    public class InputAllTypes
    {

    }
    [System.Serializable]
    public class NextTask
    {

    }
    [System.Serializable]
    public class Texture
    {
        public int LandLayer
        {
            get; set;
        }
        public int TopologyLayer
        {
            get; set;
        }
        public int GroundTexture
        {
            get; set;
        }
        public int BiomeTexture
        {
            get; set;
        }
        public int AlphaTexture
        {
            get; set;
        }
        public int TopologyTexture
        {
            get; set;
        }
        public enum LandLayerEnum
        {
            Ground, Biome, Alpha, Topology
        }
        public enum AlphaEnum
        {
            InActive, Active
        }
        public enum TopologyEnum
        {
            Active, InActive
        }
    }
}
