# ToryAgent Unity Editor Tools — 사용 가이드

ToryAgent는 Claude Code CLI와 Unity Editor를 연결하는 브릿지입니다.
Claude Code에서 MCP 도구로 호출하거나, Unity Editor의 **Claude Editor Window**에서 자연어 프롬프트로 사용할 수 있습니다.

---

## 아키텍처 요약

```
Claude Code CLI
    ↓  stdio / JSON-RPC
ToryAgent.McpServer  (Tools/ToryAgent.McpServer/)
    ↓  TCP :63211
ToryBridgeServer  (Unity Editor 프로세스 내부)
    ↓  메인 스레드 디스패치
IUnityEditorTool 구현체들
```

---

## Claude Editor Window 사용법

1. Unity 메뉴 → **ToryAgent > Claude Editor** 창 열기
2. `Settings` 항목에 `ClaudeEditorSettings` 에셋 할당
3. 텍스트 박스에 자연어 프롬프트 입력 후 **Run Claude CLI** 클릭
4. **MCP Tools 탭**에서 등록된 도구 목록 확인 가능

### ClaudeEditorSettings 주요 설정

| 항목 | 기본값 | 설명 |
|---|---|---|
| `Claude Executable Path` | (비워두면 자동 탐색) | claude CLI 경로 |
| `Use Bypass Permissions` | ON | 권한 확인 없이 실행 |
| `Timeout Seconds` | 120 | 최대 대기 시간 (초) |
| `Max Turns` | 1 | 에이전트 루프 최대 횟수. **1 = 단일 응답** (무한 실행 방지). 다단계 작업 시 늘릴 것 |
| `System Prompt` | (기본 지시문) | Claude에게 전달되는 시스템 프롬프트 |

> **중요**: `Max Turns = 1`이면 Claude가 MCP 도구를 한 번 호출하고 바로 종료됩니다.
> 여러 단계가 필요한 작업(예: 씬 탐색 → 오브젝트 배치 → 저장)은 `Max Turns`를 3~5 이상으로 올리세요.

---

## MCP 도구 전체 목록

### 씬 정보 조회

#### `get_scene_info`
현재 열린 씬의 이름, 경로, dirty 상태, 루트 오브젝트 수를 반환합니다.

```json
입력: {}
출력: { "sceneName": "SampleScene", "scenePath": "Assets/...", "isDirty": false, "rootCount": 5 }
```

#### `get_scene_hierarchy`
씬 전체 오브젝트를 트리 구조로 반환합니다.

```json
입력: { "includeInactive": true }
출력: [{ "instanceId": 1234, "name": "Main Camera", "active": true, "children": [...] }]
```

---

### 오브젝트 검색 및 정보 조회

#### `find_gameobjects`
조건에 맞는 오브젝트 목록을 검색합니다.

```json
입력: { "nameFilter": "Cube", "includeInactive": false }
출력: [{ "instanceId": 1234, "name": "Cube", "path": "Root/Cube" }]
```

#### `get_gameobject_details`
특정 오브젝트의 상세 정보(Transform, 컴포넌트 목록, 프로퍼티)를 반환합니다.

```json
입력: { "instanceId": 1234 }
출력: { "name": "Cube", "active": true, "position": {...}, "components": [...] }
```

#### `get_selection`
현재 에디터에서 선택된 오브젝트 정보를 반환합니다.

```json
입력: {}
출력: { "selectedCount": 1, "objects": [{ "instanceId": 1234, "name": "Cube" }] }
```

---

### 오브젝트 생성 및 삭제

#### `create_gameobject`
빈 GameObject를 생성합니다.

```json
입력: { "name": "MyObject", "parentInstanceId": 0 }
출력: { "success": true, "instanceId": 5678, "name": "MyObject" }
```

#### `create_primitive`
프리미티브 타입의 GameObject를 생성합니다.

```json
입력: {
  "primitiveType": "Cube",        // Cube | Sphere | Capsule | Cylinder | Plane | Quad
  "name": "FloorTile",
  "position": { "x": 0, "y": 0, "z": 0 },
  "scale": { "x": 1, "y": 1, "z": 1 },
  "parentInstanceId": 0
}
출력: { "success": true, "instanceId": 5678, "name": "FloorTile" }
```

#### `duplicate_gameobject`
오브젝트를 복제합니다.

```json
입력: { "instanceId": 1234 }
출력: { "success": true, "instanceId": 9999, "name": "Cube (1)" }
```

#### `delete_gameobject`
오브젝트를 씬에서 삭제합니다 (Undo 가능).

```json
입력: { "instanceId": 1234 }
출력: { "success": true }
```

---

### 오브젝트 속성 변경

#### `set_transform`
오브젝트의 위치/회전/스케일을 설정합니다.

