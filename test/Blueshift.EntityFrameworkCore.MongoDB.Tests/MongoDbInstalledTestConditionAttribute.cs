using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;

namespace Blueshift.EntityFrameworkCore.MongoDB.Tests
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly)]
    public class MongoDbInstalledTestConditionAttribute : Attribute, ITestCondition
    {
        public bool IsMet => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && File.Exists(MongoDbConstants.MongodExe);

        public string SkipReason =>
            $"RuntimeInformation.IsOSPlatform(OSPlatform.Windows): {RuntimeInformation.IsOSPlatform(OSPlatform.Windows)}\r\n" +
            $"File.Exists({MongoDbConstants.MongodExe}): {File.Exists(MongoDbConstants.MongodExe)}\r\n" +
            "MongoDB is not installed on the local system or its location is unknown.";
    }
}