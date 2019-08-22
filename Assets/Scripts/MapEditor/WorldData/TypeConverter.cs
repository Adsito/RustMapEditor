using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class TypeConverter {

    public static float[,] shortMapToFloatArray(TerrainMap<short> terrainMap)
    {
        float[,] heights = new float[terrainMap.res, terrainMap.res];
        for (int i = 0; i < heights.GetLength(0); i++)
        {
            for (int j = 0; j < heights.GetLength(1); j++)
            {
                heights[i, j] = BitUtility.Short2Float(terrainMap[i, j]);
            }
        }
        return heights;
    }

    public static byte[] floatArrayToByteArray(float[,] floatArray)
    {
        short[] shortArray = new short[floatArray.GetLength(0) * floatArray.GetLength(1)];

        for (int i = 0; i < floatArray.GetLength(0); i++)
        {
            for (int j = 0; j < floatArray.GetLength(1); j++)
            {
                shortArray[(i * floatArray.GetLength(0)) + j] = BitUtility.Float2Short(floatArray[i, j]);
            }
        }

        byte[] byteArray = new byte[shortArray.Length * 2];

        Buffer.BlockCopy(shortArray, 0, byteArray, 0, byteArray.Length);

        return byteArray;
    }

    public static float[] multiToSingle(float[,,] multiArray, int size)
    {
        float[] singleArray = new float[multiArray.GetLength(0) * multiArray.GetLength(1) * size];

        for (int i = 0; i < multiArray.GetLength(0); i++)
        {
            for (int j = 0; j < multiArray.GetLength(1); j++)
            {
                for (int k = 0; k < size; k++)
                {
                    singleArray[i * multiArray.GetLength(1) * size + (j * size + k)] = multiArray[i, j, k];
                }
            }
        }
        
        return singleArray;
    }

    public static float[] multiToSingle(float[,,] multiArray)
    {
        return multiToSingle(multiArray, multiArray.GetLength(2));
    }

    public static float[,,] singleToMulti(float[] singleArray, int texturesAmount)
    {
        int length = (int)Math.Sqrt(singleArray.Length / texturesAmount);
        float[,,] multiArray = new float[length, length, texturesAmount];
        for (int i = 0; i < multiArray.GetLength(0); i++)
        {
            for (int j = 0; j < multiArray.GetLength(1); j++)
            {
                for (int k = 0; k < multiArray.GetLength(2); k++)
                {
                    multiArray[i, j, k] = singleArray[i * multiArray.GetLength(1) * multiArray.GetLength(2) + (j * multiArray.GetLength(2) + k)];
                }
            }
        }
        return multiArray;
    }
    public static float[,,] MultiNormalised(float[,,] multiArray, int texturesAmount)
    {
        int length = (int)Math.Sqrt(multiArray.Length / texturesAmount);
        float[] splatWeights = new float[multiArray.GetLength(2)];
        for (int i = 0; i < multiArray.GetLength(0); i++)
        {
            for (int j = 0; j < multiArray.GetLength(1); j++)
            {
                for (int k = 0; k < multiArray.GetLength(2); k++)
                {
                    splatWeights[k] = multiArray[i, j, k];
                }
                float normalisedWeights = splatWeights.Sum(); // Normalize so that sum of all texture weights = 1. Stops the black shit from the swamps.
                for (int k = 0; k < multiArray.GetLength(2); k++)
                {
                    splatWeights[k] /= normalisedWeights;
                    multiArray[i, j, k] = splatWeights[k];
                }
            }
        }
        return multiArray;
    }
}
