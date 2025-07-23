using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SimpleGameChat
{
    public class ChatBox : MonoBehaviour
    {
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private InputField _inputField;
        [SerializeField] private Transform _messagesRoot;
        [SerializeField] private ChatMessage _chatMessagePrefab;
        [SerializeField] private VerticalLayoutGroup _verticalLayoutGroup;

        private readonly List<ChatMessage> _messages = new();
        private float _moveDistance;

        public bool IsFocusingOnInputField => EventSystem.current.currentSelectedGameObject == _inputField.gameObject;

        public UnityEvent<string> OnSubmit;
        private Coroutine _scrollToBottomCoroutine;

        private void Awake()
        {
            var lineHeight = ((RectTransform)_chatMessagePrefab.transform).rect.height;
            var spacing = _verticalLayoutGroup.spacing;
            _moveDistance = lineHeight + spacing;

            _inputField.onSubmit.AddListener(OnInputFieldSubmit);
        }

        private void OnDestroy()
        {
            _inputField.onSubmit.RemoveListener(OnInputFieldSubmit);
        }

        public void AddMessage(string context)
        {
            AddMessage(context, Color.white);
        }

        public void AddMessage(string context, Color color)
        {
            if (Mathf.Approximately(_scrollRect.verticalNormalizedPosition, 0f))
            {
                DelayScrollToBottom();
            }

            var message = Instantiate(_chatMessagePrefab,  _messagesRoot);
            message.Set(context, color);
            _messages.Add(message);
        }

        public void ClearMessages()
        {
            foreach (var msg in _messages)
            {
                Destroy(msg.gameObject);
            }
            _messages.Clear();
        }

        public void FocusInputField()
        {
            EventSystem.current.SetSelectedGameObject(_inputField.gameObject);
        }

        public void UnfocusInputField()
        {
            if (EventSystem.current.currentSelectedGameObject == _inputField.gameObject)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        public void MoveUp()
        {
            var adjustableHeight = _scrollRect.content.rect.height - _scrollRect.viewport.rect.height;
            if (adjustableHeight == 0f) return;
            var normalized = _moveDistance / adjustableHeight;
            _scrollRect.verticalNormalizedPosition += normalized;
        }

        public void MoveDown()
        {
            var adjustableHeight = _scrollRect.content.rect.height - _scrollRect.viewport.rect.height;
            if (adjustableHeight == 0f) return;
            var normalized = _moveDistance / adjustableHeight;
            _scrollRect.verticalNormalizedPosition -= normalized;
        }

        private void OnInputFieldSubmit(string context)
        {
            OnSubmit.Invoke(context);
            _inputField.SetTextWithoutNotify(string.Empty);
        }

        private void DelayScrollToBottom()
        {
            if (_scrollToBottomCoroutine != null)
            {
                StopCoroutine(_scrollToBottomCoroutine);
            }
            _scrollToBottomCoroutine = StartCoroutine(DelayScrollToBottomRoutine());
        }

        private IEnumerator DelayScrollToBottomRoutine()
        {
            yield return null;
            _scrollRect.verticalNormalizedPosition = 0f;
            _scrollToBottomCoroutine = null;
        }
    }
}