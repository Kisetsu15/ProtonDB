using ProtonDB.Server;
namespace ProtonDB.Service;

public class Worker : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        var server = new QueryServer(9090);
        await server.StartAsync();
    }
}