using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using AsyncIO;
using NetMQ;
using NetMQ.Sockets;
using UnityEngine;

/// <summary>
///     Example of requester who only sends Hello. Very nice guy.
///     You can copy this class and modify Run() to suits your needs.
///     To use this class, you just instantiate, call Start() when you want to start and Stop() when you want to stop.
/// </summary>
public class HelloRequester : RunAbleThread
{
    /// <summary>
    ///     Request Hello message to server and receive message back. Do it 10 times.
    ///     Stop requesting when Running=false.
    /// </summary>
    protected override void Run()
    {
        ForceDotNet.Force(); // this line is needed to prevent unity freeze after one use, not sure why yet
        using (RequestSocket client = new RequestSocket())
        {
            client.Connect("tcp://localhost:5555");
			string message = ShipDialogue.message;

            for (int i = 0; i < 1 && Running; i++)
            {
                Debug.Log("Sending Hello");
                client.SendFrame(message);
                // ReceiveFrameString() blocks the thread until you receive the string, but TryReceiveFrameString()
                // do not block the thread, you can try commenting one and see what the other does, try to reason why
                // unity freezes when you use ReceiveFrameString() and play and stop the scene without running the server
//                string message = client.ReceiveFrameString();
//                Debug.Log("Received: " + message);
                // string message = null;
				
				// message = client.ReceiveFrameString();
				
                bool gotMessage = false;
                while (Running)
                {
                    gotMessage = client.TryReceiveFrameString(out message); // this returns true if it's successful
                    if (gotMessage) break;
                }

                if (gotMessage)
				{
					Debug.Log("Received " + message);

                    // string input = "From: Atago\nTo: Roon\nAction: 1\nAnswer:this is an answer.\nAffection: 10";
                    Dictionary<string, string> dict = Regex.Matches(message, @"(\w+):\s*(.*)")
                        .Cast<Match>()
                        .ToDictionary(m => m.Groups[1].Value, m => m.Groups[2].Value);
                    // remove the additional descriptions in Action and Affection in case LLM generated these (obselet)
                    string pattern = @"\d+";
                    MatchCollection matches = Regex.Matches(dict["ActionOut"], pattern);
                    dict["ActionOut"] = matches[0].Value;
                    matches = Regex.Matches(dict["Affection"], pattern);
                    dict["Affection"] = matches[0].Value;

                    foreach (KeyValuePair<string, string> kvp in dict)
                    {
                        Debug.Log(kvp.Key + kvp.Value);
                    } 

                    // update the affection based on LLM output
                    GameManager.instance.Affection[ShipDialogue.To+"-"+ShipDialogue.From] = Int32.Parse(dict["Affection"]);

                    // update the message for the next LLM input
                    string name1 = ShipDialogue.To;
                    ShipDialogue.To = ShipDialogue.From;
                    ShipDialogue.From = name1;
                    ShipDialogue.Affection = GameManager.instance.Affection[ShipDialogue.To+'-'+ShipDialogue.From];
                    ShipDialogue.Action = Int32.Parse(dict["ActionOut"]);
                    ShipDialogue.Words = dict["Answer"];
                    message = "From: "+ShipDialogue.From+"\nTo: "+ShipDialogue.To+"\nWords: "+ShipDialogue.Words+"\nActionIn: "+ShipDialogue.Action.ToString()+"\nAffection: "+ShipDialogue.Affection.ToString();

					ShipDialogue.message = message;
                    ShipDialogue.messageDisplay = "From: "+ShipDialogue.From+"\nTo: "+ShipDialogue.To+"\n\n"+ShipDialogue.Words;

                    ShipDialogue.newMsg = true;
                    
				}
            }
        }

        NetMQConfig.Cleanup(); // this line is needed to prevent unity freeze after one use, not sure why yet
    }
	

}