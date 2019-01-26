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
                tempArray[i, j] = (array[i, j] * scale);
            }
        }

        return array = tempArray;
    }
	
	public static float[,] overwrite(float[,] array, float[,] target, int x, int y)
	{
			for (int i = 0; i < array.GetLength(0); i++)
			{
				for (int j = 0; j < array.GetLength(1); j++)
				{
					target[i + x, j + y] = array[i, j];
				}
			}
			return target;
	}
	
		public static float[,] overwrite(float[,] array, float[,] target, int x, int y, int x1, int y1, int w, int l)
	{
			for (int i = x1; i < l + x1; i++)
			{
				for (int j = y1; j < w + y1; j++)
				{
					target[i + x - x1, j + y - y1] = array[i, j];
				}
			}
			return target;
	}
	
		
	
	
}
