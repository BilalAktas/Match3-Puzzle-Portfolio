using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Portfolio.Match3.Core
{
    /// <summary>
    /// Enum representing the state of the game.
    /// </summary>
    public enum GameState
    {
        Idle,
        Play
    }
    
    /// <summary>
    /// Manages overall game state, score, timer, and end screen logic.
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        public GameState CurrentGameState;
        
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _bestScoreText;
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private int TotalTime;
        private float currentTime;
        private int _score;

        [SerializeField] private GameObject _gameEndParent;
        [SerializeField] private TextMeshProUGUI _gameEndBestScoreText;
        [SerializeField] private TextMeshProUGUI _gameEndScoreText;
        [SerializeField] private Button _gameEndReTryButton;
        
        
        /// <summary>
        /// Initializes the game, sets up timer and retry button.
        /// </summary>
        private void Start()
        {
            Application.targetFrameRate = 60;
            
            UpdateBestScore();
            currentTime = TotalTime;
            
            CurrentGameState = GameState.Play;
            
            _gameEndReTryButton.onClick.AddListener(ReTry);
        }
        
        /// <summary>
        /// Reloads the current scene to restart the game.
        /// </summary>
        private void ReTry() => SceneManager.LoadSceneAsync(0);

        private void Update()
        {
            if(currentTime>0)
                CountTimer();
        }
        
        /// <summary>
        /// Decreases the timer, updates the UI, and ends the game when time runs out.
        /// </summary>
        private void CountTimer()
        {
            currentTime -= Time.deltaTime;
            _timerText.text = ((int)currentTime).ToString();

            if (currentTime <= 0)
            {
                UpdateBestScore();
                CurrentGameState = GameState.Idle;
                _gameEndParent.SetActive(true);
            }
                
        }
        
        /// <summary>
        /// Checks and updates the best score using PlayerPrefs.
        /// </summary>
        private void UpdateBestScore()
        {
            var bestScore = PlayerPrefs.GetInt("BestScore", 0);
            if (_score>bestScore)
                PlayerPrefs.SetInt("BestScore", _score);

            _bestScoreText.text = PlayerPrefs.GetInt("BestScore", 0).ToString();
            _gameEndBestScoreText.text = $"Best Score: {PlayerPrefs.GetInt("BestScore", 0)}";
        }
        
        /// <summary>
        /// Add points to score and update the UI text
        /// </summary>
        public void Score(int point)
        {
            _score += point;
            _scoreText.text = _score.ToString();
            _gameEndScoreText.text = $"Current Score: {_score}";
            UpdateBestScore();
        }
    }
}