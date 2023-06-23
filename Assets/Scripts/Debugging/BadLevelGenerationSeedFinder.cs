using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BadLevelGenerationSeedFinder : MonoBehaviour
{
    LevelGenerator generator;
    public int generatedLevels;
    bool inProgress;
    List<int> ungeneratedSeeds = new List<int>();
    public List<int> generatedSeeds = new List<int>();
    public List<int> failedSeeds = new List<int>();

    private void Start()
    {
        generator = GetComponent<LevelGenerator>();
    }
    void Update()
    {
        if(inProgress)
        {
            LevelData levelData = LevelDataGenerator.Initialize(ungeneratedSeeds[0]);
            DunGenes.Instance.gameData.CurrentLevel = levelData;
            try
            {
                generator.GenerateTemplates(levelData, new Vector2Int(20, 20), levelData.amountOfRoomsCap, levelData.amountOfSections);
                generatedSeeds.Add(ungeneratedSeeds[0]);
            }
            catch
            {
                failedSeeds.Add(ungeneratedSeeds[0]);
                DebugLog.ReportBrokenSeed(failedSeeds[failedSeeds.Count - 1], 0, "Generation");
            }
            ungeneratedSeeds.RemoveAt(0);
            generatedLevels++;
            if(ungeneratedSeeds.Count == 0)
            {
                inProgress = false;
            }
        }
    }
    public void TryGenerateLevel()
    {
        if (inProgress) { return; }
        generatedLevels = 0;
        inProgress = true;
        while (ungeneratedSeeds.Count < 100)
        {
            int newSeed = UnityEngine.Random.Range(0, int.MaxValue);
            if (ungeneratedSeeds.Any(i => i == newSeed)) { continue; }
            ungeneratedSeeds.Add(newSeed);
        }
    }
}
