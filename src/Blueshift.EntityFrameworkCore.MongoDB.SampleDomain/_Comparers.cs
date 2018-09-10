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

        public sealed override bool Equals(T item1, T item2)
            => (item1 == null && item2 == null)
               || (item1 != null && item2 != null
                   && MemberwiseEquals(item1, item2));

        protected abstract bool MemberwiseEquals(T item1, T item2);
    }

    public static class EqualityComparerExtensions
    {
        public static bool CompareCollections<T>(
            this EqualityComparer<T> equalityComparer,
            ICollection<T> collection1,
            ICollection<T> collection2)
            => collection1.Count == collection2.Count
               && collection1.All(item1 => collection2.Contains(item1, equalityComparer))
               && collection2.All(item2 => collection1.Contains(item2, equalityComparer));
    }

    public class EmployeeEqualityComparer : BaseEqualityComparer<Employee>
    {
        private readonly SpecialtyEqualityComparer _specialtyEqualityComparer = new SpecialtyEqualityComparer();

        protected override bool MemberwiseEquals(Employee employee1, Employee employee2)
            => Equals(employee1.Id, employee2.Id)
               && string.Equals(employee1.FirstName, employee2.FirstName, StringComparison.Ordinal)
               && string.Equals(employee1.LastName, employee2.LastName, StringComparison.Ordinal)
               && employee1.Age == employee2.Age
               && (_specialtyEqualityComparer?.CompareCollections(employee1.Specialties, employee2.Specialties) ?? true);
    }

    public class SpecialtyEqualityComparer : BaseEqualityComparer<Specialty>
    {
        protected override bool MemberwiseEquals(Specialty specialty1, Specialty specialty2)
            => string.Equals(specialty1.AnimalType, specialty2.AnimalType, StringComparison.Ordinal)
               && specialty1.Task == specialty2.Task;
    }

    public class AnimalEqualityComparer : BaseEqualityComparer<Animal>
    {
        private EnclosureEqualityComparer _enclosureEqualityComparer;

        protected override bool MemberwiseEquals(Animal animal1, Animal animal2)
            => Equals(animal1.Id, animal2.Id)
               && animal1.GetType() == animal2.GetType()
               && string.Equals(animal1.Name, animal2.Name, StringComparison.Ordinal)
               && animal1.Age == animal2.Age
               && animal1.Height == animal2.Height
               && animal1.Weight == animal2.Weight
               && Equals(animal1.Enclosure?.Id, animal2.Enclosure?.Id)
               && (_enclosureEqualityComparer?.Equals(animal1.Enclosure, animal2.Enclosure) ?? true);

        public AnimalEqualityComparer WithEnclosureEqualityComparer(Action<EnclosureEqualityComparer> configurator = null)
        {
            _enclosureEqualityComparer = new EnclosureEqualityComparer();
            configurator?.Invoke(_enclosureEqualityComparer);
            return this;
        }
    }

    public class EnclosureEqualityComparer : BaseEqualityComparer<Enclosure>
    {
        private AnimalEqualityComparer _animalEqualityComparer = null;
        private readonly ScheduleEqualityComparer _scheduleEqualityComparer
            = new ScheduleEqualityComparer();

        protected override bool MemberwiseEquals(Enclosure enclosure1, Enclosure enclosure2)
            => Equals(enclosure1.Id, enclosure2.Id)
               && string.Equals(enclosure1.Name, enclosure2.Name, StringComparison.Ordinal)
               && string.Equals(enclosure1.AnimalEnclosureType, enclosure2.AnimalEnclosureType, StringComparison.Ordinal)
               && (_animalEqualityComparer?.CompareCollections(enclosure1.Animals, enclosure2.Animals) ?? true)
               && (_scheduleEqualityComparer?.Equals(enclosure1.WeeklySchedule, enclosure2.WeeklySchedule) ?? true);

        public EnclosureEqualityComparer WithAnimalEqualityComparer(Action<AnimalEqualityComparer> configurator = null)
        {
            _animalEqualityComparer = new AnimalEqualityComparer();
            configurator?.Invoke(_animalEqualityComparer);
            return this;
        }

        public EnclosureEqualityComparer ConfigureWeeklyScheduleEqualityComparer(
            Action<ScheduleEqualityComparer> configurator)
        {
            configurator.Invoke(_scheduleEqualityComparer);
            return this;
        }
    }

    public class ScheduleEqualityComparer : BaseEqualityComparer<Schedule>
    {
        private readonly ZooAssignmentEqualityComparer _zooAssignmentEqualityComparer
            = new ZooAssignmentEqualityComparer();
        private EmployeeEqualityComparer _approverEqualityComparer;

        protected override bool MemberwiseEquals(Schedule schedule1, Schedule schedule2)
            => (_approverEqualityComparer?.Equals(schedule1.Approver, schedule2.Approver) ?? true)
                && _zooAssignmentEqualityComparer.CompareCollections(schedule1.Assignments, schedule2.Assignments);

        public ScheduleEqualityComparer WithApproverEqualityComparer(
            Action<EmployeeEqualityComparer> configurator = null)
        {
            _approverEqualityComparer = new EmployeeEqualityComparer();
            configurator?.Invoke(_approverEqualityComparer);
            return this;
        }

        public ScheduleEqualityComparer ConfigureZooAssignmentEqualityComparer(
            Action<ZooAssignmentEqualityComparer> configurator)
        {
            configurator.Invoke(_zooAssignmentEqualityComparer);
            return this;
        }
    }

    public class ZooAssignmentEqualityComparer : BaseEqualityComparer<ZooAssignment>
    {
        private EmployeeEqualityComparer _employeeEqualityComparer;

        protected override bool MemberwiseEquals(ZooAssignment zooAssignment1, ZooAssignment zooAssignment2)
            => Equals(zooAssignment1.Offset, zooAssignment2.Offset)
               && Equals(zooAssignment1.Task, zooAssignment2.Task)
               && (_employeeEqualityComparer?.Equals(zooAssignment1.Assignee, zooAssignment2.Assignee) ?? true);

        public ZooAssignmentEqualityComparer WithEmployeeEqualityComparer()
        {
            _employeeEqualityComparer = new EmployeeEqualityComparer();
            return this;
        }
    }
}
