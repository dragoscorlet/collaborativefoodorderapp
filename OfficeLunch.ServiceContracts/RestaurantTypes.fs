namespace OfficeLunch.DomainModels

module RestaurantTypes = 

    open System

    [<Measure>]
    type RON
    [<Measure>]
    type USD
    [<Measure>]
    type EUR


    type RestaurantListing = 
        {
            name: string
            description: string
            imageUrl: string
            rating : RestaurantRating
        }
    and RestaurantRating = OneStar | TwoStars | ThreeStars | FourStars | FiveStars

    and RestaurantDetail = 
        {
            categories: Category list
            schedule : DeliverySchedule
            adress : string
            phonenumber: string
        }
    and DeliverySchedule =
        {
            Monday: DeliveryInterval
            Tuesday: DeliveryInterval
            Wednesday: DeliveryInterval
            Thursday: DeliveryInterval
            Friday: DeliveryInterval
            Saturday: DeliveryInterval
            Sunday: DeliveryInterval
        }
    and DeliveryInterval =
        {
            opensAt: TimeSpan
            closesAt: TimeSpan
        }
    and Category = 
        {
            name:string
            products:Product list
        }
    and Product =
        {
            name: string
            ingredients: string list
            grams: int
            price: Price
        }
    and Price = RONPrice of decimal<RON> | USDPrice of decimal<USD> | EURPrice of decimal<EUR>