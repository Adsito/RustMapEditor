/*
 * File: IslandGenerator.cs
 * Author: Juuso Tenhunen
 * Date: 24.3.2016
 * Modified: 24.3.2016
 * 
 * Summary:
 * Procedural island generator for Unity3D's Terrain-objects.
 * Requires IslandGeneratorEditor.cs for custom inspector view.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System;

//This adds the script to the component menu for easy access.
[AddComponentMenu("Island Generator/Island Generator")]
public class IslandGenerator : MonoBehaviour {

    public Terrain terrain;

    //Variables for Cellular Automata
    private int smoothTimes = 5;
    private int neighboringWalls = 4;

    //Maximum radius for shores.
    public int maxRadius;
    //This is actually used in calculation. We need second variable,
    //because the first one is managed by GUI and is min 20 and max 100,
    //but the radius can be bigger/smaller depending on resolution.
    //So we need to calculate how much bigger/smaller it is and store it in
    //a separate variable, so we don't mess the original up.
    private int maxRadius2;

    //Temporary Scaled down dimensions of the map.
    private int width;
    private int height;

    //Scaling multiplier for the map
    private int scaleHeightMultip;
    private int scaleWidthMultip;

    private int terrainreso;

    //Temporary ap dimensions for different scaling stages.
    //These and the two above variables are used when the basemap is
    //created using Cellular Automata. Original dimensions needs to be
    //scaled down a bit to get good results
    private int scaledWidth;
    private int scaledHeight;

    public Thread th1;

    //This is used to abort Thread th1.
    public bool aborted = false;

    //Every pixel in the Terrain and how many pixels the Thread has gone through.
    //Used to determine whether or not shore calculation is finished
    private int gridSize;
    private int calculatedPixels;

    //Progress of the shore calculation
    public string prog;

    //Original map dimensions
    private int terrainDataMapHeight;
    private int terrainDataMapWidth;

    //Variables for the Perlin Noise
    public float perlinHeight = 0.02f;
    public float perlinScale = 20.0f;

    public int mapSmoothTimes = 10;

    //Seed of randomization and Boolean if we randomize the seed or not
    public string seed;
    public bool useRandomSeed = true;

    public List<string> usedSeeds = new List<string>(0);

    //Initial map noise fill
    [Range(0, 100)]
    public int randomFillPercent = 50;

    //Heightmap variables
    int[,] map;
    int[,] mapx2;
    float[,] heights;
	float[,] heights2;

    void Awake()
    {
        terrain = GetComponent<Terrain>();
    }

    /// <summary>
    /// Initial function to create the base map
    /// </summary>
    public void StartGeneration()
    {
        terrain = GetComponent<Terrain>();

        int mapHeight = terrain.terrainData.heightmapHeight;
        int mapWidth = terrain.terrainData.heightmapWidth;

        //Let's figure out how many times we need to shrink/grow our map
        //for the cellular automata to work. 64*64 seems to be the best resolution.
        scaleHeightMultip = (int)(mapHeight / 64);
        scaleWidthMultip = (int)(mapWidth / 64);

        height = mapHeight / scaleHeightMultip;
        width = mapWidth / scaleWidthMultip;

        heights = new float[terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight];
        heights2 = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapWidth);

        GenerateMap();
    }

    /// <summary>
    /// Thread function. Shore calculation takes long, so it is handled in a thread to
    /// prevent Unity from freezing.
    /// </summary>
    void Thread1()
    {
        for (int x = 0; x < scaledWidth; x++)
        {
            for (int y = 0; y < scaledHeight; y++)
            {
                //If we get aborted == true from the Custom Inspector
                //we need to break the loops to stop this Thread
                if (aborted)
                    break;
                calculatedPixels++;
                prog = (((float)calculatedPixels / (float)gridSize) * 100.0f).ToString("F0") + "%";
                if (mapx2[x, y] == 2)
                {
                    RandomDrop(x, y);
                }
            }
            if (aborted)
                break;
        }

        //Thread has finished it's loop and we can turn this boolean back to false
        //in the case of it was true
        aborted = false;
    }

    /// <summary>
    /// Function for lowering the heightmap until the lowest point hits 0.
    /// </summary>
    public void ResetSeaFloor()
    {
        terrain = GetComponent<Terrain>();

        heights2 = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapWidth);
        bool hitFloor = false;

        //Even if we hit 0 at some point, the function runs until the end of the map,
        //so there wont be any height differences. E.G. if the lowest point is at the
        //center of the map, it doesn't stop there but goes through the map so every
        //point is equally lowered.
        while (!hitFloor)
        {
            for (int x = 0; x < terrain.terrainData.heightmapWidth; x++)
            {
                for (int y = 0; y < terrain.terrainData.heightmapHeight; y++)
                {
                    heights2[x, y] -= 0.001f;
                    if (heights2[x, y] <= 0)
                        hitFloor = true;
                }
            }
        }

        terrain.terrainData.SetHeights(0, 0, heights2);
    }

    /// <summary>
    /// Basic function to smooth the heightmap. It selects a pixel, calculates the mean
    /// of the pixels around it and applies the mean to the original pixel.
    /// </summary>
    public void BlendHeights()
	{
        terrain = GetComponent<Terrain>();

        int hMapWidth = terrain.terrainData.heightmapWidth;
        int hMapHeight = terrain.terrainData.heightmapHeight;

        heights2 = terrain.terrainData.GetHeights(0, 0, hMapWidth, hMapHeight);

		for (int x = 0; x < hMapWidth; x++)
		{
			for (int y = 0; y < hMapHeight; y++)
			{
				float pointHeight = 0.0f;
				float blendHeight = pointHeight;
				float pixelCount = 0.0f;

				for (int x2 = x-1; x2 < x + 2; x2++)
				{
					for (int y2 = y-1; y2 < y + 2; y2++)
					{
                        if (x2 >= 0 && y2 >= 0 && x2 < hMapWidth && y2 < hMapHeight)
                        {
                            blendHeight = blendHeight + heights2[x2, y2];
                            pixelCount++;
                        }
					}
				}
				blendHeight = blendHeight / pixelCount;
				//print ("point: "+pointHeight+", blend: "+blendHeight);
				//if(blendHeight < 0.1f) heights2[x,y] = blendHeight;
                heights2[x, y] = blendHeight;
			}
		}
		terrain.terrainData.SetHeights(0, 0, heights2);
	}

    /// <summary>
    /// Function that applies Sine wave to the shores to smooth them out.
    /// It uses x and y coordinates to determine how far away from the edges it is,
    /// calculates the wave (twice, so it is sort of like a bump) and applies it
    /// to the heightmap. The x,y position is the top of the bump.
    /// </summary>
    /// <param name="xCoord">X coordinate of the pixel</param>
    /// <param name="yCoord">Y coordinate of the pixel</param>
    void RandomDrop(int xCoord, int yCoord)
    {
        //First be determine the radius, how large the wave will be.
        //The more center in the map it is, the larger the radius (=more gentle shore).
        //Radius can't be bigger than the predefined maximum.
        int radius = terrainDataMapWidth;

        int xDiff = terrainDataMapWidth - xCoord;
        int yDiff = terrainDataMapHeight - yCoord;

        if (xDiff < radius)
            radius = xDiff;

        if (yDiff < radius)
            radius = yDiff;

        if (xCoord < radius)
            radius = xCoord;

        if (yCoord < radius)
            radius = yCoord;

        if (radius > maxRadius2)
            radius = maxRadius2;

        for (int x = 0; x < radius*2; x++)
        {
            for (int y = 0; y < radius*2; y++)
            {
                
                //Calculation of the "bump" on a given coordinate
                float px = (float)x / (float)radius/2;
                float py = (float)y / (float)radius/2;

                float cosval = Mathf.Sin(px * Mathf.PI);
                float cosval2 = Mathf.Sin(py * Mathf.PI);

                //height of bump on that coordinate
				float tmpHeight = (cosval/10) * cosval2;

                //if the bump height is less than the height in the original heightmap, it is applied to it.
				if ((heights[(xCoord - radius) + x, (yCoord - radius) + y]) < 0.1f && (heights[(xCoord - radius) + x, (yCoord - radius) + y]) <= tmpHeight)
				{
					heights[(xCoord - radius) + x, (yCoord - radius) + y] = tmpHeight;
				}               
            }
        }
    }

    /// <summary>
    /// Function to add additional Perlin noise to the heightmap.
    /// </summary>
    public void PerlinNoise()
    {
        terrain = GetComponent<Terrain>();

        int cHeight = terrain.terrainData.heightmapWidth;
        int cWidth = terrain.terrainData.heightmapHeight;

        float[,] originalMap = terrain.terrainData.GetHeights(0, 0, cWidth, cHeight);

        float rnd = (float)(DateTime.Now.Millisecond)/1000;

        for (int x = 0; x < cWidth; x++)
        {
            for (int y = 0; y < cHeight; y++)
            {

                float px = (float)x / (float)cWidth;
                float py = (float)y / (float)cHeight;

                originalMap[x, y] += perlinHeight * Mathf.PerlinNoise((px + rnd) * perlinScale, (py + rnd) * perlinScale);
                
            }
        }

        terrain.terrainData.SetHeights(0, 0, originalMap);
    }

    /// <summary>
    /// Function to flatten the Heightmap. This is done before every new Map Generation,
    /// </summary>
    void Flatten()
    {
        for (int i = 0; i < terrain.terrainData.heightmapWidth; i++)
        {
            for (int k = 0; k < terrain.terrainData.heightmapHeight; k++)
            {
                heights[i, k] = 0;
            }
        }

        terrain.terrainData.SetHeights(0, 0, heights);
    }

    /// <summary>
    /// Function to scale the map up and center it.
    /// </summary>
    /// <param name="thisHeight">Current Height of the map that is going to be scaled</param>
    /// <param name="thisWidth">Current Width of the map that is going to be scaled</param>
    /// <param name="thisMap">Current map that is going to be scaled</param>
    void ScaleMap(int thisHeight, int thisWidth, int[,] thisMap)
    {
        scaledHeight = thisHeight * 2;
        scaledWidth = thisWidth * 2;

        mapx2 = new int[scaledWidth, scaledHeight];

        for (int x = 0; x < thisWidth; x++)
        {
            for (int y = 0; y < thisHeight; y++)
            {
                if (thisMap[x, y] == 1)
                {
                    mapx2[x * 2, y * 2] = 1;
                    mapx2[x * 2 + 1, y * 2] = 1;
                    mapx2[x * 2, y * 2 + 1] = 1;
                    mapx2[x * 2 + 1, y * 2 + 1] = 1;
                }

                else
                {
                    mapx2[x * 2, y * 2] = 0;
                    mapx2[x * 2 + 1, y * 2] = 0;
                    mapx2[x * 2, y * 2 + 1] = 0;
                    mapx2[x * 2 + 1, y * 2 + 1] = 0;
                }
            }
        }

    }

    /// <summary>
    /// Invert the '0' to '1'.
    /// </summary>
    void InvertMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                map[x, y] = (map[x, y] == 0) ? 1 : 0;
            }
        }

    }

    /// <summary>
    /// Initial Map Generation using Cellulart Automata.
    /// Original idea from Sebastian Lague's Cellular Automata tutorials from Youtube.
    /// </summary>
    void GenerateMap()
    {
        //Flat the map
        Flatten();

        map = new int[width, height];

        //Fill the map with Random Noise
        RandomFillMap();

        //Perfrom the Cellular Automata
        for (int i = 0; i < smoothTimes; i++)
        {
            SmoothMap();
        }

        //Invert the map because I was stupid and lazy...
        InvertMap();

        //Scale the map up x times, since it was shrunk x times initially.
        //This is necessary because big grids (e.g. 512x512 terrain heightmap)
        //doesn't make too good job with Cellular Automata, and it doesn't create
        //good looking maps for island generation (64*64 was way better). This is
        //why the map is shrunk initially so we can get better results with CA and
        //then rescaled to fit the original map.
        ScaleMap(height, width, map);

        while (scaledHeight < height * scaleHeightMultip)
        {
            ScaleMap(scaledHeight, scaledWidth, mapx2);
        }

        //Find Edges for the heightmap
        FindEdges();

        //Cellular Automata has created our grid and 0 means ocean, 1 means island,
        //so we raise the heightmap where there is land
        for (int x = 0; x < scaledWidth; x++)
        {
            for (int y = 0; y < scaledHeight; y++)
            {
                if (mapx2[x, y] == 1)
                    heights[x, y] = 0.1f;
                if (mapx2[x, y] == 0)
                    heights[x, y] = 0.0f;
            }
        }

        terrain.terrainData.SetHeights(0, 0, heights);
        heights2 = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapWidth);

    }

    /// <summary>
    /// Function to start a Thread to calculate the shores in the map
    /// </summary>
    public void CalculateShores()
    {
        terrain = GetComponent<Terrain>();

        terrainreso = terrain.terrainData.heightmapResolution;

        //Depending on the resolution, we need to change the maximum radius (for shore steepness) accordingly.
        //Radius is tested while resolution is 513, so then it works on it's own, but if the resolution is higher,
        //then radius needs to be bigger too and vice versa
        maxRadius2 = (int)(maxRadius * (((float)terrainreso - 1.0f) / 512.0f));

        gridSize = scaledHeight * scaledWidth;
        calculatedPixels = 0;

        terrainDataMapHeight = terrain.terrainData.heightmapHeight;
        terrainDataMapWidth = terrain.terrainData.heightmapWidth;

        th1 = new Thread(Thread1);

        th1.Start();
    }

    /// <summary>
    /// This is called after the Thread has calculated the shores
    /// and they can be applied to the map.
    /// </summary>
    public void SmoothShores()
    {
        terrain = GetComponent<Terrain>();
        terrain.terrainData.SetHeights(0, 0, heights);
        heights2 = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapWidth);
    }

    /// <summary>
    /// This algorithm finds all the edges of the original Cellular Automata map
    /// between land and water and marks them so the shores can be calculated there.
    /// </summary>
    void FindEdges()
    {
        for (int x = 1; x < scaledWidth - 1; x++)
        {
            for (int y = 1; y < scaledHeight - 1; y++)
            {
                if (mapx2[x, y] == 1)
                {
                    for (int x2 = x - 1; x2 < x + 2; x2++)
                    {
                        for (int y2 = y - 1; y2 < y + 2; y2++)
                        {
                            if (mapx2[x2, y2] == 0) mapx2[x2, y2] = 2;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// This fills the map with random noise, so the Cellular Automata can use that
    /// noise to create the map.
    /// Original idea from Sebastian Lague's Cellular Automata tutorials from Youtube.
    /// </summary>
    void RandomFillMap()
    {
        if (useRandomSeed)
        {
            seed = DateTime.Now.GetHashCode().ToString();
        }

        if (seed == null)
            seed = "0";

        System.Random pseudoRandom = new System.Random(seed.GetHashCode());

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    map[x, y] = 1;
                }
                else
                {
                    map[x, y] = (pseudoRandom.Next(0, 100) < randomFillPercent) ? 1 : 0;
                }
            }
        }
    }

    /// <summary>
    /// Actual Cellular Automata function.
    /// Original idea from Sebastian Lague's Cellular Automata tutorials from Youtube.
    /// </summary>
    void SmoothMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int neighborWallTiles = GetSurroundingWallCount(x, y);

                if (neighborWallTiles > neighboringWalls)
                    map[x, y] = 1;
                else if (neighborWallTiles < neighboringWalls)
                    map[x, y] = 0;

            }
        }
    }

    /// <summary>
    /// Function for Cellular Automata. This finds how many "walls" each pixel has around it and
    /// decides whether or not keep/make that pixel a wall or not.
    /// Original idea from Sebastian Lague's Cellular Automata tutorials from Youtube.
    /// </summary>
    /// <param name="gridX">X position of the pixel in hand</param>
    /// <param name="gridY">Y position of the pixel in hand</param>
    /// <returns>How many neighbouring walls that pixel has</returns>
    int GetSurroundingWallCount(int gridX, int gridY)
    {
		int wallCount = 0;

		for (int neighborX = gridX - 1; neighborX <= gridX + 1; neighborX ++)
        {
			for (int neighborY = gridY - 1; neighborY <= gridY + 1; neighborY ++)
            {
				if (neighborX >= 0 && neighborX < width && neighborY >= 0 && neighborY < height)
                {
					if (neighborX != gridX || neighborY != gridY)
                    {
						wallCount += map[neighborX,neighborY];
					}
				}

				else
                {
					wallCount ++;
				}
			}
		}

		return wallCount;
	}
}
