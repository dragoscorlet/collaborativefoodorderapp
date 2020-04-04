module PollAddressPage

open ApplicationModel
open PollMakerPage


let render getLink model = 
    Main.PollAddress()
        .Address( getLink (PollPage (model.poll.id.ToString())))
        .Elt()
