using UnityEditor;
using UnityEngine;

namespace TST.Editor
{
    /// <summary>
    /// DreamEventData ScriptableObject 에셋 일괄 생성 메뉴.
    /// Tools > TST > Create Dream Event Assets 를 실행하면
    /// Assets/PROJECT-TSN/ScriptableObjects/Dream/Events/ 아래에
    /// 샘플 꿈 이벤트 에셋을 생성합니다.
    ///
    /// 이미 존재하는 에셋은 덮어쓰지 않습니다.
    /// dialogue 필드는 null로 두고 Story SO 에셋 연결 후 사용하십시오.
    /// </summary>
    public static class CreateDreamEventAssets
    {
        private const string OutputFolder = "Assets/PROJECT-TSN/ScriptableObjects/Dream/Events";

        [MenuItem("Tools/TST/Create Dream Event Assets")]
        public static void Create()
        {
            EnsureFolder(OutputFolder);

            CreateSampleDialogueEvent();
            CreateSampleChoiceEvent();
            CreateSampleDiceEvent();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[TST] DreamEventData 에셋 생성 완료 → {OutputFolder}");
        }

        // ── 대화 전용 이벤트 (가장 단순한 구조) ────────────────────────────

        private static void CreateSampleDialogueEvent()
        {
            const string fileName = "DreamEvent_Dialogue_Sample";
            string assetPath = $"{OutputFolder}/{fileName}.asset";

            if (AssetDatabase.LoadAssetAtPath<DreamEventData>(assetPath) != null)
            {
                Debug.Log($"[TST] 이미 존재하여 건너뜀: {assetPath}");
                return;
            }

            DreamEventData so = ScriptableObject.CreateInstance<DreamEventData>();
            so.eventId              = "dream_dialogue_sample";
            so.eventTitle           = "샘플 대화 이벤트";
            so.triggerRecordType    = RecordType.CosmicTrace;
            so.checkEndingOnComplete = false;
            so.dialogueSequences    = System.Array.Empty<StoryData>();
            so.nodes = new DreamEventData.DreamNode[]
            {
                new DreamEventData.DreamNode
                {
                    nodeId      = "node_01",
                    nodeType    = DreamEventData.DreamNodeType.Dialogue,
                    dialogue    = null,    // Story SO 에셋 연결 필요
                    nextNodeId  = "node_end"
                },
                new DreamEventData.DreamNode
                {
                    nodeId      = "node_end",
                    nodeType    = DreamEventData.DreamNodeType.End,
                    nextNodeId  = ""
                },
            };

            AssetDatabase.CreateAsset(so, assetPath);
            Debug.Log($"[TST] 생성: {assetPath}");
        }

        // ── 선택지 분기 이벤트 ────────────────────────────────────────────

        private static void CreateSampleChoiceEvent()
        {
            const string fileName = "DreamEvent_Choice_Sample";
            string assetPath = $"{OutputFolder}/{fileName}.asset";

            if (AssetDatabase.LoadAssetAtPath<DreamEventData>(assetPath) != null)
            {
                Debug.Log($"[TST] 이미 존재하여 건너뜀: {assetPath}");
                return;
            }

            DreamEventData so = ScriptableObject.CreateInstance<DreamEventData>();
            so.eventId              = "dream_choice_sample";
            so.eventTitle           = "샘플 선택지 이벤트";
            so.triggerRecordType    = RecordType.CelestialBody;
            so.checkEndingOnComplete = false;
            so.dialogueSequences    = System.Array.Empty<StoryData>();
            so.nodes = new DreamEventData.DreamNode[]
            {
                // 도입 대화
                new DreamEventData.DreamNode
                {
                    nodeId      = "node_intro",
                    nodeType    = DreamEventData.DreamNodeType.Dialogue,
                    dialogue    = null,
                    nextNodeId  = "node_choice"
                },
                // 선택지 분기
                new DreamEventData.DreamNode
                {
                    nodeId   = "node_choice",
                    nodeType = DreamEventData.DreamNodeType.Choice,
                    choices  = new DreamEventData.DreamChoice[]
                    {
                        new DreamEventData.DreamChoice
                        {
                            label                  = "선택지 A",
                            nextNodeId             = "node_result_a",
                            requiresCondition      = false,
                            requiredEnlightenment  = 0f
                        },
                        new DreamEventData.DreamChoice
                        {
                            label                  = "선택지 B (조건부)",
                            nextNodeId             = "node_result_b",
                            requiresCondition      = true,
                            requiredEnlightenment  = 30f
                        },
                    }
                },
                // 결과 A
                new DreamEventData.DreamNode
                {
                    nodeId     = "node_result_a",
                    nodeType   = DreamEventData.DreamNodeType.Dialogue,
                    dialogue   = null,
                    nextNodeId = "node_end"
                },
                // 결과 B
                new DreamEventData.DreamNode
                {
                    nodeId     = "node_result_b",
                    nodeType   = DreamEventData.DreamNodeType.Dialogue,
                    dialogue   = null,
                    nextNodeId = "node_end"
                },
                // 종료
                new DreamEventData.DreamNode
                {
                    nodeId   = "node_end",
                    nodeType = DreamEventData.DreamNodeType.End,
                    nextNodeId = ""
                },
            };

            AssetDatabase.CreateAsset(so, assetPath);
            Debug.Log($"[TST] 생성: {assetPath}");
        }

        // ── 주사위 판정 이벤트 ────────────────────────────────────────────

        private static void CreateSampleDiceEvent()
        {
            const string fileName = "DreamEvent_Dice_Sample";
            string assetPath = $"{OutputFolder}/{fileName}.asset";

            if (AssetDatabase.LoadAssetAtPath<DreamEventData>(assetPath) != null)
            {
                Debug.Log($"[TST] 이미 존재하여 건너뜀: {assetPath}");
                return;
            }

            DreamEventData so = ScriptableObject.CreateInstance<DreamEventData>();
            so.eventId              = "dream_dice_sample";
            so.eventTitle           = "샘플 주사위 이벤트";
            so.triggerRecordType    = RecordType.Phenomenon;
            so.checkEndingOnComplete = false;
            so.dialogueSequences    = System.Array.Empty<StoryData>();
            so.nodes = new DreamEventData.DreamNode[]
            {
                // 도입 대화
                new DreamEventData.DreamNode
                {
                    nodeId      = "node_intro",
                    nodeType    = DreamEventData.DreamNodeType.Dialogue,
                    dialogue    = null,
                    nextNodeId  = "node_dice"
                },
                // 주사위 판정
                new DreamEventData.DreamNode
                {
                    nodeId               = "node_dice",
                    nodeType             = DreamEventData.DreamNodeType.Dice,
                    diceMax              = 6,
                    rewindLimit          = 1,
                    diceSuccessThreshold = 4,
                    successNodeId        = "node_success",
                    failNodeId           = "node_fail"
                },
                // 성공 결과
                new DreamEventData.DreamNode
                {
                    nodeId     = "node_success",
                    nodeType   = DreamEventData.DreamNodeType.Dialogue,
                    dialogue   = null,
                    nextNodeId = "node_end"
                },
                // 실패 결과
                new DreamEventData.DreamNode
                {
                    nodeId     = "node_fail",
                    nodeType   = DreamEventData.DreamNodeType.Dialogue,
                    dialogue   = null,
                    nextNodeId = "node_end"
                },
                // 종료
                new DreamEventData.DreamNode
                {
                    nodeId   = "node_end",
                    nodeType = DreamEventData.DreamNodeType.End,
                    nextNodeId = ""
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
