namespace ObjectGenerator.ObjectGenerator_v3
{
    public class ListRuleSet
    {
        internal IList<Rule> Rules { get; set; }
        public ListRuleSet()
        {
            Rules = new List<Rule>();
        }
        public ListRuleSet SetPreset(Preset preset)
        {
            Rules.Add(new Rule()
            {
                RuleType = RuleType.SetProperty,
                Preset = preset
            });
            return this;
        }
        public ListRuleSet SetFormat(string format)
        {
            Rules.Add(new Rule()
            {
                RuleType = RuleType.PostExecution,
                Format = format
            });
            return this;
        }
        public ListRuleSet SetValue(dynamic value)
        {
            Rules.Add(new Rule()
            {
                RuleType = RuleType.SetProperty,
                Value = value
            });
            return this;
        }
        public ListRuleSet SetCondition(Operator operation, dynamic value)
        {
            Rules.Add(new Rule()
            {
                RuleType = RuleType.Conditional,
                Operator = operation,
                Value = value
            });
            return this;
        }
    }
}
