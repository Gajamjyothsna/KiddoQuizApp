using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;
using UnityEngine.Networking;

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

        public List<Sprite> quizImageAnswers;
    }
    #endregion

    #region Priavte Variables
    [Header("Quiz Data Model Debugging")]
    public QuizDataManager.QuizData quizData;

    [Header("QuizdataManager Reference")]
    [SerializeField] private QuizDataManager quizDataManager;

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
    [SerializeField] private Text _questionTMP;
    [SerializeField] private Button[] _options;

    [Header("Score UI Component")]
    [SerializeField] private Text _scoreTMP;
    [SerializeField] private TextMeshProUGUI _pointsTMP;

    [Header("Question Number UI Component")]
    [SerializeField] private Text _questionNumberTMP;

    [Header("Star Images")]
    [SerializeField] private Image[] starImages;
    [SerializeField] private Color starFadedColor;

    [Header("Option ColorCodes")]
    [SerializeField] private Color CorrectAnswerColor;
    [SerializeField] private Color WrongAnswerColor;

    [SerializeField] private Animator _correctAnswerAnimator;

    [SerializeField] private TextMeshProUGUI _continentMainNameTMP;
    [SerializeField] private TextMeshProUGUI _continentQuestionPanelTMP;


    [Header("GoodJob PopUp UI")]
    [SerializeField] private GameObject _goodJobPopUp;
    [SerializeField] private TextMeshProUGUI _GoodJobContinentTMP;
    [SerializeField] private Text _GoodJobRankTMP;
    [SerializeField] private Image[] _goodJobStarImages;

    [Header("QuestionAndAnswerPanel")]
    [SerializeField] private GameObject _questionAndAnswerPanel;

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

    [Header("Question Progression Bar Images")]
    [SerializeField] private Sprite[] _progressionBarSprites;
    [SerializeField] private Image _progressionBarImage;

    [Header("Congraulations PopUP UI components")]
    [SerializeField] private Text continentText;
    [SerializeField] private Text _starsCount;

    [Header("Played Contest PopUp UI")]
    [SerializeField] private GameObject _contestPlayedPopUp;

    private List<QuizDataManager.Question> _currentCategoryQuestions;
    private int _currentQuestionIndex = 0;
    private int _correctAnswersCount = 0; // Counter for correct answers
    private int userScore = 0;
    private int currentAttempt = 0; // Track the number of attempts for the current question
    private int userPoints = 0;

    private string _continentName;
    private string _category;

    #endregion

    #region Init methods
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
        this._category = _category;

    }

    private void SetBGData(string _continent)
    {
        int index = _bgDataModel.FindIndex(x=>x._continentType.ToString() == _continent);

        for(int i=0;i<_panelBGs.Count;i++)
        {
            _panelBGs[i].sprite = _bgDataModel[index]._continentBG;
        }

       _progressionBarImage.sprite = _progressionBarSprites[_correctAnswersCount];
    }

    #endregion

    
    private void DisplayQuestionsByCategory(string category)
    {
        _quizTopicSelectionPanel.SetActive(false);
        _quizSelectionPanel.SetActive(true);
        _questionPanel.SetActive(true);


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
        _correctAnswerAnimator.SetTrigger("newQuestion");

        if (_currentQuestionIndex < _currentCategoryQuestions.Count)
        {
            var question = _currentCategoryQuestions[_currentQuestionIndex];
            _questionTMP.text = question.question;

            _questionNumberTMP.text = "QUESTION " + (_currentQuestionIndex + 1);

            // Reset attempts for the new question
            currentAttempt = 0;

            // Set the progression bar sprite based on the current question index
            if (_currentQuestionIndex < _progressionBarSprites.Length)
            {
                _progressionBarImage.sprite = _progressionBarSprites[_currentQuestionIndex];
            }

            ResetStars();
            ResetOptionsColors();

            for (int i = 0; i < _options.Length; i++)
            {
                if (i < question.options.Count)
                {
                    _options[i].gameObject.SetActive(true);
                    _options[i].GetComponentInChildren<Text>().text = question.options[i];
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
        }
    }

    #region ButtonActions Methods
    private void OnOptionSelected(int selectedOptionIndex)
    {
        var currentQuestion = _currentCategoryQuestions[_currentQuestionIndex];

        currentAttempt++; // Increment the attempt count

        if (currentQuestion.options[selectedOptionIndex] == currentQuestion.answer)
        {
            Debug.Log("Correct answer!");

            LoadImageFromData(_currentQuestionIndex);

            ResetOptionAnimation();

            _options[selectedOptionIndex].GetComponent<Image>().color = CorrectAnswerColor;
            _options[selectedOptionIndex].GetComponent<Image>().color = new Color(CorrectAnswerColor.r, CorrectAnswerColor.g, CorrectAnswerColor.b, 1);

            foreach(var option in _options)
            {
                option.interactable = false;
            }

            // Calculate points based on the number of attempts
            int pointsAwarded = Mathf.Max(4 - (currentAttempt - 1), 1); // Ensure at least 1 point is awarded

            _correctAnswerAnimator.SetTrigger("correct");

            userPoints += pointsAwarded;
            //  _pointsTMP.text = userPoints.ToString();

            _scoreTMP.text = userPoints.ToString();
           

            // Increment correct answer count
            _correctAnswersCount++;

            // Mark the question as answered
            currentQuestion.isAnswered = true;

            // Move to the next question
            _currentQuestionIndex++;

            if (_currentQuestionIndex <= _currentCategoryQuestions.Count)
            {
                StartCoroutine(DelayTheNextQuestion());

                IEnumerator DelayTheNextQuestion()
                {
                    yield return new WaitForSeconds(3f);
                    _GoodJobContinentTMP.text = _continentName;
                    _GoodJobRankTMP.text = "YOU HAVE WON " + " + " + pointsAwarded.ToString() + " " + "STARS";
                    if(pointsAwarded < 4)
                    {
                        int starCount = _goodJobStarImages.Length - pointsAwarded;
                        Debug.LogError("StarCount" + starCount);
                        for(int i = 0; i < starCount; i++)
                        {
                            _goodJobStarImages[i].color = starFadedColor;
                        }
                    }
                    _correctAnswerAnimator.SetBool("goodJob", true);
                    _goodJobPopUp.SetActive(true);
                    _questionAndAnswerPanel.SetActive(false);
                    yield return new WaitForSeconds(5f);
                    _correctAnswerAnimator.SetBool("goodJob", false);
                    _goodJobPopUp.SetActive(false);
                    _questionAndAnswerPanel.SetActive(true);

                    if(_currentQuestionIndex == 10)
                    {
                        ShowCongratulationsPopup();
                    }
                    else if(_currentQuestionIndex < 10) DisplayCurrentQuestion();
                }
            }
            else
            {
                Debug.Log("Quiz completed!");
                ShowCongratulationsPopup();
            }
        }
        else
        {
            Debug.Log("Incorrect answer. Try again or show a hint.");

            if (currentAttempt <= starImages.Length)
            {
                starImages[currentAttempt - 1].color = starFadedColor; // Change the color of the star to white
            }

            _options[selectedOptionIndex].GetComponent<Image>().color = WrongAnswerColor;
            _options[selectedOptionIndex].GetComponent<Image>().color = new Color(WrongAnswerColor.r, WrongAnswerColor.g, WrongAnswerColor.b, 1);
            _options[selectedOptionIndex].interactable = false;

            _options[selectedOptionIndex].GetComponent<Animator>().SetBool("wrongOption", true);
        }
    }

    public void OnCategoryButtonClick(string category)
    {
        if(quizDataManager.quizDataScriptableObject.continentIndividualScoreDatas.Find(x=>x.continentType.ToString() == _continentName).categoryIndividualDatas.Find(x=>x.category.ToString() == category).isPlayed == false)
        {
            DisplayQuestionsByCategory(category);

            SetQuizUI(category);
        }
        else
        {
            _contestPlayedPopUp.SetActive(true);
            _quizTopicSelectionPanel.SetActive(false);
        }
        
    }

    public void CloseContestPopUP()
    {
        _contestPlayedPopUp.SetActive(false);
        _quizTopicSelectionPanel.SetActive(true);
    }

    #endregion

    private void ShowCongratulationsPopup()
    {
        continentText.text = _category.ToUpper() + " OF" + " " + _continentName.ToUpper();
        _starsCount.text = userPoints.ToString();
        _congratulationsPopup.SetActive(true); // Show the popup
        _questionsPanel.SetActive(false);
        quizDataManager.SetScoreDataInScriptableObject(_continentName, _category,userPoints);
        

        StartCoroutine(OpenTheHomePage());

        IEnumerator OpenTheHomePage()
        {
            yield return new WaitForSeconds(2f);
            _congratulationsPopup.SetActive(false);
            _quizTopicSelectionPanel.SetActive(true);
        }

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

    #region Reset Methods

    private void ResetOptionAnimation()
    {
        foreach(var option in _options)
        {
            option.GetComponent<Animator>().SetBool("wrongOption", false);
        }
    }

    private void ResetStars()
    {
        foreach (var star in starImages)
        {
            star.color = Color.white; // Reset all stars to yellow
        }

        foreach(var starImage in _goodJobStarImages)
        {
            starImage.color = Color.white;
        }

    }

    private void ResetOptionsColors()
    {
        foreach(var option in _options)
        {
            option.GetComponent<Image>().color = new Color(255, 255, 255, 1);
            option.interactable = true;
            option.GetComponent<Animator>().SetBool("wrongOption", false);
        }

    }

    #endregion

    private void LoadImageFromData(int index)
    {
      _answerImage.sprite =  _headerImageClasses.Find(x => x.continentType.ToString() == _continentName).continentHeaderImageData.Find(x=>x._continentCategory.ToString() == _category).quizImageAnswers[index];
    }

}
