using System;
using System.Collections.Generic;
using Home.UI;
using SOEventSystem.Shared;
using UnityEngine;
using UnityEngine.Events;

namespace UI
{
    /// <summary>
    /// The Message Queue allows to enqueue messages that can be displayed in some kind of popup or other display system.
    /// It allows adding messages that need confirmation or that can be confirmed and denied.
    /// </summary>
    /// <author>Ole Jürgensen (ole@vrnerds.de)</author>
    [CreateAssetMenu(fileName = "New Message Queue", menuName = "Queue/Message Queue")]
    public class MessageQueue : ScriptableObjectSingleton<MessageQueue>
    {
        private readonly List<Message> _messageQueue = new List<Message>();

        public event Action MessageAdded;

        /// <summary>
        /// Retrieves the next pending message from the queue. The message is then removed from the queue automatically.
        /// </summary>
        /// <returns>The next pending message</returns>
        public Message GetNextMessage()
        {
            if (!HasNext()) return null;

            Message nextMessage = _messageQueue[0];
            _messageQueue.Remove(nextMessage);
            return nextMessage;
        }

        /// <summary>
        /// Query whether there is any pending element in the message queue.
        /// </summary>
        /// <returns>True iff there is at least one pending message in the queue</returns>
        public bool HasNext()
        {
            return _messageQueue.Count > 0;
        }

        /// <summary>
        /// Add an error message to the queue. An error message must be checked off by the user to close.
        /// </summary>
        /// <param name="text">The main text of the message</param>
        /// <param name="header">The headline text of the message</param>
        /// <param name="onOpen">A callback that is to be invoked when the message is displayed</param>
        /// <param name="onClose">A callback that is to be invoked when the message is closed or skipped</param>
        /// <param name="buttonText">The text that is to be displayed on the button</param>
        public void AddErrorMessage(string text, string header = "Error",
            Action onOpen = null, Action onClose = null,
            string buttonText = "OK")
        {
            var messageButton = new MessageButton {Text = buttonText, Clicked = null};
            AddMessage(text, header, true, onOpen, onClose, new List<MessageButton> {messageButton});
        }

        /// <summary>
        /// Add a message to the queue that should only be closed when checked off by the user.
        /// Use onClose callback to react to user confirmation.
        /// </summary>
        /// <param name="text">The main text of the message</param>
        /// <param name="header">The headline text of the message</param>
        /// <param name="onOpen">A callback that is to be invoked when the message is displayed</param>
        /// <param name="onClose">A callback that is to be invoked when the message is closed or skipped</param>
        /// <param name="buttonText">The text to be displayed on the button</param>
        /// <param name="onConfirm">A callback that is to be invoked when the message is confirmed</param>
        public void AddConfirmMessage(string text, string header = "Info",
            Action onOpen = null, Action onClose = null,
            string buttonText = "OK", UnityAction onConfirm = null)
        {
            var messageButton = new MessageButton {Text = buttonText, Clicked = onConfirm};
            AddMessage(text, header, true, onOpen, onClose, new List<MessageButton> {messageButton});
        }

        /// <summary>
        /// Add a message to the queue that can be confirmed or denied by the user.
        /// </summary>
        /// <param name="text">The main text of the message</param>
        /// <param name="header">The headline text of the message</param>
        /// <param name="onOpen">A callback that is to be invoked when the message is displayed</param>
        /// <param name="onClose">A callback that is to be invoked when the message is closed or skipped</param>
        /// <param name="yesButtonText">The text to be displayed on the confirmation button</param>
        /// <param name="onYes">Action triggered on user confirmation</param>
        /// <param name="noButtonText">The text to be displayed on the denial button</param>
        /// <param name="onNo">Action triggered on user denial</param>
        public void AddYesNoMessage(string text, string header = "Info",
            Action onOpen = null, Action onClose = null,
            string yesButtonText = "YES", UnityAction onYes = null,
            string noButtonText = "NO", UnityAction onNo = null)
        {
            var yesButton = new MessageButton {Text = yesButtonText, Clicked = onYes};
            var noButton = new MessageButton {Text = noButtonText, Clicked = onNo};
            AddMessage(text, header, true, onOpen, onClose, new List<MessageButton> {yesButton, noButton});
        }


