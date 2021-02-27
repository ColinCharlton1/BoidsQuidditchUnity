using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnvironmentData", menuName = "ScriptableObjects/EnvironmentData", order = 1)]
public class EnvironmentScriptable : ScriptableObject
{
    public float maxHeight;
    public float xMax;
    public float xMin;
    public float zMax;
    public float zMin;
}

