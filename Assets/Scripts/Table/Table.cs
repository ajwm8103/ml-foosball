using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Table : MonoBehaviour
{
    [Header("Prefabs")]
    public Rod rodPrefab;
    public GameObject tableFloorPrefab;
    public GameObject tableWallPrefab;
    public Ball ballPrefab;

    [Header("Table Parameters")]
    public TableScriptableObject tso;

    // Private component references
    Ball myBall;

    Rod[] rods;

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

        RegenerateTable();
    }

    private void RegenerateTable()
    {
        // Create walls and floor
        float tableThickness = 0.10f;
        float floorYPos = tso.rodDiameter / 2f + tso.foosmanBodyHeight + tso.foosmanFootHeight + 0.007f + tableThickness / 2F;
        GameObject floor = Instantiate(tableFloorPrefab, transform.position + 
        Vector3.down*floorYPos, Quaternion.identity);
        floor.transform.parent = transform;
        floor.transform.localScale = new Vector3(tso.tableLength.GetValue(), tableThickness, tso.tableWidth.GetValue());

        float wallYPos = -floorYPos + tso.tableDepth / 2f;
        GameObject wallOne = Instantiate(tableWallPrefab, transform.position + Vector3.left * (tso.tableWidth + tableThickness / 2f)
        + Vector3.up*wallYPos, Quaternion.identity);
        wallOne.transform.parent = transform;
        wallOne.transform.localScale = new Vector3(tableThickness, tso.tableDepth + tableThickness, tso.tableLength.GetValue());
        // Optionally add ramps

        // Create rods
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
            rod.tso = tso;
            rod.GenerateFoosmen();
        }

        // Blue
        foreach (RodType rodType in rodTypes)
        {
            Rod rod = Instantiate(rodPrefab, transform.position + Vector3.right * (tso.tableLength * (0.5f - rodToLengthPercent[rodType].GetValue())), rodPrefab.transform.rotation);
            rods[(int)rodType+4] = rod;
            rod.rodType = rodType;
            rod.team = Team.BLUE;
            rod.transform.localScale = new Vector3(tso.rodDiameter.GetValue(), 0.5f, tso.rodDiameter.GetValue());
            rod.tso = tso;
            rod.GenerateFoosmen();
        }
    }

    private void StartPoint()
    {
        // Spawn ball
        myBall = Instantiate(ballPrefab, transform.position, Quaternion.identity);
        myBall.transform.localScale = Vector3.one * tso.ballDiameter.GetValue();
        myBall.myRigidbody.mass = tso.ballMass.GetValue();
        myBall.myRigidbody.AddForce(Vector3.right * Random.Range(-1f, 1f), ForceMode.Impulse);
    }
}