        /// <summary>
        /// Add a message to the queue that has an Inputfield and two Buttons
        /// </summary>
        /// <param name="text">The main text of the message</param>
        /// <param name="placeholderText">The displayed text when the inputfield is empty</param>
        /// <param name="startText">Text that is initially filled into the input field</param>
        /// <param name="header">The headline text of the message</param>
        /// <param name="type"></param>
        /// <param name="onOpen">A callback that is to be invoked when the message is displayed</param>
        /// <param name="onClose">A callback that is to be invoked when the message is closed or skipped</param>
        /// <param name="yesButtonText">The text to be displayed on the confirmation button</param>
        /// <param name="onYes">Action triggered on user confirmation</param>
        /// <param name="noButtonText">The text to be displayed on the denial button</param>
        /// <param name="onNo">Action triggered on user denial</param>
        /// <param name="inputValid">Validates the input for visualization</param>
        public void AddInputFieldMessage(string text, string placeholderText, string startText = "",
            string header = "Info",
            InputFieldHelper.InputFieldType type = InputFieldHelper.InputFieldType.PlayerName, Action onOpen = null,
            Action onClose = null,
            string yesButtonText = "YES", UnityAction<string> onYes = null,
            string noButtonText = "NO", UnityAction onNo = null, Predicate<string> inputValid = null)
        {
            var inputField = new MessageInputField
                {Placeholder = placeholderText, Text = startText, IsValid = inputValid, Type = type};
            var yesButton = new MessageButton {Text = yesButtonText, ClickedWithText = onYes};
            var noButton = new MessageButton {Text = noButtonText, Clicked = onNo};
            AddMessage(text, header, true, onOpen, onClose, new List<MessageButton> {yesButton, noButton},
                new List<MessageInputField> {inputField});
        }

        /// <summary>
        /// Add a message to the queue that should be closed automatically when a new message poops up.
        /// </summary>
        /// <param name="text">The main text of the message</param>
        /// <param name="header">The headline text of the message</param>
        /// <param name="onOpen">A callback that is to be invoked when the message is displayed</param>
        /// <param name="onClose">A callback that is to be invoked when the message is closed or skipped</param>
        /// <param name="messageButtons">Optional list of buttons displayed in the message</param>
        /// <param name="lifeTime">Time after which this message should be automatically closed</param>
        public void AddVolatileMessage(string text, string header = "Info",
            Action onOpen = null, Action onClose = null,
            List<MessageButton> messageButtons = null, float lifeTime = 0)
        {
            AddMessage(text, header, false, onOpen, onClose, messageButtons, null, lifeTime);
        }

        /// <summary>
        /// Add a message to the queue that should be closed automatically when a new message poops up.
        /// This message will have one confirmation button.
        /// </summary>
        /// <param name="text">The main text of the message</param>
        /// <param name="header">The headline text of the message</param>
        /// <param name="onOpen">A callback that is to be invoked when the message is displayed</param>
        /// <param name="onClose">A callback that is to be invoked when the message is closed or skipped</param>
        /// <param name="okButtonText">Text displayed on the confirmation button</param>
        /// <param name="onOkButton">Action triggered on user confirmation</param>
        /// <param name="lifeTime">Time after which this message should be automatically closed</param>
        public void AddVolatileButtonMessage(string text, string header = "Info",
            Action onOpen = null, Action onClose = null,
            string okButtonText = "OK", UnityAction onOkButton = null,
            float lifeTime = 0)
        {
            AddButtonMessage(text, header, false, onOpen, onClose, okButtonText, onOkButton, lifeTime);
        }

        public void AddButtonMessage(string text, string header = "Info", bool needsConfirmation = false,
            Action onOpen = null, Action onClose = null,
            string okButtonText = "OK", UnityAction onOkButton = null,
            float lifeTime = 0)
        {
            var messageButtons =
                new List<MessageButton> {new MessageButton {Clicked = onOkButton, Text = okButtonText}};
            AddMessage(text, header, needsConfirmation, onOpen, onClose, messageButtons, lifeTime: lifeTime);
        }

        private void AddMessage(string text, string header, bool needsConfirmation, Action onOpen, Action onClose,
            List<MessageButton> buttons, List<MessageInputField> inputFields = null, float lifeTime = 0)
        {
            _messageQueue.RemoveAll(msg => !msg.NeedsConfirmation);
            var message = new Message
            {
                Header = header,
                Text = text,
                NeedsConfirmation = needsConfirmation,
                Closed = onClose,
                Opened = onOpen,
                Buttons = buttons ?? new List<MessageButton>(),
                InputFields = inputFields,
                LifeTime = lifeTime
            };
            _messageQueue.Add(message);
            MessageAdded?.Invoke();
        }

        /// <summary>
        /// Remove a specific message from the queue.
        /// </summary>
        /// <param name="message">The message object to remove</param>
        public void RemoveMessage(Message message)
        {
            if (!_messageQueue.Contains(message)) return;

            _messageQueue.Remove(message);
        }
    }
}