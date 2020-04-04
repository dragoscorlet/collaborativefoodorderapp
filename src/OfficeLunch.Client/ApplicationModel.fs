module ApplicationModel

open System
open OfficeLunch.DomainModels.PollTypes
open Bolero

type Page =
    | [<EndPoint "/">] LogInPage
    | [<EndPoint "/cities">] CityPicker
    | [<EndPoint "/pollmaker/{cityName}">] PollMaker of cityName:string
    | [<EndPoint "/greet">] Greet
    | [<EndPoint "/polladress">] PollAddress
    | [<EndPoint "/poll/{id}">] PollPage of id:string


type Model =
    {
        page: Page
        error: string option
        username: string
        password: string
        signedInAs: option<User>
        signInFailed: bool
        suggestInput: string
        restaurantSearchResult: RestaurantListingView[] option
        poll : Poll
        pollAddress : string
        cities : string list
    }

and RestaurantListingView = 
    {
        name:string
        thumbnailUrl:string
        description:string
        rating:string
        isChecked:bool
    } 
and PollOptionView =
    {   
        id:Guid
        title: string
        thumbnailUrl: string
        votes: int
        votedBy: string list
        isSelected: bool
    }
