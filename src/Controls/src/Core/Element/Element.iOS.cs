#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Platform;
using UIKit;

namespace Microsoft.Maui.Controls
{
	public partial class Element
	{
		public static void MapAutomationPropertiesIsInAccessibleTree(IElementHandler handler, Element element)
		{
			if (handler.PlatformView is not UIView control)
				return;

			// Participation is owned by AccessibilityRoleResolver so this override, semantics,
			// gestures and handler defaults are resolved together, and clearing it restores the default.
			bool? value = element.IsSet(AutomationProperties.IsInAccessibleTreeProperty)
				? (bool?)element.GetValue(AutomationProperties.IsInAccessibleTreeProperty)
				: null;

			AccessibilityRoleResolver.SetIsInAccessibleTree(control, handler.VirtualView as IView, value);
		}

		public static void MapAutomationPropertiesExcludedWithChildren(IElementHandler handler, Element view)
		{
			if (handler.PlatformView is not UIView control)
				return;

			bool? value = view.IsSet(AutomationProperties.ExcludedWithChildrenProperty)
				? (bool?)view.GetValue(AutomationProperties.ExcludedWithChildrenProperty)
				: null;

			AccessibilityRoleResolver.SetExcludedWithChildren(control, value);
		}

		static void MapAutomationPropertiesIsInAccessibleTree(IElementHandler handler, IElement element)
		{
			if (element is Element e)
				MapAutomationPropertiesIsInAccessibleTree(handler, e);
		}

		static void MapAutomationPropertiesExcludedWithChildren(IElementHandler handler, IElement element)
		{
			if (element is Element e)
				MapAutomationPropertiesExcludedWithChildren(handler, e);
		}
	}
}