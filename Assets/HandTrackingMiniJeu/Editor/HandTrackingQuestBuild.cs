using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace HandTrackingMiniJeu.Editor
{
    [InitializeOnLoad]
    public static class HandTrackingQuestBuild
    {
        const string PendingKey="HandTrackingMiniJeu.PendingAndroidBuild";
        const string ReportPath="Tools/Integrations/hand-android-build.json";
        const string CancelPath="Tools/Integrations/hand-build.cancel";
        public const string ApkPath="Builds/Quest3/HandTrackingMiniJeu.apk";
        [Serializable] class Result { public bool running; public bool success; public string result; public string apk; public ulong size; public int errors; }
        static HandTrackingQuestBuild(){if(SessionState.GetBool(PendingKey,false))EditorApplication.update+=TryBuild;}

        [MenuItem("Tools/Hand Tracking MiniJeu/Build Quest 3 APK")]
        public static void Build()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play mode before building.");
            if(File.Exists(CancelPath))File.Delete(CancelPath);
            if(!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android,BuildTarget.Android))throw new InvalidOperationException("Install Android Build Support first.");
            Directory.CreateDirectory("Builds/Quest3");
            File.WriteAllText(ReportPath,JsonUtility.ToJson(new Result{running=true,apk=ApkPath}));
            SessionState.SetBool(PendingKey,true);
            EditorApplication.update-=TryBuild;EditorApplication.update+=TryBuild;
            if(EditorUserBuildSettings.activeBuildTarget!=BuildTarget.Android)
            {
                if(!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android,BuildTarget.Android))
                {SessionState.SetBool(PendingKey,false);throw new InvalidOperationException("Unable to switch to Android.");}
            }
        }
        static void TryBuild()
        {
            if(!SessionState.GetBool(PendingKey,false)){EditorApplication.update-=TryBuild;return;}
            if(File.Exists(CancelPath))
            {EditorApplication.update-=TryBuild;SessionState.SetBool(PendingKey,false);File.WriteAllText(ReportPath,JsonUtility.ToJson(new Result{result="Cancelled before build to include scene changes",apk=ApkPath}));return;}
            if(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorUserBuildSettings.activeBuildTarget!=BuildTarget.Android)return;
            EditorApplication.update-=TryBuild;SessionState.SetBool(PendingKey,false);
            var result=new Result{apk=ApkPath};
            try
            {
                EditorUserBuildSettings.buildAppBundle=false;
                var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{HandTrackingSceneBuilder.ScenePath},locationPathName=ApkPath,target=BuildTarget.Android,options=BuildOptions.None});
                result.result=report.summary.result.ToString();result.errors=(int)report.summary.totalErrors;result.size=report.summary.totalSize;
                result.success=report.summary.result==BuildResult.Succeeded && File.Exists(ApkPath);
            }
            catch(Exception error){result.result=error.Message;Debug.LogException(error);}
            File.WriteAllText(ReportPath,JsonUtility.ToJson(result,true));
        }
    }
}
