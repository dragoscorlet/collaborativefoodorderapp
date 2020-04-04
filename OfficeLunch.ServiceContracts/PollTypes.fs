namespace OfficeLunch.DomainModels

module PollTypes = 

    open System

    let rec quicksortSequential aList = 
        match aList with
        | [] -> []
        | firstElement :: restOfList ->
            let smaller, larger = List.partition(fun number -> number < firstElement) restOfList
            quicksortSequential smaller @ (firstElement :: quicksortSequential larger)

    type User = 
        {
            name:string
            id:Guid
        }

    type Poll = 
        {   
            id:Guid
            options: PollOption list
            owner: User
            endTime: DateTime
            status: PollStatus
            title: string
        }

    and PollOption =
        {   
            id:Guid
            title: string
            thumbnailUrl: string
            votes: int
            votedBy: string list
        }

    and PollStatus = Created | InProgress | Closed

