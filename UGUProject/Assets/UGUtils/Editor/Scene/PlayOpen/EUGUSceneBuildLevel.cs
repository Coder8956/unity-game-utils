using UGUE.Editor.Scene.Common;
using UGUE.Editor.Scene.Const;
using UnityEditor;

namespace UGUE.Editor.Scene.PlayOpen
{
    public class EUGUSceneVillageCoope
    {
        private const string TitleName = "BuilLevel";
        private const string ScenePath = "Assets/Tmp/Scenes/BuildLevel.unity";
        private const string MenuOpenTitle = EUGUSceneConst.MenuTitleOpen + TitleName;

        /// <summary>
        /// 运行场景
        /// </summary>
        // [MenuItem(MenuPlayTitle, priority = UGUESceneConst.PlayTitlePriority)]
        // private static void PlayScene()
        // {
        //     if (EditorApplication.isPlaying) return;
        //
        //     UGUESceneCommon.OpenScene(ScenePath);
        //
        //     if (!EditorApplication.isPlaying)
        //         EditorApplication.isPlaying = true;
        // }

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