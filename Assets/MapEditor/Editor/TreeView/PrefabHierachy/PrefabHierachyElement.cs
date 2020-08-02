using System;

namespace RustMapEditor.UI
{
	[Serializable]
	internal class PrefabHierachyElement : TreeElement
	{
	    public string prefabName, type, category;
        public uint rustID;
        public PrefabDataHolder prefabDataHolder;

		public PrefabHierachyElement (string name, int depth, int id) : base (name, depth, id)
		{
            if (!String.IsNullOrEmpty(name))
            {
                var values = name.Split(':');
                prefabName = values[0];
                type = values[1];
                category = values[2];
                rustID = uint.Parse(values[3]);
            }
        }
	}
}