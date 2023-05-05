using ObjectGenerator;
using ObjectGenerator.ObjectGenerator_v3;
using System.Reflection;



var map = new Map();
var generator = new Generator(map);

var cliente = new ClienteModel();
generator.Populate(cliente);
Console.WriteLine(cliente.id);

PropertyInfo[] properties = cliente.GetType().GetProperties();
var teste = properties[0].Name;

var lista = new List<string>();
generator.PopulateList(lista);
Console.WriteLine(cliente.id);

