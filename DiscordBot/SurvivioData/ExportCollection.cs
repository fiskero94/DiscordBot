using System.Collections.Generic;
using System.Linq;

namespace SurvivioData
{
    public class ExportCollection
    {
        private Dictionary<string, object> _data;
        public string Name { get; private set; }
        public List<ExportCollection> Children { get; private set; }
        public ExportCollection Parent { get; private set; }

        public ExportCollection(Dictionary<string, object> data, string name)
        {
            _data = data;
            Name = name;
            Children = new List<ExportCollection>();
            foreach (string key in _data.Keys)
                if (_data[key] is Dictionary<string, object>)
                    Children.Add(new ExportCollection((Dictionary<string, object>)_data[key], this, key));
        }

        private ExportCollection(Dictionary<string, object> data, ExportCollection parent, string name) : this(data, name) => Parent = parent;

        public string String(string field) => (string)_data[field];
        public double Double(string field) => (double)_data[field];
        public bool Boolean(string field) => (bool)_data[field];
        public string[] StringArray(string field) => (string[])((object[])_data[field]).OfType<string>();
        public double[] DoubleArray(string field) => (double[])((object[])_data[field]).OfType<double>();
        public bool[] BooleanArray(string field) => (bool[])((object[])_data[field]).OfType<bool>();

        public ExportCollection Find(string collection)
        {
            collection = collection.Trim('\"');
            List<ExportCollection> children = new List<ExportCollection>() { this };
            children = GetAllChildren(children);
            return children.Single(c => (c.Name == collection || c.Name == "\"" + collection + "\""));
        }

        public ExportCollection Find(string parent, string collection)
        {
            collection = collection.Trim('\"');
            parent = parent.Trim('\"');
            List<ExportCollection> children = new List<ExportCollection>() { this };
            children = GetAllChildren(children).Where(c => c.Parent != null).ToList();
            return children.Single(c => (c.Name == collection || c.Name == "\"" + collection + "\"") &&
                                        (c.Parent.Name == parent || c.Parent.Name == "\"" + parent + "\""));
        }

        public ExportCollection Find(string grandParent, string parent, string collection)
        {
            collection = collection.Trim('\"');
            parent = parent.Trim('\"');
            grandParent = grandParent.Trim('\"');
            List<ExportCollection> children = new List<ExportCollection>() { this };
            children = GetAllChildren(children).Where(c => c.Parent != null && c.Parent.Parent != null).ToList();
            return children.Single(c => (c.Name == collection || c.Name == "\"" + collection + "\"") &&
                                        (c.Parent.Name == parent || c.Parent.Name == "\"" + parent + "\"") &&
                                        (c.Parent.Parent.Name == grandParent || c.Parent.Parent.Name == "\"" + grandParent + "\""));
        }

        private List<ExportCollection> GetAllChildren(List<ExportCollection> children)
        {
            var all = children.SelectMany(c => c.Children).ToList();
            return all.Any() ? all.Concat(GetAllChildren(all)).ToList() : all;
        }
    }
}