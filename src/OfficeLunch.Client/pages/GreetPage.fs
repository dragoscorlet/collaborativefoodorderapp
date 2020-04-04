module GreetPage

open Bolero.Html
open ApplicationModel
open System
open PollMakerPage

let render getLink  model = 
    Main.Greet()
        .Name(model.signedInAs.Value.name)
        .Proceed(cond (model.poll.id = Guid.Empty) <| function
            | true -> 
                h1 [] [a [attr.href (getLink CityPicker)] [text "Create Poll"]]
            |false -> 
                h1 [] [
                    a [attr.href (getLink CityPicker)] [text "Create Poll"]
                    div[] [text "or"]
                    a [attr.href (getLink (PollPage (model.poll.id.ToString())))] [text "Join Poll"]
                    ])
        .Elt()
