using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using timrlink.net.CLI.Actions;
using timrlink.net.Core.API;
using timrlink.net.Core.Service;

using API = timrlink.net.Core.API;

namespace timrlink.net.CLI.Test
{
    public class ProjectTimeDatabaseExportActionTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void FromDateNullAndToDateValid()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
            
            var timrSyncMock = new Mock<TimrSync>(MockBehavior.Strict);
            var projectTimeServiceMock = new Mock<IProjectTimeService>(MockBehavior.Loose);
            var userServiceMock = new Mock<IUserService>(MockBehavior.Strict);

            var taskService =
                new TaskService(loggerFactory.CreateLogger<TaskService>(), loggerFactory, timrSyncMock.Object);

            var memoryContext = new DatabaseContext();
            var importAction = new ProjectTimeDatabaseExportAction(loggerFactory, memoryContext, "2022-10-01", to: null, userServiceMock.Object, taskService, projectTimeServiceMock.Object);
            Assert.ThrowsAsync<ArgumentException>(() => importAction.Execute());
        }
        
        [Test]
        public void FromDateValidAndToDateNull()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
            
            var timrSyncMock = new Mock<TimrSync>(MockBehavior.Strict);
            var projectTimeServiceMock = new Mock<IProjectTimeService>(MockBehavior.Loose);
            var userServiceMock = new Mock<IUserService>(MockBehavior.Strict);

            var taskService =
                new TaskService(loggerFactory.CreateLogger<TaskService>(), loggerFactory, timrSyncMock.Object);

            var memoryContext = new DatabaseContext();
            var importAction = new ProjectTimeDatabaseExportAction(loggerFactory, memoryContext, null, to: "2022-10-01", userServiceMock.Object, taskService, projectTimeServiceMock.Object);
            Assert.ThrowsAsync<ArgumentException>(() => importAction.Execute());
        }
        
        [Test]
        public void FromDateValidAndToDateInvalid()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
            
            var timrSyncMock = new Mock<TimrSync>(MockBehavior.Strict);
            var projectTimeServiceMock = new Mock<IProjectTimeService>(MockBehavior.Loose);
            var userServiceMock = new Mock<IUserService>(MockBehavior.Strict);

            var taskService =
                new TaskService(loggerFactory.CreateLogger<TaskService>(), loggerFactory, timrSyncMock.Object);

            var memoryContext = new DatabaseContext();
            var importAction = new ProjectTimeDatabaseExportAction(loggerFactory, memoryContext, "2022-10-01", to: "2022-10-01:9999", userServiceMock.Object, taskService, projectTimeServiceMock.Object);
            Assert.ThrowsAsync<ArgumentException>(() => importAction.Execute());
        }
        
        [Test]
        public void FromDateInvalidAndToDateValid()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
            
            var timrSyncMock = new Mock<TimrSync>(MockBehavior.Strict);
            var projectTimeServiceMock = new Mock<IProjectTimeService>(MockBehavior.Loose);
            var userServiceMock = new Mock<IUserService>(MockBehavior.Strict);

            var taskService =
                new TaskService(loggerFactory.CreateLogger<TaskService>(), loggerFactory, timrSyncMock.Object);

            var memoryContext = new DatabaseContext();
            var importAction = new ProjectTimeDatabaseExportAction(loggerFactory, memoryContext, "2022-10-01:9999", to: "2022-10-01", userServiceMock.Object, taskService, projectTimeServiceMock.Object);
            Assert.ThrowsAsync<ArgumentException>(() => importAction.Execute());
        }

        [Test]
        public void FromDateAferToDate()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
            
            var timrSyncMock = new Mock<TimrSync>(MockBehavior.Strict);
            var projectTimeServiceMock = new Mock<IProjectTimeService>(MockBehavior.Loose);
            var userServiceMock = new Mock<IUserService>(MockBehavior.Strict);

            var taskService =
                new TaskService(loggerFactory.CreateLogger<TaskService>(), loggerFactory, timrSyncMock.Object);

            var memoryContext = new DatabaseContext();
            var importAction = new ProjectTimeDatabaseExportAction(loggerFactory, memoryContext, "2022-10-02", to: "2022-10-01", userServiceMock.Object, taskService, projectTimeServiceMock.Object);
            Assert.ThrowsAsync<ArgumentException>(() => importAction.Execute());
        }
        
        [Test]
        public async System.Threading.Tasks.Task InsertProjectTimeToDatabase()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

            const string userUuid = "32c8c87e-43ea-11ed-b878-0242ac120002";
            const string taskUuid = "2909B8F0-4996-4D51-A2BA-1EB690AB2102";
            
            var users = new List<User>
            {
                new User 
                { 
                    externalId = "John Carmack", 
                    uuid = userUuid
                }
            };

            var task = new Task
            {
                name = "Customer B",
                uuid = taskUuid
            };
            var tasks = new List<Task> { task };
            
            var startTime = DateTime.Parse("2022-10-02T10:30:00+02:00");
            var endTime = DateTime.Parse("2022-10-02T11:30:00+02:00");

            var projectTime = new API.ProjectTime();
            projectTime.startTime = startTime;
            projectTime.startTimeZone = "+02:00";
            projectTime.endTime = endTime;
            projectTime.endTimeZone = "+02:00";
            projectTime.userUuid = userUuid;
            projectTime.uuid = "83f1bd14-43ea-11ed-b878-0242ac120002";
            projectTime.taskUuid = taskUuid;
            projectTime.billable = true;
            projectTime.changed = true;
            projectTime.breakTime = 12;
            projectTime.duration = 1000;
            projectTime.closed = false;


            var projectTimes = new List<Core.API.ProjectTime> { projectTime };
            
            var projectTimeServiceMock = new Mock<IProjectTimeService>(MockBehavior.Strict);
            projectTimeServiceMock
                .Setup(service => service.GetProjectTimes(It.IsAny<DateTime>(), It.IsAny<DateTime>(), null, null, null, null, null))
                .ReturnsAsync(projectTimes);
            
            var userServiceMock = new Mock<IUserService>(MockBehavior.Strict);
            userServiceMock
                .Setup(service => service.GetUsers())
                .ReturnsAsync(users);
            
            var taskServiceMock = new Mock<ITaskService>(MockBehavior.Loose);
            taskServiceMock
                .Setup(service => service.GetTaskHierarchy(It.IsAny<GetTasksRequest>())).ReturnsAsync(new List<Task>());
            taskServiceMock
                .Setup(service => service.FlattenTasks(It.IsAny<IList<Task>>()))
                .Returns(tasks);

            var memoryContext = new DatabaseContext();
            var importAction = new ProjectTimeDatabaseExportAction(loggerFactory, memoryContext, "2022-10-02", to: "2022-10-02", userServiceMock.Object, taskServiceMock.Object, projectTimeServiceMock.Object);
            await importAction.Execute();

            var projectTimeDatabase = memoryContext.ProjectTimes.First();
            Assert.AreEqual(true, projectTimeDatabase.Billable);
            Assert.AreEqual(true, projectTimeDatabase.Changed);
            Assert.AreEqual(false, projectTimeDatabase.Closed);
            Assert.AreEqual(12, projectTimeDatabase.BreakTime);
            Assert.AreEqual(1000, projectTimeDatabase.Duration);
            Assert.IsNull(projectTimeDatabase.Deleted);
            
            var metadata = memoryContext.Metadata.FirstOrDefault();
            Assert.IsNull(metadata);
        }

        [Test]
        public async System.Threading.Tasks.Task TestIfDeleteFlagIsSet()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

            const string userUuid = "32c8c87e-43ea-11ed-b878-0242ac120002";
            const string taskUuid = "2909B8F0-4996-4D51-A2BA-1EB690AB2102";
            
            var users = new List<User>
            {
                new User 
                { 
                    externalId = "John Carmack", 
                    uuid = userUuid
                }
            };

            var task = new Task
            {
                name = "Customer B",
                uuid = taskUuid
            };
            var tasks = new List<Task> { task };
            
            var startTime = DateTime.Parse("2022-10-02T10:30:00+02:00");
            var endTime = DateTime.Parse("2022-10-02T11:30:00+02:00");

            var projectTime = new API.ProjectTime();
            projectTime.startTime = startTime;
            projectTime.startTimeZone = "+02:00";
            projectTime.endTime = endTime;
            projectTime.endTimeZone = "+02:00";
            projectTime.userUuid = userUuid;
            projectTime.uuid = "83f1bd14-43ea-11ed-b878-0242ac120002";
            projectTime.taskUuid = taskUuid;
            projectTime.billable = true;
            projectTime.changed = true;
            projectTime.breakTime = 12;
            projectTime.duration = 1000;
            projectTime.closed = false;


            var projectTimes = new List<API.ProjectTime> { projectTime };
            
            var projectTimeServiceMock = new Mock<IProjectTimeService>(MockBehavior.Strict);
            projectTimeServiceMock
                .Setup(service => service.GetProjectTimes(It.IsAny<DateTime>(), It.IsAny<DateTime>(), null, null, null, null, null))
                .ReturnsAsync(projectTimes);
            
            var userServiceMock = new Mock<IUserService>(MockBehavior.Strict);
            userServiceMock
                .Setup(service => service.GetUsers())
                .ReturnsAsync(users);
            
            var taskServiceMock = new Mock<ITaskService>(MockBehavior.Loose);
            taskServiceMock
                .Setup(service => service.GetTaskHierarchy(It.IsAny<GetTasksRequest>())).ReturnsAsync(new List<Task>());
            taskServiceMock
                .Setup(service => service.FlattenTasks(It.IsAny<IList<Task>>()))
                .Returns(tasks);

            var memoryContext = new DatabaseContext();
            var importAction = new ProjectTimeDatabaseExportAction(loggerFactory, memoryContext, "2022-10-02", to: "2022-10-03", userServiceMock.Object, taskServiceMock.Object, projectTimeServiceMock.Object);
            await importAction.Execute();
            
            var projectTimeDatabase = memoryContext.ProjectTimes.First();
            Assert.AreEqual(true, projectTimeDatabase.Billable);
            Assert.AreEqual(true, projectTimeDatabase.Changed);
            Assert.AreEqual(false, projectTimeDatabase.Closed);
            Assert.AreEqual(12, projectTimeDatabase.BreakTime);
            Assert.AreEqual(1000, projectTimeDatabase.Duration);
            Assert.IsNull(projectTimeDatabase.Deleted);
            
            projectTimeServiceMock
                .Setup(service => service.GetProjectTimes(It.IsAny<DateTime>(), It.IsAny<DateTime>(), null, null, null, null, null))
                .ReturnsAsync(new List<Core.API.ProjectTime>());
            
            await importAction.Execute();

            Assert.AreEqual(true, projectTimeDatabase.Billable);
            Assert.AreEqual(true, projectTimeDatabase.Changed);
            Assert.AreEqual(false, projectTimeDatabase.Closed);
            Assert.AreEqual(12, projectTimeDatabase.BreakTime);
            Assert.AreEqual(1000, projectTimeDatabase.Duration);
            Assert.IsNotNull(projectTimeDatabase.Deleted);
            
            var metadata = memoryContext.Metadata.FirstOrDefault();
            Assert.IsNull(metadata);
        }
        
        [Test]
        public async System.Threading.Tasks.Task UpdateMovedTaskInOtherPeriod()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

            const string userUuid = "32c8c87e-43ea-11ed-b878-0242ac120002";
            const string taskUuid = "2909B8F0-4996-4D51-A2BA-1EB690AB2102";
            
            var users = new List<User>
            {
                new User 
                { 
                    externalId = "John Carmack", 
                    uuid = userUuid
                }
            };

            var task = new Task
            {
                name = "Customer B",
                uuid = taskUuid
            };
            var tasks = new List<Task> { task };
            
            var startTime = DateTime.Parse("2022-10-02T10:30:00+02:00");
            var endTime = DateTime.Parse("2022-10-02T11:30:00+02:00");

            var projectTime = new API.ProjectTime();
            projectTime.startTime = startTime;
            projectTime.startTimeZone = "+02:00";
            projectTime.endTime = endTime;
            projectTime.endTimeZone = "+02:00";
            projectTime.userUuid = userUuid;
            projectTime.uuid = "83f1bd14-43ea-11ed-b878-0242ac120002";
            projectTime.taskUuid = taskUuid;
            projectTime.billable = true;
            projectTime.changed = true;
            projectTime.breakTime = 12;
            projectTime.duration = 1000;
            projectTime.closed = false;
            projectTime.description = "Donnerkogel";


            var projectTimes = new List<Core.API.ProjectTime> { projectTime };
            
            var projectTimeServiceMock = new Mock<IProjectTimeService>(MockBehavior.Strict);
            projectTimeServiceMock
                .Setup(service => service.GetProjectTimes(It.IsAny<DateTime>(), It.IsAny<DateTime>(), null, null, null, null, null))
                .ReturnsAsync(projectTimes);
            
            var userServiceMock = new Mock<IUserService>(MockBehavior.Strict);
            userServiceMock
                .Setup(service => service.GetUsers())
                .ReturnsAsync(users);
            
            var taskServiceMock = new Mock<ITaskService>(MockBehavior.Loose);
            taskServiceMock
                .Setup(service => service.GetTaskHierarchy(It.IsAny<GetTasksRequest>())).ReturnsAsync(new List<Task>());
            taskServiceMock
                .Setup(service => service.FlattenTasks(It.IsAny<IList<Task>>()))
                .Returns(tasks);

            var memoryContext = new DatabaseContext();
            
            var importAction = new ProjectTimeDatabaseExportAction(loggerFactory, memoryContext, "2022-10-02",
                to: "2022-10-03", userServiceMock.Object, taskServiceMock.Object,
                projectTimeServiceMock.Object);
            await importAction.Execute();
            
            // First we check if the project time is correctly inserted
            var projectTimeDatabase = memoryContext.ProjectTimes.Single();
            Assert.AreEqual(true, projectTimeDatabase.Billable);
            Assert.AreEqual(true, projectTimeDatabase.Changed);
            Assert.AreEqual(false, projectTimeDatabase.Closed);
            Assert.AreEqual(12, projectTimeDatabase.BreakTime);
            Assert.AreEqual(1000, projectTimeDatabase.Duration);
            Assert.AreEqual("Donnerkogel", projectTimeDatabase.Description);
            Assert.IsNull(projectTimeDatabase.Deleted);

            // Then we "change the date" of the project time and simulate here it has moved to another period
            projectTimeServiceMock
                .Setup(service => service.GetProjectTimes(It.IsAny<DateTime>(), It.IsAny<DateTime>(), null, null,
                    null, null, null))
                .ReturnsAsync(new List<Core.API.ProjectTime>());

            await importAction.Execute();

            // Now the deleted flag must be set
            var projectTimeDatabase2 = memoryContext.ProjectTimes.ToList().First();
            Assert.AreEqual(true, projectTimeDatabase2.Billable);
            Assert.AreEqual(true, projectTimeDatabase2.Changed);
            Assert.AreEqual(false, projectTimeDatabase2.Closed);
            Assert.AreEqual(12, projectTimeDatabase2.BreakTime);
            Assert.AreEqual(1000, projectTimeDatabase2.Duration);
            Assert.IsNotNull(projectTimeDatabase2.Deleted);

            // Here we update the description and return the project time one day earlier
            projectTime.description = "Dachstein";

            projectTimeServiceMock
                .Setup(service => service.GetProjectTimes(It.IsAny<DateTime>(), It.IsAny<DateTime>(), null, null,
                    null, null, null))
                .ReturnsAsync(projectTimes);

            // Now we return our project time one day earlier and want it to be reactived. Deleted flag must be removed again
            // We simulate this by returning the project time in this time period
            var importAction2 = new ProjectTimeDatabaseExportAction(loggerFactory, memoryContext,
                "2022-10-01", to: "2022-10-02", userServiceMock.Object, taskServiceMock.Object,
                projectTimeServiceMock.Object);
            await importAction2.Execute();

            // Now the deleted flag must be removed again
            Assert.AreEqual(true, projectTimeDatabase.Billable);
            Assert.AreEqual(true, projectTimeDatabase.Changed);
            Assert.AreEqual(false, projectTimeDatabase.Closed);
            Assert.AreEqual(12, projectTimeDatabase.BreakTime);
            Assert.AreEqual(1000, projectTimeDatabase.Duration);
            Assert.AreEqual("Dachstein", projectTimeDatabase.Description);
            Assert.IsNull(projectTimeDatabase.Deleted);

            var metadata = memoryContext.Metadata.FirstOrDefault();
            Assert.IsNull(metadata);
        }
        
        [Test]
        public async System.Threading.Tasks.Task TestIfTimezoneAreWorkingCorrectly()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

            const string userUuid = "32c8c87e-43ea-11ed-b878-0242ac120002";
            const string taskUuid = "2909B8F0-4996-4D51-A2BA-1EB690AB2102";
            
            var users = new List<User>
            {
                new User 
                { 
                    externalId = "John Carmack", 
                    uuid = userUuid
                }
            };

            var task = new Task
            {
                name = "Customer B",
                uuid = taskUuid
            };
            var tasks = new List<Task> { task };

            var projectTime = new API.ProjectTime();
            
            var startTime = DateTime.Parse("2022-10-02T10:30:00+07:00");
            var endTime = DateTime.Parse("2022-10-02T11:30:00+07:00");

            projectTime.startTime = startTime;
            projectTime.startTimeZone = "+07:00";
            projectTime.endTime = endTime;
            projectTime.endTimeZone = "+07:00";
            projectTime.userUuid = userUuid;
            projectTime.uuid = "83f1bd14-43ea-11ed-b878-0242ac120002";
            projectTime.taskUuid = taskUuid;
            projectTime.billable = true;
            projectTime.changed = true;
            projectTime.breakTime = 12;
            projectTime.duration = 1000;
            projectTime.closed = false;

            var startTimeOffset = projectTime.GetStartTimeOffset();
            var endTimeOffset = projectTime.GetStartTimeOffset();
            
            var projectTimes = new List<API.ProjectTime> { projectTime };
            
            var projectTimeServiceMock = new Mock<IProjectTimeService>(MockBehavior.Strict);
            projectTimeServiceMock
                .Setup(service => service.GetProjectTimes(It.IsAny<DateTime>(), It.IsAny<DateTime>(), null, null, null, null, null))
                .ReturnsAsync(projectTimes);
            
            var userServiceMock = new Mock<IUserService>(MockBehavior.Strict);
            userServiceMock
                .Setup(service => service.GetUsers())
                .ReturnsAsync(users);
            
            var taskServiceMock = new Mock<ITaskService>(MockBehavior.Loose);
            taskServiceMock
                .Setup(service => service.GetTaskHierarchy(It.IsAny<GetTasksRequest>())).ReturnsAsync(new List<Task>());
            taskServiceMock
                .Setup(service => service.FlattenTasks(It.IsAny<IList<Task>>()))
                .Returns(tasks);

            var memoryContext = new DatabaseContext();
            var importAction = new ProjectTimeDatabaseExportAction(loggerFactory, memoryContext, "2022-10-02", to: "2022-10-03", userServiceMock.Object, taskServiceMock.Object, projectTimeServiceMock.Object);
            await importAction.Execute();
            
            var projectTimeDatabase = memoryContext.ProjectTimes.First();
            
            Assert.AreEqual(startTime, projectTimeDatabase.StartTime);
            Assert.AreEqual(endTime, projectTimeDatabase.EndTime);
            Assert.AreEqual(true, projectTimeDatabase.Billable);
            Assert.AreEqual(true, projectTimeDatabase.Changed);
            Assert.AreEqual(false, projectTimeDatabase.Closed);
            Assert.AreEqual(12, projectTimeDatabase.BreakTime);
            Assert.AreEqual(1000, projectTimeDatabase.Duration);
            Assert.IsNull(projectTimeDatabase.Deleted);

            var metadata = memoryContext.Metadata.FirstOrDefault();
            Assert.IsNull(metadata);
        }
        
        
         [Test]
        public async System.Threading.Tasks.Task TestWithTimesInDifferentTimezonesOnTheSameDay()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

            const string userUuid = "32c8c87e-43ea-11ed-b878-0242ac120002";
            const string taskUuid = "2909B8F0-4996-4D51-A2BA-1EB690AB2102";
            
            var users = new List<User>
            {
                new User 
                { 
                    externalId = "John Carmack", 
                    uuid = userUuid
                }
            };

            var task = new Task
            {
                name = "Customer B",
                uuid = taskUuid
            };
            var tasks = new List<Task> { task };

            var projectTime1 = new API.ProjectTime();
            
            var startTimeTimezonePlusTen = DateTime.Parse("2022-10-02T10:30:00+10:00");
            var endTimeTimeZonePlusTen = DateTime.Parse("2022-10-02T11:30:00+10:00");

            projectTime1.startTime = startTimeTimezonePlusTen;
            projectTime1.startTimeZone = "+10:00";
            projectTime1.endTime = endTimeTimeZonePlusTen;
            projectTime1.endTimeZone = "+10:00";
            projectTime1.userUuid = userUuid;
            projectTime1.uuid = "83f1bd14-43ea-11ed-b878-0242ac120002";
            projectTime1.taskUuid = taskUuid;
            projectTime1.billable = true;
            projectTime1.changed = true;
            projectTime1.breakTime = 12;
            projectTime1.duration = 1000;
            projectTime1.closed = false;

         
            var startTimeTimezoneMinusTen = DateTime.Parse("2022-10-02T10:30:00-10:00");
            var endTimeTimezoneMinusTen = DateTime.Parse("2022-10-02T11:30:00-10:00");

            var projectTime2 = new API.ProjectTime();
            
            projectTime2.startTime = startTimeTimezoneMinusTen;
            projectTime2.startTimeZone = "-10:00";
            projectTime2.endTime = endTimeTimezoneMinusTen;
            projectTime2.endTimeZone = "-10:00";
            projectTime2.userUuid = userUuid;
            projectTime2.uuid = "576ff819-e9c1-4104-96d2-8e8ebd6ff6d4";
            projectTime2.taskUuid = taskUuid;
            projectTime2.billable = true;
            projectTime2.changed = true;
            projectTime2.breakTime = 12;
            projectTime2.duration = 1000;
            projectTime2.closed = false;
            
            var projectTimes = new List<API.ProjectTime> { projectTime1, projectTime2 };
            
            var projectTimeServiceMock = new Mock<IProjectTimeService>(MockBehavior.Strict);
            projectTimeServiceMock
                .Setup(service => service.GetProjectTimes(It.IsAny<DateTime>(), It.IsAny<DateTime>(), null, null, null, null, null))
                .ReturnsAsync(projectTimes);
            
            var userServiceMock = new Mock<IUserService>(MockBehavior.Strict);
            userServiceMock
                .Setup(service => service.GetUsers())
                .ReturnsAsync(users);
            
            var taskServiceMock = new Mock<ITaskService>(MockBehavior.Loose);
            taskServiceMock
                .Setup(service => service.GetTaskHierarchy(It.IsAny<GetTasksRequest>())).ReturnsAsync(new List<Task>());
            taskServiceMock
                .Setup(service => service.FlattenTasks(It.IsAny<IList<Task>>()))
                .Returns(tasks);

            var memoryContext = new DatabaseContext();
            var importAction = new ProjectTimeDatabaseExportAction(loggerFactory, memoryContext, "2022-10-02", to: "2022-10-03", userServiceMock.Object, taskServiceMock.Object, projectTimeServiceMock.Object);
            await importAction.Execute();
            
            var projectTimeDatabase1 = memoryContext.ProjectTimes.First();
            
            Assert.AreEqual(startTimeTimezonePlusTen, projectTimeDatabase1.StartTime);
            Assert.AreEqual(endTimeTimeZonePlusTen, projectTimeDatabase1.EndTime);
            Assert.AreEqual(true, projectTimeDatabase1.Billable);
            Assert.AreEqual(true, projectTimeDatabase1.Changed);
            Assert.AreEqual(false, projectTimeDatabase1.Closed);
            Assert.AreEqual(12, projectTimeDatabase1.BreakTime);
            Assert.AreEqual(1000, projectTimeDatabase1.Duration);
            Assert.IsNull(projectTimeDatabase1.Deleted);

            var projectTimeDatabase2 = memoryContext.ProjectTimes.ToList().ElementAt(1);
            
            Assert.AreEqual(startTimeTimezoneMinusTen, projectTimeDatabase2.StartTime);
            Assert.AreEqual(endTimeTimezoneMinusTen, projectTimeDatabase2.EndTime);
            Assert.AreEqual(true, projectTimeDatabase2.Billable);
            Assert.AreEqual(true, projectTimeDatabase2.Changed);
            Assert.AreEqual(false, projectTimeDatabase2.Closed);
            Assert.AreEqual(12, projectTimeDatabase2.BreakTime);
            Assert.AreEqual(1000, projectTimeDatabase2.Duration);
            Assert.IsNull(projectTimeDatabase2.Deleted);

            var metadata = memoryContext.Metadata.FirstOrDefault();
            Assert.IsNull(metadata);
        }
    }
}