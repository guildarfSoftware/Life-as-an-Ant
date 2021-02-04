using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageManager : MonoBehaviour
{
    static MessageManager instance;
    [SerializeField] GameObject messageWindow;
    [SerializeField] Button buttonAccept, buttonCancel;
    [SerializeField] Text messageText;

    float cooldDownTimer;
    const float TimeBetweenMessages = 1f; //1 seccond since closing 1 message until another can show up
    bool messageActive;

    Queue<string> MessageQueue = new Queue<string>();
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

    public static void Message(string textInfo)
    {
        if(instance.MessageQueue.Contains(textInfo)) return; //avoid repeated messages
        instance.MessageQueue.Enqueue(textInfo);
    }

    void MessagePopUp(string messageBody)
    {
        messageActive = true;
        messageWindow.SetActive(true);
        Time.timeScale = 0;

        messageText.text = messageBody;

    }

    public void CloseMessage()
    {
        messageWindow.SetActive(false);
        messageActive = false;
        cooldDownTimer = TimeBetweenMessages;
        Time.timeScale = 1;
    }



}
