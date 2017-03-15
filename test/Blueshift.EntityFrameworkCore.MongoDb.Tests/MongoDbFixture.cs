using System;
using System.Diagnostics;
using System.IO;
using Blueshift.EntityFrameworkCore.Infrastructure;
using Blueshift.EntityFrameworkCore.MongoDB.Tests.TestDomain;
using Microsoft.Extensions.DependencyInjection;

namespace Blueshift.EntityFrameworkCore.MongoDB.Tests
{
    public class MongoDbFixture : IDisposable
    {
        private Process _mongodProcess;

        public MongoDbFixture()
        {
            Directory.CreateDirectory(MongoDbConstants.DataFolder);
            _mongodProcess = Process.Start(
                new ProcessStartInfo
                {
                    FileName = Environment.ExpandEnvironmentVariables(name: MongoDbConstants.MongodExe),
                    Arguments = $@"-vvvvv --port {MongoDbConstants.MongodPort} --logpath "".data\{MongoDbConstants.MongodPort}.log"" --dbpath ""{MongoDbConstants.DataFolder}""",
                    CreateNoWindow = true,
                    UseShellExecute = false
                });
        }

        public TestMongoDbContext TestMongoDbContext => new ServiceCollection()
                .AddDbContext<TestMongoDbContext>(options => options.UseMongoDb(connectionString: MongoDbConstants.MongoUrl))
                .BuildServiceProvider()
                .GetService<TestMongoDbContext>();

        public void Dispose()
        {
            if (_mongodProcess != null && !_mongodProcess.HasExited)
            {
                Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = MongoDbConstants.MongoExe,
                        Arguments = $@"""{MongoDbConstants.MongoUrl}/admin"" --eval ""db.shutdownServer();""",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    });
                _mongodProcess.WaitForExit(milliseconds: 5000);
                _mongodProcess.Dispose();
                _mongodProcess = null;
            }
        }
    }
}