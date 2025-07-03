using ProtonDB.Service;
Host.CreateDefaultBuilder(args)
    .UseWindowsService()
    .ConfigureServices(services => {
        services.AddHostedService<Worker>();
    })
    .Build()
    .Run();
