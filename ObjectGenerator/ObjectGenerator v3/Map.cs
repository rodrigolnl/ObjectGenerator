using System.Security.Cryptography.X509Certificates;

namespace ObjectGenerator.ObjectGenerator_v3
{
    public class Map : BaseMap
    {
        public Map()
        {
            RuleSet<ClienteModel>()
                .SetPreset(x => x.Nome, Preset.NomeCompleto)
                .SetPresetOnConditional(x => x.CpfCnpj, If.True, x => x.Pessoa == Pessoa.Fisica, Preset.CpfValido)
                .SetPresetOnConditional(x => x.CpfCnpj, If.True, x => x.Pessoa == Pessoa.Juridica, Preset.CnpjValido)
                .SetValue(x => x.Apelido, x => x.Nome.Split(' ')[0])
                .SetFormatOnConditional(x => x.CpfCnpj, If.True, x => x.Pessoa == Pessoa.Fisica, "###.###.###-##")
                .SetFormatOnConditional(x => x.CpfCnpj, If.False, x => x.Pessoa == Pessoa.Fisica, "##.###.###/####-##");

            RuleSet<OutroModel>()
                .SetValue(x => x.teste, "TESTANDO");


            //.SetValueOnConditional(x => x.nome, If.True, x => x.id == null, "Rodrigo Lopes");
            //    .SetPreset(x => x.nome, Preset.Nome)
            //    .SetPreset(x => x.sobrenome, Preset.Sobrenome)
            //    .SetPreset(x => x.email, Preset.Email)
            //    .SetPreset(x => x.celular, Preset.Celular)
            //    .SetFormat(x => x.celular, "(##) #####-####")
            //    .SetPreset(x => x.cnpj, Preset.Int32String)
            //    .SetFormat(x => x.cnpj, "###,##")
            //    .SetCondition(x => x.idade, Operator.Between, new int[] {20, 30})
            //    .SetCondition(x => x.data, Operator.LessThan, DateTime.Now);
        }
    }
}
