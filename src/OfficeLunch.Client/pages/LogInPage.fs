module LogInPage


open Bolero.Remoting.Client
open ApplicationModel
open PollMakerPage
open Elmish
open OfficeLunch.Client
open OfficeLunch.DomainModels.PollTypes
open System

type LogIn = 
    | SetUserName of string
    | GetLogedInAs
    | SendLogIn
    | RecvLogedIn of option<User>
    | SendLogOut
    | RecvLogOut
    | LogInError of exn

let getPollId (poll:Poll) = 
    if poll.id = Guid.Empty 
    then None
    else Some poll.id

let update remote message model = 
    match message with
    | SetUserName name -> {model with username = name}, Cmd.none
    | GetLogedInAs -> model, Cmd.ofAuthorized remote.getUser () RecvLogedIn LogInError
    | RecvLogedIn user -> {model with signedInAs = user}, Cmd.none
    | SendLogIn -> model, Cmd.ofAsync remote.logIn (model.username, getPollId (model.poll)) RecvLogedIn LogInError
    | SendLogOut -> model, Cmd.ofAsync remote.logOut () (fun _ -> RecvLogOut) LogInError
    | RecvLogOut -> {model with signedInAs = None; signInFailed = false}, Cmd.none
    | LogInError exn -> model, Cmd.none


let render model dispatch = 
    Main.LogIn()
        .Username(model.username, fun s -> dispatch (SetUserName s))
        .LogIn(fun _ -> dispatch SendLogIn)
        .UserDetails(match model.signedInAs with None -> "nothing yet" | Some user -> user.id.ToString())
        .Elt()