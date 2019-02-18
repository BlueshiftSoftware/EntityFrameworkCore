using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Blueshift.EntityFrameworkCore.MongoDB.Annotations;
using Blueshift.EntityFrameworkCore.MongoDB.Infrastructure;
using JetBrains.Annotations;
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
        public DbSet<Enclosure> Enclosures { get; set; }

        public ZooDbContext(DbContextOptions<ZooDbContext> zooDbContextOptions)
            : base(zooDbContextOptions)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            const string connectionString = "mongodb://localhost";
            //optionsBuilder.UseMongoDb(connectionString);

            var mongoUrl = new MongoUrl(connectionString);
            //optionsBuilder.UseMongoDb(mongoUrl);

            MongoClientSettings settings = MongoClientSettings.FromUrl(mongoUrl);
            //settings.SslSettings = new SslSettings
            //{
            //    EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12
            //};

            optionsBuilder.UseMongoDb(settings);
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }

    [BsonKnownTypes(typeof(Tiger), typeof(PolarBear), typeof(Otter))]
    [BsonDiscriminator(RootClass = true)]
    public abstract class Animal
    {
        // When using attribute-driven data modeling, either [BsonId] or [Key]
        // is required for the primary key field; either will work with the
        // MongoDb C# driver EFCore adapter
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ObjectId AnimalId { get; [UsedImplicitly] private set; }

        public string Name { get; set; }

        public decimal Age { get; set; }

        public decimal Height { get; set; }

        public decimal Weight { get; set; }

        [ConcurrencyCheck, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string ConcurrencyField { get; [UsedImplicitly] private set; }

        [Denormalize(nameof(SampleDomain.Enclosure.Name))]
        public Enclosure Enclosure { get; set; }
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
        // When using attribute-driven data modeling, either [BsonId] or [Key]
        // is required for the primary key field; either will work with the
        // MongoDb C# driver EFCore adapter
        [BsonId, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ObjectId EmployeeId { get; [UsedImplicitly] private set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        [BsonElement]
        public string FullName => string.IsNullOrWhiteSpace(FirstName)
            ? LastName
            : $"{LastName}, {FirstName}";

        public decimal Age { get; set; }

        public IList<Specialty> Specialties { get; set; } = new List<Specialty>();

        [BsonIgnore]
        public string Ignored => $"This string should never show up in the database.";

        public Employee Manager { get; set; }

        public IList<Employee> DirectReports { get; set; } = new List<Employee>();
    }

    public enum ZooTask
    {
        Feeding,
        Training,
        Exercise,
        TourGuide
    }

    [Owned]
    public class Specialty
    {
        public string AnimalType { get; set; }

        public ZooTask Task { get; set; }
    }

    public class Enclosure
    {
        // When using attribute-driven data modeling, either [BsonId] or [Key]
        // is required for the primary key field; either will work with the
        // MongoDb C# driver EFCore adapter
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ObjectId EnclosureId { get; [UsedImplicitly] private set; }

        public string Name { get; set; }

        public string AnimalEnclosureType { get; set; }

        [Denormalize(nameof(Animal.Name))]
        public IList<Animal> Animals { get; [UsedImplicitly] private set; } = new List<Animal>();

        public Schedule WeeklySchedule { get; set; }

    }

    [Owned]
    public class Schedule
    {
        public IList<ZooAssignment> Assignments { get; [UsedImplicitly] private set; } = new List<ZooAssignment>();

        [Denormalize(nameof(Employee.FirstName), nameof(Employee.LastName))]
        public Employee Approver { get; set; }
    }

    public class ZooAssignment
    {
        public TimeSpan Offset { get; set; }

        public ZooTask Task { get; set; }

        [Denormalize(nameof(Employee.FirstName), nameof(Employee.LastName))]
        public Employee Assignee { get; set; }
    }
}