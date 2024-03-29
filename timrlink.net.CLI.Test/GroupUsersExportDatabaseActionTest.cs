using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using timrlink.net.CLI.Actions;
using timrlink.net.Core.API;
using timrlink.net.Core.Service;
using API = timrlink.net.Core.API;
using Task = System.Threading.Tasks.Task;

namespace timrlink.net.CLI.Test
{
    public class GroupUsersDatabaseExportActionTest
    {
        private class UserGroup
        {
            public API.Group group;
            public List<User> users;
        }
        
        [Test]
        public async Task GroupUsersDatabaseExportAction()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

            var groupServiceMock = new Mock<IGroupService>(MockBehavior.Loose);
            var options = InMemorySqLiteContextOptions();

            // Setup
            {
                var group1 = new API.Group
                {
                    description = "Series with airplanes for kids",
                    externalId = "99C4",
                    name = "Super Wings",
                    parentExternalId = "77C1"
                };

                var group2 = new API.Group
                {
                    description = "Nice animation series kids",
                    externalId = "77C8",
                    name = "Paw Patrol",
                    parentExternalId = "77C1"
                };

                var user1 = new User() { uuid = "9d33c475-0da2-4b21-95b9-feca948cc80a" };
                var user2 = new User() { uuid = "80814e96-aa2b-4bbe-a7fe-ba25f3b60e2e" };
                var user3 = new User() { uuid = "88a6ead0-4731-40e1-a5de-d873f094cace" };
                var user4 = new User() { uuid = "e25a59c8-d4de-4531-aae0-241a8c1c7115" };
                var user5 = new User() { uuid = "27da471e-9e72-4c3d-bc52-000c926d1ff9" };
                
                var userGroup1 = new UserGroup()
                {
                    group = group1,
                    users = new List<User> {user1, user2, user3}
                };
                
                var userGroup2 = new UserGroup()
                {
                    group = group2,
                    users = new List<User> {user3, user4, user5}
                };

                var memoryContext = new DatabaseContext(options);
                await memoryContext.Database.EnsureCreatedAsync();
                
                BuildGroupService(groupServiceMock, userGroup1, userGroup2);
            }

            // Test group users action
            {
                var memoryContext = new DatabaseContext(options);
                var groupUsersAction = new GroupUsersDatabaseExportAction(loggerFactory, memoryContext, groupServiceMock.Object);
                await groupUsersAction.Execute();
            }
            
            // Verification
            {
                var memoryContext = new DatabaseContext(options);
                var group1 = memoryContext.Group.Single(g => g.ExternalId == "99C4");

                Assert.AreEqual("Series with airplanes for kids", group1.Description);
                Assert.AreEqual("Super Wings", group1.Name);
                Assert.AreEqual("77C1", group1.ParentalExternalId);

                var group2 = memoryContext.Group.Single(g => g.ExternalId == "77C8");

                Assert.AreEqual("Nice animation series kids", group2.Description);
                Assert.AreEqual("Paw Patrol", group2.Name);
                Assert.AreEqual("77C1", group2.ParentalExternalId);

                var group1User1 =
                    memoryContext.GroupUsers.Single(gu => gu.UserUUID == "9d33c475-0da2-4b21-95b9-feca948cc80a");
                Assert.AreEqual(1, group1User1.GroupId);

                var group1User2 =
                    memoryContext.GroupUsers.Single(gu => gu.UserUUID == "80814e96-aa2b-4bbe-a7fe-ba25f3b60e2e");
                Assert.AreEqual(1, group1User2.GroupId);

                var groupUser3 = memoryContext.GroupUsers
                    .Where(gu => gu.UserUUID == "88a6ead0-4731-40e1-a5de-d873f094cace")
                    .OrderBy(gu => gu.GroupId)
                    .ToList();
                Assert.AreEqual(2, groupUser3.Count());
                Assert.AreEqual(1, groupUser3.ElementAt(0).GroupId);
                Assert.AreEqual(2, groupUser3.ElementAt(1).GroupId);

                var group2User4 =
                    memoryContext.GroupUsers.Single(gu => gu.UserUUID == "e25a59c8-d4de-4531-aae0-241a8c1c7115");
                Assert.AreEqual(2, group2User4.GroupId);

                var group2User5 =
                    memoryContext.GroupUsers.Single(gu => gu.UserUUID == "27da471e-9e72-4c3d-bc52-000c926d1ff9");
                Assert.AreEqual(2, group2User5.GroupId);
            }
        }
        
        [Test]
        public async Task UpdateGroupUsersDatabaseExportAction()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
            
