open System

open MessageType
open Suave
open Suave.Operators
open Suave.Filters
open Suave.Successful
open Newtonsoft.Json
open Suave.Sockets
open Suave.Sockets.Control
open Suave.WebSocket
open Newtonsoft.Json.Serialization
open Akka.FSharp
open System.Collections.Generic
open TwitterEngine

let server = System.create "server" (Configuration.defaultConfig())

let twitterServer=spawn server "TwitterServer" Server

let networkManager = new Dictionary<string,WebSocket>()

type UserCredentials = {
    UserId: string
    Password: string
}

let websocket (webSocket : WebSocket) (context: HttpContext) =
  socket {

    let mutable loop = true
    while loop do
      let! message = webSocket.read()
      match message with
      
      | (Text, data, true) ->
        let stringVar = UTF8.toString data
        let messageObject = JsonConvert.DeserializeObject<Tweet>(stringVar)
        let userName = messageObject.User
        let Type = messageObject.Type
        match Type with
        |"Connection"->
            if networkManager.ContainsKey(messageObject.User) = false then
                networkManager.Add(messageObject.User,webSocket)
        |"Tweet"->
            let subListTask = twitterServer<?TweetMessage(messageObject)
            let responseCollection = Async.RunSynchronously(subListTask,10000)
            for res in responseCollection do
                if(networkManager.ContainsKey(res)) then
                    let tweet =JsonConvert.SerializeObject(messageObject)
                    printfn "Tweet is %s" tweet
                    let byteRes =
                        tweet
                        |> System.Text.Encoding.ASCII.GetBytes
                        |> ByteSegment
                    do! networkManager.Item(res).send Text byteRes true
        |"Retweet" ->
            let subListTask = twitterServer<?TweetMessage(messageObject)
            let responseCollection = Async.RunSynchronously(subListTask,10000)
            for res in responseCollection do
                if(networkManager.ContainsKey(res)) then
                    let tweet =JsonConvert.SerializeObject(messageObject)
                    printfn "Tweet is %s" tweet
                    let byteRes =
                        tweet
                        |> System.Text.Encoding.ASCII.GetBytes
                        |> ByteSegment
                    do! networkManager.Item(res).send Text byteRes true
                

      | (Close, _, _) ->
        let blankResponse = [||] |> ByteSegment
        do! webSocket.send Close blankResponse true

        loop <- false

      | _ -> ()
    }

let JSON jsonVal =
    let jsonSerializerSettings = JsonSerializerSettings()
    jsonSerializerSettings.ContractResolver <- CamelCasePropertyNamesContractResolver()

    JsonConvert.SerializeObject(jsonVal, jsonSerializerSettings)
    |> OK
    >=> Writers.setMimeType "application/json; charset=utf-8"

let valFromJson<'a> jsonVal =
  JsonConvert.DeserializeObject(jsonVal, typeof<'a>) :?> 'a

let getCredentialsFromJson jsonVal =
  JsonConvert.DeserializeObject(jsonVal, typeof<UserCredentials>) :?> UserCredentials


let fetchResourceFromRequest<'a> (request : HttpRequest) =
    let fetchString (rawByteForm: byte[]) = System.Text.Encoding.UTF8.GetString(rawByteForm)
    request.rawForm |> fetchString |> valFromJson<'a>

let parseCredentials (request : HttpRequest) =
    let getString (rawByteForm: byte[]) = System.Text.Encoding.UTF8.GetString(rawByteForm)
    request.rawForm |> getString |> getCredentialsFromJson


let UserLogin (request: HttpRequest) = 
    let credentials = parseCredentials request
    if credentials.Password = "ThisIsMyPassword!StayAwayCreep" then
        handShake websocket
    else
        OK "Incorrect Password! User not Authorized!"


let UserLogout = 
    fun(user)->
        networkManager.Remove(user)
        OK "Logout Successful"

let QueryMentions=
    fun(mentions)->
        let task =twitterServer<?QueryMentions(mentions)
        let res = Async.RunSynchronously(task,10000)
        printfn "This is the response %A" res
        let responseInJsonFormat = JsonConvert.SerializeObject(res)
        OK responseInJsonFormat

let QueryHashTag=
    fun(tags)->
        let task =twitterServer<?QueryHashTags(tags)
        let res = Async.RunSynchronously(task,10000)
        printfn "This is the response %A" res
        let responseInJsonFormat = JsonConvert.SerializeObject(res)
        OK responseInJsonFormat

let AllSubscribersQuery = 
    fun(user)->
        let task =twitterServer<?QuerySubscribers(user)
        let res = Async.RunSynchronously(task,10000)
        printfn "This is the response %A" res
        let responseInJsonFormat = JsonConvert.SerializeObject(res)
        OK responseInJsonFormat

let  FetchAlltweets = 
    fun(s)->
        let task =twitterServer<?AllTweets
        let res = Async.RunSynchronously(task,10000)
        let responseInJsonFormat = JsonConvert.SerializeObject(res)
        OK responseInJsonFormat

let SubscribeToAPI=
    fun (firstUser,secondUser) ->
        let task = twitterServer<?Subscribe(firstUser.ToString(),secondUser.ToString())
        let res = Async.RunSynchronously(task,10000)
        let responseInJsonFormat = {message=res}
        let res = JsonConvert.SerializeObject(responseInJsonFormat)
        OK res

let Register =
    fun (a) ->
        let task = twitterServer<?RegisterUser(a.ToString())
        let res = Async.RunSynchronously(task,10000)
        let responseInJsonFormat = {message=res}
        let res = JsonConvert.SerializeObject(responseInJsonFormat)
        OK res

let twitterAPP =
    choose
        [ 
        path "/sampleSocket" >=> handShake websocket
        GET >=> choose
            [ path "/" >=> OK "Index"
              path "/hello" >=> OK "Hello!"
              pathScan "/getAllTweets/%s" FetchAlltweets
              pathScan "/Register/%s" Register
              pathScan "/Subscribe/%s/%s" SubscribeToAPI
              pathScan "/QuerySubs/%s" AllSubscribersQuery
              pathScan "/hashTagQuery/%s" QueryHashTag
              pathScan "/mentionsQuery/%s" QueryMentions
              pathScan "/Logout/%s" UserLogout
            ]
                 ]

[<EntryPoint>]
let main argv =
    printfn "Twitter is live now! Let them tweets rolling."
    startWebServer defaultConfig twitterAPP            
    0 