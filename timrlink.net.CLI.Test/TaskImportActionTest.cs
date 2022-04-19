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
    public class TaskImportActionTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async System.Threading.Tasks.Task TaskCreationNoUpdate()
        {
            var customerATask = new Task
            {
                name = "Customer A",
                externalId = "Customer A",
                uuid = Guid.NewGuid().ToString(),
                bookable = false,
                billable = true
            };
            
            var tasks = new List<Task>();
            var updatedTasks = new List<Task>();

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

            var timrSyncMock = new Mock<TimrSync>(MockBehavior.Strict);
            timrSyncMock
                .Setup(timrSync => timrSync.GetTasksAsync(It.IsAny<GetTasksRequest1>()))
                .ReturnsAsync(new GetTasksResponse(new [] { customerATask } ));
            timrSyncMock
                .Setup(timrSync => timrSync.AddTaskAsync(It.IsAny<AddTaskRequest>()))
                .Callback((AddTaskRequest addTaskRequest) => tasks.Add(addTaskRequest.AddTaskRequest1))
                .ReturnsAsync(new AddTaskResponse());
            timrSyncMock
                .Setup(timrSync => timrSync.UpdateTaskAsync(It.IsAny<UpdateTaskRequest>()))
                .Callback<UpdateTaskRequest>(request => updatedTasks.Add(request.UpdateTaskRequest1))
                .ReturnsAsync(new UpdateTaskResponse());

            var taskService = new TaskService(loggerFactory.CreateLogger<TaskService>(), loggerFactory, timrSyncMock.Object);

            var importAction = new TaskImportAction(loggerFactory, "data/tasks.csv", false, taskService);
            await importAction.Execute();

            Assert.AreEqual(3, tasks.Count);

            {
                var task = tasks[0];
                Assert.AreEqual("Project1", task.name);
                Assert.AreEqual("Customer A|Project1", task.externalId);
                Assert.AreEqual("Customer A", task.parentExternalId);
                Assert.AreEqual(false, task.bookable);
                Assert.AreEqual(false, task.billable);
                Assert.IsNull(task.description);
                Assert.IsNull(task.start);
                Assert.IsNull(task.end);
                Assert.IsFalse(task.descriptionRequired);
            }

            {
                var task = tasks[1];
                Assert.AreEqual("Task1", task.name);
                Assert.AreEqual("Customer A|Project1|Task1", task.externalId);
                Assert.AreEqual("Customer A|Project1", task.parentExternalId);
                Assert.AreEqual(true, task.bookable);
                Assert.AreEqual(false, task.billable);
                Assert.AreEqual("Awesome", task.description);
                Assert.IsNull(task.start);
                Assert.IsNull(task.end);
                Assert.IsFalse(task.descriptionRequired);
            }

            {
                var task = tasks[2];
                Assert.AreEqual("Project2", task.name);
                Assert.AreEqual("Customer A|Project2", task.externalId);
                Assert.AreEqual("Customer A", task.parentExternalId);
                Assert.AreEqual(false, task.bookable);
                Assert.AreEqual(true, task.billable);
                Assert.IsTrue(String.IsNullOrEmpty(task.description));
                Assert.AreEqual(new DateTime(2019, 05, 16, 0, 0, 0), task.start);
                Assert.IsNull(task.end);
                Assert.IsTrue(task.descriptionRequired);
            }
            
            Assert.AreEqual(1, updatedTasks.Count);
            
            {
                var task = updatedTasks[0];
                Assert.AreEqual("Project1", task.name);
                Assert.AreEqual("Customer A|Project1", task.externalId);
                Assert.AreEqual("Customer A", task.parentExternalId);
                Assert.AreEqual(true, task.bookable);
                Assert.AreEqual(true, task.billable);
                Assert.IsEmpty(task.description);
                Assert.IsNull(task.start);
                Assert.IsNull(task.end);
                Assert.IsTrue(task.descriptionRequired);
            }
        }

        [Test]
        public async System.Threading.Tasks.Task TaskCreationNoUpdateCustomFields()
        {
            var addedTasks = new List<Task>();
            var updatedTasks = new List<Task>();

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

            var timrSyncMock = new Mock<TimrSync>(MockBehavior.Strict);
            timrSyncMock
                .Setup(timrSync => timrSync.GetTasksAsync(It.IsAny<GetTasksRequest1>()))
                .ReturnsAsync(new GetTasksResponse(new Task[0]));
            timrSyncMock
                .Setup(timrSync => timrSync.AddTaskAsync(It.IsAny<AddTaskRequest>()))
                .Callback((AddTaskRequest addTaskRequest) => addedTasks.Add(addTaskRequest.AddTaskRequest1))
                .ReturnsAsync(new AddTaskResponse());
            timrSyncMock
                .Setup(timrSync => timrSync.UpdateTaskAsync(It.IsAny<UpdateTaskRequest>()))
                .Callback<UpdateTaskRequest>(request => updatedTasks.Add(request.UpdateTaskRequest1))
                .ReturnsAsync(new UpdateTaskResponse());

            var taskService = new TaskService(loggerFactory.CreateLogger<TaskService>(), loggerFactory, timrSyncMock.Object);

            var importAction = new TaskImportAction(loggerFactory, "data/tasks_customfields.csv", false, taskService);
            await importAction.Execute();
            
            Assert.AreEqual(4, addedTasks.Count);

            {
                var task = addedTasks[0];
                Assert.AreEqual("Customer A", task.name);
                Assert.AreEqual("Customer A", task.externalId);
                Assert.IsNull(task.parentExternalId);
                Assert.IsNull(task.parentUuid);
                Assert.AreEqual(false, task.bookable);
                Assert.AreEqual(false, task.billable);
                Assert.IsNull(task.description);
                Assert.IsNull(task.start);
                Assert.IsNull(task.end);
                Assert.IsNull(task.customField1);
                Assert.IsNull(task.customField2);
                Assert.IsNull(task.customField3);
            }

            {
                var task = addedTasks[1];
                Assert.AreEqual("Project1", task.name);
                Assert.AreEqual("Customer A|Project1", task.externalId);
                Assert.AreEqual("Customer A", task.parentExternalId);
                Assert.IsNull(task.parentUuid);
                Assert.AreEqual(false, task.bookable);
                Assert.AreEqual(false, task.billable);
                Assert.IsNull(task.description);
                Assert.IsNull(task.start);
                Assert.IsNull(task.end);
                Assert.IsNull(task.customField1);
                Assert.IsNull(task.customField2);
                Assert.IsNull(task.customField3);
            }

            {
                var task = addedTasks[2];
                Assert.AreEqual("Task1", task.name);
                Assert.AreEqual("Customer A|Project1|Task1", task.externalId);
                Assert.AreEqual("Customer A|Project1", task.parentExternalId);
                Assert.IsNull(task.parentUuid);
                Assert.AreEqual(true, task.bookable);
                Assert.AreEqual(false, task.billable);
                Assert.AreEqual("Awesome", task.description);
                Assert.IsNull(task.start);
                Assert.IsNull(task.end);
                Assert.IsEmpty(task.customField1);
                Assert.AreEqual("2", task.customField2);
                Assert.AreEqual("Feld3", task.customField3);
            }

            {
                var task = addedTasks[3];
                Assert.AreEqual("Project2", task.name);
                Assert.AreEqual("Customer A|Project2", task.externalId);
                Assert.AreEqual("Customer A", task.parentExternalId);
                Assert.IsNull(task.parentUuid);
                Assert.AreEqual(false, task.bookable);
                Assert.AreEqual(true, task.billable);
                Assert.IsTrue(String.IsNullOrEmpty(task.description));
                Assert.AreEqual(new DateTime(2019, 05, 16, 0, 0, 0), task.start);
                Assert.IsNull(task.end);
                Assert.AreEqual("1", task.customField1);
                Assert.IsEmpty(task.customField2);
                Assert.IsEmpty(task.customField3);
            }
            
            Assert.AreEqual(1, updatedTasks.Count);
            
            {
                var task = updatedTasks[0];
                Assert.AreEqual("Project1", task.name);
                Assert.AreEqual("Customer A|Project1", task.externalId);
                Assert.AreEqual("Customer A", task.parentExternalId);
                Assert.AreEqual(true, task.bookable);
                Assert.AreEqual(true, task.billable);
                Assert.IsEmpty(task.description);
                Assert.IsNull(task.start);
                Assert.IsNull(task.end);
            }
        }

        [Test]
        public async System.Threading.Tasks.Task TaskCreationUpdate()
        {
            var customerATask = new Task
            {
                name = "Customer A",
                uuid = Guid.NewGuid().ToString(),
                bookable = false,
                billable = true
            };

            var addedTasks = new List<Task>();
            var updatedTasks = new List<Task>();

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

            var timrSyncMock = new Mock<TimrSync>(MockBehavior.Strict);
            timrSyncMock
                .Setup(timrSync => timrSync.GetTasksAsync(It.IsAny<GetTasksRequest1>()))
                .ReturnsAsync(new GetTasksResponse(new[] { customerATask }));
            timrSyncMock
                .Setup(timrSync => timrSync.AddTaskAsync(It.IsAny<AddTaskRequest>()))
                .Callback<AddTaskRequest>(addTaskRequest => addedTasks.Add(addTaskRequest.AddTaskRequest1))
                .ReturnsAsync(new AddTaskResponse());
            timrSyncMock
                .Setup(timrSync => timrSync.UpdateTaskAsync(It.IsAny<UpdateTaskRequest>()))
                .Callback<UpdateTaskRequest>(request => updatedTasks.Add(request.UpdateTaskRequest1))
                .ReturnsAsync(new UpdateTaskResponse());

            var taskService = new TaskService(loggerFactory.CreateLogger<TaskService>(), loggerFactory, timrSyncMock.Object);

            var importAction = new TaskImportAction(loggerFactory, "data/tasks.csv", true, taskService);
            await importAction.Execute();

            Assert.AreEqual(2, updatedTasks.Count);

            {
                var task = updatedTasks[0];
                Assert.AreEqual("Customer A", task.name);
                Assert.AreEqual("Customer A", task.externalId);
                Assert.IsNull(task.parentExternalId);
                Assert.IsNull(task.parentUuid);
                Assert.AreEqual(false, task.bookable);
                Assert.AreEqual(true, task.billable);
                Assert.IsNull(task.description);
                Assert.IsNull(task.start);
                Assert.IsNull(task.end);
            }
            
            {
                var task = updatedTasks[1];
                Assert.AreEqual("Project1", task.name);
                Assert.AreEqual("Customer A|Project1", task.externalId);
                Assert.AreEqual("Customer A", task.parentExternalId);
                Assert.IsNull(task.parentUuid);
                Assert.AreEqual(true, task.bookable);
                Assert.AreEqual(true, task.billable);
                Assert.IsTrue(String.IsNullOrEmpty(task.description));
                Assert.IsNull(task.start);
                Assert.IsNull(task.end);
            }

            Assert.AreEqual(3, addedTasks.Count);

            {
                var task = addedTasks[0];
                Assert.AreEqual("Project1", task.name);
                Assert.AreEqual("Customer A|Project1", task.externalId);
                Assert.AreEqual("Customer A", task.parentExternalId);
                Assert.IsNull(task.parentUuid);
                Assert.AreEqual(false, task.bookable);
                Assert.AreEqual(false, task.billable);
                Assert.IsNull(task.description);
                Assert.IsNull(task.start);
                Assert.IsNull(task.end);
            }

            {
                var task = addedTasks[1];
                Assert.AreEqual("Task1", task.name);
                Assert.AreEqual("Customer A|Project1|Task1", task.externalId);
                Assert.AreEqual("Customer A|Project1", task.parentExternalId);
                Assert.IsNull(task.parentUuid);
                Assert.AreEqual(true, task.bookable);
                Assert.AreEqual(false, task.billable);
                Assert.AreEqual("Awesome", task.description);
                Assert.IsNull(task.start);
                Assert.IsNull(task.end);
            }

            {
                var task = addedTasks[2];
                Assert.AreEqual("Project2", task.name);
                Assert.AreEqual("Customer A|Project2", task.externalId);
                Assert.AreEqual("Customer A", task.parentExternalId);
                Assert.IsNull(task.parentUuid);
                Assert.AreEqual(false, task.bookable);
                Assert.AreEqual(true, task.billable);
                Assert.IsTrue(String.IsNullOrEmpty(task.description), "String.IsNullOrEmpty(task.description)");
                Assert.AreEqual(new DateTime(2019, 05, 16, 0, 0, 0), task.start);
                Assert.IsNull(task.end);
            }
        }

        [Test]
        public async System.Threading.Tasks.Task TaskCreationWithSubtasks()
        {
            var addTasks = new List<Task>();

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

            var timrSyncMock = new Mock<TimrSync>(MockBehavior.Strict);
            timrSyncMock
                .Setup(timrSync => timrSync.GetTasksAsync(It.IsAny<GetTasksRequest1>()))
                .ReturnsAsync(new GetTasksResponse(new Task[0]));
            timrSyncMock
                .Setup(timrSync => timrSync.AddTaskAsync(It.IsAny<AddTaskRequest>()))
                .Callback<AddTaskRequest>(addTaskRequest => addTasks.Add(addTaskRequest.AddTaskRequest1))
                .ReturnsAsync(new AddTaskResponse());

            var taskService = new TaskService(loggerFactory.CreateLogger<TaskService>(), loggerFactory, timrSyncMock.Object);

            var importAction = new TaskImportAction(loggerFactory, "data/tasks_with_subtasks.csv", false, taskService);
            await importAction.Execute();

            Assert.AreEqual(9, addTasks.Count);

            {
                var task = addTasks[0];
                Assert.AreEqual("Customer A", task.name);
                Assert.AreEqual("Customer A", task.externalId);
                Assert.IsNull(task.parentExternalId);
                Assert.IsNull(task.parentUuid);
                Assert.AreEqual(false, task.bookable);
                Assert.AreEqual(false, task.billable);
                Assert.IsNull(task.description);
                Assert.IsNull(task.start);
                Assert.IsNull(task.end);
                Assert.IsNull(task.customField1);
                Assert.IsNull(task.customField2);
                Assert.IsNull(task.customField3);
            }

            {
                var task = addTasks[1];
                Assert.AreEqual("Project1", task.name);
                Assert.AreEqual("Customer A|Project1", task.externalId);
                Assert.AreEqual(expected: "Customer A", task.parentExternalId);
                Assert.IsNull(task.parentUuid);
                Assert.AreEqual(false, task.bookable);
                Assert.AreEqual(false, task.billable);
                Assert.IsNull(task.description);
                Assert.IsNull(task.start);
                Assert.IsNull(task.end);
                Assert.IsNull(task.customField1);
                Assert.IsNull(task.customField2);
                Assert.IsNull(task.customField3);
            }

            {
                var task = addTasks[2];
                Assert.AreEqual("Task1", task.name);
                Assert.AreEqual("Customer A|Project1|Task1", task.externalId);
                Assert.AreEqual("Customer A|Project1", task.parentExternalId);
                Assert.IsNull(task.parentUuid);
                Assert.AreEqual(true, task.bookable);
                Assert.AreEqual(false, task.billable);
                Assert.AreEqual("Awesome", task.description);
                Assert.IsNull(task.start);
                Assert.IsNull(task.end);
                Assert.AreEqual("test", task.customField1);
                Assert.IsNull(task.customField2);
                Assert.IsNull(task.customField3);
            }

            {
                var task = addTasks[3];
                Assert.AreEqual("Support", task.name);
                Assert.AreEqual("Customer A|Project1|Task1|Support", task.externalId);
                Assert.AreEqual("Customer A|Project1|Task1", task.parentExternalId);
                Assert.IsNull(task.parentUuid);
                Assert.AreEqual(true, task.bookable);
                Assert.AreEqual(false, task.billable);
                Assert.IsNull(task.description);
                Assert.IsNull(task.start);
                Assert.IsNull(task.end);
                Assert.IsNull(task.customField1);
                Assert.IsNull(task.customField2);
                Assert.IsNull(task.customField3);
            }

            {
                var task = addTasks[4];
                Assert.AreEqual("Sales", task.name);
                Assert.AreEqual("Customer A|Project1|Task1|Sales", task.externalId);
                Assert.AreEqual("Customer A|Project1|Task1", task.parentExternalId);
                Assert.IsNull(task.parentUuid);
                Assert.AreEqual(true, task.bookable);
                Assert.AreEqual(false, task.billable);
                Assert.IsNull(task.description);
                Assert.IsNull(task.start);
                Assert.IsNull(task.end);
                Assert.IsNull(task.customField1);
                Assert.IsNull(task.customField2);
                Assert.IsNull(task.customField3);
            }
            
            {
                var task = addTasks[5];
                Assert.AreEqual("Subtask1", task.name);
                Assert.AreEqual("Customer A|Project1|Subtask1", task.externalId);
                Assert.AreEqual("Customer A|Project1", task.parentExternalId);
                Assert.IsNull(task.parentUuid);
                Assert.AreEqual(true, task.bookable);
                Assert.AreEqual(true, task.billable);
                Assert.IsNull(task.description);
                Assert.IsNull(task.start);
                Assert.IsNull(task.end);
                Assert.IsNull(task.customField1);
                Assert.IsNull(task.customField2);
                Assert.IsNull(task.customField3);
            }

            {
                var task = addTasks[6];
                Assert.AreEqual("Project3", task.name);
                Assert.AreEqual("Customer A|Project3", task.externalId);
                Assert.AreEqual("Customer A", task.parentExternalId);
                Assert.IsNull(task.parentUuid);
                Assert.AreEqual(true, task.bookable);
                Assert.AreEqual(true, task.billable);
                Assert.AreEqual("", task.description);
                Assert.IsNull(task.start);
                Assert.IsNull(task.end);
                Assert.AreEqual("", task.customField1);
                Assert.IsNull(task.customField2);
                Assert.IsNull(task.customField3);
            }

            {
                var task = addTasks[7];
                Assert.AreEqual("Subtask3", task.name);
                Assert.AreEqual("Customer A|Project3|Subtask3", task.externalId);
                Assert.AreEqual("Customer A|Project3", task.parentExternalId);
                Assert.IsNull(task.parentUuid);
                Assert.AreEqual(true, task.bookable);
                Assert.AreEqual(true, task.billable);
                Assert.IsNull(task.description);
                Assert.IsNull(task.start);
                Assert.IsNull(task.end);
                Assert.IsNull(task.customField1);
                Assert.IsNull(task.customField2);
                Assert.IsNull(task.customField3);
            }

            {
                var task = addTasks[8];
                Assert.AreEqual("Project2", task.name);
                Assert.AreEqual("Customer A|Project2", task.externalId);
                Assert.AreEqual("Customer A", task.parentExternalId);
                Assert.IsNull(task.parentUuid);
                Assert.AreEqual(false, task.bookable);
                Assert.AreEqual(true, task.billable);
                Assert.AreEqual("", task.description);
                Assert.AreEqual(new DateTime(2019, 05, 16, 0, 0, 0), task.start);
                Assert.IsNull(task.end);
                Assert.AreEqual("1", task.customField1);
                Assert.IsNull(task.customField2);
                Assert.IsNull(task.customField3);
            }
        }

        [Test]
        public async System.Threading.Tasks.Task TaskCreationWithAddress()
        {
            var createdTasks = new List<Task>();
            var updatedTasks = new List<Task>();

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

            var timrSyncMock = new Mock<TimrSync>(MockBehavior.Strict);
            timrSyncMock
                .Setup(timrSync => timrSync.GetTasksAsync(It.IsAny<GetTasksRequest1>()))
                .ReturnsAsync(new GetTasksResponse(new Task[0]));
            timrSyncMock
                .Setup(timrSync => timrSync.AddTaskAsync(It.IsAny<AddTaskRequest>()))
                .Callback((AddTaskRequest addTaskRequest) => createdTasks.Add(addTaskRequest.AddTaskRequest1))
                .ReturnsAsync(new AddTaskResponse());
            timrSyncMock
                .Setup(timrSync => timrSync.UpdateTaskAsync(It.IsAny<UpdateTaskRequest>()))
                .Callback<UpdateTaskRequest>(request => updatedTasks.Add(request.UpdateTaskRequest1))
                .ReturnsAsync(new UpdateTaskResponse());

            var taskService = new TaskService(loggerFactory.CreateLogger<TaskService>(), loggerFactory, timrSyncMock.Object);

            var importAction = new TaskImportAction(loggerFactory, "data/tasks_with_address.csv", false, taskService);
            await importAction.Execute();

            Assert.AreEqual(3, createdTasks.Count);
            
            {
                var task = createdTasks[0];
                Assert.AreEqual("Orts basiert", task.name);
                Assert.AreEqual("Orts basiert", task.externalId);
                Assert.IsNull(task.parentExternalId);
                Assert.AreEqual(false, task.bookable);
                Assert.AreEqual(false, task.billable);
                Assert.IsNull(task.description);
                Assert.IsNull(task.start);
                Assert.IsNull(task.end);
                Assert.Null(task.address);
                Assert.Null(task.city);
                Assert.Null(task.zipCode);
                Assert.Null(task.state);
                Assert.Null(task.country);
                Assert.Null(task.latitude);
                Assert.Null(task.longitude);
            }

            {
                var task = createdTasks[1];
                Assert.AreEqual("Poolhall", task.name);
                Assert.AreEqual("Orts basiert|Poolhall", task.externalId);
                Assert.AreEqual("Orts basiert", task.parentExternalId);
                Assert.AreEqual(true, task.bookable);
                Assert.AreEqual(true, task.billable);
                Assert.AreEqual("", task.description);
                Assert.IsNull(task.start);
                Assert.IsNull(task.end);
                Assert.AreEqual("Wattstraße 6", task.address);
                Assert.AreEqual("Linz", task.city);
                Assert.AreEqual("4030", task.zipCode);
                Assert.AreEqual("",task.state);
                Assert.AreEqual("AT", task.country);
                Assert.AreEqual(48.24676258791299, task.latitude);
                Assert.AreEqual(14.265460834572343, task.longitude);
            }

            {
                var task = createdTasks[2];
                Assert.AreEqual("Burgerking", task.name);
                Assert.AreEqual("Orts basiert|Burgerking", task.externalId);
                Assert.AreEqual("Orts basiert", task.parentExternalId);
                Assert.AreEqual(false, task.bookable);
                Assert.AreEqual(true, task.billable);
                Assert.AreEqual("",task.description);
                Assert.AreEqual(new DateTime(2019, 05, 16, 0, 0, 0), task.start);
                Assert.IsNull(task.end);
                Assert.AreEqual("Salzburger Str. 385", task.address);
                Assert.AreEqual("Linz", task.city);
                Assert.AreEqual("4030", task.zipCode);
                Assert.AreEqual("Oberösterreich", task.state);
                Assert.AreEqual("DE", task.country);
                Assert.IsNull(task.latitude);
                Assert.IsNull(task.longitude);
            }
            
            Assert.AreEqual(1, updatedTasks.Count);
            
            {
                var task = updatedTasks[0];
                Assert.AreEqual("Orts basiert", task.name);
                Assert.AreEqual("Orts basiert", task.externalId);
                Assert.AreEqual("", task.parentExternalId);
                Assert.IsTrue(task.bookable);
                Assert.IsTrue(task.billable);
                Assert.AreEqual("", task.description);
                Assert.IsNull(task.start);
                Assert.IsNull(task.end);
                Assert.AreEqual("Martinistraße 8/2",task.address);
                Assert.AreEqual("Leonding",task.city);
                Assert.AreEqual("4060", task.zipCode);
                Assert.AreEqual("", task.state);
                Assert.AreEqual("AT", task.country);
                Assert.AreEqual(48.246461, task.latitude);
                Assert.AreEqual(14.261041, task.longitude);
            }
        }
    }
}
