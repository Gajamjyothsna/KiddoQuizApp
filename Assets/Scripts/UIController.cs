using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public QuizDataManager.QuizData quizData;

    [SerializeField] private GameObject _quizTopicSelectionPanel;
    [SerializeField] private GameObject _quizSelectionPanel;

    [SerializeField] private TextMeshProUGUI _questionTMP;
    [SerializeField] private Button[] _options;

    private List<QuizDataManager.Question> _currentCategoryQuestions;
    private int _currentQuestionIndex = 0;

    public void SetQuizData(QuizDataManager.QuizData data)
    {
        quizData = data;
    }

    public void OnCategoryButtonClick(string category)
    {
        DisplayQuestionsByCategory(category);
    }

    private void DisplayQuestionsByCategory(string category)
    {
        _quizTopicSelectionPanel.SetActive(false);
        _quizSelectionPanel.SetActive(true);

        if (quizData == null || quizData.questions == null)
        {
            Debug.LogError("QuizData is null or empty.");
            return;
        }

        _currentCategoryQuestions = quizData.questions.FindAll(q => q.category == category);
        _currentQuestionIndex = 0;

        if (_currentCategoryQuestions.Count > 0)
        {
            DisplayCurrentQuestion();
        }
        else
        {
            Debug.Log($"No questions found for category {category}");
        }
    }

    private void DisplayCurrentQuestion()
    {
        if (_currentQuestionIndex < _currentCategoryQuestions.Count)
        {
            var question = _currentCategoryQuestions[_currentQuestionIndex];
            _questionTMP.text = question.question;

            for (int i = 0; i < _options.Length; i++)
            {
                if (i < question.options.Count)
                {
                    _options[i].gameObject.SetActive(true);
                    _options[i].GetComponentInChildren<TextMeshProUGUI>().text = question.options[i];
                    _options[i].onClick.RemoveAllListeners();  // Clear previous listeners
                    int optionIndex = i; // Capture the index for the current option
                    _options[i].onClick.AddListener(() => OnOptionSelected(optionIndex));
                }
                else
                {
                    _options[i].gameObject.SetActive(false);
                }
            }
        }
        else
        {
            Debug.Log("Quiz completed!");
            // Optionally, display a completion message or reset the quiz.
        }
    }

    private void OnOptionSelected(int selectedOptionIndex)
    {
        var currentQuestion = _currentCategoryQuestions[_currentQuestionIndex];

        if (currentQuestion.options[selectedOptionIndex] == currentQuestion.answer)
        {
            Debug.Log("Correct answer!");

            // Move to the next question
            _currentQuestionIndex++;

            if (_currentQuestionIndex < _currentCategoryQuestions.Count)
            {
                DisplayCurrentQuestion();
            }
            else
            {
                Debug.Log("Quiz completed!");
                // Optionally, display a completion message or reset the quiz.
            }
        }
        else
        {
            Debug.Log("Incorrect answer. Try again or show a hint.");
            // Optionally, provide feedback for the wrong answer
        }
    }
}
