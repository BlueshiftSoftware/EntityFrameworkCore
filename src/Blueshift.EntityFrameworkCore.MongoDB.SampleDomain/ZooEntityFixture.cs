using System;
using System.Collections.Generic;
using System.Linq;
using Blueshift.EntityFrameworkCore.MongoDB.Adapter;

namespace Blueshift.EntityFrameworkCore.MongoDB.SampleDomain
{
    internal static class ZooExtensions
    {
        public static TAnimal WithEnclosure<TAnimal>(this TAnimal animal, Enclosure enclosure)
            where TAnimal : Animal
        {
            animal.Enclosure = enclosure;
            enclosure.Animals.Add(animal);
            return animal;
        }

        public static Enclosure WithSchedule(this Enclosure enclosure, Action<Schedule> configurator = null)
        {
            enclosure.WeeklySchedule = new Schedule();
            configurator?.Invoke(enclosure.WeeklySchedule);
            return enclosure;
        }

        public static Schedule WithApprover(this Schedule schedule, Employee employee)
        {
            schedule.Approver = employee;
            return schedule;
        }

        public static Schedule WithAssignment(this Schedule schedule, ZooAssignment zooAssignment)
        {
            schedule.Assignments.Add(zooAssignment);
            return schedule;
        }

        public static Schedule WithAssignment(
            this Schedule schedule,
            TimeSpan offset,
            Employee assignee,
            ZooTask assignedTask)
            => schedule.WithAssignment(new ZooAssignment
            {
                Offset = offset,
                Assignee = assignee,
                Task = assignedTask
            });
    }

    public class ZooEntityFixture
    {
        static ZooEntityFixture()
        {
            EntityFrameworkConventionPack.Register(type => true);
        }

        public ZooEntities Entities => new ZooEntities();
    }

