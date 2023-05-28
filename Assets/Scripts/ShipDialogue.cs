using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using UnityEngine;
using TMPro;
// websocket for LangChain scripts
using AsyncIO;
using NetMQ;
using NetMQ.Sockets;

public class ShipDialogue : MonoBehaviour
{
    //mark whether there is a new message from python server to display
    public static bool newMsg = false;
    //mark whether SKK is speaking
    bool SKKSpeaking = false;
    bool Running = false;
    
    public static string message = "";
    public static string messageDisplay = "";
    
    //initialize the message between ship girls
    public static string From = "Atago";
    public static string To = "Roon";
    public static int Affection = 25;
    public static int Action = 1;
    public static string Words = "Roon, why are you stalking the commander!? You have no chance of winning commander's affection. Commander only loves the big sister type. Yander ship girls like you are too violent for the commander.";
    
    public TextMeshProUGUI textComponent;  
    private HelloRequester _helloRequester;

    // sprite Render
    public SpriteRenderer spriteRendererFrom;
    public SpriteRenderer spriteRendererTo;
    
    void ChangeSprite()
    {
        // change sprite by the conversation character, From which and To which
        spriteRendererFrom.sprite = GameManager.instance.SpriteDic[ShipDialogue.From]; 
        spriteRendererTo.sprite = GameManager.instance.SpriteDic[ShipDialogue.To]; 
    }    

    // Start is called before the first frame update
    void Start()
    {
        // get receiver's affection towards sender
        Affection = GameManager.instance.Affection[To+'-'+From];
        message = "From: "+From+"\nTo: "+To+"\nWords: "+Words+"\nActionIn: "+Action.ToString()+"\nAffection: "+Affection.ToString();
        messageDisplay = "From: "+From+"\nTo: "+To+"\n\n"+Words;
        Running = true;
        textComponent.text = messageDisplay;
        _helloRequester = new HelloRequester();
        _helloRequester.Start();

        // Update affection display
        GameManager.instance.affectionTextFrom.text = "Affection: "+GameManager.instance.Affection[ShipDialogue.From+'-'+ShipDialogue.To];
        GameManager.instance.affectionTextTo.text = "Affection: "+GameManager.instance.Affection[ShipDialogue.To+'-'+ShipDialogue.From];

    }

    void UpdateDialogue()
    {
            // update the battle status based on LLM Action output
            if (Action == 1)
            {
                GameManager.instance.battleIsOn = true;
                GameManager.instance.alterText.text = ShipDialogue.From+" launches drones to attack "+ShipDialogue.To+"!!";
            }
            else
            {
                GameManager.instance.battleIsOn = false;
                GameManager.instance.alterText.text = "The battle has stopped.";
            }

            textComponent.text = messageDisplay;
            _helloRequester = new HelloRequester();
            _helloRequester.Start();
            ChangeSprite();

            // Update affection display
            GameManager.instance.affectionTextFrom.text = "Affection: "+GameManager.instance.Affection[ShipDialogue.From+'-'+ShipDialogue.To];
            GameManager.instance.affectionTextTo.text = "Affection: "+GameManager.instance.Affection[ShipDialogue.To+'-'+ShipDialogue.From];

    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetMouseButtonDown(0))
        if (newMsg & Running)
        {
            UpdateDialogue();
            newMsg = false;
            if (SKKSpeaking)
            {
                //stop after SKK speaking
                Running = false;
            }
        }
    }

    public void ReadStringInput(string s)
    {
        //Debug.Log(s);
        //compile player's message to the ship girl
        _helloRequester.Stop();
        To = "Atago";
        From = "SKK";
        Affection = GameManager.instance.Affection[To+'-'+From];
        Action = 2;
        message = "From: Commander\nTo: "+To+"\nWords: "+s+"\nActionIn: "+Action.ToString()+"\nAffection: "+Affection.ToString();
        messageDisplay = "From: "+From+"\nTo: "+To+"\n\n"+s;
        //update diaglogue box
        textComponent.text = messageDisplay;
        //send request to python server
        _helloRequester = new HelloRequester();
        _helloRequester.Start();
        //update sprite
        ChangeSprite();

        SKKSpeaking = true;
       
    }

    void Stop()
    {
        Running = false;
    }

    IEnumerator TypeLine()
    {
        textComponent.text = "123";
        yield return new WaitForSeconds(1);
    }

    IEnumerator RequesterLine()
    {
        ForceDotNet.Force();
        using (RequestSocket client = new RequestSocket())
        {
            client.Connect("tcp://localhost:5555");
            string message = textComponent.text;

            for (int i = 0; i < 1; i++)
            {
                client.SendFrame(message);
                bool gotMessage = false;
                while (Running)
                {
                    gotMessage = client.TryReceiveFrameString(out message); // this returns true if it's successful
                    if (gotMessage) break;
                } 
                if (gotMessage) Debug.Log("Received " + message);
                textComponent.text = message;
            }
        }
        NetMQConfig.Cleanup();
        yield return new WaitForSeconds(1);
    }

}
