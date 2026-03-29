using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;

namespace TST
{
    public enum EffectType
    {
        FLASHES_START,
        MuzzleFlash1,
        MuzzleFlash2,
        MuzzleFlash3,
        MuzzleFlash4,
        MuzzleFlash5,
        MuzzleFlash6,
        MuzzleFlash7,
        MuzzleFlash8,
        MuzzleFlash9,
        SmokeTrail,

        FLASHES_END,

        IMPACTS_START,
        Brick_Impact,
        Dirt_Impact,
        Metal_Impact,
        IMPACTS_END,

        BLOOD_START,
        Blood_Impact,
        Blood_Smash,
        BLOOD_END,

        BOMB_START,
        BombEffect,
        BOMB_END,
    }

    [System.Serializable]
    public class EffectData
    {
        public EffectType type;
        public GameObject prefab;
        public float duration;
    }

    public class EffectManager : SingletonBase<EffectManager>
    {
        public List<EffectData> effectContainer = new List<EffectData>();

        private const string EFFECT_PREFAB_PATH = "Effects/";

        public void Initialize()
        {
            for (int idx = (int)EffectType.FLASHES_START + 1; idx < (int)EffectType.FLASHES_END; idx++)
            {
                EffectType effectType = (EffectType)idx;
                string effectName = effectType.ToString();

                EffectData effectData = new EffectData();
                effectData.type = effectType;
                effectData.prefab = Resources.Load<GameObject>(EFFECT_PREFAB_PATH + "Flashes/" + effectName);
                effectData.duration = 3.0f;
                effectContainer.Add(effectData);
            }

            for (int idx = (int)EffectType.BLOOD_START + 1; idx < (int)EffectType.BLOOD_END; idx++)
            {
                EffectType effectType = (EffectType)idx;
                string effectName = effectType.ToString();

                EffectData effectData = new EffectData();
                effectData.type = effectType;
                effectData.prefab = Resources.Load<GameObject>(EFFECT_PREFAB_PATH + "Bloods/" + effectName);
                effectData.duration = 3.0f;
                effectContainer.Add(effectData);
            }

            for (int idx = (int)EffectType.IMPACTS_START + 1; idx < (int)EffectType.IMPACTS_END; idx++)
            {
                EffectType effectType = (EffectType)idx;
                string effectName = effectType.ToString();
                string[] arr = effectName.Split('_');
                string newEffectName = string.Join("",arr);

                EffectData effectData = new EffectData();
                effectData.type = effectType;
                effectData.prefab = Resources.Load<GameObject>(EFFECT_PREFAB_PATH + "Impacts/" + newEffectName);
                effectData.duration = 3.0f;
                effectContainer.Add(effectData);
            }


            for (int idx = (int)EffectType.BOMB_START + 1; idx < (int)EffectType.BOMB_END; idx++)
            {
                EffectType effectType = (EffectType)idx;
                string effectName = effectType.ToString();
                string[] arr = effectName.Split('_');
                string newEffectName = string.Join("", arr);

                EffectData effectData = new EffectData();
                effectData.type = effectType;
                effectData.prefab = Resources.Load<GameObject>(EFFECT_PREFAB_PATH + "Bomb/" + newEffectName);
                effectData.duration = 3.0f;
                effectContainer.Add(effectData);
            }
        }

        public GameObject SpawnEffect(EffectType type, Vector3 position, Vector3 rotation)
        {
            EffectData data = effectContainer.Find(x => x.type == type);

            if (data == null)
            {
                Debug.LogError("Effect not found");
                return null;
            }

            GameObject effect = Instantiate(data.prefab);
            effect.gameObject.SetActive(true);
            effect.transform.SetPositionAndRotation(position, Quaternion.Euler(rotation));

            Destroy(effect, data.duration);

            return effect;
        }

        public GameObject SpawnEffect(GameObject effectObject)
        {
            EffectData data = effectContainer.Find(x => x.prefab.name.Equals(effectObject.name));

            if (data == null)
            {
                Debug.LogError("Effect not found");
                return null;
            }

            GameObject effect = Instantiate(data.prefab);
            effect.gameObject.SetActive(true);

            Destroy(effect, data.duration);

            return effect;
        }

        public GameObject SpawnEffect(EffectType type)
        {
            EffectData data = effectContainer.Find(x => x.type == type);
            if (data == null)
            {
                Debug.LogError("Effect not found");
                return null;
            }
            
            GameObject effect = Instantiate(data.prefab);
            effect.gameObject.SetActive(true);

            Destroy(effect, data.duration);

            return effect;
        }
    }
}
