using System;
using System.Linq;
using EditorVariables;

namespace EditorMaths
{
    public static class ArrayMaths
    {
        /// <summary>
        /// Sets all the elements in the selected area of the array to the specified value.
        /// </summary>
        /// <param name="dmns">The area of the array to perform the operations.</param>
        public static float[,] SetValues(float[,] array, float value, Dimensions dmns = null)
        {
            if (dmns != null)
            {
                for (int i = dmns.x0; i < dmns.x1; i++)
                {
                    for (int j = dmns.z0; j < dmns.z1; j++)
                    {
                        array[i, j] = value;
                    }
                }
            }
            else
            {
                for (int i = 0; i < array.GetLength(0); i++)
                {
                    for (int j = 0; j < array.GetLength(1); j++)
                    {
                        array[i, j] = value;
                    }
                }

            }
            return array;
        }
        /// <summary>
        /// Rotates the array CW or CCW.
        /// </summary>
        /// <param name="CW">CW = 90°, CCW = 270°</param>
        /// <param name="dmns">The area of the array to perform the operations.</param>
        public static float[,] Rotate(float[,] array, bool CW, Dimensions dmns = null)
        {
            float[,] tempArray = array;
            if (dmns != null)
            {
                if (CW)
                {
                    for (int i = dmns.x0; i < dmns.x1; i++)
                    {
                        for (int j = dmns.z0; j < dmns.z1; j++)
                        {
                            tempArray[i, j] = array[j, array.GetLength(1) - i - 1];
                        }
                    }
                }
                else
                {
                    for (int i = dmns.x0; i < dmns.x1; i++)
                    {
                        for (int j = dmns.z0; j < dmns.z1; j++)
                        {
                            tempArray[i, j] = array[array.GetLength(0) - j - 1, i];
                        }
                    }
                }
            }
            else
            {
                if (CW)
                {
                    for (int i = 0; i < array.GetLength(0); i++)
                    {
                        for (int j = 0; j < array.GetLength(1); j++)
                        {
                            tempArray[i, j] = array[j, array.GetLength(1) - i - 1];
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < array.GetLength(0); i++)
                    {
                        for (int j = 0; j < array.GetLength(1); j++)
                        {
                            tempArray[i, j] = array[array.GetLength(0) - j - 1, i];
                        }
                    }
                }
            }
            return tempArray;
        }
        /// <summary>
        /// Flips the values of the array.
        /// </summary>
        /// <param name="dmns">The area of the array to perform the operations.</param>
        public static float[,] Invert(float[,] array, Dimensions dmns = null)
        {
            if (dmns != null)
            {
                for (int i = dmns.x0; i < dmns.x1; i++)
                {
                    for (int j = dmns.z0; j < dmns.z1; j++)
                    {
                        array[i, j] = 1 - array[i, j];
                    }
                }
            }
            else
            {
                for (int i = 0; i < array.GetLength(0); i++)
                {
                    for (int j = 0; j < array.GetLength(1); j++)
                    {
                        array[i, j] = 1 - array[i, j];
                    }
                }
            }
            return array;
        }
        /// <summary>
        /// Normalises the values of the array between 2 floats.
        /// </summary>
        /// <param name="normaliseLow">Min value of the array.</param>
        /// <param name="normaliseHigh">Max value of the array.</param>
        /// <param name="dmns">The area of the array to perform the operations.</param>
        public static float[,] Normalise(float[,] array, float normaliseLow, float normaliseHigh, Dimensions dmns = null)
        {
            float highestPoint = 0f, lowestPoint = 1f, currentHeight = 0f, heightRange = 0f, normalisedHeightRange = 0f, normalisedHeight = 0f;
            if (dmns != null)
            {
                for (int i = dmns.x0; i < dmns.x1; i++)
                {
                    for (int j = dmns.z0; j < dmns.z1; j++)
                    {
                        currentHeight = array[i, j];
                        if (currentHeight < lowestPoint)
                        {
                            lowestPoint = currentHeight;
                        }
                        else if (currentHeight > highestPoint)
                        {
                            highestPoint = currentHeight;
                        }
                    }
                }
                heightRange = highestPoint - lowestPoint;
                normalisedHeightRange = normaliseHigh - normaliseLow;
                for (int i = dmns.x0; i < dmns.x1; i++)
                {
                    for (int j = dmns.z0; j < dmns.z1; j++)
                    {
                        normalisedHeight = ((array[i, j] - lowestPoint) / heightRange) * normalisedHeightRange;
                        array[i, j] = normaliseLow + normalisedHeight;
                    }
                }
            }
            else
            {
                for (int i = 0; i < array.GetLength(0); i++)
                {
                    for (int j = 0; j < array.GetLength(1); j++)
                    {
                        currentHeight = array[i, j];
                        if (currentHeight < lowestPoint)
                        {
                            lowestPoint = currentHeight;
                        }
                        else if (currentHeight > highestPoint)
                        {
                            highestPoint = currentHeight;
                        }
                    }
                }
                heightRange = highestPoint - lowestPoint;
                normalisedHeightRange = normaliseHigh - normaliseLow;
                for (int i = 0; i < array.GetLength(0); i++)
                {
                    for (int j = 0; j < array.GetLength(1); j++)
                    {
                        normalisedHeight = ((array[i, j] - lowestPoint) / heightRange) * normalisedHeightRange;
                        array[i, j] = normaliseLow + normalisedHeight;
                    }
                }
            }
            return array;
        }
        /// <summary>
        /// Offsets the values of the array by the specified value.
        /// </summary>
        /// <param name="dmns">The area of the array to perform the operations.</param>
        /// <param name="clampOffset">Prevent array values from overflowing.</param>
        public static float[,] Offset(float[,] array, float offset, bool clampOffset, Dimensions dmns = null)
        {
            float[,] tempArray = array;
            if (dmns != null)
            {
                for (int i = dmns.x0; i < dmns.x1; i++)
                {
                    for (int j = dmns.z0; j < dmns.z1; j++)
                    {
                        if (clampOffset == true)
                        {
                            if ((array[i, j] + offset > 1f || array[i, j] + offset < 0f))
                            {
                                return array;
                            }
                            else
                            {
                                tempArray[i, j] += offset;
                            }
                        }
                        else
                        {
                            tempArray[i, j] += offset;
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < array.GetLength(0); i++)
                {
                    for (int j = 0; j < array.GetLength(1); j++)
                    {
                        if (clampOffset == true)
                        {
                            if ((array[i, j] + offset > 1f || array[i, j] + offset < 0f))
                            {
                                return array;
                            }
                            else
                            {
                                tempArray[i, j] += offset;
                            }
                        }
                        else
                        {
                            tempArray[i, j] += offset;
                        }
                    }
                }
            }
            return tempArray;
        }
        public static float[,] ShortMapToFloatArray(TerrainMap<short> terrainMap)
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
        public static byte[] FloatArrayToByteArray(float[,] floatArray)
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
        public static float[] MultiToSingle(float[,,] multiArray, int size)
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
        public static float[] MultiToSingle(float[,,] multiArray)
        {
            return MultiToSingle(multiArray, multiArray.GetLength(2));
        }
        public static float[,,] SingleToMulti(float[] singleArray, int texturesAmount)
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
        public static float[,,] MultiToSingleNormalised(float[,,] multiArray, int texturesAmount)
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
                    float normalisedWeights = splatWeights.Sum();
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
}