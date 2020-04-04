module OfficeLunch.Client.Main

open System
open Elmish
open Bolero
open Bolero.Html
open Bolero.Remoting
open Bolero.Remoting.Client
open Bolero.Templating.Client
open Blazor.Extensions
open Microsoft.AspNetCore.Components 
open OfficeLunch.DomainModels.PollTypes
open ApplicationModel
open Messages
open LogInPage

let initModel =
    {
        page = CityPicker
        error = None
        username = "" 
        password = ""
        signedInAs = None
        signInFailed = false
        suggestInput = ""
        restaurantSearchResult = None
        poll = 
            {   
                id = Guid.Empty
                options = []
                owner = {
                    name = ""
                    id = Guid.Empty}
                endTime = DateTime.Now
                status = PollStatus.Created
                title = String.Empty
            }
        pollAddress = String.Empty
        cities = []
    }

let update restaurantRemote userRemote message model =
    match message with
    | PollMakerMsg pollMsg -> 
        let (newModel, newCmd) = (PollMakerPage.update userRemote pollMsg model)
        newModel, Cmd.map PollMakerMsg newCmd
    | CitiyPickerMsg cityMsg ->
        let (newModel, newCmd) = (CityPickerPage.update restaurantRemote cityMsg model)
        newModel, Cmd.map CitiyPickerMsg newCmd
    | LogInMsg logIn ->
        let (newModel, newCmd) = (LogInPage.update userRemote logIn model)
        newModel, Cmd.map LogInMsg newCmd
    | PollVotingMsg voteMsg ->
        let (newModel, newCmd) = (PollVotingPage.update userRemote voteMsg model)
        newModel, Cmd.map PollVotingMsg newCmd
    | SetPage page ->
        { model with page = page }, Cmd.none   
    | Error RemoteUnauthorizedException ->
        { model with error = Some "You have been logged out."; signedInAs = None }, Cmd.none
    | Error exn ->
        { model with error = Some exn.Message }, Cmd.none
    | ClearError ->
        { model with error = None }, Cmd.none

/// Connects the routing system to the Elmish application.
let router = Router.infer SetPage (fun model -> model.page)

type Main = Template<"wwwroot/main.html">


let menuItem (model: Model) (page: Page) (text: string) =
    Main.MenuItem()
        .Active(if model.page = page then "is-active" else "")
        .Url(router.Link page)
        .Text(text)
        .Elt()


let view model dispatch =
    Main()
        .Menu(concat [
            menuItem model (PollMaker "tt") "Search for restaurants"
            menuItem model CityPicker "PickCity"
            menuItem model LogInPage "LogIn"

        ])
        .Body(
            cond model.page <| function
            | PollAddress -> PollAddressPage.render router.Link model
            | PollMaker cityName -> PollMakerPage.render cityName model (fun poolMsg ->  dispatch (PollMakerMsg poolMsg))
            | CityPicker -> CityPickerPage.render router.Link model (fun cityMsg -> dispatch (CitiyPickerMsg cityMsg))
            | LogInPage -> 
                cond model.signedInAs <| function
                   | None -> LogInPage.render model (fun loginMsg -> dispatch (LogInMsg loginMsg))
                   | Some _ -> CityPickerPage.render router.Link model (fun cityMsg -> dispatch (CitiyPickerMsg cityMsg))
            | PollPage pollId -> 
                cond model.signedInAs <| function 
                    | Some _ -> PollVotingPage.render (Guid.Parse(pollId)) model (fun voteMsg -> dispatch (PollVotingMsg voteMsg))
                    | None -> LogInPage.render model (fun loginMsg -> dispatch (LogInMsg loginMsg))
        )
        .Error(
            cond model.error <| function
            | None -> empty
            | Some err ->
                Main.ErrorNotification()
                    .Text(err)
                    .Hide(fun _ -> dispatch ClearError)
                    .Elt()
        )
        .Elt()

let buildHubConnection (builder:HubConnectionBuilder) = 
    builder.WithUrl("/SRHub", fun opt ->
        opt.LogLevel <- SignalRLogLevel.Trace 
        opt.Transport <- HttpTransportType.WebSockets
        ()).Build()


let recieved (connection:HubConnection) initial = 
    let sub (dispatch:(Message -> unit)) = 
        connection.On("sendMessage", fun value -> System.Threading.Tasks.Task.Run(Action (fun () -> ()))) |> ignore
    Cmd.ofSub sub
    
type MyApp() =
    inherit ProgramComponent<Model, Message>()
    
    [<Inject>]
    member val _hubConnectionBuilder = Unchecked.defaultof<HubConnectionBuilder> with get, set

    override this.Program =
        let restaurantService = this.Remote<RestaurantService>()
        let userService = this.Remote<UserService>()
        let hubConnection = buildHubConnection this._hubConnectionBuilder
        hubConnection.StartAsync() |> ignore
        let update = update restaurantService userService
        Program.mkProgram (fun _ -> initModel, Cmd.ofMsg (LogInMsg GetLogedInAs)) update view
        |> Program.withRouter router
#if DEBUG
        |> Program.withHotReload
        |> Program.withSubscription (recieved hubConnection)
#endif
