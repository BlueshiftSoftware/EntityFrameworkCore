using System;
using System.Diagnostics;
using System.IO;

namespace Blueshift.MongoDB.Tests.Shared
{
    public class MongoDbFixture : IDisposable
    {
        private static readonly bool IsCiBuild;
        private Process _mongodProcess;

        static MongoDbFixture()
        {
            IsCiBuild =
                (bool.TryParse(Environment.ExpandEnvironmentVariables("%APPVEYOR%"), out bool isAppVeyor) && isAppVeyor) ||
                (bool.TryParse(Environment.ExpandEnvironmentVariables("%TRAVIS%"), out bool isTravisCi) && isTravisCi);
        }

        public MongoDbFixture()
        {
            if (!IsCiBuild)
            {
                Directory.CreateDirectory(MongoDbConstants.DataFolder);
                _mongodProcess = Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = MongoDbConstants.MongodExe,
                        Arguments = $@"-vvv --port {MongoDbConstants.MongodPort} --logpath "".data\{MongoDbConstants.MongodPort}.log"" --dbpath ""{MongoDbConstants.DataFolder}""",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    });
            }
        }

        public virtual void Dispose()
        {
            if (!IsCiBuild && _mongodProcess != null && !_mongodProcess.HasExited)
            {
                Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = MongoDbConstants.MongoExe,
                        Arguments = $@"""{MongoDbConstants.MongoUrl}/admin"" --eval ""db.shutdownServer({{ force: true, timeoutSecs: 30 }});""",
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