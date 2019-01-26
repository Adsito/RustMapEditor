using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static WorldConverter;
using static WorldSerialization;
using Random=UnityEngine.Random;

[Serializable]
public class MapIO : MonoBehaviour {
    
    public TerrainTopology.Enum topologyLayer;
    public TerrainTopology.Enum oldTopologyLayer;
	public TerrainTopology.Enum targetTopologyLayer;
	public TerrainSplat.Enum terrainLayer;
	public TerrainSplat.Enum targetTerrainLayer;
	public TerrainBiome.Enum targetBiomeLayer;
	public TerrainBiome.Enum paintBiomeLayer;
	public PrefabLookup prefabs;
    public int landSelectIndex = 0;
    public string landLayer = "ground";
    LandData selectedLandLayer;
    public WorldSerialization.PrefabData prefabData;


    static TopologyMesh topology;
	static TopologyMesh pTopologyMesh;
	
    public void saveTopologyLayer()
    {
        if (topology == null)
            topology = GameObject.FindGameObjectWithTag("Topology").GetComponent<TopologyMesh>();

        LandData topologyData = GameObject.FindGameObjectWithTag("Land").transform.Find("Topology").GetComponent<LandData>();
        TerrainMap<int> topologyMap = new TerrainMap<int>(topology.top,1);
        float[,,] splatMap = TypeConverter.singleToMulti(topologyData.splatMap,2);
		
        if (splatMap == null)
        {
            Debug.LogError("Splatmap is null");
            return;
        }
        //Debug.Log(topologyMap.BytesTotal());

        for (int i = 0; i < topologyMap.res; i++)
        {
            for (int j = 0; j < topologyMap.res; j++)
            {
                if(splatMap[i,j,0] > 0)
                {
                    topologyMap[i, j] = topologyMap[i, j] | (int)oldTopologyLayer;
                }
                if (splatMap[i, j, 1] > 0)
                {
                    topologyMap[i, j] = topologyMap[i, j] & ~(int)oldTopologyLayer;
                }
            }
        }


        topology.top = topologyMap.ToByteArray();
    }

	public void terrainToTopology(float threshhold)
	{
		LandData topologyData = GameObject.FindGameObjectWithTag("Land").transform.Find("Topology").GetComponent<LandData>();
		float[,,] splatMap = TypeConverter.singleToMulti(topologyData.splatMap, 2);
		
		LandData groundLandData = GameObject.FindGameObjectWithTag("Land").transform.Find("Ground").GetComponent<LandData>();
		float[,,] targetGround = TypeConverter.singleToMulti(groundLandData.splatMap, 8);
		int t = TerrainSplat.TypeToIndex((int)targetTerrainLayer);
		
		
		for (int i = 0; i < targetGround.GetLength(0); i++)
        {
            for (int j = 0; j < targetGround.GetLength(1); j++)
            {
                if (targetGround[i,j,t] >= threshhold)
				{
					splatMap[i, j, 0] = float.MaxValue;
					splatMap[i, j, 1] = float.MinValue;
				}
            }
        }
		topologyData.setData(splatMap, "topology");
        topologyData.setLayer();
		saveTopologyLayer();
		
	}
	
	public void clearBiome()
	{
		LandData biomeLandData = GameObject.FindGameObjectWithTag("Land").transform.Find("Biome").GetComponent<LandData>();
		float[,,] newBiome = TypeConverter.singleToMulti(biomeLandData.splatMap, 4);
		
		for (int i = 0; i < newBiome.GetLength(0); i++)
        {
            for (int j = 0; j < newBiome.GetLength(0); j++)
            {
                
				newBiome[i, j, 0] = 0f;
				newBiome[i, j, 1] = 1f;                
				newBiome[i, j, 2] = 0f;
				newBiome[i, j, 3] = 0f;				
            }
        }
		biomeLandData.setData(newBiome, "biome");
		biomeLandData.setLayer();
		changeLandLayer();
	}
	
	public void invertBiome()
	{
		LandData biomeLandData = GameObject.FindGameObjectWithTag("Land").transform.Find("Biome").GetComponent<LandData>();
		float[,,] newBiome = TypeConverter.singleToMulti(biomeLandData.splatMap, 4);
		float[,,] oldBiome = TypeConverter.singleToMulti(biomeLandData.splatMap, 4);
		
		for (int i = 0; i < newBiome.GetLength(0); i++)
        {
            for (int j = 0; j < newBiome.GetLength(0); j++)
            {
                
				newBiome[i, j, 0] = oldBiome[i, j, 1];
				newBiome[i, j, 1] = oldBiome[i, j, 0];
				
            }
        }
		biomeLandData.setData(newBiome, "biome");
		biomeLandData.setLayer();
		changeLandLayer();
	}
	
	
	public void clearAlpha()
	{
		LandData alphaLandData = GameObject.FindGameObjectWithTag("Land").transform.Find("Alpha").GetComponent<LandData>();
		float[,,] newAlpha = TypeConverter.singleToMulti(alphaLandData.splatMap, 2);
		for (int i = 0; i < newAlpha.GetLength(0); i++)
        {
            for (int j = 0; j < newAlpha.GetLength(1); j++)
            {
                								
					newAlpha[i, j, 0] = 1f;
					newAlpha[i, j, 1] = 0f;
								
            }
        }
		alphaLandData.setData(newAlpha, "alpha");
		alphaLandData.setLayer();
		changeLandLayer();
	}
	
	
	public void paintBiomeHeight(int z1, int z2)
	{
		Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
		float[,] baseMap = land.terrainData.GetHeights(0, 0, land.terrainData.heightmapWidth, land.terrainData.heightmapHeight);
		
		LandData biomeLandData = GameObject.FindGameObjectWithTag("Land").transform.Find("Biome").GetComponent<LandData>();
		float[,,] newBiome = TypeConverter.singleToMulti(biomeLandData.splatMap, 4);
		
		int t = TerrainBiome.TypeToIndex((int)paintBiomeLayer);
		
        if (land.terrainData.heightmapWidth > 2048) //The heightmap is twice the resolution of the Alphamap so this scales it down to 2048 for accurate painting.
        {
            for (int i = 0; i < 4096; i++)
            {
                for (int j = 0; j < 4096; j++)
                {
                    if (baseMap[i, j] * 1000f > z1 && baseMap[i, j] * 1000f < z2)
                    {
                        newBiome[i / 2, j / 2, 0] = 0f;
                        newBiome[i / 2, j / 2, 1] = 0f;
                        newBiome[i / 2, j / 2, 2] = 0f;
                        newBiome[i / 2, j / 2, 3] = 0f;
                        newBiome[i / 2, j / 2, t] = 1f;
                    }

                }
            }
        }
        else
        {
            for (int i = 0; i < newBiome.GetLength(0); i++)
            {
                for (int j = 0; j < newBiome.GetLength(1); j++)
                {
                    if (baseMap[i, j]*1000f > z1 && baseMap[i,j]*1000f < z2)
				    {
				        newBiome[i, j, 0] = 0f;
				        newBiome[i, j, 1] = 0f;                
				        newBiome[i, j, 2] = 0f;
				        newBiome[i, j, 3] = 0f;
				        newBiome[i, j, t] = 1f;
				    }				
				
                }
            }
        }
		biomeLandData.setData(newBiome, "biome");
		biomeLandData.setLayer();
		changeLandLayer();
	}
	
