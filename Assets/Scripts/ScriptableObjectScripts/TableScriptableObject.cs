using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ErrorFloat
{
    public float average;
    public float std;
    public ErrorFloat(float average, float std)
    {
        this.average = average;
        this.std = std;
    }

    public float GetValue(){
        return average + Random.Range(-std, std);
    }
}

[CreateAssetMenu(fileName = "Table Configuration", menuName = "ScriptableObject/Table Configuration", order = 1)]
public class TableScriptableObject : ScriptableObject
{
    // Lengths in metres
    [Header("Table Properties")]
    public ErrorFloat tableLength = new ErrorFloat(1.42f, 0.01f);
    public ErrorFloat tableWidth = new ErrorFloat(0.77f, 0.005f);
    public ErrorFloat tableDepth = new ErrorFloat(0.10f, 0.005f);
    public bool slopes = false;
    public ErrorFloat kBumper = new ErrorFloat(0.10f, 0.005f); // N/m

    [Header("Ball Properties")]
    public ErrorFloat ballMass = new ErrorFloat(0.024f, 0.001f); // kg
    public ErrorFloat ballDiameter = new ErrorFloat(0.036f, 0.0005f);

    [Header("Rod and Position Properties")]
    public ErrorFloat rodDiameter = new ErrorFloat(0.022f, 0.0005f);
    public ErrorFloat rodMass = new ErrorFloat(1.506f, 0.020f);

    [Header("Foosman Properties")]
    public ErrorFloat foosmanMass = new ErrorFloat(0.030f, 0.001f); // kg
    public ErrorFloat foosmanDepth = new ErrorFloat(0.044f, 0.0002f);
    public ErrorFloat foosmanWidth = new ErrorFloat(0.038f, 0.0002f);
    public ErrorFloat foosmanBodyHeight = new ErrorFloat(0.044f, 0.0002f);
    public ErrorFloat foosmanFootHeight = new ErrorFloat(0.01f, 0.0001f);
    public ErrorFloat foosmanFootWidth = new ErrorFloat(0.023f, 0.0001f);
    public ErrorFloat foosmanShoulderHeight = new ErrorFloat(0.014f, 0.0001f);
    public ErrorFloat foosmanHeadHeight = new ErrorFloat(0.021f, 0.0001f);
    public ErrorFloat foosmanHeadWidth = new ErrorFloat(0.015f, 0.0001f);
    public ErrorFloat foosmanHeadDepth = new ErrorFloat(0.015f, 0.0001f);


}
