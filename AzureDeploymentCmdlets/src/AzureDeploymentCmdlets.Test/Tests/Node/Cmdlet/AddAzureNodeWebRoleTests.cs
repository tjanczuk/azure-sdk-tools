﻿// ----------------------------------------------------------------------------------
//
// Copyright 2011 Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using System.IO;
using AzureDeploymentCmdlets.Cmdlet;
using AzureDeploymentCmdlets.Node.Cmdlet;
using AzureDeploymentCmdlets.Properties;
using AzureDeploymentCmdlets.Test.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AzureDeploymentCmdlets.Test.Tests.Node.Cmdlet
{
    [TestClass]
    public class AddAzureNodeWebRoleTests : TestBase
    {
        [TestMethod]
        public void AddAzureNodeWebRoleProcess()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                new NewAzureServiceCommand().NewAzureServiceProcess(files.RootPath, "AzureService");
                new AddAzureNodeWebRoleCommand().AddAzureNodeWebRoleProcess("WebRole", 1, Path.Combine(files.RootPath, "AzureService"));

                AzureAssert.ScaffoldingExists(Path.Combine(files.RootPath, "AzureService", "WebRole"), Path.Combine(Resources.NodeScaffolding, Resources.WebRole));
            }
        }
    }
}