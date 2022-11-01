using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

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

    public RodAction(float torque, float force, bool gripped)
    {
        this.torque = torque;
        this.force = force;
        this.gripped = gripped;
    }
}

public class HandState
{
    public float angle;
    public float angularVelocity;
    public float position;
    public RodType rodType;
    public bool hasPostActed = false;

    public HandState(float angle, float angularVelocity, float position, RodType rodType){
        this.angle = angle;
        this.angularVelocity = angularVelocity;
        this.position = position;
        this.rodType = rodType;
    }

}

public class FoosballAgent : Agent
{

    private float m_Existential;

    [Header("Component References")]
    public FoosballAgent opponent;

    [Header("Game Stats")]
    [SerializeField]
    public PlayerType playerType = PlayerType.PLAYER1;
    public Team team = Team.RED;

    public HandState leftHandState;
    public HandState rightHandState;

    // States for action
    public bool hasActed = false;

    // Private vars

    ActionSegment<int> frameDiscreteActions;
    ActionSegment<float> frameContinuousActions;

    // References to components
    FoosballEnvController envController;
    FoosballSettings m_foosballSettings;
    FoosballEnvController.AgentInfo m_agentInfo;

    EnvironmentParameters m_ResetParams;

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
        leftHandState = new HandState(0f, 0f, 0f, RodType.GOALIE);
        rightHandState = new HandState(0f, 0f, 0f, RodType.GOALIE);
        frameDiscreteActions = new ActionSegment<int>();
        frameContinuousActions = new ActionSegment<float>();

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
        hasActed = false;
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
        FoosmenState[] opponentStates = opponent.GetFoosmenStates();
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
        for (int i = 0; i < 4; i++)
        {
            Rod rod = envController.table.rods[i + ((int)team) * 4];
            states[i] = rod.GetState();
        }
        return states;
    }

    public void Move(ActionSegment<float> continuousActions, ActionSegment<int> discreteActions)
    {
        leftHandState.hasPostActed = false;
        rightHandState.hasPostActed = false;
        frameContinuousActions = continuousActions;
        frameDiscreteActions = discreteActions;
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
        /*if (leftHandDesiredType == RodType.NONE){
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
                Debug.Log(le)
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
        }*/
        HandleHand(leftHandDesiredType, ref leftHandState.position, leftDesiredTorque, leftDesiredForce, ref rodActions, ref leftHandState.rodType);
        HandleHand(rightHandDesiredType, ref rightHandState.position, rightDesiredTorque, rightDesiredForce, ref rodActions, ref rightHandState.rodType);


        // Set the rod actions
        envController.table.MoveTeam(rodActions, team);

        hasActed = true;
        envController.AttemptPostAction();
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
                    handPosition = Mathf.Min(rodPos, handPosition + m_foosballSettings.handVelocity * Time.deltaTime);
                }
                else
                {
                    handPosition = Mathf.Max(rodPos, handPosition - m_foosballSettings.handVelocity * Time.deltaTime);
                }
                // If within the tolerance, snap it to the position
                if (Mathf.Abs(handPosition - rodPos) < m_foosballSettings.handGripMaxDistance)
                {
                    handPosition = rodPos;
                }
            }
        }
    }

    public void PostAction()
    {
        if (!leftHandState.hasPostActed){
            // Means that the hand wasn't used at all for a rod, so we need to calculate it ourselves
            leftHandState.angularVelocity += m_foosballSettings.maxHandTorque;
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
        
        if (m_foosballSettings.effectsAmount != 0)
        {
            Arm leftArm = envController.table.arms[team][ArmHandedness.LEFT];
            Arm rightArm = envController.table.arms[team][ArmHandedness.RIGHT];
            if (leftArm != null) leftArm.UpdateHand(leftHandState.position, leftHandState.rodType);
            if (rightArm != null) rightArm.UpdateHand(rightHandState.position, rightHandState.rodType);
        }

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

            if (Input.GetKey(KeyCode.Alpha1))
            {
                discreteActionsOut[(int)DiscreteActionIndexSingles.LEFT_TYPE] = (int)RodType.GOALIE;
            }
            if (Input.GetKey(KeyCode.Alpha2))
            {
                discreteActionsOut[(int)DiscreteActionIndexSingles.LEFT_TYPE] = (int)RodType.DEFENDERS;
            }
            if (Input.GetKey(KeyCode.Alpha3))
            {
                discreteActionsOut[(int)DiscreteActionIndexSingles.LEFT_TYPE] = (int)RodType.MIDFIELDERS;
            }
            if (Input.GetKey(KeyCode.Alpha4))
            {
                discreteActionsOut[(int)DiscreteActionIndexSingles.LEFT_TYPE] = (int)RodType.OFFENSIVE;
            }
            if (Input.GetKey(KeyCode.Alpha5))
            {
                discreteActionsOut[(int)DiscreteActionIndexSingles.LEFT_TYPE] = (int)RodType.NONE;
            }
            if (Input.GetKey(KeyCode.Alpha6))
            {
                discreteActionsOut[(int)DiscreteActionIndexSingles.RIGHT_TYPE] = (int)RodType.GOALIE;
            }
            if (Input.GetKey(KeyCode.Alpha7))
            {
                discreteActionsOut[(int)DiscreteActionIndexSingles.RIGHT_TYPE] = (int)RodType.DEFENDERS;
            }
            if (Input.GetKey(KeyCode.Alpha8))
            {
                discreteActionsOut[(int)DiscreteActionIndexSingles.RIGHT_TYPE] = (int)RodType.MIDFIELDERS;
            }
            if (Input.GetKey(KeyCode.Alpha9))
            {
                discreteActionsOut[(int)DiscreteActionIndexSingles.RIGHT_TYPE] = (int)RodType.OFFENSIVE;
            }
            if (Input.GetKey(KeyCode.Alpha0))
            {
                discreteActionsOut[(int)DiscreteActionIndexSingles.RIGHT_TYPE] = (int)RodType.NONE;
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
