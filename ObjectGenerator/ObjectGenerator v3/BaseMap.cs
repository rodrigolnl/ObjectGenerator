namespace ObjectGenerator.ObjectGenerator_v3
{
    public class BaseMap
    {
        internal IList<IRuleSet> Rules { get; set; }
        internal BaseMap()
        {
            Rules = new List<IRuleSet>();
        }
        public RuleSet<T> RuleSet<T>() where T : new()
        {
            var ruleSet = new RuleSet<T>();
            Rules.Add(ruleSet);
            return ruleSet;
        }
    }
}
