using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class PuzzleData
{
    public string id;
    public List<PuzzleRequirement> requirements;
    public bool isCompleted = false;

    public PuzzleData(string name, List<PuzzleRequirement> reqs, bool complete)
    {
        this.id = name;
        this.requirements = reqs;
        this.isCompleted = complete;
    }
}
