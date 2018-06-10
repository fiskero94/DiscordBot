using System;
using System.Collections.Generic;
using System.Linq;

namespace SurvivioData
{
    public class ExportCollection : IEquatable<ExportCollection>, IComparable<ExportCollection>
    {
        private Dictionary<string, object> _data;
        private List<ExportCollection> _children;
        private List<Field> _fields;

        public string Name { get; private set; }
        public ExportCollection Parent { get; private set; }
        public List<ExportCollection> Children => new List<ExportCollection>(_children);
        public List<Field> Fields => new List<Field>(_fields);

        public ExportCollection(Dictionary<string, object> data, string name)
        {
            Name = name;
            _data = data;
            _fields = new List<Field>();
            _data.Keys.ToList().ForEach(key => new Field(key, _data[key]));
            _fields.Sort();
            _children = new List<ExportCollection>();
            _data.Keys.ToList().Where(key => _data[key] is Dictionary<string, object>)
                      .ToList().ForEach(key => _children.Add(new ExportCollection((Dictionary<string, object>)_data[key], this, key)));
            _children.Sort();
        }

        private ExportCollection(Dictionary<string, object> data, ExportCollection parent, string name) : this(data, name) => Parent = parent;

        public string String(string field) => (string)_data[field];
        public double Double(string field) => (double)_data[field];
        public bool Boolean(string field) => (bool)_data[field];
        public string[] StringArray(string field) => (string[])((object[])_data[field]).OfType<string>();
        public double[] DoubleArray(string field) => (double[])((object[])_data[field]).OfType<double>();
        public bool[] BooleanArray(string field) => (bool[])((object[])_data[field]).OfType<bool>();

        public string Display(string field)
        {
            object value = _data[field];
            if (value is string) return value as string;
            else if (value is double) return FormatDouble((double)value);
            else if (value is bool) return (bool)value ? "Yes" : "No";
            else if (value is string[]) return ConcatenateArray(value as string[]);
            else if (value is double[]) return ConcatenateArray(((double[])value).ToList().Select(d => FormatDouble(d)).ToArray());
            else if (value is bool[]) return ConcatenateArray(((bool[])value).ToList().Select(b => b ? "Yes" : "No").ToArray());
            else return "Invalid Type";
        }

        private string FormatDouble(double number) => number.ToString("N2").TrimEnd('0').TrimEnd('.');

        private string ConcatenateArray(string[] array)
        {
            string concatenated = "";
            for (int i = 0; i < array.Length; i++)
                concatenated += array[i] + ", ";
            return concatenated.TrimEnd(' ').TrimEnd(',');
        }

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

        public bool Equals(ExportCollection other) => other is null ? false : Name.Equals(other.Name);
        public int CompareTo(ExportCollection other) => other is null ? 1 : Name.CompareTo(other.Name);
        public override string ToString() => Name;
    }
}