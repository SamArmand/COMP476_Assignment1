using UnityEngine;
using System;

public class NPC : MonoBehaviour {

    public NPC target;

    public bool haveFlag;
    public bool isHome;

    public NPC pursuer;

    private float moveSpeed; //will double as maxSpeed

    private float maxAcceleration;

    private float nearSpeed;
    private float nearRadius;

    public Game game;

    private int frozen = Animator.StringToHash("Frozen");
    private int unfrozen = Animator.StringToHash("Basic_Run_01");

    public enum State {PURSUING, WANDERING, CAPTURING, UNFREEZE, FROZEN };


    public State state;

    public float speed;

    public GameObject destination;

    private float rotation;
    private Vector3 desiredDirection;

    private Vector3 velocity;

    private float targetRadius = 3f;
    private float slowRadius = 2f;

    private float deltaTime;

    public Vector3 direction;

    // Use this for initialization
    void Start()
    {
        speed = 0;

        moveSpeed = 10;
        nearSpeed = 3;
        nearRadius = 3;

        //isHome = true;
        haveFlag = false;
        state = State.WANDERING;

        desiredDirection = transform.forward;
        rotation = 0;

        maxAcceleration = 10f;

        

    }

    void FixedUpdate()
    {

        if (!game.gameover)
        {

            deltaTime = Time.deltaTime;

            if (state == State.CAPTURING && (destination == null || !haveFlag && ((gameObject.tag == "BlueTeam" && game.redFlag.carrier != null) || (gameObject.tag == "RedTeam" && game.blueFlag.carrier != null))))
            {
                state = State.WANDERING;
            }

            if (state == State.PURSUING && target != null && (transform.position - target.transform.position).magnitude <= 1)
            {
                Touch(target);
            }

            if (state == State.UNFREEZE && target != null && (transform.position - target.transform.position).magnitude <= 1)
            {
                Save(target);
            }

            if (transform.localRotation.x != 0 || transform.localRotation.z != 0)
            {
                transform.localRotation = new Quaternion(0, transform.localRotation.y, 0, 0);
            }

            if (transform.localPosition.y != 0)
            {
                transform.localPosition = new Vector3(transform.localPosition.x, 0, transform.localPosition.z);
            }

            if (transform.position.x > game.plane.transform.position.x + 35)
            {
                transform.position = new Vector3(game.plane.transform.position.x - 32, transform.position.y, transform.position.z);
            }

            else if (transform.position.x < game.plane.transform.position.x - 35)
            {
                transform.position = new Vector3(game.plane.transform.position.x + 32, transform.position.y, transform.position.z);
            }

            if (transform.position.z > game.plane.transform.position.z + 35)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, game.plane.transform.position.z - 32);
            }

