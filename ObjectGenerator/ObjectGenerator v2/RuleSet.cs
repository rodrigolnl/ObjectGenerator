using System.Linq.Expressions;
using System.Reflection;

namespace ObjectGenerator.ObjectGenerator_v2
{
    public class RuleSet<T> : IRuleSet where T : new()
    {
        public IList<Rule> Rules { get; set; }

        public RuleSet()
        {
            Rules = new List<Rule>();
        }

        public Type Type => typeof(T);

        public RuleSet<T> NewRule(Expression<Func<T, dynamic>> property, Preset preset)
        {
            Rules.Add(new Rule()
            {
                Property = GetPropertyInfo(property),
                Preset = preset
            });
            return this;
        }
        public RuleSet<T> NewRule(Expression<Func<T, dynamic>> property, Preset preset, string format)
        {
            Rules.Add(new Rule()
            {
                Property = GetPropertyInfo(property),
                Preset = preset,
                Format = format
            });
            return this;
        }
        public RuleSet<T> NewRule(Expression<Func<T, dynamic>> property, dynamic value)
        {
            Rules.Add(new Rule()
            {
                Property = GetPropertyInfo(property),
                Value = value
            });
            return this;
        }
        public RuleSet<T> NewRule(Expression<Func<T, dynamic>> property, string value, string format)
        {
            Rules.Add(new Rule()
            {
                Property = GetPropertyInfo(property),
                Value = value,
                Format = format
            });
            return this;
        }
        public RuleSet<T> NewRule(Expression<Func<T, dynamic>> property, Expression<Func<T, dynamic>> propertySource)
        {
            Rules.Add(new Rule()
            {
                Property = GetPropertyInfo(property),
                PropertySource = GetPropertyInfo(propertySource)
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
