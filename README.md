# RudderStack Analytics - Unity
Check the [GitHub codebase](https://github.com/rudderlabs/rudder-sdk-unity-v2) to get a more hands-on understanding of the SDK.

## Installing the Unity SDK
To integrate the RudderStack Unity SDK with your project download rudder-sdk-unity.unitypackage from our [GitHub repository](https://github.com/rudderlabs/rudder-sdk-unity-v2) or import it from Unity AssetStore

## QuickStart
### SDK setup on website

Sign up to RudderStack Cloud.
Set up a .NET source in your dashboard. You should be able to see a write key for this source:

![image](https://www.rudderstack.com/docs/images/event-stream-sources/dotnet-write-key-new.png)

### Test the SDK

![image](https://lh3.googleusercontent.com/drive-viewer/AAOQEOShQkt6Ldl_F9BGs-s8LyKMnAdDejtLSbklUlFHi5-1e75piD0Z5dOSzHlzuQxZGuAknlcjBxT-T3yBJ9R4cPYm6B9QqA=s1600)

Do everything in following order:

1. Open the scene `Assets/RudderStack/RudderAnalytics SDK/Examples/Example.unity`
2. Run it
3. Try to put the **Device Token** *(optionaly)*
4. Try to put the **Advertising ID** *(optionaly)*
5. Set **Data Plane URL** and **Write Key**
6. Click **Initialize** button
7. Enter **User ID** and click **Identify**

Next you can test events *(right-top corner)*, aliasing, or automatic scene detection *(left-bottom corner)*

### Use the SDK in your project

1. Open the scene, which is going to be loaded first in your game
2. Right click in Hierarchy -> RudderStack Object  
   Or in top menu: GameObject -> RudderStack Object  
   This object is going to be marked as `DontDestroyOnLoad` so you don't need to create it in other scenes.  
   **In fact you should not create it more than once!**
3. There are three components on the object.
   You may disable `RS Screen View` or `Rs Logger`
   by clicking on the check box in top left corner of the component if you don't need them.

Next you should write the code.
You may check `Assets/RudderStack/RudderAnalytics SDK/Examples/TestController.cs` for an example.

#### Initialization

The analytics should be initialized every time your game starts.

```csharp
var config = new RSConfig(dataPlaneUrl)
    .SetGzip(true)                  // This is optional
    .SetAutoCollectAdvertId(true);  // This is optional
RSAnalytics.Initialize(writeKey, config);
```

#### Setting Device Token and Advertising Id

**These values must be set before Initialization!**
```csharp
RSClient.PutAdvertisingId(advertisementId);
RSClient.PutDeviceToken(deviceToken);
```
These values are going to be send only if you've initialized `RsAnalytics` with `SetAutoCollectAdvertId(true)` in `RSConfig`!

#### Identifying the user

The identification persists between runs so it should be done only once.

```csharp
var traits = new Dict() { {"Trait 1", 0}, {"Trait 2", "E"} };
RSAnalytics.Client.Identify(userId, traits);
```

#### Tracking events

```csharp
RSAnalytics.Client.Track("Enemy killed", new Dictionary<string, object> { { "Exp", 22 }, {"Type", "Rat"} } );
```
Similarly you can send such events as `Page`, `Screen`, `Group`.

#### Changing User Id

To change the User Id use Alias:

```csharp
RSAnalytics.Client.Alias(newUserId);
```
#### Deleting the user

```csharp
RSAnalytics.Client.Reset();
```
After the user is reset its *id* and *traits* are lost.
You have to identify them again by calling `Identify()`.
