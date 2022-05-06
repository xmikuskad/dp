using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class ConsoleMessage
{
    static string datePatt = "HH:mm:ss.fff";

    public string condition { get; private set; }
    public string stackTrace { get; private set; }
    public string msgTime { get; private set; }
    public LogType msgType { get; private set; }

    public ConsoleMessage(string condition, string stackTrace, LogType type)
    {
        this.condition = condition;
        this.stackTrace = stackTrace;
        this.msgType = type;
        this.msgTime = DateTime.Now.ToString(datePatt);
    }
}
