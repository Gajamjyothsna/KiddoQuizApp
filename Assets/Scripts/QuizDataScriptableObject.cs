using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuizDataScriptableObject", menuName = "Quiz/ContinentData", order = 1)]
public class QuizDataScriptableObject : ScriptableObject
{
    public List<QuizDataManager.Question> questions;
    public List<ContinentIndividualScoreData> continentIndividualScoreDatas;
    public int TotalScore;
}

[System.Serializable]
public class ContinentIndividualScoreData
{
    public UIController.ContinentType continentType;
    public List<CategoryIndividualData> categoryIndividualDatas;
}

[System.Serializable]
public class CategoryIndividualData
{
    public UIController.Category category;
    public bool isPlayed;
    public int _categoryScore;
}
