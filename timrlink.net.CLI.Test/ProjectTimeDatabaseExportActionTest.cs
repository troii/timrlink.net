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

            var memoryContext = InMemoryContext(Guid.NewGuid().ToString());
            var importAction = new ProjectTimeDatabaseExportAction(loggerFactory, memoryContext, "2022-10-01", null,
                userServiceMock.Object, taskService, projectTimeServiceMock.Object);
            
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

            var memoryContext = InMemoryContext(Guid.NewGuid().ToString());
            var importAction = new ProjectTimeDatabaseExportAction(loggerFactory, memoryContext, null, "2022-10-01",
                userServiceMock.Object, taskService, projectTimeServiceMock.Object);
            
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

            var memoryContext = InMemoryContext(Guid.NewGuid().ToString());
            var importAction = new ProjectTimeDatabaseExportAction(loggerFactory, memoryContext, "2022-10-01",
                "2022-10-01:9999", userServiceMock.Object, taskService, projectTimeServiceMock.Object);
            
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

            var memoryContext = InMemoryContext(Guid.NewGuid().ToString());
            var importAction = new ProjectTimeDatabaseExportAction(loggerFactory, memoryContext, "2022-10-01:9999",
                "2022-10-01", userServiceMock.Object, taskService, projectTimeServiceMock.Object);
            
            Assert.ThrowsAsync<ArgumentException>(() => importAction.Execute());
        }

        [Test]
        public void FromDateAfterToDate()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

            var timrSyncMock = new Mock<TimrSync>(MockBehavior.Strict);
            var projectTimeServiceMock = new Mock<IProjectTimeService>(MockBehavior.Loose);
            var userServiceMock = new Mock<IUserService>(MockBehavior.Strict);

            var taskService =
                new TaskService(loggerFactory.CreateLogger<TaskService>(), loggerFactory, timrSyncMock.Object);

            var memoryContext = InMemoryContext(Guid.NewGuid().ToString());
            var importAction = new ProjectTimeDatabaseExportAction(loggerFactory, memoryContext, "2022-10-02",
                "2022-10-01", userServiceMock.Object, taskService, projectTimeServiceMock.Object);
            
            Assert.ThrowsAsync<ArgumentException>(() => importAction.Execute());
        }

        [Test]
        public async System.Threading.Tasks.Task InsertProjectTimeToDatabase()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

            var options = InMemoryContextOptions(Guid.NewGuid().ToString());

            ProjectTimeDatabaseExportAction importAction;
            {
                var user = new User
                {
                    externalId = "John Carmack",
                    uuid = "32c8c87e-43ea-11ed-b878-0242ac120002"
                };

                var task = new Task
                {
                    name = "Customer B",
                    uuid = "2909B8F0-4996-4D51-A2BA-1EB690AB2102"
                };

                var projectTime = new API.ProjectTime
                {
                    startTime = DateTime.Parse("2022-10-02T10:30:00+02:00"),
                    startTimeZone = "+02:00",
                    endTime = DateTime.Parse("2022-10-02T11:30:00+02:00"),
                    endTimeZone = "+02:00",
                    userUuid = user.uuid,
                    uuid = "83f1bd14-43ea-11ed-b878-0242ac120002",
                    taskUuid = task.uuid,
                    billable = true,
                    changed = true,
                    breakTime = 12,
                    duration = 1000,
                    closed = false
                };

                var projectTimeService = BuildProjectTimeServiceMock(projectTime);
                var userService = BuildUserService(user);
                var taskService = BuildTaskService(task);

                var memoryContext = new DatabaseContext(options);
                importAction = new ProjectTimeDatabaseExportAction(loggerFactory, memoryContext, "2022-10-02",
                    "2022-10-02", userService, taskService, projectTimeService);
            }

            await importAction.Execute();

            {
                var memoryContext = new DatabaseContext(options);
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
        }

        [Test]
        public async System.Threading.Tasks.Task TestIfDeleteFlagIsSet()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

            var options = InMemoryContextOptions(Guid.NewGuid().ToString());
            ProjectTimeDatabaseExportAction importAction;

            {
                var user = new User
                {
                    externalId = "John Carmack",
                    uuid = "32c8c87e-43ea-11ed-b878-0242ac120002"
                };

                var task = new Task
                {
                    name = "Customer B",
                    uuid = "2909B8F0-4996-4D51-A2BA-1EB690AB2102"
                };

                var memoryContext = new DatabaseContext(options);
                memoryContext.ProjectTimes.Add(new ProjectTime
                {
                    UUID = Guid.Parse("83f1bd14-43ea-11ed-b878-0242ac120002"),
                    StartTime = DateTime.Parse("2022-10-02T10:30:00+02:00"),
                    EndTime = DateTime.Parse("2022-10-02T11:30:00+02:00"),
                    User = "John Carmack",
                    Task = "[Customer B]",
                    Billable = true,
                    Changed = true,
                    BreakTime = 12,
                    Duration = 1000,
                    Closed = false
                });
                await memoryContext.SaveChangesAsync();

                var projectTimeService = BuildProjectTimeServiceMock();
                var userService = BuildUserService(user);
                var taskService = BuildTaskService(task);
                importAction = new ProjectTimeDatabaseExportAction(loggerFactory, memoryContext, "2022-10-02",
                    "2022-10-03", userService, taskService, projectTimeService);
            }

            {
                var memoryContext = new DatabaseContext(options);
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

            await importAction.Execute();

            {
                var memoryContext = new DatabaseContext(options);
                var projectTimeDatabase = memoryContext.ProjectTimes.First();
                Assert.AreEqual(true, projectTimeDatabase.Billable);
                Assert.AreEqual(true, projectTimeDatabase.Changed);
                Assert.AreEqual(false, projectTimeDatabase.Closed);
                Assert.AreEqual(12, projectTimeDatabase.BreakTime);
                Assert.AreEqual(1000, projectTimeDatabase.Duration);
                Assert.IsNotNull(projectTimeDatabase.Deleted);

                var metadata = memoryContext.Metadata.FirstOrDefault();
                Assert.IsNull(metadata);
            }
        }

        [Test]
        public async System.Threading.Tasks.Task UpdateMovedTaskInOtherPeriod()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

            var options = InMemoryContextOptions(Guid.NewGuid().ToString());
            ProjectTimeDatabaseExportAction importAction;
            User user;
            Task task;

            { 
                user = new User
                {
                    externalId = "John Carmack",
                    uuid = "32c8c87e-43ea-11ed-b878-0242ac120002"
                };

                task = new Task
                {
                    name = "Customer B",
                    uuid = "2909B8F0-4996-4D51-A2BA-1EB690AB2102"
                };

                var memoryContext = new DatabaseContext(options);

                memoryContext.ProjectTimes.Add(new ProjectTime
                {
                    UUID = Guid.Parse("83f1bd14-43ea-11ed-b878-0242ac120002"),
                    StartTime = DateTime.Parse("2022-10-02T10:30:00+02:00"),
                    EndTime = DateTime.Parse("2022-10-02T11:30:00+02:00"),
                    User = "John Carmack",
                    Task = "[Customer B]",
                    Billable = true,
                    Changed = true,
                    BreakTime = 12,
                    Duration = 1000,
                    Closed = false,
                    Description = "Donnerkogel"
                });

                var projectTimeService = BuildProjectTimeServiceMock();
                var userService = BuildUserService(user);
                var taskService = BuildTaskService(task);

                importAction = new ProjectTimeDatabaseExportAction(loggerFactory, memoryContext, "2022-10-02",
                    "2022-10-03", userService, taskService,
                    projectTimeService);
            }

            await importAction.Execute();

            {
                // First we check if the project time is correctly inserted
                var memoryContext = new DatabaseContext(options);
                var projectTimeDatabase = memoryContext.ProjectTimes.Single();
                Assert.AreEqual(true, projectTimeDatabase.Billable);
                Assert.AreEqual(true, projectTimeDatabase.Changed);
                Assert.AreEqual(false, projectTimeDatabase.Closed);
                Assert.AreEqual(12, projectTimeDatabase.BreakTime);
                Assert.AreEqual(1000, projectTimeDatabase.Duration);
                Assert.AreEqual("Donnerkogel", projectTimeDatabase.Description);
                Assert.IsNull(projectTimeDatabase.Deleted);
            }

            await importAction.Execute();

            {
                // Now the deleted flag must be set
                var memoryContext = new DatabaseContext(options);
                var projectTimeDatabase = memoryContext.ProjectTimes.Single();
                Assert.AreEqual(true, projectTimeDatabase.Billable);
                Assert.AreEqual(true, projectTimeDatabase.Changed);
                Assert.AreEqual(false, projectTimeDatabase.Closed);
                Assert.AreEqual(12, projectTimeDatabase.BreakTime);
                Assert.AreEqual(1000, projectTimeDatabase.Duration);
                Assert.IsNotNull(projectTimeDatabase.Deleted);
            }

            {
                var projectTime = new API.ProjectTime
                {
                    startTime = DateTime.Parse("2022-10-02T10:30:00+07:00"),
                    startTimeZone = "+07:00",
                    endTime = DateTime.Parse("2022-10-02T11:30:00+07:00"),
                    endTimeZone = "+07:00",
                    userUuid = "32c8c87e-43ea-11ed-b878-0242ac120002",
                    uuid = "83f1bd14-43ea-11ed-b878-0242ac120002",
                    taskUuid = "2909B8F0-4996-4D51-A2BA-1EB690AB2102",
                    billable = true,
                    changed = true,
                    breakTime = 12,
                    duration = 1000,
                    closed = false,
                    // Here we update the description and return the project time one day earlier
                    description = "Dachstein"
                };
                
                var projectTimeService = BuildProjectTimeServiceMock(projectTime);
                var userService = BuildUserService(user);
                var taskService = BuildTaskService(task);

                var memoryContext = new DatabaseContext(options);
                importAction = new ProjectTimeDatabaseExportAction(loggerFactory, memoryContext, "2022-10-01",
                    "2022-10-02", userService, taskService,
                    projectTimeService);
            }
            
            await importAction.Execute();

            {
                // Now the deleted flag must be removed again
                var memoryContext = new DatabaseContext(options);
                var projectTimeDatabase = memoryContext.ProjectTimes.Single();
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
        }

        [Test]
        public async System.Threading.Tasks.Task TestIfTimezoneAreWorkingCorrectly()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
            var options = InMemoryContextOptions(Guid.NewGuid().ToString());
            ProjectTimeDatabaseExportAction importAction;
            
            {
                var user = new User
                {
                    externalId = "John Carmack",
                    uuid = "32c8c87e-43ea-11ed-b878-0242ac120002"
                };

                var task = new Task
                {
                    name = "Customer B",
                    uuid = "2909B8F0-4996-4D51-A2BA-1EB690AB2102"
                };
                
                var projectTime = new API.ProjectTime
                {
                    startTime = DateTime.Parse("2022-10-02T10:30:00+07:00"),
                    startTimeZone = "+07:00",
                    endTime = DateTime.Parse("2022-10-02T11:30:00+07:00"),
                    endTimeZone = "+07:00",
                    userUuid = user.uuid,
                    uuid = "83f1bd14-43ea-11ed-b878-0242ac120002",
                    taskUuid = task.uuid,
                    billable = true,
                    changed = true,
                    breakTime = 12,
                    duration = 1000,
                    closed = false
                };
                
                var projectTimeService = BuildProjectTimeServiceMock(projectTime);
                var userService = BuildUserService(user);
                var taskService = BuildTaskService(task);

                var memoryContext = new DatabaseContext(options);
                importAction = new ProjectTimeDatabaseExportAction(loggerFactory, memoryContext, "2022-10-02",
                    "2022-10-03", userService, taskService, projectTimeService);
            }

            await importAction.Execute();

            {
                var memoryContext = new DatabaseContext(options);
                var projectTimeDatabase = memoryContext.ProjectTimes.First();

                var expectedStartTime = new DateTime(2022, 10, 2, 10, 30, 0);
                var expectedEndTime = new DateTime(2022, 10, 2, 11, 30, 0);

                Assert.AreEqual(expectedStartTime, projectTimeDatabase.StartTime);
                Assert.AreEqual(expectedEndTime, projectTimeDatabase.EndTime);
                Assert.AreEqual(true, projectTimeDatabase.Billable);
                Assert.AreEqual(true, projectTimeDatabase.Changed);
                Assert.AreEqual(false, projectTimeDatabase.Closed);
                Assert.AreEqual(12, projectTimeDatabase.BreakTime);
                Assert.AreEqual(1000, projectTimeDatabase.Duration);
                Assert.IsNull(projectTimeDatabase.Deleted);

                var metadata = memoryContext.Metadata.FirstOrDefault();
                Assert.IsNull(metadata);
            }
        }

        [Test]
        public async System.Threading.Tasks.Task TestWithTimesInDifferentTimezonesOnTheSameDay()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
            var options = InMemoryContextOptions(Guid.NewGuid().ToString());
            ProjectTimeDatabaseExportAction importAction;

            {
                var user = new User
                {
                    externalId = "John Carmack",
                    uuid = "32c8c87e-43ea-11ed-b878-0242ac120002"
                };

                var task = new Task
                {
                    name = "Customer B",
                    uuid = "2909B8F0-4996-4D51-A2BA-1EB690AB2102"
                };
                var tasks = new List<Task> {task};

                var projectTime1 = new API.ProjectTime
                {
                    startTime = DateTime.Parse("2022-10-02T10:30:00+10:00"),
                    startTimeZone = "+10:00",
                    endTime = DateTime.Parse("2022-10-02T11:30:00+10:00"),
                    endTimeZone = "+10:00",
                    userUuid = user.uuid,
                    uuid = "83f1bd14-43ea-11ed-b878-0242ac120002",
                    taskUuid = task.uuid,
                    billable = true,
                    changed = true,
                    breakTime = 12,
                    duration = 1000,
                    closed = false
                };

                var projectTime2 = new API.ProjectTime
                {
                    startTime = DateTime.Parse("2022-10-02T10:30:00-10:00"),
                    startTimeZone = "-10:00",
                    endTime = DateTime.Parse("2022-10-02T11:30:00-10:00"),
                    endTimeZone = "-10:00",
                    userUuid = user.uuid,
                    uuid = "576ff819-e9c1-4104-96d2-8e8ebd6ff6d4",
                    taskUuid = task.uuid,
                    billable = true,
                    changed = true,
                    breakTime = 12,
                    duration = 1000,
                    closed = false
                };
                
                var projectTimeService = BuildProjectTimeServiceMock(projectTime1, projectTime2);
                var userService = BuildUserService(user);
                var taskService = BuildTaskService(task);

                var memoryContext = new DatabaseContext(options);
                importAction = new ProjectTimeDatabaseExportAction(loggerFactory, memoryContext, "2022-10-02",
                    "2022-10-03", userService, taskService, projectTimeService);
            }
            
            await importAction.Execute();

            {
                var memoryContext = new DatabaseContext(options);
                
                var expectedStartTimeTimezonePlusTen = new DateTime(2022, 10, 02, 10, 30, 0);
                var expectedEndTimeTimeZonePlusTen = new DateTime(2022, 10, 02, 11, 30, 0);

                var projectTimeDatabase1 = memoryContext.ProjectTimes.First();
                Assert.AreEqual(expectedStartTimeTimezonePlusTen, projectTimeDatabase1.StartTime);
                Assert.AreEqual(expectedEndTimeTimeZonePlusTen, projectTimeDatabase1.EndTime);
                Assert.AreEqual(true, projectTimeDatabase1.Billable);
                Assert.AreEqual(true, projectTimeDatabase1.Changed);
                Assert.AreEqual(false, projectTimeDatabase1.Closed);
                Assert.AreEqual(12, projectTimeDatabase1.BreakTime);
                Assert.AreEqual(1000, projectTimeDatabase1.Duration);
                Assert.IsNull(projectTimeDatabase1.Deleted);

                var projectTimeDatabase2 = memoryContext.ProjectTimes.ToList().ElementAt(1);

                Assert.AreEqual(expectedStartTimeTimezonePlusTen, projectTimeDatabase2.StartTime);
                Assert.AreEqual(expectedEndTimeTimeZonePlusTen, projectTimeDatabase2.EndTime);
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

        [Test]
        public async System.Threading.Tasks.Task TestWithTimesInDifferentTimezonesDifferentDay()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
            var options = InMemoryContextOptions(Guid.NewGuid().ToString());
            ProjectTimeDatabaseExportAction importAction;
            
            {
                var user = new User
                {
                    externalId = "John Carmack",
                    uuid = "32c8c87e-43ea-11ed-b878-0242ac120002"
                };

                var task = new Task
                {
                    name = "Customer B",
                    uuid = "2909B8F0-4996-4D51-A2BA-1EB690AB2102"
                };
                
                var projectTime = new API.ProjectTime
                {
                    startTime = DateTime.Parse("2022-10-03T23:00:00+10:00"),
                    startTimeZone = "+10:00",
                    endTime = DateTime.Parse("2022-10-03T23:30:00+10:00"),
                    endTimeZone = "+10:00",
                    userUuid = user.uuid,
                    uuid = "83f1bd14-43ea-11ed-b878-0242ac120002",
                    taskUuid = task.uuid,
                    billable = true,
                    changed = true,
                    breakTime = 12,
                    duration = 1000,
                    closed = false
                };

                var projectTimeService = BuildProjectTimeServiceMock(projectTime);
                var userService = BuildUserService(user);
                var taskService = BuildTaskService(task);

                var memoryContext = new DatabaseContext(options);
                importAction = new ProjectTimeDatabaseExportAction(loggerFactory, memoryContext, "2022-10-02",
                    "2022-10-03", userService, taskService, projectTimeService);
            }

            await importAction.Execute();

            {
                var memoryContext = new DatabaseContext(options);
                var projectTimeDatabase1 = memoryContext.ProjectTimes.First();

                var expectedStartTimeTimezonePlusTen = new DateTime(2022, 10, 03, 23, 0, 0);
                var expectedEndTimeTimeZonePlusTen = new DateTime(2022, 10, 03, 23, 30, 0);

                Assert.AreEqual(expectedStartTimeTimezonePlusTen, projectTimeDatabase1.StartTime);
                Assert.AreEqual(expectedEndTimeTimeZonePlusTen, projectTimeDatabase1.EndTime);
                Assert.AreEqual(true, projectTimeDatabase1.Billable);
                Assert.AreEqual(true, projectTimeDatabase1.Changed);
                Assert.AreEqual(false, projectTimeDatabase1.Closed);
                Assert.AreEqual(12, projectTimeDatabase1.BreakTime);
                Assert.AreEqual(1000, projectTimeDatabase1.Duration);
                Assert.IsNull(projectTimeDatabase1.Deleted);

                var metadata = memoryContext.Metadata.FirstOrDefault();
                Assert.IsNull(metadata);
            }
        }

        [Test]
        public async System.Threading.Tasks.Task TestWithTimesIncludedInToDate()
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

            var startTimeTimezonePlusTen = DateTime.Parse("2022-10-03T23:00:00+10:00");
            var endTimeTimeZonePlusTen = DateTime.Parse("2022-10-03T23:30:00+10:00");

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

            var projectTimes = new List<API.ProjectTime> { projectTime1 };

            var projectTimeServiceMock = new Mock<IProjectTimeService>(MockBehavior.Strict);
            projectTimeServiceMock
                .Setup(service =>
                    service.GetProjectTimes(It.IsAny<DateTime>(), It.IsAny<DateTime>(), null, null, null, null, null))
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

            var memoryContext = new DatabaseContext(InMemoryContextOptions(Guid.NewGuid().ToString()));
            var importAction = new ProjectTimeDatabaseExportAction(loggerFactory, memoryContext, "2022-10-02",
                "2022-10-03", userServiceMock.Object, taskServiceMock.Object, projectTimeServiceMock.Object);
            await importAction.Execute();

            projectTimeServiceMock
                .Setup(service =>
                    service.GetProjectTimes(It.IsAny<DateTime>(), It.IsAny<DateTime>(), null, null, null, null, null))
                .ReturnsAsync(new List<API.ProjectTime>());

            await importAction.Execute();

            var projectTimeDatabase1 = memoryContext.ProjectTimes.First();

            var expectedStartTimeTimezonePlusTen = new DateTime(2022, 10, 03, 23, 00, 0);
            var expectedEndTimeTimeZonePlusTen = new DateTime(2022, 10, 03, 23, 30, 0);

            Assert.AreEqual(expectedStartTimeTimezonePlusTen, projectTimeDatabase1.StartTime);
            Assert.AreEqual(expectedEndTimeTimeZonePlusTen, projectTimeDatabase1.EndTime);
            Assert.AreEqual(true, projectTimeDatabase1.Billable);
            Assert.AreEqual(true, projectTimeDatabase1.Changed);
            Assert.AreEqual(false, projectTimeDatabase1.Closed);
            Assert.AreEqual(12, projectTimeDatabase1.BreakTime);
            Assert.AreEqual(1000, projectTimeDatabase1.Duration);
            Assert.IsNotNull(projectTimeDatabase1.Deleted);

            var metadata = memoryContext.Metadata.FirstOrDefault();
            Assert.IsNull(metadata);
        }

        [Test]
        public async System.Threading.Tasks.Task TestProjectTimeFromOtherTimezoneThatIsOnNextDayInOurTimezone()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
            var options = InMemoryContextOptions(Guid.NewGuid().ToString());
            ProjectTimeDatabaseExportAction importAction;
            API.ProjectTime projectTime;
            
            {
                var user = new User
                {
                    externalId = "John Carmack",
                    uuid = "32c8c87e-43ea-11ed-b878-0242ac120002"
                };

                var task = new Task
                {
                    name = "Customer B",
                    uuid = "2909B8F0-4996-4D51-A2BA-1EB690AB2102"
                };

                // This time is basically in timezone +10:00 and has same date as we have, but in our timezone this is 
                // already on 25. (next day)
                projectTime = new API.ProjectTime
                {
                    startTime = DateTime.Parse("2022-10-24T22:00:00-10:00"),
                    startTimeZone = "-10:00",
                    endTime = DateTime.Parse("2022-10-24T23:00:00-10:00"),
                    endTimeZone = "-10:00",
                    userUuid = user.uuid,
                    uuid = "83f1bd14-43ea-11ed-b878-0242ac120002",
                    taskUuid = task.uuid,
                    billable = true,
                    changed = true,
                    breakTime = 12,
                    duration = 1000,
                    closed = false,
                    description = "Blau"
                };
                
                var projectTimeService = BuildProjectTimeServiceMock(projectTime);
                var userService = BuildUserService(user);
                var taskService = BuildTaskService(task);

                var memoryContext = new DatabaseContext(options);
                importAction = new ProjectTimeDatabaseExportAction(loggerFactory, memoryContext, "2022-10-24",
                    "2022-10-24", userService, taskService, projectTimeService);
            }

            // Initial import
            await importAction.Execute();

            {
                var memoryContext = new DatabaseContext(options);
                var projectTimeDatabase = memoryContext.ProjectTimes.First();
                Assert.AreEqual("Blau", projectTimeDatabase.Description);
            }
            
            // Now change description of project time
            projectTime.description = "Rot";
            
            // Do it again just to verify that the entry is not going to be deleted.
            await importAction.Execute();
                
            {
                var memoryContext = new DatabaseContext(options);
                var projectTimeDatabase = memoryContext.ProjectTimes.First();
                var expectedStartTimeTimezonePlusTen = new DateTime(2022, 10, 24, 22, 00, 0);
                var expectedEndTimeTimeZonePlusTen = new DateTime(2022, 10, 24, 23, 00, 0);

                Assert.AreEqual(expectedStartTimeTimezonePlusTen, projectTimeDatabase.StartTime);
                Assert.AreEqual(expectedEndTimeTimeZonePlusTen, projectTimeDatabase.EndTime);
                Assert.AreEqual(true, projectTimeDatabase.Billable);
                Assert.AreEqual(true, projectTimeDatabase.Changed);
                Assert.AreEqual(false, projectTimeDatabase.Closed);
                Assert.AreEqual(12, projectTimeDatabase.BreakTime);
                Assert.AreEqual(1000, projectTimeDatabase.Duration);
                Assert.AreEqual("Rot", projectTimeDatabase.Description);
                Assert.IsNull(projectTimeDatabase.Deleted);

                var metadata = memoryContext.Metadata.FirstOrDefault();
                Assert.IsNull(metadata);
            }
        }
        
        private static DatabaseContext InMemoryContext(string databaseName)
        {
            var options = InMemoryContextOptions(databaseName);
            var memoryContext = new DatabaseContext(options);
            return memoryContext;
        }

        private static DbContextOptions InMemoryContextOptions(string databaseName)
        {
            var options = new DbContextOptionsBuilder()
                .UseInMemoryDatabase(databaseName)
                .Options;
            return options;
        }

        private IProjectTimeService BuildProjectTimeServiceMock(params API.ProjectTime[] projectTimes)
        {
            var projectTimeServiceMock = new Mock<IProjectTimeService>(MockBehavior.Strict);
            projectTimeServiceMock
                .Setup(service =>
                    service.GetProjectTimes(It.IsAny<DateTime>(), It.IsAny<DateTime>(), null, null, null, null, null))
                .ReturnsAsync(projectTimes.ToList());
            return projectTimeServiceMock.Object;
        }

        private ITaskService BuildTaskService(params Task[] tasks)
        {
            var taskServiceMock = new Mock<ITaskService>(MockBehavior.Loose);
            taskServiceMock
                .Setup(service => service.GetTaskHierarchy(It.IsAny<GetTasksRequest>()))
                .ReturnsAsync(tasks.ToList());

            taskServiceMock
                .Setup(service => service.FlattenTasks(It.IsAny<IList<Task>>()))
                .Returns((IList<Task> tasks) => TaskService.FlattenTasks(tasks));

            return taskServiceMock.Object;
        }

        private IUserService BuildUserService(params User[] users)
        {
            var userServiceMock = new Mock<IUserService>(MockBehavior.Strict);
            userServiceMock
                .Setup(service => service.GetUsers())
                .ReturnsAsync(users.ToList);
            return userServiceMock.Object;
        }
    }
}
