How to update the SDK
========================
Additional guide:
 - https://www.youtube.com/watch?v=Sp7vUE3Hmtw&embeds_euri=https%3A%2F%2Fpublisher.unity.com%2F&embeds_origin=https%3A%2F%2Fpublisher.unity.com&feature=emb_logo

1. Go to the `Asset Store Tools/Asset Store Validator` in file menu bar
2. Choose path and Click `Validate`.
![image](https://lh3.googleusercontent.com/drive-viewer/AAOQEOTq4gwWCMEUH9GswXMADDvKgtsv9GQAAyDx2DiqykrG1k7dzy-SlS4XB-luIWkcG5P1_aHIQY6XbaydTbm5Hlv9KcTh=s1600)

3. Go to the https://publisher.unity.com/packages
4. Select project and click `Upload via Unity Editor`
![image](https://lh3.googleusercontent.com/drive-viewer/AAOQEOQoL4wR0U5D1bIPDCU3GDhHbTH6N4qInOnYUGV4WuXnm8kYU-jcRELZEMyiDRoyCnJqLhkhyhnfExvhrWcJqE9wuYPcAg=s1600)
5. Select path and click `Publish`

How to test the SDK on XBox
========================
_Make sure that you have installed Windows SDK_  

Additional guide:
 - https://learn.microsoft.com/en-us/windows/uwp/xbox-apps/getting-started_
 - https://learn.microsoft.com/en-us/windows/uwp/xbox-apps/development-environment-setup

1. Go to the `File/Build Settings..` in file menu bar
2. Choose platform `UWP` and click `Build`
![image](https://lh3.googleusercontent.com/drive-viewer/AAOQEOR_hU7w2xHm7hTZjFw46NHjDczy5YT7wHdCojr3RcXuGSiQjNqwCP8xTDH7UovhnNp3Wk0E7Up-TsgY5YNE60z5haz5=s1600)
3. Run the build in VisualStudio  
4. Setup Remote Machine:  
In the Solution Explorer, right-click the project and select Properties. Select the Debug tab, change Target device to Remote Machine, type the IP address or hostname of your Xbox One console into the Remote machine field, and select Universal (Unencrypted Protocol) in the Authentication Mode drop-down list.
5. Select x64 from the dropdown to the left of the green play button in the top menu bar.  
6. When you press F5, your app will build and start to deploy on your Xbox One.  
7. The first time you do this, Visual Studio will prompt you for a PIN for your Xbox One. You can get a PIN by starting Dev Home on your Xbox One and selecting the Show Visual Studio pin button.  
8. After you have paired, your app will start to deploy  
