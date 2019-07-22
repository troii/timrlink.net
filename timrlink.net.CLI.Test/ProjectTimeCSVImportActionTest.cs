using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using timrlink.net.CLI.Actions;
using timrlink.net.Core.API;
using timrlink.net.Core.Service;

namespace Tests
{
    public class ProjectTimeCSVImportActionTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async System.Threading.Tasks.Task ParseCSV()
        {
            List<ProjectTime> projectTimes = new List<ProjectTime>();

            var loggerFactory = new LoggerFactory();

            var projectTimeServiceMock = new Mock<IProjectTimeService>(MockBehavior.Loose);
            projectTimeServiceMock
                .Setup(service => service.SaveProjectTime(It.IsAny<ProjectTime>()))
                .Callback((ProjectTime projectTime) => projectTimes.Add(projectTime));
            projectTimeServiceMock
                .Setup(service => service.SaveProjectTimes(It.IsAny<IList<ProjectTime>>()))
                .Callback((IEnumerable<ProjectTime> pts) => projectTimes.AddRange(pts));

            var taskServiceMock = new Mock<ITaskService>(MockBehavior.Loose);
            taskServiceMock.Setup(service => service.CreateExternalIdDictionary(It.IsAny<IEnumerable<Task>>(), It.IsAny<Func<Task, string>>())).ReturnsAsync(new Dictionary<string, Task>());

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
