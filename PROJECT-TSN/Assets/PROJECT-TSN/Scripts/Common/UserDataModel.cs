//using System;
//using System.Collections.Generic;
//using UnityEngine;

//namespace TST
//{
//    [Serializable]
//    public struct UserPlacedItem
//    {
//        public string itemId;
//        public int cellX;
//        public int cellY;
//        public int rotation; // 필요 없으면 0으로 고정
//    }

//    public class UserDataModel : SingletonBase<UserDataModel>
//    {
//        public event Action<string> OnSelectedItemChanged;
//        public event Action OnInventoryChanged;
//        public event Action OnGardenChanged;
//        public event Action OnBuildingsChanged;

//        public bool isSelected = false;
//        public string SelectedItemId { get; private set; }

//        [SerializeField] SerializableWrapDictionary<string, int> inventoryCounts = new SerializableWrapDictionary<string, int>();
//        [SerializeField] SerializableWrapDictionary<Vector2Int, UserPlacedItem> gardenPlaced = new SerializableWrapDictionary<Vector2Int, UserPlacedItem>();

//        // ─── 방치형 게임 데이터 ───────────────────────────────
//        [SerializeField] List<BuildingSaveData> placedBuildings = new List<BuildingSaveData>();
//        [SerializeField] SerializableWrapDictionary<string, double> savedResources = new SerializableWrapDictionary<string, double>();
//        public double LastQuitTime { get; set; } // Unix timestamp

//        public void Initialize()
//        {
//            // 저장된 리소스를 ResourceManager로 로드
//            var resDict = new System.Collections.Generic.Dictionary<ResourceType, double>();
//            foreach (var kv in savedResources)
//            {
//                if (System.Enum.TryParse(kv.Key, out ResourceType rt))
//                    resDict[rt] = kv.Value;
//            }
//            if (ResourceManager.Singleton != null)
//                ResourceManager.Singleton.Initialize(resDict);
//        }

//        public void SaveResources()
//        {
//            if (ResourceManager.Singleton == null) return;
//            savedResources.Clear();
//            foreach (var kv in ResourceManager.Singleton.GetAllResources())
//                savedResources[kv.Key.ToString()] = kv.Value;
//        }

//        // ─── Building Save Data ───────────────────────────────
//        public void AddBuildingData(BuildingSaveData data)
//        {
//            placedBuildings.RemoveAll(b => b.cellX == data.cellX && b.cellY == data.cellY);
//            placedBuildings.Add(data);
//            OnBuildingsChanged?.Invoke();
//        }

//        public void UpdateBuildingData(BuildingSaveData data)
//        {
//            for (int i = 0; i < placedBuildings.Count; i++)
//            {
//                if (placedBuildings[i].cellX == data.cellX && placedBuildings[i].cellY == data.cellY)
//                {
//                    placedBuildings[i] = data;
//                    return;
//                }
//            }
//        }

//        public bool TryGetBuildingData(int cellX, int cellY, out BuildingSaveData data)
//        {
//            foreach (var b in placedBuildings)
//            {
//                if (b.cellX == cellX && b.cellY == cellY)
//                {
//                    data = b;
//                    return true;
//                }
//            }
//            data = default;
//            return false;
//        }

//        public void RemoveBuildingData(int cellX, int cellY)
//        {
//            int removed = placedBuildings.RemoveAll(b => b.cellX == cellX && b.cellY == cellY);
//            if (removed > 0) OnBuildingsChanged?.Invoke();
//        }

//        public IEnumerable<BuildingSaveData> GetAllBuildings() => placedBuildings;

//        public void SelectItem(string itemId)
//        {
//            isSelected = true;
//            SelectedItemId = itemId;
//            OnSelectedItemChanged?.Invoke(itemId);
//        }

//        public void ClearSelectedItem()
//        {
//            // �̹� ������ ��������� �ߺ� �̺�Ʈ ����
//            if (!isSelected && string.IsNullOrEmpty(SelectedItemId))
//                return;

//            isSelected = false;
//            SelectedItemId = null;

//            // UI/��Ʈ�ѷ��� �����ϰ� ���� ���ɼ��� ����
//            OnSelectedItemChanged?.Invoke(null);
//        }

//        public int GetItemCount(string itemId)
//        {
//            return inventoryCounts.TryGetValue(itemId, out int c) ? c : 0;
//        }

//        public IEnumerable<KeyValuePair<string, int>> GetAllInventoryCounts()
//        {
//            foreach (var kv in inventoryCounts)
//                yield return kv;
//        }

//        public IEnumerable<UserPlacedItem> GetAllPlaced()
//        {
//            foreach (var kv in gardenPlaced)
//                yield return kv.Value;
//        }

//        public void AddItem(string itemId, int amount = 1)
//        {
//            if (!inventoryCounts.ContainsKey(itemId))
//                inventoryCounts[itemId] = 0;

//            inventoryCounts[itemId] += amount;
//            OnInventoryChanged?.Invoke();
//        }

