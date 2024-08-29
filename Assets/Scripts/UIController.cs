using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;
using UnityEngine.Networking;
using static System.Net.WebRequestMethods;
using UnityEditor.Analytics;

public class UIController : MonoBehaviour
{

    #region BGs Data Model
    [System.Serializable]
    public class BGDataModel
    {
        public Sprite _continentBG;
        public ContinentType _continentType;
    }

    [System.Serializable]
    public enum ContinentType
    {
        Asia,
        Antarctica,
        Africa,
        Australia,
        Europe,
        NorthAmerica,
        SouthAmerica
    }
    [System.Serializable]
    public enum Category
    {
        TheLand,
        TheEconomy,
        ThePeople
    }

    [System.Serializable]
    public class HeaderImageClass
    {
        public ContinentType continentType;
        public List<ContinentCategoryHeaderImage> continentHeaderImageData;
    }
    [System.Serializable]
    public class ContinentCategoryHeaderImage
    {
        public Category _continentCategory;
        public Sprite _headerImage;
    }
    #endregion
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
    [SerializeField] private TextMeshProUGUI _pointsTMP;

    [Header("Question Number UI Component")]
    [SerializeField] private TextMeshProUGUI _questionNumberTMP;

    [Header("Star Images")]
    [SerializeField] private Image[] starImages;
    [SerializeField] private Color starOriginalColor;

    [Header("Option ColorCodes")]
    [SerializeField] private Color CorrectAnswerColor;
    [SerializeField] private Color WrongAnswerColor;

    [SerializeField] private Animator _correctAnswerAnimator;

    [SerializeField] private TextMeshProUGUI _continentMainNameTMP;
    [SerializeField] private TextMeshProUGUI _continentQuestionPanelTMP;


    [Header("GoodJob PopUp UI")]
    [SerializeField] private GameObject _goodJobPopUp;
    [SerializeField] private TextMeshProUGUI _GoodJobContinentTMP;
    [SerializeField] private TextMeshProUGUI _GoodJobRankTMP;

    [Header("Answer UI")]
    [SerializeField] private Image _answerImage;
    [SerializeField] private Image _goodJobPanelAnswerImage;

    [Header("Animation Elements")]
    [SerializeField] private GameObject _questionPanel;
    [SerializeField] private GameObject _optionPanel;

    [Header("BG Sprite List")]
    [SerializeField] private List<Image> _panelBGs;
    [SerializeField] private List<BGDataModel> _bgDataModel;

    [Header("Header Images List")]
    [SerializeField] private List<HeaderImageClass> _headerImageClasses;

    [Header("Header Image Reference")]
    [SerializeField] private Image _headerIM;

    private List<QuizDataManager.Question> _currentCategoryQuestions;
    private int _currentQuestionIndex = 0;
    private int _correctAnswersCount = 0; // Counter for correct answers
    private int userScore = 0;
    private int currentAttempt = 0; // Track the number of attempts for the current question
    private int userPoints = 0;

    private string _continentName;

    public void SetQuizData(QuizDataManager.QuizData data, string continent)
    {
        quizData = data;
        _continentName = continent; 
        _continentMainNameTMP.text = continent;
        _continentQuestionPanelTMP.text = continent;

        SetBGData(continent);

    }

    private void SetQuizUI(string _category)
    {
      _headerIM.sprite =  _headerImageClasses.Find(x => x.continentType.ToString() == _continentName).continentHeaderImageData.Find(x=>x._continentCategory.ToString() == _category)._headerImage;
    }

    private void SetBGData(string _continent)
    {
        int index = _bgDataModel.FindIndex(x=>x._continentType.ToString() == _continent);

        for(int i=0;i<_panelBGs.Count;i++)
        {
            _panelBGs[i].sprite = _bgDataModel[index]._continentBG;
        }
    }
    public void OnCategoryButtonClick(string category)
    {
        DisplayQuestionsByCategory(category);

        SetQuizUI(category);
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

            _questionNumberTMP.text = "Question " + (_currentQuestionIndex + 1);

            // Reset attempts for the new question
            currentAttempt = 0;
            ResetStars();
            ResetOptionsColors();
           // ResetPostion();

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

        currentAttempt++; // Increment the attempt count

        if (currentQuestion.options[selectedOptionIndex] == currentQuestion.answer)
        {
            string imageUrl = "https://cdn2.thecatapi.com/images/ebv.jpg";
            StartCoroutine(LoadImageFromURL(imageUrl, _answerImage));
            StartCoroutine(LoadImageFromURL(imageUrl, _goodJobPanelAnswerImage));
            Debug.Log("Correct answer!");

            _options[selectedOptionIndex].GetComponent<Image>().color = CorrectAnswerColor;
            _options[selectedOptionIndex].GetComponent<Image>().color = new Color(CorrectAnswerColor.r, CorrectAnswerColor.g, CorrectAnswerColor.b, 1);

            // Calculate points based on the number of attempts
            int pointsAwarded = Mathf.Max(4 - (currentAttempt - 1), 1); // Ensure at least 1 point is awarded

            userPoints += pointsAwarded;
            _pointsTMP.text = userPoints.ToString();

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
                StartCoroutine(DelayTheNextQuestion());

                IEnumerator DelayTheNextQuestion()
                {
                    _correctAnswerAnimator.SetBool("Correct", true);
                    yield return new WaitForSeconds(2f);
                    _GoodJobContinentTMP.text = _continentName;
                    _GoodJobRankTMP.text = " + " + pointsAwarded.ToString();
                    _goodJobPopUp.SetActive(true);
                    _correctAnswerAnimator.SetBool("Correct", false);
                    yield return new WaitForSeconds(2.5f);
                    _goodJobPopUp.SetActive(false);
                    DisplayCurrentQuestion();
                }
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

            if (currentAttempt <= starImages.Length)
            {
                starImages[currentAttempt - 1].color = Color.white; // Change the color of the star to white
            }
            _options[selectedOptionIndex].GetComponent<Image>().color = WrongAnswerColor;
            _options[selectedOptionIndex].GetComponent<Image>().color = new Color(WrongAnswerColor.r, WrongAnswerColor.g, WrongAnswerColor.b, 1);
            _wrongAnswerPopUp.SetActive(true);
          
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

    private void ResetStars()
    {
        foreach (var star in starImages)
        {
            star.color = Color.yellow; // Reset all stars to yellow
        }
    }

    private void ResetOptionsColors()
    {
        foreach(var option in _options)
        {
            option.GetComponent<Image>().color = new Color(255, 255, 255, 0);
        }

       
    }

   private void ResetPostion()
    {
        Debug.Log("ResetPostion");
       
    }

    IEnumerator LoadImageFromURL(string url, Image uiImage)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to load image: " + request.error);
        }
        else
        {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            uiImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
    }

}
