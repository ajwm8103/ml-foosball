using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class FoosballAgent : Agent
{

    private float m_Existential;

    // Private vars

    // References to components
    FoosballEnvController envController;

    EnvironmentParameters m_ResetParams;

    public struct VisibleState
    {
        public float fospos;
        public VisibleState(float fospos
        )
        {
            this.fospos = fospos;
        }

        public void AddObservations(VectorSensor sensor)
        {
            // 31
            //Debug.Log(localPosition.x);
            //Debug.Log(localPosition.y);
            sensor.AddObservation(fospos);
        }
    }

    public override void Initialize()
    {
        base.Initialize();
        //m_brawlSettings = FindObjectOfType<BrawlSettings>();

        envController = transform.parent.GetComponentInParent<FoosballEnvController>();
        m_Existential = 1f / envController.maxSteps;

        // Set params ?

        m_ResetParams = Academy.Instance.EnvironmentParameters;

        ResetAgent();
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
        VisibleState myVisibleState = GetVisibleState();
        myVisibleState.AddObservations(sensor);

        // Opponent state

        // Global state
        sensor.AddObservation(envController.totalSteps / envController.maxSteps);
    }

    public VisibleState GetVisibleState()
    {
        VisibleState vs = new VisibleState();
        return vs;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
