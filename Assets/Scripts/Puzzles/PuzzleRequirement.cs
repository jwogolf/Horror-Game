using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;

public enum PuzzleRequirementType
{
    Location,
    MultimeterMode,
    HasItem,
    Confidence,
    Delirium
    // Add more as needed
}

[System.Serializable]
public class PuzzleRequirement
{
    public PuzzleRequirementType type;
    public Vector3 targetLocation;  // for Location
    public float radius = 20f;

    public MultiMeter.ScanMode requiredScanMode;  // for MultimeterMode
    public string requiredItemId;  // for HasItem
    public float lowThresh;  // for Confidence or Delirium
    public float highThresh;  // for Confidence or Delirium

    public bool oneTimeRequirement = false;
    public bool requirementSatisfied = false;

    public string ToSaveString()
    {
        StringBuilder saveData = new StringBuilder();

        saveData.AppendLine($"Requirement Type:{type}");
        saveData.AppendLine($"Target Location:{vectorToString(targetLocation)}");
        saveData.AppendLine($"Radius:{radius}");
        saveData.AppendLine($"Required Scan Mode:{requiredScanMode}");
        saveData.AppendLine($"Required Item ID:{requiredItemId}");
        saveData.AppendLine($"Low Threshold:{lowThresh}");
        saveData.AppendLine($"High Threshold:{highThresh}");
        saveData.AppendLine($"One Time Requirement:{oneTimeRequirement}");
        saveData.AppendLine($"Requirement Satisfied:{requirementSatisfied}");

        return saveData.ToString();
    }

    private string vectorToString(Vector3 v)
    {
        return $"{v.x:F3},{v.y:F3},{v.z:F3}";
    }
}