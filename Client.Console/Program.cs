namespace Client.Console
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;

    class Program
    {
        static readonly HttpClient Client = new() { Timeout = TimeSpan.FromMinutes(1) };
        static readonly Random Random = new();
        const string ApiBaseUrl = "http://localhost:5004/api/Order";

        static async Task Main(string[] args)
        {
            while (true)
            {
                Console.Write("Enter # of orders to send, or empty to quit: ");
                var line = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(line))
                    break;

                int limit;
                int loops = 1;
                var segments = line.Split(',');
                if (segments.Length == 2)
                {
                    loops = int.TryParse(segments[1], out int result) ? result : 1;
                    limit = int.TryParse(segments[0], out result) ? result : 1;
                }
                else if (!int.TryParse(line, out limit))
                    limit = 1;

                for (var pass = 0; pass < loops; pass++)
                {
                    var tasks = new List<Task<string>>();

                    for (var i = 0; i < limit; i++)
                    {
                        var order = new OrderModel
                        {
                            Id = Guid.NewGuid(),
                            CustomerNumber = $"CUSTOMER{i}",
                            PaymentCardNumber = i % 4 == 0 ? "5999" : "4000-1234",
                            Notes = new string('*', 1000 * (i + 1))
                        };

                        tasks.Add(Execute(order));
                    }

                    await Task.WhenAll(tasks);

                    Console.WriteLine();
                    Console.WriteLine("Results {0}/{1}", pass + 1, loops);

                    foreach (var task in tasks)
                        Console.WriteLine(task.Result);
                }
            }
        }

        static async Task<string> Execute(OrderModel order)
        {
            try
            {
                var postUrl =
                    $"{ApiBaseUrl}?Id={order.Id:D}&CustomerNumber={Uri.EscapeDataString(order.CustomerNumber)}&PaymentCardNumber={Uri.EscapeDataString(order.PaymentCardNumber)}";

                var responseMessage = await Client.PostAsync(postUrl, null);

                responseMessage.EnsureSuccessStatusCode();

                if (responseMessage.StatusCode == HttpStatusCode.Accepted)
                {
                    await Task.Delay(2000);
                    await Task.Delay(Random.Next(6000));

                    var orderAddress = $"{ApiBaseUrl}?id={order.Id:D}";

                    var patchResponse = await Client.PatchAsync(orderAddress, null);
                    patchResponse.EnsureSuccessStatusCode();

                    do
                    {
                        await Task.Delay(5000);

                        var getResponse = await Client.GetAsync(orderAddress);
                        getResponse.EnsureSuccessStatusCode();

                        var getResult = await getResponse.Content.ReadFromJsonAsync<OrderStatusModel>();
                        if (getResult == null)
                            return $"ORDER: {order.Id:D} STATUS: unknown";

                        if (getResult.State is "Completed" or "Faulted")
                            return $"ORDER: {order.Id:D} STATUS: {getResult.State}";

                        Console.Write(".");
                    }
                    while (true);
                }

                return await responseMessage.Content.ReadAsStringAsync();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return exception.Message;
            }
        }
    }

    public class OrderModel
    {
        public Guid Id { get; set; }
        public string CustomerNumber { get; set; } = string.Empty;
        public string PaymentCardNumber { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public class OrderStatusModel
    {
        public Guid OrderId { get; set; }
        public string State { get; set; } = string.Empty;
    }
}
