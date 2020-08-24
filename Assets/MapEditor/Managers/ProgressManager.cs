using UnityEditor;

public static class ProgressManager
{
    /// <summary>Removes any finished progress bars with the same name.</summary>
    public static void RemoveProgressBars(string progressName)
    {
        for (int i = 0; i < Progress.GetCount(); i++)
        {
            var progress = Progress.GetProgressById(Progress.GetId(i));
            if (progress.finished && progress.name.Contains(progressName))
                progress.Remove();
        }
    }
}