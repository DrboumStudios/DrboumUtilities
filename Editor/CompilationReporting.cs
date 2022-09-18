using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEditor;
using UnityEditor.Compilation;
using Debug = UnityEngine.Debug;

namespace Drboum.Utilities.Editor
{
    [InitializeOnLoad]
    public class CompilationDiagnostic
    {

        private const string AssemblyReloadEventsEditorPref = "AssemblyReloadEventsTime";
        private const string AssemblyCompilationEventsEditorPref = "AssemblyCompilationEvents";
        private static readonly StringBuilder _buildEvents = new StringBuilder();
        private static readonly Stopwatch Stopwatch;

        static CompilationDiagnostic()
        {
            Stopwatch = new Stopwatch();
            CompilationPipeline.compilationStarted += CompilationPipelineOnAssemblyCompilationStarted;
            CompilationPipeline.compilationFinished += CompilationPipelineOnAssemblyCompilationFinished;
            AssemblyReloadEvents.beforeAssemblyReload += AssemblyReloadEventsOnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += AssemblyReloadEventsOnAfterAssemblyReload;
        }

        private static void CompilationPipelineOnAssemblyCompilationStarted(object assembly)
        {
            Stopwatch.Start();
        }

        private static void CompilationPipelineOnAssemblyCompilationFinished(object assembly)
        {
            Stopwatch.Stop();
            _buildEvents.AppendFormat("compilation total: {0:0.00}s\n", Stopwatch.ElapsedMilliseconds / 1000f);
        }

        private static void AssemblyReloadEventsOnBeforeAssemblyReload()
        {
            EditorPrefs.SetString(AssemblyReloadEventsEditorPref, DateTime.UtcNow.ToBinary().ToString());
            EditorPrefs.SetString(AssemblyCompilationEventsEditorPref, _buildEvents.ToString());
            Stopwatch.Reset();
        }
        
        private static void AssemblyReloadEventsOnAfterAssemblyReload()
        {
            string binString = EditorPrefs.GetString(AssemblyReloadEventsEditorPref);

            if ( long.TryParse(binString, out long bin) )
            {
                DateTime date = DateTime.FromBinary(bin);
                TimeSpan time = DateTime.UtcNow - date;
                string compilationTimes = EditorPrefs.GetString(AssemblyCompilationEventsEditorPref);
                if ( !string.IsNullOrEmpty(compilationTimes) )
                {
                    Debug.Log("Compilation Report\n" + compilationTimes + "Assembly Reload Time: " + time.TotalSeconds +
                              "s\n");
                }
            }
            EditorPrefs.DeleteKey(AssemblyReloadEventsEditorPref);
            EditorPrefs.DeleteKey(AssemblyCompilationEventsEditorPref);
        }
    }
}