using ObjectGenerator.ObjectGenerator;

namespace ObjectGenerator
{
    public class Mapping : ObjectGeneratorMap
    {
        public Mapping()
        {
            var Cliente = Add<ClienteModel>();
            //Cliente.map(x => x.nome, PreSet.Nome);
            //Cliente.map(x => x.sobrenome, PreSet.Sobrenome);
            //Cliente.map(x => x.email, PreSet.Email);
            //Cliente.map(x => x.celular, PreSet.Celular, "(##) #####-####");
            //Cliente.map(x => x.cnpj, PreSet.CnpjValido);
        }
    }
}
