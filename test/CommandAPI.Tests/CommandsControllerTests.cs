using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using CommandAPI.Controllers;
using CommandAPI.Models;
using Moq;

namespace CommandAPI.Tests
{
    public class CommandsControllerTests : IDisposable
    {
        DbContextOptionsBuilder<CommandContext> optionsBuilder;
        CommandContext dbContext;
        Mock<IHostingEnvironment> mockEnvironment;
        CommandsController controller;

        public CommandsControllerTests()
        {
            optionsBuilder = new DbContextOptionsBuilder<CommandContext>();
            optionsBuilder.UseInMemoryDatabase("UnitTestInMemBD");
            dbContext = new CommandContext(optionsBuilder.Options);
            mockEnvironment = new Mock<IHostingEnvironment>();
            mockEnvironment.Setup(m => m.EnvironmentName).Returns("UnitTest");
            controller = new CommandsController(dbContext,
            mockEnvironment.Object);
        }

        public void Dispose()
        {
            optionsBuilder = null;
            foreach (var cmd in dbContext.CommandItems)
            {
                dbContext.CommandItems.Remove(cmd);
            }
            dbContext.SaveChanges();
            dbContext.Dispose();
            mockEnvironment = null;
            controller = null;
        }

        //ACTION 1 Tests: GET       /api/commands

        //TEST 1.1 REQUEST OBJECTS WHEN NONE EXIST – RETURN "NOTHING"
        [Fact]
        public void GetCommandItems_ReturnsZeroItems_WhenDBIsEmpty()
        {
            //Arrange            

            //Act
            var result = controller.GetCommandItems();

            //Assert
            Assert.Empty(result.Value);
        }

        //TEST 1.2 REQUEST OBJECTS WHEN ONE EXISTS – RETURN SINGLE OBJECT
        [Fact]
        public void GetCommandItemsReturnsOneItemWhenDBHasOneObject()
        {
            //Arrange
            var command = new Command
            {
                HowTo = "Do Somethting",
                Platform = "Some Platform",
                CommandLine = "Some Command"
            };

            dbContext.CommandItems.Add(command);
            dbContext.SaveChanges();

            //Act
            var result = controller.GetCommandItems();

            //Assert
            Assert.Single(result.Value);
        }

        //TEST 1.3 REQUEST OBJECTS WHEN N EXIST – RETURN N OBJECTS
        [Fact]
        public void GetCommandItemsReturnNItemsWhenDBHasNObjects()
        {
            //Arrange
            var command = new Command
            {
                HowTo = "Do Somethting",
                Platform = "Some Platform",
                CommandLine = "Some Command"
            };
            var command2 = new Command
            {
                HowTo = "Do Somethting",
                Platform = "Some Platform",
                CommandLine = "Some Command"
            };
            dbContext.CommandItems.Add(command);
            dbContext.CommandItems.Add(command2);
            dbContext.SaveChanges();

            //Act
            var result = controller.GetCommandItems();

            //Assert
            Assert.Equal(2, result.Value.Count());
        }

        //TEST 1.4 REQUEST ANY OBJECTS – RETURN CORRECT TYPE
        [Fact]
        public void GetCommandItemsReturnsTheCorrectType()
        {
            //Arrange

            //Act
            var result = controller.GetCommandItems();

            //Assert
            Assert.IsType<ActionResult<IEnumerable<Command>>>(result);
        }

        //END OF ACTION 1 Tests: GET       /api/commands
        //---------------------------------------------------------------

        //ACTION 2 Tests: GET       /api/commands/id

        //TEST 2.1 RESOURCE ID IS INVALID - RETURN NULL
        [Fact]
        public void GetCommandItemReturnsNullResultWhenInvalidID()
        {
            //Arrange
            //DB should be empty, any ID will be invalid

            //Act
            var result = controller.GetCommandItem(0);

            //Assert
            Assert.Null(result.Value);
        }

