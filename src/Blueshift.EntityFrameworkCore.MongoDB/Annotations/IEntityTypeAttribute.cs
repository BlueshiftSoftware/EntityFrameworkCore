using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Blueshift.EntityFrameworkCore.Annotations
{
    public interface IEntityTypeAttribute
    {
        void Apply([NotNull] InternalEntityTypeBuilder entityTypeBuilder);
    }
}