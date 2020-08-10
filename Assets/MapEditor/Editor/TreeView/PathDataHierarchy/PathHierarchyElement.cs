using System;

namespace RustMapEditor.UI
{
    [Serializable]
    internal class PathHierarchyElement : TreeElement
    {
        public string pathName;
        public float width, innerPadding, outerPadding, innerFade, outerFade;
        public PathDataHolder pathData;

        public PathHierarchyElement(string name, int depth, int id) : base(name, depth, id)
        {
            if (!String.IsNullOrEmpty(name))
            {
                var values = name.Split(':');
                pathName = values[0];
                width = float.Parse(values[1]);
                innerPadding = float.Parse(values[2]);
                outerPadding = float.Parse(values[3]);
                innerFade = float.Parse(values[4]);
                outerFade = float.Parse(values[5]);
            }
        }
    }
}