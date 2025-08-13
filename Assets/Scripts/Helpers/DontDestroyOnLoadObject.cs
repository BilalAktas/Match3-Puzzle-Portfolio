using UnityEngine;

namespace Portfolio.WordGame.Helpers
{
    public class DontDestroyOnLoadObject : MonoBehaviour
    {
        private void Start()
        {
            DontDestroyOnLoad(this);
        }
    }
}

