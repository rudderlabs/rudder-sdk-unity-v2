<p align="center">
  <a href="https://rudderstack.com/">
    <img src="https://user-images.githubusercontent.com/59817155/121357083-1c571300-c94f-11eb-8cc7-ce6df13855c9.png">
  </a>
</p>

<p align="center"><b>The Customer Data Platform for Developers</b></p>

<p align="center">
  <a href="https://github.com/rudderlabs/rudder-sdk-unity-v2/releases">
    <img src="https://img.shields.io/static/v1?label=Version&message=2.0.0&color=Green">
    </a>
  <a href="https://github.com/rudderlabs/rudder-sdk-unity-v2/releases">
    <img src="https://img.shields.io/static/v1?label=Stability&message=Beta&color=105ed5">
    </a>
</p>

<p align="center">
  <b>
    <a href="https://rudderstack.com">Website</a>
    ·
    <a href="https://www.rudderstack.com/docs/sources/event-streams/sdks/rudderstack-unity-sdk/v2/">Documentation</a>
    ·
    <a href="https://rudderstack.com/join-rudderstack-slack-community">Community Slack</a>
  </b>
</p>

---


# RudderStack Unity SDK

The RudderStack Unity SDK lets you track your game event data on all the major platforms like Android, iOS, Windows, macOS, Xbox, and PlayStation.

For detailed documentation on the Unity SDK, click [**here**](https://www.rudderstack.com/docs/sources/event-streams/sdks/rudderstack-unity-sdk/v2/).

## Prerequisites

- Download and install the [Unity development kit](https://store.unity.com/download).
- Sign up for [RudderStack Cloud](https://app.rudderstack.com/signup).
- [Set up a Unity source](https://www.rudderstack.com/docs/dashboard-guides/sources/#adding-a-source) in your [RudderStack Cloud dashboard](https://app.rudderstack.com/). **Note the write key for this source**.

## Get started with the Unity SDK

1. [Download](https://github.com/rudderlabs/rudder-sdk-unity-v2/releases) `rudder-sdk-unity.unitypackage`.

2. Import the downloaded package to your project. From the **Assets** menu, go to **Import Package**  > **Custom Package...**.

3. Select `rudder-sdk-unity.unitypackage` from the downloaded location and click **Open**.

4. Click **Import** in the import popup.

5. Add **RudderStack.prefab** file from the path `Assets/RudderStack/RudderAnalytics SDK/Prefabs/RudderStack.prefab` to every scene in your Unity app. Also, make sure that `RudderStack.prefab` is linked to `RSMaster`, `RSScreenView`, and `RSLogger` scripts.

6. Import the SDK
```csharp
using RudderStack.Unity;
```

7. Initialize the SDK as shown. Replace `WRITE_KEY` and `DATA_PLANE_URL` with the actual values obtained in the [SDK setup requirements](https://www.rudderstack.com/docs/sources/event-streams/sdks/rudderstack-unity-sdk/v2/#sdk-setup-requirements) section.

```csharp
RSAnalytics.Initialize("WRITE_KEY",
		new RSConfig(dataPlaneUrl: "DATA_PLANE_URL"));

// for coroutine
StartCoroutine(RSAnalytics.InitializeRoutine("WRITE_KEY",
		new RSConfig(dataPlaneUrl: "DATA_PLANE_URL")));
```

## Sending events

See the [documentation](https://www.rudderstack.com/docs/sources/event-streams/sdks/rudderstack-unity-sdk/v2/) for details on the supported events. 

A sample `track` call is shown below:

```csharp
RSAnalytics.Client.Track("sample_track",
                new Dictionary<string, object> {
                { "key_1", "value_1" },
                { "key_2", 4 },
                { "key_3", 4.2 },
                { "key_4", true }
                });
```

## Contribute

We would love to see you contribute to this project. Get more information on how to contribute [**here**](./CONTRIBUTING.md).

## About RudderStack

[**RudderStack**](https://rudderstack.com/) is a **customer data platform for developers**. Our tooling makes it easy to deploy pipelines that collect customer data from every app, website and SaaS platform, then activate it in your warehouse and business tools.

More information on RudderStack can be found [**here**](https://github.com/rudderlabs/rudder-server).

## Contact us

For more support on using the RudderStack Unity SDK, [**contact us**](https://rudderstack.com/contact/) or start a conversation in our [**Community Slack**](https://rudderstack.com/join-rudderstack-slack-community).
