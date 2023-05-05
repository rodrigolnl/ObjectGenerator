namespace ObjectGenerator.ObjectGenerator_v2
{
    public class BaseMap
    {
        public IList<IRuleSet> Rules { get; set; }
        public BaseMap()
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
