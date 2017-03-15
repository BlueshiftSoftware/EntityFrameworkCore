using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Blueshift.EntityFrameworkCore.Annotations
{
    public interface IModelAttribute
    {
        void Apply([NotNull] InternalModelBuilder modelBuilder);
    }
}