        //TEST 2.2 RESOURCE ID IS INVALID - RETURN 404 NOT FOUND
        [Fact]
        public void GetCommandItemReturns404NotFoundWhenInvalidID()
        {
            //Arrange
            //DB should be empty, any ID will be invalid

            //Act
            var result = controller.GetCommandItem(0);

            //Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }


        //TEST 2.3 RESOURCE ID IS VALID - CORRECT RETURN TYPE
        [Fact]
        public void GetCommandItemReturnsTheCorrectType()
        {
            //Arrange
            var command = new Command
            {
                HowTo = "Do Somethting",
                Platform = "Some Platform",
                CommandLine = "Some Command"
            };

            dbContext.CommandItems.Add(command);
            dbContext.SaveChanges();

            var cmdId = command.Id;

            //Act
            var result = controller.GetCommandItem(cmdId);

            //Assert
            Assert.IsType<ActionResult<Command>>(result);
        }


        //TEST 2.4 RESOURCE ID IS VALID - CORRECT RESOURCE RETURNED
        [Fact]
        public void GetCommandItemReturnsTheCorrectResouce()
        {
            //Arrange
            var command = new Command
            {
                HowTo = "Do Somethting",
                Platform = "Some Platform",
                CommandLine = "Some Command"
            };

            dbContext.CommandItems.Add(command);
            dbContext.SaveChanges();

            var cmdId = command.Id;

            //Act
            var result = controller.GetCommandItem(cmdId);

            //Assert
            Assert.Equal(cmdId, result.Value.Id);
        }


        //END OF ACTION 2 Tests: GET       /api/commands/id
        //---------------------------------------------------------------


        //ACTION 3 TESTS: POST       /api/commands
        //TEST 3.1 VALID OBJECT SUBMITTED – OBJECT COUNT INCREMENTS BY 1
        [Fact]
        public void PostCommandItemObjectCountIncrementWhenValidObject()
        {
            //Arrange
            var command = new Command
            {
                HowTo = "Do Somethting",
                Platform = "Some Platform",
                CommandLine = "Some Command"
            };
            var oldCount = dbContext.CommandItems.Count();

            //Act
            var result = controller.PostCommandItem(command);

            //Assert
            Assert.Equal(oldCount + 1, dbContext.CommandItems.Count());
        }

        //TEST 3.2 VALID OBJECT SUBMITTED – 201 CREATED RETURN CODE
        [Fact]
        public void PostCommandItemReturns201CreatedWhenValidObject()
        {
            //Arrange
            var command = new Command
            {
                HowTo = "Do Somethting",
                Platform = "Some Platform",
                CommandLine = "Some Command"
            };

            //Act
            var result = controller.PostCommandItem(command);

            //Assert
            Assert.IsType<CreatedAtActionResult>(result.Result);
        }

        //END OF ACTION 3 Tests: POST       /api/commands
        //---------------------------------------------------------------

        //ACTION 4 TESTS: PUT       /api/commands/id

        //TEST 4.1 VALID OBJECT SUBMITTED – FIELD IS UPDATED
        [Fact]
        public void PutCommandItem_AttributeUpdated_WhenValidObject()
        {
            //Arrange
            var command = new Command
            {
                HowTo = "Do Somethting",
                Platform = "Some Platform",
                CommandLine = "Some Command"
            };
            dbContext.CommandItems.Add(command);
            dbContext.SaveChanges();

            var cmdId = command.Id;

            command.HowTo = "UPDATED";

            //Act
            controller.PutCommandItem(cmdId, command);
            var result = dbContext.CommandItems.Find(cmdId);

            //Assert
            Assert.Equal(command.HowTo, result.HowTo);
        }

        //TEST 4.2 VALID OBJECT SUBMITTED – 204 NO CONTENT RETURN CODE
        [Fact]
        public void PutCommandItem_Returns204_WhenValidObject()
        {
            //Arrange
            var command = new Command
            {
                HowTo = "Do Somethting",
                Platform = "Some Platform",
                CommandLine = "Some Command"
            };
            dbContext.CommandItems.Add(command);
            dbContext.SaveChanges();

            var cmdId = command.Id;

            command.HowTo = "UPDATED";

            //Act
            var result = controller.PutCommandItem(cmdId, command);

            //Assert
            Assert.IsType<NoContentResult>(result);
        }

        //TEST 4.3 INVALID OBJECT SUBMITTED – 400 BAD REQUEST RETURN CODE
        [Fact]
        public void PutCommandItem_Returns400_WhenInvalidObject()
        {
            //Arrange
            var command = new Command
            {
                HowTo = "Do Somethting",
                Platform = "Some Platform",
                CommandLine = "Some Command"
            };
            dbContext.CommandItems.Add(command);
            dbContext.SaveChanges();

            var cmdId = command.Id + 1;

            command.HowTo = "UPDATED";

            //Act
            var result = controller.PutCommandItem(cmdId, command);

            //Assert
            Assert.IsType<BadRequestResult>(result);
        }

        //TEST 4.4 INVALID OBJECT SUBMITTED – OBJCT REMAINS UNCHANGED
        [Fact]
        public void PutCommandItem_AttributeUnchanged_WhenInvalidObject()
        {
            //Arrange
            var command = new Command
            {
                HowTo = "Do Somethting",
                Platform = "Some Platform",
                CommandLine = "Some Command"
            };
            dbContext.CommandItems.Add(command);
            dbContext.SaveChanges();

            var command2 = new Command
            {
                Id = command.Id,
                HowTo = "UPDATED",
                Platform = "UPDATED",
                CommandLine = "UPDATED"
            };

            //Act
            controller.PutCommandItem(command.Id + 1, command2);
            var result = dbContext.CommandItems.Find(command.Id);

            //Assert
            Assert.Equal(command.HowTo, result.HowTo);
        }



        //END OF ACTION 4 Tests: PUT       /api/commands/id
        //---------------------------------------------------------------

        //TEST 5.1 VALID OBJECT ID SUBMITTED – OBJECT DECREMENTS BY 1
        [Fact]
        public void DeleteCommandItem_ObjectsDecrement_WhenValidObjectID()
        {
            //Arrange
            var command = new Command
            {
                HowTo = "Do Somethting",
                Platform = "Some Platform",
                CommandLine = "Some Command"
            };
            dbContext.CommandItems.Add(command);
            dbContext.SaveChanges();

            var cmdId = command.Id;
            var objCount = dbContext.CommandItems.Count();

            //Act
            controller.DeleteCommandItem(cmdId);

            //Assert
            Assert.Equal(objCount - 1, dbContext.CommandItems.Count());
        }

        //TEST 5.2 VALID OBJECT ID SUBMITTED – 200 OK RETURN CODE
        [Fact]
        public void DeleteCommandItem_Returns200OK_WhenValidObjectID()
        {
            //Arrange
            var command = new Command
            {
                HowTo = "Do Somethting",
                Platform = "Some Platform",
                CommandLine = "Some Command"
            };
            dbContext.CommandItems.Add(command);
            dbContext.SaveChanges();

            var cmdId = command.Id;

            //Act
            var result = controller.DeleteCommandItem(cmdId);

            //Assert
            Assert.Null(result.Result);
        }

        //TEST 5.3 INVALID OBJECT ID SUBMITTED – 404 NOT FOUND RETURN CODE
        [Fact]
        public void DeleteCommandItem_Returns404NotFound_WhenValidObjectID()
        {
            //Arrange

            //Act
            var result = controller.DeleteCommandItem(-1);

            //Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        //TEST 5.4 INVALID OBJECT ID SUBMITTED – OBJECT COUNT REMAINS UNCHANGED
        [Fact]
        public void DeleteCommandItem_ObjectCountNotDecremented_WhenValidObjectID()
        {
            //Arrange
            var command = new Command
            {
                HowTo = "Do Somethting",
                Platform = "Some Platform",
                CommandLine = "Some Command"
            };
            dbContext.CommandItems.Add(command);
            dbContext.SaveChanges();

            var cmdId = command.Id;
            var objCount = dbContext.CommandItems.Count();

            //Act
            var result = controller.DeleteCommandItem(cmdId + 1);

            //Assert
            Assert.Equal(objCount, dbContext.CommandItems.Count());
        }

        //ACTION 5 TESTS: DELETE       /api/commands/id

        //END OF ACTION 5 TESTS: DELETE       /api/commands/id
        //---------------------------------------------------------------
    }
}
