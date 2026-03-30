//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.SceneManagement;

//#if UNITY_EDITOR
//using UnityEditor;
//#endif


//namespace TST
//{
//    public enum SceneType
//    {
//        None,
//        Empty,
//        GardenPrototype,
//        // Content Scenes
//        Title,
//        Ingame,
//    }

//    public class Main : SingletonBase<Main>
//    {
//        private bool isInitialized = false;

//        private void Start()
//        {
//            Initialize();
//#if UNITY_EDITOR
//            Scene activeScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
//            if (activeScene.name.Equals("Main"))
//            {
//                ChangeScene(SceneType.Title);
//            }
//#else
//            ChangeScene(SceneType.Title);
//#endif
//        }
//        string startItemId = "deco_grass_01";
//        public void Initialize()
//        {
//            if (isInitialized)
//                return;

//            isInitialized = true;

//            //var newEventSystemPrefab = Resources.Load<GameObject>("TST.EventSystem");
//            //var newInstEventSystem = Instantiate(newEventSystemPrefab);
//            //DontDestroyOnLoad(newInstEventSystem);

//            // 필요한 기본 시스템 초기화
//            //UIManager.Singleton.Initialize();
//            //UserDataModel.Singleton.Initialize();
//            //EffectManager.Singleton.Initialize();
//            //GameDataModel.Singleton.Initialize();
//            UserDataModel.Singleton.Initialize();

//            UserDataModel.Singleton.AddItem(startItemId, 999);
//            UserDataModel.Singleton.SelectItem(startItemId);
//            //SoundManager.Singleton.Initialize();
//        }

//        public void SetBootStrapperState<T>() where T : SceneBase
//        {
//            GameObject newSceneBase = new GameObject(typeof(T).Name);
//            newSceneBase.transform.SetParent(transform);
//            currentSceneController = newSceneBase.AddComponent<T>();
//        }

//        public void SystemQuit()
//        {
//            // TODO : 만약에 게임 종료 전에 자동으로 처리해야할 내용이 있다면, 여기서 처리할 것.



//            // 게임 종료.
//#if UNITY_EDITOR
//            EditorApplication.isPlaying = false;
//#else
//            Application.Quit();
//#endif
//        }

//        #region Main Scene Management Core

//        [SerializeField] private SceneType currentScene = SceneType.None;

//        public bool IsOnProgressSceneChange { get; private set; } = false;

//        public SceneBase CurrentSceneController => currentSceneController;
//        private SceneBase currentSceneController;

//        public void ChangeScene(SceneType sceneType, bool isForceLoad = false, System.Action onSceneLoadCompleted = null)
//        {
//            // 이미 같은 씬이면 무시하도록 처리
//            if (currentScene == sceneType && false == isForceLoad)
//                return;

//            switch (sceneType)
//            {
//                case SceneType.Title:
//                    //ChangeScene<TitleScene>(SceneType.Title, onSceneLoadCompleted);
//                    currentScene = SceneType.Title;
//                    break;
//                case SceneType.GardenPrototype:
//                    ChangeScene<IngameMapScene>(SceneType.GardenPrototype, onSceneLoadCompleted);
//                    currentScene = SceneType.Ingame;
//                    break;
//            }
//        }

//        private void ChangeScene<T>(SceneType sceneType, System.Action onSceneLoadCompleted = null) where T : SceneBase
//        {
//            if (IsOnProgressSceneChange)
//                return;

//            StartCoroutine(ChangeSceneAsync<T>(sceneType, onSceneLoadCompleted));
//        }

//        private IEnumerator ChangeSceneAsync<T>(SceneType sceneType, System.Action onSceneLoadCompleted = null) where T : SceneBase
//        {
//            IsOnProgressSceneChange = true;
//            UIManager.Singleton.HideAllUI();
//            //UIManager.Show<LoadingUI>(UIList.LoadingUI);

//            // 기존에 혹시나 Current SceneBase 가 존재한다면 End 처리를 해준다.
//            if (currentSceneController != null)
//            {
//                yield return StartCoroutine(currentSceneController.OnEnd());
//                Destroy(currentSceneController.gameObject);
//                currentSceneController = null;
//            }

//            // Empty 씬으로 먼저 전환한다.
//            AsyncOperation asyncToEmpty = SceneManager.LoadSceneAsync(SceneType.Empty.ToString(), LoadSceneMode.Single);
//            yield return new WaitUntil(() => asyncToEmpty.isDone);

//            // 새로운 명령 받은 SceneBase를 추가한다.
//            GameObject newSceneBase = new GameObject(typeof(T).Name);
//            newSceneBase.transform.SetParent(transform);
//            currentSceneController = newSceneBase.AddComponent<T>();

//            // 새로운 명령 받은 SceneBase의 OnStart를 호출한다.
//            yield return StartCoroutine(currentSceneController.OnStart());

//            // 로딩 UI를 닫아준다.
//            //UIManager.Hide<LoadingUI>(UIList.LoadingUI);

//            // 넘겨받았던 콜백 함수를 불러다 준다.
//            onSceneLoadCompleted?.Invoke();
//            IsOnProgressSceneChange = false;
//        }



//        #endregion
//    }
//}
