using System;
using System.Collections.Generic;
using System.Linq;
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
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

            var users = new List<User>
            {
                new User { externalId = "John Dow" },
                new User { externalId = "John Cena" }
            };

            var tasks = new List<Task>();

            var timrSyncMock = new Mock<TimrSync>(MockBehavior.Strict);
            timrSyncMock
                .Setup(timrSync => timrSync.GetTasksAsync(It.IsAny<GetTasksRequest1>()))
                .ReturnsAsync(new GetTasksResponse(new Task[0]));
            timrSyncMock
                .Setup(timrSync => timrSync.AddTaskAsync(It.IsAny<AddTaskRequest>()))
                .Callback((AddTaskRequest addTaskRequest) => tasks.Add(addTaskRequest.AddTaskRequest1))
                .ReturnsAsync(new AddTaskResponse());

            var projectTimeServiceMock = new Mock<IProjectTimeService>(MockBehavior.Loose);
            var userServiceMock = new Mock<IUserService>(MockBehavior.Strict);
            userServiceMock
                .Setup(service => service.GetUsers())
                .ReturnsAsync(users);

            var taskService = new TaskService(loggerFactory.CreateLogger<TaskService>(), loggerFactory, timrSyncMock.Object);

            var importAction = new ProjectTimeCSVImportAction(loggerFactory.CreateLogger<ProjectTimeCSVImportAction>(), taskService, userServiceMock.Object, projectTimeServiceMock.Object);
            await importAction.Execute("data/projecttime.csv");

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

        [Test(Description = "Tests partial creation of required Tasks including logic in TaskService")]
        public async System.Threading.Tasks.Task PartialTaskCreation()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

            var user = new User { externalId = "John Dow" };
            var tasks = new List<Task>
            {
                new Task
                {
                    name = "Customer A",
                    externalId = "Customer A",
                    uuid = "F38761C7-3974-4C01-9FD4-DA5027C346F6",
                    subtasks = new[]
                    {
                        new Task
                        {
                            name = "Project1",
                            externalId = "Customer A|Project1",
                            parentExternalId = "Customer A",
                            uuid = "4E5B79E0-7E0F-4FC1-9837-44C910256E04",
                            subtasks = new[]
                            {
                                new Task
                                {
                                    name = "Task3",
                                    externalId = "Customer A|Project1|Task3",
                                    parentExternalId = "Customer A|Project1",
                                    uuid = "40D2207E-EA0E-4570-A72D-100D3DC83C5C"
                                }
                            }
                        }
                    }
                },
                new Task
                {
                    name = "Customer B",
                    uuid = "2909B8F0-4996-4D51-A2BA-1EB690AB2102"
                },
                new Task
                {
                    name = "Customer C",
                    uuid = "126EF139-0026-44A1-929A-473AC9F4E991"
                }
            };
            var newTasks = new List<Task>();
            var updatedTasks = new List<Task>();

            var timrSyncMock = new Mock<TimrSync>();
            timrSyncMock
                .Setup(timrSync => timrSync.GetTasksAsync(It.IsAny<GetTasksRequest1>()))
                .ReturnsAsync(new GetTasksResponse(tasks.ToArray()));
            timrSyncMock
                .Setup(timrSync => timrSync.AddTaskAsync(It.IsAny<AddTaskRequest>()))
                .Callback((AddTaskRequest addTaskRequest) => newTasks.Add(addTaskRequest.AddTaskRequest1));
            timrSyncMock
                .Setup(timrSync => timrSync.UpdateTaskAsync(It.IsAny<UpdateTaskRequest>()))
                .Callback((UpdateTaskRequest updateTaskRequest) => updatedTasks.Add(updateTaskRequest.UpdateTaskRequest1));

            var projectTimeServiceMock = new Mock<IProjectTimeService>(MockBehavior.Loose);
            var userServiceMock = new Mock<IUserService>(MockBehavior.Strict);
            userServiceMock
                .Setup(service => service.GetUsers())
                .ReturnsAsync(new List<User> { user });

            var taskService = new TaskService(loggerFactory.CreateLogger<TaskService>(), loggerFactory, timrSyncMock.Object);

            var importAction = new ProjectTimeCSVImportAction(loggerFactory.CreateLogger<ProjectTimeCSVImportAction>(), taskService, userServiceMock.Object, projectTimeServiceMock.Object);
            await importAction.Execute("data/projecttime_partial_tasks.csv");

            Assert.AreEqual(6, newTasks.Count);

            var cAp1t1 = newTasks[0];
            Assert.AreEqual("Task1", cAp1t1.name);
            Assert.AreEqual("Customer A|Project1|Task1", cAp1t1.externalId);
            Assert.AreEqual("Customer A|Project1", cAp1t1.parentExternalId);

            Assert.AreEqual(1, updatedTasks.Count);

            var cB = updatedTasks[0];
            Assert.AreEqual("Customer B", cB.name);
            Assert.AreEqual("Customer B", cB.externalId);
            Assert.IsNull(cB.parentExternalId);

            Assert.IsFalse(updatedTasks.Any(task => task.externalId == "Customer C"), "Task 'Customer C' should not get updated when not contained in project time import");
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
                .Setup(service => service.GetTaskHierarchy(It.IsAny<GetTasksRequest>())).ReturnsAsync(new List<Task>());
            taskServiceMock
                .Setup(service => service.FlattenTasks(It.IsAny<IList<Task>>()))
                .Returns<IEnumerable<Task>>(TaskService.FlattenTasks);

            var userServiceMock = new Mock<IUserService>(MockBehavior.Strict);
            userServiceMock
                .Setup(service => service.GetUsers())
                .ReturnsAsync(users);

            var importAction = new ProjectTimeCSVImportAction(loggerFactory.CreateLogger<ProjectTimeCSVImportAction>(), taskServiceMock.Object, userServiceMock.Object, projectTimeServiceMock.Object);
            await importAction.Execute("data/projecttime.csv");

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
                .Setup(service => service.GetTaskHierarchy(It.IsAny<GetTasksRequest>())).ReturnsAsync(new List<Task>());
            taskServiceMock
                .Setup(service => service.FlattenTasks(It.IsAny<IEnumerable<Task>>()))
                .Returns<IEnumerable<Task>>(TaskService.FlattenTasks);

            var userServiceMock = new Mock<IUserService>(MockBehavior.Strict);
            userServiceMock
                .Setup(service => service.GetUsers())
                .ReturnsAsync(users);

            var importAction = new ProjectTimeCSVImportAction(loggerFactory.CreateLogger<ProjectTimeCSVImportAction>(), taskServiceMock.Object, userServiceMock.Object, projectTimeServiceMock.Object);
            await importAction.Execute("data/projecttime.csv");

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
