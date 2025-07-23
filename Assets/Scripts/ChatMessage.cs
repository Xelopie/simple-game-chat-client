using UnityEngine;
using UnityEngine.UI;

namespace SimpleGameChat
{
    public class ChatMessage : MonoBehaviour
    {
        [SerializeField] private Text _text;

        public void Set(string message, Color color)
        {
            _text.color = color;
            _text.text = message;
        }
    }
}