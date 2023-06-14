
namespace ObjectGenerator
{
    public class ClienteModel
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public Pessoa Pessoa { get; set; }
        public string CpfCnpj { get; set; }
        public string Apelido { get; set; }
        public IList<DateTime> Datas { get; set; }
        public IList<OutroModel> Outros { get; set; }
    }
    public enum Pessoa
    {
        Fisica,
        Juridica
    }
}

