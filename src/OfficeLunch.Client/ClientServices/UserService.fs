namespace OfficeLunch.Client


open Bolero.Remoting
open OfficeLunch.DomainModels.PollTypes
open System

type UserService = 
    {
        getUser: unit -> Async<User>
        logIn: string*(Guid option) -> Async<User option>
        logOut : unit -> Async<unit>
        createPoll: Poll -> Async<Poll>
        vote: Guid*Guid -> Async<Guid>
        unVote: Guid*Guid -> Async<Guid>
        getPoll:Guid -> Async<Poll>
    }

    interface IRemoteService with
        member this.BasePath = "/users"
  