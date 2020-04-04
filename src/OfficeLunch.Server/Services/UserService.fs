namespace OfficeLunch.Server

open Bolero.Remoting.Server
open Microsoft.AspNetCore.Hosting
open OfficeLunch.DataAccess
open System

    
type UserService(ctx: IRemoteContext, env: IWebHostEnvironment) = 
    inherit RemoteHandler<OfficeLunch.Client.UserService>()

    override this.Handler = 
        {
            getUser = ctx.Authorize <| fun () -> async {
                let userId = ctx.HttpContext.User.Identity.Name
                let name = UserDataProvider.getUserName (Guid.Parse(userId))

                return {name = name; id = Guid.Parse(userId)}
            }

            logIn = fun (userName, pollId) -> async {
                    
                let userId = match pollId with
                | None -> UserDataProvider.createUser userName
                | Some id -> UserDataProvider.joinPoll userName id

                return match userId with
                | None -> None
                | Some id ->
                    ctx.HttpContext.AsyncSignIn(id.ToString(), TimeSpan.FromDays(365.)) |> ignore
                    Some {name = userName; id = id}
            }

            logOut = fun () -> async {
                return! ctx.HttpContext.AsyncSignOut()
            }

            createPoll = fun poll -> async {
                return PollDataProvider.createPoll poll 
            }

            getPoll = fun pollId -> async {
                return PollDataProvider.getPollDetails pollId
            }

            vote = fun (optionId, userId) -> async {
                UserDataProvider.vote optionId userId |> ignore
                return optionId
            }

            unVote = fun (optionId, userId) -> async {
                UserDataProvider.unVote optionId userId |> ignore
                return optionId
            }
        }