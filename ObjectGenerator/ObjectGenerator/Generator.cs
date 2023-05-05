using System.Reflection;

namespace ObjectGenerator.ObjectGenerator
{
    public class Generator : Mapping
    {
        public T Generate<T>(int sufixo = 1, int? seed = null) where T : new()
        {
            var rnd = seed == null ? new Random() : new Random((int)seed);
            var new_object = new T();
            PropertyInfo[] properties = typeof(T).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (property.PropertyType == typeof(DateTime))
                    property.SetValue(new_object, new DateTime(rnd.NextInt64(628000000000000000, 645000000000000000)));

                else if (property.PropertyType.IsArray)
                {
                    if (property.PropertyType.GetElementType() == typeof(int))
                    {
                        var array = new int[rnd.Next(20)];
                        for (int i = 0; i < array.Length; i++)
                            array[i] = rnd.Next();
                        property.SetValue(new_object, array);
                    }
                    else if (property.PropertyType.GetElementType() == typeof(long))
                    {
                        var array = new long[rnd.Next(20)];
                        for (int i = 0; i < array.Length; i++)
                            array[i] = RandomLong(seed);
                        property.SetValue(new_object, array);
                    }
                    else if (property.PropertyType.GetElementType() == typeof(float))
                    {
                        var array = new float[rnd.Next(20)];
                        for (int i = 0; i < array.Length; i++)
                            array[i] = (float)rnd.NextDouble();
                        property.SetValue(new_object, array);
                    }
                    else if (property.PropertyType.GetElementType() == typeof(double))
                    {
                        var array = new double[rnd.Next(20)];
                        for (int i = 0; i < array.Length; i++)
                            array[i] = rnd.NextDouble();
                        property.SetValue(new_object, array);
                    }
                    else if (property.PropertyType.GetElementType() == typeof(bool))
                    {
                        var array = new bool[rnd.Next(20)];
                        for (int i = 0; i < array.Length; i++)
                            array[i] = rnd.Next(2) == 1;
                        property.SetValue(new_object, array);
                    }
                    else if (property.PropertyType.GetElementType() == typeof(char))
                    {
                        var abc = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                        var array = new char[rnd.Next(20)];
                        for (int i = 0; i < array.Length; i++)
                            array[i] = abc[rnd.Next(26)];
                        property.SetValue(new_object, array);
                    }
                    else if (property.PropertyType.GetElementType() == typeof(string))
                    {
                        var array = new string[rnd.Next(20)];
                        for (int i = 0; i < array.Length; i++)
                            array[i] = $"{new_object.GetType().Name}.{property.Name}.{i}";
                        property.SetValue(new_object, array);
                    }
                    else if (property.PropertyType.GetElementType() == typeof(decimal))
                    {
                        var array = new decimal[rnd.Next(20)];
                        for (int i = 0; i < array.Length; i++)
                            array[i] = (decimal)(rnd.NextDouble() + rnd.Next(100));
                        property.SetValue(new_object, array);
                    }
                    else if (property.PropertyType.GetElementType() != null && property.PropertyType.GetElementType().BaseType == typeof(Enum))
                    {
                        var array = Array.CreateInstance(property.PropertyType.GetElementType(), rnd.Next(20));
                        Array enum_values = Enum.GetValues(property.PropertyType.GetElementType());
                        for (int i = 0; i < array.Length; i++)
                            array.SetValue(enum_values.GetValue(rnd.Next(enum_values.Length)), i);
                        property.SetValue(new_object, array);
                    }
                }

                else if (property.PropertyType == typeof(int) || Nullable.GetUnderlyingType(property.PropertyType) == typeof(int))
                    property.SetValue(new_object, rnd.Next());
                else if (property.PropertyType == typeof(long) || Nullable.GetUnderlyingType(property.PropertyType) == typeof(long))
                    property.SetValue(new_object, RandomLong(seed));
                else if (property.PropertyType == typeof(float) || Nullable.GetUnderlyingType(property.PropertyType) == typeof(float))
                    property.SetValue(new_object, (float)rnd.NextDouble());
                else if (property.PropertyType == typeof(double) || Nullable.GetUnderlyingType(property.PropertyType) == typeof(double))
                    property.SetValue(new_object, rnd.NextDouble());
                else if (property.PropertyType == typeof(bool) || Nullable.GetUnderlyingType(property.PropertyType) == typeof(bool))
                    property.SetValue(new_object, rnd.Next(2) == 1);
                else if (property.PropertyType == typeof(char) || Nullable.GetUnderlyingType(property.PropertyType) == typeof(char))
                    property.SetValue(new_object, "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[rnd.Next(26)]);
                else if (property.PropertyType == typeof(string) || Nullable.GetUnderlyingType(property.PropertyType) == typeof(string))
                    property.SetValue(new_object, $"{new_object.GetType().Name}.{property.Name}.{sufixo}");
                else if (property.PropertyType == typeof(decimal) || Nullable.GetUnderlyingType(property.PropertyType) == typeof(decimal))
                    property.SetValue(new_object, (decimal)(rnd.NextDouble() + rnd.Next(100)));
                else if (property.PropertyType.BaseType != null)
                {
                    if (property.PropertyType.BaseType == typeof(Enum) || Nullable.GetUnderlyingType(property.PropertyType.BaseType) == typeof(Enum))
                    {
                        Array enum_values = Enum.GetValues(property.PropertyType);
                        var enum_value = enum_values.GetValue(rnd.Next(enum_values.Length));
                        property.SetValue(new_object, enum_value);
                    }
                }

            }
            return RunMap<T>(new_object, rnd);
        }
        public List<T> GenerateList<T>(int? entries = null, int? seed = null) where T : new()
        {
            var rnd = seed == null ? new Random() : new Random((int)seed);
            entries = entries == null ? rnd.Next(1, 50) : entries;
            List<T> lst = new List<T>();
            for (int i = 1; i <= entries; i++)
            {
                if (typeof(T).BaseType == typeof(object))
                    lst.Add(Generate<T>(i));
                else if (typeof(T) == typeof(int))
                    lst.Add((T)Convert.ChangeType(rnd.Next(), typeof(T)));
                else if (typeof(T) == typeof(long))
                    lst.Add((T)Convert.ChangeType(RandomLong(seed), typeof(T)));
                else if (typeof(T) == typeof(float))
                    lst.Add((T)Convert.ChangeType((float)rnd.NextDouble(), typeof(T)));
                else if (typeof(T) == typeof(double))
                    lst.Add((T)Convert.ChangeType(rnd.NextDouble(), typeof(T)));
                else if (typeof(T) == typeof(bool))
                    lst.Add((T)Convert.ChangeType(rnd.Next(2) == 1, typeof(T)));
                else if (typeof(T) == typeof(char))
                    lst.Add((T)Convert.ChangeType("ABCDEFGHIJKLMNOPQRSTUVWXYZ"[rnd.Next(26)], typeof(T)));
                else if (typeof(T) == typeof(string))
                    lst.Add((T)Convert.ChangeType($"String.{i}", typeof(T)));
                else if (typeof(T) == typeof(decimal))
                    lst.Add((T)Convert.ChangeType((decimal)(rnd.NextDouble() + rnd.Next(100)), typeof(T)));
                else if (typeof(T).BaseType == typeof(Enum))
                {
                    Array enum_values = Enum.GetValues(typeof(T));
                    var enum_value = enum_values.GetValue(rnd.Next(enum_values.Length));
                    lst.Add((T)Convert.ChangeType(enum_value, typeof(T)));
                }
                else if (typeof(T) == typeof(DateTime))
                    lst.Add((T)Convert.ChangeType(new DateTime(rnd.NextInt64(628000000000000000, 645000000000000000)), typeof(T)));
            }
            return lst;
        }
        public IList<T> GenerateIList<T>(int? entries = null, int? seed = null) where T : new() => GenerateList<T>(entries, seed);

        #region [ Private ]
        private static long RandomLong(int? seed = null)
        {
            var rnd = seed == null ? new Random() : new Random((int)seed);
            byte[] bytes = new byte[8];
            rnd.NextBytes(bytes);
            return BitConverter.ToInt64(bytes, 0);
        }
        private T RunMap<T>(T objeto, Random rnd)
        {
            foreach (var map in Mappings)
            {
                if (map.Type == typeof(T))
                    map.Rnd = rnd;
                return map.run<T>(objeto);
            }
            throw new Exception();
        }
    }

    #endregion [ Private ]

}
