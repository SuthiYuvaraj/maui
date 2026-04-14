using System;
using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Android.Views.Accessibility;

namespace Microsoft.Maui.Platform
{
	internal class RadioButtonContentViewGroup : ContentViewGroup
	{
		bool _isChecked;

		// Raised when the user activates the radio button via keyboard or arrow-key navigation
		public event EventHandler? CheckedChange;

		internal RadioButtonContentViewGroup(Context context) : base(context)
		{
			Focusable = true;
			// Allow focus in touch mode so that App.Tap() gives this view keyboard focus,
			// enabling subsequent PressKeyCode calls to be delivered here.
			FocusableInTouchMode = true;
			Clickable = true;
		}

		public bool IsChecked
		{
			get => _isChecked;
			set
			{
				if (_isChecked == value)
					return;
				_isChecked = value;
				SendAccessibilityEvent(EventTypes.ViewClicked);
			}
		}

		public override bool DispatchKeyEvent(KeyEvent? e)
		{
			if (e?.Action == KeyEventActions.Down)
			{
				switch (e.KeyCode)
				{
					case Keycode.Enter:
					case Keycode.NumpadEnter:
					case Keycode.Space:
					case Keycode.DpadCenter:
						CheckedChange?.Invoke(this, EventArgs.Empty);
						return true;

					// Arrow keys move focus to the next/previous sibling AND select it,
					// matching native Android RadioGroup behaviour.
					case Keycode.DpadDown:
					case Keycode.DpadRight:
						if (MoveAndSelectSibling(forward: true))
							return true;
						break;

					case Keycode.DpadUp:
					case Keycode.DpadLeft:
						if (MoveAndSelectSibling(forward: false))
							return true;
						break;
				}
			}

			return base.DispatchKeyEvent(e);
		}

		public override void OnInitializeAccessibilityNodeInfo(AccessibilityNodeInfo? info)
		{
			base.OnInitializeAccessibilityNodeInfo(info);

			if (info is null)
				return;

			info.ClassName = "android.widget.RadioButton";
			info.Checkable = true;
			info.Checked = _isChecked;
		}

		// Moves keyboard focus to the next/previous sibling RadioButtonContentViewGroup
		// and fires CheckedChange on it (select-on-navigate, like native RadioGroup).
		bool MoveAndSelectSibling(bool forward)
		{
			if (Parent is not ViewGroup parent)
				return false;

			var siblings = new List<RadioButtonContentViewGroup>();
			for (int i = 0; i < parent.ChildCount; i++)
			{
				if (parent.GetChildAt(i) is RadioButtonContentViewGroup rb)
					siblings.Add(rb);
			}

			int currentIndex = siblings.IndexOf(this);
			if (currentIndex < 0)
				return false;

			int nextIndex = forward ? currentIndex + 1 : currentIndex - 1;
			if (nextIndex < 0 || nextIndex >= siblings.Count)
				return false;

			var target = siblings[nextIndex];
			target.RequestFocus();
			target.CheckedChange?.Invoke(target, EventArgs.Empty);
			return true;
		}
	}
}
