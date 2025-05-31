using UnityEngine;
using System;
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

    public PuzzleRequirement(
    PuzzleRequirementType? t = null,
    Vector3? location = null,
    float r = 20f,
    string reqItem = null,
    MultiMeter.ScanMode? mode = null,
    float? lowT = null,
    float? highT = null,
    bool oneTime = false,
    bool satisfied = false)
    {
        this.type = t ?? default;
        this.targetLocation = location ?? Vector3.zero;
        this.requiredItemId = reqItem;
        this.requiredScanMode = mode ?? default;
        this.lowThresh = lowT ?? float.MinValue;
        this.highThresh = highT ?? float.MaxValue;
        this.oneTimeRequirement = oneTime;
        this.requirementSatisfied = satisfied;
    }
}
