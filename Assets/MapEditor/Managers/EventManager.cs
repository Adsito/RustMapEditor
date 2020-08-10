using System;

public static class EventManager
{
    public delegate void MapManagerCallback(string mapName = "");

    /// <summary>Called after a map has been loaded. Calls on both map loaded and map created.</summary>
    public static event MapManagerCallback MapLoaded;

    public static event MapManagerCallback MapSaved;

    public static void OnMapLoaded(string mapName = "") => MapLoaded?.Invoke(mapName);
    public static void OnMapSaved(string mapName = "") => MapSaved?.Invoke(mapName);
}