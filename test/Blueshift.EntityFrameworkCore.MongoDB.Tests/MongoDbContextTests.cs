using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Blueshift.EntityFrameworkCore.MongoDB.SampleDomain;
using Blueshift.MongoDB.Tests.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Xunit;

namespace Blueshift.EntityFrameworkCore.MongoDB.Tests
{
    public class MongoDbContextTests : MongoDbContextTestBase
    {
        public MongoDbContextTests(MongoDbFixture mongoDbFixture)
            : base(mongoDbFixture)
        {
        }

        private static IEqualityComparer<Specialty> SpecialtyComparer
            => new FuncEqualityComparer<Specialty>((specialtiy1, specialty2)
                => string.Equals(specialtiy1.AnimalType, specialty2.AnimalType, StringComparison.Ordinal)
                    && specialtiy1.Task == specialty2.Task);

        private static IEqualityComparer<Employee> EmployeeComparer
            => new FuncEqualityComparer<Employee>((employee1, employee2)
                => Equals(employee1.Id, employee2.Id)
                    && string.Equals(employee1.FirstName, employee2.FirstName, StringComparison.Ordinal)
                    && string.Equals(employee1.LastName, employee2.LastName, StringComparison.Ordinal)
                    && employee1.Age == employee2.Age
                    && EnumerableEqualityComparer<Specialty>.Equals(employee1.Specialties, employee2.Specialties, SpecialtyComparer));

        private static IEqualityComparer<Animal> AnimalComparer
            => new FuncEqualityComparer<Animal>((animal1, animal2)
                => Equals(animal1.Id, animal2.Id)
                    && animal1.GetType() == animal2.GetType()
                    && string.Equals(animal1.Name, animal2.Name, StringComparison.Ordinal)
                    && animal1.Age == animal2.Age
                    && animal1.Height == animal2.Height
                    && animal1.Weight == animal2.Weight);

        [Fact]
        public void Can_query_from_mongodb()
        {
            Assert.Empty(_zooDbContext.Animals.ToList());
            Assert.Empty(_zooDbContext.Employees.ToList());
        }

        [Fact]
        public void Can_write_simple_record()
        {
            var employee = new Employee { FirstName = "Taiga", LastName = "Masuta", Age = 31.7 };
            _zooDbContext.Add(employee);
            _zooDbContext.SaveChanges(acceptAllChangesOnSuccess: true);
            Assert.Equal(employee, _zooDbContext.Employees.Single(), EmployeeComparer);
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
            Assert.Equal(employee, _zooDbContext.Employees
                .Single(searchedEmployee => searchedEmployee.Specialties
                    .Any(speciality => speciality.Task == ZooTask.Feeding)), EmployeeComparer);
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
            Assert.Equal(insertedEntities, queriedEntities, AnimalComparer);
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
                AnimalComparer);
            Assert.Equal(
                insertedEntities.OfType<PolarBear>().Single(),
                _zooDbContext.Animals.OfType<PolarBear>().Single(),
                AnimalComparer);
            Assert.Equal(
                insertedEntities.OfType<SeaOtter>().Single(),
                _zooDbContext.Animals.OfType<SeaOtter>().Single(),
                AnimalComparer);
            Assert.Equal(
                insertedEntities.OfType<EurasianOtter>().Single(),
                _zooDbContext.Animals.OfType<EurasianOtter>().Single(),
                AnimalComparer);
            IList<Otter> insertedOtters = insertedEntities
                .OfType<Otter>()
                .OrderBy(otter => otter.Name)
                .ToList();
            IList<Otter> queriedOtters = _zooDbContext.Animals
                .OfType<Otter>()
                .OrderBy(otter => otter.Name)
                .ToList();
            Assert.Equal(insertedOtters, queriedOtters, AnimalComparer);
        }

        [Fact]
        public async void Can_query_to_list_async()
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
            IList<Animal> queriedEntities = await _zooDbContext.Animals
                .OrderBy(animal => animal.Name)
                .ToListAsync();
            Assert.Equal(insertedEntities, queriedEntities, AnimalComparer);
        }

        [Fact]
        public async void Can_query_first_or_default_async()
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
                await _zooDbContext.Animals.OfType<Tiger>().FirstOrDefaultAsync(),
                AnimalComparer);
        }

        [Fact]
        public void Can_update_existing_entity()
        {
            Animal animal = new Tiger { Name = "Tigger", Age = 6.4, Weight = 270, Height = .98 };
            EntityEntry entityEntry = _zooDbContext.Add(animal);
            Assert.Equal(EntityState.Added, entityEntry.State);
            Assert.Null(animal.ConcurrencyField);
            Assert.Equal(1, _zooDbContext.SaveChanges(acceptAllChangesOnSuccess: true));
            Assert.Equal(EntityState.Unchanged, entityEntry.State);
            Assert.NotNull(animal.ConcurrencyField);

            Assert.NotNull(entityEntry.OriginalValues[nameof(animal.ConcurrencyField)]);

            animal.Name = "Tigra";
            _zooDbContext.ChangeTracker.DetectChanges();
            Assert.Equal(EntityState.Modified, entityEntry.State);
            Assert.Equal(1, _zooDbContext.SaveChanges(acceptAllChangesOnSuccess: true));
        }

        [Fact]
        public void Concurrency_field_prevents_updates()
        {
            Animal animal = new Tiger { Name = "Tigger", Age = 6.4, Weight = 270, Height = .98 };
            EntityEntry entityEntry = _zooDbContext.Add(animal);
            Assert.Equal(1, _zooDbContext.SaveChanges(acceptAllChangesOnSuccess: true));
            Assert.False(string.IsNullOrWhiteSpace(animal.ConcurrencyField));

            string newConcurrencyToken = Guid.NewGuid().ToString();
            entityEntry.Property(nameof(Animal.ConcurrencyField)).OriginalValue = newConcurrencyToken;
            typeof(Animal)
                .GetTypeInfo()
                .GetProperty(nameof(Animal.ConcurrencyField))
                .SetValue(animal, newConcurrencyToken);
            Assert.Equal(0, _zooDbContext.SaveChanges(acceptAllChangesOnSuccess: true));
        }
    }
}