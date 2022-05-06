module MessageType

open System

open System.Collections.Generic

type Tweet=
    {
        Type:string
        User:string
        TweetText : String;
        HashTag : List<String>;
        Mentions : List<String>;
    }
type SocketConnectionObject =
    {
        Type:string
        User:string
    }
type SampleResponseType =
    {
        message:string
    }
type TwitterServer =
    |InitiateTimers
    |RegisterUser of String
    |TweetMessage of Tweet
    |QuerySubscribers of string
    |QueryHashTags of string
    |QueryMentions of string
    |Login of string
    |Logout of string
    |Simulate
    |Retweet of string * string 
    |PrintQueryTag of string
    |PrintQueryMention of  string
    |PrintQuerySubs 
    |Subscribe of string * string
    |AllTweets
    |Done