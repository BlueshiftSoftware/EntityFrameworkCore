using Blueshift.EntityFrameworkCore.MongoDB.Diagnostics.EventPayloads;
using Blueshift.EntityFrameworkCore.MongoDB.Properties;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;

namespace Blueshift.EntityFrameworkCore.MongoDB.Diagnostics
{
    /// <inheritdoc cref="CoreLoggerExtensions"/>
    public static class DocumentLoggerExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void DocumentQueryCommand(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnostics,
            string documentQuery)
        {
            var definition = DocumentStrings.LogDocumentQueryCommand;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    documentQuery);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new DocumentQueryEvent(
                        definition,
                        DocumentQueryCommand,
                        documentQuery));
            }
        }

        private static string DocumentQueryCommand(EventDefinitionBase definition, EventData payload)
        {
            var eventDefinition = (EventDefinition<string>) definition;
            var documentQueryEvent = (DocumentQueryEvent) payload;
            return eventDefinition.GenerateMessage(documentQueryEvent.DocumentQuery);
        }
    }
}
