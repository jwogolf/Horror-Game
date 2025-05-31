using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System;
using System.Linq;

public class GameSaveManager : MonoBehaviour
{
    [SerializeField] FirstPersonController player;
    [SerializeField] TimeManager timeManager;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] InventorySystem inventory;
    [SerializeField] PuzzleManager puzzleManager;


    private string saveFileName = "gamesave.json";
    private string savePath;

    private void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, saveFileName);
    }

    public void SaveGame()
    {
        GameSaveData data = new GameSaveData
        {
            player = new PlayerData
            {
                position = playerPrefab.transform.position,
                health = player.getHealth(),
                stamina = player.getStamina(),
                delirium = player.getDelirium(),
                confidence = player.getConfidence()
            },
            inventory = new InventoryData
            {
                items = inventory.getAllItems(),
                abilities = inventory.getAbilities(),
            },
            time = new TimeData
            {
                Day = timeManager.Days,
                Hour = timeManager.Hours,
                Minute = timeManager.Minutes,
            },
            puzzles = new PuzzleInfo
            {
                puzzles = puzzleManager.getAllPuzzles(),
                arcs = puzzleManager.getAllArcs(),
            }
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log($"Game saved to {savePath}");

    }

    public void LoadGame()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("Save file not found at " + savePath);
            return;
        }

        string json = File.ReadAllText(savePath);
        GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);

        if (data == null)
        {
            Debug.LogWarning("Failed to parse save data.");
            return;
        }

        // Restore player
        // disable characyer controller to move player
        CharacterController controller = playerPrefab.GetComponent<CharacterController>();
        if (controller != null) controller.enabled = false;
        playerPrefab.transform.position = data.player.position;
        if (controller != null) controller.enabled = true;

        player.setHealth(data.player.health);
        player.setStamina(data.player.stamina);
        player.setDelirium(data.player.delirium);
        player.setConfidence(data.player.confidence);

        // Restore inventory
        inventory.items = data.inventory.items;
        inventory.abilities = data.inventory.abilities;

        // Restore Time
        timeManager.suppressHourTransition = true;

        timeManager.Days = data.time.Day;
        timeManager.Hours = data.time.Hour;
        timeManager.Minutes = data.time.Minute;

        timeManager.updateSky(data.time.Hour, data.time.Minute);

        timeManager.suppressHourTransition = false;

        // Restore Puzzles
        puzzleManager.setPuzzles(data.puzzles.puzzles);
        puzzleManager.setArcs(data.puzzles.arcs);
    }
}