//        public bool TryPlace(string itemId, int cellX, int cellY, int rotation = 0)
//        {
//            var key = new Vector2Int(cellX, cellY);
//            if (gardenPlaced.ContainsKey(key))
//                return false;

//            gardenPlaced[key] = new UserPlacedItem
//            {
//                itemId = itemId,
//                cellX = cellX,
//                cellY = cellY,
//                rotation = rotation
//            };

//            OnGardenChanged?.Invoke();
//            isSelected = false;
//            return true;
//        }

//        public bool TryRemove(int cellX, int cellY)
//        {
//            var key = new Vector2Int(cellX, cellY);
//            if (!gardenPlaced.Remove(key))
//                return false;

//            OnGardenChanged?.Invoke();
//            return true;
//        }

//        public bool IsOccupied(int cellX, int cellY)
//        {
//            return gardenPlaced.ContainsKey(new Vector2Int(cellX, cellY));
//        }

//        public bool TryGetPlaced(int cellX, int cellY, out UserPlacedItem placed)
//        {
//            return gardenPlaced.TryGetValue(new Vector2Int(cellX, cellY), out placed);
//        }

//        //        #region SAVE / LOAD Core Method
//        //        private SaveLoadDataWrapper<T> LoadData<T>() where T : RootDataDTO
//        //        {
//        //#if UNITY_EDITOR
//        //            string path = $"Assets/PROJECT TST/Anothers/Editor Saved Data/Json/{typeof(T).Name}.json";
//        //#else
//        //            string path = $"{Application.persistentDataPath}/{typeof(T).Name}.json";
//        //#endif
//        //            if (FileManager.ReadFileData(path, out string loadedEditorData))
//        //            {
//        //                // JSON ������ȭ
//        //                var wrapper = JsonConvert.DeserializeObject<SaveLoadDataWrapper<T>>(loadedEditorData);

//        //                return wrapper;
//        //            }

//        //            Debug.Log($"Failed to Load Data {typeof(T).Name}");
//        //            return null;
//        //        }

//        ////        private void SaveData<T>(Dictionary<int, T> newData) where T : RootDataDTO
//        ////        {
//        ////#if UNITY_EDITOR
//        ////            string jsonPath = $"Assets/PROJECT TST/Anothers/Editor Saved Data/Json/{typeof(T).Name}.json";
//        ////            string csvPath = $"Assets/PROJECT TST/Anothers/Editor Saved Data/Csv/{typeof(T).Name}.csv";
//        ////#else
//        ////            string jsonPath = $"{Application.persistentDataPath}/{typeof(T).Name}.json";
//        ////            string csvPath = $"{Application.persistentDataPath}/{typeof(T).Name}.csv";
//        ////#endif
//        ////            // JSON ����
//        ////            SaveLoadDataWrapper<T> wrapper = new SaveLoadDataWrapper<T>();
//        ////            foreach (var dic in newData)
//        ////            {
//        ////                wrapper.Values.Add(newData[dic.Key]);
//        ////            }

//        ////            var settings = new JsonSerializerSettings
//        ////            {
//        ////                ContractResolver = new ParentFirstContractResolver(),
//        ////                Converters = new List<JsonConverter>
//        ////                {
//        ////                    new Vector3Converter(),
//        ////                    new QuaternionConverter()
//        ////                },
//        ////                Formatting = Formatting.Indented
//        ////            };

//        ////            if (wrapper == null || !wrapper.Values.Any())
//        ////                return;

//        ////            var jsonData = JsonConvert.SerializeObject(wrapper, settings);
//        ////            FileManager.WriteFileFromString(jsonPath, jsonData);
//        ////            Debug.Log($"Save Data to JSON Success: {jsonData}");

//        ////            // CSV ����
//        ////            if (SaveToCsv(wrapper.Values, csvPath))
//        ////                Debug.Log($"Save Data to CSV Success: {csvPath}");
//        ////        }

//        ////        private static bool SaveToCsv<T>(IEnumerable<T> dataCollection, string filePath) where T : RootDataDTO
//        ////        {
//        ////            if (dataCollection == null || !dataCollection.Any())
//        ////            {
//        ////                Debug.LogError("Data collection is null or empty.");
//        ////                return false;
//        ////            }

//        ////            var csvBuilder = new StringBuilder();

//        ////            // �θ� Ŭ�������� ���������� �Ӽ� ����
//        ////            var properties = GetPropertiesInHierarchy(typeof(T));

//        ////            // ��� ����
//        ////            csvBuilder.AppendLine(string.Join(",", properties.Select(p => p.Name)));

//        ////            // ������ �߰�
//        ////            foreach (var data in dataCollection)
//        ////            {
//        ////                var values = properties.Select(p =>
//        ////                {
//        ////                    var value = p.GetValue(data);

//        ////                    if (value == null)
//        ////                        return ""; // Null ���� �� ���ڿ��� ó��

