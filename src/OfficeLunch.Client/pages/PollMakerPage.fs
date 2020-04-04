module PollMakerPage

open Bolero
open Bolero.Html
open OfficeLunch.DomainModels.PollTypes
open OfficeLunch.DomainModels.RestaurantTypes
open ApplicationModel
open Elmish
open OfficeLunch.Client
open System

type Main = Template<"wwwroot/main.html">


type PollMakerMessage = 
    | AddPollOption of PollOption
    | RemovePollOption of PollOption
    | CreatePoll
    | CreatedPoll of Poll
    | SetPollTitle of string
    | SetPollEndTime of DateTime
    | Error of exn


let addPollOption listing dispatch = 
    dispatch(AddPollOption 
        {   
            id = Guid.Empty
            title = listing.name
            thumbnailUrl = listing.thumbnailUrl
            votes = 0
            votedBy = []
        })

let tryRemovePollOption listing dispatch = 
    dispatch(RemovePollOption
        {   
            id = Guid.Empty
            title = listing.name
            thumbnailUrl = listing.thumbnailUrl
            votes = 0
            votedBy = []
        })

let restaurantsList listing  dispatch = 
    ul[] [
        li [] [input [
            attr.``type`` "checkbox"
            bind.``checked`` listing.isChecked (fun isChecked -> 
                if isChecked 
                then addPollOption listing dispatch 
                else tryRemovePollOption listing dispatch)
        ]]
        li [] [img [attr.src listing.thumbnailUrl]]
        li [] [text listing.description]
        li [] [text listing.rating]
    ] 

let render (cityName:string) model dispatch =
    Main.PollMaker()
        .SearchResults(cond model.restaurantSearchResult <| function
            | None -> 
                div[][text "no restaurants"]
            | Some results -> forEach results <| fun result -> restaurantsList result dispatch)
        .CreatePoll(fun _ -> dispatch CreatePoll)
        .PollTitle(model.poll.title, fun title -> dispatch (SetPollTitle title))
        .PollEndTime(model.poll.endTime.ToString(), fun endTime -> dispatch (SetPollEndTime (DateTime.Parse(endTime))))
        .SelectedCity(cityName)
        .Elt()

let addOption model option = {model with poll = {model.poll with options = model.poll.options @ [option]}}

let removeOption model (option: PollOption) = 
    {model with poll = 
                {model.poll with options = model.poll.options  
                                |> Seq.where (fun o -> not(String.Equals(o.title, option.title))) 
                                |> Seq.toList}}

let mapRating rating = 
    match rating with
    | OneStar -> "1"
    | TwoStars -> "2"
    | ThreeStars -> "3"
    | FourStars -> "4"
    | FiveStars -> "5"

let updateListings model (listings:RestaurantListing[]) = 
    {model with restaurantSearchResult = Some (listings |> Seq.map (fun r -> 
                                            {
                                                name = r.name 
                                                description =r.description 
                                                thumbnailUrl = r.imageUrl 
                                                rating = mapRating r.rating
                                                isChecked = false 
                                            }) |> Seq.toArray) }

let update userRemote message model =
    match message with
    | AddPollOption option ->
        addOption model option, Cmd.none
    | RemovePollOption option ->
        removeOption model option, Cmd.none
    | CreatePoll -> 
        {model with poll = {model.poll with owner = model.signedInAs.Value}}, 
        Cmd.ofAsync userRemote.createPoll {model.poll with owner = model.signedInAs.Value} CreatedPoll Error
    | SetPollEndTime endTime -> {model with poll = {model.poll with endTime = endTime}}, Cmd.none
    | SetPollTitle title -> {model with poll = {model.poll with title = title}}, Cmd.none
    | CreatedPoll poll -> {model with page = PollAddress; poll = poll }, Cmd.none
    | Error _ -> model, Cmd.none
