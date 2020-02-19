using System;

namespace RustMapEditor.UI
{
    [Serializable]
    public class PrefabsListElement : TreeElement
    {
        public string prefabName;
        public uint rustID;

        public PrefabsListElement(string name, int depth, int id, string path = "") : base(name, depth, id)
        {
            if (!String.IsNullOrEmpty(name))
            {
                prefabName = name;
                rustID = AssetManager.ToID(path);
                base.name = prefabName + rustID.ToString();
            }
        }
    }
}