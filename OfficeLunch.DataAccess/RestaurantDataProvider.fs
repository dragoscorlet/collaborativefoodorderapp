namespace OfficeLunch.DataAccess

module RestaurantDataProvider = 

    open FSharp.Data
    open OfficeLunch.DomainModels.RestaurantTypes

    type RestaurantListingsJson = JsonProvider<"./samples/restaurants.json">
    type RestaurantDetailsJson = JsonProvider<"./samples/products.json">

    let parseRating jsonValue = 
        match jsonValue with
        | lowerBound when lowerBound <= 1 -> OneStar
        | 2 -> TwoStars
        | 3 -> ThreeStars
        | 4 -> FourStars
        | _ -> FiveStars

    let getRestaurantListings cityName = 
        RestaurantListingsJson.GetSamples() |> Seq.map (fun record -> 
            {
                name = record.Name 
                description = record.Description 
                imageUrl = record.ImageUrl
                rating = parseRating record.Rating}) |> Seq.toArray

    let getProducts restaurantName = 
        RestaurantDetailsJson.GetSamples() |> Seq.map (fun record ->
        {            
            schedule = {
                Monday   = {
                    opensAt = record.Schedule.Monday.OpensAt.TimeOfDay
                    closesAt = record.Schedule.Monday.ClosesAt.TimeOfDay} 
                Tuesday  = {
                    opensAt = record.Schedule.Monday.OpensAt.TimeOfDay 
                    closesAt = record.Schedule.Monday.ClosesAt.TimeOfDay}
                Wednesday= {
                    opensAt = record.Schedule.Monday.OpensAt.TimeOfDay 
                    closesAt = record.Schedule.Monday.ClosesAt.TimeOfDay}
                Thursday = {
                    opensAt = record.Schedule.Monday.OpensAt.TimeOfDay
                    closesAt = record.Schedule.Monday.ClosesAt.TimeOfDay}
                Friday   = {
                    opensAt = record.Schedule.Monday.OpensAt.TimeOfDay
                    closesAt = record.Schedule.Monday.ClosesAt.TimeOfDay} 
                Saturday = {
                    opensAt = record.Schedule.Monday.OpensAt.TimeOfDay
                    closesAt = record.Schedule.Monday.ClosesAt.TimeOfDay}
                Sunday   = {
                    opensAt = record.Schedule.Monday.OpensAt.TimeOfDay
                    closesAt = record.Schedule.Monday.ClosesAt.TimeOfDay} 

            }
            adress = record.Adress
            phonenumber = record.Phonenumber
            categories = record.Categories |> Seq.map (fun c -> 
                {
                    name = c.Name
                    products = c.Products |> Seq.map (fun p -> 
                        {
                            name = p.Name
                            ingredients = p.Ingredients |> Seq.toList
                            grams = p.Grams
                            price = RONPrice (p.Price * 1.0m<RON>)
                        }) |> Seq.toList
                }) |> Seq.toList
        }) |> Seq.head