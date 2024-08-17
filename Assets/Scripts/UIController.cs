using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class UIController : MonoBehaviour
{
    [Header("Quiz Data Model Debugging")]
    public QuizDataManager.QuizData quizData;

    [Header("UI References")]
    [SerializeField] private GameObject _quizTopicSelectionPanel;
    [SerializeField] private GameObject _quizSelectionPanel;
    [SerializeField] private GameObject _questionsPanel;

    [Header("Congratulations Popup")]
    [SerializeField] private GameObject _congratulationsPopup; // Popup GameObject for showing after 10 correct answers

    [Header("Wrong Answer Popup")]
    [SerializeField] private GameObject _wrongAnswerPopUp;
    [SerializeField] private TextMeshProUGUI _hintTMP;
    [SerializeField] private TextMeshProUGUI _wrongAnswerContentTMP;

    [Header("Questions Panel UI Components")]
    [SerializeField] private TextMeshProUGUI _questionTMP;
    [SerializeField] private Button[] _options;

    [Header("Score UI Component")]
    [SerializeField] private TextMeshProUGUI _scoreTMP;

    private List<QuizDataManager.Question> _currentCategoryQuestions;
    private int _currentQuestionIndex = 0;
    private int _correctAnswersCount = 0; // Counter for correct answers
    private int userScore = 0;

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

        // Filter questions by category and that haven't been answered
        _currentCategoryQuestions = quizData.questions
            .Where(q => q.category == category && !q.isAnswered)
            .OrderBy(q => Random.value) // Shuffle the questions randomly
            .Take(10) // Select only the first 10 questions
            .ToList();

        _currentQuestionIndex = 0;

        if (_currentCategoryQuestions.Count > 0)
        {
            DisplayCurrentQuestion();
        }
        else
        {
            Debug.Log($"No unanswered questions found for category {category}");
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

            userScore += 1;
            _scoreTMP.text = userScore.ToString();

            // Increment correct answer count
            _correctAnswersCount++;

            // Check if 10 correct answers have been given
            if (_correctAnswersCount >= 10)
            {
                ShowCongratulationsPopup();
                _correctAnswersCount = 0; // Reset the count if you want to allow showing the popup again after another 10 correct answers
            }

            // Mark the question as answered
            currentQuestion.isAnswered = true;

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
            _wrongAnswerPopUp.SetActive(true);
            if(string.IsNullOrEmpty(currentQuestion.hint)) 
            {
                _wrongAnswerContentTMP.text = "You have attempted the wrong answer. Please read the question twice.";
                _hintTMP.transform.parent.gameObject.SetActive(false);  
            }
            else
            {
                _hintTMP.text = "<b>Hint : </b>" + currentQuestion.hint;
            }
            // Optionally, provide feedback for the wrong answer
        }
    }

    private void ShowCongratulationsPopup()
    {
        _congratulationsPopup.SetActive(true); // Show the popup
        _questionsPanel.SetActive(false);
        // Optionally, you can add more logic here, such as pausing the quiz or giving rewards
    }

    public void OnClosePopup()
    {
        _congratulationsPopup.SetActive(false); // Hide the popup when the user closes it
        _questionsPanel.SetActive(true);
        _quizTopicSelectionPanel.SetActive(true);
        _quizSelectionPanel.SetActive(false);
    }

    public void CloseWrongAnswerPopUp()
    {
        _wrongAnswerPopUp.SetActive(false) ;
        _hintTMP.text = "";
    }
}
