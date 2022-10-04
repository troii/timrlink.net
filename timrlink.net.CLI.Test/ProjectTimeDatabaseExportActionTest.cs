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
    public class ProjectTimeDatabaseExportActionTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async System.Threading.Tasks.Task FromDateNullAndToDateValid()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
            
            var tasks = new List<Task>();

            var timrSyncMock = new Mock<TimrSync>(MockBehavior.Strict);
            var projectTimeServiceMock = new Mock<IProjectTimeService>(MockBehavior.Loose);
            var userServiceMock = new Mock<IUserService>(MockBehavior.Strict);

            var taskService =
                new TaskService(loggerFactory.CreateLogger<TaskService>(), loggerFactory, timrSyncMock.Object);

            var importAction = new ProjectTimeDatabaseExportAction(loggerFactory, "test", "2022-10-01 12:01", to: null, userServiceMock.Object, taskService, projectTimeServiceMock.Object);
            Assert.ThrowsAsync<ArgumentException>(() => importAction.Execute());
        }
        
        [Test]
        public async System.Threading.Tasks.Task FromDateValidAndToDateNull()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
            
            var tasks = new List<Task>();

            var timrSyncMock = new Mock<TimrSync>(MockBehavior.Strict);
            var projectTimeServiceMock = new Mock<IProjectTimeService>(MockBehavior.Loose);
            var userServiceMock = new Mock<IUserService>(MockBehavior.Strict);

            var taskService =
                new TaskService(loggerFactory.CreateLogger<TaskService>(), loggerFactory, timrSyncMock.Object);

            var importAction = new ProjectTimeDatabaseExportAction(loggerFactory, "test", null, to: "2022-10-01 12:01", userServiceMock.Object, taskService, projectTimeServiceMock.Object);
            Assert.ThrowsAsync<ArgumentException>(() => importAction.Execute());
        }
        
        [Test]
        public async System.Threading.Tasks.Task FromDateValidAndToDateInvalid()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
            
            var tasks = new List<Task>();

            var timrSyncMock = new Mock<TimrSync>(MockBehavior.Strict);
            var projectTimeServiceMock = new Mock<IProjectTimeService>(MockBehavior.Loose);
            var userServiceMock = new Mock<IUserService>(MockBehavior.Strict);

            var taskService =
                new TaskService(loggerFactory.CreateLogger<TaskService>(), loggerFactory, timrSyncMock.Object);

            var importAction = new ProjectTimeDatabaseExportAction(loggerFactory, "test", "2022-10-01 09:01", to: "2022-10-01 12:9999", userServiceMock.Object, taskService, projectTimeServiceMock.Object);
            Assert.ThrowsAsync<ArgumentException>(() => importAction.Execute());
        }
        
        [Test]
        public async System.Threading.Tasks.Task FromDateInvalidAndToDateValid()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
            
            var tasks = new List<Task>();

            var timrSyncMock = new Mock<TimrSync>(MockBehavior.Strict);
            var projectTimeServiceMock = new Mock<IProjectTimeService>(MockBehavior.Loose);
            var userServiceMock = new Mock<IUserService>(MockBehavior.Strict);

            var taskService =
                new TaskService(loggerFactory.CreateLogger<TaskService>(), loggerFactory, timrSyncMock.Object);

            var importAction = new ProjectTimeDatabaseExportAction(loggerFactory, "test", "2022-10-01 09:9999", to: "2022-10-01 12:00", userServiceMock.Object, taskService, projectTimeServiceMock.Object);
            Assert.ThrowsAsync<ArgumentException>(() => importAction.Execute());
        }
        
        [Test]
        public async System.Threading.Tasks.Task FromDateValidAndToDateValid()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
            
            var tasks = new List<Task>();

            var timrSyncMock = new Mock<TimrSync>(MockBehavior.Strict);
            var projectTimeServiceMock = new Mock<IProjectTimeService>(MockBehavior.Loose);
            var userServiceMock = new Mock<IUserService>(MockBehavior.Strict);

            var taskService =
                new TaskService(loggerFactory.CreateLogger<TaskService>(), loggerFactory, timrSyncMock.Object);

            var importAction = new ProjectTimeDatabaseExportAction(loggerFactory, "test", "2022-10-01 09:10", to: "2022-10-01 12:00", userServiceMock.Object, taskService, projectTimeServiceMock.Object);
            await importAction.Execute();
        }
        
        [Test]
        public async System.Threading.Tasks.Task FromDateAferToDate()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
            
            var tasks = new List<Task>();

            var timrSyncMock = new Mock<TimrSync>(MockBehavior.Strict);
            var projectTimeServiceMock = new Mock<IProjectTimeService>(MockBehavior.Loose);
            var userServiceMock = new Mock<IUserService>(MockBehavior.Strict);

            var taskService =
                new TaskService(loggerFactory.CreateLogger<TaskService>(), loggerFactory, timrSyncMock.Object);

            var importAction = new ProjectTimeDatabaseExportAction(loggerFactory, "test", "2022-10-02 09:10", to: "2022-10-01 12:00", userServiceMock.Object, taskService, projectTimeServiceMock.Object);
            Assert.ThrowsAsync<ArgumentException>(() => importAction.Execute());
        }
    }
}