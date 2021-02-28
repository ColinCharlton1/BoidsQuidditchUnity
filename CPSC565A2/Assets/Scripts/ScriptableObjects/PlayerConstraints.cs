using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerConstraints", menuName = "ScriptableObjects/PlayerConstraints", order = 1)]
public class PlayerConstraints : ScriptableObject
{
    public float playerAvoidDistance = 5.0f;
    public float playerAvoidStrength = 5.0f;
    public float barrierAvoidDistance = 5.0f;
    public float barrierAvoidStrength = 5.0f;
    public float environmentConstraintDistance = 5.0f;
    public float environmentConstraintStrength = 5.0f;
}