            var groupServiceMock = new Mock<IGroupService>(MockBehavior.Loose);
            var options = InMemorySqLiteContextOptions();

            // Setup
            {
                var memoryContext = new DatabaseContext(options);
                await memoryContext.Database.EnsureCreatedAsync();

                // Relation to user1
                var group1User1 = new GroupUsers()
                {
                    UserUUID =  "9d33c475-0da2-4b21-95b9-feca948cc80a",
                    GroupId = 17
                };
                
                // Relation to user2
                var group1Users2 = new GroupUsers()
                {
                    UserUUID =  "88a6ead0-4731-40e1-a5de-d873f094cace",
                    GroupId = 17
                };
                
                var group1Database = new Group()
                {
                    Id = 17,
                    Description = "Superheroes of Paw Patrol",
                    ExternalId = "77C8",
                    Name = "Mighty Pups",
                    ParentalExternalId = "77C2",
                };
                
                memoryContext.Add(group1Database);
                memoryContext.Add(group1User1);
                memoryContext.Add(group1Users2);
                
                
                // Relation to user3
                var group2User3 = new GroupUsers()
                {
                    UserUUID =  "e25a59c8-d4de-4531-aae0-241a8c1c7115",
                    GroupId = 18
                };
                
                // Relation to user4
                var group2Users4 = new GroupUsers()
                {
                    UserUUID =  "27da471e-9e72-4c3d-bc52-000c926d1ff9",
                    GroupId = 18
                };
                
                var group2 = new Group()
                {
                    Id = 18,
                    Description = "Nice series with airplanes for kids",
                    ExternalId = "99A5",
                    Name = "Superwings",
                    ParentalExternalId = "77C2",
                };
                
                memoryContext.Add(group2);
                memoryContext.Add(group2User3);
                memoryContext.Add(group2Users4);
                
                await memoryContext.SaveChangesAsync();
                
                var group1 = new API.Group
                {
                    description = "Nice animation series kids",
                    externalId = "77C8",
                    name = "Paw Patrol",
                    parentExternalId = "77C1"
                };
                
                var user1 = new User() { uuid = "9d33c475-0da2-4b21-95b9-feca948cc80a" };
                
                var userGroup1 = new UserGroup()
                {
                    group = group1,
                    users = new List<User>(){ user1 }
                };
                
                BuildGroupService(groupServiceMock, userGroup1);
            }

            // Test group users action
            {
                var memoryContext = new DatabaseContext(options);
                var groupUsersAction = new GroupUsersDatabaseExportAction(loggerFactory, memoryContext, groupServiceMock.Object);
                await groupUsersAction.Execute();
            }

            // Verification
            {
                var memoryContext = new DatabaseContext(options);
                var group1 = memoryContext.Group.Single(g => g.ExternalId == "77C8");
                
                Assert.AreEqual("Nice animation series kids", group1.Description);
                Assert.AreEqual("Paw Patrol", group1.Name);
                Assert.AreEqual("77C1", group1.ParentalExternalId);
                Assert.AreEqual(17, group1.Id);
                
                var group2 = memoryContext.Group.SingleOrDefault(g => g.ExternalId == "99A5");
                
                // Verify if 2nd group got deleted
                Assert.Null(group2);
                
                // There should be only one GroupUser entry left, because Group 18 got deleted and in Group 17
                // there is only one user left
                var group1User1 =  memoryContext.GroupUsers.Single();
                
                Assert.AreEqual(17, group1User1.GroupId);
                Assert.AreEqual("9d33c475-0da2-4b21-95b9-feca948cc80a", group1User1.UserUUID);
            }
        }

        private void BuildGroupService(Mock<IGroupService> groupServiceMock, params UserGroup[] userGroups)
        {
            var groups = userGroups.Select(ug => ug.group).ToList();
            
            groupServiceMock
                .Setup(service => service.GetGroups())
                .ReturnsAsync(groups.ToList());
            
            groupServiceMock
                .Setup(service => service.FlattenGroups(It.IsAny<IList<API.Group>>()))
                .Returns(groups);

            foreach (var userGroup in userGroups)
            {
                groupServiceMock
                    .Setup(service => service.GetGroupUsers(userGroup.group))
                    .ReturnsAsync(userGroup.users);
            }
        }
        
        private static DbContextOptions InMemorySqLiteContextOptions()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            // Microsoft In-Memory DB doesn't enforce constraints
            return new DbContextOptionsBuilder()
                .UseSqlite(connection)
                .Options;
        }
    }
}