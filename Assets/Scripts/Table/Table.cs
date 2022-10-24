using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Table : MonoBehaviour
{
    [Header("Prefabs")]
    public Rod rodPrefab;

    [Header("Table Parameters")]
    public TableScriptableObject tso;

    // Start is called before the first frame update
    void Start()
    {
        Reset();
        RegenerateTable();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Reset()
    {
        // Set tso errorfloats
        List<ErrorFloat> errorFloats = new List<ErrorFloat>{ tso.tableLength, tso.tableWidth, tso.tableDepth,
        tso.kBumper, tso.ballMass, tso.ballDiameter, tso.rodDiameter, tso.rodMass, tso.goalieBumperSpacing, tso.defendersSpacing,
        tso.midfieldersSpacing, tso.offensiveSpacing, tso.goalieLengthPercent, tso.defendersLengthPercent, tso.midfieldersLengthPercent,
        tso.offensiveLengthPercent, tso.foosmanMass, tso.foosmanDepth, tso.foosmanWidth, tso.foosmanBodyHeight,
        tso.foosmanFootHeight, tso.foosmanFootWidth, tso.foosmanShoulderHeight, tso.foosmanHeadHeight,
        tso.foosmanHeadHeight, tso.foosmanHeadDepth};
        foreach (ErrorFloat ef in errorFloats)
        {
            ef.SetValue();
            Debug.Log(ef.average);
            Debug.Log(ef.GetValue());
            Debug.Log("-");
        }
    }

    private void RegenerateTable()
    {
        // Create walls and floor

        // Optionally add ramps

        // Create rods
        Rod testRod = Instantiate(rodPrefab, transform.position, rodPrefab.transform.rotation);
        testRod.rodType = RodType.MIDFIELDERS;
        testRod.transform.localScale = new Vector3(tso.rodDiameter.GetValue(), 0.5f, tso.rodDiameter.GetValue());
        testRod.tso = tso;
        testRod.GenerateFoosmen();
    }
}
