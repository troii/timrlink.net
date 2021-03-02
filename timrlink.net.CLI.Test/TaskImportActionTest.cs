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
        public async System.Threading.Tasks.Task TaskCreationInvalidHeader()
        {
            var loggerFactory = new LoggerFactory();

            var taskServiceMock = new Mock<ITaskService>(MockBehavior.Strict);
            taskServiceMock
                .Setup(service => service.GetTaskHierarchy())
                .ReturnsAsync(new List<Task>());
            taskServiceMock
                .Setup(service => service.FlattenTasks(It.IsAny<IEnumerable<Task>>()))
                .Returns((IEnumerable<Task> tasks) => TaskService.FlattenTasks(tasks));
            
            var importAction = new TaskImportAction(loggerFactory, "data/tasks_bad_header.csv", false, taskServiceMock.Object);
            await importAction.Execute();
        }
        
        [Test]
        public async System.Threading.Tasks.Task TaskCreationDuplicateExternalId()
        {
            var loggerFactory = new LoggerFactory();

            var taskServiceMock = new Mock<ITaskService>(MockBehavior.Strict);
            taskServiceMock
                .Setup(service => service.GetTaskHierarchy())
                .ReturnsAsync(new List<Task>());
            taskServiceMock
                .Setup(service => service.FlattenTasks(It.IsAny<IEnumerable<Task>>()))
                .Returns((IEnumerable<Task> tasks) => TaskService.FlattenTasks(tasks));
            
            var importAction = new TaskImportAction(loggerFactory, "data/tasks_duplicate_ExternalId.csv", false, taskServiceMock.Object);
            await importAction.Execute();
        }

        [Test]
        public async System.Threading.Tasks.Task TaskCreationNoUpdate()
        {
            var tasks = new List<Task>();
            var updatedTasks = new List<Task>();

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
                .Setup(service => service.UpdateTask(It.IsAny<Task>()))
                .Callback((Task task) => updatedTasks.Add(task))
                .Returns(System.Threading.Tasks.Task.CompletedTask);
            taskServiceMock
                .Setup(service => service.SynchronizeTasksByExternalId(It.IsAny<IDictionary<string, Task>>(), It.IsAny<List<Task>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IEqualityComparer<Task>>()))
                .Callback((IDictionary<string, Task> _, IList<Task> t, bool u, bool d, IEqualityComparer<Task> e) => tasks.AddRange(t))
                .Returns(System.Threading.Tasks.Task.CompletedTask);

            var importAction = new TaskImportAction(loggerFactory, "data/tasks.csv", false, taskServiceMock.Object);
            await importAction.Execute();

            Assert.AreEqual(4, tasks.Count);
            Assert.AreEqual(1, updatedTasks.Count);
            
            var customerATask = tasks[0];
            Assert.AreEqual("Customer A", customerATask.name);
            Assert.IsNull(customerATask.parentExternalId);
            Assert.IsNull(customerATask.externalId);
            Assert.AreEqual(false, customerATask.bookable);
            Assert.AreEqual(false, customerATask.billable);
            Assert.IsNull(customerATask.description);
            Assert.IsNull(customerATask.start);
            Assert.IsNull(customerATask.end);

            var project1Task = updatedTasks[0];
            Assert.AreEqual("Project1", project1Task.name);
            Assert.IsNull(project1Task.parentExternalId);
            Assert.IsNull(project1Task.externalId);
            Assert.AreEqual(customerATask.uuid, project1Task.parentUuid);
            Assert.AreEqual(true, project1Task.bookable);
            Assert.AreEqual(true, project1Task.billable);
            Assert.IsTrue(String.IsNullOrEmpty(project1Task.description));
            Assert.IsNull(project1Task.start);
            Assert.IsNull(project1Task.end);
            
            var task1 = tasks[2];
            Assert.AreEqual("Task1", task1.name);
            Assert.IsNull(task1.parentExternalId);
            Assert.AreEqual(task1.externalId, "A11");
            Assert.AreEqual(true, task1.bookable);
            Assert.AreEqual(false, task1.billable);
            Assert.AreEqual(project1Task.uuid, task1.parentUuid);
            Assert.AreEqual("Awesome", task1.description);
            Assert.IsNull(task1.start);
            Assert.IsNull(task1.end);
            
            var project2 = tasks[3];
            Assert.AreEqual("Project2", project2.name);
            Assert.IsNull(project2.parentExternalId);
            Assert.IsNull(project2.externalId);
            Assert.AreEqual(false, project2.bookable);
            Assert.AreEqual(true, project2.billable);
            Assert.AreEqual(project2.parentUuid, customerATask.uuid);
            Assert.IsTrue(String.IsNullOrEmpty(project2.description));
            Assert.AreEqual(project2.start, new DateTime(2019, 05, 16, 0, 0, 0));
            Assert.IsNull(project2.end);
        }

        [Test]
        public async System.Threading.Tasks.Task TaskCreationNoUpdateCustomFields()
        {
            var tasks = new List<Task>();
            var updatedTasks = new List<Task>();

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
                .Setup(service => service.UpdateTask(It.IsAny<Task>()))
                .Callback((Task task) => updatedTasks.Add(task))
                .Returns(System.Threading.Tasks.Task.CompletedTask);
            taskServiceMock
                .Setup(service => service.SynchronizeTasksByExternalId(It.IsAny<IDictionary<string, Task>>(), It.IsAny<List<Task>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IEqualityComparer<Task>>()))
                .Callback((IDictionary<string, Task> _, IList<Task> t, bool u, bool d, IEqualityComparer<Task> e) => tasks.AddRange(t))
                .Returns(System.Threading.Tasks.Task.CompletedTask);

            var importAction = new TaskImportAction(loggerFactory, "data/tasks_customfields.csv", false, taskServiceMock.Object);
            await importAction.Execute();

            Assert.AreEqual(4, tasks.Count);
            /*
            var taskDictionary = tasks.GroupBy(task => task.uuid).ToDictionary(group => group.Last().externalId, group => group.Last());
            */
            
            Assert.AreEqual(4, tasks.Count);
            Assert.AreEqual(1, updatedTasks.Count);
            
            var customerATask = tasks[0];
            Assert.AreEqual("Customer A", customerATask.name);
            Assert.IsNull(customerATask.parentExternalId);
            Assert.IsNull(customerATask.externalId);
            Assert.AreEqual(false, customerATask.bookable);
            Assert.AreEqual(false, customerATask.billable);
            Assert.IsNull(customerATask.description);
            Assert.IsNull(customerATask.start);
            Assert.IsNull(customerATask.end);
            Assert.IsNull(customerATask.customField1);
            Assert.IsNull(customerATask.customField2);
            Assert.IsNull(customerATask.customField3);

            var project1Task = updatedTasks[0];
            Assert.AreEqual("Project1", project1Task.name);
            Assert.IsNull(project1Task.parentExternalId);
            Assert.IsNull(project1Task.externalId);
            Assert.AreEqual(customerATask.uuid, project1Task.parentUuid);
            Assert.AreEqual(true, project1Task.bookable);
            Assert.AreEqual(true, project1Task.billable);
            Assert.IsTrue(String.IsNullOrEmpty(project1Task.description));
            Assert.IsNull(project1Task.start);
            Assert.IsNull(project1Task.end);
            Assert.IsEmpty(project1Task.customField1);
            Assert.IsEmpty(project1Task.customField2);
            Assert.IsEmpty(project1Task.customField3);
            
            var task1 = tasks[2];
            Assert.AreEqual("Task1", task1.name);
            Assert.IsNull(task1.parentExternalId);
            Assert.AreEqual(task1.externalId, "A11");
            Assert.AreEqual(true, task1.bookable);
            Assert.AreEqual(false, task1.billable);
            Assert.AreEqual(project1Task.uuid, task1.parentUuid);
            Assert.AreEqual("Awesome", task1.description);
            Assert.IsNull(task1.start);
            Assert.IsNull(task1.end);
            Assert.IsEmpty(task1.customField1);
            Assert.AreEqual("2", task1.customField2);
            Assert.AreEqual("Feld3", task1.customField3);
            
            var project2 = tasks[3];
            Assert.AreEqual("Project2", project2.name);
            Assert.IsNull(project2.parentExternalId);
            Assert.IsNull(project2.externalId);
            Assert.AreEqual(false, project2.bookable);
            Assert.AreEqual(true, project2.billable);
            Assert.AreEqual(project2.parentUuid, customerATask.uuid);
            Assert.IsTrue(String.IsNullOrEmpty(project2.description));
            Assert.AreEqual(project2.start, new DateTime(2019, 05, 16, 0, 0, 0));
            Assert.IsNull(project2.end);
            Assert.AreEqual("1", project2.customField1);
            Assert.IsEmpty(project2.customField2);
            Assert.IsEmpty(project2.customField3);

            /*{
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
            }*/
        }

        [Test]
        public async System.Threading.Tasks.Task TaskCreationUpdate()
        {
            var uuid = Guid.NewGuid().ToString();
            
            var task3 = new Task
            {
                name = "Task3",
                uuid = uuid,
                bookable = false,
                billable = true,
                externalId = "A11"
            };
            var addedTasks = new List<Task>();
            var updatedTasks = new List<Task>();

            var loggerFactory = new LoggerFactory();

            var taskServiceMock = new Mock<ITaskService>(MockBehavior.Strict);
            taskServiceMock
                .Setup(service => service.GetTaskHierarchy())
                .ReturnsAsync(new List<Task> { task3 });
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

            Assert.AreEqual(2, updatedTasks.Count);
            Assert.AreEqual(3, addedTasks.Count);
            
             var customerATask = addedTasks[0];
            Assert.AreEqual("Customer A", customerATask.name);
            Assert.IsNull(customerATask.parentExternalId);
            Assert.IsNull(customerATask.externalId);
            Assert.AreEqual(false, customerATask.bookable);
            Assert.AreEqual(false, customerATask.billable);
            Assert.IsNull(customerATask.description);
            Assert.IsNull(customerATask.start);
            Assert.IsNull(customerATask.end);

            var project1Task = updatedTasks[1];
            Assert.AreEqual("Project1", project1Task.name);
            Assert.IsNull(project1Task.parentExternalId);
            Assert.IsNull(project1Task.externalId);
            Assert.AreEqual(customerATask.uuid, project1Task.parentUuid);
            Assert.AreEqual(true, project1Task.bookable);
            Assert.AreEqual(true, project1Task.billable);
            Assert.IsTrue(String.IsNullOrEmpty(project1Task.description));
            Assert.IsNull(project1Task.start);
            Assert.IsNull(project1Task.end);

            var project2 = addedTasks[2];
            Assert.AreEqual("Project2", project2.name);
            Assert.IsNull(project2.parentExternalId);
            Assert.IsNull(project2.externalId);
            Assert.AreEqual(false, project2.bookable);
            Assert.AreEqual(true, project2.billable);
            Assert.AreEqual(project2.parentUuid, customerATask.uuid);
            Assert.IsTrue(String.IsNullOrEmpty(project2.description));
            Assert.AreEqual(project2.start, new DateTime(2019, 05, 16, 0, 0, 0));
            Assert.IsNull(project2.end);
            
            var task1 = updatedTasks[0];
            Assert.AreEqual("Task1", task1.name);
            Assert.AreEqual("A11", task1.externalId);
            Assert.IsNull(task1.parentExternalId);
            Assert.AreEqual(project1Task.uuid, task1.parentUuid);
            Assert.AreEqual(uuid, task1.uuid);
            Assert.AreEqual(true, task1.bookable);
            Assert.AreEqual(false, task1.billable);
            Assert.AreEqual("Awesome", task1.description);
            Assert.IsNull(task1.start);
            Assert.IsNull(task1.end);
        }
    }
}
