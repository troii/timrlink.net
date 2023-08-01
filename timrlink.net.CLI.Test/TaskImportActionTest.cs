using System;
using System.Collections.Generic;
using System.Linq;
using CsvHelper.TypeConversion;
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
            var updatedTasks = new List<Task>();

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

            var timrSyncMock = new Mock<TimrSync>(MockBehavior.Strict);
            timrSyncMock
                .Setup(timrSync => timrSync.GetTasksAsync(It.IsAny<GetTasksRequest1>()))
                .ReturnsAsync(new GetTasksResponse(new Task[0]));
            timrSyncMock
                .Setup(timrSync => timrSync.AddTaskAsync(It.IsAny<AddTaskRequest>()))
                .Callback<AddTaskRequest>(addTaskRequest => addTasks.Add(addTaskRequest.AddTaskRequest1))
                .ReturnsAsync(new AddTaskResponse());
            timrSyncMock
                .Setup(timrSync => timrSync.UpdateTaskAsync(It.IsAny<UpdateTaskRequest>()))
                .Callback<UpdateTaskRequest>(updateTaskRequest => updatedTasks.Add(updateTaskRequest.UpdateTaskRequest1))
                .ReturnsAsync(new UpdateTaskResponse());

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
            
            Assert.AreEqual(1, updatedTasks.Count);
            
            {
                var task = updatedTasks[0];
                Assert.AreEqual("Project1", task.name);
                Assert.AreEqual("Customer A|Project1", task.externalId);
                Assert.AreEqual(expected: "Customer A", task.parentExternalId);
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
        }
        
        [Test]
        public async System.Threading.Tasks.Task TaskCreationNoUpdateAndAlreadyExist()
        {
            var addTasks = new List<Task>();
            var updatedTasks = new List<Task>();
            
            var spongebobTask = new Task
            {
                name = "Spongebob",
                externalId = "Spongebob",
                description = "Patrick",
                uuid = Guid.NewGuid().ToString(),
                bookable = true,
                billable = false
            };

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

            var timrSyncMock = new Mock<TimrSync>(MockBehavior.Strict);
            timrSyncMock
                .Setup(timrSync => timrSync.GetTasksAsync(It.IsAny<GetTasksRequest1>()))
                .ReturnsAsync(new GetTasksResponse(new Task[] { spongebobTask }));
            timrSyncMock
                .Setup(timrSync => timrSync.AddTaskAsync(It.IsAny<AddTaskRequest>()))
                .Callback<AddTaskRequest>(addTaskRequest => addTasks.Add(addTaskRequest.AddTaskRequest1))
                .ReturnsAsync(new AddTaskResponse());
            timrSyncMock
                .Setup(timrSync => timrSync.UpdateTaskAsync(It.IsAny<UpdateTaskRequest>()))
                .Callback<UpdateTaskRequest>(updateTaskRequest => updatedTasks.Add(updateTaskRequest.UpdateTaskRequest1))
                .ReturnsAsync(new UpdateTaskResponse());

            var taskService = new TaskService(loggerFactory.CreateLogger<TaskService>(), loggerFactory, timrSyncMock.Object);

            var importAction = new TaskImportAction(loggerFactory, "data/simple_task.csv", false, taskService);
            await importAction.Execute();

            Assert.AreEqual(0, addTasks.Count);
            Assert.AreEqual(0, updatedTasks.Count);
        }
        
        [Test]
        public async System.Threading.Tasks.Task TaskCreationNoUpdateAndAlreadyExistButDifferent()
        {
            var addTasks = new List<Task>();
            var updatedTasks = new List<Task>();
            
            var spongebobTask = new Task
            {
                name = "Spongebob",
                description = "Bikiny Bottom",
                uuid = Guid.NewGuid().ToString(),
                bookable = true,
                billable = false
            };

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

            var timrSyncMock = new Mock<TimrSync>(MockBehavior.Strict);
            timrSyncMock
                .Setup(timrSync => timrSync.GetTasksAsync(It.IsAny<GetTasksRequest1>()))
                .ReturnsAsync(new GetTasksResponse(new Task[] { spongebobTask }));
            timrSyncMock
                .Setup(timrSync => timrSync.AddTaskAsync(It.IsAny<AddTaskRequest>()))
                .Callback<AddTaskRequest>(addTaskRequest => addTasks.Add(addTaskRequest.AddTaskRequest1))
                .ReturnsAsync(new AddTaskResponse());
            timrSyncMock
                .Setup(timrSync => timrSync.UpdateTaskAsync(It.IsAny<UpdateTaskRequest>()))
                .Callback<UpdateTaskRequest>(updateTaskRequest => updatedTasks.Add(updateTaskRequest.UpdateTaskRequest1))
                .ReturnsAsync(new UpdateTaskResponse());

            var taskService = new TaskService(loggerFactory.CreateLogger<TaskService>(), loggerFactory, timrSyncMock.Object);

            var importAction = new TaskImportAction(loggerFactory, "data/simple_task.csv", false, taskService);
            await importAction.Execute();

            Assert.AreEqual(0, addTasks.Count);
            Assert.AreEqual(0, updatedTasks.Count);
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
        
        [Test]
        public async System.Threading.Tasks.Task TaskCreationWithBudget()
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

            var importAction = new TaskImportAction(loggerFactory, "data/tasks_with_budget.csv", false, taskService);
            await importAction.Execute();

            Assert.AreEqual(4, createdTasks.Count);
            
            /// Task;Bookable;Billable;Description;Start;End;DescriptionRequired;BudgetPlanningType;BudgetPlanningTypeInherited;HoursPlanned;HourlyRate;BudgetPlanned
            
            {
                /// Berg;True;False;Awesome;;;False;NONE;False;1.00;4.00;16.00 
                var task = updatedTasks[0];
                Assert.AreEqual("Berg", task.name);
                Assert.AreEqual("Berg", task.externalId);
                Assert.IsEmpty(task.parentExternalId);
                Assert.AreEqual(true, task.bookable);
                Assert.AreEqual(false, task.billable);
                Assert.AreEqual("Awesome", task.description);
                Assert.IsNull(task.start);
                Assert.IsNull(task.end);
                Assert.AreEqual(expected:BudgetPlanningType.NONE, task.budgetPlanningType);
                Assert.IsFalse(task.budgetPlanningTypeInherited);
                Assert.AreEqual(1.0,task.hoursPlanned);
                Assert.AreEqual(4.0, task.hourlyRate);
                Assert.AreEqual(16.0, task.budgetPlanned);
            }
            
            {
                /// Berg|Priel;True;True;;;;True;TASK_HOURLY_RATE;True;2.00;8.00;32.00 
                var task = createdTasks[2];
                Assert.AreEqual("Priel", task.name);
                Assert.AreEqual("Berg|Priel", task.externalId);
                Assert.AreEqual("Berg", task.parentExternalId);
                Assert.AreEqual(true, task.bookable);
                Assert.AreEqual(true, task.billable);
                Assert.AreEqual("", task.description);
                Assert.IsNull(task.start);
                Assert.IsNull(task.end);
                Assert.AreEqual(expected:BudgetPlanningType.TASK_HOURLY_RATE, task.budgetPlanningType);
                Assert.IsTrue(task.budgetPlanningTypeInherited);
                Assert.AreEqual(2.0,task.hoursPlanned);
                Assert.AreEqual(8.0, task.hourlyRate);
                Assert.AreEqual(32.0, task.budgetPlanned);
            }
            
            {
                /// Fluss;false;true;;2019-05-16;;true;USER_HOURLY_RATE;False;3.00;12.00;48.00 
                var task = updatedTasks[1];
                Assert.AreEqual("Fluss", task.name);
                Assert.AreEqual("Fluss", task.externalId);
                Assert.IsEmpty(task.parentExternalId);
                Assert.IsFalse(task.bookable);
                Assert.IsTrue(task.billable);
                Assert.IsEmpty(task.description);
                Assert.AreEqual(new DateTime(2019, 05, 16, 0, 0, 0), task.start);
                Assert.IsNull(task.end);
                Assert.AreEqual(expected:BudgetPlanningType.USER_HOURLY_RATE, task.budgetPlanningType);
                Assert.IsFalse(task.budgetPlanningTypeInherited);
                Assert.AreEqual(3.0,task.hoursPlanned);
                Assert.AreEqual(12.0, task.hourlyRate);
                Assert.AreEqual(48.0, task.budgetPlanned);
            }
            
            {
                /// Fluss|Enns;false;true;;2019-05-16;;true;FIXED_PRICE;False;4.00;16.00;64.00
                var task = createdTasks[3];
                Assert.AreEqual("Enns", task.name);
                Assert.AreEqual("Fluss|Enns", task.externalId);
                Assert.AreEqual("Fluss", task.parentExternalId);
                Assert.AreEqual(false, task.bookable);
                Assert.AreEqual(true, task.billable);
                Assert.AreEqual("",task.description);
                Assert.AreEqual(new DateTime(2019, 05, 16, 0, 0, 0), task.start);
                Assert.IsNull(task.end);
                Assert.AreEqual(expected:BudgetPlanningType.FIXED_PRICE, task.budgetPlanningType);
                Assert.IsFalse(task.budgetPlanningTypeInherited);
                Assert.AreEqual(4.0,task.hoursPlanned);
                Assert.AreEqual(16.0, task.hourlyRate);
                Assert.AreEqual(64.0, task.budgetPlanned);
            }
        }
        
        [Test]
        public void TaskCreationWithBudgetAndInvalidBudgetType()
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

            var importAction = new TaskImportAction(loggerFactory, "data/task_with_wrong_budget_type.csv", false, taskService);
            
            Assert.ThrowsAsync<TypeConverterException>(() => importAction.Execute());
        }
        
        [Test]
        public async System.Threading.Tasks.Task TaskCreationWithBudgetAndNoBudgetTypeColumn()
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

            var importAction = new TaskImportAction(loggerFactory, "data/task_with_budget_and_no_budget_type.csv", false, taskService);
            
            await importAction.Execute();

            Assert.AreEqual(1, createdTasks.Count);
            
            {
                /// No Budget Type;True;True;Awesome;;;False;False;10,00;5,00;50,00
                var task = createdTasks[0];
                Assert.AreEqual("No Budget Type", task.name);
                Assert.AreEqual("No Budget Type", task.externalId);
                Assert.IsEmpty(task.parentExternalId);
                Assert.AreEqual(true, task.bookable);
                Assert.AreEqual(true, task.billable);
                Assert.AreEqual("Awesome",task.description);
                Assert.IsNull(task.start);
                Assert.IsNull(task.end);
                Assert.IsNull(task.budgetPlanningType);
                Assert.IsNull(task.budgetPlanningTypeInherited);
                Assert.IsNull(task.hoursPlanned);
                Assert.IsNull(task.hourlyRate);
                Assert.IsNull(task.budgetPlanned);
                Assert.IsFalse(task.budgetPlanningTypeSpecified);
                Assert.IsFalse(task.budgetPlanningTypeInheritedSpecified);
                Assert.IsFalse(task.hourlyRateSpecified);
                Assert.IsFalse(task.hourlyRateSpecified);
                Assert.IsFalse(task.budgetPlannedSpecified);
            }
        }
    }
}
