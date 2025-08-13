using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScene : MonoBehaviour
{
   private void Start() => SceneManager.LoadSceneAsync(1);
}