            else if (transform.position.z < game.plane.transform.position.z - 35)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, game.plane.transform.position.z + 32);
            }

            if (pursuer == null && target == null && state != State.CAPTURING && state != State.FROZEN && state != State.WANDERING)
            {
                state = State.WANDERING;
            }

            if (pursuer != null && (transform.position - pursuer.transform.position).magnitude < 15)
            {
                if (state == State.CAPTURING && !haveFlag && (transform.position - pursuer.transform.position).magnitude < 5)
                {
                    state = State.WANDERING;

                    if (gameObject.tag == "BlueTeam")
                        game.someoneOnRedFlag = false;
                    else
                        game.someoneOnBlueFlag = false;
                }

                if (game.aiBehaviour == Game.AIBehaviour.AI_BEHAVIOUR_1)
                    KinematicFlee();
                else
                    SteeringFlee();
            }

            else if (state == State.CAPTURING || state == State.PURSUING || state == State.UNFREEZE)
            {

                if (game.aiBehaviour == Game.AIBehaviour.AI_BEHAVIOUR_1)
                    KinematicArrive();
                else
                {
                    SteeringArrive();
                }

            }

            else if (state == State.WANDERING)
            {
                Wander();
            }

            transform.position += velocity * deltaTime;

        }

    }

    void OnTriggerEnter(Collider other)
    {

        if (isHome && state != State.FROZEN && ((gameObject.tag == "BlueTeam" && other.gameObject.tag == "RedTeam") || gameObject.tag == "RedTeam" && other.gameObject.tag == "BlueTeam"))
        {
            if (other.gameObject.GetComponent<NPC>().state != State.FROZEN)
            {
                Touch(other.gameObject.GetComponent<NPC>());

                if (other.gameObject.GetComponent<NPC>().pursuer != null)
                {
                    other.gameObject.GetComponent<NPC>().pursuer.StopPursuing();
                    other.gameObject.GetComponent<NPC>().pursuer = null;
                }
            }
        }

        if (state != State.FROZEN && ((gameObject.tag == "BlueTeam" && other.gameObject.tag == "BlueTeam") || gameObject.tag == "RedTeam" && other.gameObject.tag == "RedTeam"))
        {
            if (other.gameObject.GetComponent<NPC>().state == State.FROZEN)
            {
                Save(other.gameObject.GetComponent<NPC>());

                if (other.gameObject.GetComponent<NPC>().pursuer != null)
                {
                    other.gameObject.GetComponent<NPC>().pursuer.StopPursuing();
                    other.gameObject.GetComponent<NPC>().pursuer = null;
                }
            }
        }

        if (state == State.CAPTURING && gameObject.tag == "BlueTeam" && other.gameObject.tag == "RedFlag" && !haveFlag)
        {
            haveFlag = true;
            game.redFlag.carrier = this;
        }

        else if (state == State.CAPTURING && gameObject.tag == "RedTeam" && other.gameObject.tag == "BlueFlag" && !haveFlag)
        {
            haveFlag = true;
            game.blueFlag.carrier = this;
        }

        else if ((gameObject.tag == "BlueTeam" && other.gameObject.tag == "RedHome") || (gameObject.tag == "RedTeam" && other.gameObject.tag == "BlueHome"))
        {
            isHome = false;
            if (state != State.CAPTURING)
            {
                StopPursuing();
                SetToWander();
            }
        }

        else if ((gameObject.tag == "BlueTeam" && other.gameObject.tag == "BlueHome") || (gameObject.tag == "RedTeam" && other.gameObject.tag == "RedHome"))
        {
            isHome = true;
            if (pursuer != null)
            {
                pursuer.StopPursuing();
            }

            if (haveFlag && gameObject.tag == "BlueTeam")
            {
                haveFlag = false;
                game.ScoreBlue();
            }

            if (haveFlag && gameObject.tag == "RedTeam")
            {
                haveFlag = false;
                game.ScoreRed();
            }

        }
    }



    public void Pursue(NPC target) 
    {
        state = State.PURSUING;
        this.target = target;
        target.pursuer = this;
    }

    public void Unfreeze(NPC target)
    {
        state = State.UNFREEZE;
        this.target = target;
    }

    public void SetToWander()
    {
        state = State.WANDERING;
        destination = null;
    }

    public void Freeze()
    {
        gameObject.GetComponent<Animation>().Play();

        if (state == State.CAPTURING)
        {



            if (haveFlag)
            {
                haveFlag = false;
                if (gameObject.tag == "BlueTeam")
                {
                    game.RestoreRedFlag();
                }
                else if (gameObject.tag == "RedTeam")
                {
                    game.RestoreBlueFlag();
                }
            }

            if (gameObject.tag == "BlueTeam")
            {
                game.someoneOnRedFlag = false;
            }
            else if (gameObject.tag == "RedTeam")
            {
                game.someoneOnBlueFlag = false;
            }

            if (gameObject.tag == "RedTeam")
            {
                Debug.Log("Red capturer frozen " + game.someoneOnBlueFlag);
            }

        }

        state = State.FROZEN;
        velocity = Vector3.zero;
        StopPursuing();

    }

    public void Touch(NPC npc)
    {
        npc.Freeze();
        StopPursuing();
        SetToWander();
    }

    public void Save(NPC npc)
    {
        npc.SetToWander();

        npc.gameObject.GetComponent<Animation>().Play(); ;
        npc.pursuer = null;
        StopPursuing();
        SetToWander();
    }

    public void StopPursuing()
    {
        if (target != null)
            target.pursuer = null;
        this.target = null;
        haveFlag = false;
    }

    public void CaptureFlag()
    {
        state = State.CAPTURING;

        if (gameObject.tag == "BlueTeam")
        {
            destination = game.redFlag.gameObject;
        }
        else if (gameObject.tag == "RedTeam")
        {
            
            destination = game.blueFlag.gameObject;
            
        }

        target = null;
    }

    private void KinematicArrive()
    {
        direction = CalculateDirection();

        if (speed <= nearSpeed)
        {

            //If small distance, speed is ok, just move

            //If larger distance, turn in place, then move
            if ((target != null && (target.transform.position - transform.position).magnitude > nearRadius) || (state == State.CAPTURING && (destination.transform.position - transform.position).magnitude > nearRadius))
            {
                Face(direction);

                if (Vector3.Angle(transform.forward, direction) <= 5)
                {
                    speed = moveSpeed;
                }

            }

        }

        else if (speed > nearSpeed)
        {
            if (Vector3.Angle(transform.forward, direction) <= 22.5)
            {
                Face(direction);
            }

            else
            {
                speed = 0;
            }

        }

        direction.y = transform.forward.y;

        velocity = (direction * speed);
    }

    private void Face(Vector3 direction)
    {

        //create the rotation we need to be in to look at the target
        Quaternion lookRotation = Quaternion.LookRotation(direction);

        float angle = Quaternion.Angle(transform.rotation, lookRotation);
        float timeToComplete = angle / 200f;
        float donePercentage = Mathf.Min(1F, deltaTime / timeToComplete);

        //rotate towards a direction, but not immediately (rotate a little every frame)
        //The 3rd parameter is a number between 0 and 1, where 0 is the start rotation and 1 is the end rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, donePercentage);
    }

    private Vector3 CalculateDirection()
    {
        //Direction Calculation

        direction = Vector3.zero;

        if ((state == State.PURSUING || state == State.UNFREEZE && target != null))
        {
            direction = ((target.transform.position + (target.speed * target.transform.forward * 20)) - transform.position).normalized;
        }

        else if (state == State.CAPTURING && haveFlag && gameObject.tag == "BlueTeam")
        {
            destination = game.blueFlagPeg;
        }

        else if (state == State.CAPTURING && haveFlag && gameObject.tag == "RedTeam")
        {
            destination = game.redFlagPeg;
        }

        if (state == State.CAPTURING)
        {
            direction = new Vector3(destination.transform.position.x - transform.position.x, transform.forward.y, destination.transform.position.z - transform.position.z).normalized;
        }

        if ((target != null && Math.Abs(transform.position.x - target.transform.position.x) > 35))
            direction = new Vector3(direction.x * -1, direction.y, direction.z);

        if ((target != null && Math.Abs(transform.position.z - target.transform.position.z) > 35))
            direction = new Vector3(direction.x, direction.y, direction.z * -1);

        //Speed calculation

        else if (target != null && (target.transform.position - transform.position).magnitude <= nearRadius)
        {
            speed = nearSpeed;
        }

        else if (state == State.CAPTURING && (destination.transform.position - transform.position).magnitude <= nearRadius)
        {
            speed = nearSpeed;
        }

        return direction;
    }

    private void KinematicFlee()
    {

        if (!haveFlag)
        {
            SetToWander();

            if (gameObject.tag == "BlueTeam")
                game.someoneOnRedFlag = false;
            else if (gameObject.tag == "RedTeam")
                game.someoneOnBlueFlag = false;

        }

        Vector3 direction = (transform.position - pursuer.transform.position).normalized;

        //If small distance, speed is ok, just move

        //If larger distance, turn in place, then move
        if ((pursuer.transform.position - transform.position).magnitude > nearRadius)
        {

            speed = 0;

            Face(direction);

            if (Vector3.Angle(transform.forward, direction) <= 5)
            {
                speed = moveSpeed + 3;
            }

        }

        direction.y = transform.forward.y;
        velocity = (direction * speed);
    }

    private void Wander()
    {

        if (Vector3.Angle(transform.forward, desiredDirection) < 2)
        {

            float random = UnityEngine.Random.Range(-1f, 1f);
            rotation = random * 30;

            desiredDirection = Quaternion.AngleAxis(rotation, transform.up) * transform.forward;

        }

        Face(desiredDirection);

        velocity = transform.forward * moveSpeed;

    }

    private void SteeringArrive()
    {

        direction = CalculateDirection();

        if (state == State.PURSUING)
        {
            direction = ((target.transform.position + (target.speed * target.direction * 2)) - transform.position).normalized;
        }

        else if (state == State.UNFREEZE)
        {
            direction = (target.transform.position - transform.position).normalized;
        }

        Vector3 acceleration = Vector3.zero;

        float distance = 0;

        if ((state == State.PURSUING || state == State.UNFREEZE))
        {
            distance = (target.transform.position - transform.position).magnitude;
        }
        else if (state == State.CAPTURING)
        {
            distance = (destination.transform.position - transform.position).magnitude;
        }

        if (distance < targetRadius)
        {
            //do nothing
        }

        if (distance > slowRadius)
        {
            speed = moveSpeed;
        }
        else
        {
            speed = moveSpeed * distance / slowRadius;
        }

        Face(direction);

        if (speed <= nearSpeed)
        {
            //If small distance, speed is ok, just move

            //If larger distance, turn in place, then move
            if ((target != null && (target.transform.position - transform.position).magnitude > nearRadius) || (state == State.CAPTURING && (destination.transform.position - transform.position).magnitude > nearRadius))
            {
                Face(direction);
            }
        }

        else
        {
            if (Vector3.Angle(transform.forward, direction) <= 22.5)
            {
                Face(direction);
            }

        }

        acceleration = maxAcceleration * direction;

        velocity += acceleration * deltaTime;

        if (velocity.magnitude > moveSpeed)
        {
            Debug.Log("SLOWING DOWN");
            velocity.Normalize();
            velocity *= moveSpeed;
        }

    }

    private void SteeringFlee()
    { 

        Vector3 acceleration = Vector3.zero;

        if (!haveFlag)
        {
            SetToWander();

            if (gameObject.tag == "BlueTeam")
                game.someoneOnRedFlag = false;
            else if (gameObject.tag == "RedTeam")
                game.someoneOnBlueFlag = false;

        }

        direction = (transform.position - (pursuer.transform.position + (pursuer.speed * pursuer.direction * 2))).normalized;

        if ((transform.position - pursuer.transform.position).magnitude >= 20)
        {

            Face(direction);

            acceleration = maxAcceleration * direction;

            velocity += acceleration * deltaTime;

            if (velocity.magnitude > moveSpeed + 3)
            {
                velocity.Normalize();
                velocity *= (moveSpeed + 3);
            }

        }



    }

}
