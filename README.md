
<h1 align="center">unity-behavior-bug-reports</h1>

## About

- This repository was created to highlight issues I ran into using the new [Unity Behaviors](https://docs.unity3d.com/Packages/com.unity.behavior@1.0/manual/index.html) package. Specifically relating to serialization/deserialization of the behavior agent graphs.
- This repo will be submitted to Unity's issue tracker. Once the tickets are submitted, I'll update this README.md document linking back to those tickets so resolution can be tracked.
- For more additional context see my [Unity discussion thread](https://discussions.unity.com/t/behavior-errors-deserializing-behaviorgraph-json-after-re-creating-related-gameobjects/1534184).

## Context
- **Editor Version:** `Unity 6 Preview` - `6000.0.20f1`
- **Package:** `com.unity.behavior` - `v1.0.2`

## Bugs
### Issue #1 - [IN-86386 - GlobalObjectID Mismatch](https://unity3d.atlassian.net/servicedesk/customer/portal/2/IN-86386) -> USER ERROR
- The first issue is related to loading/deserializing behavior graph data back into re-created GameObjects.
- Error:
```InvalidOperationException: An error occured while deserializing asset reference GUID=[7a89880255e2246de83870fb9c1e9803]. Asset is not yet loaded and will result in a null reference.```
- Seems to happen due to a mismatch of GlobalObjectID. See thread for additional information and my proposed solution/workaround.

### Reproduction Steps
0. Checkout commit https://github.com/adrian-miasik/unity-behavior-bug-reports/releases/tag/bug-1
1. Open `serialization-global-object-id-case.unity` scene
2. Enter Play Mode
3. Press `Save` button
4. Press `F5` hotkey to restart scene
5. Press `Load` button
6. Observe console for assertions

---

### Issue #2 - [IN-86387 - Type Construction Cast](https://unity3d.atlassian.net/servicedesk/customer/portal/2/IN-86387) -> RESOLVED IN PACKAGE `V1.0.3`
- The second issue is related to loading/deserializing behavior graph data back into re-created GameObject as well, but this time is unable to construct a type for each custom property found within the saved JSON behavior graph data. Though I have come across this bug with built-in types too as mentioned in the thread.
- Error: 
```
ArgumentException: Failed to construct type. Could not resolve type from TypeName=[Unity.Behavior.ComponentToComponentBlackboardVariable`2[[UnityEngine.Transform, UnityEngine.CoreModule, UnityEngine.Transform, UnityEngine.CoreModule]], Unity.Behavior].` Rethrow as AggregateException: One or more errors occurred.
```
- Might be an internal behavior package bug as `@Darren_Kelly` described: https://discussions.unity.com/t/behavior-errors-deserializing-behaviorgraph-json-after-re-creating-related-gameobjects/1534184/3
- Possibly resolved in upcoming package v1.0.3

### Reproduction Steps
0. Checkout commit https://github.com/adrian-miasik/unity-behavior-bug-reports/releases/tag/bug-2
1. Open `serialization-type-construct-fail-case.unity` scene
2. Enter Play Mode
3. Press `Save` button
4. Press `F5` hotkey to restart scene
5. Press `Load` button
6. Observe console for assertions

---

### Issue #3 - Incorrect Node States Upon Deserialization: TODO UNITY TICKET
- Third issue is related to loading/deserializing behavior graph data in _either_ a original/re-created GameObject.
- No specific error, but the behavior graph is unable to progress along the branch/move on to the next node action after loading. Where if you didn't load the behavior graph, the branch would complete/move through all nodes. (E.G. provided below. Notice navigation action state)
- Original:
<p align="center">
  <img src="https://github.com/user-attachments/assets/822fa0dc-e16e-4e1b-a03c-c25b6505517a" width="500">
</p>

- Loaded:
<p align="center">
  <img src="https://github.com/user-attachments/assets/e14de308-9e48-4b8d-9f6a-63631c63887d" width="500">
</p>

- See thread comment for additional details: https://discussions.unity.com/t/behavior-errors-deserializing-behaviorgraph-json-after-re-creating-related-gameobjects/1534184/19

### Reproduction Steps
0. Checkout commit https://github.com/adrian-miasik/unity-behavior-bug-reports/releases/tag/bug-3
1. Open `serialization-incorrect-status-upon-loading-case` scene
2. Enter Play Mode
3. Press `Save` button before agents reach their target positions.
4. Press `Load` button before agents reach their target positions.
5. Observe console for lack of logs. (See behavior graph debug mode)

Note: You might have to re-assign the 'Queue Slot' variable to the navigation node action on the Agent behavior graph.
Note: I've added a pause toggle (ESC key) so you can investigate the graph debug mode while game is running but not progressing.

---

### Issue #4 - Construction Type Cast (Inheritance) - TODO UNITY TICKET
- The forth issue is related to loading/deserializing behavior graph data back into re-created GameObjects. When clearing a variable from a behavior agent that derives from some sort of base class. And when that base class is utilized in a node action. We get as assert on deserialization. Setting the deriving Blackboard Variable (bbv) class to null, should remove all references of that variable type on every node in the behavior agent, but doesn't seem to. It seems to retain the type and try to re-construct the saved type. Normally it wouldn't/shouldn't retain the type after clearing. Though not sure entirely if it's related to inheritance/derived types, but this is how I managed to reproduce the error in `1.0.3`. 1.0.3 fixed most construct type errors I encountered, except for this specific one.
- Error:
```C#
ArgumentException: Failed to construct type. Could not resolve type from TypeName=[Unity.Behavior.ComponentToComponentBlackboardVariable`2[[QueueSlot, Assembly-CSharp, QueueBase, Assembly-CSharp]], Unity.Behavior].
```

When clearing/nullifying a BBV in the described case above, we still retain it's type in the node JSON data on serialization. This is looking at the navigation action node references.
```JSON
"QueueBase": {
  "$type": "Unity.Behavior.ComponentToComponentBlackboardVariable`2[[QueueSlot, Assembly-CSharp, QueueBase, Assembly-CSharp]], Unity.Behavior",
  "m_LinkedVariable": {
    "$id": 6,
    "m_Value": "GlobalObjectId_V1-0-00000000000000000000000000000000-0-0",
    "GUID": "0b7458251b662f41acad256b113a27da",
    "Name": "Queue Slot"
  },
  "m_Value": "GlobalObjectId_V1-0-00000000000000000000000000000000-0-0",
  "GUID": "",
  "Name": ""
```

Normally clearing/nullifying a BBV should remove reference to the type as seen below. This is looking at the navigation action node references.
Note: This example was when the QueueSlot derived from MonoBehavior, not QueueBase.
```JSON
"QueueSlot": {
  "$id": 6,
  "GUID": "0b7458251b662f41acad256b113a27da",
  "Name": "Queue Slot",
  "m_Value": "GlobalObjectId_V1-0-00000000000000000000000000000000-0-0"
},
```

### Reproduction Steps
0. Checkout commit https://github.com/adrian-miasik/unity-behavior-bug-reports/releases/tag/bug-4
1. Open `serialization-construct-type-error-1.0.3.unity` scene
2. Enter Play Mode
3. Press `Save` button
5. Press `Load` button
6. Observe console for assertions

Note: You might have to re-assign the 'Queue Slot' variable to the navigation node action on the Agent behavior graph.

---


## Call Stacks
### Issue #1 - GlobalObjectID Mismatch:
```C#
InvalidOperationException: An error occured while deserializing asset reference GUID=[7a89880255e2246de83870fb9c1e9803]. Asset is not yet loaded and will result in a null reference.
Unity.Behavior.Serialization.Json.DeserializationResult.Throw () (at ./Library/PackageCache/com.unity.behavior/com.unity.serialization/Runtime/Unity.Serialization/Json/JsonSerialization+FromJson.cs:123)
Unity.Behavior.Serialization.Json.JsonSerialization.FromJsonOverride[T] (System.String json, T& container, Unity.Behavior.Serialization.Json.JsonSerializationParameters parameters) (at ./Library/PackageCache/com.unity.behavior/com.unity.serialization/Runtime/Unity.Serialization/Json/JsonSerialization+FromJson.cs:236)
Unity.Behavior.RuntimeSerializationUtility+JsonBehaviorSerializer.Deserialize (System.String graphJson, Unity.Behavior.BehaviorGraph graph, Unity.Behavior.RuntimeSerializationUtility+IUnityObjectResolver`1[TSerializedFormat] resolver) (at ./Library/PackageCache/com.unity.behavior/Runtime/Utilities/RuntimeSerializationUtility.cs:139)
Unity.Behavior.BehaviorGraphAgent.Deserialize[TSerializedFormat] (TSerializedFormat serialized, Unity.Behavior.RuntimeSerializationUtility+IBehaviorSerializer`1[TSerializedFormat] serializer, Unity.Behavior.RuntimeSerializationUtility+IUnityObjectResolver`1[TSerializedFormat] resolver) (at ./Library/PackageCache/com.unity.behavior/Runtime/Execution/Components/BehaviorGraphAgent.cs:359)
Unity.Behavior.SerializationExample.SerializationExampleSceneController.Load () (at Assets/Samples/Behavior/1.0.2/Runtime Serialization/SerializationExampleSceneController.cs:124)
Unity.Behavior.SerializationExample.SerializationExampleSceneController.OnGUI () (at Assets/Samples/Behavior/1.0.2/Runtime Serialization/SerializationExampleSceneController.cs:90)
```

### Issue #2 - Type Construction Cast:
```C#
ArgumentException: Failed to construct type. Could not resolve type from TypeName=[Unity.Behavior.ComponentToComponentBlackboardVariable`2[[QueueSlot, Assembly-CSharp, QueueSlot, Assembly-CSharp]], Unity.Behavior].
Unity.Behavior.Serialization.Json.DeserializationResult.Throw () (at ./Library/PackageCache/com.unity.behavior/com.unity.serialization/Runtime/Unity.Serialization/Json/JsonSerialization+FromJson.cs:123)
Unity.Behavior.Serialization.Json.JsonSerialization.FromJsonOverride[T] (System.String json, T& container, Unity.Behavior.Serialization.Json.JsonSerializationParameters parameters) (at ./Library/PackageCache/com.unity.behavior/com.unity.serialization/Runtime/Unity.Serialization/Json/JsonSerialization+FromJson.cs:236)
Unity.Behavior.RuntimeSerializationUtility+JsonBehaviorSerializer.Deserialize (System.String graphJson, Unity.Behavior.BehaviorGraph graph, Unity.Behavior.RuntimeSerializationUtility+IUnityObjectResolver`1[TSerializedFormat] resolver) (at ./Library/PackageCache/com.unity.behavior/Runtime/Utilities/RuntimeSerializationUtility.cs:139)
Unity.Behavior.BehaviorGraphAgent.Deserialize[TSerializedFormat] (TSerializedFormat serialized, Unity.Behavior.RuntimeSerializationUtility+IBehaviorSerializer`1[TSerializedFormat] serializer, Unity.Behavior.RuntimeSerializationUtility+IUnityObjectResolver`1[TSerializedFormat] resolver) (at ./Library/PackageCache/com.unity.behavior/Runtime/Execution/Components/BehaviorGraphAgent.cs:359)
Unity.Behavior.SerializationExample.SerializationExampleSceneController.Load () (at Assets/Samples/Behavior/1.0.2/Runtime Serialization/SerializationExampleSceneController.cs:124)
Unity.Behavior.SerializationExample.SerializationExampleSceneController.OnGUI () (at Assets/Samples/Behavior/1.0.2/Runtime Serialization/SerializationExampleSceneController.cs:90)
```
---

### Issue #3 - TODO
- Not applicable. See behavior graph screenshots.

---

### Issue #4 - Type Construction Cast (Inherited):
```C#
ArgumentException: Failed to construct type. Could not resolve type from TypeName=[Unity.Behavior.ComponentToComponentBlackboardVariable`2[[QueueSlot, Assembly-CSharp, QueueBase, Assembly-CSharp]], Unity.Behavior].
Unity.Behavior.Serialization.Json.DeserializationResult.Throw () (at ./Library/PackageCache/com.unity.behavior/com.unity.serialization/Runtime/Unity.Serialization/Json/JsonSerialization+FromJson.cs:121)
Unity.Behavior.Serialization.Json.JsonSerialization.FromJsonOverride[T] (System.String json, T& container, Unity.Behavior.Serialization.Json.JsonSerializationParameters parameters) (at ./Library/PackageCache/com.unity.behavior/com.unity.serialization/Runtime/Unity.Serialization/Json/JsonSerialization+FromJson.cs:236)
Unity.Behavior.RuntimeSerializationUtility+JsonBehaviorSerializer.Deserialize (System.String graphJson, Unity.Behavior.BehaviorGraph graph, Unity.Behavior.RuntimeSerializationUtility+IUnityObjectResolver`1[TSerializedFormat] resolver) (at ./Library/PackageCache/com.unity.behavior/Runtime/Utilities/RuntimeSerializationUtility.cs:139)
Unity.Behavior.BehaviorGraphAgent.Deserialize[TSerializedFormat] (TSerializedFormat serialized, Unity.Behavior.RuntimeSerializationUtility+IBehaviorSerializer`1[TSerializedFormat] serializer, Unity.Behavior.RuntimeSerializationUtility+IUnityObjectResolver`1[TSerializedFormat] resolver) (at ./Library/PackageCache/com.unity.behavior/Runtime/Execution/Components/BehaviorGraphAgent.cs:359)
Unity.Behavior.SerializationExample.SerializationExampleSceneController.Load () (at Assets/Samples/Behavior/1.0.2/Runtime Serialization/SerializationExampleSceneController.cs:171)
Unity.Behavior.SerializationExample.SerializationExampleSceneController.OnGUI () (at Assets/Samples/Behavior/1.0.2/Runtime Serialization/SerializationExampleSceneController.cs:91)
```
