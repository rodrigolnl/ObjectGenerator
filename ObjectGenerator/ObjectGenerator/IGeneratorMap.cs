
namespace ObjectGenerator.ObjectGenerator
{
    public interface IGeneratorMap
    {
        public S run<S>(S objeto);
        public Type Type => Type;
        public Random Rnd { get; set; }
    }
}
