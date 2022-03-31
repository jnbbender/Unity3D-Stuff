using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class UILogger : MonoBehaviour
{
    // Adjust via the Inspector
    public int maxLines = 8;
    public int height = 250;
    public int width = 500;

    private Queue<string> queue = new Queue<string>();
    private string currentText = "";

    void OnEnable()
    {
        Application.logMessageReceivedThreaded += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceivedThreaded -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        // Delete oldest message
        if (queue.Count >= maxLines) queue.Dequeue();

        queue.Enqueue(logString);

        var builder = new StringBuilder();
        foreach (string st in queue)
        {
            builder.Append(st).Append("\n");
        }

        currentText = builder.ToString();
    }

    void OnGUI()
    {
        GUI.Label(
           new Rect(
               5,                   // x, left offset
               5, //Screen.height - 150, // y, bottom offset
               width,                // width
               height                 // height
           ),
           currentText,             // the display text
           GUI.skin.textArea        // use a multi-line text area
        );
    }
}