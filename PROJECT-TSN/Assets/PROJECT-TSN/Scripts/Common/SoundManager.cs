//using ClockStone;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//namespace TST
//{
//    public class SoundManager : SingletonBase<SoundManager>
//    {
//        public string IngameStageStandardSoundName { get; set; }

//        public float Volume_Master
//        {
//            get => AudioController.GetGlobalVolume();
//            set => AudioController.SetGlobalVolume(value);
//        }

//        public float Volume_Music
//        {
//            get => AudioController.GetCategoryVolume("BGM");
//            set => AudioController.SetCategoryVolume("BGM", value);
//        }

//        public float Volume_SFX
//        {
//            get => AudioController.GetCategoryVolume("SFX");
//            set => AudioController.SetCategoryVolume("SFX", value);
//        }

//        public float Volume_UI
//        {
//            get => AudioController.GetCategoryVolume("UI");
//            set => AudioController.SetCategoryVolume("UI", value);
//        }
        
//        public void Initialize()
//        {
//            // TODO : Sound Option Setting Initialize
//            GameObject audioControllerPrefab = Resources.Load<GameObject>("Sound System/TST.AudioController");
//            GameObject audioControllerInstance = Instantiate(audioControllerPrefab);
//            DontDestroyOnLoad(audioControllerInstance.gameObject);

//            // 일단 샘플로 저장..
//            IngameStageStandardSoundName = "BGM_Ingame";
//        }

//        public void PlayBGM(string bgmName, bool isIngameStandardSound = false)
//        {
//            AudioController.PlayMusic(bgmName, Volume_Music);

//            if (isIngameStandardSound)
//                IngameStageStandardSoundName = bgmName;
//        }

//        public void StopBGM()
//        {
//            AudioController.StopMusic();
//        }

//        public void StopAllSound()
//        {
//            AudioController.StopAll();
//        }

//        public void PlaySFX(string sfxName, Vector3 position, Transform parentObj = null, float delay = 0f)
//        {
//            AudioController.Play(sfxName, position, parentObj, Volume_SFX, delay);
//        }

//        public void StopSFX(string sfxName)
//        {
//            AudioController.Stop(sfxName);
//        }

//        public void PlayUISFX(string sfxName)
//        {
//            AudioController.Play(sfxName, Camera.main.transform, Volume_UI);
//        }

//        public bool isPlayingSameSound(string sftName)
//        {
//            return AudioController.IsPlaying(sftName);
//        }
//    }
//}
