module TwitterEngine

open System

open Akka.FSharp
open MessageType
open System.Collections.Generic



let mutable operationsCount=0
let mutable finalOperations=0;
let mutable maxSubscriber=0;
let mutable queriesCount=0;
let mutable tweetsCount=0;

let server = System.create "server" (Configuration.defaultConfig())

let clientActor(mailbox:Actor<_>) =
    let rec loop(x) = actor {
        let! message = mailbox.Receive()
        match message with
            |_->printfn ""
        return! loop(x)
    }
    loop(0)

let clientActorRef = spawn server "clientActorRef" clientActor
let mutable printActorRef = clientActorRef

let sampleActorObject(mailbox:Actor<_>) =
    let rec loop() = actor{
        let! message = mailbox.Receive();
        printfn "%A" message
        return! loop()
    }
    loop()


let Server(mailbox:Actor<_>) =
    let mutable TweetsCollection = new Dictionary<int,Tweet>()
    let mutable usersCollectionSet = new HashSet<string>()
    let mutable usersLoggedInSet = new HashSet<string>()
    let mutable tweetMessageCollection = new Dictionary<String,HashSet<Tweet>>()
    let mutable mentionsCollection = new Dictionary<String,HashSet<Tweet>>()
    let mutable hashTagsCollection = new Dictionary<String,HashSet<Tweet>>()
    let mutable subscribedToCollection = new Dictionary<string,List<string>>()
    let mutable SubscribersCollection = new Dictionary<String,List<String>>()
    let rec loop() = actor {
        let! message = mailbox.Receive()
        match message with
            |InitiateTimers->
                ()
            |RegisterUser(username)->
                printfn "Registering..."
                usersCollectionSet.Add(username)|>ignore
                let response = "Registration Successfull"
                printfn "Username %s successfully registered!" username 
                mailbox.Sender() <? response |> ignore

               
            |TweetMessage(tweet)->
                TweetsCollection.Add(TweetsCollection.Count,tweet)
                let username = tweet.User
                let tweetHashTagList = tweet.HashTag
                let tweetMentionsList = tweet.Mentions
                let tweetSetCollection = new HashSet<Tweet>()
                let tagSetCollection = new HashSet<Tweet>()
                let mentionSetCollection = new HashSet<Tweet>()
                if tweetMessageCollection.ContainsKey(username) then
                    tweetMessageCollection.Item(username).Add(tweet)|>ignore
                else
                    tweetMessageCollection.Add(username,tweetSetCollection)|>ignore
                    tweetMessageCollection.Item(username).Add(tweet)|>ignore
                for hashTag in tweetHashTagList do
                    if hashTagsCollection.ContainsKey(hashTag) then
                        hashTagsCollection.Item(hashTag).Add(tweet)|>ignore
                    else
                        hashTagsCollection.Add(hashTag,tagSetCollection)
                        hashTagsCollection.Item(hashTag).Add(tweet)|>ignore
                for mentions in tweetMentionsList do
                    if mentionsCollection.ContainsKey(mentions) then  
                        mentionsCollection.Item(mentions).Add(tweet)|>ignore
                    else
                        mentionsCollection.Add(mentions,mentionSetCollection)
                        mentionsCollection.Item(mentions).Add(tweet)|>ignore
                
                let mutable responseCollection = new List<string>()
                if(SubscribersCollection.ContainsKey(username)) then
                    responseCollection <- SubscribersCollection.Item(username)
                mailbox.Sender()<!responseCollection|>ignore
            |Subscribe(firstUser,secondUser)->
                let subscriberList = new List<string>()
                let subscribedToList = new List<string>()
                if subscribedToCollection.ContainsKey(firstUser) then
                    subscribedToCollection.Item(firstUser).Add(secondUser)
                else 
                    subscribedToCollection.Add(firstUser,subscribedToList)
                    subscribedToCollection.Item(firstUser).Add(secondUser)
                if SubscribersCollection.ContainsKey(secondUser) then
                    SubscribersCollection.Item(secondUser).Add(firstUser)
                else
                    SubscribersCollection.Add(secondUser,subscriberList)
                    SubscribersCollection.Item(secondUser).Add(firstUser)
                mailbox.Sender()<!(sprintf "You successfully subscribed to %s" secondUser)
            |QuerySubscribers(userName)->
                let tweetCollection=new List<Tweet>()
                if(subscribedToCollection.ContainsKey(userName))then
                    let subscribersList=subscribedToCollection.Item(userName)
                    for index in subscribersList do
                        if(tweetMessageCollection.ContainsKey(index))then
                            for j in tweetMessageCollection.Item(index) do
                                tweetCollection.Add(j)
                mailbox.Sender()<?tweetCollection|>ignore
                printfn "Subscribers queried by user %s" userName
            |QueryHashTags(hashTag)->
                let hashTagList=new List<Tweet>()
                if (hashTag<>" ") then
                    if(hashTagsCollection.ContainsKey(hashTag))then
                        for index in hashTagsCollection.Item(hashTag) do
                            hashTagList.Add(index)   
                mailbox.Sender()<?hashTagList|>ignore
            |QueryMentions(queriedActor)->
                let mentionList=new List<Tweet>()
                if(mentionsCollection.ContainsKey(queriedActor))then
                    for index in mentionsCollection.Item(queriedActor) do
                        mentionList.Add(index)
                mailbox.Sender()<?mentionList|>ignore
            |Logout(userName)->
                 if(usersLoggedInSet.Contains(userName))then
                     usersLoggedInSet.Remove(userName)
                     printfn "User %s logged out" userName
            |Login(userName)->
                if(not (usersLoggedInSet.Contains(userName)))then
                    usersLoggedInSet.Add(userName) |>ignore
                    printfn "User %s Logged In" userName 
            |AllTweets->
                mailbox.Sender()<?TweetsCollection|>ignore
            |_ -> printf ""
        return! loop()
    }
    loop()