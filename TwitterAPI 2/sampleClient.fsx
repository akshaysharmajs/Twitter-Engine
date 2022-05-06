#load "MessageType.fs"
#r "nuget: Akka.FSharp"
#r "nuget: Suave"
#r "nuget: Newtonsoft.Json"
#r "System.Net.Http.dll"

open Newtonsoft.Json
open Akka.FSharp
open System.Net.WebSockets
open System.Threading
open System.Text
open System
open System.Net.Http
open MessageType
open System.Collections.Generic

printfn "Enter your username to continue to Twitter"

let userName = System.Console.ReadLine()
let httpClientVal = new HttpClient()
let mutable webSocketVal = new ClientWebSocket()
let t = webSocketVal.ConnectAsync(new System.Uri("ws://127.0.0.1:8080/sampleSocket"), CancellationToken.None)
while t.IsCompleted = false do
   
printfn "Connected to Twitter now"
let tempConnectionObject = {Type="Connection";User=userName;TweetText="";HashTag=new List<string>();Mentions=new List<string>()}
let tempJsonObjectBytes=JsonConvert.SerializeObject(tempConnectionObject)
printfn "%s" tempJsonObjectBytes
let encodedJsonData = Encoding.UTF8.GetBytes(tempJsonObjectBytes)
let bufferData =  new ArraySegment<Byte>(encodedJsonData,0,encodedJsonData.Length)
let xtVal = webSocketVal.SendAsync(bufferData,WebSocketMessageType.Text,true,CancellationToken.None)


let responseObjectTask = httpClientVal.GetAsync("http://localhost:8080/Register/"+userName)
while responseObjectTask.IsCompleted = false do
    ()
let responseJsonObject = responseObjectTask.Result
let jsonContent = responseJsonObject.Content
let jsonByteArray = jsonContent.ReadAsByteArrayAsync()
while jsonByteArray.IsCompleted=false do
    ()
let jsonString = System.Text.Encoding.Default.GetString(jsonByteArray.Result);
let responseObject = JsonConvert.DeserializeObject<SampleResponseType>(jsonString)
printfn "Response is %A" responseObject


let mutable recieveBuffer =  WebSocket.CreateClientBuffer(1000000,1000000)

let cancellation =true
async{
    
    while cancellation do
        let receiveTask = webSocketVal.ReceiveAsync(recieveBuffer,CancellationToken.None)
        while receiveTask.IsCompleted=false && receiveTask.Result.EndOfMessage=false do
            ()

        let responseString = Encoding.ASCII.GetString(recieveBuffer.Array,0,receiveTask.Result.Count)
        if(responseString<>"") then
            printfn "This is new %s" responseString
}|>Async.Start

