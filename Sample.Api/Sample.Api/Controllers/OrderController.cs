using MassTransit;
using MassTransit.Transports;
using Microsoft.AspNetCore.Mvc;
using Sample.Components.Consumers;
using Sample.Contracts;

namespace Sample.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
    readonly ILogger<OrderController> _logger;
        readonly IRequestClient<SubmitOrder> _submitOrderRequestClient;
        readonly ISendEndpointProvider _sendEndpointProvider;
        readonly IRequestClient<CheckOrder> _checkOrderClient;
        readonly IPublishEndpoint _publishEndpoint;

        public OrderController(ILogger<OrderController> logger, IRequestClient<SubmitOrder> submitOrderRequestClient,
            ISendEndpointProvider sendEndpointProvider, IRequestClient<CheckOrder> checkOrderClient, IPublishEndpoint publishEndpoint)
        {
            _logger = logger;
            _submitOrderRequestClient = submitOrderRequestClient;
            _sendEndpointProvider = sendEndpointProvider;
            _checkOrderClient = checkOrderClient;
            _publishEndpoint = publishEndpoint;
        }

      

        [HttpGet]
        public async Task<IActionResult> Get(Guid id)
        {

            var (status, notFound) = await _checkOrderClient.GetResponse<OrderStatus, OrderNotFound>(new { OrderId = id });

            if (status.IsCompletedSuccessfully)
            {
                var response = await status;
                return Ok(response.Message);
            }
            else
            {
                var response = await notFound;
                return NotFound(response.Message);
            }
        }


        [HttpPost]
        public async Task<IActionResult> Post(Guid Id,string CustomerNumber,string PaymentCardNumber,string? Notes)
        {
            var (accepted,reject)  = await _submitOrderRequestClient.GetResponse<OrderSubmisionAccepted,OrderSubmisionRejected>(new
            {
                OrderId=Id,
                CustomerNumber= CustomerNumber,
                InVar.Timestamp,
                PaymentCardNumber = PaymentCardNumber,
                Notes=Notes 
            });

            if (accepted.IsCompleted)
            {
                var response = await accepted;
               
                return Accepted(response);
            }
            else
            {
                var response = await reject;
                return  BadRequest(response.Message);

            }
        }

        [HttpPatch]
        public async Task<IActionResult> Patch(Guid id)
        {
            await _publishEndpoint.Publish<OrderAccepted>(new
            {
                OrderId = id,
                InVar.Timestamp,
            });

            return Accepted();
        }

        [HttpPut]
        public async Task<IActionResult> Put(Guid id, string customerNumber)
        {
            var endpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri("queue:submit-order"));

            await endpoint.Send<SubmitOrder>(new
            {
                OrderId = id,
                InVar.Timestamp,
                CustomerNumber = customerNumber
            });

            return Accepted();
        }

    }
}
