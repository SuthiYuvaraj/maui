using System;
using System.Collections.Concurrent;
using System.Globalization;

namespace Microsoft.Maui
{
	/// <summary>
	/// Provides culture change detection functionality for MAUI controls.
	/// </summary>
	internal static class CultureTracker
	{
		static CultureInfo? s_currentCulture;
		static readonly ConcurrentDictionary<WeakReference, Action> s_subscribers = new();

		/// <summary>
		/// Checks if the culture has changed since the last call and notifies subscribers if it has.
		/// </summary>
		public static void CheckForCultureChanges()
		{
			var currentCulture = CultureInfo.CurrentCulture;
			
			if (s_currentCulture == null || !s_currentCulture.Equals(currentCulture))
			{
				s_currentCulture = currentCulture;
				NotifyCultureChanged();
			}
		}

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

		static void NotifyCultureChanged()
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