let mutable whileFlag = true 
while whileFlag do
    printfn "Select the number for the operation you want to perform"
    printfn "1. Tweet"
    printfn "2. ReTweet"
    printfn "3. Search all who you have subscribed to"
    printfn "4. Search tags"
    printfn "5. Search mention"
    printfn "6. Subscribe"
    printfn "7. Exit"
    let userInputNumber = System.Console.ReadLine();
    
    let inputAsInteger = userInputNumber |>int
    match inputAsInteger with
            |1 -> 
                printfn "Enter the message you want to Tweet"
                let TweetMessage = System.Console.ReadLine();
                printfn "Enter the hashtag for the Tweet"
                let hashTag = System.Console.ReadLine();
                let hashTagArray = hashTag.Split ','
                let mutable hashTagCollection = new List<string>()
                for index in hashTagArray do
                    hashTagCollection.Add(index.Remove(0,1))
                printfn "Enter the user you want to mention in the Tweet"
                let userIdToMention = System.Console.ReadLine();
                printfn "%s %s %s" TweetMessage hashTag userIdToMention
                let userIdToMentionArray = userIdToMention.Split '@'
                let mutable mentionsCollection = new List<string>()
                for index in 1 .. (userIdToMentionArray.Length-1) do
                    mentionsCollection.Add(userIdToMentionArray.[index])
                let tweetMessage = {Type="Tweet";User=userName;TweetText = TweetMessage; HashTag=hashTagCollection;Mentions=mentionsCollection}
                let tweetMessageJson=string(JsonConvert.SerializeObject(tweetMessage))
                let encodedTweet = Encoding.UTF8.GetBytes(tweetMessageJson)
                let bufferArray =  new ArraySegment<Byte>(encodedTweet,0,encodedTweet.Length)
                webSocketVal.SendAsync(bufferArray,WebSocketMessageType.Text,true,CancellationToken.None)|>ignore

            |2->     
                printfn "Retweeted!"
                let responseTaskObject = httpClientVal.GetAsync("http://localhost:8080/getAllTweets/allusers")
                while responseTaskObject.IsCompleted = false do
                    ()
                let responseJsonObject = responseTaskObject.Result
                let jsonContent = responseJsonObject.Content
                let byteArrayObject = jsonContent.ReadAsByteArrayAsync()
                while byteArrayObject.IsCompleted=false do
                    ()
                let jsonString = System.Text.Encoding.Default.GetString(byteArrayObject.Result);
                let responseObject = JsonConvert.DeserializeObject<Dictionary<int,Tweet>>(jsonString)
                printfn "Response is %A" responseObject
                printfn "Enter the index of tweet you want to retweet"
                let userNumber = System.Console.ReadLine(); 
                let userNumberInt = userNumber |>int
                let mutable tweet = responseObject.Item(userNumberInt)
                let reTweet = {Type="Retweet";User=userName;TweetText=tweet.TweetText;HashTag=tweet.HashTag;Mentions=tweet.Mentions}
                printfn "This is the retweet %A" reTweet
                let tweetMessageJson=string(JsonConvert.SerializeObject(reTweet))
                let encodedTweet = Encoding.UTF8.GetBytes(tweetMessageJson)
                let bufferArray =  new ArraySegment<Byte>(encodedTweet,0,encodedTweet.Length)
                webSocketVal.SendAsync(bufferArray,WebSocketMessageType.Text,true,CancellationToken.None)|>ignore

            |3 -> 
                let responseTaskObject = httpClientVal.GetAsync("http://localhost:8080/QuerySubs/"+userName)
                while responseTaskObject.IsCompleted = false do
                    ()
                let responseJsonObject = responseTaskObject.Result
                let jsonContent = responseJsonObject.Content
                let byteArrayObject = jsonContent.ReadAsByteArrayAsync()
                while byteArrayObject.IsCompleted=false do
                    ()
                let jsonString = System.Text.Encoding.Default.GetString(byteArrayObject.Result);
                let responseObject = JsonConvert.DeserializeObject<List<Tweet>>(jsonString)
                printfn "Response is %A" responseObject
                printfn "Find subscribers"
            |4 -> 
                printfn "Enter the hashTag you want to search(without #)"
                let hashtag = System.Console.ReadLine();
                let responseTaskObject = httpClientVal.GetAsync("http://localhost:8080/hashTagQuery/"+hashtag)
                while responseTaskObject.IsCompleted = false do
                    ()
                let responseJsonObject = responseTaskObject.Result
                let jsonContent = responseJsonObject.Content
                let byteArrayObject = jsonContent.ReadAsByteArrayAsync()
                while byteArrayObject.IsCompleted=false do
                    ()
                let jsonString = System.Text.Encoding.Default.GetString(byteArrayObject.Result);
                let responseObject = JsonConvert.DeserializeObject<List<Tweet>>(jsonString)
                printfn "Response is %A" responseObject
                printfn "Find hashtags"
            |5 ->
                printfn "Enter the user whose mentions you want to find"
                let mention = System.Console.ReadLine();
                let responseTaskObject = httpClientVal.GetAsync("http://localhost:8080/mentionsQuery/"+mention)
                while responseTaskObject.IsCompleted = false do
                    ()
                let responseJsonObject = responseTaskObject.Result
                let jsonContent = responseJsonObject.Content
                let byteArrayObject = jsonContent.ReadAsByteArrayAsync()
                while byteArrayObject.IsCompleted=false do
                    ()
                let jsonString = System.Text.Encoding.Default.GetString(byteArrayObject.Result);
                let responseObject = JsonConvert.DeserializeObject<List<Tweet>>(jsonString)
                printfn "Response is %A" responseObject
                printfn "Find hashtag"
            |6 ->
                printfn "Enter the user you want to subscribe to?"
                let userIdToSubscribeTo = System.Console.ReadLine()|>string; 
                let responseTaskObject = httpClientVal.GetAsync("http://localhost:8080/Subscribe/"+userName+"/"+userIdToSubscribeTo)
                while responseTaskObject.IsCompleted = false do
                    ()
                let responseJsonObject = responseTaskObject.Result
                let jsonContent = responseJsonObject.Content
                let byteArrayObject = jsonContent.ReadAsByteArrayAsync()
                while byteArrayObject.IsCompleted = false do
                    ()
                let jsonString = System.Text.Encoding.Default.GetString(byteArrayObject.Result);
                let responseObject = JsonConvert.DeserializeObject<SampleResponseType>(jsonString)
                printfn "Response is %A" responseObject
            |7-> 
                whileFlag <- false 
                let responseTaskObject = httpClientVal.GetAsync("http://localhost:8080/Logout/"+userName)
                while responseTaskObject.IsCompleted do
                    ()
                let task = webSocketVal.CloseAsync(WebSocketCloseStatus.NormalClosure,"Close",CancellationToken.None)
                while task.IsCompleted do
                    ()
            |_ ->
                printfn "Not a valid input"
