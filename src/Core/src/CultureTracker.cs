using System;
using System.Collections.Concurrent;
using System.Globalization;

namespace Microsoft.Maui
{
	/// <summary>
	/// Provides culture change notification functionality for MAUI controls.
	/// </summary>
	public static class CultureTracker
	{
		static readonly ConcurrentDictionary<WeakReference, Action> s_subscribers = new();

		/// <summary>
		/// Subscribes an object to culture change notifications.
		/// </summary>
		/// <param name="subscriber">The object to subscribe</param>
		/// <param name="action">The action to invoke when culture changes</param>
		public static void Subscribe(object subscriber, Action action)
		{
			var weakRef = new WeakReference(subscriber);
			s_subscribers.TryAdd(weakRef, action);
		}

		/// <summary>
		/// Unsubscribes an object from culture change notifications.
		/// </summary>
		/// <param name="subscriber">The object to unsubscribe</param>
		public static void Unsubscribe(object subscriber)
		{
			// Find and remove the weak reference
			foreach (var kvp in s_subscribers)
			{
				if (kvp.Key.IsAlive && ReferenceEquals(kvp.Key.Target, subscriber))
				{
					s_subscribers.TryRemove(kvp.Key, out _);
					break;
				}
			}
		}

		/// <summary>
		/// Notifies all subscribers that the culture has changed.
		/// Call this method from your application when you change the culture at runtime.
		/// </summary>
		/// <example>
		/// <code>
		/// // In your application code when changing culture:
		/// CultureInfo.CurrentCulture = new CultureInfo("de-DE");
		/// CultureTracker.NotifyCultureChanged();
		/// </code>
		/// </example>
		public static void NotifyCultureChanged()
		{
			// Clean up dead references and notify live ones
			var deadRefs = new System.Collections.Generic.List<WeakReference>();
			
			foreach (var kvp in s_subscribers)
			{
				if (!kvp.Key.IsAlive)
				{
					deadRefs.Add(kvp.Key);
				}
				else
				{
					try
					{
						kvp.Value?.Invoke();
					}
					catch
					{
						// Ignore exceptions from subscribers to prevent one bad subscriber from affecting others
					}
				}
			}

			// Clean up dead references
			foreach (var deadRef in deadRefs)
			{
				s_subscribers.TryRemove(deadRef, out _);
			}
		}
	}
}