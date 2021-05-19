using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MessageManager : MonoBehaviour
{
    static MessageManager instance;
    [SerializeField] GameObject messageWindow;
    [SerializeField] Button buttonAccept, buttonCancel;
    [SerializeField] Text messageText;

    [SerializeField] Text tittleText;

    float cooldDownTimer;
    const float TimeBetweenMessages = 1f; //1 seccond since closing 1 message until another can show up
    bool messageActive;

    Queue<MessageInfo> MessageQueue = new Queue<MessageInfo>();
    void Start()
    {
        if (instance != null) Debug.LogWarning("multiple messageManager instances");
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        cooldDownTimer -= Time.deltaTime;
        if (cooldDownTimer <= 0 && MessageQueue.Count != 0)
        {
            MessagePopUp(MessageQueue.Dequeue());
        }
    }

    public static void Message(string title, string textInfo, UnityAction accept, UnityAction cancel)
    {

        MessageInfo message = new MessageInfo(title, textInfo, accept, cancel);
        if (instance.MessageQueue.Contains(message)) return; //avoid repeated messages
        instance.MessageQueue.Enqueue(message);
    }

    void MessagePopUp(MessageInfo message)
    {
        messageActive = true;
        messageWindow.SetActive(true);
        Time.timeScale = 0;

        buttonAccept.onClick.RemoveAllListeners();
        buttonCancel.onClick.RemoveAllListeners();

        tittleText.text = message.title;
        messageText.text = message.body;

        if (message.cancelAction != null)
        {
            buttonCancel.gameObject.SetActive(true);
            buttonCancel.onClick.AddListener(message.cancelAction);
            buttonCancel.onClick.AddListener(CloseMessage);
        }
        else
        {
            buttonCancel.gameObject.SetActive(false);
        }

        if(message.acceptAction != null)
        {
            buttonAccept.onClick.AddListener(message.acceptAction);
        }
        buttonAccept.onClick.AddListener(CloseMessage);

    }

    public void CloseMessage()
    {
        messageWindow.SetActive(false);
        messageActive = false;
        cooldDownTimer = TimeBetweenMessages;
        Time.timeScale = 1;
    }

    struct MessageInfo
    {
        public string title;
        public string body;
        public UnityAction acceptAction;
        public UnityAction cancelAction;

        public MessageInfo(string title, string body, UnityAction acceptAction, UnityAction cancelAction)
        {
            this.title = title;
            this.body = body;
            this.acceptAction = acceptAction;
            this.cancelAction = cancelAction;
        }

        public override bool Equals(object obj)
        {
            return obj is MessageInfo info &&
                   title == info.title &&
                   body == info.body;
        }

        public override int GetHashCode()
        {
            int hashCode = 1624306882;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(title);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(body);
            return hashCode;
        }
    }

}
