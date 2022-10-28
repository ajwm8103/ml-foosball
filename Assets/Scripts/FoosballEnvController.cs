using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TableType { ENGSCI };
public enum RulesType { ENGSCI, GLOBAL };
public enum PlayerType { PLAYER1, PLAYER2, STAND, BOT};
public enum RodType { GOALIE, DEFENDERS, MIDFIELDERS, OFFENSIVE, NONE };
public enum ArmHandedness { LEFT, RIGHT };
public enum Team { RED, BLUE };
public enum DiscreteActionValueDoubles { GOALIE_HOLD, DEFENDERS_HOLD, MIDFIELDERS_HOLD, OFFENSIVE_HOLD };
public enum DiscreteActionIndexSingles { LEFT_TYPE, RIGHT_TYPE };
public enum ContinuousActionIndexSingles { LEFT_TORQUE, LEFT_FORCE, RIGHT_TORQUE, RIGHT_FORCE };
public enum ContinuousActionIndexDoubles { GOALIE_TORQUE, GOALIE_FORCE, DEFENDERS_TORQUE, DEFENDERS_FORCE, MIDFIELDERS_TORQUE, MIDFIELDERS_FORCE, OFFENSIVE_TORQUE, OFFENSIVE_FORCE };
public class FoosballEnvController : MonoBehaviour
{
    // Match Params
    [Header("Match Params")]
    public int maxSteps = 1000;

    // Match details
    [Header("Match Stats")]
    public List<AgentInfo> agents;
    public int totalSteps;

    [System.Serializable]
    public class AgentInfo
    {
        public int points = 0; // until ma
        public FoosballAgent agent;
        [HideInInspector]
        public float totalReward;
        public int totalWins = 0;

        public void AddReward(float x)
        {
            agent.AddReward(x);
            totalReward += x;
        }
    }
    private AgentInfo m_blueAgent;
    private AgentInfo m_redAgent;

    public Table table;


    // Start is called before the first frame update
    void Start()
    {
        table.Setup(this);
        ResetEnv();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ResetEnv()
    {
        totalSteps = 0;
        //firstTeam = Random.Range(0, 2) == 1 ? Team.RED : Team.BLUE;

        // Reset Agents
        foreach (AgentInfo agentInfo in agents)
        {
            //Debug.Log(string.Format("{0} {1}", agentInfo.agent.name, agentInfo.totalReward));
            //var randomPosX = Random.Range(-5f, 5f);
            agentInfo.points = 0;
            agentInfo.totalReward = 0f;
            agentInfo.agent.SetAgentInfo(agentInfo);
            agentInfo.agent.ResetAgent();
            // Do something for respawning, randomize spawn points + instant spawn at start
        }
    }

    public void AttemptPostAction(){
        // Check if PostAction is valid
        bool valid = true;
        foreach (AgentInfo agentInfo in agents)
        {
            if (!agentInfo.agent.hasActed)
            {
                valid = false;
                break;
            }
        }
        if (!valid) return;

        foreach (AgentInfo agentInfo in agents)
        {
            //Debug.Log(string.Format("{0} {1}", agentInfo.agent.team, agentInfo.totalReward));
            //agentInfo.agent.PostAction();

            agentInfo.agent.hasActed = false;
        }

        table.PostAction();
    }
}
