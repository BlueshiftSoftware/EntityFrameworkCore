using System;

namespace Blueshift.EntityFrameworkCore.MongoDB.Tests
{
    public class MongoDbConstants
    {
        public const int MongodPort = 27017;
        public static readonly string MongodExe = Environment.ExpandEnvironmentVariables(@"%ProgramW6432%\MongoDB\Server\3.2\bin\mongod.exe");
        public static readonly string MongoExe = Environment.ExpandEnvironmentVariables(@"%ProgramW6432%\MongoDB\Server\3.2\bin\mongo.exe");
        public static readonly string DataFolder = $@".data\Port-{MongodPort}";
        public static readonly string MongoUrl = $"mongodb://localhost:{MongodPort}";
    }
}