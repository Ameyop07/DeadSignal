using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;
using System.Linq;
public static class BuildGame
{
    public static string Begin(){foreach(var call in EditorApplication.delayCall?.GetInvocationList()??new System.Delegate[0])if(call.Method.DeclaringType.FullName.Contains("BuildGame"))EditorApplication.delayCall-=(EditorApplication.CallbackFunction)call;File.WriteAllText("Builds/build-result.txt","Building...");EditorApplication.update+=ScheduledBuild;return "Build scheduled; result will be written to Builds/build-result.txt";}
    static void ScheduledBuild(){EditorApplication.update-=ScheduledBuild;try{Main();}catch(System.Exception e){File.WriteAllText("Builds/build-result.txt","Failed: "+e);throw;}}
    public static string Main()
    {
        if(EditorApplication.isPlaying)throw new System.Exception("Stop Play Mode before building.");
        PlayerSettings.runInBackground=true;
        Directory.CreateDirectory("Builds/DeadSignal");
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),locationPathName="Builds/DeadSignal/DeadSignal.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
        string result=report.summary.result+"; errors="+report.summary.totalErrors+"; warnings="+report.summary.totalWarnings+"; bytes="+report.summary.totalSize;
        File.WriteAllText("Builds/build-result.txt",result);
        if(report.summary.result!=BuildResult.Succeeded)throw new System.Exception(result);
        return result;
    }
}
