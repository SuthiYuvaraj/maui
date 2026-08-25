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

	// Snapshot of the coordinates MAUI needs from a MotionEvent, captured synchronously.
	// MotionEvent instances are pooled by Android and may be recycled (and reused for a
	// later event) as soon as dispatch returns, so callers must never hang on to the
	// MotionEvent itself for deferred/async use (e.g. PointerEventArgs.GetPosition()).
	internal readonly struct MotionEventPosition
	{
		public MotionEventPosition(float rawX, float rawY, float x, float y)
		{
			RawX = rawX;
			RawY = rawY;
			X = x;
			Y = y;
		}

		public float RawX { get; }
		public float RawY { get; }
		public float X { get; }
		public float Y { get; }
	}

	internal static MotionEventPosition CapturePosition(this MotionEvent e) =>
		new(e.RawX, e.RawY, e.GetX(), e.GetY());

	internal static Point? CalculatePosition(this MotionEventPosition position, IElement? sourceElement, IElement? relativeElement)
	{
		var context = sourceElement?.Handler?.MauiContext?.Context;

		if (context == null)
			return null;

		if (relativeElement == null)
		{
			return new Point(context.FromPixels(position.RawX), context.FromPixels(position.RawY));
		}

		if (relativeElement == sourceElement)
		{
			return new Point(context.FromPixels(position.X), context.FromPixels(position.Y));
		}

		if (relativeElement?.Handler?.PlatformView is AView aView)
		{
			var location = aView.GetLocationOnScreenPx();

			var x = position.RawX - location.X;
			var y = position.RawY - location.Y;

			return new Point(context.FromPixels(x), context.FromPixels(y));
		}

		return null;
	}
}
