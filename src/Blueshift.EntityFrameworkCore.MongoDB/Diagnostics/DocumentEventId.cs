using System.Diagnostics;
using Blueshift.EntityFrameworkCore.MongoDB.Diagnostics.EventPayloads;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Blueshift.EntityFrameworkCore.MongoDB.Diagnostics
{
    /// <inheritdoc cref="CoreEventId"/>
    public static class DocumentEventId
    {
        /// <summary>
        /// Base id for Document DB events.
        /// </summary>
        public const int DocumentBaseId = CoreEventId.CoreBaseId * 100;
        private enum DocumentLogId
        {
            QueryCommand = DocumentBaseId + 1000
        }

        private static EventId MakeCommandId(DocumentLogId id)
            => new EventId((int)id, $"{DbLoggerCategory.Database.Command.Name}.{id}");

        /// <summary>
        ///     <para>
        ///         Executing a document query.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Database.Command" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="DocumentQueryEvent" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId DocumentQueryCommand = MakeCommandId(DocumentLogId.QueryCommand);
    }
}
