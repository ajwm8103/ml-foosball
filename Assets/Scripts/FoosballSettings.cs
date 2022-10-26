using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoosballSettings : MonoBehaviour
{
    [Header("Game Settings")]
    public TableType boardType = TableType.ENGSCI;
    public RulesType rulesType = RulesType.ENGSCI;
    public float handVelocity = 0.88f; // m/s

    [Header("Visual Settings")]
    public int effectsAmount = 1; // 0 none, 1 normal, 2 fancy
    public Color redColor;
    public Color blueColor;
}
