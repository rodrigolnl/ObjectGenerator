
using Newtonsoft.Json;
using System.Reflection;
using System.Text;

namespace ObjectGenerator.ObjectGenerator_v2
{
    public class Generator : Map
    {
        #region [ Variables ]

        GeneratorBase? GeneratorBase;
        int? BaseSeed;
        int Count = 0;

        #endregion [ Variables ]

        #region [ Constants ]
        const int max_array_items = 20;
        const int max_list_items = 50;
        BaseMap Map;

        #endregion [ Constants ]

        #region [ Constructor ]
        public Generator(BaseMap map, int? seed = null)
        {
            using var r = new StreamReader("C:\\Users\\rodrigo.lima\\source\\repos\\ObjectGenerator\\ObjectGenerator\\ObjectGenerator v2\\base_generator.json");
            string json = r.ReadToEnd();
            GeneratorBase = JsonConvert.DeserializeObject<GeneratorBase>(json);
            BaseSeed = seed;
            Map = map;
        }

        #endregion [ Constructor ]

        #region [ Public ]

        public T Generate<T>(int? seed = null, Random? random = null)
        {
            seed = seed == null ? BaseSeed : seed;
            var ruleSet = Map.Rules.Where(x => x.Type == typeof(T)).FirstOrDefault();
            var rnd = seed == null ? new Random() : new Random((int)seed);
            rnd = random == null ? rnd : random;
            var new_object = Activator.CreateInstance<T>();
            PropertyInfo[] properties = typeof(T).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (!(property.PropertyType != typeof(string) && property.PropertyType.BaseType == typeof(object)))
                {
                    var rules = new List<Rule>();
                    if (ruleSet != null)
                        rules = ruleSet.Rules.Where(x => x.Property == property).ToList();
                    if (rules.Count > 0)
                    {
                        foreach (var rule in rules)
                        {
                            var encapsuled_value = rule.Value != null ? new List<dynamic>() { rule.Value } : null;
                            NewRandom<T>(rnd, SetValue, rule.Preset, encapsuled_value, rule.Format, new_object, property);
                        }
                    }
                    else
                    {
                        NewRandom<T>(rnd, SetValue, null, null, null, new_object, property);
                    }
                }
            }
            PostGenerationActions(new_object, ruleSet);
            return new_object;
        }
        public List<T> GenerateList<T>(int? seed = null, Preset? preset = null, dynamic? value = null, string? format = null, int? entries = null)
        {
            seed = seed == null ? BaseSeed : seed;
            var rnd = seed == null ? new Random() : new Random((int)seed);
            var temp_rnd = new Random();
            entries = entries == null ? temp_rnd.Next(1, max_list_items) : entries;
            List<T> lst = new List<T>();
            for (int i = 1; i <= entries; i++)
            {
                var encapsuled_value = value != null ? new List<dynamic>() { value } : null;
                NewRandom<T>(rnd, lst.Add, preset, encapsuled_value, format, null, null);
            }
            return lst;
        }
        public List<T> GenerateList<T>(int? seed, string format) => GenerateList<T>(seed, null, null, format, null);
        public List<T> GenerateList<T>(int? seed, int entries) => GenerateList<T>(seed, null, null, null, entries);

        #endregion [ Public ]

        #region [ Private ]

