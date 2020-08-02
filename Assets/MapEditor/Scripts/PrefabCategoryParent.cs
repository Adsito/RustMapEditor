using UnityEngine;

[ExecuteInEditMode]
public class PrefabCategoryParent : MonoBehaviour
{
    public void Update()
    {
        if (gameObject.transform.childCount == 0)
        {
            PrefabManager.PrefabCategories.Remove(gameObject.name);
            GameObject.DestroyImmediate(gameObject);
        }
    }
}
