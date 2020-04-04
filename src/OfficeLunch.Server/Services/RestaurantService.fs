namespace OfficeLunch.Server

open Bolero.Remoting.Server
open Microsoft.AspNetCore.Hosting
open OfficeLunch.DataAccess

type RestaurantService(ctx: IRemoteContext, env: IWebHostEnvironment) = 
    inherit RemoteHandler<OfficeLunch.Client.RestaurantService>()
    
    override this.Handler = 
        {   
            listCities = fun () -> async { return [|"BV"; "CJ"; "TGV"; "CT"; "TM"|]}

            listRestaurants  = fun cityName -> async { return RestaurantDataProvider.getRestaurantListings cityName }

            getRestaurantDetails = fun restaurantName ->  async {return RestaurantDataProvider.getProducts restaurantName}
        }