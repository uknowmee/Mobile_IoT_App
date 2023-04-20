using Servers.DbManagers;
using Servers.Listeners;
using Servers.ProxyMaker;
using Servers.Validators;

namespace Servers;

internal static class ServerStarter
{
    private static SrvDbManager MakeDb()
    {
        SrvDbManager dbManager = new SrvDbManager();
        if (!dbManager.EnsureCreated())
        {
            Console.Out.WriteLine("Database found.");
        }
        else
        {
            Console.Out.WriteLine("Database not found, creating...");
            dbManager.InitialInserts();
        }

        return dbManager;
    }

    private static ListenersManager SaveMqttUpdates(SrvDbManager dbManager)
    {
        ListenersManager listenersManager = new ListenersManager(dbManager);
        return listenersManager;
    }

    private static void MakeRest(string[] args, SrvDbManager dbManager, ListenersManager listenersManager)
    {
        var builder = WebApplication.CreateBuilder(args);

//      Add services to the container.
        builder.Services.AddSingleton(listenersManager);
        builder.Services.AddSingleton(dbManager);
        builder.Services.AddSingleton(new SessionTokenValidator(dbManager));
        builder.Services.AddSingleton(new ProxyManager(dbManager));
        
        builder.Services.AddSingleton<EmailValidator>();
        builder.Services.AddSingleton<PassValidator>();

        builder.Services.AddControllers();

//      Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();
//      Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }

    private static void RunTests()
    {
        
    }
    
    private static void Main(string[] args)
    {
        try
        {
            SrvDbManager dbManager = MakeDb();
            ListenersManager listenersManager = SaveMqttUpdates(dbManager);
            MakeRest(args, dbManager, listenersManager);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}