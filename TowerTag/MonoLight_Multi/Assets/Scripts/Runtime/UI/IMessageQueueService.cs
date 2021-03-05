using System;
using System.Collections.Generic;
using Home.UI;
using UnityEngine.Events;

namespace UI {
    public class Message {
        public string Header { get; set; }
        public string Text { get; set; }
        public bool NeedsConfirmation { get; set; }
        public Action Opened { get; set; }
        public Action Closed { get; set; }
        public List<MessageButton> Buttons { get; set; }
        public List<MessageInputField> InputFields { get; set; }
        public float LifeTime { get; set; }
    }

    public class InputfieldMessage : Message {
        public Action<string> OnConfirm { get; set; }
    }

    public class MessageButton {
        public string Text { get; set; }
        public UnityAction Clicked { get; set; }
        public UnityAction<string> ClickedWithText { get; set; }
    }

    public class MessageInputField {
        public string Text { get; set; }
        public string Placeholder { get; set; }
        public Predicate<string> IsValid { get; set; }

        public InputFieldHelper.InputFieldType Type { get; set; }
    }

    public interface IMessageQueueService {
        event Action MessageAdded;
        Message GetNextMessage();
        bool HasNext();

        void AddErrorMessage(string text, string header = "Error", Action onOpen = null, Action onClose = null,
            string buttonText = "OK");

        void AddConfirmMessage(string text, string header = "Info",
            Action onOpen = null, Action onClose = null,
            string buttonText = "OK", UnityAction onConfirm = null);

        void AddYesNoMessage(string text, string header = "Info",
            Action onOpen = null, Action onClose = null,
            string yesButtonText = "YES", UnityAction onYes = null,
            string noButtonText = "NO", UnityAction onNo = null);

        void AddInputFieldMessage(string text, string placeholderText, string startText = "", string header = "Info",
            InputFieldHelper.InputFieldType type = InputFieldHelper.InputFieldType.PlayerName,
            Action onOpen = null, Action onClose = null,
            string yesButtonText = "YES", UnityAction<string> onYes = null,
            string noButtonText = "NO", UnityAction onNo = null);

        void AddVolatileMessage(string text, string header = "Info",
            Action onOpen = null, Action onClose = null,
            List<MessageButton> messageButtons = null, float lifeTime = 0);

        void AddVolatileButtonMessage(string text, string header = "Info",
            Action onOpen = null, Action onClose = null,
            string okButtonText = "OK", UnityAction onOkButton = null);

        void RemoveMessage(Message message);
    }

    public class MessageQueueService : IMessageQueueService {
        public event Action MessageAdded {
            add => MessageQueue.Singleton.MessageAdded += value;
            remove => MessageQueue.Singleton.MessageAdded -= value;
        }

        public Message GetNextMessage() {
            return MessageQueue.Singleton.GetNextMessage();
        }

        public bool HasNext() {
            return MessageQueue.Singleton.HasNext();
        }

        public void AddErrorMessage(string text, string header, Action onOpen, Action onClose, string buttonText) {
            MessageQueue.Singleton.AddErrorMessage(text, header, onOpen, onClose, buttonText);
        }

        public void AddConfirmMessage(string text, string header, Action onOpen, Action onClose, string buttonText,
            UnityAction onConfirm) {
            MessageQueue.Singleton.AddConfirmMessage(text, header, onOpen, onClose, buttonText, onConfirm);
        }

        public void AddYesNoMessage(string text, string header, Action onOpen, Action onClose, string yesButtonText,
            UnityAction onYes, string noButtonText, UnityAction onNo) {
            MessageQueue.Singleton.AddYesNoMessage(text, header, onOpen, onClose, yesButtonText, onYes, noButtonText,
                onNo);
        }

        public void AddInputFieldMessage(string text, string placeholderText, string startText, string header,
            InputFieldHelper.InputFieldType type,
            Action onOpen, Action onClose, string yesButtonText, UnityAction<string> onYes, string noButtonText,
            UnityAction onNo) {
            MessageQueue.Singleton.AddInputFieldMessage(text, placeholderText, startText, header, type,
                onOpen, onClose,
                yesButtonText, onYes, noButtonText, onNo);
        }

        public void AddVolatileMessage(string text, string header, Action onOpen, Action onClose,
            List<MessageButton> messageButtons, float lifeTime) {
            MessageQueue.Singleton.AddVolatileMessage(text, header, onOpen, onClose, messageButtons, lifeTime);
        }

        public void AddVolatileButtonMessage(string text, string header, Action onOpen, Action onClose,
            string okButtonText, UnityAction onOkButton) {
            MessageQueue.Singleton.AddVolatileButtonMessage(text, header, onOpen, onClose, okButtonText, onOkButton);
        }

        public void RemoveMessage(Message message) {
            MessageQueue.Singleton.RemoveMessage(message);
        }
    }
}