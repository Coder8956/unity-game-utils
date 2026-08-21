using UGUE.Editor.Scene.Common;
using UGUE.Editor.Scene.Const;
using UnityEditor;

namespace UGUE.Editor.Scene.PlayOpen
{
    public class EUGUSceneEntrance
    {
        private const string TitleName = "Launch";
        private const string ScenePath = "Assets/DPGame/Scenes/Launch.scene";
        private const string MenuPlayTitle = EUGUSceneConst.MenuTitlePlay + TitleName;
        private const string MenuOpenTitle = EUGUSceneConst.MenuTitleOpen + TitleName;

        /// <summary>
        /// 运行场景
        /// </summary>
        [MenuItem(MenuPlayTitle, priority = EUGUSceneConst.PlayTitlePriority)]
        private static void PlayScene()
        {
            if (EditorApplication.isPlaying) return;

            EUGUSceneCommon.OpenScene(ScenePath);

            if (!EditorApplication.isPlaying)
                EditorApplication.isPlaying = true;
        }

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