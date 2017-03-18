using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;

namespace Blueshift.EntityFrameworkCore.MongoDB.Adapter
{
    public class AbstractClassConvention : ConventionBase, IClassMapConvention
    {
        public AbstractClassConvention()
            : base(Regex.Replace(nameof(AbstractClassConvention), pattern: "Convention$", replacement: ""))
        {
        }

        public virtual void Apply([NotNull] BsonClassMap classMap)
        {
            Check.NotNull(classMap, nameof(classMap));
            if (classMap.ClassType.GetTypeInfo().IsAbstract)
            {
                classMap.SetDiscriminatorIsRequired(discriminatorIsRequired: true);
            }
        }
    }
}