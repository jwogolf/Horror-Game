using System.Collections.Generic;
using UnityEngine;

public class Notebook : MonoBehaviour
{
    private List<string> entries = new List<string>();

    public void AddEntry(string entry)
    {
        entries.Add(entry);
        Debug.Log($"[Notebook Entry Added] {entry}");
    }

    public void PrintAllEntries()
    {
        Debug.Log("==== Notebook Entries ====");
        foreach (var entry in entries)
        {
            Debug.Log(entry);
        }
    }

    public bool HasEntry(string entry)
    {
        return entries.Contains(entry);
    }

    // IMPLEMENT AFTER NOTEBOOK ORGANIZATION AND MULTIPLE ENTRIES
    public string ToSaveString()
    {
        return null;
    }
}