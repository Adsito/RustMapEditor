using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Crosstales.FB;

public class MainMenu : MonoBehaviour
{
    MapIO mapIO;
    void Start()
    {
        mapIO = GameObject.FindGameObjectWithTag("MapIO").GetComponent<MapIO>();
    }
    public void LoadMap()
    {
        string loadFile = "";
        loadFile = FileBrowser.OpenSingleFile("Import Map File", loadFile, "map");
        var blob = new WorldSerialization();
        if (loadFile == "")
        {
            return;
        }
        blob.Load(loadFile);
        mapIO.loadPath = loadFile;
        mapIO.Load(blob);
    }
    public void Quit()
    {
        Application.Quit();
    }
}
