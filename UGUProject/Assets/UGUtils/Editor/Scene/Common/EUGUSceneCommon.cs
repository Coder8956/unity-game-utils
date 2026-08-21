using UnityEditor.SceneManagement;

namespace UGU.Editor
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