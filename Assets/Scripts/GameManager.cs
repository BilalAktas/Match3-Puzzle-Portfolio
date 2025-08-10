using TMPro;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
   [SerializeField] private TextMeshProUGUI _scoreText;
   private float _score;
   
   /// <summary>
   /// Add points to score and update the UI text
   /// </summary>
   public void Score(float point)
   {
      _score += point;
      _scoreText.text = $"Score: {_score}";
   }
}
