# RudderStack Analytics - Unity
Check the [GitHub codebase](https://github.com/rudderlabs/rudder-sdk-unity-v2) to get a more hands-on understanding of the SDK.

## Installing the Unity SDK 
To integrate the RudderStack Unity SDK with your project download rudder-sdk-unity.unitypackage from our [GitHub repository](https://github.com/rudderlabs/rudder-sdk-unity-v2) or import it from Unity AssetStore

## QuickStart
**SDK setup on website**
Sign up to RudderStack Cloud.
Set up a .NET source in your dashboard. You should be able to see a write key for this source:

![image](https://www.rudderstack.com/docs/images/event-stream-sources/dotnet-write-key-new.png)

**SDK setup on Unity**
For testing SDK follow this steps:
 - Open scene `Example` in Assets/RudderStack/Examples. 
 - Click play and go to runtime. 
 - Set your DataPlane and WriteKey in InputFields. 
 - Click on `Initialize` button.
 - Fill UserId, EventName, PropertyType, PropertyValue
 - Click on `Send Track` button.
*(Optional)*
 - Fill DeviceToken and AdvertisingId  
 - Click on `Set User` button.

![image](https://lh6.googleusercontent.com/JyZM9W8i-elIBqbhQA6PPXG7jqvD4Yrv-Mun91QVpcOAuhMv-WYCp5oABX-PTzcVGLM=w2400)

## Initializing the RudderStack client
To initialize the RudderStack client, add `RudderStack` prefab to your scene.
Set dataPlaneUrl and writeKey fields in script `RudderStackInitializer` on `RudderStack` GameObject.

![image](https://lh5.googleusercontent.com/EDRNI1qUdzwuGPCEfe_K-_1-0Gz6V2ETx878rwyuOMQfaQTHczoXmOxNLvW-0CsPk_Y=w2400)
## Identify user
Use this method for identify user
```sh
RsAnalytics.Client.PutAdvertisingId(id);
RsAnalytics.Client.PutDeviceToken(token);
```

## Track
You can record the users’ in-game activity through the track method. Every user action is called an event.
A sample track event is as shown:
```
RudderAnalytics.Client.Track(
    "UserId",
    "EventName",
    new Dictionary<string, object> {  {"PropertyType", "PropertyValue"}, }
);
```
