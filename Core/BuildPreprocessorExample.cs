using System.Diagnostics;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
#endif
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ItsBaptiste.Toolbox.Core {
    public class BuildPreprocessorExample : IPreprocessBuildWithReport, IPostprocessBuildWithReport {
        public int callbackOrder => 0;

        private static string _projectFolder;

        public void OnPostprocessBuild(BuildReport report) {
            Debug.Log(report);
        }

        public void OnPreprocessBuild(BuildReport report) {
            ClearFolder();
            // ZipFolder(projectFolder);
        }

        private static void ClearFolder() {
            FileUtil.DeleteFileOrDirectory("Build/");
            _projectFolder = $"{Path.Combine(Application.dataPath, "../")}/Build";
            Directory.CreateDirectory(_projectFolder);
        }


        static void ZipFolder(string folderPath, string zipName = "test") {
            Process proc = new Process();
            proc.StartInfo.FileName = @"C:\Program Files\7-Zip\7zFM.exe";
            proc.StartInfo.Arguments = $"a -tzip \"{zipName}\" \"{folderPath}\"";
            proc.Start();
        }
    }
}