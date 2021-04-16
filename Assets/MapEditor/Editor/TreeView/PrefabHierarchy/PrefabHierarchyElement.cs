using System;

namespace RustMapEditor.UI
{
	[Serializable]
	public class PrefabHierarchyElement : TreeElement
	{
	    public string PrefabName, Type, Category;
        public uint RustID;
        public PrefabDataHolder prefabDataHolder;

		public PrefabHierarchyElement (int depth, int id, string name, string type= "", string category = "", uint rustID = 0) : base (name, depth, id)
		{
            if (!String.IsNullOrEmpty(name))
            {
                PrefabName = name.Split(':')[0];
                Type = type;
                Category = category;
                RustID = rustID;
            }
        }
	}
}