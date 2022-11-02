using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arm : MonoBehaviour
{
    // Start is called before the first frame update
    public ArmHandedness handedness;

    // Private vars

    // References to components
    FoosballSettings m_foosballSettings;
    FoosballEnvController m_foosballEnvController;
    GameObject handPosObject;
    Transform handAngleObject;
    Table m_table;
    HandState handState;

    void Awake()
    {
        m_foosballSettings = FindObjectOfType<FoosballSettings>();
    }

    public void Setup(Table table, FoosballEnvController foosballEnvController, GameObject handPosObject, HandState handState)
    {
        m_table = table;
        m_foosballEnvController = foosballEnvController;
        this.handPosObject = handPosObject;
        handAngleObject = handPosObject.transform.Find("HandAngle");
        this.handState = handState;
    }

    public void UpdateHand(float handPosition, RodType handRodType){
        Vector3 oldPos = handPosObject.transform.position;
        handPosObject.transform.position = new Vector3(m_table.transform.position.x - m_table.tso.tableLength/2f + handPosition, oldPos.y, oldPos.z);
        handPosObject.GetComponent<Renderer>().material.color = handRodType == RodType.NONE ? m_foosballSettings.inactiveHandColor : m_foosballSettings.activeHandColor;
        Debug.Log(handState.angle);
        handAngleObject.rotation = Quaternion.Euler(0f, 0f, Mathf.Rad2Deg * handState.angle);
    }
}
