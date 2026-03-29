using UnityEngine;

namespace TST
{
    public class GameOptionManager : SingletonBase<GameOptionManager>
    {
        private void Start()
        {
            Initialize();
        }

        public void Initialize()
        {
            //int currentQuality = QualitySettings.GetQualityLevel();
            //QualitySettings.SetQualityLevel(2);
            //Screen.fullScreenMode = FullScreenMode.FullScreenWindow;

            //Resolution[] resolution = Screen.resolutions;  

            //for (int i = 0; i < resolution.Length; i++)
            //{
            //    Debug.Log(resolution[i].width + "x" + Screen.resolutions[i].height);
            //    Debug.Log(resolution[i].refreshRateRatio.value);
            //}
        }

        public void SetQuality(int qualityLevel)
        {
            //int currentQuality = QualitySettings.GetQualityLevel();
            QualitySettings.SetQualityLevel(qualityLevel);
        }

        public int GetCurrentQuality()
        {
            return QualitySettings.GetQualityLevel();
        }
    }
}
