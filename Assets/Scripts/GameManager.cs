using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
public enum Factions { TeamRed, TeamBlue, Neutral };
public class GameManager : MonoBehaviour {

    public static GameManager instance = null;
    public Text mainText, statusText, affectionTextFrom, affectionTextTo, alterText;
    public bool battleIsOn = false;
    private float battleTime = 0;
    public List<Transform> teamRed = new List<Transform>();
    public List<Transform> teamBlue = new List<Transform>();
    public List<Transform> fighters = new List<Transform>();
    public int ShipsDestroyed;
    bool ShowStats = true;

    // ship girl name list and affection data
    public List<string> ShipGirl = new List<string>();

    public IDictionary<string, int> Affection = new Dictionary<string, int>();

    // ship girl sprite dictionary
    public IDictionary<string, Sprite> SpriteDic = new Dictionary<string, Sprite>();
    public Sprite[] spriteArray;

    void Awake()
    {
        //just a simple singleton pattern for the game manager
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }
    void Start()
    {
        battleIsOn = false;
        Cursor.visible = false;
        //initialize ship gril names and affections among all of them
        ShipGirl.Add("Atago");
        ShipGirl.Add("Roon");
        //initialize the 2 by 2 affections among ship girls
        foreach (string name1 in ShipGirl){
            foreach (string name2 in ShipGirl){
                Affection.Add(name1+'-'+name2, 25);
            }
            // affection to the commander
            Affection.Add(name1+'-'+"SKK",90);
            Affection.Add("SKK"+'-'+name1,90);
        }

        // assign sprite to ship girls (need to update it to assign by name of the sprites in the asset folder)
        int i = 0;
        foreach (string name in ShipGirl){
            SpriteDic.Add(name, spriteArray[i+1]);
            i = i + 1;
        }
        SpriteDic.Add("SKK", spriteArray[0]);

        alterText.text = "";

    }

   
   
    private void Update()
    {
        if (battleIsOn) battleTime += Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.I))
        {
            ShowStats = !ShowStats;
            StatusUpdate();
            // battleIsOn = !battleIsOn;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }
    public void CheckWin()
    {
        if (teamBlue.Count == 0)
        {
            battleIsOn = false;
            mainText.text = "<color=red>RED TEAM WON.\nBATTLE LASTED " + (battleTime % 60).ToString("F1") + " SECONDS</color>";
        }
        else if (teamRed.Count == 0)
        {
            battleIsOn = false;
            mainText.text = "<color=blue>BLUE VICTORY.\nBATTLE LASTED " + (battleTime % 60).ToString("F1") + " SECONDS</color>";
        }
        else
            mainText.text = "";
    }
    public void StatusUpdate()
    {
        string o = "";
        o = "EPIC SPACE COMBAT\n";
        o += "(press i to toggle info)\n\n";
        if (ShowStats)
        {
            o += "======================\n";
            o += "Total ships: " + fighters.Count + "\n";
            o += "Destroyed ships: " + ShipsDestroyed + "\n\n";
            o += "\nTeam Red: " + teamRed.Count;
            o += "\nTeam Blue: " + teamBlue.Count;
            o += "\n\n";
            //sort the fighters according to their kills
            fighters = fighters.OrderBy(e => e.GetComponent<AIFighterPilot>().kills).ToList();
            fighters.Reverse();
            foreach (Transform t in fighters)
            {
                o += t.name + "\t" + t.GetComponent<AIFighterPilot>().kills + " kills\n";
            }

            
        }
        statusText.text = o;

        
    }
}