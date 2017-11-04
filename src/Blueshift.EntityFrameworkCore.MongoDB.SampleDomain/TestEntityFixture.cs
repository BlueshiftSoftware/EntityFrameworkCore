using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace Blueshift.EntityFrameworkCore.MongoDB.SampleDomain
{
    public class TestEntityFixture
    {
        private static T SetId<T>(T entity, ObjectId? objectId = null)
        {
            BsonClassMap bsonClassMap = BsonClassMap.LookupClassMap(typeof(T));
            bsonClassMap.IdMemberMap.Setter(entity, objectId ?? ObjectId.GenerateNewId());
            return entity;
        }

        public static Employee TaigaMasuta { get; } = SetId(new Employee
        {
            FirstName = "Taiga",
            LastName = "Masuta",
            Age = 31.7M,
            Specialties =
            {
                new Specialty {AnimalType = nameof(Tiger), Task = ZooTask.Feeding},
                new Specialty {AnimalType = nameof(Tiger), Task = ZooTask.Exercise},
                new Specialty {AnimalType = nameof(Tiger), Task = ZooTask.TourGuide},
                new Specialty {AnimalType = nameof(Tiger), Task = ZooTask.TourGuide},
            }
        });

        public static readonly Enclosure TigerEnclosure =
            SetId(new Enclosure {AnimalEnclosureType = nameof(Tiger), Name = "Tiger Pen"});

        public static readonly Enclosure OtterEnclosure =
            SetId(new Enclosure {AnimalEnclosureType = nameof(Otter), Name = "Otter Tank"});

        public static readonly Enclosure PolarBearEnclosure =
            SetId(new Enclosure {AnimalEnclosureType = nameof(PolarBear), Name = "Igloo"});

        public static readonly ICollection<Enclosure> Enclosures = new []
            {
                TigerEnclosure,
                PolarBearEnclosure,
                OtterEnclosure
            }
            .OrderBy(enclosure => enclosure.AnimalEnclosureType)
            .ThenBy(enclosure => enclosure.Name)
            .ToList();

        public static readonly Tiger Tigger =
            SetId(new Tiger {Name = "Tigger", Age = 6.4M, Weight = 270, Height = .98M, Enclosure = TigerEnclosure});

        public static readonly PolarBear Ursus =
            SetId(new PolarBear
            {
                Name = "Ursus",
                Age = 4.9M,
                Weight = 612,
                Height = 2.7M,
                Enclosure = PolarBearEnclosure
            });

        public static readonly SeaOtter Hydron =
            SetId(new SeaOtter {Name = "Hydron", Age = 1.8M, Weight = 19, Height = .3M, Enclosure = OtterEnclosure});

        public static readonly EurasianOtter Yuri =
            SetId(new EurasianOtter {Name = "Yuri", Age = 1.8M, Weight = 19, Height = .3M, Enclosure = OtterEnclosure});

        public static readonly ICollection<Animal> Animals = new Animal[]
            {
                Tigger,
                Ursus,
                Hydron,
                Yuri
            }
            .OrderBy(animal => animal.GetType().Name)
            .ThenBy(animal => animal.Name)
            .ToList();
    }
}
