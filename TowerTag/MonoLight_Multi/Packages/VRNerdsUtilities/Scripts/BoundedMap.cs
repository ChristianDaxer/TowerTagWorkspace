using System.Collections.Generic;

public class BoundedMap<TKey, TValue> {
    private Dictionary<TKey, TValue> _dictionary;
    private List<TKey> _removalList;
    private int _bounds;

    public TValue this[TKey key] {
        get { return _dictionary[key]; }
        set {
            if (!_dictionary.ContainsKey(key)) {
                Add(key, value);
            }
            else {
                _dictionary[key] = value;
            }
        }
    }

    public int Count {
        get { return _dictionary.Count; }
    }

    public BoundedMap(int bounds) {
        _bounds = bounds;
        _dictionary = new Dictionary<TKey, TValue>();
        _removalList = new List<TKey>(bounds);
    }

    public void Add(TKey key, TValue value) {
        _dictionary.Add(key, value);
        while (_dictionary.Count > _bounds && _removalList.Count > 0) {
            _dictionary.Remove(_removalList[0]);
            _removalList.RemoveAt(0);
        }

        _removalList.Add(key);
    }

    public void Clear() {
        _dictionary.Clear();
        _removalList.Clear();
    }

    public void Remove(TKey key) {
        _dictionary.Remove(key);
        _removalList.Remove(key);
    }

    public bool TryGetValue(TKey key, out TValue value) {
        return _dictionary.TryGetValue(key, out value);
    }

    public bool ContainsKey(TKey key) {
        return _dictionary.ContainsKey(key);
    }

    public bool ContainsValue(TValue value) {
        return _dictionary.ContainsValue(value);
    }
}