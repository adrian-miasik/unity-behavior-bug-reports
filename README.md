
<h1 align="center">unity-bug-behavior-types</h1>

## About

- This repository was created to highlight 2 issues I ran into using the new [Unity Behaviors](https://docs.unity3d.com/Packages/com.unity.behavior@1.0/manual/index.html) package. Specifically relating to serialization/deserialization of the behavior agent graphs.
- This repo will submitted to Unity's issue tracker. Once the tickets are submitted, I'll update this README.md document linking back to those tickets so resolution can be tracked.
- For more additional context see my [Unity discussion thread](https://discussions.unity.com/t/behavior-errors-deserializing-behaviorgraph-json-after-re-creating-related-gameobjects/1534184).

## Context
- **Editor Version:** `Unity 6 Preview` - `6000.0.20f1`
- **Package:** `com.unity.behavior` - `v1.0.2`

## Bugs
### Issue #1 - GlobalObjectID - `// TODO: ISSUE TRACKER URL`
- The first issue is related to loading/deserializing behavior graph data back into re-created GameObjects.
- Error:
```InvalidOperationException: An error occured while deserializing asset reference GUID=[7a89880255e2246de83870fb9c1e9803]. Asset is not yet loaded and will result in a null reference.```
- Seems to happen due to a mismatch of GlobalObjectID. See thread for additional information and my proposed solution/workaround.

### Reproduction Steps
1. Open `serialization-global-object-id-case.unity` scene
2. Enter Play Mode
3. Press `Save` button
4. Press `F5` hotkey to restart scene
5. Press `Load` button
6. Observe console for assertions

---

### Issue #2 - Type Construction Cast - `// TODO: ISSUE TRACKER URL`
- The second issue is related to being unable to construct a type for each custom property found within the saved JSON behavior graph data.
- Error: 
```
ArgumentException: Failed to construct type. Could not resolve type from TypeName=[Unity.Behavior.ComponentToComponentBlackboardVariable`2[[UnityEngine.Transform, UnityEngine.CoreModule, UnityEngine.Transform, UnityEngine.CoreModule]], Unity.Behavior].` Rethrow as AggregateException: One or more errors occurred.
```
- Might be an internal behavior package bug as `@Darren_Kelly` described: https://discussions.unity.com/t/behavior-errors-deserializing-behaviorgraph-json-after-re-creating-related-gameobjects/1534184/3

### Reproduction Steps
- `// TODO`


---

## Call Stacks
### Issue #1:
```
InvalidOperationException: An error occured while deserializing asset reference GUID=[7a89880255e2246de83870fb9c1e9803]. Asset is not yet loaded and will result in a null reference.
Unity.Behavior.Serialization.Json.DeserializationResult.Throw () (at ./Library/PackageCache/com.unity.behavior/com.unity.serialization/Runtime/Unity.Serialization/Json/JsonSerialization+FromJson.cs:123)
Unity.Behavior.Serialization.Json.JsonSerialization.FromJsonOverride[T] (System.String json, T& container, Unity.Behavior.Serialization.Json.JsonSerializationParameters parameters) (at ./Library/PackageCache/com.unity.behavior/com.unity.serialization/Runtime/Unity.Serialization/Json/JsonSerialization+FromJson.cs:236)
Unity.Behavior.RuntimeSerializationUtility+JsonBehaviorSerializer.Deserialize (System.String graphJson, Unity.Behavior.BehaviorGraph graph, Unity.Behavior.RuntimeSerializationUtility+IUnityObjectResolver`1[TSerializedFormat] resolver) (at ./Library/PackageCache/com.unity.behavior/Runtime/Utilities/RuntimeSerializationUtility.cs:139)
Unity.Behavior.BehaviorGraphAgent.Deserialize[TSerializedFormat] (TSerializedFormat serialized, Unity.Behavior.RuntimeSerializationUtility+IBehaviorSerializer`1[TSerializedFormat] serializer, Unity.Behavior.RuntimeSerializationUtility+IUnityObjectResolver`1[TSerializedFormat] resolver) (at ./Library/PackageCache/com.unity.behavior/Runtime/Execution/Components/BehaviorGraphAgent.cs:359)
Unity.Behavior.SerializationExample.SerializationExampleSceneController.Load () (at Assets/Samples/Behavior/1.0.2/Runtime Serialization/SerializationExampleSceneController.cs:124)
Unity.Behavior.SerializationExample.SerializationExampleSceneController.OnGUI () (at Assets/Samples/Behavior/1.0.2/Runtime Serialization/SerializationExampleSceneController.cs:90)

```

### Issue #2:
- `// TODO`
