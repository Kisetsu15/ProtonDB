using ProtonDB.Server;
using ProtonDB.Server.Core;
namespace ProtonDB.Service;

public class Worker : BackgroundService{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        Meta.Initialize();
        var server = new QueryServer(9090);
        await server.StartAsync();
    }
}