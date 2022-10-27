using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arm : MonoBehaviour
{
    // Start is called before the first frame update
    GameObject handPosObject;
    ArmHandedness handedness;

    // Private vars

    // References to components
    FoosballSettings m_foosballSettings;
    FoosballEnvController m_foosballEnvController;
    Table m_table;

    void Start()
    {
        m_foosballSettings = FindObjectOfType<FoosballSettings>();
    }

    public void Setup(Table table, FoosballEnvController foosballEnvController)
    {
        m_table = table;
        m_foosballEnvController = foosballEnvController;
    }

    public void UpdateHand(float handPosition, RodType handRodType){
        Vector3 oldPos = handPosObject.transform.position;
        handPosObject.transform.position = new Vector3(m_table.transform.position.x - m_table.tso.tableLength/2f + handPosition, oldPos.y, oldPos.z);
    }
}
