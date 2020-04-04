namespace OfficeLunch.DataAccess

open FSharp.Data.Sql.Transactions

module UserDataProvider = 

    open FSharp.Data.Sql
    open System

    type sql = SqlDataProvider<Common.DatabaseProviderTypes.MSSQLSERVER, "Server=localhost\SQLEXPRESS;Database=officelunch;Trusted_Connection=True;">
    let TransactionOptions = {IsolationLevel = IsolationLevel.DontCreateTransaction; Timeout = TimeSpan.FromSeconds(1.0)}
    
    let context = sql.GetDataContext(TransactionOptions)

    let vote optionId userId = 
    
        let exists = query { 
            for vote in context.Dbo.PollOptionVotes do 
            where (vote.IdUser = userId && vote.IdOption = optionId)
            select (Some vote)
            exactlyOneOrDefault
            }
    
        match exists with
        | Some _ -> ()
        | None -> 
            context.Dbo.PollOptionVotes.Create(optionId, userId) |> ignore  
            context.SubmitUpdates()

    let unVote optionId userId = 

        let vote = query { 
            for vote in context.Dbo.PollOptionVotes do 
            where (vote.IdUser = userId && vote.IdOption = optionId)
            select (Some vote)
            exactlyOneOrDefault
            }

        match vote with
        | Some value -> 
            value.Delete()
            context.SubmitUpdates()
        | None -> ()


    let getUserName userId = 
        query {
            for user in context.Dbo.Users do
            where (user.Id = userId)
            select (user.Name)
            exactlyOneOrDefault
        }

    let createUser userName = 
        let userId = Guid.NewGuid()
        context.Dbo.Users.Create(userId,userName) |> ignore
        context.SubmitUpdates()
        Some userId


    let joinPoll userName pollId = 
        let exists = query { 
            for members in context.Dbo.PollMembers do 
            join user in context.Dbo.Users on (members.UserId = user.Id)
            where (members.PollId = pollId && userName = user.Name)
            select (Some members)
            exactlyOneOrDefault
            }

        match exists with
        | Some _ -> None
        | None ->
            let userId = Guid.NewGuid()
            context.Dbo.Users.Create(userId,userName) |> ignore
            context.Dbo.PollMembers.Create(pollId, userId) |> ignore
            context.SubmitUpdates()
            Some userId
