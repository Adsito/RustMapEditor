using UnityEngine;
using XNode;

[CreateNodeMenu("Functions/HeightMap/Normalise HeightMap")]
public class NormaliseHeightMapNode : Node
{
    [Input(ShowBackingValue.Never, ConnectionType.Override)] public NodeVariables.NextTask PreviousTask;
    [Output(ShowBackingValue.Never, ConnectionType.Override)] public NodeVariables.NextTask NextTask;
    [HideInInspector] public float normaliseLow = 450f, normaliseHigh = 1000f, normaliseBlend = 1f;
    public override object GetValue(NodePort port)
    {
        return null;
    }
    public void RunNode()
    {
        MapIO mapIO = GameObject.FindGameObjectWithTag("MapIO").GetComponent<MapIO>();
        mapIO.NormaliseHeightmap(normaliseLow / 1000f, normaliseHigh / 1000f, normaliseBlend);
    }
}
