using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using RustMapEditor.Variables;

namespace RustMapEditor.Maths
{
    public static class Array
    {
        /// <summary>Sets all the elements in the selected area of the array to the specified value.</summary>
        /// <param name="dmns">The area of the array to perform the operations.</param>
        public static float[,] SetValues(float[,] array, float value, Dimensions dmns = null)
        {
            if (dmns != null)
            {
                Parallel.For(dmns.x0, dmns.x1, i =>
                {
                    for (int j = dmns.z0; j < dmns.z1; j++)
                    {
                        array[i, j] = value;
                    }
                });
            }
            else
            {
                Parallel.For(0, array.GetLength(0), i =>
                {
                    for (int j = 0; j < array.GetLength(1); j++)
                    {
                        array[i, j] = value;
                    }
                });
            }
            return array;
        }
        public static bool[,] SetValues(bool[,] array, bool value, Dimensions dmns = null)
        {
            if (dmns != null)
            {
                Parallel.For(dmns.x0, dmns.x1, i =>
                {
                    for (int j = dmns.z0; j < dmns.z1; j++)
                    {
                        array[i, j] = value;
                    }
                });
            }
            else
            {
                Parallel.For(0, array.GetLength(0), i =>
                {
                    for (int j = 0; j < array.GetLength(1); j++)
                    {
                        array[i, j] = value;
                    }
                });
            }
            return array;
        }
        /// <summary>Clamps all the values to within the set range.</summary>
        /// <param name="dmns">The area of the array to perform the operations.</param>
        public static float[,] ClampValues(float[,] array, float minValue, float maxValue, Dimensions dmns = null)
        {
            if (dmns != null)
            {
                Parallel.For(dmns.x0, dmns.x1, i =>
                {
                    for (int j = dmns.z0; j < dmns.z1; j++)
                    {
                        array[i, j] = Mathf.Clamp(array[i, j], minValue, maxValue);
                    }
                });
            }
            else
            {
                Parallel.For(0, array.GetLength(0), i =>
                {
                    for (int j = 0; j < array.GetLength(1); j++)
                    {
                        array[i, j] = Mathf.Clamp(array[i, j], minValue, maxValue);
                    }
                });
            }
            return array;
        }
        /// <summary>Rotates the array CW or CCW.</summary>
        /// <param name="CW">CW = 90°, CCW = 270°</param>
        /// <param name="dmns">The area of the array to perform the operations.</param>
        public static float[,] Rotate(float[,] array, bool CW, Dimensions dmns = null)
        {
            float[,] newArray = new float[array.GetLength(0), array.GetLength(1)];
            if (dmns != null)
            {
                if (CW)
                {
                    Parallel.For(dmns.x0, dmns.x1, i =>
                    {
                        for (int j = dmns.z0; j < dmns.z1; j++)
                        {
                            newArray[i, j] = array[j, array.GetLength(1) - i - 1];
                        }
                    });
                }
                else
                {
                    Parallel.For(dmns.x0, dmns.x1, i =>
                    {
                        for (int j = dmns.z0; j < dmns.z1; j++)
                        {
                            newArray[i, j] = array[array.GetLength(0) - j - 1, i];
                        }
                    });
                }
            }
            else
            {
                if (CW)
                {
                    Parallel.For(0, array.GetLength(0), i =>
                    {
                        for (int j = 0; j < array.GetLength(1); j++)
                        {
                            newArray[i, j] = array[j, array.GetLength(1) - i - 1];
                        }
                    });
                }
                else
                {
                    Parallel.For(0, array.GetLength(0), i =>
                    {
                        for (int j = 0; j < array.GetLength(1); j++)
                        {
                            newArray[i, j] = array[array.GetLength(0) - j - 1, i];
                        }
                    });
                }
            }
            return newArray;
        }
        /// <summary>Flips the values of the array.</summary>
        /// <param name="dmns">The area of the array to perform the operations.</param>
        public static float[,] Invert(float[,] array, Dimensions dmns = null)
        {
            if (dmns != null)
            {
                Parallel.For(dmns.x0, dmns.x1, i =>
                {
                    for (int j = dmns.z0; j < dmns.z1; j++)
                    {
                        array[i, j] = 1 - array[i, j];
                    }
                });
            }
            else
            {
                Parallel.For(0, array.GetLength(0), i =>
                {
                    for (int j = 0; j < array.GetLength(1); j++)
                    {
                        array[i, j] = 1 - array[i, j];
                    }
                });
            }
            return array;
        }
        public static float[,,] Invert(float[,,] array, Dimensions dmns = null)
        {
            if (dmns != null)
            {
                Parallel.For(dmns.x0, dmns.x1, i =>
                {
                    for (int j = dmns.z0; j < dmns.z1; j++)
                    {
                        for (int k = 0; k < array.GetLength(2); k++)
                        {
                            array[i, j, k] = 1 - array[i, j, k];
                        }
                    }
                });
            }
            else
            {
                Parallel.For(0, array.GetLength(0), i =>
                {
                    for (int j = 0; j < array.GetLength(1); j++)
                    {
                        for (int k = 0; k < array.GetLength(2); k++)
                        {
                            array[i, j, k] = 1 - array[i, j, k];
                        }
                    }
                });
            }
            return array;
        }
        public static bool[,] Invert(bool[,] array, Dimensions dmns = null)
        {
            if (dmns != null)
            {
                Parallel.For(dmns.x0, dmns.x1, i =>
                {
                    for (int j = dmns.z0; j < dmns.z1; j++)
                    {
                        array[i, j] = !array[i, j];
                    }
                });
            }
            else
            {
                Parallel.For(0, array.GetLength(0), i =>
                {
                    for (int j = 0; j < array.GetLength(1); j++)
                    {
                        array[i, j] = !array[i, j];
                    }
                });
            }
            return array;
        }
        /// <summary>Normalises the values of the array between 2 floats.</summary>
        /// <param name="normaliseLow">Min value of the array.</param>
        /// <param name="normaliseHigh">Max value of the array.</param>
        /// <param name="dmns">The area of the array to perform the operations.</param>
        public static float[,] Normalise(float[,] array, float normaliseLow, float normaliseHigh, Dimensions dmns = null)
        {
            float highestPoint = 0f, lowestPoint = 1f, heightRange = 0f, normalisedHeightRange = 0f;
            if (dmns != null)
            {
                Parallel.For(dmns.x0, dmns.x1, i =>
                {
                    for (int j = dmns.z0; j < dmns.z1; j++)
                    {
                        if (array[i, j] < lowestPoint)
                        {
                            lowestPoint = array[i, j];
                        }
                        else if (array[i, j] > highestPoint)
                        {
                            highestPoint = array[i, j];
                        }
                    }
                });
                heightRange = highestPoint - lowestPoint;
                normalisedHeightRange = normaliseHigh - normaliseLow;
                Parallel.For(dmns.x0, dmns.x1, i =>
                {
                    for (int j = dmns.z0; j < dmns.z1; j++)
                    {
                        array[i, j] = normaliseLow + ((array[i, j] - lowestPoint) / heightRange) * normalisedHeightRange;
                    }
                });
            }
            else
            {
                Parallel.For(0, array.GetLength(0), i =>
                {
                    for (int j = 0; j < array.GetLength(1); j++)
                    {
                        if (array[i, j] < lowestPoint)
                        {
                            lowestPoint = array[i, j];
                        }
                        else if (array[i, j] > highestPoint)
                        {
                            highestPoint = array[i, j];
                        }
                    }
                });
                heightRange = highestPoint - lowestPoint;
                normalisedHeightRange = normaliseHigh - normaliseLow;
                Parallel.For(0, array.GetLength(0), i =>
                {
                    for (int j = 0; j < array.GetLength(1); j++)
                    {
                        array[i, j] = normaliseLow + ((array[i, j] - lowestPoint) / heightRange) * normalisedHeightRange;
                    }
                });
            }
            return array;
        }
        /// <summary>Offsets the values of the array by the specified value.</summary>
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
            Parallel.For(0, heights.GetLength(0), i =>
            {
                for (int j = 0; j < heights.GetLength(1); j++)
                {
                    heights[i, j] = BitUtility.Short2Float(terrainMap[i, j]);
                }
            });
            return heights;
        }
        public static byte[] FloatArrayToByteArray(float[,] array)
        {
            short[] shortArray = new short[array.GetLength(0) * array.GetLength(1)];

            Parallel.For(0, array.GetLength(0), i =>
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    shortArray[(i * array.GetLength(0)) + j] = BitUtility.Float2Short(array[i, j]);
                }
            });

            byte[] byteArray = new byte[shortArray.Length * 2];

            Buffer.BlockCopy(shortArray, 0, byteArray, 0, byteArray.Length);

            return byteArray;
        }
        public static float[] MultiToSingle(float[,,] array, int size)
        {
            float[] singleArray = new float[array.GetLength(0) * array.GetLength(1) * size];

            Parallel.For(0, array.GetLength(0), i =>
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    for (int k = 0; k < size; k++)
                    {
                        singleArray[i * array.GetLength(1) * size + (j * size + k)] = array[i, j, k];
                    }
                }
            });
            return singleArray;
        }
        public static float[] MultiToSingle(float[,,] array)
        {
            return MultiToSingle(array, array.GetLength(2));
        }
        public static float[,,] SingleToMulti(float[] array, int texturesAmount)
        {
            int length = (int)Math.Sqrt(array.Length / texturesAmount);
            float[,,] multiArray = new float[length, length, texturesAmount];
            Parallel.For(0, multiArray.GetLength(0), i =>
            {
                for (int j = 0; j < multiArray.GetLength(1); j++)
                {
                    for (int k = 0; k < multiArray.GetLength(2); k++)
                    {
                        multiArray[i, j, k] = array[i * multiArray.GetLength(1) * multiArray.GetLength(2) + (j * multiArray.GetLength(2) + k)];
                    }
                }
            });
            return multiArray;
        }
        public static float[,,] MultiToSingleNormalised(float[,,] array, int texturesAmount)
        {
            int length = (int)Math.Sqrt(array.Length / texturesAmount);
            Parallel.For(0, array.GetLength(0), i =>
            {
                float[] splatWeights = new float[array.GetLength(2)];
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    for (int k = 0; k < array.GetLength(2); k++)
                    {
                        splatWeights[k] = array[i, j, k];
                    }
                    float normalisedWeights = splatWeights.Sum();
                    for (int k = 0; k < array.GetLength(2); k++)
                    {
                        splatWeights[k] /= normalisedWeights;
                        array[i, j, k] = splatWeights[k];
                    }
                }
            });
            return array;
        }
        public static float[,,] BoolToMulti(bool[,] array)
        {
            float[,,] multiArray = new float[array.GetLength(0), array.GetLength(1), 2];
            Parallel.For(0, array.GetLength(0), i =>
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        multiArray[i, j, k] = (array[i, j]) ? 1f : 0f;
                    }
                }
            });
            return multiArray;
        }
        public static bool[,] MultiToBool(float[,,] array)
        {
            bool[,] boolArray = new bool[array.GetLength(0), array.GetLength(1)];
            Parallel.For(0, array.GetLength(0), i =>
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    boolArray[i, j] = (array[i, j, 0] > 0.5f) ? true : false; 
                }
            });
            return boolArray;
        }
    }
}