using System.Collections.Generic;
using System.Linq;
using Community;
using Newtonsoft.Json.Linq;
using Unity.Properties;
using UnityEditor;
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
        private List<GameObject> m_queueSlots = new();
        private GameObjectResolver m_GameObjectResolver = new();
        private RuntimeSerializationUtility.JsonBehaviorSerializer m_JsonSerializer = new();

        // Data Cache
        [SerializeField] private GenericDictionary<GameObject, Vector3> m_agentPositions = new();
        private bool m_savedThisSession;

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
                m_queueSlots.Add(queueSlot.gameObject);
                
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
            m_agentPositions.Clear();

            foreach (var agent in m_agents)
            {
                string data = agent.GetComponent<BehaviorGraphAgent>().Serialize(m_JsonSerializer, m_GameObjectResolver);
                m_saveFile.m_behaviorData.Add(agent.name, data);
                m_agentPositions.Add(agent, agent.transform.position);
            }

            m_savedThisSession = true;
            
            PrintJSONGlobalObjectIDs();
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

            if (!m_savedThisSession)
            {
                Debug.Log("Printing save data GlobalObjectID's...");
                PrintJSONGlobalObjectIDs();

                Debug.Log("Printing current QueueSlot GlobalObjectIDs...");
                List<string> currentIDs = PrintQueueSlotGlobalObjectIDs();

                Debug.Log("Now replacing save data GlobalObjectID's with current GlobalObjectIDs...");
                ReplaceJSONGlobalObjectIDs(currentIDs);
            }
            
            foreach (var agent in m_agents)
            {
                if (m_saveFile.m_behaviorData.TryGetValue(agent.name, out var data))
                {
                    agent.GetComponent<BehaviorGraphAgent>().Deserialize(data, m_JsonSerializer, m_GameObjectResolver);
                    agent.transform.position = m_agentPositions[agent];
                }
            }
        }

        [Button]
        private void PrintJSONGlobalObjectIDs()
        {
            // Allocate
            List<string> globalObjectIDs = new();
            
            // Iterate through each saved agents behavior graph JSON data...
            foreach (string behaviorJSON in m_saveFile.m_behaviorData.Values)
            {
                ExtractGlobalObjectIDsFromJObject(JObject.Parse(behaviorJSON), globalObjectIDs);
            }

            // Iterate through each found JSON GlobalObjectID...
            foreach (string globalObjectID in globalObjectIDs)
            {
                // If found JSON GlobalObjectID can't be converted to GlobalObjectId struct...
                if (!GlobalObjectId.TryParse(globalObjectID, out GlobalObjectId id))
                {
                    Debug.LogWarning("The following string could not be converted to a GlobalObjectId struct: " + 
                                     globalObjectID);
                }
                // Otherwise, GlobalObjectId struct is valid...
                else
                {
                    Object obj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id);

                    if (obj != null)
                    {
                        // Print
                        Debug.Log("'" + id + "' is " + obj.name, obj);
                    }
                    else
                    {
                        Debug.Log(id);
                    }
                }
            }
        }

        private void ExtractGlobalObjectIDsFromJObject(JToken token, List<string> result)
        {
            // Verify
            if (token == null)
            {
                Debug.LogWarning("Provided JToken is null. Unable to extract properties.");
                return;
            }

            if (token is JObject jObj)
            {
                foreach (KeyValuePair<string, JToken> kvp in jObj)
                {
                    if (kvp.Key == "m_Value" && kvp.Value.Type == JTokenType.String)
                    {
                        string value = kvp.Value.ToString();

                        if (value.StartsWith("GlobalObjectId"))
                        {
                            result.Add(value);
                        }
                    }
                
                    // Recursively iterate through the rest of the objects values
                    ExtractGlobalObjectIDsFromJObject(kvp.Value, result);
                }
            }
            else if (token is JArray jArray)
            {
                foreach (JToken item in jArray)
                {
                    // Recursively iterate through the rest of the array value
                    ExtractGlobalObjectIDsFromJObject(item, result);
                }
            }
        }

        private void ReplaceJSONGlobalObjectIDs(List<string> currentIDs)
        {
            // Iterate through each saved agents behavior graph JSON data...
            for (int i = 0; i < m_saveFile.m_behaviorData.Values.Count; i++)
            {
                // Fetch JSON for agent...
                string behaviorJSON = m_saveFile.m_behaviorData.Values.ElementAt(i);
                
                // Replace first occuring GlobalObjectID with provided GlobalObjectID.
                string replacedBehaviorJSON = ReplaceGlobalObjectIDInJObject(JObject.Parse(behaviorJSON), 
                    currentIDs[i]);
                
                // Save updated JSON back into save file...
                m_saveFile.m_behaviorData[m_agents[i].name] = replacedBehaviorJSON;
            }
        }

        private string ReplaceGlobalObjectIDInJObject(JToken token, string replacementID)
        {
            // Verify
            if (token == null)
            {
                Debug.LogWarning("Provided JToken is null. Unable to extract properties.");
                return "{}"; // Empty JSON
            }
            
            if (token is JObject jObj)
            {
                foreach (KeyValuePair<string, JToken> kvp in jObj)
                {
                    if (kvp.Key == "m_Value" && kvp.Value.Type == JTokenType.String)
                    {
                        string value = kvp.Value.ToString();

                        if (value.StartsWith("GlobalObjectId"))
                        {
                            Debug.Log("Attempting to replace '" + kvp.Value + "' with new id of: '" + 
                                      replacementID + "'.");
                            
                            jObj[kvp.Key] = new JValue(replacementID);

                            Debug.Log("Value has been replaced and is now: " + jObj[kvp.Key]);

                            break;
                        }
                    }
                    
                    // Recursively iterate through the rest of the objects values
                    ReplaceGlobalObjectIDInJObject(kvp.Value, replacementID);
                }
            }
            else if (token is JArray jArray)
            {
                foreach (JToken item in jArray)
                {
                    // Recursively iterate through the rest of the array value
                    ReplaceGlobalObjectIDInJObject(item, replacementID);
                }
            }

            return token.ToString();
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

        private List<string> PrintGlobalObjectIDs(List<Object> objects)
        {
            if (objects.Count <= 0)
            {
                Debug.LogWarning("Unable to print GlobalObjectID's. No provided objects found.");
                return new();
            }

            return SerializationHelper.PrintGlobalObjectIDs(objects);
        }

        [Button("Print QueueSlot GlobalObjectID's")]
        private List<string> PrintQueueSlotGlobalObjectIDs()
        {
            return PrintGlobalObjectIDs(new List<Object>(m_queueSlots));
        }
    }
}