```json
입력: {
  "instanceId": 1234,
  "position": { "x": 1, "y": 0, "z": 2 },
  "rotation": { "x": 0, "y": 45, "z": 0 },
  "scale": { "x": 2, "y": 2, "z": 2 }
}
```

#### `set_gameobject_name`
오브젝트 이름을 변경합니다.

```json
입력: { "instanceId": 1234, "name": "NewName" }
```

#### `set_gameobject_active`
오브젝트를 활성화/비활성화합니다.

```json
입력: { "instanceId": 1234, "active": false }
```

#### `set_gameobject_parent`
오브젝트의 부모를 변경합니다. `parentInstanceId`를 0으로 설정하면 루트로 이동합니다.

```json
입력: { "instanceId": 1234, "parentInstanceId": 5678, "worldPositionStays": true }
```

---

### 컴포넌트 조작

#### `get_components`
오브젝트에 붙어있는 컴포넌트 목록을 반환합니다.

```json
입력: { "instanceId": 1234 }
출력: { "components": ["Transform", "MeshRenderer", "BoxCollider"] }
```

#### `add_component`
컴포넌트를 추가합니다.

```json
입력: { "instanceId": 1234, "componentType": "Rigidbody" }
```

#### `set_component_property`
리플렉션으로 컴포넌트의 프로퍼티/필드를 수정합니다.

```json
입력: {
  "instanceId": 1234,
  "componentType": "Rigidbody",
  "propertyName": "mass",
  "value": "5.0"
}
```

---

### 머티리얼 생성 및 적용

#### `create_material`
새 머티리얼을 생성합니다. 기본 셰이더는 URP Lit입니다.

```json
입력: {
  "assetPath": "Assets/Materials/RedMat.mat",
  "shaderName": "Universal Render Pipeline/Lit",
  "color": { "r": 1, "g": 0, "b": 0, "a": 1 }
}
출력: { "success": true, "assetPath": "Assets/Materials/RedMat.mat" }
```

#### `assign_material`
오브젝트의 Renderer에 머티리얼을 할당합니다.

```json
입력: { "instanceId": 1234, "materialAssetPath": "Assets/Materials/RedMat.mat" }
```

#### `set_material_color`
머티리얼의 색상을 변경합니다.

```json
입력: {
  "materialAssetPath": "Assets/Materials/RedMat.mat",
  "color": { "r": 0, "g": 1, "b": 0, "a": 1 },
  "propertyName": "_BaseColor"
}
```

---

### 씬 저장

#### `save_scene`
현재 씬을 저장합니다.

```json
입력: {}
출력: { "success": true, "sceneName": "SampleScene" }
```

---

### 에셋 관리

#### `list_assets`
지정한 폴더의 에셋 목록을 반환합니다.

```json
입력: { "path": "Assets/PROJECT-A", "filter": "t:Material", "recursive": true }
출력: { "assets": ["Assets/PROJECT-A/Mat1.mat", ...] }
```

> `filter`는 AssetDatabase.FindAssets 검색 필터 형식을 따릅니다.
> 예: `t:Texture2D`, `t:ScriptableObject`, `t:Prefab`

#### `create_folder`
새 폴더를 생성합니다.

```json
입력: { "parentPath": "Assets", "folderName": "MyFolder" }
출력: { "success": true, "guid": "..." }
```

---

## 활용 예시 프롬프트

### 씬 탐색

```
현재 씬의 오브젝트 목록을 계층 구조로 보여줘
```

```
"Player"라는 이름을 가진 오브젝트를 찾아서 Transform 정보를 알려줘
```

### 오브젝트 배치

```
위치 (2, 0, 3)에 Cube를 만들고 이름을 "FloorTile_01"로 지정해줘
```

```
"Environment"라는 빈 오브젝트를 만들고,
그 안에 Plane을 하나 배치해서 바닥으로 써줘
```

### 머티리얼 적용

```
Assets/Materials/GreenGrass.mat 머티리얼을 만들고 (녹색),
씬에 있는 Plane 오브젝트에 할당해줘
```

### 다단계 작업 (Max Turns ≥ 3 권장)

```
씬을 파악하고, 정원 배치에 맞게 Plane 바닥 1개와
Sphere 나무 5개를 적당한 위치에 배치하고 씬을 저장해줘
```

---

## 주의사항

- 모든 도구는 **Undo 지원** — 작업 후 Ctrl+Z로 되돌릴 수 있습니다.
- `set_component_property`는 리플렉션 기반으로 복잡한 타입(Vector3 등)은 지원하지 않을 수 있습니다.
- 머티리얼 경로는 반드시 `Assets/`로 시작하는 프로젝트 상대 경로여야 합니다.
- 씬 변경 후에는 반드시 `save_scene`을 호출해야 저장됩니다.
- Unity Editor가 실행 중이고 ToryBridgeServer가 포트 63211에서 리스닝 중이어야 합니다.
  (에디터 로드 시 `[InitializeOnLoad]`로 자동 시작됩니다.)
