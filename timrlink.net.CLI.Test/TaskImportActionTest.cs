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
            var tasks = new List<Task>();

            var loggerFactory = new LoggerFactory();

            var taskServiceMock = new Mock<ITaskService>(MockBehavior.Strict);
            taskServiceMock
                .Setup(service => service.GetTaskHierarchy())
                .ReturnsAsync(new List<Task>());
            taskServiceMock
                .Setup(service => service.FlattenTasks(It.IsAny<IEnumerable<Task>>()))
                .Returns((IEnumerable<Task> tasks) => TaskService.FlattenTasks(tasks));
            taskServiceMock
                .Setup(service => service.CreateExternalIdDictionary(It.IsAny<IEnumerable<Task>>(), It.IsAny<Func<Task, string>>()))
                .ReturnsAsync(new Dictionary<string, Task>());
            taskServiceMock
                .Setup(service => service.AddTask(It.IsAny<Task>()))
                .Callback((Task task) => tasks.Add(task))
                .Returns(System.Threading.Tasks.Task.CompletedTask);
            taskServiceMock
                .Setup(service => service.SynchronizeTasksByExternalId(It.IsAny<IDictionary<string, Task>>(), It.IsAny<List<Task>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IEqualityComparer<Task>>()))
                .Callback((IDictionary<string, Task> _, IList<Task> t, bool u, bool d, IEqualityComparer<Task> e) => tasks.AddRange(t))
                .Returns(System.Threading.Tasks.Task.CompletedTask);

            var importAction = new TaskImportAction(loggerFactory, "data/tasks.csv", false, taskServiceMock.Object);
            await importAction.Execute();

            Assert.AreEqual(5, tasks.Count);

            {
                var task = tasks[0];
                Assert.AreEqual("Customer A", task.name);
                Assert.AreEqual("Customer A", task.externalId);
                Assert.IsNull(task.parentExternalId);
                Assert.AreEqual(false, task.bookable);
                Assert.AreEqual(false, task.billable);
                Assert.IsNull(task.description);
                Assert.IsNull(task.start);
                Assert.IsNull(task.end);
            }

            {
                var task = tasks[1];
                Assert.AreEqual("Project1", task.name);
                Assert.AreEqual("Customer A|Project1", task.externalId);
                Assert.AreEqual("Customer A", task.parentExternalId);
                Assert.AreEqual(false, task.bookable);
                Assert.AreEqual(false, task.billable);
                Assert.IsNull(task.description);
                Assert.IsNull(task.start);
                Assert.IsNull(task.end);
            }

            {
                var task = tasks[2];
                Assert.AreEqual("Task1", task.name);
                Assert.AreEqual("Customer A|Project1|Task1", task.externalId);
                Assert.AreEqual("Customer A|Project1", task.parentExternalId);
                Assert.AreEqual(true, task.bookable);
                Assert.AreEqual(false, task.billable);
                Assert.AreEqual("Awesome", task.description);
                Assert.IsNull(task.start);
                Assert.IsNull(task.end);
            }

            {
                // same as task[1] - but update will be performed
                var task = tasks[3];
                Assert.AreEqual("Project1", task.name);
                Assert.AreEqual("Customer A|Project1", task.externalId);
                Assert.AreEqual("Customer A", task.parentExternalId);
                Assert.AreEqual(true, task.bookable);
                Assert.AreEqual(true, task.billable);
                Assert.IsTrue(String.IsNullOrEmpty(task.description));
                Assert.IsNull(task.start);
                Assert.IsNull(task.end);
            }

            {
                var task = tasks[4];
                Assert.AreEqual("Project2", task.name);
                Assert.AreEqual("Customer A|Project2", task.externalId);
                Assert.AreEqual("Customer A", task.parentExternalId);
                Assert.AreEqual(false, task.bookable);
                Assert.AreEqual(true, task.billable);
                Assert.IsTrue(String.IsNullOrEmpty(task.description));
                Assert.AreEqual(new DateTime(2019, 05, 16, 0, 0, 0), task.start);
                Assert.IsNull(task.end);
            }
        }

        [Test]
        public async System.Threading.Tasks.Task TaskCreationNoUpdateCustomFields()
        {
            var tasks = new List<Task>();

            var loggerFactory = new LoggerFactory();

            var taskServiceMock = new Mock<ITaskService>(MockBehavior.Loose);
            taskServiceMock
                .Setup(service => service.GetTaskHierarchy()).ReturnsAsync(new List<Task>());
            taskServiceMock
                .Setup(service => service.FlattenTasks(It.IsAny<IEnumerable<Task>>())).Returns((IEnumerable<Task> tasks) => TaskService.FlattenTasks(tasks));
            taskServiceMock
                .Setup(service => service.CreateExternalIdDictionary(It.IsAny<IEnumerable<Task>>(), It.IsAny<Func<Task, string>>())).ReturnsAsync(new Dictionary<string, Task>());
            taskServiceMock
                .Setup(service => service.AddTask(It.IsAny<Task>()))
                .Callback((Task task) => tasks.Add(task));
            taskServiceMock
                .Setup(service => service.SynchronizeTasksByExternalId(It.IsAny<IDictionary<string, Task>>(), It.IsAny<List<Task>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IEqualityComparer<Task>>()))
                .Callback((IDictionary<string, Task> _, IList<Task> t, bool u, bool d, IEqualityComparer<Task> e) => tasks.AddRange(t));

            var importAction = new TaskImportAction(loggerFactory, "data/tasks_customfields.csv", false, taskServiceMock.Object);
            await importAction.Execute();

            Assert.AreEqual(5, tasks.Count);
            var taskDictionary = tasks.GroupBy(task => task.uuid).ToDictionary(group => group.Last().externalId, group => group.Last());

            {
                var task = tasks[0];
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
                var task = tasks[1];
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
                var task = tasks[2];
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
                // same as task[1] - but update will be performed
                var task = tasks[3];
                Assert.AreEqual("Project1", task.name);
                Assert.AreEqual("Customer A|Project1", task.externalId);
                Assert.AreEqual("Customer A", task.parentExternalId);
                Assert.IsNull(task.parentUuid);
                Assert.AreEqual(true, task.bookable);
                Assert.AreEqual(true, task.billable);
                Assert.IsTrue(String.IsNullOrEmpty(task.description));
                Assert.IsNull(task.start);
                Assert.IsNull(task.end);
                Assert.IsEmpty(task.customField1);
                Assert.IsEmpty(task.customField2);
                Assert.IsEmpty(task.customField3);
            }

            {
                var task = tasks[4];
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

            var loggerFactory = new LoggerFactory();

            var taskServiceMock = new Mock<ITaskService>(MockBehavior.Strict);
            taskServiceMock
                .Setup(service => service.GetTaskHierarchy())
                .ReturnsAsync(new List<Task> { customerATask });
            taskServiceMock
                .Setup(service => service.FlattenTasks(It.IsAny<IEnumerable<Task>>()))
                .Returns((IEnumerable<Task> tasks) => TaskService.FlattenTasks(tasks));
            taskServiceMock
                .Setup(service => service.CreateExternalIdDictionary(It.IsAny<IEnumerable<Task>>(), It.IsAny<Func<Task, string>>()))
                .ReturnsAsync(new Dictionary<string, Task>());
            taskServiceMock
                .Setup(service => service.AddTask(It.IsAny<Task>()))
                .Callback((Task task) => addedTasks.Add(task))
                .Returns(System.Threading.Tasks.Task.CompletedTask);
            taskServiceMock
                .Setup(service => service.UpdateTask(It.IsAny<Task>()))
                .Callback((Task task) => updatedTasks.Add(task))
                .Returns(System.Threading.Tasks.Task.CompletedTask);
            taskServiceMock
                .Setup(service => service.SynchronizeTasksByExternalId(It.IsAny<IDictionary<string, Task>>(), It.IsAny<List<Task>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IEqualityComparer<Task>>()))
                .Callback((IDictionary<string, Task> _, IList<Task> t, bool u, bool d, IEqualityComparer<Task> e) => addedTasks.AddRange(t))
                .Returns(System.Threading.Tasks.Task.CompletedTask);

            var importAction = new TaskImportAction(loggerFactory, "data/tasks.csv", false, taskServiceMock.Object);
            await importAction.Execute();

            Assert.AreEqual(1, updatedTasks.Count);

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
            
            Assert.AreEqual(4, addedTasks.Count);

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
                // same as task[0] - but update will be performed
                var task = addedTasks[2];
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
            }
        }
    }
}
