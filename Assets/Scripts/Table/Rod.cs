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

    public RodType rodType;
    public Team team;
    [HideInInspector]
    public TableScriptableObject tso;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void GenerateFoosmen(){
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
                foosman.Generate(tso);
                foosmanPos -= spacing;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
