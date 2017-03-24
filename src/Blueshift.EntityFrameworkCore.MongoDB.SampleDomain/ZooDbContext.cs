using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Blueshift.EntityFrameworkCore.MongoDB.Annotations;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace Blueshift.EntityFrameworkCore.MongoDB.SampleDomain
{
    [MongoDatabase("zooDb")]
    public class ZooDbContext : DbContext
    {
        public DbSet<Animal> Animals { get; set; }
        public DbSet<Employee> Employees { get; set; }

        public ZooDbContext()
            : this(new DbContextOptions<ZooDbContext>())
        {
        }

        public ZooDbContext(DbContextOptions<ZooDbContext> zooDbContextOptions)
            : base(zooDbContextOptions)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = "mongodb://localhost";
            //optionsBuilder.UseMongoDb(connectionString);

            var mongoUrl = new MongoUrl(connectionString);
            //optionsBuilder.UseMongoDb(mongoUrl);

            MongoClientSettings settings = MongoClientSettings.FromUrl(mongoUrl);
            //settings.SslSettings = new SslSettings
            //{
            //    EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12
            //};
            //optionsBuilder.UseMongoDb(settings);

            MongoClient mongoClient = new MongoClient(settings);
            optionsBuilder.UseMongoDb(mongoClient);
        }
    }

    [BsonKnownTypes(typeof(Tiger), typeof(PolarBear), typeof(Otter))]
    [BsonDiscriminator(Required = true, RootClass = true)]
    public abstract class Animal
    {
        [BsonId]
        public ObjectId Id { get; private set; }
        public string Name { get; set; }
        public double Age { get; set; }
        public double Height { get; set; }
        public double Weight { get; set; }
    }

    [BsonDiscriminator("panthera tigris")]
    public class Tiger : Animal { }

    [BsonDiscriminator("Ursus maritimus")]
    public class PolarBear : Animal { }

    [BsonDiscriminator("Lutrinae")]
    [BsonKnownTypes(typeof(SeaOtter), typeof(EurasianOtter))]
    public abstract class Otter : Animal { }

    [BsonDiscriminator("Enhydra lutris")]
    public class SeaOtter : Otter { }

    [BsonDiscriminator("Lutra lutra")]
    public class EurasianOtter : Otter { }

    public class Employee
    {
        [Key]
        public ObjectId Id { get; private set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [BsonIgnore]
        public string FullName => string.IsNullOrWhiteSpace(FirstName)
            ? LastName
            : $"{LastName}, {FirstName}";

        public double Age { get; set; }
        public List<Specialty> Specialties { get; set; } = new List<Specialty>();
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