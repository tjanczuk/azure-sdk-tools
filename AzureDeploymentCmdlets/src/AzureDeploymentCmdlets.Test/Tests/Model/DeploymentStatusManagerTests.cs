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

using System.ServiceModel;
using AzureDeploymentCmdlets.Model;
using AzureDeploymentCmdlets.Properties;
using AzureDeploymentCmdlets.Test.TestData;
using AzureDeploymentCmdlets.WAPPSCmdlet;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AzureDeploymentCmdlets.Test.Tests.Model
{
    [TestClass]
    public class DeploymentStatusManagerTests : TestBase
    {
        private const string serviceName = "AzureService";
        private readonly string slot = ArgumentConstants.Slots[Slot.Production];

        [TestMethod]
        public void SetDeploymentStatusProcessTest()
        {
            SimpleServiceManagement channel = new SimpleServiceManagement();
            string newStatus = DeploymentStatus.Suspended;
            string currentStatus = DeploymentStatus.Running;
            bool statusUpdated = false;
            channel.UpdateDeploymentStatusBySlotThunk = ar => 
            {
                statusUpdated = true;
                channel.GetDeploymentBySlotThunk = ar2 => new Deployment(serviceName, slot, newStatus);
            };
            channel.GetDeploymentBySlotThunk = ar => new Deployment(serviceName, slot, currentStatus);

            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                files.CreateAzureSdkDirectoryAndImportPublishSettings();
                AzureService service = new AzureService(files.RootPath, serviceName, null);
                new DeploymentStatusManager(channel).SetDeploymentStatusProcess(service.Paths.RootPath, newStatus, slot, Data.ValidSubscriptionName[0], serviceName);

                Assert.IsTrue(statusUpdated);
            }
        }

        [TestMethod]
        public void SetDeploymentStatusProcessSetTransitioningServiceTest()
        {
            SimpleServiceManagement channel = new SimpleServiceManagement();
            string newStatus = DeploymentStatus.Suspended;
            string currentStatus = DeploymentStatus.RunningTransitioning;
            string resultMessage;
            string expectedMessage = string.Format(Resources.ServiceIsInTransitionState, slot, serviceName, currentStatus);
            bool statusUpdated = false;
            channel.UpdateDeploymentStatusBySlotThunk = ar =>
            {
                statusUpdated = true;
                channel.GetDeploymentBySlotThunk = ar2 => new Deployment(serviceName, slot, newStatus);
            };
            channel.GetDeploymentBySlotThunk = ar => new Deployment(serviceName, slot, currentStatus);

            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                files.CreateAzureSdkDirectoryAndImportPublishSettings();
                AzureService service = new AzureService(files.RootPath, serviceName, null);
                resultMessage = new DeploymentStatusManager(channel).SetDeploymentStatusProcess(service.Paths.RootPath, newStatus, slot, Data.ValidSubscriptionName[0], serviceName);

                Assert.IsFalse(statusUpdated);
                Assert.AreEqual<string>(expectedMessage, resultMessage);
            }
        }

        [TestMethod]
        public void SetDeploymentStatusProcessSetStatusToActualStatusTest()
        {
            SimpleServiceManagement channel = new SimpleServiceManagement();
            string newStatus = DeploymentStatus.Suspended;
            string currentStatus = DeploymentStatus.Suspended;
            string resultMessage;
            string expectedMessage = string.Format(Resources.DeploymentAlreadyInState, slot, serviceName, currentStatus);
            bool statusUpdated = false;
            channel.UpdateDeploymentStatusBySlotThunk = ar =>
            {
                statusUpdated = true;
                channel.GetDeploymentBySlotThunk = ar2 => new Deployment(serviceName, slot, newStatus);
            };
            channel.GetDeploymentBySlotThunk = ar => new Deployment(serviceName, slot, currentStatus);

            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                files.CreateAzureSdkDirectoryAndImportPublishSettings();
                AzureService service = new AzureService(files.RootPath, serviceName, null);
                resultMessage = new DeploymentStatusManager(channel).SetDeploymentStatusProcess(service.Paths.RootPath, newStatus, slot, Data.ValidSubscriptionName[0], serviceName);

                Assert.IsFalse(statusUpdated);
                Assert.AreEqual<string>(expectedMessage, resultMessage);
            }
        }

        [TestMethod]
        public void SetDeploymentStatusProcessDeploymentDoesNotExistTest()
        {
            SimpleServiceManagement channel = new SimpleServiceManagement();
            string newStatus = DeploymentStatus.Running;
            string resultMessage;
            string expectedMessage = string.Format(Resources.ServiceSlotDoesNotExist, serviceName, slot);
            bool statusUpdated = false;
            channel.UpdateDeploymentStatusBySlotThunk = ar =>
            {
                statusUpdated = true;
                channel.GetDeploymentBySlotThunk = ar2 => new Deployment(serviceName, slot, newStatus);
            };
            channel.GetDeploymentBySlotThunk = ar => { throw new EndpointNotFoundException(); };

            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                files.CreateAzureSdkDirectoryAndImportPublishSettings();
                AzureService service = new AzureService(files.RootPath, serviceName, null);
                resultMessage = new DeploymentStatusManager(channel).SetDeploymentStatusProcess(service.Paths.RootPath, newStatus, slot, Data.ValidSubscriptionName[0], serviceName);

                Assert.IsFalse(statusUpdated);
                Assert.AreEqual<string>(expectedMessage, resultMessage);
            }
        }
    }
}