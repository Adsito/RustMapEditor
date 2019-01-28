using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTransformations {

    public static float[,] resize(float [,] array, int size)
    {
        float[,] tempArray = new float[size, size];
        int limit = array.GetLength(0) > size ? size : array.GetLength(0);

        for (int i = 0; i < limit; i++)
        {
            for (int j = 0; j < limit; j++)
            {
                tempArray[i, j] = array[i, j];
            }
        }

        return array = tempArray;
    }


    public static float[,] rotateCCW(float[,] array)
    {
        float[,] tempArray = new float[array.GetLength(0), array.GetLength(1)];

        for (int i = 0; i < array.GetLength(0); i++)
        {
            for (int j = 0; j < array.GetLength(1); j++)
            {
                tempArray[i, j] = array[array.GetLength(0) - j - 1, i];
            }
        }

        return array = tempArray;
    }

    public static float[,] rotateCW(float[,] array)
    {
        float[,] tempArray = new float[array.GetLength(0), array.GetLength(1)];

        for (int i = 0; i < array.GetLength(0); i++)
        {
            for (int j = 0; j < array.GetLength(1); j++)
            {
                tempArray[i, j] = array[j, array.GetLength(1) - i - 1];
            }
        }

        return array = tempArray;
    }

    public static float[,] flip(float[,] array)
    {
        float[,] tempArray = new float[array.GetLength(0), array.GetLength(1)];

        for (int i = 0; i < array.GetLength(0); i++)
        {
            for (int j = 0; j < array.GetLength(1); j++)
            {
                tempArray[i, j] = array[i, array.GetLength(0) - j - 1];
            }
        }

        return array = tempArray;
    }

    public static float[,] transpose(float[,] array)
    {
        float[,] tempArray = new float[array.GetLength(0), array.GetLength(1)];

        for (int i = 0; i < array.GetLength(0); i++)
        {
            for (int j = 0; j < array.GetLength(1); j++)
            {
                tempArray[i, j] = array[j, i];
            }
        }

        return array = tempArray;
    }
    
    public static float[,] scale(float[,] array, float scale)
    {
        float[,] tempArray = new float[array.GetLength(0), array.GetLength(1)];

        for (int i = 0; i < array.GetLength(0); i++)
        {
            for (int j = 0; j < array.GetLength(1); j++)
            {
                tempArray[i, j] = array[i, j] * scale;
            }
        }

        return array = tempArray;
    }

}
