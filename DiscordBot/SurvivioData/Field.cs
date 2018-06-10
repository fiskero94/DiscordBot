using System;
using System.Collections.Generic;
using System.Text;

namespace SurvivioData
{
    public class Field : IEquatable<Field>, IComparable<Field>
    {
        public string Name { get; private set; }
        public object Value { get; private set; }

        public Field(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public bool Equals(Field other) => other is null ? false : Name.Equals(other.Name);
        public int CompareTo(Field other) => other is null ? 1 : Name.CompareTo(other.Name);
        public override string ToString() => Name + ": " + Value;
    }
}