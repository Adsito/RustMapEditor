using UnityEngine;

public interface IFileSystem
{
    T Load<T>(string filePath, bool bComplain = true) where T : Object;

    string[] FindAll(string folder, string search);

    AsyncOperation LoadAsync(string filePath);
}