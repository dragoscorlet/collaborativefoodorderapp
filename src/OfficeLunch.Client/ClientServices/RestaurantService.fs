namespace OfficeLunch.Client

open Bolero.Remoting
open OfficeLunch.DomainModels.RestaurantTypes

type RestaurantService =
    {
        listCities: unit -> Async<string[]>
        listRestaurants: string -> Async<RestaurantListing[]>
        getRestaurantDetails: string -> Async<RestaurantDetail>

    }

    interface IRemoteService with
        member this.BasePath = "/restaurants"