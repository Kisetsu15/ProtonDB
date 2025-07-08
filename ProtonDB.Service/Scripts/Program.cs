using ProtonDB.Service;

/// <summary>
/// Entry point for the ProtonDB Worker Service application.
/// </summary>
/// <remarks>
/// Configures and runs the .NET Worker Service as a Windows Service.
/// Registers the <see cref="Worker"/> as a hosted background service.
/// </remarks>
Host.CreateDefaultBuilder(args)
    .UseWindowsService()
    .ConfigureServices(services => {
        services.AddHostedService<Worker>();
    })
    .Build()
    .Run();