using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
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
            var tasks = new List<Task>();

            var loggerFactory = new LoggerFactory();

            var projectTimeServiceMock = new Mock<IProjectTimeService>(MockBehavior.Loose);

            var taskServiceMock = new Mock<ITaskService>(MockBehavior.Loose);
            taskServiceMock
                .Setup(service => service.CreateExternalIdDictionary(It.IsAny<IEnumerable<Task>>(), It.IsAny<Func<Task, string>>())).ReturnsAsync(new Dictionary<string, Task>());
            taskServiceMock
                .Setup(service => service.AddTask(It.IsAny<Task>()))
                .Callback((Task task) => tasks.Add(task));

            var importAction = new ProjectTimeCSVImportAction(loggerFactory, "data/projecttime.csv", taskServiceMock.Object, projectTimeServiceMock.Object);
            await importAction.Execute();

            Assert.AreEqual(3, tasks.Count);

            Assert.AreEqual("INTERNAL", tasks[0].name);
            Assert.AreEqual("INTERNAL", tasks[0].externalId);
            Assert.IsNull(tasks[0].parentExternalId);

            Assert.AreEqual("Holiday", tasks[1].name);
            Assert.AreEqual("INTERNAL|Holiday", tasks[1].externalId);
            Assert.AreEqual("INTERNAL", tasks[1].parentExternalId);

            Assert.AreEqual("PM", tasks[2].name);
            Assert.AreEqual("INTERNAL|PM", tasks[2].externalId);
            Assert.AreEqual("INTERNAL", tasks[2].parentExternalId);
        }

        [Test]
        public async System.Threading.Tasks.Task ParseCSV()
        {
            var projectTimes = new List<Core.API.ProjectTime>();

            var loggerFactory = new LoggerFactory();

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

            var importAction = new ProjectTimeCSVImportAction(loggerFactory, "data/projecttime.csv", taskServiceMock.Object, projectTimeServiceMock.Object);
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
    }
}
