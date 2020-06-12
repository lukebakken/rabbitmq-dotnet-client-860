using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace repro
{
    class Program
    {
        const int threadCount = 32;
        const int count = 20;

        static void Main(string[] args)
        {
            ThreadPool.SetMinThreads(threadCount, threadCount);

            var factory = new ConnectionFactory
            {
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true,
                HostName = "ravel",
                Port = 5672,
                UserName = "guest",
                Password = "guest",
                VirtualHost = "/",
                RequestedConnectionTimeout = new TimeSpan(0, 0, 2),
                UseBackgroundThreadsForIO = false,
                DispatchConsumersAsync = true
            };

            var connection = factory.CreateConnection();

            Console.WriteLine("Creating channels...");
            var totalTicksCounter = Environment.TickCount;

            var tasks = new Task[count];
            for (int i = 0; i < count; ++i)
            {
                tasks[i] = Task.Factory.StartNew(
                    obj => {
                        var ticksCounter = Environment.TickCount;
                        var channel = connection.CreateModel();
                        var cost = Environment.TickCount - ticksCounter;
                        Console.WriteLine($"ID:{(int)obj}, Cost:{cost:N0}ms.");
                    }, i);
            }

            Task.WhenAll(tasks).Wait();
            var totalCost = Environment.TickCount - totalTicksCounter;
            Console.WriteLine($"Done, Total Cost:{totalCost:N0}ms.");

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
