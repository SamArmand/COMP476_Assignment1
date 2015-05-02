using UnityEngine;
using System.Collections;

public class Flag : MonoBehaviour {

    public NPC carrier;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
        if (carrier != null)
        {
            transform.position = carrier.transform.position + new Vector3(0, 10, 0);

            if (carrier.state != NPC.State.CAPTURING)
                carrier.state = NPC.State.CAPTURING;

            if (!carrier.haveFlag)
                carrier.haveFlag = true;

            if (carrier.gameObject.tag == "RedTeam" && carrier.destination != carrier.game.redFlagPeg)
                carrier.destination = carrier.game.redFlagPeg;

            if (carrier.gameObject.tag == "BlueTeam" && carrier.destination != carrier.game.blueFlagPeg)
                carrier.destination = carrier.game.blueFlagPeg;

        }

	}
}
