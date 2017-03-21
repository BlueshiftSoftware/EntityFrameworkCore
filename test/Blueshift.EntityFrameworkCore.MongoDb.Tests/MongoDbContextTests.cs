using System;
using System.Collections.Generic;
using System.Linq;
using Blueshift.EntityFrameworkCore.MongoDB.SampleDomain;
using MongoDB.Bson;
using Xunit;

namespace Blueshift.EntityFrameworkCore.MongoDB.Tests
{
    public class MongoDbContextTests : IClassFixture<MongoDbFixture>, IDisposable
    {
        private ZooDbContext _zooDbContext;

        public MongoDbContextTests(MongoDbFixture mongoDbFixture)
        {
            _zooDbContext = mongoDbFixture.ZooDbContext;
            _zooDbContext.Database.EnsureCreated();
        }

        [Fact]
        public void Can_query_from_mongodb()
        {
            Assert.Empty(_zooDbContext.Animals.ToList());
            Assert.Empty(_zooDbContext.Employees.ToList());
        }

        private class FuncEqualityComparer<T> : IEqualityComparer<T>
        {
            private Func<T, T, bool> _comparer;
            public FuncEqualityComparer(Func<T, T, bool> comparer)
            {
                _comparer = comparer;
            }

            public bool Equals(T x, T y) => _comparer(x, y);

            public int GetHashCode(T obj) => obj.GetHashCode();
        }

        private bool AreEqual(Specialty specialtiy1, Specialty specialty2)
            => (specialtiy1 == null && specialty2 == null)
                || (specialtiy1 != null
                    && specialty2 != null
                    && string.Equals(specialtiy1.AnimalType, specialty2.AnimalType, StringComparison.Ordinal)
                    && specialtiy1.Task == specialty2.Task);

        private bool AreEqual(IList<Specialty> specialites1, IList<Specialty> specialties2)
            => (specialites1 == null && specialties2 == null)
                || (specialites1 != null
                    && specialties2 != null
                    && specialites1.Count == specialties2.Count
                    && !specialites1.Where((specialty1, index) => !AreEqual(specialty1, specialties2[index])).Any());

        private bool AreEqual(Employee employee1, Employee employee2)
            => (employee1 == null && employee2 == null)
                || (employee1 != null
                    && employee2 != null
                    && Equals(employee1.Id, employee2.Id)
                    && string.Equals(employee1.FirstName, employee2.FirstName, StringComparison.Ordinal)
                    && string.Equals(employee1.LastName, employee2.LastName, StringComparison.Ordinal)
                    && employee1.Age == employee2.Age
                    && AreEqual(employee1.Specialties, employee2.Specialties));

        private bool AreEqual(Animal animal1, Animal animal2)
            => (animal1 == null && animal2 == null)
                || (animal1 != null
                    && animal2 != null
                    && Equals(animal1.Id, animal2.Id)
                    && animal1.GetType() == animal2.GetType()
                    && string.Equals(animal1.Name, animal2.Name, StringComparison.Ordinal)
                    && animal1.Age == animal2.Age
                    && animal1.Height == animal2.Height
                    && animal1.Weight == animal2.Weight);

        [Fact]
        public void Can_write_simple_record()
        {
            var employee = new Employee { FirstName = "Taiga", LastName = "Masuta", Age = 31.7 };
            _zooDbContext.Add(employee);
            _zooDbContext.SaveChanges(acceptAllChangesOnSuccess: true);
            Assert.Equal(employee, _zooDbContext.Employees.Single(), new FuncEqualityComparer<Employee>(AreEqual));
        }

