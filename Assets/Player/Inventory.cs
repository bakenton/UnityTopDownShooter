using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    [Header("Inventory")]
    public List<string> keys = new List<string>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddKey(string keyName)
    {
        if (!keys.Contains(keyName))
        {
            keys.Add(keyName);
            Debug.Log($"[Inventory] Added key: {keyName}. Total keys: {keys.Count}");
        }
    }

    public bool HasKey(string keyName)
    {
        return keys.Contains(keyName);
    }

    public void RemoveKey(string keyName)
    {
        if (keys.Remove(keyName))
        {
            Debug.Log($"[Inventory] Removed key: {keyName}. Total keys: {keys.Count}");
        }
    }
}