using System;
using System.Collections;
using NativeWebSocket;
using UnityEngine;

namespace SimpleGameChat
{
    public class Demo : MonoBehaviour
    {
        private const string ChangeNicknameFormat = "/nickname {0}";
        private const string ClearMessagesCommand = "/clear";

        [SerializeField] private string _uri;
        [SerializeField] private ChatBox _chatBox;
        [SerializeField] private LoginPanel _loginPanel;
        [SerializeField] private DebugPanel _debugPanel;

        [Header("Connection Events Notifications")] [SerializeField]
        private string _connectingNotification;

        [SerializeField] private string _connectedNotification;
        [SerializeField] private string _disconnectedNotification;
        [SerializeField] private string _failedToConnectNotification;
        [SerializeField] private string _reconnectingNotification;

        private WebSocket _webSocket;
        private string _nickname;
        private bool _isConnectionOpened;
        private bool _hasEnteredChatScreen;
        private int _lastSubmitFrame;

        private void Awake()
        {
            Application.runInBackground = true;
            _chatBox.OnSubmit.AddListener(OnChatSubmit);
            _loginPanel.OnSubmit.AddListener(OnLoginPanelSubmit);
        }

        private void OnDestroy()
        {
            if (didStart)
            {
                Disconnect();
            }

            _chatBox.OnSubmit.RemoveListener(OnChatSubmit);
        }

        private void Update()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            _webSocket?.DispatchMessageQueue();
#endif
            if (!_hasEnteredChatScreen)
            {
                if (!_loginPanel.IsFocusingOnInputField)
                {
                    _loginPanel.FocusInputField();
                }
                return;
            }

            // Besides checking the key input and chat box focus status,
            // also compare the last submit frame to prevent immediate re-focusing onto the input field
            // when submitting the chat input by pressing the enter key
            if (!_chatBox.IsFocusingOnInputField && Input.GetKeyDown(KeyCode.Return)
                                                 && _lastSubmitFrame != Time.frameCount)
            {
                _chatBox.FocusInputField();
            }

            // Most of the games allow scrolling of chat history when typing
            if (_chatBox.IsFocusingOnInputField)
            {
                if (Input.GetKeyUp(KeyCode.UpArrow))
                {
                    _chatBox.MoveUp();
                }
                else if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    _chatBox.MoveDown();
                }
            }

            // Show whether the player is typing or focusing on gameplay (just for debug)
            if (_chatBox.IsFocusingOnInputField)
                _debugPanel.ShowTyping();
            else
                _debugPanel.ShowPlaying();
        }

        #region Network

        private async void Connect(bool isReconnect = false)
        {
            try
            {
                if (_webSocket != null)
                {
                    Disconnect();
                }

                _webSocket = new WebSocket(_uri);
                _webSocket.OnOpen += OnConnectionOpened;
                _webSocket.OnClose += OnConnectionClosed;
                _webSocket.OnMessage += OnReceiveMessage;
                _chatBox.AddMessage(!isReconnect ? _connectingNotification : _reconnectingNotification, Color.cyan);
                await _webSocket.Connect();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private async void Disconnect()
        {
            try
            {
                if (_webSocket == null)
                    return;

                _webSocket.OnOpen -= OnConnectionOpened;
                _webSocket.OnClose -= OnConnectionClosed;
                _webSocket.OnMessage -= OnReceiveMessage;

                if (_isConnectionOpened)
                    await _webSocket.Close();
                else
                    _webSocket.CancelConnection();

                _webSocket = null;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private async void Send(string context)
        {
            try
            {
                if (!_isConnectionOpened) return;
                if (string.IsNullOrEmpty(context)) return;
                await _webSocket.SendText(context);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        #endregion

        #region Callbacks

        private void OnLoginPanelSubmit(string nickname)
        {
            _nickname = nickname;
            _loginPanel.Hide();
            Connect();
            StartCoroutine(EnterChatScreenRoutine());
        }

        private void OnConnectionOpened()
        {
            _isConnectionOpened = true;
            _chatBox.AddMessage(_connectedNotification, Color.green);

            Send(string.Format(ChangeNicknameFormat, _nickname));
        }

        private void OnConnectionClosed(WebSocketCloseCode closeCode)
        {
            if (_isConnectionOpened)
            {
                _isConnectionOpened = false;
                _chatBox.AddMessage(_disconnectedNotification, Color.red);
            }
            else
            {
                _chatBox.AddMessage(_failedToConnectNotification, Color.red);
            }

            Connect(true);
        }

        private void OnReceiveMessage(byte[] data)
        {
            var message = System.Text.Encoding.UTF8.GetString(data);
            _chatBox.AddMessage(message);
        }

        private void OnChatSubmit(string context)
        {
            if (context == ClearMessagesCommand)
            {
                _chatBox.ClearMessages();
            }
            else
            {
                if (!string.IsNullOrEmpty(context))
                    Send(context);
            }

            _lastSubmitFrame = Time.frameCount;
            _chatBox.UnfocusInputField();
        }

        #endregion

        private IEnumerator EnterChatScreenRoutine()
        {
            yield return new WaitForEndOfFrame();
            _hasEnteredChatScreen = true;
        }
    }
}