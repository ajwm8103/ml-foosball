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
    private GameObject ShouldersPrefab;
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
        GameObject torso = Instantiate(torsoPrefab, transform.position, Quaternion.identity);
        torso.transform.parent = transform;
    }
}
