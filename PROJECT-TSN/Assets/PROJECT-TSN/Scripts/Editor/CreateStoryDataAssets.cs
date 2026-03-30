using UnityEditor;
using UnityEngine;

namespace TST.Editor
{
    /// <summary>
    /// StoryData ScriptableObject 에셋 일괄 생성 메뉴.
    /// Tools > TST > Create Story Data Assets 를 실행하면
    /// Assets/PROJECT-TSN/ScriptableObjects/Story/ 아래에
    /// 프롤로그, 엔딩, 꿈 진입 나레이션 에셋 3개를 생성합니다.
    ///
    /// 이미 존재하는 에셋은 덮어쓰지 않습니다.
    /// portrait / backgroundCg 는 null로 두고 나중에 아트 에셋을 연결하십시오.
    /// </summary>
    public static class CreateStoryDataAssets
    {
        private const string OutputFolder = "Assets/PROJECT-TSN/ScriptableObjects/Story";

        [MenuItem("Tools/TST/Create Story Data Assets")]
        public static void Create()
        {
            EnsureFolder(OutputFolder);

            CreatePrologue();
            CreateEndingEnlightenment();
            CreateDreamIntro();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[TST] StoryData 에셋 생성 완료 → {OutputFolder}");
        }

        // ── 프롤로그 ────────────────────────────────────────────────────

        private static void CreatePrologue()
        {
            const string fileName = "Story_Prologue";
            string assetPath = $"{OutputFolder}/{fileName}.asset";

            if (AssetDatabase.LoadAssetAtPath<StoryData>(assetPath) != null)
            {
                Debug.Log($"[TST] 이미 존재하여 건너뜀: {assetPath}");
                return;
            }

            StoryData so = ScriptableObject.CreateInstance<StoryData>();
            so.storyId  = "prologue";
            so.nextPhase = GamePhase.DayAttic;
            so.goToTitle = false;
            so.lines = new StoryData.DialogueLine[]
            {
                new StoryData.DialogueLine
                {
                    speakerName  = "",
                    text         = "도시의 불빛이 별을 지운 지 오래됐다.",
                    portrait     = null,
                    backgroundCg = null
                },
                new StoryData.DialogueLine
                {
                    speakerName  = "",
                    text         = "하지만 이 다락방 창문만은 달랐다.",
                    portrait     = null,
                    backgroundCg = null
                },
                new StoryData.DialogueLine
                {
                    speakerName  = "주인공",
                    text         = "... 오늘 밤도 뭔가 보인다.",
                    portrait     = null,
                    backgroundCg = null
                },
                new StoryData.DialogueLine
                {
                    speakerName  = "",
                    text         = "녹슨 망원경. 빛바랜 성도. 그리고 설명할 수 없는 신호들.",
                    portrait     = null,
                    backgroundCg = null
                },
                new StoryData.DialogueLine
                {
                    speakerName  = "",
                    text         = "그것이 무엇인지, 아직 아무도 모른다.",
                    portrait     = null,
                    backgroundCg = null
                },
                new StoryData.DialogueLine
                {
                    speakerName  = "",
                    text         = "하지만 당신은 — 알아낼 것이다.",
                    portrait     = null,
                    backgroundCg = null
                },
            };

            AssetDatabase.CreateAsset(so, assetPath);
            Debug.Log($"[TST] 생성: {assetPath}");
        }

        // ── 계몽 엔딩 ───────────────────────────────────────────────────

        private static void CreateEndingEnlightenment()
        {
            const string fileName = "Story_Ending_Enlightenment";
            string assetPath = $"{OutputFolder}/{fileName}.asset";

            if (AssetDatabase.LoadAssetAtPath<StoryData>(assetPath) != null)
            {
                Debug.Log($"[TST] 이미 존재하여 건너뜀: {assetPath}");
                return;
            }

            StoryData so = ScriptableObject.CreateInstance<StoryData>();
            so.storyId   = "ending_enlightenment";
            so.nextPhase = GamePhase.DayAttic;   // goToTitle = true 이므로 실제로는 무시됨
            so.goToTitle = true;
            so.lines = new StoryData.DialogueLine[]
            {
                new StoryData.DialogueLine
                {
                    speakerName  = "",
                    text         = "마지막 신호가 들어온 날 밤,",
                    portrait     = null,
                    backgroundCg = null
                },
                new StoryData.DialogueLine
                {
                    speakerName  = "",
                    text         = "망원경 렌즈 너머로 무언가가 — 이쪽을 바라보고 있었다.",
                    portrait     = null,
                    backgroundCg = null
                },
                new StoryData.DialogueLine
                {
                    speakerName  = "주인공",
                    text         = "... 드디어 찾았어.",
                    portrait     = null,
                    backgroundCg = null
                },
                new StoryData.DialogueLine
                {
                    speakerName  = "",
                    text         = "그것은 천체도 현상도 아니었다.",
                    portrait     = null,
                    backgroundCg = null
                },
                new StoryData.DialogueLine
                {
                    speakerName  = "",
                    text         = "오랫동안 잊혀진, 우주의 언어였다.",
                    portrait     = null,
                    backgroundCg = null
                },
                new StoryData.DialogueLine
                {
                    speakerName  = "주인공",
                    text         = "이제 나는 이해한다. 왜 그 신호들이 내게 왔는지.",
                    portrait     = null,
                    backgroundCg = null
                },
                new StoryData.DialogueLine
                {
                    speakerName  = "",
                    text         = "어떤 진실은 혼자 안고 가는 것이 아니라,",
                    portrait     = null,
                    backgroundCg = null
                },
                new StoryData.DialogueLine
                {
                    speakerName  = "",
                    text         = "— 다음 사람에게 전달하는 것이다.",
                    portrait     = null,
                    backgroundCg = null
                },
            };

            AssetDatabase.CreateAsset(so, assetPath);
            Debug.Log($"[TST] 생성: {assetPath}");
        }

        // ── 꿈 진입 공통 나레이션 ────────────────────────────────────────

        private static void CreateDreamIntro()
        {
            const string fileName = "Story_DreamIntro";
            string assetPath = $"{OutputFolder}/{fileName}.asset";

            if (AssetDatabase.LoadAssetAtPath<StoryData>(assetPath) != null)
            {
                Debug.Log($"[TST] 이미 존재하여 건너뜀: {assetPath}");
                return;
            }

            StoryData so = ScriptableObject.CreateInstance<StoryData>();
            so.storyId   = "dream_intro";
            so.nextPhase = GamePhase.Dream;
            so.goToTitle = false;
            so.lines = new StoryData.DialogueLine[]
            {
                new StoryData.DialogueLine
                {
                    speakerName  = "",
                    text         = "잠이 든다.",
                    portrait     = null,
                    backgroundCg = null
                },
                new StoryData.DialogueLine
                {
                    speakerName  = "",
                    text         = "그러나 의식은 — 어딘가 다른 곳으로 흘러간다.",
                    portrait     = null,
                    backgroundCg = null
                },
                new StoryData.DialogueLine
                {
                    speakerName  = "",
                    text         = "흔적이 부른다.",
                    portrait     = null,
                    backgroundCg = null
                },
            };

            AssetDatabase.CreateAsset(so, assetPath);
            Debug.Log($"[TST] 생성: {assetPath}");
        }

        // ── 폴더 보장 ────────────────────────────────────────────────────

        private static void EnsureFolder(string folderPath)
        {
            string[] parts   = folderPath.Split('/');
            string   current = parts[0]; // "Assets"

            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
