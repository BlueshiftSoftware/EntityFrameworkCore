using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blueshift.EntityFrameworkCore.MongoDB.SampleDomain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

namespace Blueshift.EntityFrameworkCore.MongoDB.Tests
{
    public class MongoDbContextTests : MongoDbContextTestBase, IClassFixture<ZooEntityFixture>
    {
        private readonly ZooEntities _zooEntities;

        public MongoDbContextTests(ZooEntityFixture zooEntityFixture)
        {
            _zooEntities = zooEntityFixture.Entities;
        }

        [Fact]
        public async Task Can_query_from_mongodb()
        {
            await ExecuteUnitOfWorkAsync(async zooDbContext =>
            {
                Assert.Empty(await zooDbContext.Animals.ToListAsync());
                Assert.Empty(await zooDbContext.Employees.ToListAsync());
                Assert.Empty(await zooDbContext.Enclosures.ToListAsync());
            });
        }

        [Fact]
        public async Task Can_write_simple_record()
        {
            var employee = new Employee { FirstName = "Taiga", LastName = "Masuta", Age = 31.7M };

            await ExecuteUnitOfWorkAsync(async zooDbContext =>
            {
                zooDbContext.Add(employee);
                Assert.Equal(1, await zooDbContext.SaveChangesAsync(acceptAllChangesOnSuccess: true));
            });

            await ExecuteUnitOfWorkAsync(async zooDbContext =>
            {
                Assert.Equal(employee, await zooDbContext.Employees.SingleAsync(), new EmployeeEqualityComparer());
            });
        }

        [Fact]
        public async Task Can_write_complex_record()
        {
            await ExecuteUnitOfWorkAsync(async zooDbContext =>
            {
                zooDbContext.Add(_zooEntities.TaigaMasuta);
                Assert.Equal(1, await zooDbContext.SaveChangesAsync(acceptAllChangesOnSuccess: true));
            });
        }

        [Fact]
        public async Task Can_write_polymorphic_records()
        {
            await ExecuteUnitOfWorkAsync(async zooDbContext =>
            {
                zooDbContext.Animals.AddRange(_zooEntities.Animals);
                Assert.Equal(
                    _zooEntities.Entities.Count,
                    await zooDbContext.SaveChangesAsync(acceptAllChangesOnSuccess: true));
            });

            await ExecuteUnitOfWorkAsync(async zooDbContext =>
            {
                IList<Animal> queriedEntities = await zooDbContext.Animals
                    .OrderBy(animal => animal.Name)
                    .ThenBy(animal => animal.Height)
                    .ToListAsync();
                Assert.Equal(_zooEntities.Animals,
                    queriedEntities,
                    new AnimalEqualityComparer());
            });
        }

        [Fact]
        public async Task Can_update_existing_entity()
        {
            await ExecuteUnitOfWorkAsync(async zooDbContext =>
            {
                EntityEntry entityEntry = zooDbContext.Add(_zooEntities.Tigger);
                Assert.Equal(EntityState.Added, entityEntry.State);
                Assert.Equal(5, await zooDbContext.SaveChangesAsync(acceptAllChangesOnSuccess: true));
                Assert.Equal(EntityState.Unchanged, entityEntry.State);
                Assert.NotNull(_zooEntities.Tigger.ConcurrencyField);

                Assert.NotNull(entityEntry.OriginalValues[nameof(_zooEntities.Tigger.ConcurrencyField)]);

                _zooEntities.Tigger.Name = "Tigra";
                zooDbContext.ChangeTracker.DetectChanges();
                Assert.Equal(EntityState.Modified, entityEntry.State);
                Assert.Equal(1, await zooDbContext.SaveChangesAsync(acceptAllChangesOnSuccess: true));
            });

            await ExecuteUnitOfWorkAsync(async zooDbContext =>
            {
                Assert.Equal(
                    _zooEntities.Tigger,
                    await zooDbContext.Animals.OfType<Tiger>().FirstOrDefaultAsync(),
                    new AnimalEqualityComparer());
            });
        }

