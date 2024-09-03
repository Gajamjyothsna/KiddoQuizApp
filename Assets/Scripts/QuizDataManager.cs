using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class QuizDataManager : MonoBehaviour
{
    [System.Serializable]
    public class Question
    {
        public string category;
        public string question;
        public List<string> options;
        public string answer;
        public bool isAnswered = false;  // New field to track if the question is answered
    }

    [System.Serializable]
    public class QuizData
    {
        public List<Question> questions;
    }

    [SerializeField] private UIController _uiController;
    [SerializeField] public QuizDataScriptableObject quizDataScriptableObject;
    [SerializeField] private List<string> jsonFileNames; // List of JSON file names (without extension)
    [SerializeField] private int selectedJsonIndex = 0; // Index to select the specific JSON file

    public string _continentName;

    private void OnValidate()
    {
        if (quizDataScriptableObject != null && jsonFileNames != null && jsonFileNames.Count > 0)
        {
            // Ensure the index is within range
            if (selectedJsonIndex >= 0 && selectedJsonIndex < jsonFileNames.Count)
            {
                DeserializeJsonToScriptableObject(jsonFileNames[selectedJsonIndex]);
                _continentName = jsonFileNames[selectedJsonIndex];
            }
            else
            {
                Debug.LogWarning("Selected JSON index is out of range.");
            }
        }
        else
        {
            Debug.LogWarning("QuizDataScriptableObject or jsonFileNames is not assigned or empty.");
        }
    }

    private void DeserializeJsonToScriptableObject(string fileName)
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(fileName);

        if (jsonFile != null)
        {
            try
            {
                string jsonData = jsonFile.text;

                QuizData quizData = JsonUtility.FromJson<QuizData>(jsonData);

                if (quizDataScriptableObject != null)
                {
                    quizDataScriptableObject.questions = quizData.questions;

                    // Save the ScriptableObject to disk so that the data is persisted
                    EditorUtility.SetDirty(quizDataScriptableObject);
                    AssetDatabase.SaveAssets();

                    Debug.Log($"Data has been successfully deserialized from {fileName} and stored in the ScriptableObject.");
                }
                else
                {
                    Debug.LogError("QuizDataScriptableObject reference is missing.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error during JSON deserialization: {e.Message}");
            }
        }
        else
        {
            Debug.LogError($"Could not find the JSON file with name: {fileName} in Resources.");
        }
    }

    void Start()
    {
        if (quizDataScriptableObject != null && _uiController != null)
        {
            // Pass the deserialized data from the ScriptableObject to the UIController
            _uiController.SetQuizData(new QuizData { questions = quizDataScriptableObject.questions }, _continentName);
        }
        else
        {
            Debug.LogError("QuizDataScriptableObject or UIController is not assigned.");
        }
    }

    public void SetScoreDataInScriptableObject(string _continentType, string _category, int totalScore)
    {
        int _ContinentInex = quizDataScriptableObject.continentIndividualScoreDatas.FindIndex(x => x.continentType.ToString() == _continentType);
        int _CategoryIndex = quizDataScriptableObject.continentIndividualScoreDatas[_ContinentInex].categoryIndividualDatas.FindIndex(x => x.category.ToString() == _category);
        quizDataScriptableObject.continentIndividualScoreDatas[_ContinentInex].categoryIndividualDatas[_CategoryIndex].isPlayed = true;
        quizDataScriptableObject.continentIndividualScoreDatas[_ContinentInex].categoryIndividualDatas[_CategoryIndex]._categoryScore = totalScore;
        quizDataScriptableObject.TotalScore+= totalScore;
    }
}
