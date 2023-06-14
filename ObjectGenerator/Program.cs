using ObjectGenerator;
using ObjectGenerator.ObjectGenerator_v3;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;


//List<int> teste = new List<int>() { 1};
//var a = teste.GetType().GetElementType();

var map = new Map();
var generator = new Generator(map);

var cliente = new ClienteModel();
generator.Populate(cliente);
Console.WriteLine(cliente);
//PropertyInfo[] properties = cliente.GetType().GetProperties();
//var teste = properties[0].Name;

//var lista = new List<string>();
//generator.PopulateList(lista);
//Console.WriteLine(cliente);
//object[] teste = new object[1];

//teste[0] = new ClienteModel()
//{
//    id = 1,
//};
//var outro = teste.ToList();

//Console.WriteLine(outro[0]);

//var obj = new ClienteModel();
//obj = (ClienteModel)outro[0];
//Console.WriteLine(obj.id);

//var teste = new ClienteModel();
//PropertyInfo[] properties = teste.GetType().GetProperties();
//var a = properties[0].PropertyType.GenericTypeArguments[0];
//Console.WriteLine((properties[0].GetType().IsGenericType && (properties[0].GetType().GetGenericTypeDefinition() == typeof(List<>))));

//void teste(Func<dynamic, bool> action)
//{
//    var a = new ClienteModel()
//    {
//        id = 1,
//    };
//    var teste = action(a);
//    Console.WriteLine(teste);
//}

//teste(x => x.id == 1);