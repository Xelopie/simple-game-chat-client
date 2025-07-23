using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SimpleGameChat
{
    public class LoginPanel : MonoBehaviour
    {
        [SerializeField] private InputField _inputField;
        [SerializeField] private GameObject _emptyNicknameError;

        public bool IsFocusingOnInputField => EventSystem.current.currentSelectedGameObject == _inputField.gameObject;
        public UnityEvent<string> OnSubmit;

        private void Awake()
        {
            _inputField.onSubmit.AddListener(OnInputFieldSubmit);
        }

        private void OnDestroy()
        {
            _inputField.onSubmit.RemoveListener(OnInputFieldSubmit);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void FocusInputField()
        {
            EventSystem.current.SetSelectedGameObject(_inputField.gameObject);
        }

        private void OnInputFieldSubmit(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                _emptyNicknameError.SetActive(true);
                _inputField.ActivateInputField();
                return;
            }
            OnSubmit.Invoke(input);
        }
    }
}