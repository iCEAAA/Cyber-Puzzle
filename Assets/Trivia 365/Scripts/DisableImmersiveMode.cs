//Code by Zocker1996 - https://forum.unity.com/threads/disable-immersivemode-unity5.313911/#post-3090959
//Attach this script to a gameobject in the scene if you want to use it

using UnityEngine;

public class DisableImmersiveMode : MonoBehaviour
{
    // Use this for initialization
    private void Awake()
    {
        SetupAndroidTheme(ToARGB(Color.white), ToARGB(Color.white));
    }

    private static void SetupAndroidTheme(int primaryARGB, int darkARGB)
    {
#if  UNITY_ANDROID && !UNITY_EDITOR
    string label = Application.productName;
    Screen.fullScreen = false;
    AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
    activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
    {
        AndroidJavaClass layoutParamsClass = new AndroidJavaClass("android.view.WindowManager$LayoutParams");
        int flagFullscreen = layoutParamsClass.GetStatic<int>("FLAG_FULLSCREEN");
        int flagNotFullscreen = layoutParamsClass.GetStatic<int>("FLAG_FORCE_NOT_FULLSCREEN");
        int flagDrawsSystemBarBackgrounds = layoutParamsClass.GetStatic<int>("FLAG_DRAWS_SYSTEM_BAR_BACKGROUNDS");
        AndroidJavaObject windowObject = activity.Call<AndroidJavaObject>("getWindow");
        windowObject.Call("clearFlags", flagFullscreen);
        windowObject.Call("addFlags", flagNotFullscreen);
        windowObject.Call("addFlags", flagDrawsSystemBarBackgrounds);
        int sdkInt = new AndroidJavaClass("android.os.Build$VERSION").GetStatic<int>("SDK_INT");
        int lollipop = 21;
        if (sdkInt > lollipop)
        {
            windowObject.Call("setStatusBarColor", darkARGB);
            string myName = activity.Call<string>("getPackageName");
            AndroidJavaObject packageManager = activity.Call<AndroidJavaObject>("getPackageManager");
            AndroidJavaObject drawable = packageManager.Call<AndroidJavaObject>("getApplicationIcon", myName);
            AndroidJavaObject taskDescription = new AndroidJavaObject("android.app.ActivityManager$TaskDescription", label, drawable.Call<AndroidJavaObject>("getBitmap"), primaryARGB);
            activity.Call("setTaskDescription", taskDescription);
        }
    }));
#endif
    }

    private static int ToARGB(Color color)
    {
        Color32 c = color;
        byte[] b = new byte[] { c.b, c.g, c.r, c.a };
        return System.BitConverter.ToInt32(b, 0);
    }
}
