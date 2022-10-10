#if (UNITY_ANDROID || UNITY_IOS)
#define SUPPORTED_PLATFORM
#endif

using UnityEditor;
using UnityEngine;
using QuizApp.Editor;
using System.Collections.Generic;
using System;
using System.IO;

[CustomEditor(typeof(AdmobManager))]
public class EditorAdmobManager : Editor
{
#if USE_ADMOB
    private AdmobManager TargetScript;

    SerializedProperty testMode = null;
    SerializedProperty ConsentCanvas = null;
    SerializedProperty EnableInterstitialAds = null;
    SerializedProperty EnableRewardedAds = null;

    SerializedProperty ShowInterstitialAdAfterXGameovers = null;

#if UNITY_ANDROID
    SerializedProperty AndroidAppId = null;
    SerializedProperty AndroidInterstitialId = null;
    SerializedProperty AndroidRewardedVideoId = null;
#elif UNITY_IOS
    SerializedProperty IosAppId = null;
    SerializedProperty IosInterstitialId = null;
    SerializedProperty IosRewardedVideoId = null;
#endif
#endif

#if SUPPORTED_PLATFORM
    private static readonly char[] Seperators = new char[] { ';', ',', ' ' };
    private const string AdmobKey = "USE_ADMOB";
#endif

    private void OnEnable()
    {
#if USE_ADMOB
        TargetScript = (AdmobManager)target;

        testMode = serializedObject.FindProperty("testMode");
        ConsentCanvas = serializedObject.FindProperty("ConsentCanvas");
        EnableInterstitialAds = serializedObject.FindProperty("EnableInterstitialAds");
        EnableRewardedAds = serializedObject.FindProperty("EnableRewardedAds");

        ShowInterstitialAdAfterXGameovers = serializedObject.FindProperty("ShowInterstitialAdAfterXGameovers");

#if UNITY_ANDROID
    AndroidAppId = serializedObject.FindProperty("AndroidAppId");
        AndroidInterstitialId = serializedObject.FindProperty("AndroidInterstitialId");
        AndroidRewardedVideoId = serializedObject.FindProperty("AndroidRewardedVideoId");
#elif UNITY_IOS
        IosAppId = serializedObject.FindProperty("IosAppId");
        IosInterstitialId = serializedObject.FindProperty("IosInterstitialId");
        IosRewardedVideoId = serializedObject.FindProperty("IosRewardedVideoId");
#endif
#endif
    }

    public override void OnInspectorGUI()
    {
#if USE_ADMOB
        Undo.RecordObject(TargetScript, "");

        serializedObject.UpdateIfRequiredOrScript();


        if (TargetScript.testMode == TestMode.Enabled)
        {
            EditorGUILayout.PropertyField(testMode);
            EditorGUILayout.PropertyField(ConsentCanvas);
            EditorGUILayout.PropertyField(EnableInterstitialAds);
            EditorGUILayout.PropertyField(EnableRewardedAds);

            GUILayout.Space(5);

            if (TargetScript.EnableInterstitialAds)
                EditorGUILayout.PropertyField(ShowInterstitialAdAfterXGameovers);
        }
        else
        {
            EditorGUILayout.PropertyField(testMode);
            EditorGUILayout.PropertyField(ConsentCanvas);
            EditorGUILayout.PropertyField(EnableInterstitialAds);
            EditorGUILayout.PropertyField(EnableRewardedAds);

            GUILayout.Space(5);

            if (TargetScript.EnableInterstitialAds)
                EditorGUILayout.PropertyField(ShowInterstitialAdAfterXGameovers);

            GUILayout.Space(10);

#if UNITY_ANDROID
            if (TargetScript.EnableInterstitialAds || TargetScript.EnableRewardedAds)
                EditorGUILayout.PropertyField(AndroidAppId);
            if (TargetScript.EnableInterstitialAds)
                EditorGUILayout.PropertyField(AndroidInterstitialId);
            if (TargetScript.EnableRewardedAds)
                EditorGUILayout.PropertyField(AndroidRewardedVideoId);
#elif UNITY_IOS
            if (TargetScript.EnableInterstitialAds || TargetScript.EnableRewardedAds)
                EditorGUILayout.PropertyField(IosAppId);
            if (TargetScript.EnableInterstitialAds)
                EditorGUILayout.PropertyField(IosInterstitialId);
            if (TargetScript.EnableRewardedAds)
                EditorGUILayout.PropertyField(IosRewardedVideoId);
#endif
        }

        serializedObject.ApplyModifiedProperties();

        GUILayout.Space(10);

        GUI.color = Color.red;

        if (GUILayout.Button("Disable Admob Ads", CustomStyles.m_standardBtn))
            ToggleDefine(false);

        GUI.color = Color.white;
#else
        GUILayout.Space(10);

        GUI.color = Color.green;

        if (GUILayout.Button("Enable Admob Ads", CustomStyles.m_standardBtn))
            ToggleDefine();

        GUI.color = Color.white;

        GUILayout.Space(10);
#endif
    }

    private static void ToggleDefine(bool Add = true)
    {
#if SUPPORTED_PLATFORM
        var curTarget = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
        var curDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(curTarget);

        if (Add)
        {
            if (!AssetDatabase.IsValidFolder(Path.Combine("Assets", "GoogleMobileAds")))
            {
                if (EditorUtility.DisplayDialog("Error", "We could not find the Admob plugin in your project.", "Download Plugin", "Close"))
                    Application.OpenURL("https://github.com/googleads/googleads-mobile-unity/releases");

                return;
            }

            if (!curDefineSymbols.Contains(AdmobKey))
            {
                string[] DefineSymbols = curDefineSymbols.Split(Seperators, StringSplitOptions.RemoveEmptyEntries);
                List<string> newDefineSymbols = new List<string>(DefineSymbols);
                newDefineSymbols.Add(AdmobKey);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(curTarget, string.Join(";", newDefineSymbols.ToArray()));
            }
        }
        else
        {
            if (curDefineSymbols.Contains(AdmobKey))
            {
                string[] DefineSymbols = curDefineSymbols.Split(Seperators, StringSplitOptions.RemoveEmptyEntries);
                List<string> newDefineSymbols = new List<string>(DefineSymbols);
                newDefineSymbols.Remove(AdmobKey);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(curTarget, string.Join(";", newDefineSymbols.ToArray()));
            }
        }
#else
        EditorUtility.DisplayDialog("Error", "Admob is only supported on IOS and Android.", "OK");

#endif
    }
}
