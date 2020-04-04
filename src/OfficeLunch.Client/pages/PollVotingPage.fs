module PollVotingPage

open Bolero.Html
open OfficeLunch.DomainModels.PollTypes
open ApplicationModel
open System
open PollMakerPage
open Elmish
open OfficeLunch.Client

type PoolVotingMessage = 
    | LoadPoll of Guid
    | LoadedPoll of Poll
    | VoteOption of Guid*Guid
    | VotedOption of Guid
    | UnVoteOption of Guid*Guid
    | UnVotedOption of Guid
    | LoadOptionDetails of string
    | LoadedOptionDetails of string
    | VoteError of exn

let optionList userId (options: PollOptionView seq) dispatch = 
    ul[] [
        forEach options <| fun option -> li [on.click (fun _ -> (dispatch (LoadOptionDetails option.title)))] [
            input [
                attr.``type`` "checkbox"
                bind.``checked`` option.isSelected (fun isChecked ->
                    if isChecked
                    then dispatch (VoteOption (option.id, userId)) 
                    else dispatch (UnVoteOption (option.id, userId)))]
            p [] [text option.title]
            p [] [text ("Votes number: " + option.votes.ToString()) ]
            ]
    ]

let getOptionsView (options:PollOption) = 
    {
        isSelected = false
        id = options.id
        title = options.title
        thumbnailUrl = options.thumbnailUrl
        votes = options.votes
        votedBy = options.votedBy
    }

let render pollId model dispatch  =
    Main.Poll()
        .Title(cond (model.poll.id = pollId) <| function
            | false -> 
                dispatch (LoadPoll pollId) 
                p [] [text model.poll.title]
            | true -> p [] [text model.poll.title])
        .EndTime(model.poll.endTime.ToString())
        .Options(optionList model.signedInAs.Value.id (model.poll.options |> Seq.map(fun o -> getOptionsView o)) dispatch)
        .Elt()

let addVotes votes optionId (options: PollOption list) =     
    options |> List.map(fun option -> 
        if option.id = optionId
        then {option with votes = option.votes + votes}
        else option)

let vote optionId model = {model with poll = {model.poll with options = (addVotes 1 optionId model.poll.options)}}
let unVote optionId model = {model with poll = {model.poll with options = (addVotes -1 optionId model.poll.options)}}


let update userRemote message model =
    match message with
    | VoteOption (optionId, userId) -> model, Cmd.ofAsync userRemote.vote (optionId,userId)  VotedOption VoteError
    | VotedOption optionId -> vote optionId model, Cmd.none
    | UnVoteOption (optionId, userId) -> model, Cmd.ofAsync userRemote.unVote (optionId, userId)  UnVotedOption VoteError
    | UnVotedOption optionId -> unVote optionId model, Cmd.none
    | LoadPoll pollId -> model, Cmd.ofAsync userRemote.getPoll pollId LoadedPoll VoteError
    | LoadedPoll poll -> {model with poll = poll}, Cmd.none
    | LoadOptionDetails restaurantName -> model, Cmd.none //Cmd.ofAsync restaurantRemote.getMenu restaurantName // get restaurant menu here
    | LoadedOptionDetails _ -> model, Cmd.none
    | VoteError _ -> model, Cmd.none