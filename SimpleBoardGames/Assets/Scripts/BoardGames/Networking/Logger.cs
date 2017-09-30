using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace BoardGames.Networking
{
	public struct Message
	{
		public string Text;
		public Color Color;

		public Message(string text) : this(text, Color.white) { }
		public Message(string text, Color col) { Text = text; Color = col; }

		public static Message Error(string text) { return new Message(text, Color.red); }
		public static Message Warning(string text) { return new Message(text, Color.yellow); }
	}


	/// <summary>
	/// A thread-safe logger that keeps track of past messages.
	/// </summary>
	public class Logger
	{
		/// <summary>
		/// If true, then messages are also logged to the Unity log file.
		/// </summary>
		public bool LogInUnity;

		/// <summary>
		/// The max number of messages allowed.
		/// New messages replace the oldest ones.
		/// </summary>
		public int MaxMessages
		{
			get { return maxMessages; }
			set
			{
				maxMessages = value;

				if (messages.Count > MaxMessages)
				{
					int nToRemove = messages.Count - MaxMessages;
					messages.RemoveRange(0, nToRemove);

					if (OnRemoveMessages != null)
						OnRemoveMessages(nToRemove);
				}
			}
		}
		private int maxMessages;

		/// <summary>
		/// Raised when some of the oldest messages have been removed.
		/// The argument is the number of messages.
		/// </summary>
		public event System.Action<int> OnRemoveMessages;
		/// <summary>
		/// Raised when a new message is added.
		/// </summary>
		public event System.Action<Message> OnNewMessage;

		private List<Message> messages = new List<Message>();
		private object messagesLocker = new object();


		public Logger(int maxMessages, bool logInUnity = true)
		{
			MaxMessages = maxMessages;
			LogInUnity = logInUnity;
		}


		public void Add(Message msg)
		{
			lock (messagesLocker)
			{
				//Remove old messages to make room for this one.
				int nToRemove = 0;
				if (messages.Count + 1 > MaxMessages)
				{
					nToRemove = messages.Count + 1 - MaxMessages;
					messages.RemoveRange(0, nToRemove);

					if (OnRemoveMessages != null)
						OnRemoveMessages(nToRemove);
				}

				messages.Add(msg);

				//Log the message in the Unity log file.
				if (LogInUnity)
				{
					if (msg.Color == Color.red)
						Debug.LogError(msg.Text);
					else if (msg.Color == Color.yellow)
						Debug.LogWarning(msg.Text);
					else
						Debug.Log(msg.Text);
				}

				//Raise the "Add message" event.
				if (OnNewMessage != null)
					OnNewMessage(msg);
			}
		}
	}
}
