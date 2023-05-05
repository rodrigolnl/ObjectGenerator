using System.Reflection;

namespace ObjectGenerator.ObjectGenerator_v2
{
    public class Rule
    {
        public PropertyInfo Property { get; set; }
        public Preset? Preset { get; set; }
        public dynamic? Value { get; set; }
        public string? Format { get; set; }
        public PropertyInfo? PropertySource { get; set; }
        public Operator Operator { get; set; }
    }
}
