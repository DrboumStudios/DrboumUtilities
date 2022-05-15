using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEditor;
using UnityEditor.Compilation;
using Debug = UnityEngine.Debug;
namespace Drboum.Utilities.Editor {
    [InitializeOnLoad]
    public class AsmdefDebug {

        private const           string AssemblyReloadEventsEditorPref      = "AssemblyReloadEventsTime";
        private const           string AssemblyCompilationEventsEditorPref = "AssemblyCompilationEvents";
        private static readonly int    ScriptAssembliesPathLen             = "Library/ScriptAssemblies/".Length;

        private static readonly Dictionary<string, DateTime> s_StartTimes = new Dictionary<string, DateTime>();

        private static readonly StringBuilder s_BuildEvents = new StringBuilder();
        private static          double        s_CompilationTotalTime;
        private static          bool          CompilationOngoing;
        private static readonly Stopwatch     _stopwatch;
        static AsmdefDebug()
        {
            CompilationOngoing                              =  false;
            _stopwatch                                      =  new Stopwatch();
            CompilationPipeline.assemblyCompilationStarted  += CompilationPipelineOnAssemblyCompilationStarted;
            CompilationPipeline.assemblyCompilationFinished += CompilationPipelineOnAssemblyCompilationFinished;
            AssemblyReloadEvents.beforeAssemblyReload       += AssemblyReloadEventsOnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload        += AssemblyReloadEventsOnAfterAssemblyReload;
        }

        private static void CompilationPipelineOnAssemblyCompilationStarted(string assembly)
        {
            if ( CompilationOngoing == false ) {
                _stopwatch.Start();
                CompilationOngoing = true;
            }
            s_StartTimes[assembly] = DateTime.UtcNow;
        }

        private static void CompilationPipelineOnAssemblyCompilationFinished(string assembly, CompilerMessage[] arg2)
        {
            DateTime time     = s_StartTimes[assembly];
            TimeSpan timeSpan = DateTime.UtcNow - s_StartTimes[assembly];
            s_CompilationTotalTime += timeSpan.TotalMilliseconds;
            s_BuildEvents.AppendFormat("{0:0.00}s {1}\n", timeSpan.TotalMilliseconds / 1000f,
                assembly.Substring(ScriptAssembliesPathLen, assembly.Length - ScriptAssembliesPathLen));
        }

        private static void AssemblyReloadEventsOnBeforeAssemblyReload()
        {
            CompilationOngoing = false;
            _stopwatch.Stop();

            s_BuildEvents.AppendFormat("compilation total: {0:0.00}s\n", s_CompilationTotalTime / 1000f);
            s_BuildEvents.AppendFormat("compilation total RealTime: {0:0.00}s\n",
                _stopwatch.ElapsedMilliseconds / 1000f);
            EditorPrefs.SetString(AssemblyReloadEventsEditorPref,      DateTime.UtcNow.ToBinary().ToString());
            EditorPrefs.SetString(AssemblyCompilationEventsEditorPref, s_BuildEvents.ToString());
            _stopwatch.Reset();
        }

        private static void AssemblyReloadEventsOnAfterAssemblyReload()
        {
            string binString = EditorPrefs.GetString(AssemblyReloadEventsEditorPref);

            long bin = 0;
            if ( long.TryParse(binString, out bin) ) {
                DateTime date             = DateTime.FromBinary(bin);
                TimeSpan time             = DateTime.UtcNow - date;
                string   compilationTimes = EditorPrefs.GetString(AssemblyCompilationEventsEditorPref);
                if ( !string.IsNullOrEmpty(compilationTimes) ) {
                    Debug.Log("Compilation Report\n" + compilationTimes + "Assembly Reload Time: " + time.TotalSeconds +
                              "s\n");
                }
            }
            EditorPrefs.DeleteKey(AssemblyReloadEventsEditorPref);
        }
    }

}