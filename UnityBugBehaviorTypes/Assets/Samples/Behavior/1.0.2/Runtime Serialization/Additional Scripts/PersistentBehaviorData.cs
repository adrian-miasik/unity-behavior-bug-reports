using Community;
using UnityEngine;

/// <summary>
/// A scriptable object acting as a save file.
/// </summary>
[CreateAssetMenu(fileName = "New Persistent BehaviorData", 
    menuName = "Adrian Miasik/Create New 'Persistent BehaviorData'", order = 1)]
public class PersistentBehaviorData : ScriptableObject
{
    [Button("Wipe Behavior Data")]
    public void ClearBehaviorData()
    {
        m_behaviorData.Clear();
    }
    
    // First string represents object name, second string represents behavior graph JSON.
    public GenericDictionary<string, string> m_behaviorData = new();
}