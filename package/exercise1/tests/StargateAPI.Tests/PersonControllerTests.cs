using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Queries;
using StargateAPI.Controllers;
using StargateAPI.Business.Dtos;

namespace StargateAPI.Tests
{
    public class PersonControllerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly PersonController _controller;

        public PersonControllerTests()
        {
            _mediator = new Mock<IMediator>();
            _controller = new PersonController(_mediator.Object);
        }

        /// <summary>GetPeople returns OK with list of people.</summary>
        [Fact]
        public async Task GetPeople_ReturnsOk()
        {
            var expected = new GetPeopleResult
            {
                People = new List<PersonAstronaut>
                {
                    new PersonAstronaut { PersonId = 1, Name = "John Doe" }
                }
            };
            _mediator.Setup(m => m.Send(It.IsAny<GetPeople>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _controller.GetPeople();

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
            var response = Assert.IsType<GetPeopleResult>(objectResult.Value);
            Assert.Single(response.People);
        }

        /// <summary>GetPersonByName returns OK with person data.</summary>
        [Fact]
        public async Task GetPersonByName_ReturnsOk()
        {
            var expected = new GetPersonByNameResult
            {
                Person = new PersonAstronaut { PersonId = 1, Name = "John Doe" }
            };
            _mediator.Setup(m => m.Send(It.IsAny<GetPersonByName>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _controller.GetPersonByName("John Doe");

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
        }

        /// <summary>GetPersonByName exception returns 500.</summary>
        [Fact]
        public async Task GetPersonByName_Exception_Returns500()
        {
            _mediator.Setup(m => m.Send(It.IsAny<GetPersonByName>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("DB error"));

            var result = await _controller.GetPersonByName("Nobody");

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
        }

        /// <summary>CreatePerson returns OK with new ID.</summary>
        [Fact]
        public async Task CreatePerson_ReturnsOk()
        {
            var expected = new CreatePersonResult { Id = 42 };
            _mediator.Setup(m => m.Send(It.IsAny<CreatePerson>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _controller.CreatePerson("Jane Doe");

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
        }

        /// <summary>CreatePerson exception returns 500.</summary>
        [Fact]
        public async Task CreatePerson_Exception_Returns500()
        {
            _mediator.Setup(m => m.Send(It.IsAny<CreatePerson>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Duplicate name"));

            var result = await _controller.CreatePerson("John Doe");

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
        }

        /// <summary>GetPeople exception returns 500.</summary>
        [Fact]
        public async Task GetPeople_Exception_Returns500()
        {
            _mediator.Setup(m => m.Send(It.IsAny<GetPeople>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Connection failed"));

            var result = await _controller.GetPeople();

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
        }
    }
}
