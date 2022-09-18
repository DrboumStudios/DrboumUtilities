using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Assertions;

namespace GameProject
{
    public interface IReadOnlySerializableDictionary<TKey, TValue>
    {
        public bool ContainsKey(in TKey key);
        public bool TryGetValue(in TKey key, out TValue value);
        public IReadOnlyList<TKey> Keys { get; }
        public IReadOnlyList<TValue> Values { get; }
    }

    [Serializable]
    public class SerializableDictionary<TKey, TValue> : IReadOnlySerializableDictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField] private List<TKey> _keys = new();
        [SerializeField] private List<TValue> _values = new();
        private Dictionary<TKey, int> _indexBookKeeping = new();

        public IReadOnlyList<TKey> Keys => _keys;
        public IReadOnlyList<TValue> Values => _values;
        public int Count => Keys.Count;

        public void Add(TKey key, TValue value)
        {

            if ( !_indexBookKeeping.TryAdd(key, _keys.Count) )
            {
                return;
            }
            _keys.Add(key);
            _values.Add(value);
            Asserts();
        }

        public bool ContainsKey(in TKey key) => _indexBookKeeping.ContainsKey(key);

        public bool TryRemove(TKey key)
        {
            var remove = _indexBookKeeping.TryGetValue(key, out var index);
            if ( remove )
            {
                var lastKey = _keys[^1];
                _indexBookKeeping[lastKey] = index;
                _keys.RemoveAtSwapBack(index);
                _values.RemoveAtSwapBack(index);
                _indexBookKeeping.Remove(key);
            }
            Asserts();

            return remove;
        }

        private void Asserts()
        {
            Assert.AreEqual(_keys.Count, _values.Count);
            Assert.AreEqual(_keys.Count, _indexBookKeeping.Count);
        }

        public void Clear()
        {
            _keys.Clear();
            _values.Clear();
            _indexBookKeeping.Clear();
        }

        public bool TryGetValue(in TKey key, out TValue value)
        {
            var exist = _indexBookKeeping.TryGetValue(key, out var index);
            value = exist ? _values[index] : default;
            return exist;
        }

        public TValue this[TKey key] {
            get => _values[_indexBookKeeping[key]];
            set => _values[_indexBookKeeping[key]] = value;
        }

        public void OnBeforeSerialize()
        { }

        public void OnAfterDeserialize()
        {

            if ( _indexBookKeeping.Count != _keys.Count )
            {
                _indexBookKeeping.Clear();
                for ( var index = 0; index < _keys.Count; index++ )
                {
                    _indexBookKeeping.Add(_keys[index], index);
                }
                Asserts();
            }
        }
    }
}