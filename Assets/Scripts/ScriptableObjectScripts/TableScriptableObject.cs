using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ErrorFloat
{
    public float average;
    public float std;
    public ErrorFloat(float average, float std)
    {
        this.average = average;
        this.std = std;
    }
}

[CreateAssetMenu(fileName = "Legend Configuration", menuName = "ScriptableObject/Legend Configuration", order = 1)]
public class TableScriptableObject : ScriptableObject
{
    public ErrorFloat tableLength = new ErrorFloat(1.42f, 0.01f);
    public ErrorFloat tableWidth = new ErrorFloat(0.77f, 0.005f);
    public ErrorFloat tableDepth = new ErrorFloat(0.10f, 0.005f);
    public ErrorFloat rodDiameter = new ErrorFloat(0.022f, 0.0005f);
    public ErrorFloat foosmanDepth = new ErrorFloat(0.044f, 0.0002f);
    public ErrorFloat foosmanWidth = new ErrorFloat(0.038f, 0.0002f);
    public ErrorFloat foosmanBodyHeight = new ErrorFloat(0.038f, 0.0002f);
}
