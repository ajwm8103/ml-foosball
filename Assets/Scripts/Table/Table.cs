using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class Table : MonoBehaviour
{
    [Header("Prefabs")]
    public Rod rodPrefab;
    public GameObject tableFloorPrefab;
    public GameObject tableWallPrefab;
    public Ball ballPrefab;
    public GameObject handPosPrefab;
    public Arm armPrefab;

    [Header("Table Parameters")]
    public TableScriptableObject tso;

    // Public component references
    [HideInInspector]
    public Dictionary<Team, Dictionary<ArmHandedness, Arm>> arms;

    // Private component references
    Ball myBall;
    FoosballEnvController m_foosballEnvController;

    // Useful info
    public Rod[] rods;
    public Dictionary<RodType, float> rodPercent;

    // Start is called before the first frame update
    void Start()
    {
        Reset();
        StartPoint();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup(FoosballEnvController foosballEnvController){
        m_foosballEnvController = foosballEnvController;
    }

    public void Reset()
    {
        // Set tso errorfloats
        List<ErrorFloat> errorFloats = new List<ErrorFloat>{ tso.tableLength, tso.tableWidth, tso.tableDepth,
        tso.kBumper, tso.ballMass, tso.ballDiameter, tso.rodDiameter, tso.rodMass, tso.goalieBumperSpacing, tso.defendersSpacing,
        tso.midfieldersSpacing, tso.offensiveSpacing, tso.goalieLengthPercent, tso.defendersLengthPercent, tso.midfieldersLengthPercent,
        tso.offensiveLengthPercent, tso.foosmanMass, tso.foosmanDepth, tso.foosmanWidth, tso.foosmanBodyHeight,
        tso.foosmanFootHeight, tso.foosmanFootWidth, tso.foosmanShoulderHeight, tso.foosmanHeadHeight,
        tso.foosmanHeadWidth, tso.foosmanHeadDepth};
        foreach (ErrorFloat ef in errorFloats)
        {
            ef.SetValue();
            //Debug.Log(ef.average);
            //Debug.Log(ef.GetValue());
            //Debug.Log("-");
        }

        // Calculate useful info
        rodPercent = new Dictionary<RodType, float> {
            {RodType.GOALIE, tso.goalieLengthPercent.GetValue() },
            {RodType.DEFENDERS, tso.defendersLengthPercent.GetValue() },
            {RodType.MIDFIELDERS, tso.midfieldersLengthPercent.GetValue() },
            {RodType.OFFENSIVE, tso.offensiveLengthPercent.GetValue() }
        };

        RegenerateTable();
    }

    private void RegenerateTable()
    {
        int redAgentInd = m_foosballEnvController.agents[0].agent.team == Team.RED ? 0 : 1;
        FoosballAgent redAgent = m_foosballEnvController.agents[redAgentInd].agent;
        FoosballAgent blueAgent = m_foosballEnvController.agents[1-redAgentInd].agent;

        // Create walls and floor
        float tableThickness = 0.10f;
        float floorYPos = tso.rodDiameter / 2f + tso.foosmanBodyHeight + tso.foosmanFootHeight + 0.007f + tableThickness / 2F;
        GameObject floor = Instantiate(tableFloorPrefab, transform.position + 
        Vector3.down*floorYPos, Quaternion.identity);
        floor.transform.parent = transform;
        floor.transform.localScale = new Vector3(tso.tableLength.GetValue(), tableThickness, tso.tableWidth.GetValue());

        float wallYPos = -floorYPos + tso.tableDepth / 2f;
        GameObject wallOne = Instantiate(tableWallPrefab, transform.position + Vector3.back * ((tso.tableWidth + tableThickness) / 2f)
        + Vector3.up*wallYPos, Quaternion.identity);
        wallOne.transform.parent = transform;
        wallOne.transform.localScale = new Vector3(tso.tableLength.GetValue(), tso.tableDepth + tableThickness, tableThickness);

        GameObject wallTwo = Instantiate(tableWallPrefab, transform.position + Vector3.forward * ((tso.tableWidth + tableThickness) / 2f)
        + Vector3.up * wallYPos, Quaternion.identity);
        wallTwo.transform.parent = transform;
        wallTwo.transform.localScale = new Vector3(tso.tableLength.GetValue(), tso.tableDepth + tableThickness, tableThickness);

        // Optionally add ramps

        // Create rods
        Transform rodHolder = transform.Find("Rod Holder");
        if (rodHolder != null)
        {
            foreach (Transform child in rodHolder)
            {
                Destroy(child.gameObject);
            }
        }
        else
        {
            rodHolder = new GameObject().transform;
            rodHolder.parent = transform;
            rodHolder.name = "Rod Holder";
        }

        rods = new Rod[8];
        Dictionary<RodType, ErrorFloat> rodToLengthPercent = new Dictionary<RodType, ErrorFloat>{
            {RodType.GOALIE, tso.goalieLengthPercent },
            {RodType.DEFENDERS, tso.defendersLengthPercent },
            {RodType.MIDFIELDERS, tso.midfieldersLengthPercent },
            {RodType.OFFENSIVE, tso.offensiveLengthPercent },
        };

        // Red
        List<RodType> rodTypes = new List<RodType> { RodType.GOALIE, RodType.DEFENDERS, RodType.MIDFIELDERS, RodType.OFFENSIVE };

        foreach (RodType rodType in rodTypes)
        {
            Rod rod = Instantiate(rodPrefab, transform.position + Vector3.right*(tso.tableLength*(rodToLengthPercent[rodType].GetValue() - 0.5f)), rodPrefab.transform.rotation);
            rods[(int)rodType] = rod;
            rod.rodType = rodType;
            rod.team = Team.RED;
            rod.transform.localScale = new Vector3(tso.rodDiameter.GetValue(), 0.5f, tso.rodDiameter.GetValue());
            rod.name = string.Format("Red {0} Rod", Enum.GetName(typeof(RodType), rodType));
            rod.transform.parent = rodHolder;
            rod.tso = tso;
            rod.Setup(redAgent);
        }

        // Blue
        foreach (RodType rodType in rodTypes)
        {
            Rod rod = Instantiate(rodPrefab, transform.position + Vector3.right * (tso.tableLength * (0.5f - rodToLengthPercent[rodType].GetValue())), rodPrefab.transform.rotation);
            rods[(int)rodType+4] = rod;
            rod.rodType = rodType;
            rod.team = Team.BLUE;
            rod.transform.localScale = new Vector3(tso.rodDiameter.GetValue(), 0.5f, tso.rodDiameter.GetValue());
            rod.name = string.Format("Blue {0} Rod", Enum.GetName(typeof(RodType), rodType));
            rod.transform.parent = rodHolder;
            rod.tso = tso;
            rod.Setup(blueAgent);
        }

        // Create arms
        Transform armHolder = transform.Find("Arm Holder");
        if (armHolder != null){
            foreach (Transform child in armHolder)
            {
                Destroy(child.gameObject);
            }
        } else {
            armHolder = new GameObject().transform;
            armHolder.parent = transform;
            armHolder.name = "Arm Holder";
        }

        arms = new Dictionary<Team, Dictionary<ArmHandedness, Arm>>();
        for (Team armTeam = 0; (int)armTeam < 2; armTeam++)
        {
            FoosballAgent agent = armTeam == Team.RED ? redAgent : blueAgent;
            arms[armTeam] = new Dictionary<ArmHandedness, Arm>();
            for (ArmHandedness armHandedness = 0; (int)armHandedness < 2; armHandedness++)
            {
                Vector3 armPos = transform.position
                + Vector3.forward * (tso.tableWidth * (armTeam == Team.RED ? -1 : 1))
                + Vector3.right * (tso.tableLength / 4f * (armHandedness == ArmHandedness.LEFT ? -1 : 1));

                GameObject handPosObject = Instantiate(handPosPrefab, armPos, handPosPrefab.transform.rotation);
                Arm arm = Instantiate(armPrefab, armPos, armPrefab.transform.rotation);

                arm.Setup(this, m_foosballEnvController, handPosObject, armHandedness == ArmHandedness.LEFT ? agent.leftHandState : agent.rightHandState);
                arm.name = string.Format("{0} {1} Arm", Enum.GetName(typeof(Team), armTeam), Enum.GetName(typeof(ArmHandedness), armHandedness));
                handPosObject.transform.parent = arm.transform;
                arm.handedness = armHandedness;
                arm.transform.parent = armHolder;
                arms[armTeam][armHandedness] = arm;
            }
        }

    }

    private void StartPoint()
    {
        // Spawn ball
        myBall = Instantiate(ballPrefab, transform.position, Quaternion.identity);
        myBall.transform.localScale = Vector3.one * tso.ballDiameter.GetValue();
        myBall.myRigidbody.mass = tso.ballMass.GetValue();
        myBall.myRigidbody.velocity = Vector3.right * UnityEngine.Random.Range(-0.2f, 0.2f);
    }


    public void DisplayHand(Team handTeam, FoosballAgent agent){
        /*
        public RodType leftHandRodType = RodType.NONE;
        public RodType rightHandRodType = RodType.NONE;
        public float leftHandPosition = 0f; // literal position
        public float rightHandPosition = 0f; // literal position
        */

    }
    public void MoveTeam(Dictionary<RodType, RodAction> rodActions, Team actionTeam)
    {
        int offset = 4 * ((int)actionTeam);
        for (int i = 0; i < 4; i++)
        {
            RodAction test = rodActions[(RodType)i];
            //Debug.Log(String.Format("{0} {1} {2}", test.torque, test.force, test.gripped));
            rods[offset + i].SetState(rodActions[(RodType)i]);
        }
    }

    public void PostAction(){
        for (int i = 0; i < 8; i++)
        {
            rods[i].PostAction();
        }
    }
}
