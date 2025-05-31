using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

[System.Serializable]
public class PuzzleArc
{
    public string arcID;
    public string displayName;
    public List<string> puzzleIDs;

    public PuzzleArc(string id, string name, List<string> puzzles)
    {
        this.arcID = id;
        this.displayName = name;
        this.puzzleIDs = puzzles;
    }

    public bool IsComplete(PuzzleManager manager)
    {
        return puzzleIDs.All(manager.IsPuzzleComplete);
    }
}