        private static string Format(string text, string format)
        {
            var texto_formatado = "";
            if (!(format.Contains('#') || format.Contains("^#")))
                return "";
            for (int i = 0; i < format.Length; i++)
            {
                if (format[i] == '#')
                {
                    texto_formatado = texto_formatado + text.Substring(0, 1);
                    text = text.Substring(1, text.Length - 1);
                }
                else if (format[i] == '^' && i < format.Length - 1)
                {
                    if (format[i + 1] == '#')
                    {
                        texto_formatado = texto_formatado + text.Substring(0, 1).ToUpper();
                        text = text.Substring(1, text.Length - 1);
                        i++;
                    }
                }
                else
                    texto_formatado = texto_formatado + format[i];
            }
            return texto_formatado;
        }
        private static string FormatPlaces(string text, string format)
        {
            var texto_formatado = "";
            if (!(format.Contains('#') || format.Contains("^#")))
                return "";
            format = format.Replace(",", ".");
            format = format.Contains('.') ? format : format + ".";
            text = text.Replace(",", ".");
            text = text.Contains('.') ? text : text + ".";
            var format_int = format.Split(".")[0];
            var format_dec = format.Split(".")[1];

            var text_int = text.Split(".")[0];
            var text_dec = text.Split(".")[1];
            var signal = "";
            if (text_int[0] == '-')
            {
                signal = "-";
                text_int = text_int.Substring(1);
            }

            while (text_int.Length < format_int.Length)
                text_int = "0" + text_int;
            if (format_int.Contains('*'))
                texto_formatado += text_int;
            else
                for (var i = 0; i < format_int.Length; i++)
                    texto_formatado += text_int[i];

            while (text_dec.Length < format_dec.Length)
                text_dec = text_dec + "0";
            if (format_dec.Length > 0)
                texto_formatado += ",";
            if (format_dec.Contains('*'))
                texto_formatado += text_dec;
            else
                for (var i = 0; i < format_dec.Length; i++)
                    texto_formatado += text_dec[i];

            return signal + texto_formatado;
        }
        private static void SetValue(PropertyInfo property, object obj, dynamic value) => property.SetValue(obj, value);
        private void NewRandom<T>(Random rnd, Delegate action, Preset? preset = null, List<dynamic>? encapsuled_value = null, string? format = null, object? obj = null, PropertyInfo? property = null)
        {
            dynamic? content = null;
            var type = property == null ? typeof(T) : property.PropertyType;

            if (preset != null && preset != Preset.ApenasFormatar)
            {
                if (preset == Preset.Nome)
                {
                    var sexo = rnd.Next(2);
                    content = sexo == 0 ? GeneratorBase.Masculinos[rnd.Next(GeneratorBase.Masculinos.Count)] : GeneratorBase.Femininos[rnd.Next(GeneratorBase.Femininos.Count)];
                }
                else if (preset == Preset.Sobrenome)
                {
                    var sobrenome = "";
                    var length = rnd.Next(1, 4);
                    for (var i = 0; i < length; i++)
                    {
                        sobrenome += GeneratorBase.Sobrenomes[rnd.Next(GeneratorBase.Sobrenomes.Count)];
                        sobrenome += length - 1 == i ? "" : " ";
                    }
                    content = sobrenome;
                }
                else if (preset == Preset.NomeCompleto)
                {
                    var sexo = rnd.Next(2);
                    var nome = sexo == 0 ? GeneratorBase.Masculinos[rnd.Next(GeneratorBase.Masculinos.Count)] : GeneratorBase.Femininos[rnd.Next(GeneratorBase.Femininos.Count)];
                    var sobrenome = "";
                    var length = rnd.Next(1, 4);
                    for (var i = 0; i < length; i++)
                    {
                        sobrenome += GeneratorBase.Sobrenomes[rnd.Next(GeneratorBase.Sobrenomes.Count)];
                        sobrenome += length - 1 == i ? "" : " ";
                    }
                    content = nome + " " + sobrenome;
                }
                else if (preset == Preset.Email)
                {
                    var sexo = rnd.Next(2);
                    var nome = sexo == 0 ? GeneratorBase.Masculinos[rnd.Next(GeneratorBase.Masculinos.Count)] : GeneratorBase.Femininos[rnd.Next(GeneratorBase.Femininos.Count)];
                    var sobrenome = GeneratorBase.Sobrenomes[rnd.Next(GeneratorBase.Sobrenomes.Count)];
                    var combinacao = rnd.Next(2);
                    var email = "";
                    switch (combinacao)
                    {
                        case 0:
                            email = nome + sobrenome + "@" + GeneratorBase.Emails[rnd.Next(GeneratorBase.Emails.Count)];
                            break;
                        case 1:
                            email = nome + sobrenome + rnd.Next(999).ToString() + "@" + GeneratorBase.Emails[rnd.Next(GeneratorBase.Emails.Count)];
                            break;
                        default:
                            email = nome + "@" + GeneratorBase.Emails[rnd.Next(GeneratorBase.Emails.Count)];
                            break;
                    }
                    content = email.ToLower().Replace(" ", "");
                }
                else if (preset == Preset.CpfRandom)
                {
                    if (type == typeof(string))
                        content = rnd.NextInt64(10000000000, 99999999999).ToString();
                    else
                        content = rnd.NextInt64(10000000000, 99999999999);
                }
                else if (preset == Preset.CnpjRandom)
                {
                    if (type == typeof(string))
                        content = $"{rnd.NextInt64(10000000, 99999999)}000{rnd.Next(999)}";
                    else
                        content = Convert.ToInt64($"{rnd.NextInt64(10000000, 99999999)}000{rnd.Next(999)}");
                }
                else if (preset == Preset.CpfValido)
                {
                    int soma = 0, resto = 0;
                    int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
                    int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
                    string semente = rnd.Next(100000000, 999999999).ToString();
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
                    if (type == typeof(string))
                        content = semente;
                    else
                        content = Convert.ToInt64(semente);
                }
                else if (preset == Preset.CnpjValido)
                {
                    var cnpj = new int[] { 1, 0, 0, 0 }.Concat((Enumerable.Repeat(0, 8).Select(i => rnd.Next(0, 9)).ToArray())).ToArray();
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
                    if (type == typeof(string))
                        content = string.Join("", cnpj);
                    else
                        content = Convert.ToInt64(string.Join("", cnpj));
                }
                else if (preset == Preset.Telefone)
                {
                    var ddd = GeneratorBase.Prefixos[rnd.Next(GeneratorBase.Prefixos.Count)];
                    var numero = rnd.Next(0, 99999999) + 900000000;
                    if (type == typeof(string))
                        content = (ddd * 100000000 + numero).ToString();
                    else
                        content = ddd * 100000000 + numero;
                }
                else if (preset == Preset.Celular)
                {
                    var ddd = GeneratorBase.Prefixos[rnd.Next(GeneratorBase.Prefixos.Count)];
                    var numero = rnd.Next(0, 99999999) + 900000000;
                    if (type == typeof(string))
                        content = ((long)ddd * (long)1000000000 + (long)numero).ToString();
                    else
                        content = ddd * 1000000000 + numero;
                }
                else if (preset == Preset.Cep)
                {
                    if (type == typeof(string))
                        content = rnd.Next(0, 999999999).ToString();
                    else
                        content = rnd.Next(0, 999999999);
                }
                else if (preset == Preset.Int32String)
                {
                    var count = 0;
                    if (format != null)
                        count = format.Count(c => c == '#');
                    if (type == typeof(string))
                        content = rnd.Next((int)(10 ^ (count - 1)), Int32.MaxValue).ToString();
                    else
                        throw new Exception();
                }
                else if (preset == Preset.Int64String)
                {
                    var count = 0;
                    if (format != null)
                        count = format.Count(c => c == '#');
                    if (type == typeof(string))
                        content = rnd.NextInt64((int)(10 ^ (count - 1)), Int64.MaxValue).ToString();
                    else
                        throw new Exception();
                }
                else if (preset == Preset.DecimalString)
                {
                    if (type == typeof(string))
                        content = (rnd.NextDouble() + rnd.Next()).ToString();
                    else
                        throw new Exception();
                }
                else if (preset == Preset.StringLetrasAleatorias)
                {
                    int length = 100;
                    StringBuilder str_build = new StringBuilder();
                    char letter;
                    for (int i = 0; i < length; i++)
                    {
                        double flt = rnd.NextDouble();
                        int shift = Convert.ToInt32(Math.Floor(25 * flt));
                        letter = Convert.ToChar(shift + 65);
                        str_build.Append(letter);
                    }
                    content = str_build.ToString().ToLower();
                }
            }
            else
            {
                if (type.IsArray)
                {
                    if (type.GetElementType() == typeof(int))
                    {
                        var array = new int[rnd.Next(max_array_items)];
                        for (int i = 0; i < array.Length; i++)
                            array[i] = rnd.Next();
                        content = array;
                    }
                    else if (type.GetElementType() == typeof(long))
                    {
                        var array = new long[rnd.Next(max_array_items)];
                        for (int i = 0; i < array.Length; i++)
                            array[i] = rnd.NextInt64();
                        content = array;
                    }
                    else if (type.GetElementType() == typeof(float))
                    {
                        var array = new float[rnd.Next(max_array_items)];
                        for (int i = 0; i < array.Length; i++)
                            array[i] = (float)rnd.NextDouble();
                        content = array;
                    }
                    else if (type.GetElementType() == typeof(double))
                    {
                        var array = new double[rnd.Next(max_array_items)];
                        for (int i = 0; i < array.Length; i++)
                            array[i] = rnd.NextDouble();
                        content = array;
                    }
                    else if (type.GetElementType() == typeof(bool))
                    {
                        var array = new bool[rnd.Next(max_array_items)];
                        for (int i = 0; i < array.Length; i++)
                            array[i] = rnd.Next(2) == 1;
                        content = array;
                    }
                    else if (type.GetElementType() == typeof(char))
                    {
                        var abc = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                        var array = new char[rnd.Next(max_array_items)];
                        for (int i = 0; i < array.Length; i++)
                            array[i] = abc[rnd.Next(26)];
                        content = array;
                    }
                    else if (type.GetElementType() == typeof(string))
                    {
                        var array = new string[rnd.Next(max_array_items)];
                        for (int i = 0; i < array.Length; i++)
                            array[i] = $"String.{i}";
                        content = array;
                    }
                    else if (type.GetElementType() == typeof(decimal))
                    {
                        var array = new decimal[rnd.Next(max_array_items)];
                        for (int i = 0; i < array.Length; i++)
                            array[i] = (decimal)(rnd.NextDouble() + rnd.Next(100));
                        content = array;
                    }
                    else if (type.GetElementType() != null && type.GetElementType().BaseType == typeof(Enum))
                    {
                        var array = Array.CreateInstance(type.GetElementType(), rnd.Next(max_array_items));
                        Array enum_values = Enum.GetValues(type.GetElementType());
                        for (int i = 0; i < array.Length; i++)
                            array.SetValue(enum_values.GetValue(rnd.Next(enum_values.Length)), i);
                        content = array;
                    }
                }
                else
                {
                    if (type == typeof(int) || Nullable.GetUnderlyingType(type) == typeof(int))
                        content = rnd.Next();
                    else if (type == typeof(long) || Nullable.GetUnderlyingType(type) == typeof(long))
                        content = rnd.NextInt64();
                    else if (type == typeof(float) || Nullable.GetUnderlyingType(type) == typeof(float))
                        content = (float)(rnd.Next() + rnd.NextDouble());
                    else if (type == typeof(double) || Nullable.GetUnderlyingType(type) == typeof(double))
                        content = rnd.Next() + rnd.NextDouble();
                    else if (type == typeof(bool) || Nullable.GetUnderlyingType(type) == typeof(bool))
                        content = rnd.Next(2) == 1;
                    else if (type == typeof(char) || Nullable.GetUnderlyingType(type) == typeof(char))
                        content = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[rnd.Next(26)];
                    else if (type == typeof(string) || Nullable.GetUnderlyingType(type) == typeof(string))
                        content = $"String.{Count++}";
                    else if (type == typeof(decimal) || Nullable.GetUnderlyingType(type) == typeof(decimal))
                        content = (decimal)(rnd.NextDouble() + rnd.Next());
                    else if (type.BaseType == typeof(Enum) || Nullable.GetUnderlyingType(type) == typeof(Enum))
                        content = Enum.GetValues(type).GetValue(rnd.Next(Enum.GetValues(type).Length));
                    else if (type == typeof(DateTime) || Nullable.GetUnderlyingType(type) == typeof(DateTime))
                        content = new DateTime(rnd.NextInt64(628000000000000000, 645000000000000000));
                    else if (type.BaseType == typeof(object))
                        content = Generate<T>(null, rnd);
                }
            }
            var teste = content;
            if (encapsuled_value != null)
                content = encapsuled_value[0];

            if (format != null)
            {
                if (type == typeof(decimal))
                    content = Convert.ToDecimal(FormatPlaces(Convert.ToString(content), format));
                else if (type == typeof(float))
                    content = (float)Convert.ToDouble(FormatPlaces(Convert.ToString(content), format));
                else if (type == typeof(double))
                    content = Convert.ToDouble(FormatPlaces(Convert.ToString(content), format));
                else if (type == typeof(Int32))
                    content = Convert.ToInt32(Format(Convert.ToString(content), format));
                else if (type == typeof(Int64))
                    content = Convert.ToInt64(Format(Convert.ToString(content), format));
                else if (type == typeof(string))
                    content = Format(Convert.ToString(content), format);
            }

            if (obj != null)
                action.DynamicInvoke(property, obj, content);
            else
                action.DynamicInvoke(content);
        }
        private void PostGenerationActions(object obj, IRuleSet ruleSet)
        {
            if (ruleSet != null)
            {
                foreach (var rule in ruleSet.Rules)
                {
                    if (rule.PropertySource != null)
                        rule.Property.SetValue(obj, rule.PropertySource.GetValue(obj));
                }
            }

        }

        #endregion [ Private ]
    }

}
