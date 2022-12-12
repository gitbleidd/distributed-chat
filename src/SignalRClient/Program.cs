using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;

namespace SignalRClient
{
    public class Message
    {
        public int Id { get; set; }
        public string User { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime? CreatedDate { get; set; } = DateTime.Now;
    }

    internal class Program
    {
        static async Task Main(string[] args)
        {
            var task = new Task<Task<(int, string, bool)>>(async () => { await Task.Delay(100); return (0, "", true); });

            Console.WriteLine("Input port:");
            string port = Console.ReadLine();


            HubConnection connection = new HubConnectionBuilder()
                .WithUrl($"http://localhost:{port}/chathub")
                .Build();


            connection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                var newMessage = $"{user}: {message}";
                Console.WriteLine(newMessage);
            });

            connection.On<List<Message>>("ReceiveHistory", (messages) =>
            {
                foreach (var m in messages)
                {
                    Console.WriteLine($"{m.User}: {m.Content}");
                }
            });

            connection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await connection.StartAsync();
            };

            try
            {
                var tokenSource = new CancellationTokenSource();
                tokenSource.CancelAfter(TimeSpan.FromMilliseconds(1));
                CancellationToken ct = tokenSource.Token;

                await connection.StartAsync(ct);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex?.InnerException.Message);
                //Console.WriteLine(ex.StackTrace);
                Console.ReadLine();
                return;
            }

            while (true)
            {
                Console.Write("Me: ");
                string userMessage = Console.ReadLine();

                if (userMessage == "h")
                {
                    await connection.InvokeAsync("GetHistory");
                    continue;
                }

                try
                {
                    await connection.InvokeAsync("SendMessage", Guid.NewGuid().ToString(), userMessage);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}