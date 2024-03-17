using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapManager
{
    public MapManager()
    {
        _mapPaths.Add(0, "Scenes/Game.unity");
    }

    Dictionary<int, string> _mapPaths = new Dictionary<int, string>(); // Key : 맵 아이디, Value : 맵 경로

	public void LoadMap(int mapId)
	{
        LoadMap(_mapPaths[mapId]);
    }

    public void LoadMap(string mapPath)
    {
		DestroyMap();

        Debug.Log($"Enter Map {mapPath}");
        SceneManager.LoadScene($"Assets/{mapPath}", LoadSceneMode.Single);
    }

    public void DestroyMap()
	{

	}
}
