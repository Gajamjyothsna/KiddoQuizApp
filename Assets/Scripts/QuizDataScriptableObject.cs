using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuizDataScriptableObject", menuName = "Quiz/ContinentData", order = 1)]
public class QuizDataScriptableObject : ScriptableObject
{
    public List<QuizDataManager.Question> questions;
}
