using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the rod, bumper, and attached foosman
/// </summary>
public class Rod : MonoBehaviour
{
    [Header("Prefabs")]
    public Foosman foosmanPrefab;
    public GameObject bumperPrefab;

    [Header("Data")]
    public RodType rodType;
    public Team team;
    public Rigidbody rigidbody;
    [SerializeField]
    private float position;
    [HideInInspector]
    public TableScriptableObject tso;

    private RodAction actionToDo;
    private FoosballAgent controllingAgent;

    // Start is called before the first frame update
    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    public void Setup(FoosballAgent agent){
        controllingAgent = agent;
        float spacing = 0f;
        float foosmanPos = 0f;
        int foosmanNumber = 0;

        // Based on type, generate foosmen
        if (rodType == RodType.GOALIE)
        {
            spacing = tso.goalieBumperSpacing.GetValue();
            foosmanPos = spacing;
            foosmanNumber = 3;
        }
        else if (rodType == RodType.DEFENDERS)
        {
            spacing = tso.defendersSpacing.GetValue();
            foosmanPos = spacing / 2;
            foosmanNumber = 2;
        }
        else if (rodType == RodType.MIDFIELDERS)
        {
            spacing = tso.midfieldersSpacing.GetValue();
            foosmanPos = spacing * 2;
            foosmanNumber = 5;
        }
        else if (rodType == RodType.OFFENSIVE)
        {
            spacing = tso.offensiveSpacing.GetValue();
            foosmanPos = spacing;
            foosmanNumber = 3;
        }

        
        for (int i = 0; i < foosmanNumber; i++)
        {
            if (rodType == RodType.GOALIE && foosmanNumber != 1){
                // Generate a bumper block
                GameObject bumper = Instantiate(bumperPrefab, transform.position + Vector3.forward * foosmanPos, bumperPrefab.transform.rotation);
                bumper.transform.parent = transform;
                bumper.transform.localScale = new Vector3(tso.foosmanDepth.GetValue(), tso.rodDiameter.GetValue(), tso.foosmanWidth.GetValue());
                foosmanPos -= spacing;
            } else {
                Foosman foosman = Instantiate(foosmanPrefab, transform.position + Vector3.forward * foosmanPos, foosmanPrefab.transform.rotation);
                foosman.transform.parent = transform;
                foosman.team = team;
                foosman.Generate(tso);
                foosmanPos -= spacing;
            }
        }
    }

    public FoosmenState GetState(){
        
        FoosmenState s = new FoosmenState();
        return s;
    }

    public void SetState(RodAction action){
        actionToDo = action;
    }

    // Done on post-action!!! Also has improved euler spring stuff.
    public void PostAction(){
        // Apply the action, then compute bumper if needed

        HandState hand = null;
        if (controllingAgent.leftHandState.rodType == rodType){
            hand = controllingAgent.leftHandState;
        } else if (controllingAgent.rightHandState.rodType == rodType){
            hand = controllingAgent.rightHandState;
        }

        if (hand != null){
            // Held by a hand, trigger calculation
            rigidbody.AddTorque(15 * transform.up * actionToDo.torque, ForceMode.Force);
        }
        //Debug.Log(string.Format("{0} {1}", team, actionToDo.torque));
    }
}
