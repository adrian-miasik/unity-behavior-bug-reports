using Community;
using UnityEngine;

/// <summary>
/// A scriptable object acting as a save file.
/// </summary>
[CreateAssetMenu(fileName = "New Persistent BehaviorData", 
    menuName = "Adrian Miasik/Create New 'Persistent BehaviorData'", order = 1)]
public class PersistentBehaviorData : ScriptableObject
{
    [Button("Wipe All Data")]
    public void ClearAll()
    {
        ClearBehaviorData();
        ClearAgentPositionData();
    }
    
    [Button("Wipe Behavior Data")]
    public void ClearBehaviorData()
    {
        m_behaviorData.Clear();
    }

    [Button("Wipe Agent Position Data")]
    public void ClearAgentPositionData()
    {
        m_agentPositions.Clear();
    }
    
    // First string represents object name, second string represents behavior graph JSON.
    public GenericDictionary<string, string> m_behaviorData = new();
    
    // First string represents object name, vector3 represents world position.
    public GenericDictionary<string, Vector3> m_agentPositions = new();
}