using System;
using UnityEngine;

namespace TST
{
    public enum ParameterType
    {
        Fame,
        Sanity,
        Enlightenment,
        Madness
    }

    public class PlayerParameters : SingletonBase<PlayerParameters>
    {
        // ----------------------------------------------------------------
        //  Save data
        // ----------------------------------------------------------------
        [Serializable]
        public class ParametersSaveData
        {
            public float fame;
            public float sanity;
            public float enlightenment;
            public float madness;
            public double funds;
        }

        // ----------------------------------------------------------------
        //  Events
        // ----------------------------------------------------------------
        public event Action<ParameterType, float> OnParameterChanged;

        // ----------------------------------------------------------------
        //  Runtime state
        // ----------------------------------------------------------------
        private float _fame;
        private float _sanity;
        private float _enlightenment;
        private float _madness;
        private double _funds;

        private const float MinValue = 0f;
        private const float MaxValue = 100f;

        // ----------------------------------------------------------------
        //  Properties
        // ----------------------------------------------------------------
        public float Fame          => _fame;
        public float Sanity        => _sanity;
        public float Enlightenment => _enlightenment;
        public float Madness       => _madness;
        public double Funds        => _funds;

        // ----------------------------------------------------------------
        //  Modifier methods
        // ----------------------------------------------------------------
        public void AddFame(float amount)
        {
            _fame = Mathf.Clamp(_fame + amount, MinValue, MaxValue);
            OnParameterChanged?.Invoke(ParameterType.Fame, _fame);
        }

        public void AddSanity(float amount)
        {
            _sanity = Mathf.Clamp(_sanity + amount, MinValue, MaxValue);
            OnParameterChanged?.Invoke(ParameterType.Sanity, _sanity);
        }

        public void AddEnlightenment(float amount)
        {
            _enlightenment = Mathf.Clamp(_enlightenment + amount, MinValue, MaxValue);
            OnParameterChanged?.Invoke(ParameterType.Enlightenment, _enlightenment);
        }

        public void AddMadness(float amount)
        {
            _madness = Mathf.Clamp(_madness + amount, MinValue, MaxValue);
            OnParameterChanged?.Invoke(ParameterType.Madness, _madness);
        }

        public void AddFunds(double amount)
        {
            _funds = Math.Max(0.0, _funds + amount);
        }

        // Direct-set methods for save loading — fire the same events as Add variants
        public void SetFame(float value)
        {
            _fame = Mathf.Clamp(value, MinValue, MaxValue);
            OnParameterChanged?.Invoke(ParameterType.Fame, _fame);
        }

        public void SetSanity(float value)
        {
            _sanity = Mathf.Clamp(value, MinValue, MaxValue);
            OnParameterChanged?.Invoke(ParameterType.Sanity, _sanity);
        }

        public void SetEnlightenment(float value)
        {
            _enlightenment = Mathf.Clamp(value, MinValue, MaxValue);
            OnParameterChanged?.Invoke(ParameterType.Enlightenment, _enlightenment);
        }

        public void SetMadness(float value)
        {
            _madness = Mathf.Clamp(value, MinValue, MaxValue);
            OnParameterChanged?.Invoke(ParameterType.Madness, _madness);
        }

        public void SetFunds(double value)
        {
            _funds = Math.Max(0.0, value);
        }

        public void ApplySaveData(float fame, float sanity, float enlightenment, float madness, double funds)
        {
            SetFame(fame);
            SetSanity(sanity);
            SetEnlightenment(enlightenment);
            SetMadness(madness);
            SetFunds(funds);
        }

        // ----------------------------------------------------------------
        //  Persistence
        // ----------------------------------------------------------------
        public ParametersSaveData ToSaveData()
        {
            return new ParametersSaveData
            {
                fame          = _fame,
                sanity        = _sanity,
                enlightenment = _enlightenment,
                madness       = _madness,
                funds         = _funds
            };
        }

        public void FromSaveData(ParametersSaveData data)
        {
            if (data == null) return;

            _fame          = Mathf.Clamp(data.fame, MinValue, MaxValue);
            _sanity        = Mathf.Clamp(data.sanity, MinValue, MaxValue);
            _enlightenment = Mathf.Clamp(data.enlightenment, MinValue, MaxValue);
            _madness       = Mathf.Clamp(data.madness, MinValue, MaxValue);
            _funds         = Math.Max(0.0, data.funds);

            OnParameterChanged?.Invoke(ParameterType.Fame,          _fame);
            OnParameterChanged?.Invoke(ParameterType.Sanity,        _sanity);
            OnParameterChanged?.Invoke(ParameterType.Enlightenment, _enlightenment);
            OnParameterChanged?.Invoke(ParameterType.Madness,       _madness);
        }

        public void Save()
        {
            // TODO: FileManager.WriteFileFromString("save/parameters.json", JsonUtility.ToJson(ToSaveData()));
        }

        public void Load()
        {
            // TODO:
            // if (FileManager.ReadFileData("save/parameters.json", out string json))
            //     FromSaveData(JsonUtility.FromJson<ParametersSaveData>(json));
        }
    }
}
