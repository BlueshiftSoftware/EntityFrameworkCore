using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Blueshift.EntityFrameworkCore.MongoDB.Diagnostics
{
    /// <inheritdoc cref="CoreEventId"/>
    public static class MongoDbEventId
    {
        /// <summary>
        /// Base id for MongoDB provider events.
        /// </summary>
        public const int MongoDbBaseId = DocumentEventId.DocumentBaseId + 10000;
    }
}