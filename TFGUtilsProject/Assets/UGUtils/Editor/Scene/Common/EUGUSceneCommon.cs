using UnityEditor.SceneManagement;

namespace UGUE.Editor.Scene.Common
{
    public class EUGUSceneCommon
    {
        public static void OpenScene(string scenePath)
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(scenePath);
        }
    }
}

//
// public static class EditorTools
// {
//     [MenuItem("Assets/GameStart #F5", false, 999)] // 使用快捷键Shift+F5进入或退出Play模式
//     static void PlayGame()
//     {
//         if (EditorApplication.isPlaying) return;
//
//         var mainScene = "Assets/Scenes/Init.unity";
//
//         if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
//             EditorSceneManager.OpenScene(mainScene);
//
//         if (!EditorApplication.isPlaying)
//             EditorApplication.isPlaying = true;
//     }
//
//     [MenuItem("Assets/Open Hotfix Sln #F9", false, 998)]
//     public static void OpenHotfixSln()
//     {
//         var path = Directory.GetParent(Application.dataPath) + "\\HotUpdateScripts\\HotUpdateScripts.sln";
//         System.Diagnostics.Process.Start(path);
//     }
// }