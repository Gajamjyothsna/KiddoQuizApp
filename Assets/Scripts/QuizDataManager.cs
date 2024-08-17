using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

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
    }

    [System.Serializable]
    public class QuizData
    {
        public List<Question> questions;
    }

    [SerializeField] private UIController _uiController;
    void Start()
    {
        // Path to your JSON file (e.g., from StreamingAssets folder)
        string filePath = Path.Combine(Application.streamingAssetsPath, "questions.json");

        Debug.Log("1");

        if (File.Exists(filePath))
        {
            Debug.Log("2");
            try
            {
                Debug.Log("3");

                // Read the JSON file into a string
                string jsonData = File.ReadAllText(filePath);

                Debug.Log("4");

                Debug.Log("JsonData" +jsonData);
                // Deserialize the JSON string to the QuizData object
                QuizData quizData = JsonUtility.FromJson<QuizData>(jsonData);

                Debug.Log("QUestion Length" + quizData.questions.Count);

                if(_uiController != null)
                {
                    _uiController.SetQuizData(quizData);
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
}

