using System.Reflection;

namespace ObjectGenerator.ObjectGenerator_v3
{
    public class Rule
    {
        public PropertyInfo Property { get; set; }
        internal RuleType RuleType { get; set; }
        public Preset? Preset { get; set; }
        public dynamic? Value { get; set; }
        public string? Format { get; set; }
        public Func<object, dynamic>? PropertySource { get; set; }
        public Operator? Operator { get; set; }
        public Func<object, bool>? Statement { get; set; }
        public If? Conditional { get; set; }
    }
}
