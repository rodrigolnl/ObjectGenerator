
namespace ObjectGenerator.ObjectGenerator
{
    public class ObjectGeneratorMap
    {
        public IList<IGeneratorMap> Mappings { get; set; }
        public ObjectGeneratorMap()
        {
            Mappings = new List<IGeneratorMap>();
        }
        public GeneratorMap<T> Add<T>() where T : new()
        {
            var map = new GeneratorMap<T>();
            Mappings.Add(map);
            return map;
        }

    }
}
