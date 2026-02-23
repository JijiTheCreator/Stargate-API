using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Queries;
using StargateAPI.Controllers;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;

namespace StargateAPI.Tests
{
    public class AstronautDutyControllerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly AstronautDutyController _controller;

        public AstronautDutyControllerTests()
        {
            _mediator = new Mock<IMediator>();
            _controller = new AstronautDutyController(_mediator.Object);
        }

        /// <summary>BUG-1: GET dispatches GetAstronautDutiesByName (not GetPersonByName).</summary>
        [Fact]
        public async Task GetAstronautDutiesByName_DispatchesCorrectQuery_BUG1()
        {
            var expected = new GetAstronautDutiesByNameResult
            {
                Person = new PersonAstronaut { PersonId = 1, Name = "John Doe" },
                AstronautDuties = new List<AstronautDuty>()
            };

            _mediator.Setup(m => m.Send(It.IsAny<GetAstronautDutiesByName>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _controller.GetAstronautDutiesByName("John Doe");

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);

            // Verify GetAstronautDutiesByName was dispatched (not GetPersonByName)
            _mediator.Verify(m => m.Send(
                It.Is<GetAstronautDutiesByName>(q => q.Name == "John Doe"),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>GET exception returns 500.</summary>
        [Fact]
        public async Task GetAstronautDutiesByName_Exception_Returns500()
        {
            _mediator.Setup(m => m.Send(It.IsAny<GetAstronautDutiesByName>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Not found"));

            var result = await _controller.GetAstronautDutiesByName("Nobody");

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
        }

        /// <summary>BUG-4: POST has try-catch wrapper returning structured errors.</summary>
        [Fact]
        public async Task CreateAstronautDuty_ReturnsOk_BUG4()
        {
            var expected = new CreateAstronautDutyResult { Id = 99 };
            _mediator.Setup(m => m.Send(It.IsAny<CreateAstronautDuty>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var request = new CreateAstronautDuty
            {
                Name = "John Doe",
                Rank = "CPT",
                DutyTitle = "Pilot",
                DutyStartDate = DateTime.Today
            };

            var result = await _controller.CreateAstronautDuty(request);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
        }

        /// <summary>BUG-4: POST exception returns 500 (not unhandled).</summary>
        [Fact]
        public async Task CreateAstronautDuty_Exception_Returns500_BUG4()
        {
            _mediator.Setup(m => m.Send(It.IsAny<CreateAstronautDuty>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Validation failed"));

            var request = new CreateAstronautDuty
            {
                Name = "Nobody",
                Rank = "PVT",
                DutyTitle = "Test",
                DutyStartDate = DateTime.Today
            };

            var result = await _controller.CreateAstronautDuty(request);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
            var response = Assert.IsType<BaseResponse>(objectResult.Value);
            Assert.False(response.Success);
        }
    }
}
