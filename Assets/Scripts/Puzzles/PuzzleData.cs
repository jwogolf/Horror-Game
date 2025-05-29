using System.Collections.Generic;
using System.IO;
using System.Text;

[System.Serializable]
public class PuzzleData
{
    public string id;
    public List<PuzzleRequirement> requirements;
    public bool isCompleted = false;

    public string ToSaveString()
    {
        StringBuilder saveData = new StringBuilder();

        saveData.AppendLine($"Puzzle ID:{id}");
        saveData.AppendLine($"Is Completed:{isCompleted}");

        saveData.AppendLine($"Requirements:");
        foreach (var req in requirements)
        {
            saveData.AppendLine($"Puzzle Requirement:\n{req.ToSaveString()}");
        }

        return saveData.ToString();
    }
}
