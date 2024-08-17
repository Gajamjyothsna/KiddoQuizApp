using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class QuizDataManager : MonoBehaviour
{
    [System.Serializable]
    public class Question
    {
        public int index;
        public string category;
        public string question;
        public List<string> options;
        public string answer;
        public string hint;
        public bool isAnswered = false;  // New field to track if the question is answered
    }

    [System.Serializable]
    public class QuizData
    {
        public List<Question> questions;
    }

    [SerializeField] private UIController _uiController;
    [SerializeField] private QuizDataScriptableObject quizDataScriptableObject;

    private void OnValidate()
    {
        if (quizDataScriptableObject != null)
        {
            DeserializeJsonToScriptableObject();
        }
    }

    private void DeserializeJsonToScriptableObject()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "questions.json");

        if (File.Exists(filePath))
        {
            try
            {
                string jsonData = File.ReadAllText(filePath);

                QuizData quizData = JsonUtility.FromJson<QuizData>(jsonData);

                if (quizDataScriptableObject != null)
                {
                    quizDataScriptableObject.questions = quizData.questions;

                    // Save the ScriptableObject to disk so that the data is persisted
                    EditorUtility.SetDirty(quizDataScriptableObject);
                    AssetDatabase.SaveAssets();

                    Debug.Log("Data has been successfully deserialized and stored in the ScriptableObject.");
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
            Debug.LogError($"Could not find the JSON file at path: {filePath}");
        }
    }

    void Start()
    {
        if (quizDataScriptableObject != null && _uiController !=null)
        {
            // Pass the deserialized data from the ScriptableObject to the UIController
            _uiController.SetQuizData(new QuizData { questions = quizDataScriptableObject.questions });
        }
        else
        {
            Debug.LogError("QuizDataScriptableObject is not assigned.");
        }
    }
}
