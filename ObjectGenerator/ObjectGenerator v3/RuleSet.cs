using Newtonsoft.Json.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectGenerator.ObjectGenerator_v3
{
    public class RuleSet<T> : IRuleSet where T : new()
    {
        internal IList<Rule> Rules { get; set; }
        IList<Rule> IRuleSet.Rules { get => Rules; }

        public RuleSet()
        {
            Rules = new List<Rule>();
        }

        public Type Type => typeof(T);

        public RuleSet<T> SetPreset(Expression<Func<T, dynamic>> property, Preset preset)
        {
            Rules.Add(new Rule()
            {
                RuleType = RuleType.SetProperty,
                Property = GetPropertyInfo(property),
                Preset = preset
            });
            return this;
        }
        public RuleSet<T> SetPresetOnConditional(Expression<Func<T, dynamic>> property, If codition, Func<T, bool> statement, Preset preset)
        {
            Func<object, bool> capsuled = value => statement((T)value);

            Rules.Add(new Rule()
            {
                RuleType = RuleType.PropertyConditional,
                Property = GetPropertyInfo(property),
                Statement = capsuled,
                Conditional = codition,
                Preset = preset
            });
            return this;
        }

        public RuleSet<T> SetValue(Expression<Func<T, dynamic>> property, dynamic value)
        {
            Rules.Add(new Rule()
            {
                RuleType = RuleType.SetProperty,
                Property = GetPropertyInfo(property),
                Value = value
            });
            return this;
        }
        public RuleSet<T> SetValue(Expression<Func<T, dynamic>> property, Func<T, dynamic> propertySource)
        {
            Func<object, dynamic> srcCapsuled = value => propertySource((T)value);
            Rules.Add(new Rule()
            {
                RuleType = RuleType.SetPropertyFromProperty,
                Property = GetPropertyInfo(property),
                PropertySource = srcCapsuled
            });
            return this;
        }
        public RuleSet<T> SetValueOnConditional(Expression<Func<T, dynamic>> property, If codition, Func<T, bool> statement, dynamic value)
        {
            Func<object, bool> capsuled = value => statement((T)value);

            Rules.Add(new Rule()
            {
                RuleType = RuleType.PropertyConditional,
                Property = GetPropertyInfo(property),
                Statement = capsuled,
                Conditional = codition,
                Value = value
            });
            return this;
        }
        public RuleSet<T> SetValueOnConditional(Expression<Func<T, dynamic>> property, If codition, Func<T, bool> statement, Func<T, dynamic> propertySource)
        {
            Func<object, bool> capsuled = value => statement((T)value);
            Func<object, dynamic> srcCapsuled = value => propertySource((T)value);
            Rules.Add(new Rule()
            {
                RuleType = RuleType.PropertyConditional,
                Property = GetPropertyInfo(property),
                Statement = capsuled,
                Conditional = codition,
                PropertySource = srcCapsuled
            });
            return this;
        }

        public RuleSet<T> SetFormat(Expression<Func<T, dynamic>> property, string format)
        {
            Rules.Add(new Rule()
            {
                RuleType = RuleType.PostExecution,
                Property = GetPropertyInfo(property),
                Format = format
            });
            return this;
        }
        public RuleSet<T> SetFormatOnConditional(Expression<Func<T, dynamic>> property, If codition, Func<T, bool> statement, string format)
        {
            Func<object, bool> capsuled = value => statement((T)value);
            Rules.Add(new Rule()
            {
                RuleType = RuleType.PostExecution,
                Property = GetPropertyInfo(property),
                Format = format,
                Conditional = codition,
                Statement = capsuled,
            });
            return this;
        }

        public RuleSet<T> SetRange(Expression<Func<T, dynamic>> property, Operator operation, dynamic value)
        {
            Rules.Add(new Rule()
            {
                RuleType = RuleType.Range,
                Property = GetPropertyInfo(property),
                Operator = operation,
                Value = value
            });
            return this;
        }
        

        private PropertyInfo GetPropertyInfo(Expression<Func<T, dynamic>> property)
        {
            if (property.Body is MemberExpression)
                return (PropertyInfo)((MemberExpression)property.Body).Member;
            else
                return (PropertyInfo)((MemberExpression)((UnaryExpression)property.Body).Operand).Member;
        }
    }
}
