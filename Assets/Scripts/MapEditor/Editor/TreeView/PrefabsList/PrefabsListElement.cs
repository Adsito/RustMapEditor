using System;

namespace RustMapEditor.UI
{
    [Serializable]
    internal class PrefabsListElement : TreeElement
    {
        public string prefabName;
        public uint rustID;

        public PrefabsListElement(string name, int depth, int id, string path = "") : base(name, depth, id)
        {
            if (!String.IsNullOrEmpty(name))
            {
                prefabName = name;
                rustID = StringPool.Get(path);
                base.name = prefabName + rustID.ToString();
            }
        }
    }
}