    public class ZooEntities
    {
        public ZooEntities()
        {
            TaigaMasuta = new Employee
            {
                FirstName = "Taiga",
                LastName = "Masuta",
                Age = 31.7M,
                Specialties =
                {
                    new Specialty {AnimalType = nameof(Tiger), Task = ZooTask.Feeding},
                    new Specialty {AnimalType = nameof(Tiger), Task = ZooTask.Exercise},
                    new Specialty {AnimalType = nameof(Tiger), Task = ZooTask.Training}
                }
            };

            OttoVonEssenmacher = new Employee
            {
                FirstName = "Otto",
                LastName = "von Essenmacher",
                Age = 22.1M,
                Specialties =
                {
                    new Specialty {AnimalType = nameof(Otter), Task = ZooTask.Feeding},
                    new Specialty {AnimalType = nameof(Otter), Task = ZooTask.Exercise}
                }
            };

            BearOCreary = new Employee
            {
                FirstName = "Bear",
                LastName = "O'Creary",
                Age = 41.4M,
                Specialties =
                {
                    new Specialty {AnimalType = nameof(PolarBear), Task = ZooTask.Feeding},
                    new Specialty {AnimalType = nameof(PolarBear), Task = ZooTask.Training}
                }
            };

            TurGuidry = new Employee
            {
                FirstName = "Tur",
                LastName = "Guidry",
                Age = 36.7M,
                Specialties =
                {
                    new Specialty {AnimalType = nameof(Tiger), Task = ZooTask.TourGuide},
                    new Specialty {AnimalType = nameof(Otter), Task = ZooTask.TourGuide},
                    new Specialty {AnimalType = nameof(PolarBear), Task = ZooTask.TourGuide}
                }
            };

            ManAgier = new Employee
            {
                FirstName = "Man",
                LastName = "A'Gier",
                Age = 58.4M,
                Specialties =
                {
                    // those who can't zoo, manage!
                }
            };

            TigerEnclosure = new Enclosure
                {
                    AnimalEnclosureType = nameof(Tiger),
                    Name = "Tiger Pen"
                }
                .WithSchedule(schedule =>
                    schedule
                        .WithApprover(ManAgier)
                        .WithAssignment(TimeSpan.FromHours(1), TaigaMasuta, ZooTask.Feeding)
                        .WithAssignment(TimeSpan.FromHours(3), TaigaMasuta, ZooTask.Training)
                        .WithAssignment(TimeSpan.FromHours(4), TurGuidry, ZooTask.TourGuide)
                        .WithAssignment(TimeSpan.FromHours(7), TaigaMasuta, ZooTask.Feeding));

            OtterEnclosure = new Enclosure
                {
                    AnimalEnclosureType = nameof(Otter),
                    Name = "Otter Tank"
                }
                .WithSchedule(schedule =>
                    schedule
                        .WithApprover(ManAgier)
                        .WithAssignment(TimeSpan.FromHours(1), OttoVonEssenmacher, ZooTask.Feeding)
                        .WithAssignment(TimeSpan.FromHours(3), OttoVonEssenmacher, ZooTask.Exercise)
                        .WithAssignment(TimeSpan.FromHours(6), TurGuidry, ZooTask.TourGuide)
                        .WithAssignment(TimeSpan.FromHours(7), OttoVonEssenmacher, ZooTask.Feeding));

            PolarBearEnclosure = new Enclosure
                {
                    AnimalEnclosureType = nameof(PolarBear),
                    Name = "Igloo"
                }
                .WithSchedule(schedule =>
                    schedule
                        .WithApprover(ManAgier)
                        .WithAssignment(TimeSpan.FromHours(1), BearOCreary, ZooTask.Feeding)
                        .WithAssignment(TimeSpan.FromHours(3), BearOCreary, ZooTask.Training)
                        .WithAssignment(TimeSpan.FromHours(5), TurGuidry, ZooTask.TourGuide)
                        .WithAssignment(TimeSpan.FromHours(7), BearOCreary, ZooTask.Feeding));

            Tigger = new Tiger
            {
                Name = "Tigger",
                Age = 6.4M,
                Weight = 270,
                Height = .98M
            }
            .WithEnclosure(TigerEnclosure);

            Ursus = new PolarBear
            {
                Name = "Ursus",
                Age = 4.9M,
                Weight = 612,
                Height = 2.7M
            }
            .WithEnclosure(PolarBearEnclosure);

            Hydron = new SeaOtter
            {
                Name = "Hydron",
                Age = 1.8M,
                Weight = 19,
                Height = .3M
            }
            .WithEnclosure(OtterEnclosure);

            Yuri = new EurasianOtter
            {
                Name = "Yuri",
                Age = 1.8M,
                Weight = 19,
                Height = .3M
            }
            .WithEnclosure(OtterEnclosure);

            Animals = new Animal[]
            {
                Tigger,
                Ursus,
                Hydron,
                Yuri
            }
            .OrderBy(animal => animal.Name)
            .ThenBy(animal => animal.Height)
            .ToList();

            Enclosures = new[]
            {
                TigerEnclosure,
                PolarBearEnclosure,
                OtterEnclosure
            }
            .OrderBy(enclosure => enclosure.AnimalEnclosureType)
            .ThenBy(enclosure => enclosure.Name)
            .ToList();

            Employees = new[]
            {
                TaigaMasuta,
                BearOCreary,
                OttoVonEssenmacher,
                TurGuidry,
                ManAgier
            }
            .OrderBy(employee => employee.FullName)
            .ToList();

            Entities = Animals
                .Cast<ZooEntity>()
                .Concat(Enclosures)
                .Concat(Employees)
                .ToList();
        }

        public Employee TaigaMasuta { get; }

        public Employee OttoVonEssenmacher { get; }

        public Employee BearOCreary { get; }

        public Employee TurGuidry { get; }

        public Employee ManAgier { get; }

        public Enclosure TigerEnclosure { get; }

        public Enclosure OtterEnclosure { get; }

        public Enclosure PolarBearEnclosure { get; }

        public Tiger Tigger { get; }

        public PolarBear Ursus { get; }

        public SeaOtter Hydron { get; }

        public EurasianOtter Yuri { get; }

        public ICollection<Animal> Animals { get; }

        public ICollection<Enclosure> Enclosures { get; }

        public ICollection<Employee> Employees { get; }

        public ICollection<ZooEntity> Entities { get; }
    }
}
