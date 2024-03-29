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

            var projectTimeServiceMock = new Mock<IProjectTimeService>(MockBehavior.Strict);
            var taskServiceMock = new Mock<ITaskService>(MockBehavior.Loose); 
            var userServiceMock = new Mock<IUserService>(MockBehavior.Strict);
            
            // Setup
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
                    closed = false,
                    lastModifiedTime = DateTime.Parse("2022-10-02T11:30:00+02:00"),
                    lastModifiedTimeZone = "+02:00"
                };

                BuildProjectTimeServiceMock(projectTimeServiceMock, projectTime);
                BuildUserService(userServiceMock, user); 
                BuildTaskService(taskServiceMock, task);
            }

            // Test project time database export action
            {
                var memoryContext = new DatabaseContext(options);
                var importAction = new ProjectTimeDatabaseExportAction(loggerFactory, memoryContext, "2022-10-02",
                    "2022-10-02", userServiceMock.Object, taskServiceMock.Object, projectTimeServiceMock.Object);
                await importAction.Execute();
            }

            // Verification
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

            var projectTimeServiceMock = new Mock<IProjectTimeService>(MockBehavior.Strict);
            var taskServiceMock = new Mock<ITaskService>(MockBehavior.Loose); 
            var userServiceMock = new Mock<IUserService>(MockBehavior.Strict);

            // Setup
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

                BuildProjectTimeServiceMock(projectTimeServiceMock);
                BuildUserService(userServiceMock, user); 
                BuildTaskService(taskServiceMock, task);
            }

            // Test project time database export action
            {
                var memoryContext = new DatabaseContext(options);
                var importAction = new ProjectTimeDatabaseExportAction(loggerFactory, memoryContext, "2022-10-02",
                    "2022-10-03", userServiceMock.Object, taskServiceMock.Object, projectTimeServiceMock.Object);
                await importAction.Execute();
            }

            // Verification
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

            var projectTimeServiceMock = new Mock<IProjectTimeService>(MockBehavior.Strict);
            var taskServiceMock = new Mock<ITaskService>(MockBehavior.Loose); 
            var userServiceMock = new Mock<IUserService>(MockBehavior.Strict);

            // Setup
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
                    Closed = false,
                    Description = "Donnerkogel"
                });
                await memoryContext.SaveChangesAsync();

                BuildProjectTimeServiceMock(projectTimeServiceMock);
                BuildUserService(userServiceMock, user); 
                BuildTaskService(taskServiceMock, task);
            }

            // Test project time database export action
            {
                var memoryContext = new DatabaseContext(options);
                importAction = new ProjectTimeDatabaseExportAction(loggerFactory, memoryContext, "2022-10-02",
                    "2022-10-03", userServiceMock.Object, taskServiceMock.Object,
                    projectTimeServiceMock.Object);
                await importAction.Execute();
            }

            // Verification
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

            // 2nd Setup
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
                    description = "Dachstein",
                    lastModifiedTime = DateTime.Parse("2022-10-02T11:39:00+07:00"),
                    lastModifiedTimeZone = "+07:00",
                };
                
                BuildProjectTimeServiceMock(projectTimeServiceMock, projectTime);
                BuildUserService(userServiceMock);
                BuildTaskService(taskServiceMock);
            }

            // Test project time database export action - 2nd import
            {
                var memoryContext = new DatabaseContext(options);
                importAction = new ProjectTimeDatabaseExportAction(loggerFactory, memoryContext, "2022-10-01",
                    "2022-10-02", userServiceMock.Object, taskServiceMock.Object,
                    projectTimeServiceMock.Object);
                await importAction.Execute();
            }

            // Verification
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

            var projectTimeServiceMock = new Mock<IProjectTimeService>(MockBehavior.Strict);
            var taskServiceMock = new Mock<ITaskService>(MockBehavior.Loose); 
            var userServiceMock = new Mock<IUserService>(MockBehavior.Strict);
            
            // Setup
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
                    closed = false,
                    lastModifiedTime = DateTime.Parse("2022-10-02T11:30:00+07:00"),
                    lastModifiedTimeZone = "+07:00"
                };
                
                BuildProjectTimeServiceMock(projectTimeServiceMock, projectTime);
                BuildUserService(userServiceMock, user); 
                BuildTaskService(taskServiceMock, task);
            }

            // Test project time database export action
            {
                var memoryContext = new DatabaseContext(options);
                var importAction = new ProjectTimeDatabaseExportAction(loggerFactory, memoryContext, "2022-10-02",
                    "2022-10-03", userServiceMock.Object, taskServiceMock.Object, projectTimeServiceMock.Object);
                await importAction.Execute();
            }
            
            // Verification
            {
                var memoryContext = new DatabaseContext(options);
                var projectTimeDatabase = memoryContext.ProjectTimes.First();

                var expectedStartTime = DateTimeOffset.Parse("2022-10-02T10:30:00+07:00");
                var expectedEndTime = DateTimeOffset.Parse("2022-10-02T11:30:00+07:00");

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

            var projectTimeServiceMock = new Mock<IProjectTimeService>(MockBehavior.Strict);
            var taskServiceMock = new Mock<ITaskService>(MockBehavior.Loose); 
            var userServiceMock = new Mock<IUserService>(MockBehavior.Strict);
            
            // Setup
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
                    closed = false,
                    lastModifiedTime = DateTime.Parse("2022-10-02T11:30:00+10:00"),
                    lastModifiedTimeZone = "+10:00",
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
                    closed = false,
                    lastModifiedTime = DateTime.Parse("2022-10-02T11:32:00-10:00"),
                    lastModifiedTimeZone = "-10:00",
                };
                
                BuildProjectTimeServiceMock(projectTimeServiceMock, projectTime1, projectTime2);
                BuildUserService(userServiceMock, user); 
                BuildTaskService(taskServiceMock, task);
            }
            
            // Test project time database export action
            {
                var memoryContext = new DatabaseContext(options);
                var importAction = new ProjectTimeDatabaseExportAction(loggerFactory, memoryContext, "2022-10-02",
                    "2022-10-03", userServiceMock.Object, taskServiceMock.Object, projectTimeServiceMock.Object);
                await importAction.Execute();
            }

            // Verification
            {
                var memoryContext = new DatabaseContext(options);
                
                var expectedStartTime = DateTimeOffset.Parse("2022-10-02T10:30:00+10:00");
                var expectedEndTime = DateTimeOffset.Parse("2022-10-02T11:30:00+10:00");

                var projectTimeDatabase1 = memoryContext.ProjectTimes.First();
                Assert.AreEqual(expectedStartTime, projectTimeDatabase1.StartTime);
                Assert.AreEqual(expectedEndTime, projectTimeDatabase1.EndTime);
                Assert.AreEqual(true, projectTimeDatabase1.Billable);
                Assert.AreEqual(true, projectTimeDatabase1.Changed);
                Assert.AreEqual(false, projectTimeDatabase1.Closed);
                Assert.AreEqual(12, projectTimeDatabase1.BreakTime);
                Assert.AreEqual(1000, projectTimeDatabase1.Duration);
                Assert.IsNull(projectTimeDatabase1.Deleted);
                
                expectedStartTime = DateTimeOffset.Parse("2022-10-02T10:30:00-10:00");
                expectedEndTime = DateTimeOffset.Parse("2022-10-02T11:30:00-10:00");
                
                var projectTimeDatabase2 = memoryContext.ProjectTimes.ToList().ElementAt(1);

                Assert.AreEqual(expectedStartTime, projectTimeDatabase2.StartTime);
                Assert.AreEqual(expectedEndTime, projectTimeDatabase2.EndTime);
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

            var projectTimeServiceMock = new Mock<IProjectTimeService>(MockBehavior.Strict);
            var taskServiceMock = new Mock<ITaskService>(MockBehavior.Loose); 
            var userServiceMock = new Mock<IUserService>(MockBehavior.Strict);
            
            // Setup
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
                    closed = false,
                    lastModifiedTime = DateTime.Parse("2022-10-03T23:32:00+10:00"),
                    lastModifiedTimeZone = "+02:00",
                };

                BuildProjectTimeServiceMock(projectTimeServiceMock, projectTime);
                BuildUserService(userServiceMock, user); 
                BuildTaskService(taskServiceMock, task);
            }
            
            // Test project time database export action
            {
                var memoryContext = new DatabaseContext(options);
                var importAction = new ProjectTimeDatabaseExportAction(loggerFactory, memoryContext, "2022-10-02",
                    "2022-10-03", userServiceMock.Object, taskServiceMock.Object, projectTimeServiceMock.Object);
                await importAction.Execute();
            }

            // Verification
            {
                var memoryContext = new DatabaseContext(options);
                var projectTimeDatabase1 = memoryContext.ProjectTimes.First();
                
                var expectedStartTime = DateTimeOffset.Parse("2022-10-03T23:00:00+10:00");
                var expectedEndTime = DateTimeOffset.Parse("2022-10-03T23:30:00+10:00");

                Assert.AreEqual(expectedStartTime, projectTimeDatabase1.StartTime);
                Assert.AreEqual(expectedEndTime, projectTimeDatabase1.EndTime);
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
        public async System.Threading.Tasks.Task TestProjectTimeShouldNotGetDeletedIfRangeGetsReducedFrom2To1Day()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
            var options = InMemoryContextOptions(Guid.NewGuid().ToString());
            ProjectTimeDatabaseExportAction importAction;

            var projectTimeServiceMock = new Mock<IProjectTimeService>(MockBehavior.Strict);
            var taskServiceMock = new Mock<ITaskService>(MockBehavior.Loose); 
            var userServiceMock = new Mock<IUserService>(MockBehavior.Strict);
            
            // Setup
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
                    startTime = DateTime.Parse("2022-12-22T00:00:00+02:00"),
                    startTimeZone = "+02:00",
                    endTime = DateTime.Parse("2022-12-22T12:00:00+02:00"),
                    endTimeZone = "+02:00",
                    userUuid = user.uuid,
                    uuid = "83f1bd14-43ea-11ed-b878-0242ac120002",
                    taskUuid = task.uuid,
                    billable = true,
                    changed = true,
                    breakTime = 12,
                    duration = 1000,
                    closed = false,
                    description = "Blau",
                    lastModifiedTime = DateTime.Parse("2022-12-22T12:18:00+02:00"),
                    lastModifiedTimeZone = "+02:00",
                };
                
                BuildProjectTimeServiceMock(projectTimeServiceMock, projectTime);
                BuildUserService(userServiceMock, user); 
                BuildTaskService(taskServiceMock, task);
            }
            
            var expectedStartTimeTimezonePlusTen = DateTimeOffset.Parse("2022-12-22T00:00:00+02:00");
            var expectedEndTimeTimeZonePlusTen = DateTimeOffset.Parse("2022-12-22T12:00:00+02:00");
            
            // Test project time database export action
            {
                var memoryContext = new DatabaseContext(options);
                importAction = new ProjectTimeDatabaseExportAction(loggerFactory, memoryContext, "2022-12-21",
                    "2022-12-22", userServiceMock.Object, taskServiceMock.Object, projectTimeServiceMock.Object);
                // Initial import
                await importAction.Execute();
            }

            // Verification
            {
                var memoryContext = new DatabaseContext(options);
                var projectTimeDatabase = memoryContext.ProjectTimes.First();
                Assert.AreEqual(expectedStartTimeTimezonePlusTen, projectTimeDatabase.StartTime);
                Assert.AreEqual(expectedEndTimeTimeZonePlusTen, projectTimeDatabase.EndTime);
                Assert.AreEqual(true, projectTimeDatabase.Billable);
                Assert.AreEqual(true, projectTimeDatabase.Changed);
                Assert.AreEqual(false, projectTimeDatabase.Closed);
                Assert.AreEqual(12, projectTimeDatabase.BreakTime);
                Assert.AreEqual(1000, projectTimeDatabase.Duration);
                Assert.AreEqual("Blau", projectTimeDatabase.Description);
                Assert.IsNull(projectTimeDatabase.Deleted);
            }

            // Test project time database export action - import 2nd time
            {
                var memoryContext = new DatabaseContext(options);
                importAction = new ProjectTimeDatabaseExportAction(loggerFactory, memoryContext, "2022-12-21",
                    "2022-12-21", userServiceMock.Object, taskServiceMock.Object, projectTimeServiceMock.Object);
                // Do it again just to verify that the entry is not going to be deleted.
                await importAction.Execute();
            }

            // Verification
            {
                var memoryContext = new DatabaseContext(options);
                var projectTimeDatabase = memoryContext.ProjectTimes.First();

                Assert.AreEqual(expectedStartTimeTimezonePlusTen, projectTimeDatabase.StartTime);
                Assert.AreEqual(expectedEndTimeTimeZonePlusTen, projectTimeDatabase.EndTime);
                Assert.AreEqual(true, projectTimeDatabase.Billable);
                Assert.AreEqual(true, projectTimeDatabase.Changed);
                Assert.AreEqual(false, projectTimeDatabase.Closed);
                Assert.AreEqual(12, projectTimeDatabase.BreakTime);
                Assert.AreEqual(1000, projectTimeDatabase.Duration);
                Assert.AreEqual("Blau", projectTimeDatabase.Description);
                Assert.IsNull(projectTimeDatabase.Deleted);

                var metadata = memoryContext.Metadata.FirstOrDefault();
                Assert.IsNull(metadata);
            }
        }
        
        
        [Test]
        public async System.Threading.Tasks.Task TestProjectTimeShouldGetDeletedIfNotDeliveredAgain()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
            var options = InMemoryContextOptions(Guid.NewGuid().ToString());
            ProjectTimeDatabaseExportAction importAction;

            var projectTimeServiceMock = new Mock<IProjectTimeService>(MockBehavior.Strict);
            var taskServiceMock = new Mock<ITaskService>(MockBehavior.Loose); 
            var userServiceMock = new Mock<IUserService>(MockBehavior.Strict);
            
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
                    startTime = DateTime.Parse("2022-12-22T00:00:00+02:00"),
                    startTimeZone = "+02:00",
                    endTime = DateTime.Parse("2022-12-22T12:00:00+02:00"),
                    endTimeZone = "+02:00",
                    userUuid = user.uuid,
                    uuid = "83f1bd14-43ea-11ed-b878-0242ac120002",
                    taskUuid = task.uuid,
                    billable = true,
                    changed = true,
                    breakTime = 12,
                    duration = 1000,
                    closed = false,
                    description = "Blau",
                    lastModifiedTime = DateTime.Parse("2022-12-22T12:12:00+02:00"),
                    lastModifiedTimeZone = "+02:00",
                };
                
                BuildProjectTimeServiceMock(projectTimeServiceMock, projectTime);
                BuildUserService(userServiceMock, user); 
                BuildTaskService(taskServiceMock, task);
            }

            // Test project time database export action
            {
                var memoryContext = new DatabaseContext(options);
                importAction = new ProjectTimeDatabaseExportAction(loggerFactory, memoryContext, "2022-12-21",
                    "2022-12-22", userServiceMock.Object, taskServiceMock.Object, projectTimeServiceMock.Object);
                // Initial import
                await importAction.Execute();
            }

            // Verification
            {
                var memoryContext = new DatabaseContext(options);
                var projectTimeDatabase = memoryContext.ProjectTimes.First();
                Assert.AreEqual("Blau", projectTimeDatabase.Description);
            }

            // Setup
            {
                // Do not deliver project time on second run
                BuildProjectTimeServiceMock(projectTimeServiceMock);
            }
            
            // Test project time database export action - import 2nd time
            {
                var memoryContext = new DatabaseContext(options);
                importAction = new ProjectTimeDatabaseExportAction(loggerFactory, memoryContext, "2022-12-21",
                    "2022-12-22", userServiceMock.Object, taskServiceMock.Object, projectTimeServiceMock.Object);
                // Do it again just to verify that the entry is getting deleted
                await importAction.Execute();
            }

            // Verification            
            {
                var memoryContext = new DatabaseContext(options);
                var projectTimeDatabase = memoryContext.ProjectTimes.First();
                
                var expectedStartTimeTimezonePlusTen = DateTimeOffset.Parse("2022-12-22T00:00:00+02:00");
                var expectedEndTimeTimeZonePlusTen = DateTimeOffset.Parse("2022-12-22T12:00:00+02:00");

                Assert.AreEqual(expectedStartTimeTimezonePlusTen, projectTimeDatabase.StartTime);
                Assert.AreEqual(expectedEndTimeTimeZonePlusTen, projectTimeDatabase.EndTime);
                Assert.AreEqual(true, projectTimeDatabase.Billable);
                Assert.AreEqual(true, projectTimeDatabase.Changed);
                Assert.AreEqual(false, projectTimeDatabase.Closed);
                Assert.AreEqual(12, projectTimeDatabase.BreakTime);
                Assert.AreEqual(1000, projectTimeDatabase.Duration);
                Assert.AreEqual("Blau", projectTimeDatabase.Description);
                Assert.IsNotNull(projectTimeDatabase.Deleted);

                var metadata = memoryContext.Metadata.FirstOrDefault();
                Assert.IsNull(metadata);
            }
        }
        
        [Test]
        public async System.Threading.Tasks.Task TestProjectTimeFromOtherTimezoneThatIsOnNextDayInOurTimezone()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
            var options = InMemoryContextOptions(Guid.NewGuid().ToString());
            ProjectTimeDatabaseExportAction importAction;
            API.ProjectTime projectTime;
            
            var projectTimeServiceMock = new Mock<IProjectTimeService>(MockBehavior.Strict);
            var taskServiceMock = new Mock<ITaskService>(MockBehavior.Loose); 
            var userServiceMock = new Mock<IUserService>(MockBehavior.Strict);
            
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
                    description = "Blau",
                    lastModifiedTime = DateTime.Parse("2022-10-24T23:12:00-10:00"),
                    lastModifiedTimeZone = "-10:00",
                };
                
                BuildProjectTimeServiceMock(projectTimeServiceMock, projectTime);
                BuildUserService(userServiceMock, user); 
                BuildTaskService(taskServiceMock, task);
            }

            // Test project time database export action
            {
                var memoryContext = new DatabaseContext(options);
                importAction = new ProjectTimeDatabaseExportAction(loggerFactory, memoryContext, "2022-10-24",
                    "2022-10-24", userServiceMock.Object, taskServiceMock.Object, projectTimeServiceMock.Object);
                // Initial import
                await importAction.Execute();
            }

            // Verification
            {
                var memoryContext = new DatabaseContext(options);
                var projectTimeDatabase = memoryContext.ProjectTimes.First();
                Assert.AreEqual("Blau", projectTimeDatabase.Description);
            }

            // Setup
            {
                // Now change description of project time
                projectTime.description = "Rot";
            }

            // Test project time database export action - import 2nd time
            {
                // Do it again just to verify that the entry is not going to be deleted.
                await importAction.Execute();
            }

            // Verification
            {
                var memoryContext = new DatabaseContext(options);
                var projectTimeDatabase = memoryContext.ProjectTimes.First();
                
                var expectedStartTimeTimezonePlusTen = DateTimeOffset.Parse("2022-10-24T22:00:00-10:00");
                var expectedEndTimeTimeZonePlusTen = DateTimeOffset.Parse("2022-10-24T23:00:00-10:00");

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
        
        
        [Test]
        public async System.Threading.Tasks.Task TestNewColumnsOfProjectTime()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
            var options = InMemoryContextOptions(Guid.NewGuid().ToString());

            var projectTimeServiceMock = new Mock<IProjectTimeService>(MockBehavior.Strict);
            var taskServiceMock = new Mock<ITaskService>(MockBehavior.Loose); 
            var userServiceMock = new Mock<IUserService>(MockBehavior.Strict);
            
            // Setup
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
                    description = "Blue",
                    externalUserId = "99C12",
                    externalTaskId = "B7A",
                    lastModifiedTime = DateTime.Parse("2022-10-24T22:00:00-10:00"),
                    lastModifiedTimeZone = "-10:00"
                };
                
                BuildProjectTimeServiceMock(projectTimeServiceMock, projectTime);
                BuildUserService(userServiceMock, user); 
                BuildTaskService(taskServiceMock, task);
            }
            
            // Test project time database export action - import 2nd time
            {
                var memoryContext = new DatabaseContext(options);
                var importAction = new ProjectTimeDatabaseExportAction(loggerFactory, memoryContext, "2022-10-24",
                    "2022-10-24", userServiceMock.Object, taskServiceMock.Object, projectTimeServiceMock.Object);
                // Initial import
                await importAction.Execute();
            }

            // Verify
            {
                var memoryContext = new DatabaseContext(options);
                var projectTimeDatabase = memoryContext.ProjectTimes.First();
                var expectedLastModifiedTime = DateTimeOffset.Parse("10/24/2022 10:00 PM -10:00");
                var expectedStartTime = DateTimeOffset.Parse("10/24/2022 10:00 PM -10:00");
                var expectedEndTime = DateTimeOffset.Parse("10/24/2022 11:00 PM -10:00");

                Assert.AreEqual(expectedStartTime, projectTimeDatabase.StartTime);
                Assert.AreEqual(expectedEndTime, projectTimeDatabase.EndTime);
                Assert.AreEqual(true, projectTimeDatabase.Billable);
                Assert.AreEqual(true, projectTimeDatabase.Changed);
                Assert.AreEqual(false, projectTimeDatabase.Closed);
                Assert.AreEqual(12, projectTimeDatabase.BreakTime);
                Assert.AreEqual(1000, projectTimeDatabase.Duration);
                Assert.AreEqual("Blue", projectTimeDatabase.Description);
                Assert.AreEqual("99C12", projectTimeDatabase.UserExternalId);
                Assert.AreEqual("B7A", projectTimeDatabase.TaskExternalId);
                Assert.AreEqual(expectedStartTime, projectTimeDatabase.StartTime);
                Assert.AreEqual(expectedEndTime, projectTimeDatabase.EndTime);
                Assert.AreEqual(Guid.Parse("32c8c87e-43ea-11ed-b878-0242ac120002"), projectTimeDatabase.UserUUID);
                Assert.AreEqual(Guid.Parse("2909B8F0-4996-4D51-A2BA-1EB690AB2102"), projectTimeDatabase.TaskUUID);
                Assert.IsNull(projectTimeDatabase.UserEmployeeNr);
                Assert.AreEqual(expectedLastModifiedTime, projectTimeDatabase.LastModifiedTime);
                
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

        private void BuildProjectTimeServiceMock(Mock<IProjectTimeService> projectTimeServiceMock, params API.ProjectTime[] projectTimes)
        {
            projectTimeServiceMock
                .Setup(service =>
                    service.GetProjectTimes(It.IsAny<DateTime>(), It.IsAny<DateTime>(), null, null, null, null, null))
                .ReturnsAsync(projectTimes.ToList());
        }

        private void BuildTaskService(Mock<ITaskService> taskServiceMock, params Task[] tasks)
        {
            taskServiceMock
                .Setup(service => service.GetTaskHierarchy(It.IsAny<GetTasksRequest>()))
                .ReturnsAsync(tasks.ToList());

            taskServiceMock
                .Setup(service => service.FlattenTasks(It.IsAny<IList<Task>>()))
                .Returns((IList<Task> t) => TaskService.FlattenTasks(t));
        }

        private void BuildUserService(Mock<IUserService> userServiceMock, params User[] users)
        {
            userServiceMock
                .Setup(service => service.GetUsers())
                .ReturnsAsync(users.ToList);
        }
    }
}
