using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class Game : MonoBehaviour {

    public BlueHome blueHome;
    public RedHome redHome;

    public int scoreBlue;
    public int scoreRed;

    public bool someoneOnRedFlag;
    public bool someoneOnBlueFlag;

    public Flag blueFlag;
    public Flag redFlag;

    private List<NPC> reds;
    private List<NPC> blues;

    public GameObject redFlagPeg;
    public GameObject blueFlagPeg;
    public GameObject plane;

    public Text redScoreText;
    public Text blueScoreText;
    public Text winningText;

    public enum AIBehaviour { AI_BEHAVIOUR_1, AI_BEHAVIOUR_2 };
    public AIBehaviour aiBehaviour;

    public bool gameover = false;

    // Use this for initialization
    void Start () {

        scoreRed = 0;
        scoreBlue = 0;

        blues = new List<NPC>();
        reds = new List<NPC>();

        someoneOnRedFlag = false;
        someoneOnBlueFlag = false;

        NPC[] npcs = GameObject.FindObjectsOfType<NPC>();

        foreach (NPC npc in npcs)
        {

            if (npc.gameObject.tag == "BlueTeam")
                blues.Add(npc);
            else if (npc.gameObject.tag == "RedTeam")
                reds.Add(npc);
        }

        aiBehaviour = AIBehaviour.AI_BEHAVIOUR_2;

    }
	
	// Update is called once per frame
	void FixedUpdate () {

        if (!gameover)
        {

            bool activeBlueFound = false;
            bool activeRedFound = false;

            foreach (NPC blue in blues)
            {
                if (blue.state != NPC.State.FROZEN)
                {
                    activeBlueFound = true;
                    break;
                }
            }

            foreach (NPC red in reds)
            {
                if (red.state != NPC.State.FROZEN)
                {
                    activeRedFound = true;
                    break;
                }
            }


            redScoreText.text = "Red: " + scoreRed;
            blueScoreText.text = "Blue: " + scoreBlue;

            if (!activeBlueFound)
            {
                winningText.text = "All Blue Team frozen. Red Team Wins!";
                gameover = true;
            }

            else if (!activeRedFound)
            {
                winningText.text = "All Red Team frozen. Blue Team Wins!";
                gameover = true;
            }



            if (Input.GetKeyDown(KeyCode.S))
            {
                aiBehaviour = AIBehaviour.AI_BEHAVIOUR_2;
            }
            else if (Input.GetKeyDown(KeyCode.K)) {
                aiBehaviour = AIBehaviour.AI_BEHAVIOUR_1;
            }

            foreach (NPC blue in blues)
            {



                if (blue.target == null && blue.pursuer == null && blue.isHome && !someoneOnRedFlag)
                {
                    someoneOnRedFlag = true;
                    blue.CaptureFlag();
                    continue;
                }

                if (blue.state != NPC.State.PURSUING && blue.state != NPC.State.CAPTURING && blue.state != NPC.State.FROZEN)
                {

                    NPC closestSaveableNPC = null;

                    foreach (NPC teammate in blues)
                    {
                        if (blue == teammate || teammate.state != NPC.State.FROZEN)
                        {
                            continue;
                        }

                        if (closestSaveableNPC == null || (blue.transform.position - teammate.transform.position).magnitude < (blue.transform.position - closestSaveableNPC.transform.position).magnitude)
                        {
                            if (teammate.pursuer == null) {
                                teammate.pursuer = blue;
                                closestSaveableNPC = teammate;
                            }
                        }

                    }

                    if (closestSaveableNPC != null)
                    {

                        blue.Unfreeze(closestSaveableNPC);
                        continue;
                    }

                }

                if (blue.state == NPC.State.PURSUING)
                    blue.StopPursuing();

                if (blue.target == null && blue.pursuer == null && blue.isHome && blue.state != NPC.State.CAPTURING && blue.state != NPC.State.FROZEN)
                {

                    NPC closestTargetableNPC = null;

                    foreach (NPC red in reds)
                    {
                        if (!red.isHome && red.state != NPC.State.FROZEN)
                        {
                            if (closestTargetableNPC == null || (blue.transform.position - red.transform.position).magnitude < (blue.transform.position - closestTargetableNPC.transform.position).magnitude)
                            {
                                if (red.pursuer == null)
                                    closestTargetableNPC = red;
                            }
                        }
                    }

                    if (closestTargetableNPC != null)
                    {
                        blue.Pursue(closestTargetableNPC);
                        continue;
                    }

                }





            }

            foreach (NPC red in reds)
            {


                if (red.target == null && red.pursuer == null && red.isHome && !someoneOnBlueFlag)
                {
                    someoneOnBlueFlag = true;
                    red.CaptureFlag();
                    continue;
                }

                if (red.state != NPC.State.PURSUING && red.state != NPC.State.CAPTURING && red.state != NPC.State.FROZEN)
                {
                    NPC closestSaveableNPC = null;

                    foreach (NPC teammate in reds)
                    {

                        if (red == teammate || teammate.state != NPC.State.FROZEN)
                        {
                            continue;
                        }

                        if (closestSaveableNPC == null || (red.transform.position - teammate.transform.position).magnitude < (red.transform.position - closestSaveableNPC.transform.position).magnitude)
                        {
                            if (teammate.pursuer == null)
                            {
                                teammate.pursuer = red;
                                closestSaveableNPC = teammate;
                            }
                        }

                    }

                    if (closestSaveableNPC != null)
                    {

                        red.Unfreeze(closestSaveableNPC);
                        continue;
                    }

                }

                if (red.state == NPC.State.PURSUING)
                    red.StopPursuing();

                if (red.target == null && red.pursuer == null && red.isHome && red.state != NPC.State.CAPTURING && red.state != NPC.State.FROZEN)
                {

                    NPC closestTargetableNPC = null;

                    foreach (NPC blue in blues)
                    {

                        if (!blue.isHome && blue.state != NPC.State.FROZEN)
                        {
                            if (closestTargetableNPC == null || (red.transform.position - blue.transform.position).magnitude < (red.transform.position - closestTargetableNPC.transform.position).magnitude)
                            {
                                if (blue.pursuer == null)
                                    closestTargetableNPC = blue;
                            }
                        }
                    }



                    if (closestTargetableNPC != null)
                    {
                        red.Pursue(closestTargetableNPC);
                        continue;
                    }

                }





            }

        }

    }

    public void ScoreBlue()
    {
        scoreBlue++;
        redFlag.carrier.SetToWander();
        RestoreRedFlag();
    }

    public void ScoreRed()
    {
        scoreRed++;
        blueFlag.carrier.SetToWander();
        RestoreBlueFlag();
    }

    public void RestoreRedFlag()
    {
        someoneOnRedFlag = false;
        redFlag.carrier = null;
        redFlag.transform.position = new Vector3(redFlagPeg.transform.position.x, redFlag.transform.position.y - 10, redFlagPeg.transform.position.z);
    }

    public void RestoreBlueFlag()
    {
        someoneOnBlueFlag = false;
        blueFlag.carrier = null;
        blueFlag.transform.position = new Vector3(blueFlagPeg.transform.position.x, blueFlag.transform.position.y - 10, blueFlagPeg.transform.position.z);
    }

    private void SwitchBehaviour()
    {
        if (aiBehaviour == AIBehaviour.AI_BEHAVIOUR_1)
            aiBehaviour = AIBehaviour.AI_BEHAVIOUR_2;
        else
            aiBehaviour = AIBehaviour.AI_BEHAVIOUR_1;
    }
}
