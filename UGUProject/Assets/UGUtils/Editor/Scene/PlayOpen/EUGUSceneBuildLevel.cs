using UnityEditor;

namespace UGU.Editor
{
    public class EUGUSceneBuildLevel
    {
        private const string TitleName = "BuilLevel";
        private const string ScenePath = "Assets/Tmp/Scenes/BuildLevel.unity";
        private const string MenuOpenTitle = EUGUSceneConst.MenuTitleOpen + TitleName;

        /// <summary>
        /// 打开场景
        /// </summary>
        [MenuItem(MenuOpenTitle, priority = EUGUSceneConst.OpenTitlePriority)]
        private static void OpenScene()
        {
            if (EditorApplication.isPlaying) return;
            EUGUSceneCommon.OpenScene(ScenePath);
        }
    }
}
