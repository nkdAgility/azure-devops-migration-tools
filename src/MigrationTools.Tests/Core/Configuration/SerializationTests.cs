using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Tests;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;

namespace MigrationTools.Configuration.Tests
{
    [TestClass]
    public class SerializationTests
    {
        private ServiceProvider Services;

        [TestInitialize]
        public void Setup()
        {
            Services = ServiceProviderHelper.GetWorkItemMigrationProcessor();
        }

        private Zoo CreateZoo()
        {
            var animal = new LandAnimal { Name = "Martin", AnimalType = "Monkey", Tropical = true };
            var waterAnimal = new WaterAnimal { Name = "Ove", AnimalType = "Fish", FreshWater = false };
            var cage = new Cage { Adress = "Scottland", Animals = new List<Animal> { animal, waterAnimal } };
            var zoo = new Zoo { Name = "Migration", Cages = new List<Cage> { cage } };
            return zoo;
        }

        [TestMethod]
        public void TestSerialization()
        {
            var zoo = CreateZoo();
            string output = JsonConvert.SerializeObject(zoo);
            Console.WriteLine(output);
            var assignableType = typeof(ISuperOptions);
            var superOptions = AppDomain.CurrentDomain.GetAssemblies().ToList()
                   .SelectMany(x => x.GetTypes())
                   .Where(t => assignableType.IsAssignableFrom(t) && t.IsClass).ToList();
            var knownTypes = new List<Type> { typeof(Zoo) };
            knownTypes.AddRange(superOptions);
            KnownTypesBinder knownTypesBinder = new KnownTypesBinder
            {
                KnownTypes = knownTypes
            };

            string json = JsonConvert.SerializeObject(zoo, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                SerializationBinder = knownTypesBinder
            });

            Log.Information(json);

            var deserializedZoo = JsonConvert.DeserializeObject(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                SerializationBinder = knownTypesBinder
            });

            Log.Information(deserializedZoo.ToString());
        }
    }

    public class Zoo
    {
        public string Name { get; set; }
        public List<Cage> Cages { get; set; }
    }

    public interface ISuperOptions
    {
        string Name { get; }
    }

    public class Cage : ISuperOptions
    {
        public string Adress { get; set; }
        public List<Animal> Animals { get; set; }

        public string Name { get; set; }
    }

    public class Animal : ISuperOptions
    {
        public string Name { get; set; }
        public string AnimalType { get; set; }
    }

    public class WaterAnimal : Animal
    {
        public bool FreshWater { get; set; }
    }

    public class LandAnimal : Animal
    {
        public bool Tropical { get; set; }
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