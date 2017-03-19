using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Blueshift.EntityFrameworkCore.Annotations;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Blueshift.EntityFrameworkCore.MongoDb.SampleDomain
{
    [MongoDatabase("zooDb")]
    public class ZooDbContext : DbContext
    {
        public DbSet<Animal> Animals { get; set; }
        public DbSet<Employee> Employees { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMongoDb($"mongodb://localhost");
        }
    }

    [BsonKnownTypes(typeof(Tiger), typeof(PolarBear), typeof(Otter))]
    [BsonDiscriminator(Required = true, RootClass = true)]
    public abstract class Animal
    {
        public ObjectId Id { get; private set; }
        public double Age { get; set; }
        public double Height { get; set; }
        public double Weight { get; set; }
    }

    [BsonDiscriminator("panthera tigris")]
    public class Tiger { }

    [BsonDiscriminator("Ursus maritimus")]
    public class PolarBear { }

    [BsonDiscriminator("Lutrinae")]
    public class Otter { }

    public class Employee
    {
        public ObjectId Id { get; private set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [BsonIgnore]
        public string FullName => string.IsNullOrWhiteSpace(FirstName)
            ? LastName
            : $"{LastName}, {FirstName}";

        public double Age { get; set; }
        public List<Specialty> Specialties { get; set; }
    }

    public enum ZooTask
    {
        Feeding,
        Training,
        Exercise,
        TourGuide
    }

    [ComplexType]
    public class Specialty
    {
        public string AnimalType { get; set; }
        public ZooTask Task { get; set; }
    }
}