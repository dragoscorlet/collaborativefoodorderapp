namespace OfficeLunch.Server

open Microsoft.AspNetCore
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Bolero.Remoting
open Bolero.Remoting.Server
open OfficeLunch
open Bolero.Templating.Server
open Microsoft.AspNetCore.SignalR
open Microsoft.Extensions.Hosting
open System.Threading
open System.Threading.Tasks
type SRHub () = 
    inherit Hub ()

    member x.sendMessage value = x.Clients.All.SendAsync("Recieve", value)

type SRService (hubContext :IHubContext<SRHub>) =
  inherit BackgroundService ()
  
  member this.HubContext :IHubContext<SRHub> = hubContext

  override this.ExecuteAsync (stoppingToken :CancellationToken) =
    //let pingTimer = new System.Timers.Timer(100.0)
    //pingTimer.Elapsed.Add(fun v -> 
    //  this.HubContext.Clients.All.SendAsync("sendMessage",v.SignalTime.Second) |> ignore)

    //pingTimer.Start()
    Task.CompletedTask


type Startup() =

    member this.ConfigureServices(services: IServiceCollection) =
        services.AddMvcCore() |> ignore
        services.AddSignalR() |> ignore
        services.AddHostedService<SRService>() |> ignore
        services
            .AddAuthorization()
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie()
                .Services
            .AddRemoting<RestaurantService>()
            .AddRemoting<UserService>()
#if DEBUG
            .AddHotReload(templateDir = __SOURCE_DIRECTORY__ + "/../OfficeLunch.Client")

#endif
        |> ignore

    member this.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
        app
            .UseAuthentication()
            .UseRemoting()
            .UseClientSideBlazorFiles<Client.Startup>()
            .UseRouting()
            .UseEndpoints(fun endpoints ->
#if DEBUG
                endpoints.UseHotReload()
#endif
                endpoints.MapHub<SRHub>("/SRHub") |> ignore
                endpoints.MapDefaultControllerRoute() |> ignore
                endpoints.MapFallbackToClientSideBlazor<Client.Startup>("index.html") |> ignore)
                
        |> ignore

module Program =

    [<EntryPoint>]
    let main args =
        WebHost
            .CreateDefaultBuilder(args)
            .UseStartup<Startup>()
            .Build()
            .Run()
        0
