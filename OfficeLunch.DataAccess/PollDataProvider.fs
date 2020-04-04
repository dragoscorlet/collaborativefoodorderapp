namespace OfficeLunch.DataAccess

open FSharp.Data.Sql.Transactions

module PollDataProvider = 

    open FSharp.Data.Sql
    open OfficeLunch.DomainModels.PollTypes
    open System

    type sql = SqlDataProvider<Common.DatabaseProviderTypes.MSSQLSERVER, "Server=localhost\SQLEXPRESS;Database=officelunch;Trusted_Connection=True;">
    let TransactionOptions = {IsolationLevel = IsolationLevel.DontCreateTransaction; Timeout = TimeSpan.FromSeconds(1.0)}
      
    let context = sql.GetDataContext(TransactionOptions)

    let private countOptionVotes optionId = 
        query { for vote in context.Dbo.PollOptionVotes do
                where (vote.IdOption = optionId)
                select vote
                count
                }

    let private getVotersNames optionId = 
        query { for vote in context.Dbo.PollOptionVotes do
                join user in context.Dbo.Users on (vote.IdUser = user.Id)
                where (vote.IdOption = optionId)
                select user.Name
                }

    let private getPollOptions pollId = 
        query { for option in context.Dbo.PollOptions do
                where (option.PollId = pollId)
                select({
                    id = option.Id
                    title = option.Title 
                    thumbnailUrl = option.ThumbnailUrl
                    votes = countOptionVotes option.Id
                    votedBy = getVotersNames option.Id |> Seq.toList
                })}

    let private mapPollStatusId statusId = 
        match statusId with
        | 1 -> Created
        | 2 -> InProgress
        | _ -> Closed

    let private mapPollStatus status = 
        match status with
        | Created -> 1
        | InProgress -> 2
        | Closed -> 3

    let getPollDetails pollId = 
        query { for poll in context.Dbo.Polls do
                join user in context.Dbo.Users on (poll.Owner = user.Id)
                where (poll.Id = pollId)
                select({
                    id = poll.Id
                    options = getPollOptions pollId |> Seq.toList
                    owner = {
                        name = user.Name 
                        id = user.Id}
                    endTime = poll.EndTime
                    title = poll.Title
                    status = mapPollStatusId poll.Status
                })
                exactlyOneOrDefault
        }

    let createPoll (poll:Poll) = 
        let dbPoll  = {poll with id = Guid.NewGuid()} 

        context.Dbo.Polls.Create(dbPoll.id, dbPoll.endTime, dbPoll.owner.id, mapPollStatus dbPoll.status, dbPoll.title) |> ignore
        context.SubmitUpdates()
  
        let dbOptions = poll.options |> List.map (fun option ->
            let dbOption = {option with id = Guid.NewGuid()}
            context.Dbo.PollOptions.Create(dbOption.id, dbPoll.id, dbOption.thumbnailUrl, dbOption.title) |> ignore
            dbOption
            ) 
  
        context.SubmitUpdates()

        {dbPoll with options = dbOptions}
