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
        RegenerateTable();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Reset()
    {
        
    }

    private void RegenerateTable()
    {
        // Create walls and floor

        // Optionally add ramps

        // Create rods
        Rod testRod = Instantiate(rodPrefab, transform.position, Quaternion.identity);
        testRod.rodType = RodType.MIDFIELDERS;
        testRod.tso = tso;
        testRod.GenerateFoosmen();
    }
}
