using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Foosman : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField]
    private GameObject bodyPrefab;
    [SerializeField]
    private GameObject footPrefab;
    [SerializeField]
    private GameObject headPrefab;
    [SerializeField]
    private GameObject shouldersPrefab;
    [SerializeField]
    private GameObject torsoPrefab;

    // Private vars

    // References to components
    FoosballSettings m_foosballSettings;

    // Start is called before the first frame update
    void Start()
    {
        m_foosballSettings = FindObjectOfType<FoosballSettings>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Generate(TableScriptableObject tso){
        GameObject torso = Instantiate(torsoPrefab, transform.position, torsoPrefab.transform.rotation);
        torso.transform.parent = transform;
        torso.transform.localScale = new Vector3(tso.foosmanDepth.GetValue(), 0.5f*tso.foosmanWidth.GetValue(), tso.rodDiameter.GetValue());

        GameObject foot = Instantiate(footPrefab, transform.position + Vector3.down*(tso.rodDiameter/2 + tso.foosmanBodyHeight + tso.foosmanFootHeight/2), footPrefab.transform.rotation);
        foot.transform.parent = transform;
        foot.transform.localScale = new Vector3(tso.foosmanFootHeight.GetValue(), tso.foosmanFootHeight.GetValue(), tso.foosmanFootWidth.GetValue());

        GameObject head = Instantiate(headPrefab, transform.position + Vector3.up * (tso.rodDiameter / 2 + tso.foosmanShoulderHeight + tso.foosmanHeadHeight/2), headPrefab.transform.rotation);
        head.transform.parent = transform;
        head.transform.localScale = new Vector3(tso.foosmanHeadDepth.GetValue(), tso.foosmanHeadHeight.GetValue(), tso.foosmanHeadWidth.GetValue());
        Debug.Log(new Vector3(tso.foosmanHeadDepth.GetValue(), tso.foosmanHeadHeight.GetValue(), tso.foosmanHeadWidth.GetValue()));

        GameObject shoulders = Instantiate(shouldersPrefab, transform.position + Vector3.up * (tso.rodDiameter / 2 + tso.foosmanShoulderHeight/2), shouldersPrefab.transform.rotation);
        shoulders.transform.parent = transform;
        shoulders.transform.localScale = new Vector3(tso.foosmanDepth.GetValue(), tso.foosmanShoulderHeight.GetValue(), tso.foosmanWidth.GetValue());

        GameObject body = Instantiate(bodyPrefab, transform.position + Vector3.down * (tso.rodDiameter / 2 + tso.foosmanBodyHeight / 2), bodyPrefab.transform.rotation);
        body.transform.parent = transform;
        body.transform.localScale = new Vector3(tso.foosmanFootHeight.GetValue(), tso.foosmanBodyHeight.GetValue(), tso.foosmanFootWidth.GetValue()*3f / 4f);
    }
}
