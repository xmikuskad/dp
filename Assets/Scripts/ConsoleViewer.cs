using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.UI;

public class ConsoleViewer : MonoBehaviour
{

    [Header("UI elements")]
    [SerializeField]
    private TMP_Text debugText;
    [SerializeField]
    private Image debugBtn;
    [SerializeField]
    private Image warningBtn;
    [SerializeField]
    private Image errorBtn;

    [Header("Configuration")]
    [SerializeField]
    private bool allowErrors = true;
    [SerializeField]
    private bool allowWarnings = true;
    [SerializeField]
    private bool allowDebugs = true;
    private int maxMessagesCount = 20;

    [Header("Colors")]
    [SerializeField]
    private Color debugColor;
    [SerializeField]
    private Color warningColor;
    [SerializeField]
    private Color errorColor;
    [SerializeField]
    private Color exceptionColor;

    private List<ConsoleMessage> messages;

    // Start is called before the first frame update
    void Start()
    {
        messages = new List<ConsoleMessage>();
    }
    private void Awake()
    {
        Application.logMessageReceived += LogCallback;
    }

    /*void OnEnable()
    {
        Application.logMessageReceived += LogCallback;
    }*/

    void LogCallback(string condition, string stackTrace, LogType type)
    {
        if ((type == LogType.Log && !allowDebugs) || (type == LogType.Warning && !allowWarnings) || (type == LogType.Error && !allowErrors))
        {
            return;
        }

        UpdateText(new ConsoleMessage(condition, stackTrace, type));
    }

    /*void OnDisable()
    {
        Application.logMessageReceived -= LogCallback;
    }*/

    private void UpdateText(ConsoleMessage newMessage)
    {
        while (messages.Count >= maxMessagesCount)
        {
            messages.RemoveAt(messages.Count - 1);
        }
        messages.Insert(0, newMessage);


        debugText.text = string.Join("\n", messages.Select(m => "<color=#" + GetMsgColor(m.msgType) + "> [" + m.msgTime + "] " + m.condition + "\n" + m.stackTrace + " </color>").ToArray());
    }

    private string GetMsgColor(LogType type)
    {
        switch (type)
        {
            case LogType.Log: return ColorUtility.ToHtmlStringRGB(debugColor);
            case LogType.Warning: return ColorUtility.ToHtmlStringRGB(warningColor);
            case LogType.Error: return ColorUtility.ToHtmlStringRGB(errorColor);
            case LogType.Exception: return ColorUtility.ToHtmlStringRGB(exceptionColor);
            default: return ColorUtility.ToHtmlStringRGB(errorColor);
        }
    }

    public void ToggleErrors()
    {
        this.allowErrors = !this.allowErrors;
        if (this.allowErrors)
        {
            errorBtn.color = new Color(255, 255, 255);
        }
        else
        {
            errorBtn.color = Color.gray;
        }
    }
    public void ToggleWarnings()
    {
        this.allowWarnings = !this.allowWarnings;
        if (this.allowWarnings)
        {
            warningBtn.color = new Color(255, 255, 255);
        }
        else
        {
            warningBtn.color = Color.gray;
        }
    }
    public void ToggleDebugs()
    {
        this.allowDebugs = !this.allowDebugs;
        if (this.allowDebugs)
        {
            debugBtn.color = new Color(255, 255, 255);
        }
        else
        {
            debugBtn.color = Color.gray;
        }
    }
}
