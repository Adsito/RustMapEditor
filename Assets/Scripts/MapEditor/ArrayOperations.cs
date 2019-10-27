public static class ArrayOperations
{
    /// <summary>
    /// Sets all the elements in the array to the specified value.
    /// </summary>
    /// <param name="value">The value to set.</param>
    /// <returns></returns>
    public static float[,] SetValues(float [,] array, float value)
    {
        for (int i = 0; i < array.GetLength(0); i++)
        {
            for (int j = 0; j < array.GetLength(1); j++)
            {
                array[i, j] = value;
            }
        }
        return array;
    }
    /// <summary>
    /// Flips the values of the array
    /// </summary>
    /// <returns></returns>
    public static float[,] Invert(float[,] array)
    {
        for (int i = 0; i < array.GetLength(0); i++)
        {
            for (int j = 0; j < array.GetLength(1); j++)
            {
                array[i, j] = 1 - array[i, j];
            }
        }
        return array;
    }
    /// <summary>
    /// Normalises the values of the array between 2 floats.
    /// </summary>
    /// <param name="normaliseLow">Min value of the array.</param>
    /// <param name="normaliseHigh">Max value of the array.</param>
    /// <returns></returns>
    public static float[,] Normalise(float[,] array, float normaliseLow, float normaliseHigh)
    {
        float highestPoint = 0f, lowestPoint = 1f, currentHeight = 0f, heightRange = 0f, normalisedHeightRange = 0f, normalisedHeight = 0f;
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
        return array;
    }
}