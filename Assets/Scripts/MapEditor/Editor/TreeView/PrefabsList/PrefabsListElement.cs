using System;

namespace RustMapEditor.UI
{
    [Serializable]
    internal class PrefabsListElement : TreeElement
    {
        public string prefabName;
        public uint RustID;

        public PrefabsListElement(string name, int depth, int id) : base(name, depth, id)
        {
            if (!String.IsNullOrEmpty(name))
            {
                prefabName = name;
            }
        }
    }
}