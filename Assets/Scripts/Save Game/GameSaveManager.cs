using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;

public class GameSaveManager : MonoBehaviour
{
    public string saveFileName = "gamesave.txt";
    private string savePath;

    private void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, saveFileName);
    }

    public void SaveGame()
    {
        StringBuilder saveData = new StringBuilder();

        // 1. Player Stats
        saveData.AppendLine($"Player Stats:");
        FirstPersonController player = FindFirstObjectByType<FirstPersonController>();
        saveData.AppendLine($"Player Position:{vectorToString(player.transform.position)}");
        saveData.AppendLine($"Health:{player.getHealth()}");
        saveData.AppendLine($"Stamina:{player.getStamina()}");
        saveData.AppendLine($"Delirium:{player.getDelirium()}");
        saveData.AppendLine($"Confidence:{player.getConfidence()}");

        saveData.AppendLine($"--------------------");

        // 2. Inventory and Abilities
        saveData.AppendLine($"Inventory:");
        InventorySystem inventory = FindFirstObjectByType<InventorySystem>();
        saveData.AppendLine($"{inventory.ToSaveString()}");

        saveData.AppendLine($"--------------------");

        // 3. Time and Day
        saveData.AppendLine($"Time:");
        TimeManager timeManager = FindFirstObjectByType<TimeManager>();
        saveData.AppendLine($"Day:{timeManager.Days}");
        saveData.AppendLine($"Hour:{timeManager.Hours}");
        saveData.AppendLine($"Minute:{timeManager.Minutes}");

        saveData.AppendLine($"--------------------");

        // 4. Puzzle Manager
        saveData.AppendLine($"Puzzle Data:");
        PuzzleManager puzzleManager = FindFirstObjectByType<PuzzleManager>();
        saveData.AppendLine($"{puzzleManager.ToSaveString()}");

        saveData.AppendLine($"--------------------");

        // Write to file
        File.WriteAllText(savePath, saveData.ToString());
        Debug.Log($"Game saved to {savePath}");
    }

    private string vectorToString(Vector3 v)
    {
        return $"{v.x:F3},{v.y:F3},{v.z:F3}";
    }
}