	public void paintSplatHeight(float z1, float z2)
	{
		Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
		float[,] baseMap = land.terrainData.GetHeights(0, 0, land.terrainData.heightmapWidth, land.terrainData.heightmapHeight);
		
		LandData groundLandData = GameObject.FindGameObjectWithTag("Land").transform.Find("Ground").GetComponent<LandData>();
		float[,,] newGround = TypeConverter.singleToMulti(groundLandData.splatMap, 8);
		int t = TerrainSplat.TypeToIndex((int)terrainLayer);

        if (land.terrainData.heightmapWidth > 2048) //The heightmap is twice the resolution of the Alphamap so this scales it down to 2048 for accurate painting.
        {
            for (int i = 0; i < 4096; i++)
            {
                for (int j = 0; j < 4096; j++)
                {
                    if (baseMap[i, j] * 1000f > z1 && baseMap[i, j] * 1000f < z2)
                    {
                        newGround[i / 2, j / 2, 0] = 0;
                        newGround[i / 2, j / 2, 1] = 0;
                        newGround[i / 2, j / 2, 2] = 0;
                        newGround[i / 2, j / 2, 3] = 0;
                        newGround[i / 2, j / 2, 4] = 0;
                        newGround[i / 2, j / 2, 5] = 0;
                        newGround[i / 2, j / 2, 6] = 0;
                        newGround[i / 2, j / 2, 7] = 0;
                        newGround[i / 2, j / 2, t] = 1;
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < newGround.GetLength(0); i++)
            {
                for (int j = 0; j < newGround.GetLength(1); j++)
                {
                    if (baseMap[i, j]*1000f > z1 && baseMap[i,j]*1000f < z2)
				    {
					    newGround[i, j, 0] = 0;
					    newGround[i, j, 1] = 0;
					    newGround[i, j, 2] = 0;
					    newGround[i, j, 3] = 0;
					    newGround[i, j, 4] = 0;
					    newGround[i, j, 5] = 0;
					    newGround[i, j, 6] = 0;
					    newGround[i, j, 7] = 0;
					    newGround[i, j, t] = 1;								
				    }
                }
            }
        }
		groundLandData.setData(newGround, "ground");
		groundLandData.setLayer();
	}
	
	public void paintTerrainSlope(float s1, float s2)
	{
		Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
		float[,] baseMap = land.terrainData.GetHeights(0, 0, land.terrainData.heightmapWidth, land.terrainData.heightmapHeight);
		
		LandData groundLandData = GameObject.FindGameObjectWithTag("Land").transform.Find("Ground").GetComponent<LandData>();
		float[,,] newGround = TypeConverter.singleToMulti(groundLandData.splatMap, 8);
		int t = TerrainSplat.TypeToIndex((int)terrainLayer);
		
        if (land.terrainData.heightmapWidth > 4096) //The heightmap is twice the resolution of the Alphamap so this scales it down to 2048 for accurate painting.
        {
            for (int i = 1; i < 4096 - 1; i++)
            {
                for (int j = 1; j < 4096 - 1; j++)
                {
                    if ((baseMap[i, j] / baseMap[i + 1, j + 1] < s1 || baseMap[i, j] / baseMap[i - 1, j - 1] < s1) &&
                    (baseMap[i, j] / baseMap[i + 1, j + 1] > s2 || baseMap[i, j] / baseMap[i - 1, j - 1] > s2))
                    {
                        newGround[i / 2, j / 2, 0] = 0;
                        newGround[i / 2, j / 2, 1] = 0;
                        newGround[i / 2, j / 2, 2] = 0;
                        newGround[i / 2, j / 2, 3] = 0;
                        newGround[i / 2, j / 2, 4] = 0;
                        newGround[i / 2, j / 2, 5] = 0;
                        newGround[i / 2, j / 2, 6] = 0;
                        newGround[i / 2, j / 2, 7] = 0;
                        newGround[i / 2, j / 2, t] = 1;
                    }
                }
            }
        }
        else
        {
            for (int i = 1; i < newGround.GetLength(0)-1; i++)
            {
                for (int j = 1; j < newGround.GetLength(1)-1; j++)
                {
                    if ((baseMap[i, j] / baseMap[i+1,j+1] < s1 || baseMap[i, j] / baseMap[i-1,j-1] < s1) &&
				    (baseMap[i, j] / baseMap[i+1,j+1] > s2 || baseMap[i, j] / baseMap[i-1,j-1] > s2))
				    {
					    newGround[i, j, 0] = 0;
					    newGround[i, j, 1] = 0;
					    newGround[i, j, 2] = 0;
					    newGround[i, j, 3] = 0;
					    newGround[i, j, 4] = 0;
					    newGround[i, j, 5] = 0;
					    newGround[i, j, 6] = 0;
					    newGround[i, j, 7] = 0;
					    newGround[i, j, t] = 1;								
				    }
                }
            }
        }
		groundLandData.setData(newGround, "ground");
		groundLandData.setLayer();
	}
	
	public void debugSplatPixel(int x, int y)
	{
		LandData groundLandData = GameObject.FindGameObjectWithTag("Land").transform.Find("Ground").GetComponent<LandData>();
		float[,,] newGround = TypeConverter.singleToMulti(groundLandData.splatMap, 8);
		
		for (int k = 0; k < 7; k++)
		{
			Debug.LogError("k:" + newGround[x,y,k]);
		}
		
	}
	
	public void perlinOctave(int l, int p, float s)
	{
	
			
			Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
			float[,] baseMap = land.terrainData.GetHeights(0, 0, land.terrainData.heightmapWidth, land.terrainData.heightmapHeight);
			float[,] perlinSum = baseMap;
			
			for (int i = 0; i < baseMap.GetLength(0); i++)
			{
					
				for (int j = 0; j < baseMap.GetLength(0); j++)
				{
					perlinSum[i,j] = (0);
				}
			}
			
			float r = 0;
			float r1 = 0;
			float amplitude = .5f;
			
			
			for (int u = 0; u < l; u++)
			{
				
				r = Random.Range(0,10000)/100f;
				r1 =  Random.Range(0,10000)/100f;
				
				
				for (int i = 0; i < baseMap.GetLength(0); i++)
				{
					
					for (int j = 0; j < baseMap.GetLength(0); j++)
					{
						
						perlinSum[i,j] += Math.Abs(1f - Mathf.PerlinNoise(Math.Abs((Mathf.PerlinNoise(i*1f/s+r, j*1f/s+r1))-.5f), Math.Abs((Mathf.PerlinNoise(i*1f/s+r, j*1f/s+r1))-.5f)));
					}
				}
												
				s = s + p;
				
			}
			
			for (int i = 0; i < baseMap.GetLength(0); i++)
			{
					
				for (int j = 0; j < baseMap.GetLength(0); j++)
				{
					perlinSum[i,j] = (perlinSum[i,j] / l)*.5f+.25f;
				}
			}
			
			
	
			land.terrainData.SetHeights(0, 0, perlinSum);
			changeLandLayer();
	
	}	
	
	public void perlinHatred(int l, int p, float s)
	{
	
			
			Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
			float[,] baseMap = land.terrainData.GetHeights(0, 0, land.terrainData.heightmapWidth, land.terrainData.heightmapHeight);
			float[,] perlinSum = baseMap;
			
			
			for (int i = 0; i < baseMap.GetLength(0); i++)
			{
					
				for (int j = 0; j < baseMap.GetLength(0); j++)
				{
					perlinSum[i,j] = (0);
				}
			}
			
			
			float r = 0;
			float r1 = 0;
			float amplitude = 1f;
			
			
			for (int u = 0; u < l; u++)
			{
				
				r = Random.Range(0,10000)/100f;
				r1 =  Random.Range(0,10000)/100f;
				amplitude *= .3f;
				
				for (int i = 0; i < baseMap.GetLength(0); i++)
				{
					
					for (int j = 0; j < baseMap.GetLength(0); j++)
					{
						
						perlinSum[i,j] += amplitude * (Mathf.PerlinNoise(Mathf.PerlinNoise(Mathf.PerlinNoise(i*1f/s+r,j*1f/s+r), Mathf.PerlinNoise(i*1f/s+r,j*1f/s+r1)), Mathf.PerlinNoise(Mathf.PerlinNoise(i*1f/s+r,j*1f/s+r), Mathf.PerlinNoise(i*1f/s+r,j*1f/s+r1))));
					}
				}
												
				s = s + p;
				
			}
			/*
			for (int i = 0; i < baseMap.GetLength(0); i++)
			{
					
				for (int j = 0; j < baseMap.GetLength(0); j++)
				{
					perlinSum[i,j] = (perlinSum[i,j] / l)*.5f+.25f;
				}
			}
			*/
			
	
			land.terrainData.SetHeights(0, 0, perlinSum);
			changeLandLayer();
	
	}	
	
	public void diamondSquareNoise(int roughness, int height, int weight)
	{
			Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
			float[,] baseMap = land.terrainData.GetHeights(0, 0, land.terrainData.heightmapWidth, land.terrainData.heightmapHeight);
			
			int res = baseMap.GetLength(0);
			float[,] newMap = new float[res,res];
			
			//copied from robert stivanson's 'unity-diamond-square'
			//https://github.com/RobertStivanson
			
			//initialize corners
			
			newMap[0,0] = Random.Range(450,500)/1000f;
			newMap[res-1,0] = Random.Range(450,500)/1000f;
			newMap[0,res-1] = Random.Range(450,500)/1000f;
			newMap[res-1, res-1] = Random.Range(450,500)/1000f;
			
			
			int j, j2, x, y;
			float avg = 0.5f;
			float range = 1f;			
			
			for (j = res - 1; j > 1; j /= 2) 
			{
				j2 = j / 2;
			
				//diamond
				for (x = 0; x < res - 1; x += j) 
				{
					for (y = 0; y < res - 1; y += j) 
					{
						avg = newMap[x, y];
						avg += newMap[x + j, y];
						avg += newMap[x, y + j];
						avg += newMap[x + j, y + j];
						avg /= 4.0f;

						avg += (Random.Range(0,height)/1000f - height/2000f) * range;
						newMap[x + j2, y + j2] = avg;
					}
				}
				
				//square
				for (x = 0; x < res - 1; x += j2) 
				{
					for (y = (x + j2) % j; y < res - 1; y += j) 
					{
						avg = newMap[(x - j2 + res - 1) % (res - 1), y];
						avg += newMap[(x + j2) % (res - 1), y];
						avg += newMap[x, (y + j2) % (res - 1)];
						avg += newMap[x, (y - j2 + res - 1) % (res - 1)];
						avg /= 4.0f;

						
						avg += (Random.Range(0,height)/1000f - height/2000f) * range;
						
						
						newMap[x, y] = avg;

						
						if (x == 0)
						{							
							newMap[res - 1, y] = avg;
						}
						

						if (y == 0) 
						{
							newMap[x, res - 1] = avg;
						}
						
	
					}
				}
				
				range -= range * (roughness/100f);
			
			
			
			}

			//multifractalize
			for(int h = 0; h < res; h++)
			{
				for(int i = 0; i < res; i++)
				{
					//hi
					newMap[h,i] = newMap[h,i] * (weight/100f) * baseMap[h,i];
				}
			}
	
			land.terrainData.SetHeights(0, 0, newMap);
			changeLandLayer();
	
	}	
	
	
	public void christmasize()
	{
		LandData groundLandData = GameObject.FindGameObjectWithTag("Land").transform.Find("Ground").GetComponent<LandData>();
		float[,,] baseGround = TypeConverter.singleToMulti(groundLandData.splatMap, 8);
		float[,,] targetGround = TypeConverter.singleToMulti(groundLandData.splatMap, 8);
		
		for (int i = 0; i < targetGround.GetLength(0); i++)
			{
					
				for (int j = 0; j < targetGround.GetLength(0); j++)
				{
					targetGround[i,j,1] = baseGround[i,j,4];
					targetGround[i,j,4] = baseGround[i,j,1];
				}
			}
			
		groundLandData.setData(targetGround, "ground");
		groundLandData.setLayer();
		changeLandLayer();
	}
	
	public void perlinTerrain(int l, int p, float s)
	{
			
			Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
			float[,] baseMap = land.terrainData.GetHeights(0, 0, land.terrainData.heightmapWidth, land.terrainData.heightmapHeight);
			float[,] perlinSum = baseMap;
			
			for (int i = 0; i < baseMap.GetLength(0); i++)
			{
					
				for (int j = 0; j < baseMap.GetLength(0); j++)
				{
					perlinSum[i,j] = (0);
				}
			}
			
			float r = 0;
			float r1 = 0;
			
			
			
			for (int u = 0; u < l; u++)
			{
				
				r = Random.Range(0,10000)/100f;
			r1 =  Random.Range(0,10000)/100f;
				
				
				
				for (int i = 0; i < baseMap.GetLength(0); i++)
				{
					
					for (int j = 0; j < baseMap.GetLength(0); j++)
					{
						perlinSum[i,j] = perlinSum[i,j] + Mathf.PerlinNoise(i*1f/s+r, j*1f/s+r1) * .33f + .33f;
					}
				}
												
				s = s + p;
				
			}
			
			for (int i = 0; i < baseMap.GetLength(0); i++)
			{
					
				for (int j = 0; j < baseMap.GetLength(0); j++)
				{
					perlinSum[i,j] = (perlinSum[i,j] / l)*.5f+.25f;
				}
			}
			
			
	
			land.terrainData.SetHeights(0, 0, perlinSum);
			changeLandLayer();
	}
	
	public void zNudge(int z)
	{
			
			Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
			float[,] baseMap = land.terrainData.GetHeights(0, 0, land.terrainData.heightmapWidth, land.terrainData.heightmapHeight);

				
			
			for (int i = 0; i < baseMap.GetLength(0); i++)
			{
					
				for (int j = 0; j < baseMap.GetLength(0); j++)
				{
					baseMap[i,j] = baseMap[i,j] + (z / 1000f);
				}
			}
			
			
						
	
			land.terrainData.SetHeights(0, 0, baseMap);
			changeLandLayer();
	}
	
	
	public void bordering()
	{
			
			Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
			float[,] baseMap = land.terrainData.GetHeights(0, 0, land.terrainData.heightmapWidth, land.terrainData.heightmapHeight);
			float[,] floorMap = baseMap;
			float[,] gradientMap = floorMap;
			
			int dist = 150;
			int sizer = baseMap.GetLength(0);
			
			
			
			
			
						
	
			land.terrainData.SetHeights(0, 0, gradientMap);
			changeLandLayer();
	}
	
	
	public void paintPerlin(int s, float c, bool invert, bool paintBiome)
	{
		LandData biomeLandData = GameObject.FindGameObjectWithTag("Land").transform.Find("Biome").GetComponent<LandData>();
		float[,,] newBiome = TypeConverter.singleToMulti(biomeLandData.splatMap, 4);
		
		LandData groundLandData = GameObject.FindGameObjectWithTag("Land").transform.Find("Ground").GetComponent<LandData>();
		float[,,] newGround = TypeConverter.singleToMulti(groundLandData.splatMap, 8);
		int t = TerrainSplat.TypeToIndex((int)terrainLayer);
		int blendhate = 0;
		float o = 0;
		float r = Random.Range(0,10000)/100f;
		float r1 = Random.Range(0,10000)/100f;
		int index = TerrainBiome.TypeToIndex((int)targetBiomeLayer);
		Debug.LogError(r);
		for (int i = 0; i < newBiome.GetLength(0); i++)
        {
            for (int j = 0; j < newBiome.GetLength(1); j++)
            {
					o = Mathf.PerlinNoise(i*1f/s+r,j*1f/s+r1);
					o = o*c;
					
					if (paintBiome)
						o = o * newBiome[i,j, index];
					
					if (o > 1f)
						o=1f;
					
					if (invert)
						o = 1f - o; 
					
					for (int m = 0; m <=7; m++)
									{
										if (paintBiome)
										{
											blendhate = 0;
											
											if(m!=t)
											{
												if(newGround[i,j,m] > 0)												
												{
													if (blendhate == 0)
													{
														if (newBiome[i,j,index] > 0)
														{
															newGround[i, j, m] = 1f-o;
														}
													}
													else
														{
															newGround[i, j, m] = 0;
														}
													
													blendhate++;
													
												}
											}
											else
											{
												if(newGround[i, j, t] !=1)
												{
													if (newBiome[i,j,index] > 0)
														newGround[i, j, t] = o;
												}
											}	
										}
										else
										{	
											blendhate = 0;
											
											if(m!=t)
											{
												if(newGround[i,j,m] > 0)												
												{
													if (blendhate == 0)
													{
														newGround[i, j, m] = 1f-o;
													}
													else
													{
														newGround[i, j, m] = 0;
													}
													
													blendhate++;
												}
											}
											else
											{
												if(newGround[i, j, t] !=1)
												{
													newGround[i, j, t] = o;
												}
											}	
										}
									}
																			
				
            }
        }
		//dont forget this shit again
		groundLandData.setData(newGround, "ground");
		groundLandData.setLayer();
	
	}
	
	public void paintCrazing(int z, int a, int b)
	{
		//z is number of random zones, a is min size, a1 is max
		
		
		LandData groundLandData = GameObject.FindGameObjectWithTag("Land").transform.Find("Ground").GetComponent<LandData>();
		float[,,] newGround = TypeConverter.singleToMulti(groundLandData.splatMap, 8);
		int t = TerrainSplat.TypeToIndex((int)terrainLayer);
		
		
		
		int s = Random.Range(a, b);
		int uB = newGround.GetLength(0);
		
		for (int i = 0; i < z; i++)
        {
			int x = Random.Range(1, newGround.GetLength(0));
			int y = Random.Range(1, newGround.GetLength(0));
            for (int j = 0; j < s; j++)
            {
					x = x + Random.Range(-1,2);
					y = y + Random.Range(-1,2);

					if (x <= 1)
						x = 2;
					if (y <= 1)
						y = 2;
					
					if (x >= uB)
						x = uB-1;
					
					if (y >= uB)
						y = uB-1;
						
					
					newGround[x, y, 0] = 0;
					newGround[x, y, 1] = 0;
					newGround[x, y, 2] = 0;
					newGround[x, y, 3] = 0;
					newGround[x, y, 4] = 0;
					newGround[x, y, 5] = 0;
					newGround[x, y, 6] = 0;
					newGround[x, y, 7] = 0;
					//dirty
					newGround[x, y, t] = 1;								
				
            }
        }
		
		//dont forget this shit again
		groundLandData.setData(newGround, "ground");
		groundLandData.setLayer();
	
	}
	
	
	
	public void paintTerrainOutline(int w, float o)
	{
		
		
		LandData groundLandData = GameObject.FindGameObjectWithTag("Land").transform.Find("Ground").GetComponent<LandData>();
		float[,,] newGround = TypeConverter.singleToMulti(groundLandData.splatMap, 8);
		float[,,] outlineGround = TypeConverter.singleToMulti(groundLandData.splatMap, 8);
		
		int t = TerrainSplat.TypeToIndex((int)terrainLayer);
		
		int blendhate = 0;
		
		for (int n = 1; n < w+1; n++)
		{
			
			for (int i = 1; i < newGround.GetLength(0)-1; i++)
			{
				for (int j = 1; j < newGround.GetLength(1)-1; j++)
				{
					for (int k = -1; k <= 1; k++)
					{
						for (int l = -1; l <= 1; l++)
						{
								if (newGround[i+k, j+l, t] == 1)
								{
									for (int m = 0; m <=7; m++)
									{
										
										blendhate = 0;
										
										if(m!=t)
										{
											if(newGround[i,j,m] > 0)												
											{
												if (blendhate == 0)
													outlineGround[i, j, m] = 1-o;
												else
													outlineGround[i, j, m] = 0;
												
												blendhate++;
											}
										}
										else
										{
											if(newGround[i, j, t] !=1)
												outlineGround[i, j, t] = o;
										}
									}
								}
						}					
					}
				}
			}
									
		}
		
		
		groundLandData.setData(outlineGround, "ground");
		groundLandData.setLayer();
	}
	
	public void paintTopologyOutline(int w)
	{
				
		LandData topologyData = GameObject.FindGameObjectWithTag("Land").transform.Find("Topology").GetComponent<LandData>();
        float[,,] splatMap = TypeConverter.singleToMulti(topologyData.splatMap, 2);
		float[,,] sourceMap = topology.getSplatMap((int)targetTopologyLayer);
		
		//expand everything n pixels in all directions
		for (int n = 1; n <= w; n++)
		{
			
			for (int i = 1; i < sourceMap.GetLength(0)-1; i++)
			{
				for (int j = 1; j < sourceMap.GetLength(1)-1; j++)
				{
					for (int k = -1; k <= 1; k++)
					{
						for (int l = -1; l <= 1; l++)
						{
								if (sourceMap[i+k, j+l, 0] == float.MaxValue)
								{
									splatMap[i, j, 0] = float.MaxValue;
									splatMap[i, j, 1] = float.MinValue;
								}
						}					
					}
				}
			}
			
			
			for (int i = 1; i < sourceMap.GetLength(0)-1; i++)
			{
				for (int j = 1; j < sourceMap.GetLength(1)-1; j++)
				{
					sourceMap[i, j, 0] = splatMap[i, j, 0];
					sourceMap[i, j, 1] = splatMap[i, j, 1];
				}
			}
			
			
		}
		//reset the sourcemap
		sourceMap = topology.getSplatMap((int)targetTopologyLayer);
		
		//erase the original area
		
		for (int m = 0; m < sourceMap.GetLength(0); m++)
		{
			for (int o = 0; o < sourceMap.GetLength(0); o++)
			{
				if (splatMap[m, o, 0] == float.MaxValue ^ sourceMap[m, o, 0] == float.MaxValue)
				{
					splatMap[m, o, 0] = float.MaxValue;
					splatMap[m, o, 1] = float.MinValue;
				}
				else
				{
					splatMap[m, o, 0] = float.MinValue;
					splatMap[m, o, 1] = float.MaxValue;
				}
				
			}
		}
		
		topologyData.setData(splatMap, "topology");
        topologyData.setLayer();
		saveTopologyLayer();
	}
	
	public void notTopologyLayer()
	{
		LandData topologyData = GameObject.FindGameObjectWithTag("Land").transform.Find("Topology").GetComponent<LandData>();
        float[,,] splatMap = TypeConverter.singleToMulti(topologyData.splatMap, 2);
		float[,,] sourceMap = topology.getSplatMap((int)targetTopologyLayer);
		
		for (int m = 0; m < sourceMap.GetLength(0); m++)
		{
			for (int o = 0; o < sourceMap.GetLength(0); o++)
			{
				if ((splatMap[m, o, 0] == float.MaxValue && sourceMap[m, o, 0] == float.MaxValue))
				{
					splatMap[m, o, 0] = float.MinValue;
					splatMap[m, o, 1] = float.MaxValue;
				}
								
			}
		}
		
		topologyData.setData(splatMap, "topology");
        topologyData.setLayer();
		saveTopologyLayer();
		
	}
	
	public void paintHeight(float z1, float z2)
	{
		LandData topologyData = GameObject.FindGameObjectWithTag("Land").transform.Find("Topology").GetComponent<LandData>();
        float[,,] splatMap = TypeConverter.singleToMulti(topologyData.splatMap, 2);
		
		Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
		float[,] baseMap = land.terrainData.GetHeights(0, 0, land.terrainData.heightmapWidth, land.terrainData.heightmapHeight);
		
        if (land.terrainData.heightmapWidth > 2048) //The heightmap is twice the resolution of the Alphamap so this scales it down to 2048 for accurate painting.
        {
            for (int i = 0; i < 4096; i++)
            {
                for (int j = 0; j < 4096; j++)
                {
                    if (baseMap[i, j]*1000f > z1 && baseMap[i,j]*1000f < z2)
				    {
                        splatMap[i / 2, j / 2, 0] = float.MaxValue;
                        splatMap[i / 2, j / 2, 1] = float.MinValue;
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < splatMap.GetLength(0); i++)
            {
                for (int j = 0; j < splatMap.GetLength(1); j++)
                {
                    if (baseMap[i, j] * 1000f > z1 && baseMap[i, j] * 1000f < z2)
                    {
                        splatMap[i, j, 0] = float.MaxValue;
                        splatMap[i, j, 1] = float.MinValue;
                    }
                }
            }
        }
		topologyData.setData(splatMap, "topology");
        topologyData.setLayer();
		saveTopologyLayer();
	}
	
	public void paintSlope(float s)
	{
		LandData topologyData = GameObject.FindGameObjectWithTag("Land").transform.Find("Topology").GetComponent<LandData>();
        float[,,] splatMap = TypeConverter.singleToMulti(topologyData.splatMap, 2);
		
		Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
		float[,] baseMap = land.terrainData.GetHeights(0, 0, land.terrainData.heightmapWidth, land.terrainData.heightmapHeight);
		
        if (land.terrainData.heightmapWidth > 2048) //The heightmap is twice the resolution of the Alphamap so this scales it down to 2048 for accurate painting.
        {
            for (int i = 1; i < 4096 - 1; i++)
            {
                for (int j = 1; j < 4096 - 1; j++)
                {
                    if (baseMap[i, j] / baseMap[i + 1, j + 1] < s || baseMap[i, j] / baseMap[i - 1, j - 1] < s)
                    {
                        splatMap[i / 2, j / 2, 0] = float.MaxValue;
                        splatMap[i / 2, j / 2, 1] = float.MinValue;
                    }
                }
            }
        }
        else
        {
            for (int i = 1; i < splatMap.GetLength(0) - 1; i++)
            {
                for (int j = 1; j < splatMap.GetLength(1) - 1; j++)
                {
                    if (baseMap[i, j] / baseMap[i + 1, j + 1] < s || baseMap[i, j] / baseMap[i - 1, j - 1] < s)
                    {
                        splatMap[i, j, 0] = float.MaxValue;
                        splatMap[i, j, 1] = float.MinValue;
                    }
                }
            }
        }
		topologyData.setData(splatMap, "topology");
        topologyData.setLayer();
		saveTopologyLayer();
	}

	public void eraseHeight(float z1, float z2)
	{
		LandData topologyData = GameObject.FindGameObjectWithTag("Land").transform.Find("Topology").GetComponent<LandData>();
        float[,,] splatMap = TypeConverter.singleToMulti(topologyData.splatMap, 2);
		
		Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
		float[,] baseMap = land.terrainData.GetHeights(0, 0, land.terrainData.heightmapWidth, land.terrainData.heightmapHeight);
		
        if (land.terrainData.heightmapWidth > 2048) //The heightmap is twice the resolution of the Alphamap so this scales it down to 2048 for accurate painting.
        {
            for (int i = 0; i < 4096; i++)
            {
                for (int j = 0; j < 4096; j++)
                {
                    if (baseMap[i, j] * 1000f > z1 && baseMap[i, j] * 1000f < z2)
                    {
                        splatMap[i / 2, j / 2, 0] = float.MinValue;
                        splatMap[i / 2, j / 2, 1] = float.MaxValue;
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < splatMap.GetLength(0); i++)
            {
                for (int j = 0; j < splatMap.GetLength(1); j++)
                {
                    if (baseMap[i, j] * 1000f > z1 && baseMap[i, j] * 1000f < z2)
                    {
                        splatMap[i, j, 0] = float.MinValue;
                        splatMap[i, j, 1] = float.MaxValue;
                    }
                }
            }
        }
		topologyData.setData(splatMap, "topology");
        topologyData.setLayer();
		saveTopologyLayer();
	}
	
	public void invertTopologyLayer()
	{
		LandData topologyData = GameObject.FindGameObjectWithTag("Land").transform.Find("Topology").GetComponent<LandData>();
        float[,,] splatMap = TypeConverter.singleToMulti(topologyData.splatMap, 2);
        float[,,] tempMap = TypeConverter.singleToMulti(topologyData.splatMap, 2);
		for (int i = 0; i < splatMap.GetLength(0); i++)
        {
            for (int j = 0; j < splatMap.GetLength(1); j++)
            {
                splatMap[i, j, 0] = tempMap[i, j, 1];
                splatMap[i, j, 1] = tempMap[i, j, 0];
            }
        }
        topologyData.setData(splatMap, "topology");
        topologyData.setLayer();
		saveTopologyLayer();
	}
	
    public void clearTopologyLayer()
    {
        LandData topologyData = GameObject.FindGameObjectWithTag("Land").transform.Find("Topology").GetComponent<LandData>();
        float[,,] splatMap = TypeConverter.singleToMulti(topologyData.splatMap, 2);
        for (int i = 0; i < splatMap.GetLength(0); i++)
        {
            for (int j = 0; j < splatMap.GetLength(1); j++)
            {
                splatMap[i, j, 0] = float.MinValue;
                splatMap[i, j, 1] = float.MaxValue;
            }
        }
        topologyData.setData(splatMap, "topology");
        topologyData.setLayer();
    }

	public void copyTopologyLayer()
	{
		selectedLandLayer = GameObject.FindGameObjectWithTag("Land").transform.Find("Topology").GetComponent<LandData>();
        selectedLandLayer.setData(topology.getSplatMap((int)targetTopologyLayer), "topology");
		saveTopologyLayer();
		selectedLandLayer.setLayer();
		saveTopologyLayer();
	}
	
    public void changeLandLayer()
    {
        if (topology == null)
            topology = GameObject.FindGameObjectWithTag("Topology").GetComponent<TopologyMesh>();

        if (selectedLandLayer != null)
            selectedLandLayer.save();

        switch (landLayer.ToLower())
        {
            case "ground":
                selectedLandLayer = GameObject.FindGameObjectWithTag("Land").transform.Find("Ground").GetComponent<LandData>();
                break;
            case "biome":
                selectedLandLayer = GameObject.FindGameObjectWithTag("Land").transform.Find("Biome").GetComponent<LandData>();
                break;
            case "alpha":
                selectedLandLayer = GameObject.FindGameObjectWithTag("Land").transform.Find("Alpha").GetComponent<LandData>();
                break;
            case "topology":
                //updated topology values
                //selectedLandLayer.splatMap;
                saveTopologyLayer();
                selectedLandLayer = GameObject.FindGameObjectWithTag("Land").transform.Find("Topology").GetComponent<LandData>();
                selectedLandLayer.setData(topology.getSplatMap((int)topologyLayer), "topology");
				
                break;
        }
        selectedLandLayer.setLayer();
    }
    
    public float scale = 1f;
    public void scaleHeightmap()
    {
        Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
        float[,] heightMap = land.terrainData.GetHeights(0, 0, land.terrainData.heightmapWidth, land.terrainData.heightmapHeight);
        land.terrainData.SetHeights(0, 0, MapTransformations.scale(heightMap, scale));
    }

    public GameObject spawnPrefab(GameObject g, PrefabData prefabData, Transform parent = null)
    {
        Vector3 pos = new Vector3(prefabData.position.x, prefabData.position.y, prefabData.position.z);
        Vector3 scale = new Vector3(prefabData.scale.x, prefabData.scale.y, prefabData.scale.z);
        Quaternion rotation = Quaternion.Euler(new Vector3(prefabData.rotation.x, prefabData.rotation.y, prefabData.rotation.z));

        
        GameObject newObj = Instantiate(g, pos + getMapOffset(), rotation, parent);
        newObj.transform.localScale = scale;

        return newObj;
    }
	
	 

    private void cleanUpMap()
    {
        //offset = 0;
        selectedLandLayer = null;
        foreach(PrefabDataHolder g in GameObject.FindObjectsOfType<PrefabDataHolder>())
        {
            DestroyImmediate(g.gameObject);
        }

        foreach (PathDataHolder g in GameObject.FindObjectsOfType<PathDataHolder>())
        {
            DestroyImmediate(g.gameObject);
        }
    }


    public static Vector3 getTerrainSize()
    {
        return GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>().terrainData.size;
    }
    public static Vector3 getMapOffset()
    {
        //Debug.Log(0.5f * getTerrainSize());
        
		return 0.5f * getTerrainSize();
    }

	
    public void offsetHeightmap()
    {
        Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
        Vector3 difference = land.transform.position;
        float[,] heightMap = land.terrainData.GetHeights(0, 0, land.terrainData.heightmapWidth, land.terrainData.heightmapHeight);
        for (int i = 0; i < heightMap.GetLength(0); i++)
        {
            for (int j = 0; j < heightMap.GetLength(1); j++)
            {
                heightMap[i, j] = heightMap[i, j] + (difference.y / land.terrainData.size.y);
            }
        }
        land.terrainData.SetHeights(0, 0, heightMap);
        land.transform.position = Vector3.zero;
    }

    public void rotateHeightmap(bool CW)
    {
        Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
        Terrain water = GameObject.FindGameObjectWithTag("Water").GetComponent<Terrain>();
        var pathData = GameObject.FindGameObjectWithTag("Paths").GetComponentsInChildren<PathDataHolder>();
        var oldpathData = 0f;
        float[,] heightMap = land.terrainData.GetHeights(0, 0, land.terrainData.heightmapWidth, land.terrainData.heightmapHeight);
        float[,] waterMap = water.terrainData.GetHeights(0, 0, water.terrainData.heightmapWidth, water.terrainData.heightmapHeight);
        
        if (CW)
        {
            land.terrainData.SetHeights(0, 0, MapTransformations.rotateCW(heightMap));
            water.terrainData.SetHeights(0, 0, MapTransformations.rotateCW(waterMap));
            for (int i = 0; i < pathData.Length; i++)
            {
                for (int j = 0; j < pathData[i].pathData.nodes.Length; j++)
                {
                    oldpathData = pathData[i].pathData.nodes[j].x;
                    pathData[i].pathData.nodes[j].x = pathData[i].pathData.nodes[j].z;
                    pathData[i].pathData.nodes[j].z = oldpathData * -1;
                }
            }
        }
        else
        {
            land.terrainData.SetHeights(0, 0, MapTransformations.rotateCCW(heightMap));
            water.terrainData.SetHeights(0, 0, MapTransformations.rotateCCW(waterMap));
            for (int i = 0; i < pathData.Length; i++)
            {
                for (int j = 0; j < pathData[i].pathData.nodes.Length; j++)
                {
                    oldpathData = pathData[i].pathData.nodes[j].z;
                    pathData[i].pathData.nodes[j].z = pathData[i].pathData.nodes[j].x;
                    pathData[i].pathData.nodes[j].x = oldpathData * -1;
                }
            }
        }
    }
    public void rotateAlphamap(bool CW) //Todo: Have it all automagically work with a button.
    {
        LandData alphaLandData = GameObject.FindGameObjectWithTag("Land").transform.Find("Alpha").GetComponent<LandData>();
        float[,,] newAlpha = TypeConverter.singleToMulti(alphaLandData.splatMap, 2);
        float[,,] oldAlpha = TypeConverter.singleToMulti(alphaLandData.splatMap, 2);

        if (CW)
        {
            for (int i = 0; i < newAlpha.GetLength(0); i++)
            {
                for (int j = 0; j < newAlpha.GetLength(1); j++)
                {
                    newAlpha[i, j, 0] = oldAlpha[j, oldAlpha.GetLength(1) - i - 1, 0];
                    newAlpha[i, j, 1] = oldAlpha[j, oldAlpha.GetLength(1) - i - 1, 1];
                }
            }
        }
        else
        {
            for (int i = 0; i < newAlpha.GetLength(0); i++)
            {
                for (int j = 0; j < newAlpha.GetLength(1); j++)
                {
                    newAlpha[i, j, 0] = oldAlpha[oldAlpha.GetLength(0) - j - 1, i, 0];
                    newAlpha[i, j, 1] = oldAlpha[oldAlpha.GetLength(0) - j - 1, i, 1];
                }
            }
        }
        alphaLandData.setData(newAlpha, "alpha");
        alphaLandData.setLayer();
        changeLandLayer();
    }

    public void rotateGroundmap(bool CW) //Todo: Have it all automagically work with a button. rn my unity editor shits the bed trying to paint so many layers at once lol.
    {
        LandData groundLandData = GameObject.FindGameObjectWithTag("Land").transform.Find("Ground").GetComponent<LandData>();
        float[,,] newGround = TypeConverter.singleToMulti(groundLandData.splatMap, 8);
        float[,,] oldGround= TypeConverter.singleToMulti(groundLandData.splatMap, 8);

        if (CW)
        {
            for (int i = 0; i < newGround.GetLength(0); i++)
            {
                for (int j = 0; j < newGround.GetLength(1); j++)
                {
                    newGround[i, j, 0] = oldGround[j, oldGround.GetLength(1) - i - 1, 0];
                    newGround[i, j, 1] = oldGround[j, oldGround.GetLength(1) - i - 1, 1];
                    newGround[i, j, 2] = oldGround[j, oldGround.GetLength(1) - i - 1, 2];
                    newGround[i, j, 3] = oldGround[j, oldGround.GetLength(1) - i - 1, 3];
                    newGround[i, j, 4] = oldGround[j, oldGround.GetLength(1) - i - 1, 4];
                    newGround[i, j, 5] = oldGround[j, oldGround.GetLength(1) - i - 1, 5];
                    newGround[i, j, 6] = oldGround[j, oldGround.GetLength(1) - i - 1, 6];
                    newGround[i, j, 7] = oldGround[j, oldGround.GetLength(1) - i - 1, 7];
                }
            }
        }
        else
        {
            for (int i = 0; i < newGround.GetLength(0); i++)
            {
                for (int j = 0; j < newGround.GetLength(1); j++)
                {
                    newGround[i, j, 0] = oldGround[oldGround.GetLength(0) - j - 1, i, 0];
                    newGround[i, j, 1] = oldGround[oldGround.GetLength(0) - j - 1, i, 1];
                    newGround[i, j, 2] = oldGround[oldGround.GetLength(0) - j - 1, i, 2];
                    newGround[i, j, 3] = oldGround[oldGround.GetLength(0) - j - 1, i, 3];
                    newGround[i, j, 4] = oldGround[oldGround.GetLength(0) - j - 1, i, 4];
                    newGround[i, j, 5] = oldGround[oldGround.GetLength(0) - j - 1, i, 5];
                    newGround[i, j, 6] = oldGround[oldGround.GetLength(0) - j - 1, i, 6];
                    newGround[i, j, 7] = oldGround[oldGround.GetLength(0) - j - 1, i, 7];
                }
            }
        }
        groundLandData.setData(newGround, "ground");
        groundLandData.setLayer();
        changeLandLayer();
    }
    public void rotateBiomemap(bool CW) //Todo: Have it all automagically work with a button.
    {
        LandData biomeLandData = GameObject.FindGameObjectWithTag("Land").transform.Find("Biome").GetComponent<LandData>();
        float[,,] newBiome = TypeConverter.singleToMulti(biomeLandData.splatMap, 4);
        float[,,] oldBiome = TypeConverter.singleToMulti(biomeLandData.splatMap, 4);

        if (CW)
        {
            for (int i = 0; i < newBiome.GetLength(0); i++)
            {
                for (int j = 0; j < newBiome.GetLength(1); j++)
                {
                    newBiome[i, j, 0] = oldBiome[j, oldBiome.GetLength(1) - i - 1, 0];
                    newBiome[i, j, 1] = oldBiome[j, oldBiome.GetLength(1) - i - 1, 1];
                    newBiome[i, j, 2] = oldBiome[j, oldBiome.GetLength(1) - i - 1, 2];
                    newBiome[i, j, 3] = oldBiome[j, oldBiome.GetLength(1) - i - 1, 3];
                }
            }
        }
        else
        {
            for (int i = 0; i < newBiome.GetLength(0); i++)
            {
                for (int j = 0; j < newBiome.GetLength(1); j++)
                {
                    newBiome[i, j, 0] = oldBiome[oldBiome.GetLength(0) - j - 1, i, 0];
                    newBiome[i, j, 1] = oldBiome[oldBiome.GetLength(0) - j - 1, i, 1];
                    newBiome[i, j, 2] = oldBiome[oldBiome.GetLength(0) - j - 1, i, 2];
                    newBiome[i, j, 3] = oldBiome[oldBiome.GetLength(0) - j - 1, i, 3];
                }
            }
        }
        biomeLandData.setData(newBiome, "biome");
        biomeLandData.setLayer();
        changeLandLayer();
    }
    public void rotateTopologymap(bool CW) //Make it work
    {
        LandData topologyLandData = GameObject.FindGameObjectWithTag("Land").transform.Find("Topology").GetComponent<LandData>();
        float[,,] newTopology = TypeConverter.singleToMulti(topologyLandData.splatMap, 2);
        float[,,] oldTopology = TypeConverter.singleToMulti(topologyLandData.splatMap, 2);

        if (CW)
        {
            for (int i = 0; i < newTopology.GetLength(0); i++)
            {
                for (int j = 0; j < newTopology.GetLength(1); j++)
                {
                    newTopology[i, j, 0] = oldTopology[j, oldTopology.GetLength(1) - i - 1, 0];
                    newTopology[i, j, 1] = oldTopology[j, oldTopology.GetLength(1) - i - 1, 1];
                }
            }
        }
        else
        {
            for (int i = 0; i < newTopology.GetLength(0); i++)
            {
                for (int j = 0; j < newTopology.GetLength(1); j++)
                {
                    newTopology[i, j, 0] = oldTopology[oldTopology.GetLength(0) - j - 1, i, 0];
                    newTopology[i, j, 1] = oldTopology[oldTopology.GetLength(0) - j - 1, i, 1];
                }
            }
        }
        topologyLandData.setData(newTopology, "topology");
        topologyLandData.setLayer();
        changeLandLayer();
    }

    public void rotateObjects(bool CW) //Needs prefabs in scene to be all at Vector3.Zero to work.
    {
        var prefabRotate = GameObject.FindGameObjectWithTag("Prefabs");
        if (CW)
        {
            prefabRotate.transform.Rotate(0, 90, 0, Space.World);
        }
        else
        {
            prefabRotate.transform.Rotate(0, -90, 0, Space.World);
        }
    }


    public void flipHeightmap()
    {
        Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
        float[,] heightMap = land.terrainData.GetHeights(0, 0, land.terrainData.heightmapWidth, land.terrainData.heightmapHeight);
        land.terrainData.SetHeights(0, 0, MapTransformations.flip(heightMap));
    }
    public void transposeHeightmap()
    {
        Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
        float[,] heightMap = land.terrainData.GetHeights(0, 0, land.terrainData.heightmapWidth, land.terrainData.heightmapHeight);
        land.terrainData.SetHeights(0, 0, MapTransformations.transpose(heightMap));
    }

    private void loadMapInfo(MapInfo terrains)
    {
        if (MapIO.topology == null)
            topology = GameObject.FindGameObjectWithTag("Topology").GetComponent<TopologyMesh>();
        
        cleanUpMap();

        var terrainPosition = 0.5f * terrains.size;


        LandData groundLandData = GameObject.FindGameObjectWithTag("Land").transform.Find("Ground").GetComponent<LandData>();
        LandData biomeLandData = GameObject.FindGameObjectWithTag("Land").transform.Find("Biome").GetComponent<LandData>();
        LandData alphaLandData = GameObject.FindGameObjectWithTag("Land").transform.Find("Alpha").GetComponent<LandData>();
        LandData topologyLandData = GameObject.FindGameObjectWithTag("Land").transform.Find("Topology").GetComponent<LandData>();

        Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
        Terrain water = GameObject.FindGameObjectWithTag("Water").GetComponent<Terrain>();

        land.transform.position = terrainPosition;
        water.transform.position = terrainPosition;


        topology.InitMesh(terrains.topology);
       

        land.terrainData.heightmapResolution = terrains.resolution;
        land.terrainData.size = terrains.size;

        water.terrainData.heightmapResolution = terrains.resolution;
        water.terrainData.size = terrains.size;

        land.terrainData.SetHeights(0, 0, terrains.land.heights);
        water.terrainData.SetHeights(0, 0, terrains.water.heights);

        land.terrainData.alphamapResolution = terrains.resolution;
        land.terrainData.baseMapResolution = terrains.resolution - 1;
        //land.terrainData.SetDetailResolution(terrains.resolution - 1, 8);
        water.terrainData.alphamapResolution = terrains.resolution;
        water.terrainData.baseMapResolution = terrains.resolution - 1;
        //water.terrainData.SetDetailResolution(terrains.resolution - 1, 8);

        land.GetComponent<UpdateTerrainValues>().setSize(terrains.size);
        water.GetComponent<UpdateTerrainValues>().setSize(terrains.size);
        land.GetComponent<UpdateTerrainValues>().setPosition(Vector3.zero);
        water.GetComponent<UpdateTerrainValues>().setPosition(Vector3.zero);

        groundLandData.setData(terrains.splatMap, "ground");

        biomeLandData.setData(terrains.biomeMap, "biome");

        alphaLandData.setData(terrains.alphaMap, "alpha");

        topologyLandData.setData(topology.getSplatMap((int)topologyLayer), "topology");
        changeLandLayer();

        Transform prefabsParent = GameObject.FindGameObjectWithTag("Prefabs").transform;
        GameObject defaultObj = Resources.Load<GameObject>("Prefabs/DefaultPrefab");
        
		for (int i = 0; i < terrains.prefabData.Length; i++)
        {
            GameObject newObj = spawnPrefab(defaultObj, terrains.prefabData[i], prefabsParent);
            newObj.GetComponent<PrefabDataHolder>().prefabData = terrains.prefabData[i];
        }
		
		

        Transform pathsParent = GameObject.FindGameObjectWithTag("Paths").transform;
        GameObject pathObj = Resources.Load<GameObject>("Paths/Path");
        for (int i = 0; i < terrains.pathData.Length; i++)
        {
            Vector3 averageLocation = Vector3.zero;
            for (int j = 0; j < terrains.pathData[i].nodes.Length; j++)
            {
                averageLocation += terrains.pathData[i].nodes[j];
            }
            averageLocation /= terrains.pathData[i].nodes.Length;
            GameObject newObject = Instantiate(pathObj, averageLocation + terrainPosition, Quaternion.identity, pathsParent);
            newObject.GetComponent<PathDataHolder>().pathData = terrains.pathData[i];
            newObject.GetComponent<PathDataHolder>().offset = terrainPosition;
        }
    }

    public void Load(WorldSerialization blob)
    {
        Debug.Log("Map hash: " + blob.Checksum);
        WorldConverter.MapInfo terrains = WorldConverter.worldToTerrain(blob);
        loadMapInfo(terrains);
    }

	
	//Kilgoar Paste
	public void Paste(WorldSerialization blob, int x, int y)
	{
		
		
		
		

		
		
		selectedLandLayer = null;
		WorldConverter.MapInfo terrains = WorldConverter.worldToTerrain(blob);
		
		
		
		//handle for heightmap terrainmaps
		Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
		Terrain water = GameObject.FindGameObjectWithTag("Water").GetComponent<Terrain>();
        
		
				
		
		var terrainPosition = 0.5f * terrains.size;
		
		
		//other arrays
		float[,] pasteMap = terrains.land.heights;
		float[,] pasteWater = terrains.water.heights;
		float[,,] pSplat = terrains.splatMap;
		float[,,] pBiome = terrains.biomeMap;
		float[,,] pAlpha = terrains.alphaMap;
		
		
		
		land.transform.position = terrainPosition;
        water.transform.position = terrainPosition;
		
		//heightmap arrays 
		
		
		float[,] baseMap = land.terrainData.GetHeights(0, 0, land.terrainData.heightmapWidth, land.terrainData.heightmapHeight);
		float[,] baseWater = water.terrainData.GetHeights(0,0, water.terrainData.heightmapWidth, land.terrainData.heightmapHeight);
		
		
		
		
		
		if (MapIO.topology == null)
		{
			topology = GameObject.FindGameObjectWithTag("Topology").GetComponent<TopologyMesh>();
			
		}	

		//handles for topology terrainmaps from file & memory
		TerrainMap<int> topTerrainMap = topology.getTerrainMap();
		TerrainMap<int> pTerrainMap = terrains.topology;
		
		//handles for editor layers
		LandData groundLandData = GameObject.FindGameObjectWithTag("Land").transform.Find("Ground").GetComponent<LandData>();
        LandData biomeLandData = GameObject.FindGameObjectWithTag("Land").transform.Find("Biome").GetComponent<LandData>();
        LandData alphaLandData = GameObject.FindGameObjectWithTag("Land").transform.Find("Alpha").GetComponent<LandData>();
        //fuck the haters
		//LandData topologyLandData = GameObject.FindGameObjectWithTag("Land").transform.Find("Topology").GetComponent<LandData>();
		
		//arrays	
		float[,,] newGround = TypeConverter.singleToMulti(groundLandData.splatMap, 8);
		float[,,] newBiome = TypeConverter.singleToMulti(biomeLandData.splatMap, 4);
		float[,,] newAlpha = TypeConverter.singleToMulti(alphaLandData.splatMap, 2);
		
		Debug.LogError(pTerrainMap.res);
		//overwrite old topologies with new ones
		for (int i = 0; i < pTerrainMap.res; i++)
		{
			for (int j = 0; j < pTerrainMap.res; j++)
			{
			topTerrainMap[i + x, j + y] = pTerrainMap[i, j];
			}
        }
		
		//it's just magic
		topology.InitMesh(topTerrainMap);
		
		Debug.LogError(pBiome.GetLength(0));
		//combine other arrays
		for (int i = 0; i < pBiome.GetLength(0); i++)
        {
            for (int j = 0; j < pBiome.GetLength(1); j++)
            {
                newBiome[i + x, j + y, 0] = pBiome[i, j, 0];
				newBiome[i + x, j + y, 1] = pBiome[i, j, 1];                
				newBiome[i + x, j + y, 2] = pBiome[i, j, 2];
				newBiome[i + x, j + y, 3] = pBiome[i, j, 3];				
            }
        }
        
		for (int i = 0; i < pBiome.GetLength(0); i++)
        {
            for (int j = 0; j < pBiome.GetLength(1); j++)
            {
                newGround[i + x, j + y, 0] = pSplat[i, j, 0];
				newGround[i + x, j + y, 1] = pSplat[i, j, 1];                
				newGround[i + x, j + y, 2] = pSplat[i, j, 2];
				newGround[i + x, j + y, 3] = pSplat[i, j, 3];
				newGround[i + x, j + y, 4] = pSplat[i, j, 4];
				newGround[i + x, j + y, 5] = pSplat[i, j, 5];
				newGround[i + x, j + y, 6] = pSplat[i, j, 6];
				newGround[i + x, j + y, 7] = pSplat[i, j, 7];				
            }
        }
        
		for (int i = 0; i < pAlpha.GetLength(0); i++)
        {
            for (int j = 0; j < pAlpha.GetLength(1); j++)
            {
                newAlpha[i + x, j + y, 0] = pAlpha[i, j, 0];
				newAlpha[i + x, j + y, 1] = pAlpha[i, j, 1];                
				
            }
        }
		
		//arrays into editor layers, heightmaps
		
		land.terrainData.SetHeights(0, 0, MapTransformations.overwrite(pasteMap, baseMap, x, y));
		water.terrainData.SetHeights(0, 0, MapTransformations.overwrite(pasteWater, baseWater, x , y));
				
		groundLandData.setData(newGround, "ground");
		biomeLandData.setData(newBiome, "biome");
        alphaLandData.setData(newAlpha, "alpha");
		
		Transform prefabsParent = GameObject.FindGameObjectWithTag("Prefabs").transform;
        GameObject defaultObj = Resources.Load<GameObject>("Prefabs/DefaultPrefab");
        
		//what is this
		float c = (pTerrainMap.res / 2f) - (topTerrainMap.res / 2f);
		Debug.LogError(c);
		for (int i = 0; i < terrains.prefabData.Length; i++)
        {
            terrains.prefabData[i].position.x = terrains.prefabData[i].position.x  + c*2f + y*2f;
			terrains.prefabData[i].position.z = terrains.prefabData[i].position.z  + c*2f + x*2f;
			GameObject newObj = spawnPrefab(defaultObj, terrains.prefabData[i], prefabsParent);
            newObj.GetComponent<PrefabDataHolder>().prefabData = terrains.prefabData[i];
			
			
        }
		
		Transform pathsParent = GameObject.FindGameObjectWithTag("Paths").transform;
        GameObject pathObj = Resources.Load<GameObject>("Paths/Path");
        for (int i = 0; i < terrains.pathData.Length; i++)
        {
            Vector3 averageLocation = Vector3.zero;
            for (int j = 0; j < terrains.pathData[i].nodes.Length; j++)
            {
                averageLocation += terrains.pathData[i].nodes[j];
				terrains.pathData[i].nodes[j].x = terrains.pathData[i].nodes[j].x  + c*2f + y*2f;
				terrains.pathData[i].nodes[j].z = terrains.pathData[i].nodes[j].z  + c*2f + x*2f;
            }
            
			averageLocation /= terrains.pathData[i].nodes.Length;
            
			GameObject newObject = Instantiate(pathObj, averageLocation + terrainPosition, Quaternion.identity, pathsParent);
            newObject.GetComponent<PathDataHolder>().pathData = terrains.pathData[i];
            newObject.GetComponent<PathDataHolder>().offset = terrainPosition;
        }
		
		//also, fuck the haters
		changeLandLayer();
		
		/*
		VISIT HTTP://CHRONICLE.SU FOR ALL THINGS FULFILLING AND TRUE
		
		
		
		
		*/
	}

	
	//kilgoar's overloaded paster
	public void Paste(WorldSerialization blob, int x, int y, int x1, int y1, int l, int w)
	{
		
		
		
		l = l / 2;
		w = w / 2;
		x = x / 2;
		y = y / 2;
		x1=x1/2;
		y1=y1/2;
		
		
		selectedLandLayer = null;
		WorldConverter.MapInfo terrains = WorldConverter.worldToTerrain(blob);
		
		
		
		//handle for heightmap terrainmaps
		Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
		Terrain water = GameObject.FindGameObjectWithTag("Water").GetComponent<Terrain>();
        
		
				
		
		var terrainPosition = 0.5f * terrains.size;
		
		//Debug.LogError(terrainPosition);
		//other arrays
		float[,] pasteMap = terrains.land.heights;
		float[,] pasteWater = terrains.water.heights;
		float[,,] pSplat = terrains.splatMap;
		float[,,] pBiome = terrains.biomeMap;
		float[,,] pAlpha = terrains.alphaMap;
		
		
		
		land.transform.position = terrainPosition;
        water.transform.position = terrainPosition;
		
		
		
		
		//land.terrainData.size.x
		
		//heightmap array
		float[,] baseMap = land.terrainData.GetHeights(0, 0, land.terrainData.heightmapWidth, land.terrainData.heightmapHeight);
		float[,] baseWater = water.terrainData.GetHeights(0,0, water.terrainData.heightmapWidth, land.terrainData.heightmapHeight);
		
		if (MapIO.topology == null)
		{
			topology = GameObject.FindGameObjectWithTag("Topology").GetComponent<TopologyMesh>();
			//land.terrainData.SetHeights(0, 0, MapTransformations.setMap(baseMap, terrainPosition.y));
			//water.terrainData.SetHeights(0, 0, MapTransformations.setMap(baseWater, terrainPosition.y - 2));	
		}	

		//handles for topology terrainmaps from file & memory
		
		
		TerrainMap<int> topTerrainMap = topology.getTerrainMap();
		
		TerrainMap<int> pTerrainMap = terrains.topology;
		
		//handles for editor layers
		LandData groundLandData = GameObject.FindGameObjectWithTag("Land").transform.Find("Ground").GetComponent<LandData>();
        LandData biomeLandData = GameObject.FindGameObjectWithTag("Land").transform.Find("Biome").GetComponent<LandData>();
        LandData alphaLandData = GameObject.FindGameObjectWithTag("Land").transform.Find("Alpha").GetComponent<LandData>();
        //fuck the haters
		//LandData topologyLandData = GameObject.FindGameObjectWithTag("Land").transform.Find("Topology").GetComponent<LandData>();
		
		//arrays	
		float[,,] newGround = TypeConverter.singleToMulti(groundLandData.splatMap, 8);
		float[,,] newBiome = TypeConverter.singleToMulti(biomeLandData.splatMap, 4);
		float[,,] newAlpha = TypeConverter.singleToMulti(alphaLandData.splatMap, 2);
		
		//Debug.LogError(pTerrainMap.res);
		
		//overwrite old topologies with new ones
	
		
		for (int i = x1; i < l + x1; i++)
		{
			for (int j = y1; j < w + y1; j++)
			{
			topTerrainMap[i + x - x1, j + y - y1] = pTerrainMap[i, j];
			}
        }
		
		//it's just magic
		topology.InitMesh(topTerrainMap);
		
		//Debug.LogError(pBiome.GetLength(0));
		//combine other arrays
		for (int i = x1; i < l + x1; i++)
        {
            for (int j = y1; j < w + y1; j++)
            {
                //write                               read				
				newBiome[i + x - x1, j + y - y1, 0] = pBiome[i, j, 0];
				newBiome[i + x - x1, j + y - y1, 1] = pBiome[i, j, 1];                
				newBiome[i + x - x1, j + y - y1, 2] = pBiome[i, j, 2];
				newBiome[i + x - x1, j + y - y1, 3] = pBiome[i, j, 3];				
            }
        }
        
		for (int i = x1; i < l + x1; i++)
        {
            for (int j = y1; j < w + y1; j++)
            {
                newGround[i + x - x1, j + y - y1, 0] = pSplat[i, j, 0];
				newGround[i + x - x1, j + y - y1, 1] = pSplat[i, j, 1];                
				newGround[i + x - x1, j + y - y1, 2] = pSplat[i, j, 2];
				newGround[i + x - x1, j + y - y1, 3] = pSplat[i, j, 3];
				newGround[i + x - x1, j + y - y1, 4] = pSplat[i, j, 4];
				newGround[i + x - x1, j + y - y1, 5] = pSplat[i, j, 5];
				newGround[i + x - x1, j + y - y1, 6] = pSplat[i, j, 6];
				newGround[i + x - x1, j + y - y1, 7] = pSplat[i, j, 7];				
            }
        }
        
		//broken somehow
		for (int i = x1; i < l + x1; i++)
        {
            for (int j = y1; j < w + y1; j++)
            {
                
				newAlpha[i + x - x1, j + y - y1, 0] = pAlpha[i,j,0];
				newAlpha[i + x - x1, j + y - y1, 1] = pAlpha[i,j,1];
				
				
				
            }
        }
		
		//arrays into editor layers, heightmaps
		
		
		land.terrainData.SetHeights(0, 0, MapTransformations.overwrite(pasteMap, baseMap, x, y, x1, y1, w, l));
		water.terrainData.SetHeights(0, 0, MapTransformations.overwrite(pasteWater, baseWater, x , y, x1, y1, w, l));
		
		
			
		groundLandData.setData(newGround, "ground");
		biomeLandData.setData(newBiome, "biome");
        alphaLandData.setData(newAlpha, "alpha");
		
		
		
		Transform prefabsParent = GameObject.FindGameObjectWithTag("Prefabs").transform;
        GameObject defaultObj = Resources.Load<GameObject>("Prefabs/DefaultPrefab");
        
		//conversion ratio from terrain numerology to prefab hatred
		
		
		float c = (pTerrainMap.res / 2f) - (topTerrainMap.res / 2f);
		//float cz = ((topTerrainMap.res+1) /2f) / 1000;
		float cz = 1;
		float cz1 = topTerrainMap.res;
		Debug.LogError(c);
		for (int i = 0; i < terrains.prefabData.Length; i++)
        {

			terrains.prefabData[i].position.x = terrains.prefabData[i].position.x * cz + c*2f + y*2f - y1*2f;
			terrains.prefabData[i].position.z = terrains.prefabData[i].position.z * cz + c*2f + x*2f - x1*2f;			
						
						
			
			if ((((terrains.prefabData[i].position.z)*cz + cz1)> x*2f) &&
				(((terrains.prefabData[i].position.z)*cz + cz1)< x*2f + w*2f) &&
				(((terrains.prefabData[i].position.x)*cz + cz1)> y*2f) && 
				(((terrains.prefabData[i].position.x)*cz + cz1)< y*2f + l*2f))
			{
			
					GameObject newObj = spawnPrefab(defaultObj, terrains.prefabData[i], prefabsParent);
					newObj.GetComponent<PrefabDataHolder>().prefabData = terrains.prefabData[i];
			}
				
		
        }
		
		
		/*
		Transform pathsParent = GameObject.FindGameObjectWithTag("Paths").transform;
        GameObject pathObj = Resources.Load<GameObject>("Paths/Path");
        for (int i = 0; i < terrains.pathData.Length; i++)
        {
            Vector3 averageLocation = Vector3.zero;
            for (int j = 0; j < terrains.pathData[i].nodes.Length; j++)
            {
                averageLocation += terrains.pathData[i].nodes[j];
				terrains.pathData[i].nodes[j].x = terrains.pathData[i].nodes[j].x  + y*2f - y1*2f;
				terrains.pathData[i].nodes[j].z = terrains.pathData[i].nodes[j].z  + x*2f - x1*2f;
            }
            
			averageLocation /= terrains.pathData[i].nodes.Length;
            
			//see if each node is in region to paste
			if ((terrains.pathData[i].nodes[j].x < y*2f + l*2f) && (terrains.pathData[i].nodes[j].x > y*2f) && (terrains.pathData[i].nodes[j].z < x*2f + w*2f) && (terrains.pathData[i].nodes[j].z > x*2f))
			{
				GameObject newObject = Instantiate(pathObj, averageLocation + terrainPosition, Quaternion.identity, pathsParent);
				newObject.GetComponent<PathDataHolder>().pathData = terrains.pathData[i];
				newObject.GetComponent<PathDataHolder>().offset = terrainPosition;
			}
        }
		*/
		
		//HEL LLOL
		
		changeLandLayer();
		
		//selectedLandLayer.setLayer();
		
		/*
		VISIT HTTP://CHRONICLE.SU FOR ALL THINGS FULFILLING AND TRUE
		
		
		
		
		*/
	}
	
	
	//same as Paste except heightmap is blended using biome data
	public void pasteMonument(WorldSerialization blob, int x, int y, int dim)
	{
		
		x=x/2;
		y=y/2;
		
		

		
		
		selectedLandLayer = null;
		WorldConverter.MapInfo terrains = WorldConverter.worldToTerrain(blob);
		
		
		
		//handle for heightmap terrainmaps
		Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
		Terrain water = GameObject.FindGameObjectWithTag("Water").GetComponent<Terrain>();
        
		
				
		
		var terrainPosition = 0.5f * terrains.size;
		
		
		//other arrays
		float[,] pasteMap = terrains.land.heights;
		float[,] pasteWater = terrains.water.heights;
		float[,,] pSplat = terrains.splatMap;
		float[,,] pBiome = terrains.biomeMap;
		
		Debug.LogError(pBiome.GetLength(0));
		
		float[,,] pAlpha = terrains.alphaMap;
		
		
		
		land.transform.position = terrainPosition;
        water.transform.position = terrainPosition;
		
		//heightmap arrays 
		
		
		float[,] baseMap = land.terrainData.GetHeights(0, 0, land.terrainData.heightmapWidth, land.terrainData.heightmapHeight);
		float[,] baseWater = water.terrainData.GetHeights(0,0, water.terrainData.heightmapWidth, land.terrainData.heightmapHeight);
		
		
		
		
		
		if (MapIO.topology == null)
		{
			topology = GameObject.FindGameObjectWithTag("Topology").GetComponent<TopologyMesh>();
			
		}	

		//handles for topology terrainmaps from file & memory
		TerrainMap<int> topTerrainMap = topology.getTerrainMap();
		TerrainMap<int> pTerrainMap = terrains.topology;
		
		//handles for editor layers
		LandData groundLandData = GameObject.FindGameObjectWithTag("Land").transform.Find("Ground").GetComponent<LandData>();
        LandData biomeLandData = GameObject.FindGameObjectWithTag("Land").transform.Find("Biome").GetComponent<LandData>();
        LandData alphaLandData = GameObject.FindGameObjectWithTag("Land").transform.Find("Alpha").GetComponent<LandData>();
        //fuck the haters
		//LandData topologyLandData = GameObject.FindGameObjectWithTag("Land").transform.Find("Topology").GetComponent<LandData>();
		
		//arrays	
		float[,,] newGround = TypeConverter.singleToMulti(groundLandData.splatMap, 8);
		float[,,] newBiome = TypeConverter.singleToMulti(biomeLandData.splatMap, 4);
		float[,,] newAlpha = TypeConverter.singleToMulti(alphaLandData.splatMap, 2);
		
		Debug.LogError(pTerrainMap.res);
		
		
		//mash topologies together

		for (int i = 0; i < dim; i++)
		{
			for (int j = 0; j < dim; j++)
			{
			//topTerrainMap[i + x, j + y] = pTerrainMap[i, j];
				
				if (pBiome[i, j,0] > 0.5f)
				{	
					topTerrainMap[i + x, j + y] = pTerrainMap[i, j];
				}

			}
        }
		
		//it's just magic
		topology.InitMesh(topTerrainMap);
		
		Debug.LogError(pBiome.GetLength(0));
		//combine other arrays
		
		/* do not write biome. other data needs interpolation
		for (int i = 0; i < pBiome.GetLength(0); i++)
        {
            for (int j = 0; j < pBiome.GetLength(1); j++)
            {
                newBiome[i + x, j + y, 0] = pBiome[i, j, 0];
				newBiome[i + x, j + y, 1] = pBiome[i, j, 1];                
				newBiome[i + x, j + y, 2] = pBiome[i, j, 2];
				newBiome[i + x, j + y, 3] = pBiome[i, j, 3];				
            }
        }
        */
		
		for (int i = 0; i < dim; i++)
        {
            for (int j = 0; j < dim; j++)
            {
				for (int k = 0; k < 8; k++)
				{
					newGround[i + x, j + y, k] = Mathf.Lerp(newGround[i+x,j+y,k], pSplat[i,j,k], pBiome[i,j,0]);
				}
                /*
				newGround[i + x, j + y, 0] = pSplat[i, j, 0];
				newGround[i + x, j + y, 1] = pSplat[i, j, 1];                
				newGround[i + x, j + y, 2] = pSplat[i, j, 2];
				newGround[i + x, j + y, 3] = pSplat[i, j, 3];
				newGround[i + x, j + y, 4] = pSplat[i, j, 4];
				newGround[i + x, j + y, 5] = pSplat[i, j, 5];
				newGround[i + x, j + y, 6] = pSplat[i, j, 6];
				newGround[i + x, j + y, 7] = pSplat[i, j, 7];				
				*/
            }
        }
        
		
		//mash alpha layers together
		for (int i = 0; i < dim; i++)
        {
            for (int j = 0; j < dim; j++)
            {
                				
				if (pBiome[i,j,0] > 0.5f)
				{
					newAlpha[i + x, j + y, 0] = pAlpha[i, j, 0];
					newAlpha[i + x, j + y, 1] = pAlpha[i, j, 1];
				}
				
            }
        }
		
		
		//interpolate heightmaps in super clever biome mask
		for (int i = 0; i < dim; i++)
        {
            for (int j = 0; j < dim; j++)
            {
                //baseMap[i + x, j + y] = (baseMap[i + x, j + y]*(1-pBiome[i, j, 0])) + (pBiome[i, j, 0] * pasteMap[i, j]);
				baseMap[i + x, j + y] = Mathf.Lerp(baseMap[i+x, j+y], pasteMap[i,j], pBiome[i,j,0]);
				if ((j == dim-1))
				{
					
					baseMap[i + x, j + y+1] = Mathf.Lerp(baseMap[i+x, j+y], pasteMap[i,j], pBiome[i,j,0]);
				}
				
				if ((i == dim-1))
				{
					
					baseMap[i + x+1, j + y] = Mathf.Lerp(baseMap[i+x, j+y], pasteMap[i,j], pBiome[i,j,0]);
				}
				
            }
        }
		
		land.terrainData.SetHeights(0,0,baseMap);
		
		groundLandData.setData(newGround, "ground");
		biomeLandData.setData(newBiome, "biome");
        alphaLandData.setData(newAlpha, "alpha");
		
		Transform prefabsParent = GameObject.FindGameObjectWithTag("Prefabs").transform;
		GameObject defaultObj = Resources.Load<GameObject>("Prefabs/DefaultPrefab");
        
		//slit your wrists because this never works
		float c = ((topTerrainMap.res)*1f)-(topTerrainMap.res*1f-x*1f);
		//float cz= ((topTerrainMap.res) /2f) / 1000;
		float cz = 1;
		//float c= 1000/cf-3500/2f;
		//nothing fucking works!!!!
		Debug.LogError(c);
		for (int i = 0; i < terrains.prefabData.Length; i++)
        {
            //2*(size-x)+size*2
			
			terrains.prefabData[i].position.x = terrains.prefabData[i].position.x+y*2f;
			terrains.prefabData[i].position.z = terrains.prefabData[i].position.z+x*2f;
			
			GameObject newObj = spawnPrefab(defaultObj, terrains.prefabData[i], prefabsParent);
            newObj.GetComponent<PrefabDataHolder>().prefabData = terrains.prefabData[i];
			
			
        }
		
		Transform pathsParent = GameObject.FindGameObjectWithTag("Paths").transform;
        GameObject pathObj = Resources.Load<GameObject>("Paths/Path");
        for (int i = 0; i < terrains.pathData.Length; i++)
        {
            Vector3 averageLocation = Vector3.zero;
            for (int j = 0; j < terrains.pathData[i].nodes.Length; j++)
            {
                averageLocation += terrains.pathData[i].nodes[j];
				terrains.pathData[i].nodes[j].x = terrains.pathData[i].nodes[j].x + y*2f;
				terrains.pathData[i].nodes[j].z = terrains.pathData[i].nodes[j].z + x*2f;
            }
            
			averageLocation /= terrains.pathData[i].nodes.Length;
            
			GameObject newObject = Instantiate(pathObj, averageLocation + terrainPosition, Quaternion.identity, pathsParent);
            newObject.GetComponent<PathDataHolder>().pathData = terrains.pathData[i];
            newObject.GetComponent<PathDataHolder>().offset = terrainPosition;
        }
		
		//also, fuck the haters
		changeLandLayer();
		
		/*
		VISIT HTTP://CHRONICLE.SU FOR ALL THINGS FULFILLING AND TRUE
		
		
		
		
		*/
	}
	
    public void loadEmpty(int size)
    {
        loadMapInfo(WorldConverter.emptyWorld(size));
    }

    public void Save(string path)
    {
        if(selectedLandLayer != null)
            selectedLandLayer.save();
        saveTopologyLayer();

        if (GameObject.FindGameObjectWithTag("Water") == null)
            Debug.Log("Water not enabled");
        if (GameObject.FindGameObjectWithTag("Land") == null)
            Debug.Log("Land not enabled");
        Terrain terrain = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
        Terrain water = GameObject.FindGameObjectWithTag("Water").GetComponent<Terrain>();
        

        WorldSerialization world = WorldConverter.terrainToWorld(terrain, water);
        
        world.Save(path);
        //Debug.Log("Map hash: " + world.Checksum);
    }

    public void resizeTerrain(Vector3 size)
    {
        loadMapInfo(WorldConverter.emptyWorld((int)size.x));
    }
        

    public void Awake()
    {
        
        FileSystem.iface = new FileSystem_AssetBundles(@"C:\Program Files (x86)\Steam\steamapps\common\RustStaging\Bundles\Bundles");
       
        Transform prefabsParent = GameObject.FindGameObjectWithTag("Prefabs").transform;
        foreach (PrefabDataHolder pdh in GameObject.FindObjectsOfType<PrefabDataHolder>())
        {
            if (!pdh.spawnOnPlay)
                continue;

            //Debug.Log(StringPool.Get((pdh.prefabData.id)));
            
            GameObject g = FileSystem.Load<GameObject>(StringPool.Get((pdh.prefabData.id)));
            
            GameObject newObject = spawnPrefab(g, pdh.prefabData, prefabsParent);

            PrefabDataHolder prefabData = newObject.GetComponent<PrefabDataHolder>();
            if (prefabData == null)
            {
                prefabData = newObject.AddComponent<PrefabDataHolder>();
            }
            
            prefabData.prefabData = pdh.prefabData;

            Destroy(pdh.gameObject);
        }
        
    }

    void OnApplicationQuit()
    {
        Transform prefabsParent = GameObject.FindGameObjectWithTag("Prefabs").transform;
        foreach (PrefabDataHolder pdh in GameObject.FindObjectsOfType<PrefabDataHolder>())
        {
            GameObject defaultObj = Resources.Load<GameObject>("Prefabs/DefaultPrefab");
            GameObject newObject = spawnPrefab(defaultObj, pdh.prefabData, prefabsParent);
            

            PrefabDataHolder prefabData = newObject.GetComponent<PrefabDataHolder>();
            if (prefabData == null)
            {
                newObject.AddComponent<PrefabDataHolder>();
            }
            prefabData.prefabData = pdh.prefabData;

            Destroy(pdh.gameObject);
        }
    }
}
