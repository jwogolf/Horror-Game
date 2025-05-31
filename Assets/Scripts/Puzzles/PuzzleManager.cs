using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Text;

public class PuzzleManager : MonoBehaviour
{
    [SerializeField] private Notebook notebook;
    [SerializeField] private MultiMeter playerMultimeter;
    [SerializeField] private InventorySystem inventory;
    [SerializeField] private FirstPersonController player;

    public static PuzzleManager Instance { get; private set; }

    // these are public just so they can be seen in the inspector for testing
    // change to private before release
    [Header("Puzzles and Arcs")]
    public List<PuzzleData> allPuzzles = new List<PuzzleData>();
    public List<PuzzleArc> allArcs = new List<PuzzleArc>();

    private Dictionary<string, PuzzleData> puzzleLookup = new Dictionary<string, PuzzleData>();
    private Dictionary<string, PuzzleArc> arcLookup = new Dictionary<string, PuzzleArc>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        foreach (var puzzle in allPuzzles)
            puzzleLookup[puzzle.id] = puzzle;

        foreach (var arc in allArcs)
            arcLookup[arc.arcID] = arc;
    }

    // -- Puzzle State --

    public bool IsPuzzleComplete(string id)
    {
        return puzzleLookup.ContainsKey(id) && puzzleLookup[id].isCompleted;
    }

    public void MarkPuzzleComplete(string id)
    {
        if (!puzzleLookup.ContainsKey(id))
        {
            Debug.Log("MISSING PUZZLE, NO MATCHING ID");
            return;
        }

        var puzzle = puzzleLookup[id];
        if (puzzle.isCompleted) return;

        puzzle.isCompleted = true;

        notebook.AddEntry($"Puzzle {id} completed at {Time.time} seconds.");

        // Future: Trigger journal/log entry, dialogue unlock, arc check
    }

    // -- Arc State --

    public bool IsArcComplete(string arcID)
    {
        return arcLookup.ContainsKey(arcID) && arcLookup[arcID].IsComplete(this);
    }

    public PuzzleData GetPuzzle(string id)
    {
        return puzzleLookup.TryGetValue(id, out var puzzle) ? puzzle : null;
    }

    public PuzzleArc GetArc(string id)
    {
        return arcLookup.TryGetValue(id, out var arc) ? arc : null;
    }

    public void setPuzzles(List<PuzzleData> data)
    {
        allPuzzles = data;

        foreach (var puzzle in allPuzzles)
            puzzleLookup[puzzle.id] = puzzle;
    }

    public void setArcs(List<PuzzleArc> data)
    {
        allArcs = data;

        foreach (var arc in allArcs)
            arcLookup[arc.arcID] = arc;
    }

    // Optional: get list of puzzles/arcs ready to begin
    public List<PuzzleData> GetAvailablePuzzles()
    {
        return allPuzzles.Where(p => !p.isCompleted).ToList();
    }

    public List<PuzzleData> getAllPuzzles()
    {
        return allPuzzles;
    }

    public List<PuzzleArc> getAllArcs()
    {
        return allArcs;
    }

    // TEST

    bool EvaluateRequirement(PuzzleRequirement req)
    {
        if (req.oneTimeRequirement && req.requirementSatisfied) return true;

        switch (req.type)
        {
            case PuzzleRequirementType.Location:
                return Vector3.Distance(transform.position, req.targetLocation) <= req.radius;
            case PuzzleRequirementType.MultimeterMode:
                return playerMultimeter.GetScanMode() == req.requiredScanMode;
            case PuzzleRequirementType.HasItem:
                return inventory.GetQuantityByName(req.requiredItemId) > 0;
            case PuzzleRequirementType.Confidence:
                return player.getConfidence() > req.lowThresh && player.getConfidence() < req.highThresh;
            case PuzzleRequirementType.Delirium:
                return player.getDelirium() > req.lowThresh && player.getDelirium() < req.highThresh;
            default:
                return false;
        }
    }

    void Update()
    {
        foreach (var puzzle in allPuzzles)
        {
            if (puzzle.isCompleted) continue;

            bool allRequirementsMet = true;

            foreach (var req in puzzle.requirements)
            {
                if (EvaluateRequirement(req))
                {
                    req.requirementSatisfied = true;
                }
                else
                {
                    allRequirementsMet = false;
                    break;
                }
            }

            if (allRequirementsMet)
            {
                MarkPuzzleComplete(puzzle.id);
            }
        }
    }

    void Start()
    {
        //AddTestPuzzle();
    }


    void AddTestPuzzle()
    {
        string id = "Test_Puzzle_001";
        bool isCompleted = false;

        List<PuzzleRequirement> requirements = new List<PuzzleRequirement>();
        requirements.Add(new PuzzleRequirement(t: PuzzleRequirementType.Location, location: new Vector3(0f, 0f, 0f), oneTime: false));
        requirements.Add(new PuzzleRequirement(t: PuzzleRequirementType.MultimeterMode, mode: MultiMeter.ScanMode.Radiation));

        PuzzleData testPuzzle = new PuzzleData(id, requirements, isCompleted);

        allPuzzles.Add(testPuzzle);
        puzzleLookup[testPuzzle.id] = testPuzzle;
    }
}
