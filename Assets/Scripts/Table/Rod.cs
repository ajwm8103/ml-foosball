using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the rod, bumper, and attached foosman
/// </summary>
public class Rod : MonoBehaviour
{
    [Header("Prefabs")]
    Foosman foosmanPrefab;

    public RodType rodType;
    public TableScriptableObject tso;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void GenerateFoosmen(){

        // Based on type, generate foosmen
        if (rodType == RodType.GOALIE)
        {

        }
        else if (rodType == RodType.DEFENDERS)
        {

        }
        else if (rodType == RodType.MIDFIELDERS)
        {
            float spacing = tso.midfieldersSpacing.GetValue();
            float foosmanPos = spacing*2;
            for (int i = 0; i < 5; i++)
            {
                Foosman foosman = Instantiate(foosmanPrefab);
                foosmanPos -= spacing;
            }
        }
        else if (rodType == RodType.OFFENSIVE)
        {

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
