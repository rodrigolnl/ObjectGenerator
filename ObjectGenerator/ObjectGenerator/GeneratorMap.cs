
using Newtonsoft.Json;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectGenerator.ObjectGenerator
{
    public class GeneratorMap<T> : IGeneratorMap where T : new()
    {
        public T Objeto { get; set; }
        public Random Rnd { get; set; }
        public GeneratorBase? GeneratorBase { get; set; }
        public IList<Func<T, string>> Actions { get; set; }
        public IList<Action> SpecialActions { get; set; }
        public Type Type => typeof(T);
        public GeneratorMap()
        {
            Rnd = new Random();
            Objeto = new T();
            Actions = new List<Func<T, string>>();
            SpecialActions = new List<Action>();
            using var r = new StreamReader("base_generator.json");
            string json = r.ReadToEnd();
            GeneratorBase = JsonConvert.DeserializeObject<GeneratorBase>(json);
        }

        public void map(Func<T, string> action)
        {
            Actions.Add(action);
        }
        public void map(Expression<Func<T, dynamic>> prop, PreSet preset)
        {
            SpecialActions.Add(() => SpecialMap(prop, preset, null));
        }
        public void map(Expression<Func<T, dynamic>> prop, PreSet preset, string format)
        {
            SpecialActions.Add(() => SpecialMap(prop, preset, format));
        }
        public S run<S>(S objeto)
        {
            if (typeof(S) == typeof(T))
                Objeto = (T)(object)objeto;
            else
                throw new Exception();
            foreach (var action in SpecialActions)
                action();
            foreach (var action in Actions)
                action(Objeto);
            return (S)(object)Objeto;
        }

        private void SpecialMap(Expression<Func<T, dynamic>> property, PreSet preset, string? format = null)
        {
            var expr = (MemberExpression)property.Body;
            var prop = (PropertyInfo)expr.Member;

            if (preset == PreSet.Nome)
            {
                var sexo = Rnd.Next(2);
                prop.SetValue(Objeto, sexo == 0 ? GeneratorBase.Masculinos[Rnd.Next(GeneratorBase.Masculinos.Count)] : GeneratorBase.Femininos[Rnd.Next(GeneratorBase.Femininos.Count)]);
            }

            else if (preset == PreSet.Sobrenome)
            {
                var sobrenome = "";
                var length = Rnd.Next(1, 4);
                for (var i = 0; i < length; i++)
                {
                    sobrenome += GeneratorBase.Sobrenomes[Rnd.Next(GeneratorBase.Sobrenomes.Count)];
                    sobrenome += length - 1 == i ? "" : " ";
                }
                prop.SetValue(Objeto, sobrenome);
            }

            else if (preset == PreSet.NomeCompleto)
            {
                var sexo = Rnd.Next(2);
                var nome = sexo == 0 ? GeneratorBase.Masculinos[Rnd.Next(GeneratorBase.Masculinos.Count)] : GeneratorBase.Femininos[Rnd.Next(GeneratorBase.Femininos.Count)];
                var sobrenome = "";
                var length = Rnd.Next(1, 4);
                for (var i = 0; i < length; i++)
                {
                    sobrenome += GeneratorBase.Sobrenomes[Rnd.Next(GeneratorBase.Sobrenomes.Count)];
                    sobrenome += length - 1 == i ? "" : " ";
                }
                prop.SetValue(Objeto, nome + " " + sobrenome);
            }

            else if (preset == PreSet.Email)
            {
                var sexo = Rnd.Next(2);
                var nome = sexo == 0 ? GeneratorBase.Masculinos[Rnd.Next(GeneratorBase.Masculinos.Count)] : GeneratorBase.Femininos[Rnd.Next(GeneratorBase.Femininos.Count)];
                var sobrenome = GeneratorBase.Sobrenomes[Rnd.Next(GeneratorBase.Sobrenomes.Count)];
                var combinacao = Rnd.Next(2);
                var email = "";
                switch (combinacao)
                {
                    case 0:
                        email = nome + sobrenome + "@" + GeneratorBase.Emails[Rnd.Next(GeneratorBase.Emails.Count)];
                        break;
                    case 1:
                        email = nome + sobrenome + Rnd.Next(999).ToString() + "@" + GeneratorBase.Emails[Rnd.Next(GeneratorBase.Emails.Count)];
                        break;
                    default:
                        email = nome + "@" + GeneratorBase.Emails[Rnd.Next(GeneratorBase.Emails.Count)];
                        break;
                }
                prop.SetValue(Objeto, email.ToLower().Replace(" ", ""));
            }

            else if (preset == PreSet.CpfRandom)
            {
                if (prop.PropertyType == typeof(string))
                    prop.SetValue(Objeto, Rnd.NextInt64(10000000000, 99999999999).ToString());
                else
                    prop.SetValue(Objeto, Rnd.NextInt64(10000000000, 99999999999));
            }

            else if (preset == PreSet.CnpjRandom)
            {
                if (prop.PropertyType == typeof(string))
                    prop.SetValue(Objeto, $"{Rnd.NextInt64(10000000, 99999999)}000{Rnd.Next(999)}");
                else
                    prop.SetValue(Objeto, Convert.ToInt64($"{Rnd.NextInt64(10000000, 99999999)}000{Rnd.Next(999)}"));
            }

            else if (preset == PreSet.CpfValido)
            {
                int soma = 0, resto = 0;
                int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
                int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
                string semente = Rnd.Next(100000000, 999999999).ToString();
                for (int i = 0; i < 9; i++)
                    soma += int.Parse(semente[i].ToString()) * multiplicador1[i];
                resto = soma % 11;
                if (resto < 2)
                    resto = 0;
                else
                    resto = 11 - resto;
                semente = semente + resto;
                soma = 0;
                for (int i = 0; i < 10; i++)
                    soma += int.Parse(semente[i].ToString()) * multiplicador2[i];
                resto = soma % 11;
                if (resto < 2)
                    resto = 0;
                else
                    resto = 11 - resto;
                semente = semente + resto;
                if (prop.PropertyType == typeof(string))
                    prop.SetValue(Objeto, semente);
                else
                    prop.SetValue(Objeto, Convert.ToInt64(semente));
            }

            else if (preset == PreSet.CnpjValido)
            {
                var cnpj = new int[] { 1, 0, 0, 0 }.Concat((Enumerable.Repeat(0, 8).Select(i => Rnd.Next(0, 9)).ToArray())).ToArray();
                for (var x = 0; x < 2; x++)
                {
                    var digito = 0;
                    for (var i = 0; i < cnpj.Length; i++)
                    {
                        digito = cnpj[i] * (i % 8 + 2);
                    }
                    digito = 11 - digito % 11;
                    digito = digito < 10 ? digito : 0;
                    cnpj = new int[] { digito }.Concat(cnpj).ToArray();
                }
                cnpj = cnpj.Reverse().ToArray();
                if (prop.PropertyType == typeof(string))
                    prop.SetValue(Objeto, string.Join("", cnpj));
                else
                    prop.SetValue(Objeto, Convert.ToInt64(string.Join("", cnpj)));
            }

            else if (preset == PreSet.Telefone)
            {
                var ddd = GeneratorBase.Prefixos[Rnd.Next(GeneratorBase.Prefixos.Count)];
                var numero = Rnd.Next(0, 99999999) + 900000000;
                if (prop.PropertyType == typeof(string))
                    prop.SetValue(Objeto, (ddd * 100000000 + numero).ToString());
                else
                    prop.SetValue(Objeto, (ddd * 100000000 + numero));
            }

            else if (preset == PreSet.Celular)
            {
                var ddd = GeneratorBase.Prefixos[Rnd.Next(GeneratorBase.Prefixos.Count)];
                var numero = Rnd.Next(0, 99999999) + 900000000;
                if (prop.PropertyType == typeof(string))
                    prop.SetValue(Objeto, ((long)ddd * (long)1000000000 + (long)numero).ToString());
                else
                    prop.SetValue(Objeto, (ddd * 1000000000 + numero));
            }

            else if (preset == PreSet.Cep)
            {
                if (prop.PropertyType == typeof(string))
                    prop.SetValue(Objeto, (Rnd.Next(0, 999999999)).ToString());
                else
                    prop.SetValue(Objeto, (Rnd.Next(0, 999999999)));
            }

            else if (preset == PreSet.Int32String)
            {
                var count = 0;
                if (format != null)
                    count = format.Count(c => c == '#');
                if (prop.PropertyType == typeof(string))
                    prop.SetValue(Objeto, (Rnd.Next((int)(10 ^ (count - 1)), Int32.MaxValue)).ToString());
                else
                    throw new Exception();
            }

            else if (preset == PreSet.Int64String)
            {
                var count = 0;
                if (format != null)
                    count = format.Count(c => c == '#');
                if (prop.PropertyType == typeof(string))
                    prop.SetValue(Objeto, (Rnd.NextInt64((int)(10 ^ (count - 1)), Int64.MaxValue)).ToString());
                else
                    throw new Exception();
            }

            else if (preset == PreSet.DecimalString)
            {
                if (prop.PropertyType == typeof(string))
                    prop.SetValue(Objeto, ((Rnd.NextDouble() + Rnd.Next())).ToString());
                else
                    throw new Exception();
            }

            else { }

            if (format != null && prop.PropertyType == typeof(string))
                prop.SetValue(Objeto, formatar((string)prop.GetValue(Objeto), format));
        }
        private string formatar(string texto, string formato)
        {
            var texto_formatado = "";
            if (!(formato.Contains('#') || formato.Contains("^#")))
                return "";
            for (int i = 0; i < formato.Length; i++)
            {
                if (formato[i] == '#')
                {
                    texto_formatado = texto_formatado + texto.Substring(0, 1);
                    texto = texto.Substring(1, texto.Length - 1);
                }
                else if (formato[i] == '^' && i < formato.Length - 1)
                {
                    if (formato[i + 1] == '#')
                    {
                        texto_formatado = texto_formatado + texto.Substring(0, 1).ToUpper();
                        texto = texto.Substring(1, texto.Length - 1);
                        i++;
                    }
                }
                else
                    texto_formatado = texto_formatado + formato[i];
            }
            return texto_formatado;
        }
    }
}
