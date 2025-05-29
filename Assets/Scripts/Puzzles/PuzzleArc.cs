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

    public bool IsComplete(PuzzleManager manager)
    {
        return puzzleIDs.All(manager.IsPuzzleComplete);
    }

    public string ToSaveString()
    {
        StringBuilder saveData = new StringBuilder();

        saveData.AppendLine($"Arc ID:{arcID}");
        saveData.AppendLine($"Arc Name:{displayName}");

        saveData.AppendLine($"Puzzles:");
        foreach (var id in puzzleIDs)
        {
            saveData.AppendLine($"Puzzle ID:{id}");
        }

        return saveData.ToString();
    }
}