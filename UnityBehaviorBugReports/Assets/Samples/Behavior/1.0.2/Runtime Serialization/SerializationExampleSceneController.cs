using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Unity.Behavior.SerializationExample
{
    public class SerializationExampleSceneController : MonoBehaviour
    {
        private class GameObjectResolver : RuntimeSerializationUtility.IUnityObjectResolver<string>
        {
            public string Map(UnityEngine.Object obj) => obj ? obj.name : null;

            public TSerializedType Resolve<TSerializedType>(string mappedValue) where TSerializedType : Object
            {
                // It would be recommended to have a more robust way to resolve objects by name or id using a registry.
                GameObject obj = GameObject.Find(mappedValue);
                if (!obj)
                {
                    // If we didn't find the object by name in the scene, it might be a prefab.
                    GameObject[] prefabs = Resources.FindObjectsOfTypeAll<GameObject>();
                    foreach (var prefab in prefabs)
                    {
                        if (prefab.name == mappedValue)
                        {
                            return prefab as TSerializedType;
                        }
                    }

                    return null;
                }
                if (typeof(TSerializedType) == typeof(GameObject))
                {
                    return obj as TSerializedType;
                }
                if (typeof(Component).IsAssignableFrom(typeof(TSerializedType)))
                {
                    return obj.GetComponent<TSerializedType>();
                }
                return null;
            }
        }

        // References
        [SerializeField] private GameObject m_agentPrefab;
        [SerializeField] private QueueSlot m_queueSlotPrefab;
        [SerializeField] private int m_count = 10;
        [SerializeField] private PersistentBehaviorData m_saveFile;

        // Serialization
        private List<GameObject> m_agents = new();
        private List<QueueSlot> m_queueSlots = new();
        private GameObjectResolver m_GameObjectResolver = new();
        private RuntimeSerializationUtility.JsonBehaviorSerializer m_JsonSerializer = new();
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            Random.InitState(0);
            for (int idx = 0; idx < m_count; ++idx)
            {
                // Create agents
                GameObject agent = Instantiate(m_agentPrefab, transform);
                agent.name = $"Agent_{idx}";
                m_agents.Add(agent);
                
                // Create queue slots
                QueueSlot queueSlot = Instantiate(m_queueSlotPrefab, transform);
                queueSlot.name = $"QueueSlot_{idx}";
                m_queueSlots.Add(queueSlot);
                
                // Assign queue slot to agent
                agent.GetComponent<BehaviorGraphAgent>().SetVariableValue("Queue Slot", queueSlot);
            }
        }

        private void OnGUI()
        {
            GUI.Box(new Rect(5, 5, 150, 90), "Menu");
            if (GUI.Button(new Rect(10, 30, 130, 20), "Save"))
            {
                Save();
            }
            if (GUI.Button(new Rect(10, 60, 130, 20), "Load"))
            {
                Load();
            }
        }
        
        private void Update()
        {
            // Quick and dirty game restart
            if (Input.GetKeyDown(KeyCode.F5))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        [Button]
        private void Save()
        {
            m_saveFile.m_behaviorData.Clear();
            m_saveFile.m_agentPositions.Clear();

            for (int i = 0; i < m_agents.Count; i++)
            {
                // Fetch 
                GameObject agent = m_agents[i];
                
                if (agent.TryGetComponent(out BehaviorGraphAgent bga))
                {
                    // Clear
                    bga.SetVariableValue<QueueSlot>("Queue Slot", null);

                    // Save
                    string data = bga.Serialize(m_JsonSerializer, m_GameObjectResolver);
                    m_saveFile.m_behaviorData.Add(agent.name, data);
                    m_saveFile.m_agentPositions.Add(agent.name, agent.transform.position);

                    // Re-populate
                    bga.SetVariableValue("Queue Slot", m_queueSlots[i]);
                }
            }
        }

        [Button]
        private void Load()
        {
            if (m_saveFile.m_behaviorData.Count <= 0)
            {
                Debug.LogWarning("No save data found. Make sure to create save data by pressing the 'Save' button " +
                                 "during runtime before trying to load.");
                return;
            }

            for (int index = 0; index < m_agents.Count; index++)
            {
                GameObject agent = m_agents[index];
                if (m_saveFile.m_behaviorData.TryGetValue(agent.name, out var data))
                {
                    if (agent.TryGetComponent(out BehaviorGraphAgent bga))
                    {
                        // Load
                        bga.Deserialize(data, m_JsonSerializer, m_GameObjectResolver);
                        
                        // Populate
                        agent.transform.position = m_saveFile.m_agentPositions[agent.name];
                        bga.SetVariableValue("Queue Slot", m_queueSlots[index]);
                    }
                }
            }
        }
        
        [Button("Clear QueueSlots On All Agents")]
        private void ClearQueueSlotReferencesOnBehaviorAgents()
        {
            foreach (GameObject agent in m_agents)
            {
                // Try fetch BehaviorGraphAgent
                if (agent.TryGetComponent(out BehaviorGraphAgent bga))
                {
                    bga.SetVariableValue<QueueSlot>("Queue Slot", null);
                }
            }
        }

        [Button("Print Agent GlobalObjectID's")]
        private void PrintAgentGlobalObjectIDs()
        {
            if (m_agents.Count <= 0)
            {
                Debug.LogWarning("No agents found. Unable to print agent GlobalObjectID's.");
                return;
            }
            
            SerializationHelper.PrintGlobalObjectIDs(new List<Object>(m_agents));
        }
    }
}