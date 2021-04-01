using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Serilog;
using timrlink.net.CLI.Actions;
using timrlink.net.Core.API;
using timrlink.net.Core.Service;

namespace timrlink.net.CLI.Test
{
    public class ProjectTimeCSVImportActionTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async System.Threading.Tasks.Task TaskCreation()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug)); 
            
            var users = new List<User>
            {
                new User { externalId = "John Dow" },
                new User { externalId = "John Cena" }
            };

            var tasks = new List<Task>();

            var projectTimeServiceMock = new Mock<IProjectTimeService>(MockBehavior.Loose);

            var taskServiceMock = new Mock<ITaskService>(MockBehavior.Loose);
            taskServiceMock
                .Setup(service => service.CreateExternalIdDictionary(It.IsAny<IEnumerable<Task>>(), It.IsAny<Func<Task, string>>())).ReturnsAsync(new Dictionary<string, Task>());
            taskServiceMock
                .Setup(service => service.AddTask(It.IsAny<Task>()))
                .Callback((Task task) => tasks.Add(task));

            var userServiceMock = new Mock<IUserService>(MockBehavior.Strict);
            userServiceMock
                .Setup(service => service.GetUsers())
                .ReturnsAsync(users);

            var importAction = new ProjectTimeCSVImportAction(loggerFactory, "data/projecttime.csv", taskServiceMock.Object, userServiceMock.Object, projectTimeServiceMock.Object);
            await importAction.Execute();

            Assert.AreEqual(3, tasks.Count);

            var internalTask = tasks[0];
            Assert.AreEqual("INTERNAL", internalTask.name);
            Assert.AreEqual("INTERNAL", internalTask.externalId);
            Assert.IsNull(internalTask.parentExternalId);

            var holidayTask = tasks[1];
            Assert.AreEqual("Holiday", holidayTask.name);
            Assert.AreEqual("INTERNAL|Holiday", holidayTask.externalId);
            Assert.AreEqual("INTERNAL", holidayTask.parentExternalId);

            var pmTask = tasks[2];
            Assert.AreEqual("PM", pmTask.name);
            Assert.AreEqual("INTERNAL|PM", pmTask.externalId);
            Assert.AreEqual("INTERNAL", pmTask.parentExternalId);
        }

        [Test]
        public async System.Threading.Tasks.Task ProjectTimeImport()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

            var users = new List<User>
            {
                new User { externalId = "John Dow" },
                new User { externalId = "John Cena" }
            };

            var projectTimes = new List<Core.API.ProjectTime>();

            var projectTimeServiceMock = new Mock<IProjectTimeService>(MockBehavior.Loose);
            projectTimeServiceMock
                .Setup(service => service.SaveProjectTime(It.IsAny<Core.API.ProjectTime>()))
                .Callback((Core.API.ProjectTime projectTime) => projectTimes.Add(projectTime));
            projectTimeServiceMock
                .Setup(service => service.SaveProjectTimes(It.IsAny<IList<Core.API.ProjectTime>>()))
                .Callback((IEnumerable<Core.API.ProjectTime> pts) => projectTimes.AddRange(pts));

            var taskServiceMock = new Mock<ITaskService>(MockBehavior.Loose);
            taskServiceMock
                .Setup(service => service.CreateExternalIdDictionary(It.IsAny<IEnumerable<Task>>(), It.IsAny<Func<Task, string>>())).ReturnsAsync(new Dictionary<string, Task>());

            var userServiceMock = new Mock<IUserService>(MockBehavior.Strict);
            userServiceMock
                .Setup(service => service.GetUsers())
                .ReturnsAsync(users);

            var importAction = new ProjectTimeCSVImportAction(loggerFactory, "data/projecttime.csv", taskServiceMock.Object, userServiceMock.Object, projectTimeServiceMock.Object);
            await importAction.Execute();

            Assert.AreEqual(8, projectTimes.Count);
            {
                var projectTime = projectTimes[0];
                Assert.AreEqual("John Dow", projectTime.externalUserId);
                Assert.AreEqual("INTERNAL|Holiday", projectTime.externalTaskId);
                Assert.AreEqual("", projectTime.description);
                Assert.AreEqual(new DateTime(2015, 12, 01, 08, 00, 00), projectTime.startTime);
                Assert.IsNull(projectTime.startTimeZone);
                Assert.AreEqual(new DateTime(2015, 12, 01, 16, 30, 00), projectTime.endTime);
                Assert.IsNull(projectTime.endTimeZone);
                Assert.AreEqual(false, projectTime.billable);
                Assert.AreEqual(30, projectTime.breakTime);
            }
        }

        [Test]
        public async System.Threading.Tasks.Task ProjectTimeImportNonUniqueUsers()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

            var users = new List<User>
            {
                new User { login = "john.dow", externalId = "John Dow" },
                new User { login = "john.d", externalId = "John Dow" },
                new User { login = "john.cena", externalId = "John Cena" }
            };

            var projectTimes = new List<Core.API.ProjectTime>();

            var projectTimeServiceMock = new Mock<IProjectTimeService>(MockBehavior.Loose);
            projectTimeServiceMock
                .Setup(service => service.SaveProjectTime(It.IsAny<Core.API.ProjectTime>()))
                .Callback((Core.API.ProjectTime projectTime) => projectTimes.Add(projectTime));
            projectTimeServiceMock
                .Setup(service => service.SaveProjectTimes(It.IsAny<IList<Core.API.ProjectTime>>()))
                .Callback((IEnumerable<Core.API.ProjectTime> pts) => projectTimes.AddRange(pts));

            var taskServiceMock = new Mock<ITaskService>(MockBehavior.Loose);
            taskServiceMock
                .Setup(service => service.CreateExternalIdDictionary(It.IsAny<IEnumerable<Task>>(), It.IsAny<Func<Task, string>>())).ReturnsAsync(new Dictionary<string, Task>());

            var userServiceMock = new Mock<IUserService>(MockBehavior.Strict);
            userServiceMock
                .Setup(service => service.GetUsers())
                .ReturnsAsync(users);

            var importAction = new ProjectTimeCSVImportAction(loggerFactory, "data/projecttime.csv", taskServiceMock.Object, userServiceMock.Object, projectTimeServiceMock.Object);
            await importAction.Execute();

            Assert.AreEqual(2, projectTimes.Count);
            Assert.True(projectTimes.TrueForAll(projectTime => projectTime.externalUserId != "John Dow"), "contains projecttime for non-unique John Dow");

            {
                var projectTime = projectTimes[0];
                Assert.AreEqual("John Cena", projectTime.externalUserId);
                Assert.AreEqual("INTERNAL|PM", projectTime.externalTaskId);
                Assert.AreEqual("Boring Company, Boring Stuff ", projectTime.description);
                Assert.AreEqual(new DateTime(2016, 02, 26, 08, 00, 00), projectTime.startTime);
                Assert.IsNull(projectTime.startTimeZone);
                Assert.AreEqual(new DateTime(2016, 02, 26, 16, 30, 00), projectTime.endTime);
                Assert.IsNull(projectTime.endTimeZone);
                Assert.AreEqual(true, projectTime.billable);
                Assert.AreEqual(30, projectTime.breakTime);
            }
        }
    }
}
