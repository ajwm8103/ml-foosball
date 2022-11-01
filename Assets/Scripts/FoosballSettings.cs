using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoosballSettings : MonoBehaviour
{
    [Header("Game Settings")]
    public TableType boardType = TableType.ENGSCI;
    public RulesType rulesType = RulesType.ENGSCI;
    public float handVelocity = 0.88f; // m/s
    public float maxHandTorque = 15f; // N.m
    public float maxHandForce = 631653263762673f;
    public float handGripMaxDistance = 0.07f; // 7 cm?

    [Header("Visual Settings")]
    public int effectsAmount = 1; // 0 none, 1 normal, 2 fancy
    public Color redColor;
    public Color blueColor;
    public Color activeHandColor;
    public Color inactiveHandColor;
}