        [Fact]
        public async Task Concurrency_field_prevents_updates()
        {
            await ExecuteUnitOfWorkAsync(async zooDbContext =>
            {
                zooDbContext.Add(_zooEntities.Tigger);
                Assert.Equal(5, await zooDbContext.SaveChangesAsync(acceptAllChangesOnSuccess: true));
                Assert.False(string.IsNullOrWhiteSpace(_zooEntities.Tigger.ConcurrencyField));
            });

            await ExecuteUnitOfWorkAsync(async zooDbContext =>
            {
                EntityEntry entityEntry = zooDbContext.Update(_zooEntities.Tigger);
                string newConcurrencyToken = Guid.NewGuid().ToString();
                PropertyEntry propertyEntry = entityEntry.Property(nameof(Animal.ConcurrencyField));
                propertyEntry.OriginalValue = newConcurrencyToken;
                propertyEntry.Metadata.GetSetter().SetClrValue(_zooEntities.Tigger, newConcurrencyToken);

                Assert.Equal(0, await zooDbContext.SaveChangesAsync(acceptAllChangesOnSuccess: true));

                Assert.False(string.IsNullOrWhiteSpace(_zooEntities.Tigger.ConcurrencyField));
            });
        }

        [Fact]
        public async Task Can_query_complex_record()
        {
            await ExecuteUnitOfWorkAsync(async zooDbContext =>
            {
                zooDbContext.Add(_zooEntities.TaigaMasuta);
                Assert.Equal(1, await zooDbContext.SaveChangesAsync(acceptAllChangesOnSuccess: true));
            });

            await ExecuteUnitOfWorkAsync(async zooDbContext =>
            {
                Assert.Equal(_zooEntities.TaigaMasuta, await zooDbContext.Employees
                    .SingleAsync(searchedEmployee => searchedEmployee.Specialties
                        .Any(speciality => speciality.Task == ZooTask.Feeding)), new EmployeeEqualityComparer());
            });
        }

        [Fact]
        public async Task Can_query_polymorphic_sub_types()
        {
            await ExecuteUnitOfWorkAsync(async zooDbContext =>
            {
                zooDbContext.Animals.AddRange(_zooEntities.Animals);
                Assert.Equal(
                    _zooEntities.Entities.Count,
                    await zooDbContext.SaveChangesAsync(acceptAllChangesOnSuccess: true));
            });

            await ExecuteUnitOfWorkAsync(async zooDbContext =>
            {
                Assert.Equal(
                    _zooEntities.Animals.OfType<Tiger>().Single(),
                    await zooDbContext.Animals.OfType<Tiger>().SingleAsync(),
                    new AnimalEqualityComparer());
            });

            await ExecuteUnitOfWorkAsync(async zooDbContext =>
            {
                Assert.Equal(
                    _zooEntities.Animals.OfType<PolarBear>().Single(),
                    await zooDbContext.Animals.OfType<PolarBear>().SingleAsync(),
                    new AnimalEqualityComparer());
            });

            await ExecuteUnitOfWorkAsync(async zooDbContext =>
            {
                Assert.Equal(
                    _zooEntities.Animals.OfType<SeaOtter>().Single(),
                    await zooDbContext.Animals.OfType<SeaOtter>().SingleAsync(),
                    new AnimalEqualityComparer());
            });

            await ExecuteUnitOfWorkAsync(async zooDbContext =>
            {
                Assert.Equal(
                    _zooEntities.Animals.OfType<EurasianOtter>().Single(),
                    await zooDbContext.Animals.OfType<EurasianOtter>().SingleAsync(),
                    new AnimalEqualityComparer());
            });


            await ExecuteUnitOfWorkAsync(async zooDbContext =>
            {
                IList<Otter> originalOtters = _zooEntities.Animals
                    .OfType<Otter>()
                    .OrderBy(otter => otter.Name)
                    .ToList();
                IList<Otter> queriedOtters = await zooDbContext.Animals
                    .OfType<Otter>()
                    .OrderBy(otter => otter.Name)
                    .ToListAsync();
                Assert.Equal(originalOtters, queriedOtters, new AnimalEqualityComparer());
            });
        }

        [Fact]
        public async Task Can_list_async()
        {
            await ExecuteUnitOfWorkAsync(async zooDbContext =>
            {
                zooDbContext.Animals.AddRange(_zooEntities.Animals);
                Assert.Equal(
                    _zooEntities.Entities.Count,
                    await zooDbContext.SaveChangesAsync(acceptAllChangesOnSuccess: true));
            });

            await ExecuteUnitOfWorkAsync(async zooDbContext =>
            {
                Assert.Equal(_zooEntities.Animals,
                    await zooDbContext.Animals
                        .OrderBy(animal => animal.Name)
                        .ThenBy(animal => animal.Height)
                        .ToListAsync(),
                    new AnimalEqualityComparer());
            });
        }

