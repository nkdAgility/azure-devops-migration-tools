using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MigrationTools.Configuration.Tests
{
    [TestClass]
    public class SerializationTests
    {
        private Zoo CreateZoo()
        {
            var animal = new Animal { Name = "Martin", AnimalType = "Monkey" };
            var cage = new Cage { Adress = "Scottland", Animals = new List<Animal> { animal } };
            var zoo = new Zoo { Name = "Migration", Cages = new List<Cage> { cage } };
            return zoo;
        }

        [TestMethod]
        public void TestSerialization()
        {
            var zoo = CreateZoo();
            string output = JsonConvert.SerializeObject(zoo);
            Console.WriteLine(output);

            KnownTypesBinder knownTypesBinder = new KnownTypesBinder
            {
                KnownTypes = new List<Type> { typeof(Zoo), typeof(Cage), typeof(Animal) }
            };

            string json = JsonConvert.SerializeObject(zoo, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                SerializationBinder = knownTypesBinder
            });

            Console.WriteLine(json);

            var deserializedZoo = JsonConvert.DeserializeObject(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                SerializationBinder = knownTypesBinder
            });

            Console.WriteLine(deserializedZoo);
        }
    }

    public class Zoo
    {
        public string Name { get; set; }
        public List<Cage> Cages { get; set; }
    }

    public class Cage
    {
        public string Adress { get; set; }
        public List<Animal> Animals { get; set; }
    }

    public class Animal
    {
        public string Name { get; set; }
        public string AnimalType { get; set; }
    }

    public class KnownTypesBinder : ISerializationBinder
    {
        public IList<Type> KnownTypes { get; set; }

        public Type BindToType(string assemblyName, string typeName)
        {
            return KnownTypes.SingleOrDefault(t => t.Name == typeName);
        }

        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            typeName = serializedType.Name;
        }
    }

}