//        ////                    if (value is IEnumerable enumerable && !(value is string))
//        ////                    {
//        ////                        return $"\"{string.Join("&", enumerable.Cast<object>())}\""; // ����Ʈ�� '&'�� ����
//        ////                    }
//        ////                    else if (value is Vector3 vector)
//        ////                    {
//        ////                        return $"\"({vector.x},{vector.y},{vector.z})\""; // Vector3 ����
//        ////                    }
//        ////                    else if (value is Quaternion quaternion)
//        ////                    {
//        ////                        return $"\"({quaternion.x},{quaternion.y},{quaternion.z},{quaternion.w})\""; // Quaternion ����
//        ////                    }
//        ////                    else
//        ////                    {
//        ////                        return value?.ToString()?.Replace(",", " ").Replace("\"", "\"\""); // ��ǥ�� ū����ǥ �̽�������
//        ////                    }
//        ////                });

//        ////                csvBuilder.AppendLine(string.Join(",", values));
//        ////            }

//        ////            // ���� ����
//        ////            try
//        ////            {
//        ////                FileManager.WriteFileFromString(filePath, csvBuilder.ToString());
//        ////            }
//        ////            catch (IOException ex)
//        ////            {
//        ////                Debug.LogError($"File is locked or cannot be accessed: {filePath}. Error: {ex.Message}");
//        ////                return false;
//        ////            }

//        ////            return true;
//        ////        }

//        ////        // �θ� Ŭ�������� �Ӽ� ����
//        ////        private static List<FieldInfo> GetPropertiesInHierarchy(Type type)
//        ////        {
//        ////            var properties = new List<FieldInfo>();
//        ////            var types = new List<Type>();

//        ////            while (type != null && type != typeof(object))
//        ////            {
//        ////                types.Add(type);
//        ////                type = type.BaseType; // �θ� Ŭ������ �̵�
//        ////            }

//        ////            // �θ���� ������
//        ////            types.Reverse();

//        ////            for (int i = 0; i < types.Count; i++)
//        ////            {
//        ////                properties.AddRange(types[i].GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
//        ////            }

//        ////            return properties;
//        ////        }

//        //        #endregion

//        //#region Serialize & Deserialize & FindParentProperty
//        //public class ParentFirstContractResolver : DefaultContractResolver
//        //{
//        //    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
//        //    {
//        //        var properties = base.CreateProperties(type, memberSerialization);

//        //        // �θ� Ŭ������ �Ӽ��� ���� ����
//        //        return properties
//        //            .OrderBy(p => GetInheritanceDepth(p.DeclaringType)) // ��� ���̿� ���� ����
//        //            .ThenBy(p => p.Order ?? int.MaxValue) // JsonProperty(Order) �Ӽ� ����
//        //            .ToList();
//        //    }

//        //    private int GetInheritanceDepth(Type type)
//        //    {
//        //        int depth = 0;
//        //        while (type.BaseType != null)
//        //        {
//        //            depth++;
//        //            type = type.BaseType;
//        //        }
//        //        return depth;
//        //    }
//        //}

//        //public class Vector3Converter : JsonConverter
//        //{
//        //    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
//        //    {
//        //        var vector = (Vector3)value;
//        //        writer.WriteStartObject();
//        //        writer.WritePropertyName("x");
//        //        writer.WriteValue(vector.x);
//        //        writer.WritePropertyName("y");
//        //        writer.WriteValue(vector.y);
//        //        writer.WritePropertyName("z");
//        //        writer.WriteValue(vector.z);
//        //        writer.WriteEndObject();
//        //    }

//        //    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
//        //    {
//        //        var obj = JObject.Load(reader);
//        //        return new Vector3(
//        //            (float)obj["x"],
//        //            (float)obj["y"],
//        //            (float)obj["z"]
//        //        );
//        //    }

//        //    public override bool CanConvert(Type objectType)
//        //    {
//        //        return objectType == typeof(Vector3);
//        //    }
//        //}

//        //public class QuaternionConverter : JsonConverter
//        //{
//        //    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
//        //    {
//        //        var quaternion = (Quaternion)value;
//        //        writer.WriteStartObject();
//        //        writer.WritePropertyName("x");
//        //        writer.WriteValue(quaternion.x);
//        //        writer.WritePropertyName("y");
//        //        writer.WriteValue(quaternion.y);
//        //        writer.WritePropertyName("z");
//        //        writer.WriteValue(quaternion.z);
//        //        writer.WritePropertyName("w");
//        //        writer.WriteValue(quaternion.w);
//        //        writer.WriteEndObject();
//        //    }

//        //    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
//        //    {
//        //        var obj = JObject.Load(reader);
//        //        return new Quaternion(
//        //            (float)obj["x"],
//        //            (float)obj["y"],
//        //            (float)obj["z"],
//        //            (float)obj["w"]
//        //        );
//        //    }

//        //    public override bool CanConvert(Type objectType)
//        //    {
//        //        return objectType == typeof(Quaternion);
//        //    }
//        //}
//        //#endregion
//    }
//}
