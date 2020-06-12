using System;
using System.Linq;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace repro
{
    class Program {
        static void Main(string[] args) {
            // connects to RabbitMQ
            var factory = new ConnectionFactory {
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true,
                HostName = "127.0.0.1",
                Port = 5672,
                UserName = "user",
                Password = "password",
                VirtualHost = "/",
                RequestedConnectionTimeout = new TimeSpan(0, 0, 2),
                UseBackgroundThreadsForIO = false,
                DispatchConsumersAsync = true
            };

            var connection = factory.CreateConnection();

            // creates channels
            Console.WriteLine("Creating channels...");
            var totalTicksCounter = Environment.TickCount;

            /*Creating channels synchronously works fine with 6.0.0 and 6.1.0.
             *Creating channels asynchronously is very slow with 6.1.0. */
            var tasks = Enumerable.Range(0, 20).Select(
                i => Task.Factory.StartNew(
                    obj => {
                        var ticksCounter = Environment.TickCount;
                        var channel = connection.CreateModel();
                        var cost = Environment.TickCount - ticksCounter;
                        Console.WriteLine($"ID:{(int)obj}, Cost:{cost:N0}ms.");
                    },
                    i));

            // waits for completion
            Task.WhenAll(tasks).Wait();
            var totalCost = Environment.TickCount - totalTicksCounter;
            Console.WriteLine($"Done, Total Cost:{totalCost:N0}ms.");

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
