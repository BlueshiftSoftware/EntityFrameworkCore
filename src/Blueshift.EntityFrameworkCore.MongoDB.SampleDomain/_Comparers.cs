using System;
using System.Collections.Generic;
using System.Linq;

namespace Blueshift.EntityFrameworkCore.MongoDB.SampleDomain
{
    public class AnimalComparer : IComparer<Animal>
    {
        public int Compare(Animal x, Animal y)
            => x?.GetType() == y?.GetType()
                ? Comparer<string>.Default.Compare(x?.Name, y?.Name)
                : Comparer<string>.Default.Compare(x?.GetType().Name, y?.GetType().Name);
    }

    public abstract class BaseEqualityComparer<T> : EqualityComparer<T>
    {
        public override int GetHashCode(T obj)
            => obj?.GetHashCode() ?? throw new ArgumentNullException(nameof(obj));
    }

    public class AnimalEqualityComparer : BaseEqualityComparer<Animal>
    {
        public override bool Equals(Animal animal1, Animal animal2)
            => (animal1 == null && animal2 == null)
               || (animal1 != null && animal2 != null
                   && Equals(animal1.Id, animal2.Id)
                   && animal1.GetType() == animal2.GetType()
                   && string.Equals(animal1.Name, animal2.Name, StringComparison.Ordinal)
                   && animal1.Age == animal2.Age
                   && animal1.Height == animal2.Height
                   && animal1.Weight == animal2.Weight
                   && Equals(animal1.Enclosure?.Id, animal2.Enclosure?.Id));
    }

    public class AnimalWithEnclosureEqualityComparer : AnimalEqualityComparer
    {
        private readonly EnclosureEqualityComparer _enclosureEqualityComparer = new EnclosureEqualityComparer();

        public override bool Equals(Animal animal1, Animal animal2)
            => base.Equals(animal1, animal2)
                && _enclosureEqualityComparer.Equals(animal1.Enclosure, animal2.Enclosure);
    }

    public class EnclosureEqualityComparer : BaseEqualityComparer<Enclosure>
    {
        public override bool Equals(Enclosure enclosure1, Enclosure enclosure2)
            => (enclosure1 == null && enclosure2 == null)
               || (enclosure1 != null && enclosure2 != null
                   && Equals(enclosure1.Id, enclosure2.Id)
                   && string.Equals(enclosure1.Name, enclosure2.Name, StringComparison.Ordinal)
                   && string.Equals(enclosure1.AnimalEnclosureType, enclosure2.AnimalEnclosureType, StringComparison.Ordinal));
    }

    public class EnclosureWithAnimalsEqualityComparer : EnclosureEqualityComparer
    {
        private readonly AnimalEqualityComparer _animalEqualityComparer = new AnimalEqualityComparer();

        public override bool Equals(Enclosure enclosure1, Enclosure enclosure2)
            => base.Equals(enclosure1, enclosure2)
               && enclosure1.Animals.All(animal => enclosure2.Animals.Contains(animal, _animalEqualityComparer))
               && enclosure2.Animals.All(animal => enclosure1.Animals.Contains(animal, _animalEqualityComparer));

        public override int GetHashCode(Enclosure obj)
            => obj?.GetHashCode() ?? throw new ArgumentNullException(nameof(obj));
    }

    public class EmployeeEqualityComparer : BaseEqualityComparer<Employee>
    {
        public override bool Equals(Employee employee1, Employee employee2)
            => (employee1 == null && employee2 == null)
               || (employee1 != null && employee2 != null
                   && Equals(employee1.Id, employee2.Id)
                   && string.Equals(employee1.FirstName, employee2.FirstName, StringComparison.Ordinal)
                   && string.Equals(employee1.LastName, employee2.LastName, StringComparison.Ordinal)
                   && employee1.Age == employee2.Age
                   && employee1.Specialties.Count() == employee2.Specialties.Count()
                   && employee1.Specialties.All(item => employee2.Specialties.Contains(item, new SpecialtyEqualityComparer()))
                   && employee2.Specialties.All(item => employee1.Specialties.Contains(item, new SpecialtyEqualityComparer())));
    }

    public class SpecialtyEqualityComparer : BaseEqualityComparer<Specialty>
    {
        public override bool Equals(Specialty specialty1, Specialty specialty2)
            => (specialty1 == null && specialty2 == null)
               || (specialty1 != null && specialty2 != null
                   && string.Equals(specialty1.AnimalType, specialty2.AnimalType, StringComparison.Ordinal)
                   && specialty1.Task == specialty2.Task);
    }
}
