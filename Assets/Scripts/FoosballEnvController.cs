using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TableType { ENGSCI };
public enum RulesType { ENGSCI, GLOBAL };
public enum PlayerType { PLAYER1, PLAYER2, STAND, BOT};
public enum RodType { GOALIE, DEFENDERS, MIDFIELDERS, OFFENSIVE };
public enum Team { RED, BLUE };
public enum DiscreteActionKeyDoubles { GOALIE_HOLD, DEFENDERS_HOLD, MIDFIELDERS_HOLD, OFFENSIVE_HOLD };
public enum DiscreteActionKeySingles { GOALIE_LEFT, DEFENDERS_LEFT, MIDFIELDERS_LEFT, OFFENSIVE_LEFT, GOALIE_RIGHT, DEFENDERS_RIGHT, MIDFIELDERS_RIGHT, OFFENSIVE_RIGHT };
public enum ContinuousActionKey { GOALIE_TORQUE, GOALIE_FORCE, DEFENDERS_TORQUE, DEFENDERS_FORCE, MIDFIELDERS_TORQUE, MIDFIELDERS_FORCE, OFFENSIVE_TORQUE, OFFENSIVE_FORCE };
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


    // Start is called before the first frame update
    void Start()
    {
        
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
}
