# Contributing to RudderStack

Thanks for taking the time and for your help in improving this project!

## Table of contents

- [**RudderStack Contributor Agreement**](#rudderstack-contributor-agreement)
- [**Contribute to this project**](#contribute-to-this-project)
- [**Committing**](#committing)
- [**Installing and setting up RudderStack**](#installing-and-setting-up-rudderstack)
- [**Getting help**](#getting-help)

## RudderStack Contributor Agreement

To contribute to this project, we need you to sign the [**Contributor License Agreement (“CLA”)**][CLA] for the first commit you make. By agreeing to the [**CLA**][CLA]
we can add you to list of approved contributors and review the changes proposed by you.

## Contribute to this project

If you encounter a bug or have any suggestion for improving this project, you can [**submit an issue**][issue] describing your proposed change. Alternatively, you can propose a change by making a pull request and tagging our team members.

For more information on the different ways in which you can contribute to RudderStack, you can chat with us on our [**Slack**][Slack] channel.

## Committing

We prefer squash or rebase commits so that all changes from a branch are committed to master as a single commit. All pull requests are squashed when merged, but rebasing prior to merge gives you better control over the commit message.

## Installing and setting up RudderStack

To contribute to this project, you may need to install RudderStack on your machine. You can do so by following our [**docs**](https://rudderstack.com/docs/get-started/installing-and-setting-up-rudderstack) and set up RudderStack in no time.

## Build a Unity package for validation

Run these commands from the repository root with Python 3:

```sh
python3 -m unittest discover -s tests/PackageExport -v
python3 scripts/export_unitypackage.py
```

The output is `artifacts/RudderStackUnity.unitypackage`, with a `.sha256`
checksum file beside it. The `Build UnityPackage` workflow runs the same commands
for pull requests and pushes to `main`. Download its `RudderStackUnityPackage`
artifact to obtain the package and checksum from that commit.

The exporter includes `Assets/RudderStack` and its metadata. It preserves asset
GUIDs, file contents, and metadata bytes, including Unity's empty YAML mapping
keys. It needs no Unity installation or third-party Python packages. Export tests
verify every SDK file in the archive and reject missing metadata or duplicate
GUIDs. The archive format matches the existing beta package.

Package export does not compile the SDK or validate Unity imports. Use a licensed
Unity Editor 2022.3.16f1 installation with iOS Build Support for native validation.
In a clean project, install `com.unity.nuget.newtonsoft-json` 3.2.1 through Package
Manager, then import the package through **Assets > Import Package > Custom
Package**. Follow the device checks below. Xcode is also required to build the
generated iOS project. The exported package retains the checkout's package version;
this validation artifact is not a new published release.

## Offline startup regression tests

Install .NET SDK 8, then run:

```sh
dotnet build tests/OfflineStartup/OfflineStartup.csproj --configuration Release -p:UnityApiCompatibility=true
dotnet test tests/OfflineStartup/OfflineStartup.csproj --configuration Release
```

The suite compiles the SDK's actual client, request, queue, and persistence source
against the checked-in `RudderAnalytics.dll` and `NodaTime.dll`. Unity platform APIs
are replaced with small test shims. The tests use real encryption, files, and local
HTTP endpoints. Deterministic HTTP-handler tests cover timeout/cancellation,
response classification, and bounded retry timing. No live RudderStack credentials
are required. Test files stay outside `Assets` and the exported Unity package.
The compatibility build checks runtime sources against .NET Standard 2.1 using C# 8.

These tests do not verify Unity coroutine scheduling or IL2CPP. Before release:

1. Import the package into a clean Unity sample with the RudderStack prefab.
2. Build an iOS IL2CPP player and launch it in airplane mode.
3. Wait for `InitializeRoutine` to complete and track several events.
4. Restore connectivity and verify delivery without restarting or reinitializing.
5. Repeat with queued events from a previous offline launch.
6. Verify coroutine and UniTask callers on Unity 2021.3.33f1 and the repository's
   Unity 2022.3.16f1 baseline. Check Android for the shared initialization path.
7. Record device/build details and confirm no duplicate workers or startup exception.

## Getting help

For any questions, concerns, or queries, you can start by asking a question on our [**Slack**][Slack] channel.
<br><br>

### We look forward to your feedback on improving this project!


<!----variables---->

[issue]: https://github.com/rudderlabs/rudder-sdk-unity-v2/issues/new
[CLA]: https://rudderlabs.wufoo.com/forms/rudderlabs-contributor-license-agreement
[Slack]: https://rudderstack.com/join-rudderstack-slack-community/