        [Fact]
        public void Can_write_complex_record()
        {
            var employee = new Employee
            {
                FirstName = "Taiga",
                LastName = "Masuta",
                Age = 31.7,
                Specialties =
                {
                    new Specialty { AnimalType = nameof(Tiger), Task = ZooTask.Feeding },
                    new Specialty { AnimalType = nameof(Tiger), Task = ZooTask.Exercise },
                    new Specialty { AnimalType = nameof(Tiger), Task = ZooTask.TourGuide },
                    new Specialty { AnimalType = nameof(Tiger), Task = ZooTask.TourGuide },
                }
            };
            _zooDbContext.Add(employee);
            _zooDbContext.SaveChanges(acceptAllChangesOnSuccess: true);
            Assert.Equal(employee, _zooDbContext.Employees.Single(), new FuncEqualityComparer<Employee>(AreEqual));
        }

        [Fact]
        public void Can_write_polymorphic_records()
        {
            IList<Animal> insertedEntities = new Animal[]
                {
                    new Tiger { Name = "Tigger", Age = 6.4, Weight = 270, Height = .98 },
                    new PolarBear { Name = "Ursus", Age = 4.9, Weight = 612, Height = 2.7 },
                    new SeaOtter { Name = "Hydron", Age = 1.8, Weight = 19, Height = .3 },
                    new EurasianOtter { Name = "Yuri", Age = 1.8, Weight = 19, Height = .3 }
                }
                .OrderBy(animal => animal.Name)
                .ToList();
            _zooDbContext.Animals.AddRange(insertedEntities);
            _zooDbContext.SaveChanges(acceptAllChangesOnSuccess: true);
            IList<Animal> queriedEntities = _zooDbContext.Animals
                .OrderBy(animal => animal.Name)
                .ToList();
            Assert.Equal(insertedEntities.Count, queriedEntities.Count);
            for (var i = 0; i < insertedEntities.Count; i++)
            {
                Assert.Equal(insertedEntities[i], queriedEntities[i], new FuncEqualityComparer<Animal>(AreEqual));
            }
        }

        [Fact]
        public void Can_query_polymorphic_sub_types()
        {
            IList<Animal> insertedEntities = new Animal[]
                {
                    new Tiger { Name = "Tigger", Age = 6.4, Weight = 270, Height = .98 },
                    new PolarBear { Name = "Ursus", Age = 4.9, Weight = 612, Height = 2.7 },
                    new SeaOtter { Name = "Hydron", Age = 1.8, Weight = 19, Height = .3 },
                    new EurasianOtter { Name = "Yuri", Age = 1.8, Weight = 19, Height = .3 }
                }
                .OrderBy(animal => animal.Name)
                .ToList();
            _zooDbContext.Animals.AddRange(insertedEntities);
            _zooDbContext.SaveChanges(acceptAllChangesOnSuccess: true);
            Assert.Equal(
                insertedEntities.OfType<Tiger>().Single(),
                _zooDbContext.Animals.OfType<Tiger>().Single(),
                new FuncEqualityComparer<Tiger>(AreEqual));
            Assert.Equal(
                insertedEntities.OfType<PolarBear>().Single(),
                _zooDbContext.Animals.OfType<PolarBear>().Single(),
                new FuncEqualityComparer<PolarBear>(AreEqual));
            Assert.Equal(
                insertedEntities.OfType<SeaOtter>().Single(),
                _zooDbContext.Animals.OfType<SeaOtter>().Single(),
                new FuncEqualityComparer<SeaOtter>(AreEqual));
            Assert.Equal(
                insertedEntities.OfType<EurasianOtter>().Single(),
                _zooDbContext.Animals.OfType<EurasianOtter>().Single(),
                new FuncEqualityComparer<EurasianOtter>(AreEqual));
            IList<Otter> insertedOtters = insertedEntities
                .OfType<Otter>()
                .OrderBy(otter => otter.Name)
                .ToList();
            IList<Otter> queriedOtters = _zooDbContext.Animals
                .OfType<Otter>()
                .OrderBy(otter => otter.Name)
                .ToList();
            Assert.Equal(insertedOtters, queriedOtters, new FuncEqualityComparer<Otter>(AreEqual));
        }

        public void Dispose()
        {
            if (_zooDbContext != null)
            {
                _zooDbContext.Database.EnsureDeleted();
                _zooDbContext.Dispose();
                _zooDbContext = null;
            }
        }
    }
}