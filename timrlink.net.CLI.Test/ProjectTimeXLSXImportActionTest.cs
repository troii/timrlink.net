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
    public class ProjectTimeXLSXImportActionTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async System.Threading.Tasks.Task ParseSpanishDateTime()
        {
            List<Core.API.ProjectTime> projectTimes = new List<Core.API.ProjectTime>();

            var loggerFactory = new LoggerFactory();

            var projectTimeServiceMock = new Mock<IProjectTimeService>(MockBehavior.Loose);
            projectTimeServiceMock
                .Setup(service => service.SaveProjectTime(It.IsAny<Core.API.ProjectTime>()))
                .Callback((Core.API.ProjectTime projectTime) => projectTimes.Add(projectTime));
            projectTimeServiceMock
                .Setup(service => service.SaveProjectTimes(It.IsAny<IList<Core.API.ProjectTime>>()))
                .Callback((IEnumerable<Core.API.ProjectTime> pts) => projectTimes.AddRange(pts));

            var taskServiceMock = new Mock<ITaskService>(MockBehavior.Loose);
            taskServiceMock.Setup(service => service.CreateExternalIdDictionary(It.IsAny<IEnumerable<Task>>(), It.IsAny<Func<Task, string>>())).ReturnsAsync(new Dictionary<string, Task>());

            var importAction = new ProjectTimeXLSXImportAction(loggerFactory, "data/projecttime_spanish_datetime.xlsx", taskServiceMock.Object, projectTimeServiceMock.Object);
            await importAction.Execute();

            Assert.AreEqual(28, projectTimes.Count);

            {
                var projectTime = projectTimes[0];
                Assert.AreEqual("John Dow", projectTime.externalUserId);
                Assert.AreEqual("LockedTask", projectTime.externalTaskId);
                Assert.AreEqual("", projectTime.description);
                Assert.AreEqual(new DateTime(2019, 05, 08, 07, 00, 00), projectTime.startTime);
                Assert.IsNull(projectTime.startTimeZone);
                Assert.AreEqual(new DateTime(2019, 05, 08, 10, 00, 00), projectTime.endTime);
                Assert.IsNull(projectTime.endTimeZone);
                Assert.AreEqual(true, projectTime.billable);
                Assert.AreEqual(true, projectTime.changed);
                Assert.AreEqual(3 * 60, projectTime.duration);
                Assert.AreEqual(0, projectTime.breakTime);
            }

            {
                var projectTime = projectTimes[3];
                Assert.AreEqual("John Dow", projectTime.externalUserId);
                Assert.AreEqual("LockedTask", projectTime.externalTaskId);
                Assert.AreEqual("", projectTime.description);
                Assert.AreEqual(new DateTime(2019, 05, 13, 15, 58, 00), projectTime.startTime);
                Assert.IsNull(projectTime.startTimeZone);
                Assert.AreEqual(new DateTime(2019, 05, 13, 16, 23, 00), projectTime.endTime);
                Assert.IsNull(projectTime.endTimeZone);
                Assert.AreEqual(true, projectTime.billable);
                Assert.AreEqual(true, projectTime.changed);
                Assert.AreEqual(25, projectTime.duration);
                Assert.AreEqual(0, projectTime.breakTime);
            }

            {
                var projectTime = projectTimes[10];
                Assert.AreEqual("John Dow", projectTime.externalUserId);
                Assert.AreEqual("Parent|NewTask", projectTime.externalTaskId);
                Assert.AreEqual("Test für das \r\nwachte\r\n\r\nasdfasdfksdfa\r\n\r\n\r\nasdfajsdkfaskd\r\n\r\nDas sieht ja schon mal sehr gut aus!", projectTime.description);
                Assert.AreEqual(new DateTime(2019, 05, 29, 07, 40, 00), projectTime.startTime);
                Assert.IsNull(projectTime.startTimeZone);
                Assert.AreEqual(new DateTime(2019, 05, 29, 07, 40, 00), projectTime.endTime);
                Assert.IsNull(projectTime.endTimeZone);
                Assert.AreEqual(true, projectTime.billable);
                Assert.AreEqual(false, projectTime.changed);
                Assert.AreEqual(0, projectTime.duration);
                Assert.AreEqual(0, projectTime.breakTime);
            }
        }

        [Test]
        public async System.Threading.Tasks.Task ParseEnglishDateTime()
        {
            List<Core.API.ProjectTime> projectTimes = new List<Core.API.ProjectTime>();

            var loggerFactory = new LoggerFactory();

            var projectTimeServiceMock = new Mock<IProjectTimeService>(MockBehavior.Loose);
            projectTimeServiceMock
                .Setup(service => service.SaveProjectTime(It.IsAny<Core.API.ProjectTime>()))
                .Callback((Core.API.ProjectTime projectTime) => projectTimes.Add(projectTime));
            projectTimeServiceMock
                .Setup(service => service.SaveProjectTimes(It.IsAny<IList<Core.API.ProjectTime>>()))
                .Callback((IEnumerable<Core.API.ProjectTime> pts) => projectTimes.AddRange(pts));

            var taskServiceMock = new Mock<ITaskService>(MockBehavior.Loose);
            taskServiceMock.Setup(service => service.CreateExternalIdDictionary(It.IsAny<IEnumerable<Task>>(), It.IsAny<Func<Task, string>>())).ReturnsAsync(new Dictionary<string, Task>());

            var importAction = new ProjectTimeXLSXImportAction(loggerFactory, "data/projecttime_english_datetime.xlsx", taskServiceMock.Object, projectTimeServiceMock.Object);
            await importAction.Execute();

            Assert.AreEqual(28, projectTimes.Count);

            {
                var projectTime = projectTimes[0];
                Assert.AreEqual("John Dow", projectTime.externalUserId);
                Assert.AreEqual("LockedTask", projectTime.externalTaskId);
                Assert.AreEqual("", projectTime.description);
                Assert.AreEqual(new DateTime(2019, 05, 08, 07, 00, 00), projectTime.startTime);
                Assert.IsNull(projectTime.startTimeZone);
                Assert.AreEqual(new DateTime(2019, 05, 08, 10, 00, 00), projectTime.endTime);
                Assert.IsNull(projectTime.endTimeZone);
                Assert.AreEqual(true, projectTime.billable);
                Assert.AreEqual(true, projectTime.changed);
                Assert.AreEqual(3 * 60, projectTime.duration);
                Assert.AreEqual(0, projectTime.breakTime);
            }

            {
                var projectTime = projectTimes[3];
                Assert.AreEqual("John Dow", projectTime.externalUserId);
                Assert.AreEqual("LockedTask", projectTime.externalTaskId);
                Assert.AreEqual("", projectTime.description);
                Assert.AreEqual(new DateTime(2019, 05, 13, 15, 58, 00), projectTime.startTime);
                Assert.IsNull(projectTime.startTimeZone);
                Assert.AreEqual(new DateTime(2019, 05, 13, 16, 23, 00), projectTime.endTime);
                Assert.IsNull(projectTime.endTimeZone);
                Assert.AreEqual(true, projectTime.billable);
                Assert.AreEqual(true, projectTime.changed);
                Assert.AreEqual(25, projectTime.duration);
                Assert.AreEqual(0, projectTime.breakTime);
            }

            {
                var projectTime = projectTimes[10];
                Assert.AreEqual("John Dow", projectTime.externalUserId);
                Assert.AreEqual("Parent|NewTask", projectTime.externalTaskId);
                Assert.AreEqual("Test für das \r\nwachte\r\n\r\nasdfasdfksdfa\r\n\r\n\r\nasdfajsdkfaskd\r\n\r\nDas sieht ja schon mal sehr gut aus!", projectTime.description);
                Assert.AreEqual(new DateTime(2019, 05, 29, 07, 40, 00), projectTime.startTime);
                Assert.IsNull(projectTime.startTimeZone);
                Assert.AreEqual(new DateTime(2019, 05, 29, 07, 40, 00), projectTime.endTime);
                Assert.IsNull(projectTime.endTimeZone);
                Assert.AreEqual(true, projectTime.billable);
                Assert.AreEqual(false, projectTime.changed);
                Assert.AreEqual(0, projectTime.duration);
                Assert.AreEqual(0, projectTime.breakTime);
            }
        }

        [Test]
        public async System.Threading.Tasks.Task ParseGermanTime()
        {
            List<Core.API.ProjectTime> projectTimes = new List<Core.API.ProjectTime>();

            var loggerFactory = new LoggerFactory();

            var projectTimeServiceMock = new Mock<IProjectTimeService>(MockBehavior.Loose);
            projectTimeServiceMock
                .Setup(service => service.SaveProjectTime(It.IsAny<Core.API.ProjectTime>()))
                .Callback((Core.API.ProjectTime projectTime) => projectTimes.Add(projectTime));
            projectTimeServiceMock
                .Setup(service => service.SaveProjectTimes(It.IsAny<IList<Core.API.ProjectTime>>()))
                .Callback((IEnumerable<Core.API.ProjectTime> pts) => projectTimes.AddRange(pts));

            var taskServiceMock = new Mock<ITaskService>(MockBehavior.Loose);
            taskServiceMock.Setup(service => service.CreateExternalIdDictionary(It.IsAny<IEnumerable<Task>>(), It.IsAny<Func<Task, string>>())).ReturnsAsync(new Dictionary<string, Task>());

            var importAction = new ProjectTimeXLSXImportAction(loggerFactory, "data/projecttime_german_time.xlsx", taskServiceMock.Object, projectTimeServiceMock.Object);
            await importAction.Execute();

            Assert.AreEqual(3, projectTimes.Count);

            {
                var projectTime = projectTimes[0];
                Assert.AreEqual("", projectTime.externalUserId);
                Assert.AreEqual("bernhard budget", projectTime.externalTaskId);
                Assert.AreEqual("", projectTime.description);
                Assert.AreEqual(new DateTime(2019, 04, 30, 15, 12, 00), projectTime.startTime);
                Assert.AreEqual("+02:00", projectTime.startTimeZone);
                Assert.AreEqual(new DateTime(2019, 07, 15, 19, 10, 00), projectTime.endTime);
                Assert.AreEqual("+02:00", projectTime.endTimeZone);
                Assert.AreEqual(true, projectTime.billable);
                Assert.AreEqual(true, projectTime.changed);
                Assert.AreEqual(109678, projectTime.duration);
                Assert.AreEqual(0, projectTime.breakTime);
            }

            {
                var projectTime = projectTimes[1];
                Assert.AreEqual("", projectTime.externalUserId);
                Assert.AreEqual(
                    "Root Task timrTest für db Upgrade loooooooong very looooong text Root Task timrTest für db Upgrade loooooooong very looooong text Root Task timrTest für db Upgrade loooooooong very looooong text|another very long task xxxxxxxxxxxxxxxx xxxxxxxxxxxxxxxx xxxxxxxxxxxxxxxx xxxxxxxxxxxxxxxx xxxxxxxxxxxxxxxx xxxxxxxxxxxxxxxx xxxxxxxxxxxxxxxx xxxxxxxxxxxxxxxx |pfosjüfojsaopfjeoeafjsüjf438975984375970432§$%&§$%&§$%&§%$/54390860456094586 6 495 834 5680543m8v90bm389mb0683b mmm m3v 6 54v 6",
                    projectTime.externalTaskId
                );
                Assert.AreEqual("", projectTime.description);
                Assert.AreEqual(new DateTime(2019, 04, 30, 15, 12, 00), projectTime.startTime);
                Assert.AreEqual("+02:00", projectTime.startTimeZone);
                Assert.AreEqual(new DateTime(2019, 04, 30, 15, 12, 00), projectTime.endTime);
                Assert.AreEqual("+02:00", projectTime.endTimeZone);
                Assert.AreEqual(false, projectTime.billable);
                Assert.AreEqual(false, projectTime.changed);
                Assert.AreEqual(0, projectTime.duration);
                Assert.AreEqual(0, projectTime.breakTime);
            }
        }
    }
}
