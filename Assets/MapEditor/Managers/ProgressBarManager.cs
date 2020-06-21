using UnityEditor;

public static class ProgressBarManager
{
    /// <summary>The current value of the progress bar.</summary>
    public static float progressValue { get; private set; }

    /// <summary>Value to increment the progress bar when DisplayIncremental is called.</summary>
    public static float progressIncrement { get; private set; }

    /// <summary>Displays a popup progress bar, the progress is also visible in the taskbar.</summary>
    /// <param name="title">The Progress Bar title.</param>
    /// <param name="info">The info to be displayed next to the loading bar.</param>
    /// <param name="progress">The progress amount. Between 0f - 1f.</param>
    public static void Display(string title, string info, float progress)
    {
        progressValue = progress;
        EditorUtility.DisplayProgressBar(title, info, progress);
    }

    /// <summary>Displays popup progress bar that increments the progress value each time it is called by the progress increment.</summary>
    public static void DisplayIncremental(string title, string info)
    {
        EditorUtility.DisplayProgressBar(title, info, progressValue);
        AddIncrement();
    }

    /// <summary>Displays a cancelable popup progress bar, the progress is also visible in the taskbar.</summary>
    public static void DisplayCancelable(string title, string info, float progress)
    {
        progressValue = progress;
        EditorUtility.DisplayCancelableProgressBar(title, info, progress);
    }

    /// <summary>Clears the popup progress bar and stored values. Needs to be called otherwise it will persist in the editor.</summary>
    public static void Clear()
    {
        SetProgressValue(0);
        SetProgressIncrement(0);
        EditorUtility.ClearProgressBar();
    }

    public static void SetProgressValue(float value)
    {
        progressValue = value;
    }

    public static void SetProgressIncrement(float value)
    {
        progressIncrement = value;
    }

    /// <summary>Adds the current increment to the progress value.</summary>
    public static void AddIncrement()
    {
        progressValue += progressIncrement;
    }
}
