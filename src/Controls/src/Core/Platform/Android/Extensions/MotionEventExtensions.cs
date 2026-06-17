using System;
using Android.Views;
using Microsoft.Maui.Graphics;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform;

internal static class MotionEventExtensions
{
	public static bool IsSecondary(this MotionEvent me)
	{
		var buttonState = me?.ButtonState ?? MotionEventButtonState.Primary;

		return
		  buttonState == MotionEventButtonState.Secondary ||
		  buttonState == MotionEventButtonState.StylusSecondary;
	}

	internal static Point? CalculatePosition(this MotionEvent? e, IElement? sourceElement, IElement? relativeElement)
	{
		var context = sourceElement?.Handler?.MauiContext?.Context;

		if (context == null)
			return null;

		if (e == null)
			return null;

		if (relativeElement == null)
		{
			return new Point(context.FromPixels(e.RawX), context.FromPixels(e.RawY));
		}

		if (relativeElement == sourceElement)
		{
			return new Point(context.FromPixels(e.GetX()), context.FromPixels(e.GetY()));
		}

		if (relativeElement?.Handler?.PlatformView is AView aView)
		{
			var location = aView.GetLocationOnScreenPx();

			var x = e.RawX - location.X;
			var y = e.RawY - location.Y;

			return new Point(context.FromPixels(x), context.FromPixels(y));
		}

		return null;
	}

	internal static Func<IElement?, Point?> CapturePosition(this MotionEvent? e, IElement? sourceElement)
	{
		var context = sourceElement?.Handler?.MauiContext?.Context;
		if (context == null || e == null)
			return _ => null;

		var rawX = e.RawX;
		var rawY = e.RawY;
		var localX = e.GetX();
		var localY = e.GetY();

		return (relativeElement) =>
		{
			if (relativeElement == null)
				return new Point(context.FromPixels(rawX), context.FromPixels(rawY));

			if (relativeElement == sourceElement)
				return new Point(context.FromPixels(localX), context.FromPixels(localY));

			if (relativeElement.Handler?.PlatformView is AView aView)
			{
				var location = aView.GetLocationOnScreenPx();
				var x = rawX - location.X;
				var y = rawY - location.Y;

				return new Point(context.FromPixels(x), context.FromPixels(y));
			}

			return null;
		};
	}
}
