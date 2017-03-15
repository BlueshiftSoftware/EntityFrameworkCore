// Copyright (c) Blueshift Software, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using System.Resources;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Xunit;

[assembly: NeutralResourcesLanguage("en-US")]
[assembly: AssemblyMetadata("Serviceable", "True")]

[assembly: TestFramework("Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit.ConditionalTestFramework", "Microsoft.EntityFrameworkCore.Specification.Tests")]
[assembly: FrameworkSkipCondition(RuntimeFrameworks.CLR, SkipReason = "MongoDB Driver is not signed and tests will not load for .NET 4.5.1.")]