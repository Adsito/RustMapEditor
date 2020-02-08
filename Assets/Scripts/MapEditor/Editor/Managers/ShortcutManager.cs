using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using RustMapEditor.UI;

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

}