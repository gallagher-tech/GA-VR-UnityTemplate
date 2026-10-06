using UnityEngine;
using System.Collections;
using System.IO;
using SimpleFileBrowser;
using UnityEngine.Video;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Android;
using System.Reflection;

public class UIManager : MonoBehaviour
{

    /// <summary>
    /// The File Browser transform which is used to position the UI 
    /// </summary>
    public Transform BrowserTranform;

    public Dropdown SceneDropdown;

    private void Awake()
    {
        #if !UNITY_EDITOR && UNITY_ANDROID
            typeof(SimpleFileBrowser.FileBrowserHelpers).GetField("m_shouldUseSAF", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, (bool?)false);
        #endif
    }

    void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
            GetExternalReadPermission();  
#endif

        // Set filters (optional)
        // It is sufficient to set the filters just once (instead of each time before showing the file browser dialog), 
        // if all the dialogs will be using the same filters
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Videos", ".mp4", ".mov"));

        // Set default filter that is selected when the dialog is shown (optional)
        // Returns true if the default filter is set successfully
        // In this case, set Images filter as the default filter
        FileBrowser.SetDefaultFilter(".mp4");
        //BrowserTranform.gameObject.SetActive(false);
        FileBrowser.HideDialog();
    }

    private void GetExternalReadPermission()
    {
        GetPermission(Permission.ExternalStorageRead);
    }

    private static void GetPermission(string permission)
    {
        using var buildCodes = new AndroidJavaClass("android.os.Build$VERSION_CODES");
        using var buildVersion = new AndroidJavaClass("android.os.Build$VERSION");
            //Check SDK version > 29
            if (buildVersion.GetStatic<int>("SDK_INT") > buildCodes.GetStatic<int>("Q"))
        {
            using var environment = new AndroidJavaClass("android.os.Environment");
            //?hecking if permission already exists
            if (!environment.CallStatic<bool>("isExternalStorageManager"))
            {
                using var settings = new AndroidJavaClass("android.provider.Settings");
                using var uri = new AndroidJavaClass("android.net.Uri");
                using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                using var parsedUri = uri.CallStatic<AndroidJavaObject>("parse", $"package:{Application.identifier}");
                using var intent = new AndroidJavaObject("android.content.Intent",
                    settings.GetStatic<string>("ACTION_MANAGE_APP_ALL_FILES_ACCESS_PERMISSION"),
                    parsedUri);
                currentActivity.Call("startActivity", intent);
            }
        }

        if (Permission.HasUserAuthorizedPermission(permission))
        {
            Debug.Log("Permission is already granted.");
            return;
        }

        Debug.LogFormat("Requesting permission to {0}.", permission);
        Permission.RequestUserPermission(permission);
    }

public void OnSceneSelect(int index)
    {
        SceneManager.LoadScene(SceneDropdown.options[index].text, LoadSceneMode.Single);
    }

    // Update is called once per frame
    public void OpenVideoLoadDialog()
    {
        var campos = Camera.main.gameObject.transform.position;
        var camrot = Camera.main.gameObject.transform.rotation;
        ////BrowserTranform.gameObject.SetActive(true);

        //BrowserTranform.rotation = camrot;
        FileBrowser.ShowLoadDialog(OnFileSelected, null, FileBrowser.PickMode.Files, false, null, null, "Load", "Select");
        //BrowserTranform.position = new Vector3(0, -0.099f, 0);
    }

    private void OnFileSelected(string[] filePaths)
    {
        // Print paths of the selected files    
        if (filePaths.Length > 0)
        {
            string vidpath = filePaths[0];
            #if UNITY_ANDROID
            //vidpath = "/sdcard/Documents"
                Debug.Log($"Filename = {FileBrowserHelpers.GetFilename(vidpath)}");
                string destinationPath = Path.Combine(Application.persistentDataPath, FileBrowserHelpers.GetFilename(vidpath));
                Debug.Log($"Destination path = {destinationPath}");

            FileBrowserHelpers.CopyFile(vidpath, destinationPath);
            #endif
            var playerObjects = UnityEngine.Object.FindObjectsByType<VideoPlayer>(FindObjectsSortMode.None);
            foreach (var player in playerObjects)
            {
                player.source = VideoSource.Url;
                player.url = vidpath;
                player.isLooping = true;
                Debug.Log($"Set video for {player.name} to {player.url}");
            }
            // set up for the inital play
            UnityEngine.Object.FindFirstObjectByType<SyncedVideo>().InitialPlay();
        }

    }

    public void OnVolumeChange(float volume)
    {
        var playerObjects = UnityEngine.Object.FindObjectsByType<VideoPlayer>(FindObjectsSortMode.None);
        foreach (var player in playerObjects)
        {
            player.SetDirectAudioVolume(0, volume);
        }
    }

    public void OnSyncButton()
    {
        UnityEngine.Object.FindFirstObjectByType<SyncedVideo>().ForceSync();
    }

    public void OnJumpForward()
    {
        UnityEngine.Object.FindFirstObjectByType<SyncedVideo>().JumpForward();
    }

    public void OnJumpBackward()
    {
        UnityEngine.Object.FindFirstObjectByType<SyncedVideo>().JumpBackward();
    }

    public void OnRestart()
    {
        UnityEngine.Object.FindFirstObjectByType<SyncedVideo>().Restart();
    }

    public void ToggleVideoPlay()
    {
        var manager = UnityEngine.Object.FindFirstObjectByType<SyncedVideo>();
        manager.TogglePlayPause();
    }
    private void OnCancel()
    {
        Debug.Log("Cancelled file load");
        BrowserTranform.gameObject.SetActive(false);

    }
}
