using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.MongoDB.Diagnostics.EventPayloads
{
    /// <inheritdoc />
    /// <summary>
    ///   A <see cref="DiagnosticSource" /> event payload class for query command execution.
    /// </summary>
    public class DocumentQueryEvent : EventData
    {
        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentQueryEvent"/> class.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="documentQuery">A textual representation of the document query.</param>
        public DocumentQueryEvent(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] string documentQuery)
            : base(eventDefinition, messageGenerator)
        {
            DocumentQuery = Check.NotEmpty(documentQuery, nameof(documentQuery));
        }

        /// <summary>
        ///    A textual representation of the document query.
        /// </summary>
        public string DocumentQuery { get; }
    }
}
