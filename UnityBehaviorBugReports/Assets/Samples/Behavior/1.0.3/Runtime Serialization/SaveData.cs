using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "SaveData", menuName = "Scriptable Objects/SaveData")]
public class SaveData : ScriptableObject
{
    [SerializeField] public List<string> AgentData = new List<string>();
    [SerializeField] public List<Vector3> AgentPositions = new List<Vector3>();
}
