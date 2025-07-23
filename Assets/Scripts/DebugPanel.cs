using UnityEngine;
using UnityEngine.UI;

namespace SimpleGameChat
{
    public class DebugPanel : MonoBehaviour
    {
        [SerializeField] private Text _text;

        public void ShowPlaying()
        {
            _text.text = "Focusing on gameplay\n" +
                         "Press Enter to start typing";
        }

        public void ShowTyping()
        {
            _text.text = "Typing";
        }
    }
}