        [Fact]
        public async Task Can_query_first_or_default_async()
        {
            await ExecuteUnitOfWorkAsync(async zooDbContext =>
            {
                zooDbContext.Animals.AddRange(_zooEntities.Animals);
                Assert.Equal(
                    _zooEntities.Entities.Count,
                    await zooDbContext.SaveChangesAsync(acceptAllChangesOnSuccess: true));
            });

            await ExecuteUnitOfWorkAsync(async zooDbContext =>
            {
                Assert.Equal(
                    _zooEntities.Animals.OfType<Tiger>().Single(),
                    await zooDbContext.Animals.OfType<Tiger>().FirstOrDefaultAsync(),
                    new AnimalEqualityComparer());
            });
        }

        [Fact]
        public async Task Can_include_direct_collection()
        {
            await ExecuteUnitOfWorkAsync(async zooDbContext =>
            {
                zooDbContext.Enclosures.AddRange(_zooEntities.Enclosures);
                Assert.Equal(
                    _zooEntities.Entities.Count,
                    await zooDbContext.SaveChangesAsync(acceptAllChangesOnSuccess: true));
            });

            await ExecuteUnitOfWorkAsync(async zooDbContext =>
            {
                IEnumerable<Enclosure> queriedEnclosures = await zooDbContext.Enclosures
                    .Include(enclosure => enclosure.Animals)
                    .OrderBy(enclosure => enclosure.AnimalEnclosureType)
                    .ThenBy(enclosure => enclosure.Name)
                    .ToListAsync();
                Assert.Equal(_zooEntities.Enclosures,
                    queriedEnclosures,
                    new EnclosureEqualityComparer()
                        .WithAnimalEqualityComparer(animalEqualityComparer => animalEqualityComparer
                            .WithEnclosureEqualityComparer()));
            });
        }

        [Fact]
        public async Task Can_include_direct_reference()
        {
            await ExecuteUnitOfWorkAsync(async zooDbContext =>
            {
                zooDbContext.Animals.AddRange(_zooEntities.Animals);
                Assert.Equal(
                    _zooEntities.Entities.Count,
                    await zooDbContext.SaveChangesAsync(acceptAllChangesOnSuccess: true));
            });

            await ExecuteUnitOfWorkAsync(async zooDbContext =>
            {
                IEnumerable<Animal> queriedAnimals = await zooDbContext.Animals
                    .Include(animal => animal.Enclosure)
                    .OrderBy(animal => animal.Name)
                    .ThenBy(animal => animal.Age)
                    .ToListAsync();
                Assert.Equal(_zooEntities.Animals,
                    queriedAnimals,
                    new AnimalEqualityComparer()
                        .WithEnclosureEqualityComparer(enclosureEqualityComparer =>
                            enclosureEqualityComparer.WithAnimalEqualityComparer()));
            });
        }

        [Fact(Skip = "IncludeCompiler does not currently support DI or being independently overriden.")]
        public async Task Can_include_owned_collection()
        {
            // IncludeCompiler uses the entity metadata to generate the underlying join clauses,
            // however it currently does not properly support being injected through DI, being created
            // by a factory, or being independently overriden without also having to override several
            // other query-generation-related classes. This makes it virtually impossible to generate
            // the correct MongoDb-side query syntax for supporting Join and GroupJoin statements
            // against owned collections where the ownership requires a level of indirection to get to
            // the foreign key.

            await ExecuteUnitOfWorkAsync(async zooDbContext =>
            {
                zooDbContext.Enclosures.AddRange(_zooEntities.Enclosures);
                Assert.Equal(
                    _zooEntities.Entities.Count,
                    await zooDbContext.SaveChangesAsync(acceptAllChangesOnSuccess: true));
            });

            await ExecuteUnitOfWorkAsync(async zooDbContext =>
            {
                IEnumerable<Enclosure> queriedEnclosures = await zooDbContext.Enclosures
                    .Include(enclosure => enclosure.Animals)
                    .Include(enclosure => enclosure.WeeklySchedule.Assignments)
                    .ThenInclude(zooAssignment => zooAssignment.Assignee)
                    .OrderBy(enclosure => enclosure.AnimalEnclosureType)
                    .ThenBy(enclosure => enclosure.Name)
                    .ToListAsync();
                Assert.Equal(_zooEntities.Enclosures,
                    queriedEnclosures,
                    new EnclosureEqualityComparer()
                        .WithAnimalEqualityComparer(animalEqualityComparer => animalEqualityComparer
                            .WithEnclosureEqualityComparer())
                        .ConfigureWeeklyScheduleEqualityComparer(
                            scheduleEqualityComparer => scheduleEqualityComparer.ConfigureZooAssignmentEqualityComparer(
                                zooAssignmentEqualityComparer => zooAssignmentEqualityComparer.WithEmployeeEqualityComparer())));
            });
        }

