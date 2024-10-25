using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
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

        [SerializeField] private GameObject m_agentPrefab;
        [SerializeField] private int m_count;
        [SerializeField] SaveData m_SaveData;

        private List<GameObject> m_agents = new();
        private GameObjectResolver m_GameObjectResolver = new();
        private RuntimeSerializationUtility.JsonBehaviorSerializer m_JsonSerializer = new();

        private bool ignoreStart;
        
        //private Dictionary<GameObject, Vector3> m_agentPositions = new();
        //private Dictionary<GameObject, string> m_serializedAgents = new();

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            Random.InitState(0);
            for (int idx = 0; idx < m_count; ++idx)
            {
                GameObject agent = Instantiate(m_agentPrefab, transform);
                agent.name = $"Agent_{idx}";
                m_agents.Add(agent);
            }
        }

        private void OnGUI()
        {
            GUI.Box(new Rect(5, 5, 150, 90), "Menu");
            if (GUI.Button(new Rect(10, 30, 130, 20), "Save"))
            {
                SerializeAgents();
            }
            if (GUI.Button(new Rect(10, 60, 130, 20), "Load"))
            {
                DeserializeAgents();
            }
        }

        private void SerializeAgents()
        {
            m_SaveData.AgentPositions.Clear();
            m_SaveData.AgentData.Clear();
            
            foreach (var agent in m_agents)
            {
                string data = agent.GetComponent<BehaviorGraphAgent>().Serialize(m_JsonSerializer, m_GameObjectResolver);
                m_SaveData.AgentData.Add(data);
                //_serializedAgents.Add(agent, data);
                m_SaveData.AgentPositions.Add(agent.transform.position);
            }
        }

        private void DeserializeAgents()
        {
            for (var index = 0; index < m_agents.Count; index++)
            {
                var agent = m_agents[index];
                var agentData = m_SaveData.AgentData[index];
                agent.GetComponent<BehaviorGraphAgent>().Deserialize(agentData, m_JsonSerializer, m_GameObjectResolver);
                agent.transform.position = m_SaveData.AgentPositions[index];
            }
        }
    }
}