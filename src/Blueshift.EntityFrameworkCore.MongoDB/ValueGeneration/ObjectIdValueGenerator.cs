using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using MongoDB.Bson;

namespace Blueshift.EntityFrameworkCore.ValueGeneration
{
    public class ObjectIdValueGenerator : ValueGenerator<ObjectId>
    {
        public ObjectIdValueGenerator(bool generatesTemporaryValue = false)
        {
            GeneratesTemporaryValues = generatesTemporaryValue;
        }

        public override ObjectId Next(EntityEntry entry)
            => ObjectId.GenerateNewId();

        public override bool GeneratesTemporaryValues { get; }
    }
}