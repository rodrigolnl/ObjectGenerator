
namespace ObjectGenerator
{
    public class ClienteModel
    {
        public int id { get; set; }
        public string nome { get; set; }
        public string sobrenome { get; set; }
        public string email { get; set; }
        public string celular { get; set; }
        public string cnpj { get; set; }
        public int idade { get; set; }
        public DateTime data { get; set; }
        public OutroModel outro { get; set; }
    }
}
