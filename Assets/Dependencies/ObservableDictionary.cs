using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;


public class KeyValueChangedArgs : EventArgs
{
    public NotifyCollectionChangedAction Action;
    public string Key;
    public string Value;

    public KeyValueChangedArgs(NotifyCollectionChangedAction action, string key, string value) { Action = action; Key = key; Value = value; }
}

public delegate void KeyValueChangedEventHandler(KeyValueChangedArgs args);


//This is a mess right now but i suppose it shall do the job


//https://stackoverflow.com/a/14410196/14569631 
/// <summary>
///Based on ObservableDictionary - Cos and Maciej Klemarczyk
/// </summary>
public class ObservableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, INotifyCollectionChanged, INotifyPropertyChanged
{
    public ObservableDictionary() : base() { }
    public ObservableDictionary(int capacity) : base(capacity) { }
    public ObservableDictionary(IEqualityComparer<TKey> comparer) : base(comparer) { }
    public ObservableDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary) { }
    public ObservableDictionary(int capacity, IEqualityComparer<TKey> comparer) : base(capacity, comparer) { }
    public ObservableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) : base(dictionary, comparer) { }

    public event KeyValueChangedEventHandler KeyValueChanged;
    public event NotifyCollectionChangedEventHandler CollectionChanged;
    public event PropertyChangedEventHandler PropertyChanged;

    public new TValue this[TKey key]
    {
        get
        {
            return base[key];
        }
        set
        {
            TValue oldValue;
            bool exist = base.TryGetValue(key, out oldValue);
            var oldItem = new KeyValuePair<TKey, TValue>(key, oldValue);
            base[key] = value;
            var newItem = new KeyValuePair<TKey, TValue>(key, value);
            if (exist)
            {
                this.OnKeyValueChanged(new KeyValueChangedArgs(NotifyCollectionChangedAction.Replace, key.ToString(), value.ToString()));
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem, base.Keys.ToList().IndexOf(key)));
            }
            else
            {
                this.OnKeyValueChanged(new KeyValueChangedArgs(NotifyCollectionChangedAction.Add, key.ToString(), value.ToString()));
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItem, base.Keys.ToList().IndexOf(key)));
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
            }
        }
    }

    public new void Add(TKey key, TValue value)
    {
        if (!base.ContainsKey(key))
        {
            var item = new KeyValuePair<TKey, TValue>(key, value);
            base.Add(key, value);
            this.OnKeyValueChanged(new KeyValueChangedArgs(NotifyCollectionChangedAction.Add, key.ToString(), value.ToString()));
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, base.Keys.ToList().IndexOf(key)));
            this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
        }
    }

    public new bool Remove(TKey key)
    {
        TValue value;
        if (base.TryGetValue(key, out value))
        {
            var item = new KeyValuePair<TKey, TValue>(key, base[key]);
            bool result = base.Remove(key);
            //     this.OnKeyValueChanged(new KeyValueChangedArgs(NotifyCollectionChangedAction.Remove, key.ToString(), value.ToString()));
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, base.Keys.ToList().IndexOf(key)));
            this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
            return result;
        }
        return false;
    }

    public new void Clear()
    {
        base.Clear();
        this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
    }

    protected virtual void OnKeyValueChanged(KeyValueChangedArgs args)
    {
        if (KeyValueChanged != null)
            KeyValueChanged(args);
    }

    protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (this.CollectionChanged != null)
        {
            this.CollectionChanged(this, e);
        }
    }

    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (this.PropertyChanged != null)
        {
            this.PropertyChanged(this, e);
        }
    }
}