        [Fact]
        public async Task Can_include_owned_reference()
        {
            await ExecuteUnitOfWorkAsync(async zooDbContext =>
            {
                zooDbContext.Enclosures.AddRange(_zooEntities.Enclosures);
                Assert.Equal(
                    _zooEntities.Entities.Count,
                    await zooDbContext.SaveChangesAsync(acceptAllChangesOnSuccess: true));
            });

            await ExecuteUnitOfWorkAsync(async zooDbContext =>
            {
                IEnumerable<Enclosure> queriedEnclosures = await zooDbContext.Enclosures
                    .Include(enclosure => enclosure.Animals)
                    .Include(enclosure => enclosure.WeeklySchedule.Approver)
                    .OrderBy(enclosure => enclosure.AnimalEnclosureType)
                    .ThenBy(enclosure => enclosure.Name)
                    .ToListAsync();
                Assert.Equal(_zooEntities.Enclosures,
                    queriedEnclosures,
                    new EnclosureEqualityComparer()
                        .WithAnimalEqualityComparer(animalEqualityComparer => animalEqualityComparer
                            .WithEnclosureEqualityComparer())
                        .ConfigureWeeklyScheduleEqualityComparer(
                            scheduleEqualityComparer => scheduleEqualityComparer
                                .WithApproverEqualityComparer()));
            });
        }

        [Fact]
        public async Task Can_execute_group_join_without_includes()
        {
            await ExecuteUnitOfWorkAsync(async zooDbContext =>
            {
                zooDbContext.Enclosures.AddRange(_zooEntities.Enclosures);
                Assert.Equal(
                    _zooEntities.Entities.Count,
                    await zooDbContext.SaveChangesAsync(acceptAllChangesOnSuccess: true));
            });

            await ExecuteUnitOfWorkAsync(async zooDbContext =>
            {
                IEnumerable<Enclosure> queriedEnclosures = await zooDbContext.Enclosures
                    .GroupJoin(
                        zooDbContext.Employees
                            .Join(
                                zooDbContext.Enclosures.SelectMany(
                                    enclosure => enclosure.WeeklySchedule.Assignments,
                                    (enclosure, assignment) => new
                                    {
                                        EnclosureId = enclosure.Id,
                                        Assignment = assignment
                                    }),
                                employee => employee.Id,
                                enclosureAssignment => enclosureAssignment.Assignment.Assignee.Id,
                                (employee, enclosureAssignment) => new
                                {
                                    enclosureAssignment.EnclosureId,
                                    Assignment = AssignAssignee(enclosureAssignment.Assignment, employee)
                                }),
                        enclosure => enclosure.Id,
                        enclosureAssignment => enclosureAssignment.EnclosureId,
                        (enclosure, enclosureAssignments) => AssignAssignments(
                            enclosure,
                            enclosureAssignments.Select(enclosureAssignment => enclosureAssignment.Assignment)))
                    .ToListAsync();
                Assert.Equal(_zooEntities.Enclosures,
                    queriedEnclosures,
                    new EnclosureEqualityComparer()
                        .ConfigureWeeklyScheduleEqualityComparer(
                            scheduleEqualityComparer => scheduleEqualityComparer
                                .ConfigureZooAssignmentEqualityComparer(
                                    zooAssignmentEqualityComparer => zooAssignmentEqualityComparer
                                        .WithEmployeeEqualityComparer())));
            });
        }

        private static Enclosure AssignAssignments(Enclosure enclosure, IEnumerable<ZooAssignment> zooAssignments)
        {
            foreach (var pair in enclosure.WeeklySchedule.Assignments
                .Join(
                    zooAssignments,
                    includedAssignment => includedAssignment.Assignee.Id,
                    denormalizedAssignment => denormalizedAssignment.Assignee.Id,
                    (denormalizedAssignment, includedAssignment) => new
                    {
                        Assignment = denormalizedAssignment,
                        includedAssignment.Assignee
                    }))
            {
                pair.Assignment.Assignee = pair.Assignee;
            };
            return enclosure;
        }

        private static ZooAssignment AssignAssignee(ZooAssignment assignment, Employee assignee)
        {
            assignment.Assignee = assignee;
            return assignment;
        }
    }
}