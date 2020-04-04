module CityPickerPage

open Bolero.Html
open ApplicationModel
open PollMakerPage
open Elmish
open OfficeLunch.Client
open OfficeLunch.DomainModels.RestaurantTypes


type CitiyPicker = 
    |SearchCities 
    | RecievedCities of string [] 
    | RecievedRestaurantListings of RestaurantListing[]
    | ErrorRecievingListings of exn
    | SearchForRestaurants of string
    | Error of exn

let update restaurantRemote message model =
    match message with
    | SearchCities -> 
        model, 
        Cmd.ofAsync restaurantRemote.listCities ()  RecievedCities Error
    | RecievedCities cities -> {model with cities = cities |> Seq.toList}, Cmd.none
    | SearchForRestaurants input ->
        {model with suggestInput = input},
        Cmd.ofAsync restaurantRemote.listRestaurants (input) RecievedRestaurantListings ErrorRecievingListings
    | RecievedRestaurantListings result -> (updateListings model result), Cmd.none
    | ErrorRecievingListings exn -> model, Cmd.none
    | Error exn -> model, Cmd.none

let render getLink model dispatch = 
    Main.CityPicker()
        .Cities(cond model.cities.IsEmpty <| function
            | true -> 
                dispatch SearchCities
                div[][text ""]
            | false ->ul [] [ forEach model.cities <| fun city -> li [] [ 
                a [ 
                    attr.href (getLink (PollMaker city)) 
                    on.click (fun _ -> (dispatch (SearchForRestaurants city) )) ][text city]]])
        .Elt()