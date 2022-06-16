using System;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum ConsoleLogType
{
	Log = (1 << 0),
	Warning = (1 << 1),
	Error = (1 << 2),
	Assert = (1 << 3),
	Exception = (1 << 4)
}

/// <summary>
/// A console to display Unity's debug logs in-game.
/// </summary>
public class ConsoleGUI : MonoBehaviour
{
	struct Log
	{
		public string message;
		public string stackTrace;
		public LogType type;
	}

	/// <summary>
	/// The hotkey to show and hide the console window.
	/// </summary>
	public KeyCode toggleKey = KeyCode.Home;
	public KeyCode clearKey = KeyCode.End;

	public bool collapse = true;

	public ConsoleLogType toLog;

	public float width;
	public float height;

	bool show = true;

	List<Log> logs = new List<Log>();
	Vector2 scrollPosition;

	// Visual elements:

	static readonly Dictionary<LogType, Color> logTypeColors = new Dictionary<LogType, Color>()
	{
		{ LogType.Assert, Color.white },
		{ LogType.Error, Color.red },
		{ LogType.Exception, Color.red },
		{ LogType.Log, Color.white },
		{ LogType.Warning, Color.yellow },
	};

	const int margin = 20;

	Rect windowRect;
	Rect titleBarRect = new Rect(0, 0, 10000, 20);

	void OnEnable ()
	{
		windowRect = new Rect(margin, margin, width - (margin * 4), height - (margin * 4));
		Application.RegisterLogCallback(HandleLog);
	}

	void OnDisable ()
	{
		Application.RegisterLogCallback(null);
	}

	void Update ()
	{
		if (Input.GetKeyDown(toggleKey)) {
			show = !show;
		}
		if (Input.GetKeyDown(clearKey))
        	{
			logs.Clear();
		}

	}

	void OnGUI ()
	{
		if (!show) {
			return;
		}

		windowRect = GUILayout.Window(123456, windowRect, ConsoleWindow, "Console");
	}

	/// <summary>
	/// A window that displayss the recorded logs.
	/// </summary>
	/// <param name="windowID">Window ID.</param>
	void ConsoleWindow (int windowID)
	{
		scrollPosition = new Vector2(0, scrollPosition.y + 20f);
		scrollPosition = GUILayout.BeginScrollView(scrollPosition);

			// Iterate through the recorded logs.
			for (int i = 0; i < logs.Count; i++) {
				var log = logs[i];

				// Combine identical messages if collapse option is chosen.
				if (collapse) {
					var messageSameAsPrevious = i > 0 && log.message == logs[i - 1].message;

					if (messageSameAsPrevious) {
						continue;
					}
				}

				GUI.contentColor = logTypeColors[log.type];
				GUILayout.Label(log.message);
			}

		GUILayout.EndScrollView();

		GUI.contentColor = Color.white;

//		GUILayout.BeginHorizontal();

//		GUILayout.EndHorizontal();

		// Allow the window to be dragged by its title bar.
		GUI.DragWindow(titleBarRect);
	}

	/// <summary>
	/// Records a log from the log callback.
	/// </summary>
	/// <param name="message">Message.</param>
	/// <param name="stackTrace">Trace of where the message came from.</param>
	/// <param name="type">Type of message (error, exception, warning, assert).</param>
	void HandleLog (string message, string stackTrace, LogType type)
	{
		int log = 0;
		switch (type)
                {
			case LogType.Log:
				log = ((int)toLog) & (int)ConsoleLogType.Log;
				break;
			case LogType.Exception:
				log = (int)toLog & (int)ConsoleLogType.Exception;
				break;
			case LogType.Warning:
				log = (int)toLog & (int)ConsoleLogType.Warning;
				break;
			case LogType.Error:
				log = (int)toLog & (int)ConsoleLogType.Error;
				break;
			case LogType.Assert:
				log = (int)toLog & (int)ConsoleLogType.Assert;
				break;
		}

		if (log < 1)
			return;

		logs.Add(new Log() {
			message = message,
			stackTrace = stackTrace,
			type = type,
		});
	}
}
