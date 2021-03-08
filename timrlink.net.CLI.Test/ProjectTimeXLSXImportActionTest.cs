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
            
            var user1 = new User()
            {
                externalId = "John Dow"
            };

            var users = new List<User> {user1};

            var projectTimeServiceMock = new Mock<IProjectTimeService>(MockBehavior.Loose);
            projectTimeServiceMock
                .Setup(service => service.SaveProjectTime(It.IsAny<Core.API.ProjectTime>()))
                .Callback((Core.API.ProjectTime projectTime) => projectTimes.Add(projectTime));
            projectTimeServiceMock
                .Setup(service => service.SaveProjectTimes(It.IsAny<IList<Core.API.ProjectTime>>()))
                .Callback((IEnumerable<Core.API.ProjectTime> pts) => projectTimes.AddRange(pts));

            var taskServiceMock = new Mock<ITaskService>(MockBehavior.Loose);
            taskServiceMock.Setup(service => service.CreateExternalIdDictionary(It.IsAny<IEnumerable<Task>>(), It.IsAny<Func<Task, string>>())).ReturnsAsync(new Dictionary<string, Task>());
            taskServiceMock
                .Setup(service => service.GetTaskHierarchy())
                .ReturnsAsync(new List<Task>());
            
            var userServiceMock = new Mock<IUserService>(MockBehavior.Strict);
            userServiceMock
                .Setup(service => service.GetUsers())
                .ReturnsAsync(users);

            var importAction = new ProjectTimeXLSXImportAction(loggerFactory, "data/projecttime_spanish_datetime.xlsx", taskServiceMock.Object, userServiceMock.Object, projectTimeServiceMock.Object);
            await importAction.Execute();

            Assert.AreEqual(28, projectTimes.Count);

            {
                var projectTime = projectTimes[0];
                Assert.IsNull(projectTime.externalUserId);
                Assert.IsNull(projectTime.externalTaskId);
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
                Assert.IsNull(projectTime.externalUserId);
                Assert.IsNull(projectTime.externalTaskId);
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
                Assert.IsNull(projectTime.externalUserId);
                Assert.IsNull(projectTime.externalTaskId);
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
            
            var user1 = new User()
            {
                externalId = "John Dow"
            };

            var users = new List<User> {user1};

            var projectTimeServiceMock = new Mock<IProjectTimeService>(MockBehavior.Loose);
            projectTimeServiceMock
                .Setup(service => service.SaveProjectTime(It.IsAny<Core.API.ProjectTime>()))
                .Callback((Core.API.ProjectTime projectTime) => projectTimes.Add(projectTime));
            projectTimeServiceMock
                .Setup(service => service.SaveProjectTimes(It.IsAny<IList<Core.API.ProjectTime>>()))
                .Callback((IEnumerable<Core.API.ProjectTime> pts) => projectTimes.AddRange(pts));

            var taskServiceMock = new Mock<ITaskService>(MockBehavior.Loose);
            taskServiceMock.Setup(service => service.CreateExternalIdDictionary(It.IsAny<IEnumerable<Task>>(), It.IsAny<Func<Task, string>>())).ReturnsAsync(new Dictionary<string, Task>());
            taskServiceMock
                .Setup(service => service.GetTaskHierarchy())
                .ReturnsAsync(new List<Task>());
            
            var userServiceMock = new Mock<IUserService>(MockBehavior.Strict);
            userServiceMock
                .Setup(service => service.GetUsers())
                .ReturnsAsync(users);
            
            var importAction = new ProjectTimeXLSXImportAction(loggerFactory, "data/projecttime_english_datetime.xlsx", taskServiceMock.Object, userServiceMock.Object, projectTimeServiceMock.Object);
            await importAction.Execute();

            Assert.AreEqual(28, projectTimes.Count);

            {
                var projectTime = projectTimes[0];
                Assert.IsNull(projectTime.externalUserId);
                Assert.IsNull(projectTime.externalTaskId);
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
                Assert.IsNull(projectTime.externalUserId);
                Assert.IsNull(projectTime.externalTaskId);
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
                Assert.IsNull(projectTime.externalUserId);
                Assert.IsNull(projectTime.externalTaskId);
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
            
            var user1 = new User()
            {
                externalId = "John Dow"
            };

            var users = new List<User> {user1};

            var projectTimeServiceMock = new Mock<IProjectTimeService>(MockBehavior.Loose);
            projectTimeServiceMock
                .Setup(service => service.SaveProjectTime(It.IsAny<Core.API.ProjectTime>()))
                .Callback((Core.API.ProjectTime projectTime) => projectTimes.Add(projectTime));
            projectTimeServiceMock
                .Setup(service => service.SaveProjectTimes(It.IsAny<IList<Core.API.ProjectTime>>()))
                .Callback((IEnumerable<Core.API.ProjectTime> pts) => projectTimes.AddRange(pts));

            var taskServiceMock = new Mock<ITaskService>(MockBehavior.Loose);
            taskServiceMock.Setup(service => service.CreateExternalIdDictionary(It.IsAny<IEnumerable<Task>>(), It.IsAny<Func<Task, string>>())).ReturnsAsync(new Dictionary<string, Task>());
            taskServiceMock
                .Setup(service => service.GetTaskHierarchy())
                .ReturnsAsync(new List<Task>());
            
            var userServiceMock = new Mock<IUserService>(MockBehavior.Strict);
            userServiceMock
                .Setup(service => service.GetUsers())
                .ReturnsAsync(users);
            
            var importAction = new ProjectTimeXLSXImportAction(loggerFactory, "data/projecttime_german_time.xlsx", taskServiceMock.Object, userServiceMock.Object, projectTimeServiceMock.Object);
            await importAction.Execute();

            Assert.AreEqual(3, projectTimes.Count);

            {
                var projectTime = projectTimes[0];
                Assert.IsNull(projectTime.externalUserId);
                Assert.IsNull(projectTime.externalTaskId);
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
                Assert.IsNull(projectTime.externalUserId);
                Assert.IsNull(projectTime.externalTaskId);
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
        
        [Test]
        public async System.Threading.Tasks.Task ParseWith2UsersOnly1Enabled()
        {
            List<Core.API.ProjectTime> projectTimes = new List<Core.API.ProjectTime>();

            var loggerFactory = new LoggerFactory();
            
            var user1 = new User()
            {
                externalId = "Steve Jobs"
            };

            var users = new List<User> {user1};

            var projectTimeServiceMock = new Mock<IProjectTimeService>(MockBehavior.Loose);
            projectTimeServiceMock
                .Setup(service => service.SaveProjectTime(It.IsAny<Core.API.ProjectTime>()))
                .Callback((Core.API.ProjectTime projectTime) => projectTimes.Add(projectTime));
            projectTimeServiceMock
                .Setup(service => service.SaveProjectTimes(It.IsAny<IList<Core.API.ProjectTime>>()))
                .Callback((IEnumerable<Core.API.ProjectTime> pts) => projectTimes.AddRange(pts));

            var taskServiceMock = new Mock<ITaskService>(MockBehavior.Loose);
            taskServiceMock.Setup(service => service.CreateExternalIdDictionary(It.IsAny<IEnumerable<Task>>(), It.IsAny<Func<Task, string>>())).ReturnsAsync(new Dictionary<string, Task>());
            taskServiceMock
                .Setup(service => service.GetTaskHierarchy())
                .ReturnsAsync(new List<Task>());
            
            var userServiceMock = new Mock<IUserService>(MockBehavior.Strict);
            userServiceMock
                .Setup(service => service.GetUsers())
                .ReturnsAsync(users);
            
            var importAction = new ProjectTimeXLSXImportAction(loggerFactory, "data/projecttime_two_users.xlsx", taskServiceMock.Object, userServiceMock.Object, projectTimeServiceMock.Object);
            await importAction.Execute();

            Assert.AreEqual(2, projectTimes.Count);

            {
                var projectTime = projectTimes[0];
                Assert.IsNull(projectTime.externalUserId);
                Assert.IsNull(projectTime.externalTaskId);
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
                Assert.IsNull(projectTime.externalUserId);
                Assert.IsNull(projectTime.externalTaskId);
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
