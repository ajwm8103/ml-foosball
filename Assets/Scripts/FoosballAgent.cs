using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class FoosballAgent : Agent
{

    private float m_Existential;

    [Header("Game Stats")]
    [SerializeField]
    public PlayerType playerType = PlayerType.PLAYER1;
    public Team team = Team.RED;

    public RodType leftHandRodType = RodType.NONE;
    public RodType rightHandRodType = RodType.NONE;
    public float leftHandPosition = 0f; // literal position
    public float rightHandPosition = 0f; // literal position

    // Private vars

    // References to components
    FoosballAgent m_opponent;
    FoosballEnvController envController;
    FoosballSettings m_foosballSettings;
    FoosballEnvController.AgentInfo m_agentInfo;

    EnvironmentParameters m_ResetParams;

    public struct FoosmenState
    {
        public float position;
        public float angle;
        public float velocity;
        public float angularVelocity;
        public FoosmenState(float position, float angle, float velocity, float angularVelocity)
        {
            this.position = position;
            this.angle = angle;
            this.velocity = velocity;
            this.angularVelocity = angularVelocity;
        }

        public void AddObservations(VectorSensor sensor)
        {
            sensor.AddObservation(position);
            sensor.AddObservation(angle);
            sensor.AddObservation(velocity);
            sensor.AddObservation(angularVelocity);
        }
    }

    public struct RodAction
    {
        public float torque;
        public float force;
        public bool gripped;

        public RodAction(float torque, float force, bool gripped){
            this.torque = torque;
            this.force = force;
            this.gripped = gripped;
        }
    }

    // cause i basically need an input for each rod to control the spin. so i'm thinking grip + torque, both with constraints to human levels
    // but...how does grip work..???? high grip + high torque means?

    // grip is how tightly you're holding the rod. cause if i'm not holding it, then it'll just go weeeee
    // but if i am holding it, it'll stay where it is..???? not sure honestly.
    // cause like, if i want it to stay in a spot i'm gripping, when it gets hit i automatically apply a torque..? or like a spring constant i guess
    // but when i'm moving it i'm applying a more direct torque, but also have a high spring constant in case i get hit.?
    // so perhaps torque is torque and grip is spring constant on my grip
    // so high torque necessitates high grip, but high grip doesn't necessitate high torque...
    // idk. just trying to get all the simulation dynamics of real play.

    // what if i increase the torque a ton so the angular velocity is high. then let go? and want it to keep spinning?
    // that would mean i have zero applied torque, sure. but my grip would instantly stop me.
    // i like that actually. ok ok.
    // so
    //        high torque                  no torque
    // grip   does the torque with high k  stays still with high k

    // no grip floppy mode                 "        "

    // i'll try grip as a toggle then. and torque as a value w/ a clamp.
    // also grip will have a min time. so it takes time to change grip and we can't just spam it per frame.

    public override void Initialize()
    {
        base.Initialize();
        m_foosballSettings = FindObjectOfType<FoosballSettings>();

        envController = transform.parent.GetComponentInParent<FoosballEnvController>();
        m_Existential = 1f / envController.maxSteps;

        // Set params ?

        m_ResetParams = Academy.Instance.EnvironmentParameters;

        ResetAgent();
    }

    public void SetAgentInfo(FoosballEnvController.AgentInfo x)
    {
        m_agentInfo = x;
    }

    public void ResetAgent()
    {
        // Reset positions, velocities, angular velocities
    }

    // Called to start the game
    public override void OnEpisodeBegin()
    {
        ResetAgent();
    }

    // Called to see the same
    // josh don't touch cause it sees AI
    public override void CollectObservations(VectorSensor sensor)
    {
        // My state
        FoosmenState[] myStates = GetFoosmenStates();
        foreach (FoosmenState state in myStates)
        {
            state.AddObservations(sensor);
        }

        // Opponent state
        FoosmenState[] opponentStates = m_opponent.GetFoosmenStates();
        foreach (FoosmenState state in opponentStates)
        {
            state.AddObservations(sensor);
        }

        // Global state
        sensor.AddObservation(envController.totalSteps / envController.maxSteps);
    }

    public FoosmenState[] GetFoosmenStates()
    {
        FoosmenState[] states = new FoosmenState[4];
        states[0] = new FoosmenState();
        states[1] = new FoosmenState();
        states[2] = new FoosmenState();
        states[3] = new FoosmenState();
        return states;
    }

    public void Move(ActionSegment<float> continuousActions, ActionSegment<int> discreteActions)
    {
        // If hand not on desired rod, start moving it there. Otherwise twist!

        // Action data
        RodType leftHandDesiredType = (RodType)discreteActions[(int)DiscreteActionIndexSingles.LEFT_TYPE];
        RodType rightHandDesiredType = (RodType)discreteActions[(int)DiscreteActionIndexSingles.RIGHT_TYPE];
        float leftDesiredTorque = continuousActions[(int)ContinuousActionIndexSingles.LEFT_TORQUE];
        float leftDesiredForce = continuousActions[(int)ContinuousActionIndexSingles.LEFT_FORCE];
        float rightDesiredTorque = continuousActions[(int)ContinuousActionIndexSingles.RIGHT_TORQUE];
        float rightDesiredForce = continuousActions[(int)ContinuousActionIndexSingles.RIGHT_FORCE];

        Dictionary<RodType, float> rodPercent = new Dictionary<RodType, float> {
            {RodType.GOALIE, envController.table.tso.goalieLengthPercent.GetValue() },
            {RodType.DEFENDERS, envController.table.tso.defendersLengthPercent.GetValue() },
            {RodType.MIDFIELDERS, envController.table.tso.midfieldersLengthPercent.GetValue() },
            {RodType.OFFENSIVE, envController.table.tso.offensiveLengthPercent.GetValue() }
        };
        Dictionary<RodType, RodAction> rodActions = new Dictionary<RodType, RodAction> {
            {RodType.GOALIE, new RodAction(0f, 0f, false) },
            {RodType.DEFENDERS, new RodAction(0f, 0f, false) },
            {RodType.MIDFIELDERS, new RodAction(0f, 0f, false) },
            {RodType.OFFENSIVE, new RodAction(0f, 0f, false) }
        };

        // Left
        if (leftHandDesiredType == RodType.NONE){
            // Wants to let go of the rod, ok
            leftHandRodType = RodType.NONE;
        } else {
            // Trying to hold a rod
            float rodPos = envController.table.tso.tableLength * rodPercent[leftHandDesiredType];
            if (Mathf.Abs(leftHandPosition - rodPos) < m_foosballSettings.handGripMaxDistance)
            {
                // Can act
                // Set left hand 
                leftHandRodType = leftHandDesiredType;
                rodActions[leftHandRodType] = new RodAction(leftDesiredTorque, leftDesiredForce, true);

            }
            else
            {
                // Move the hand towards the target at hand move speed
                if (rodPos > leftHandPosition){
                    leftHandPosition = Mathf.Min(rodPos, m_foosballSettings.handVelocity * Time.deltaTime);
                } else {
                    leftHandPosition = Mathf.Max(rodPos, -m_foosballSettings.handVelocity * Time.deltaTime);
                }
                // If within the tolerance, snap it to the position
                if (Mathf.Abs(leftHandPosition - rodPos) < m_foosballSettings.handGripMaxDistance)
                {
                    leftHandPosition = rodPos;
                }
            }
        }

        // Move the rods
        envController.table.MoveTeam(continuousActions, discreteActions, team);
    }

    public void HandleHand(RodType handDesiredType, ref float handPosition, float desiredTorque, float desiredForce, ref Dictionary<RodType, RodAction> rodActions, ref RodType handRodType)
    {
        if (handDesiredType == RodType.NONE)
        {
            // Wants to let go of the rod, ok
            handRodType = RodType.NONE;
        }
        else
        {
            // Trying to hold a rod
            float rodPos = envController.table.tso.tableLength * envController.table.rodPercent[handDesiredType];
            if (Mathf.Abs(handPosition - rodPos) < m_foosballSettings.handGripMaxDistance)
            {
                // Can act
                // Set left hand 
                handRodType = handDesiredType;
                rodActions[handRodType] = new RodAction(desiredTorque, desiredForce, true);

            }
            else
            {
                // Move the hand towards the target at hand move speed
                if (rodPos > handPosition)
                {
                    handPosition = Mathf.Min(rodPos, m_foosballSettings.handVelocity * Time.deltaTime);
                }
                else
                {
                    handPosition = Mathf.Max(rodPos, -m_foosballSettings.handVelocity * Time.deltaTime);
                }
                // If within the tolerance, snap it to the position
                if (Mathf.Abs(handPosition - rodPos) < m_foosballSettings.handGripMaxDistance)
                {
                    handPosition = rodPos;
                }
            }
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Actions, size = 9

        m_agentInfo.AddReward(-m_Existential);

        // Move stuff

        Move(actionBuffers.ContinuousActions, actionBuffers.DiscreteActions);

        //MoveAgent(actionBuffers.DiscreteActions, actionBuffers.ContinuousActions);

        // Graphics
        /*
        if (m_foosballSettings.effectsAmount != 0)
        {
            
            m_damageText.text = damage.ToString();
            if (stunFrames > 0)
            {
                m_legendSprite.color = stunColor;
                //m_capsuleSprite.color = stunColor;
            }
            else if (m_sprinting)
            {
                m_legendSprite.color = sprintingColor;
                //m_capsuleSprite.color = sprintingColor;
            }
            else
            {
                m_legendSprite.color = defaultColor;
                //m_capsuleSprite.color = defaultColor;
            }

            // Facing
            m_legendSprite.flipX = facingDirection == -1;

            envController.DisplayAction(team, teamPosition, actionBuffers.DiscreteActions);
        }*/

        // Rewards

        /*if (isAlive)
        {
            if (bottomKO || leftKO || rightKO)
            {
                // Fell off of bottom
                KO();
            }
            else if (topKO && stunFrames != 0)
            {
                KO();
            }
        }*/
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        var continuousActionsOut = actionsOut.ContinuousActions;
        /*discreteActionsOut[(int)DiscreteActionKey.GOALIE_HOLD] = 1;
        discreteActionsOut[(int)DiscreteActionKey.DEFENDERS_HOLD] = 1;
        discreteActionsOut[(int)DiscreteActionKey.MIDFIELDERS_HOLD] = 1;
        discreteActionsOut[(int)DiscreteActionKey.OFFENSIVE_HOLD] = 1;*/
        discreteActionsOut[(int)DiscreteActionIndexSingles.LEFT_TYPE] = (int)RodType.MIDFIELDERS;
        discreteActionsOut[(int)DiscreteActionIndexSingles.RIGHT_TYPE] = (int)RodType.OFFENSIVE;

        if (playerType == PlayerType.PLAYER1)
        {
            if (Input.GetKey(KeyCode.W))
            {
                continuousActionsOut[(int)ContinuousActionIndexSingles.LEFT_FORCE] = 1;
                continuousActionsOut[(int)ContinuousActionIndexSingles.RIGHT_FORCE] = 1;
            }
            if (Input.GetKey(KeyCode.S))
            {
                continuousActionsOut[(int)ContinuousActionIndexSingles.LEFT_FORCE] = -1;
                continuousActionsOut[(int)ContinuousActionIndexSingles.RIGHT_FORCE] = -1;
            }
            if (Input.GetKey(KeyCode.A))
            {
                continuousActionsOut[(int)ContinuousActionIndexSingles.LEFT_TORQUE] = 1;
                continuousActionsOut[(int)ContinuousActionIndexSingles.RIGHT_TORQUE] = 1;
            }
            if (Input.GetKey(KeyCode.D))
            {
                continuousActionsOut[(int)ContinuousActionIndexSingles.LEFT_TORQUE] = -1;
                continuousActionsOut[(int)ContinuousActionIndexSingles.RIGHT_TORQUE] = -1;
            }
        }
        else if (playerType == PlayerType.PLAYER2)
        {
            // idk controller support?
        }
        else if (playerType == PlayerType.STAND)
        {
            // do nothing hurrah
        }
        else if (playerType == PlayerType.BOT)
        {

        }
    }

}
