# Project 4 part II
## COP5615 Distributed Operating Systems Principles

#### Group Members
* Akshay Sharma (UFID - 56006840)
* Shubham Srivastava (UFID - 27599322)

#### Checkpoints

Video uploaded at the link: Project is explained in detail in the video uploaded at the given link.

Video Link : https://drive.google.com/file/d/1kJIXu6y4uJ86gkd5CdMPa6B0ETHET0Rg/view?usp=sharing

*IF not able preview the video, please download the video first*

**What we have completed:**

* You need to design a JSON based API that  represents all messages and their replies (including errors)
* You need to re-write parts of your engine using WebSharper to implement the WebSocket interface
* You need to re-write parts of your client to use WebSockets.


### Description

The aim of this project is to develop a twitter clone and demonstrate it using a client simulator. This is implemented using a server and client model. We have used WebSockets and JSON based API to establish server client connection. We are using Suave library to implement sockets. The server and client are executed on different terminals as different process. The project is divided into three parts:

* **The Server**: server mimics the functionality of a global server which servers all the requests made by the client. The server is webserver implementing sockets and APIs. The request includes functionalities like register, tweet etc.

* **The Client**: The client here is a user. Multiple users can be registered at a time by executing the client from different terminal. se users are log in and log out randomly. Once they are logged in, they make requests to the server.

* **Twitter Engine**: It helps in demonstrating the twitter functionality based on user inputs. The twitter engine is connected to the server here.

The twitter engine demonstrates the following functionalities – register a user, tweet, retweet, query hashtag, query subscribers, query mentions (other users). The operations are selected based on the inputs given by the user.


### Running the project

***libraries required***

Install the following :
```
	F# (v5.0 or greater)
	Nuget
	Akka.net API
```

Following commands should be added to the TwitterAPI.fsx file otherwise there will be an error:

```FSharp
	#r "nuget: Akka.FSharp"
	#r "nuget: Akka.Remote"
```

Command to execute the program, run the following command into the terminal

*To start the server for Twitter Engine*

`dotnet run`

*To start the client for Twitter Engine*

`dotnet fsi sampleClient.fsx`


**program may take large amount of time to execute but this can vary depending on the architecture or also the user given input**
