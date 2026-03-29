#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TST
{
    // Boot Strapper의 용도는 에디터 상에서, 개발이 용이하도록 시스템을 별도로 불러주는 도우미 클래스.
    public class BootStrapper
    {
        private const string BootStrapperMenuPath = "TST/BootStrapper/Activate BootStrapper";
        private static bool IsActivateBootStrapper
        {
            get => UnityEditor.EditorPrefs.GetBool(BootStrapperMenuPath, false);
            set => UnityEditor.EditorPrefs.SetBool(BootStrapperMenuPath, value);
        }

        [UnityEditor.MenuItem(BootStrapperMenuPath, false)]
        private static void ActivateBootStrapper()
        {
            IsActivateBootStrapper = !IsActivateBootStrapper;
            UnityEditor.Menu.SetChecked(BootStrapperMenuPath, IsActivateBootStrapper);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void SystemBoot()
        {
            Scene activeScene = EditorSceneManager.GetActiveScene();
            if (IsActivateBootStrapper && false == activeScene.name.Equals("Main"))
            {
                InternalBoot();
            }
        }
      
        private static void InternalBoot()
        {
            Main.Singleton.Initialize();
            //OptionManager.Singleton.Initialize();
            // TODO : Custom BootStrapper Logic

            SceneManager.LoadScene(SceneType.GardenPrototype.ToString(), LoadSceneMode.Single);
            Main.Singleton.SetBootStrapperState<IngameMapScene>();

            Main.Singleton.StartCoroutine(DelayBoot());

            IEnumerator DelayBoot()
            {
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
                //SoundManager.Singleton.PlayBGM("BGM_Ingame", true);
                //UIManager.Show<CrossHair_UI>(UIList.CrossHair_UI);
                //UIManager.Show<Minimap_UI>(UIList.Minimap_UI);
                //UIManager.Show<IndicatorUI>(UIList.Indicator_UI);
                //UIManager.Show<IngameUI>(UIList.IngameUI);
                //UIManager.Show<MainHudUI>(UIList.MainHudUI);
                //UIManager.Show<InteractionUI>(UIList.InteractionUI);
                //UIManager.Show<ShortCutUI>(UIList.ShortCutUI);
            }
        }

    }
}
#endif