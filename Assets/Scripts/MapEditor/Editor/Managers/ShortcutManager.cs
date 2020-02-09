using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using RustMapEditor.UI;
using RustMapEditor.Data;

public static class ShortcutManager
{
    [Shortcut("RustMapEditor/Load Map")]
    public static void LoadMap()
    {
        Functions.LoadMapPanel();
    }

    [Shortcut("RustMapEditor/Save Map")]
    public static void SaveMap()
    {
        Functions.SaveMapPanel();
    }

    [Shortcut("RustMapEditor/New Map")]
    public static void NewMap()
    {
        Functions.NewMapPanel();
    }

    [Shortcut("RustMapEditor/Clear Progress Bar")]
    public static void ClearProgressBar()
    {
        MapIO.ClearProgressBar();
    }

    [Shortcut("RustMapEditor/Clear Layer")]
    public static void ClearLayer()
    {
        MapIO.ClearLayer(LandData.landLayer, TerrainTopology.TypeToIndex((int)LandData.topologyLayer));
    }
}