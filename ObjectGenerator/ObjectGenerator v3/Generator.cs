using ObjectGenerator.ObjectGenerator_v2;
using System.Reflection;
using System.Text;
using System.Text.Json;


namespace ObjectGenerator.ObjectGenerator_v3
{
    public class Generator
    {
        #region [ Variables ]

        private Random? Rnd;
        int Count = 0;

        #endregion [ Variables ]

        #region [ Constants | ReadOnly ]

        private readonly BaseMap Map;
        private readonly PresetData GeneratorBase;
        const int max_array_items = 20;
        const int max_list_items = 50;

        #endregion [ Constants | ReadOnly ]

        #region [ Constructor ]
        public Generator(BaseMap map)
        {
            using (var json_stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ObjectGenerator.ObjectGenerator_v3.generator_data.json"))
                GeneratorBase = JsonSerializer.Deserialize<PresetData>(json_stream) ?? new PresetData();
            Map = map;
        }

        #endregion [ Constructor ]

        #region [ Public ]

        public void Populate(object obj, int? seed = null)
        {
            Rnd = Rnd == null ? (seed == null ? new Random() : new Random((int) seed)) : Rnd;
            var ruleSet = Map.Rules.Where(x => x.Type == obj.GetType()).FirstOrDefault();
            var allRules = ruleSet == null ? new List<Rule>() : ruleSet.Rules;

            PropertyInfo[] properties = obj.GetType().GetProperties();

            //Basic operations of seting value: fixed value, random value or preset value.
            //Also get conditions for min and max value when needed.
            foreach (PropertyInfo property in properties)
            {
                var propertyRules = allRules.Where(x => x.Property == property).ToList();
                var setPropertyRules = propertyRules.Where(x => x.RuleType == RuleType.SetProperty).ToList();
                var setPropertyFromPropertyRules = propertyRules.Where(x => x.RuleType == RuleType.SetPropertyFromProperty).ToList();
                var conditionalRules = propertyRules.Where(x => x.RuleType == RuleType.Conditional).ToList();
                
                
                long? min = null, max = null;
                dynamic pool = null;
                if (conditionalRules.Any())
                {
                    if (conditionalRules.Where(x => x.Operator == Operator.GreaterThan).Any())
                    {
                        var value = conditionalRules.Where(x => x.Operator == Operator.GreaterThan).Last().Value;
                        min = value is DateTime ? value.Ticks : value;
                    }
                    if (conditionalRules.Where(x => x.Operator == Operator.LessThan).Any())
                    {
                        var value = conditionalRules.Where(x => x.Operator == Operator.LessThan).Last().Value;
                        max = value is DateTime ? value.Ticks : value;
                    }
                    if (conditionalRules.Where(x => x.Operator == Operator.Between).Any())
                        pool = conditionalRules.Where(x => x.Operator == Operator.Between).Last().Value;
                }

                if (setPropertyRules.Any() && !setPropertyFromPropertyRules.Any())
                {
                    foreach (var rule in setPropertyRules)
                    {
                        if (rule.Value != null)
                            SetValue(obj, property, rule.Value);
                        else if (rule.Preset != null)
                            SetPreset(obj, property, rule.Preset, min, max);
                    }
                }
                else if (pool != null)
                    SetValue(obj, property, pool[Rnd.Next(pool.Length)]);
                else
                    SetRandom(obj, property, null, min, max);
            }

            //Set property from property action.
            foreach (var rule in allRules.Where(x => x.RuleType == RuleType.SetPropertyFromProperty).ToList())
                SetPropertyFromProperty(obj, rule.Property, rule.PropertySource);
            
            //Format action.
            foreach (var rule in allRules.Where(x => x.RuleType == RuleType.PostExecution).ToList())
            {
                if (rule.Format != null)
                {
                    if (rule.Property.PropertyType == typeof(string))
                        rule.Property.SetValue(obj, Format(rule.Property.GetValue(obj).ToString(), rule.Format));
                    else
                        rule.Property.SetValue(obj, FormatDecimalPlaces(rule.Property.GetValue(obj).ToString(), rule.Format));
                }
            }
        }
        public T Generate<T>(int? seed = null) where T : new()
        {
            var obj = new T();
            Populate(obj, seed);
            return obj;
        }

        public void PopulateList<T>(IList<T> lst, int? seed = null, ListRuleSet? ruleSet = null)
        {
            Rnd = Rnd == null ? (seed == null ? new Random() : new Random((int)seed)) : Rnd;
            var type = typeof(T);
            var numEntries = (new Random()).Next(max_list_items);
            for(var i = 0; i < numEntries; i++)
            {
                if(type.BaseType == typeof(object) && type != typeof(string))
                {
                    var instance = (T)Activator.CreateInstance(type);
                    Populate(instance);
                    lst.Add(instance);
                }
                else
                {
                    dynamic new_value;
                    var rules = ruleSet == null ? new List<Rule>() : ruleSet.Rules;

                    var setPropertyRules = rules.Where(x => x.RuleType == RuleType.SetProperty).ToList();
                    if (setPropertyRules.Any())
                    {
                        foreach (var rule in setPropertyRules)
                        {
                            if (rule.Value != null)
                                new_value = rule.Value;
                            else if (rule.Preset != null)
                                //SetPreset(obj, property, rule.Preset, min, max);
                        }
                    }
                    else
                    {
                        new_value = SetRandom(null, null, type)
                    }

                    foreach (var rule in rules.Where(x => x.RuleType == RuleType.PostExecution).ToList())
                    {
                        if (rule.Format != null)
                        {
                            if (rule.Property.PropertyType == typeof(string))
                                new_value = Format(new_value.ToString(), rule.Format);
                            else
                                new_value = FormatDecimalPlaces(new_value.ToString(), rule.Format);
                        }
                    }
                    lst.Add(new_value);
                }
            }
        }
        public IList<T> GenerateList<T>(int? seed = null)
        {
            var lst = new List<T>();
            PopulateList(lst, seed);
            return lst;
        }

        #endregion [ Public ]

        #region [ Private ]

        private dynamic SetRandom(object? obj, PropertyInfo? property, Type? type = null, long? min = null, long? max = null)
        {
            type = property == null ? type : property.PropertyType;
            var rnd = Rnd;
            dynamic? content = null;
            if (type == null)
                throw new Exception();
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
                    content = rnd.Next((int?)min ?? 0, (int?)max ?? int.MaxValue);
                else if (type == typeof(long) || Nullable.GetUnderlyingType(type) == typeof(long))
                    content = rnd.NextInt64(min ?? 0, max ?? long.MaxValue);
                else if (type == typeof(float) || Nullable.GetUnderlyingType(type) == typeof(float))
                    content = (float)(rnd.Next() + rnd.NextDouble());
                else if (type == typeof(double) || Nullable.GetUnderlyingType(type) == typeof(double))
                    content = rnd.Next((int?)min ?? 0, (int?)max -1 ?? int.MaxValue-1) + rnd.NextDouble();
                else if (type == typeof(bool) || Nullable.GetUnderlyingType(type) == typeof(bool))
                    content = rnd.Next(2) == 1;
                else if (type == typeof(char) || Nullable.GetUnderlyingType(type) == typeof(char))
                    content = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[rnd.Next(26)];
                else if (type == typeof(string) || Nullable.GetUnderlyingType(type) == typeof(string))
                    content = $"String.{Count++}";
                else if (type == typeof(decimal) || Nullable.GetUnderlyingType(type) == typeof(decimal))
                    content = (decimal)(rnd.NextDouble() + rnd.Next((int?)min ?? 0, (int?)max - 1 ?? int.MaxValue - 1));
                else if (type.BaseType == typeof(Enum) || Nullable.GetUnderlyingType(type) == typeof(Enum))
                    content = Enum.GetValues(type).GetValue(rnd.Next(Enum.GetValues(type).Length));
                else if (type == typeof(DateTime) || Nullable.GetUnderlyingType(type) == typeof(DateTime))
                {
                    
                    content = new DateTime(rnd.NextInt64(min ?? 628000000000000000, max ?? 645000000000000000));
                }
                else if (type.BaseType == typeof(object))
                {
                    content = Activator.CreateInstance(property.PropertyType);
                    Populate(content);
                }
            }
            if(property != null && obj != null)
                property.SetValue(obj, content);
            return content;
        }
        private void SetValue(object obj, PropertyInfo property, dynamic value)
        {
            property.SetValue(obj, value);
        }
        private void SetPreset(object obj, PropertyInfo property, Preset? preset, long? min = null, long? max = null)
        {
            dynamic? content = null;
            var rnd = Rnd;
            var type = property.PropertyType;

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
                if (type == typeof(string))
                    content = rnd.Next((int?)min ?? 0, (int?)max ?? int.MaxValue).ToString();
                else
                    throw new Exception();
            }
            else if (preset == Preset.Int64String)
            {
                if (type == typeof(string))
                    content = rnd.NextInt64(min ?? 0, max ?? long.MaxValue).ToString();
                else
                    throw new Exception();
            }
            else if (preset == Preset.DecimalString)
            {
                if (type == typeof(string))
                    content = (rnd.NextDouble() + rnd.Next((int?)min ?? 0, (int?)max - 1 ?? int.MaxValue - 1)).ToString();
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
            property.SetValue(obj, content);
        }
        private void SetPropertyFromProperty(object obj, PropertyInfo property, PropertyInfo sourceProperty)
        {
            property.SetValue(obj, sourceProperty.GetValue(obj));
        }

        private string? Format(string text, string format)
        {
            var texto_formatado = "";
            if (!(format.Contains('#') || format.Contains("^#")))
                return null;
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
        private string? FormatDecimalPlaces(string text, string format)
        {
            var texto_formatado = "";
            if (!(format.Contains('#') || format.Contains("^#")))
                return null;
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
            return texto_formatado;
        }

        #endregion [ Private ]
    }

}
