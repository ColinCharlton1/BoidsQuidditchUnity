using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamManagement : MonoBehaviour
{
    public int teamSize = 5;
    public EnvironmentScriptable envConstraints;
    public GameObject Snitch_Prefab;
    public GameObject G_Prefab;
    public GameObject S_Prefab;

    private void Awake()
    {
        Instantiate(Snitch_Prefab, new Vector3((envConstraints.xMax + envConstraints.xMin) / 2,
                                                envConstraints.maxHeight / 2,
                                                (envConstraints.zMax - envConstraints.zMin) / 2),
                                                Quaternion.identity);

        float zIncrement = (envConstraints.zMax - envConstraints.zMin) / (teamSize + 1);
        float currentz = envConstraints.zMin + zIncrement;
        for (int i = 0; i < teamSize; i++)
        {
            Instantiate(G_Prefab, new Vector3(envConstraints.xMax, envConstraints.maxHeight / 2, currentz), Quaternion.identity);
            Instantiate(S_Prefab, new Vector3(envConstraints.xMin, envConstraints.maxHeight / 2, currentz), Quaternion.identity);
            currentz += zIncrement;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
