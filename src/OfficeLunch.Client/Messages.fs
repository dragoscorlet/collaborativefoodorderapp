module Messages

open ApplicationModel
open PollMakerPage
open CityPickerPage
open LogInPage
open PollVotingPage

type Message =
    | SetPage of Page
    | Error of exn
    | ClearError
    | PollMakerMsg of PollMakerMessage
    | CitiyPickerMsg of CitiyPicker
    | LogInMsg of LogIn
    | PollVotingMsg of PoolVotingMessage