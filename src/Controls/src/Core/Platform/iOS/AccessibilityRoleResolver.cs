using System.Runtime.CompilerServices;
using UIKit;

namespace Microsoft.Maui.Platform
{
    /// <summary>
    /// How a platform view participates in the UIKit accessibility tree.
    /// </summary>
    internal enum AccessibilityRole
    {
        /// <summary>Participation is left to UIKit and the handler; anything the resolver wrote is restored.</summary>
        Baseline,
        /// <summary>The view is a single accessibility element (<c>IsAccessibilityElement = true</c>).</summary>
        Element,
        /// <summary>
        /// A container with semantics. It is an element when its content is passive, and a
        /// labelled semantic group when it has actionable descendants, so those stay reachable.
        /// </summary>
        SemanticContainer,
        /// <summary>The view is not an element; its children remain reachable.</summary>
        NotElement,
    }

    /// <summary>
    /// The single owner of accessibility-tree participation on iOS and Mac Catalyst.
    /// Semantics, <c>AutomationProperties</c> and gesture changes feed inputs here instead of
    /// writing <see cref="UIView.IsAccessibilityElement"/> directly, so each decision sees every
    /// input and every write can be reverted to the captured UIKit/handler default.
    /// </summary>
    /// <remarks>
    /// Precedence (first match wins):
    /// 1. Explicit <c>AutomationProperties.IsInAccessibleTree</c>.
    /// 2. Handler-owned controls (<see cref="UIControl"/>, <see cref="SemanticSwitchContentView"/>) keep their defaults.
    /// 3. Description/Hint on a view with its own activation gesture, or on a non-container view, makes an element.
    /// 4. Description/Hint on a container defers to its content (see <see cref="AccessibilityRole.SemanticContainer"/>).
    /// 5. Otherwise the baseline is restored.
    /// <c>ExcludedWithChildren</c> is applied independently, matching the existing contract.
    /// </remarks>
    internal static class AccessibilityRoleResolver
    {
        const int MaxDescendantDepth = 10;

        static readonly ConditionalWeakTable<UIView, State> s_states = new();

        sealed class State
        {
            public bool? IsInAccessibleTree;
            public bool? ExcludedWithChildren;
            public bool HasActivationGesture;

            public AccessibilityRole Role;
            public bool? DefaultIsAccessibilityElement;
            public bool? DefaultAccessibilityElementsHidden;
        }

        internal static void SetIsInAccessibleTree(UIView platformView, IView? view, bool? value)
        {
            if (GetState(platformView, value.HasValue) is State state)
            {
                state.IsInAccessibleTree = value;
                Apply(platformView, view, state);
            }
        }

        internal static void SetExcludedWithChildren(UIView platformView, bool? value)
        {
            if (GetState(platformView, value.HasValue) is not State state)
            {
                return;
            }

            state.ExcludedWithChildren = value;

            if (value is bool hidden)
            {
                // Same default as the previous mapper: UIKit controls treat a missing value as hidden.
                state.DefaultAccessibilityElementsHidden ??= platformView.AccessibilityElementsHidden || platformView is UIControl;
                platformView.AccessibilityElementsHidden = hidden;
            }
            else if (state.DefaultAccessibilityElementsHidden is bool defaultHidden)
            {
                platformView.AccessibilityElementsHidden = defaultHidden;
                state.DefaultAccessibilityElementsHidden = null;
            }
        }

        internal static void SetHasActivationGesture(UIView platformView, IView? view, bool value)
        {
            if (GetState(platformView, value) is State state && state.HasActivationGesture != value)
            {
                state.HasActivationGesture = value;
                Apply(platformView, view, state);
            }
        }

        /// <summary>
        /// Re-evaluates participation (and the dependent heading trait) after semantics change.
        /// </summary>
        internal static void UpdateSemantics(UIView platformView, IView? view)
        {
            if (GetState(platformView, HasLabelOrHint(view?.Semantics)) is State state)
            {
                Apply(platformView, view, state);
            }
        }

        internal static bool IsTracked(UIView platformView) => s_states.TryGetValue(platformView, out _);

        /// <summary>
        /// Resolves a deferred <see cref="AccessibilityRole.SemanticContainer"/> against the live
        /// subtree, so content, gesture and child changes are observed without re-mapping.
        /// </summary>
        internal static bool ResolveContainerIsAccessibilityElement(UIView container) =>
            !HasActionableDescendant(container, 0);

        static State? GetState(UIView platformView, bool create)
        {
            if (s_states.TryGetValue(platformView, out var state))
            {
                return state;
            }

            return create ? s_states.GetValue(platformView, _ => new State()) : null;
        }

        static void Apply(UIView platformView, IView? view, State state)
        {
            var role = Resolve(platformView, view, state);
            state.Role = role;

            var mauiView = platformView as MauiView;

            if (mauiView is not null)
            {
                mauiView.DeferAccessibilityElementToContent = role == AccessibilityRole.SemanticContainer;
            }

            switch (role)
            {
                case AccessibilityRole.Element:
                    WriteIsAccessibilityElement(platformView, state, true);
                    break;
                case AccessibilityRole.NotElement:
                    WriteIsAccessibilityElement(platformView, state, false);
                    break;
                case AccessibilityRole.SemanticContainer:
                    // The getter is resolved lazily from content; keep the stored value at its default.
                    RestoreIsAccessibilityElement(platformView, state);
                    break;
                default:
                    RestoreIsAccessibilityElement(platformView, state);
                    break;
            }

            // Heading depends on the role, so re-sync it whenever any input changes the role.
            if (view?.Semantics is Semantics semantics)
            {
                SemanticExtensions.UpdateSemanticsHeading(platformView, semantics, role);
            }
        }

        static AccessibilityRole Resolve(UIView platformView, IView? view, State state)
        {
            if (state.IsInAccessibleTree is bool inTree)
            {
                return inTree ? AccessibilityRole.Element : AccessibilityRole.NotElement;
            }

            if (IsHandlerOwned(platformView))
            {
                return AccessibilityRole.Baseline;
            }

            if (!HasLabelOrHint(view?.Semantics))
            {
                return AccessibilityRole.Baseline;
            }

            if (!state.HasActivationGesture && platformView is MauiView && view is ILayout or IContentView)
            {
                return AccessibilityRole.SemanticContainer;
            }

            return AccessibilityRole.Element;
        }

        // UIStepper and UIPageControl are composite controls that UIKit does not expose as elements,
        // so semantics are allowed to promote them (preserves the previous behaviour). UISearchBar
        // forwards semantics to its text field, so promoting the bar itself would hide the field.
        static bool IsHandlerOwned(UIView platformView) =>
            platformView is SemanticSwitchContentView
            || platformView is UISearchBar
            || (platformView is UIControl && platformView is not UIStepper && platformView is not UIPageControl);

        static bool HasLabelOrHint(Semantics? semantics) =>
            semantics is not null
            && (!string.IsNullOrWhiteSpace(semantics.Description) || !string.IsNullOrWhiteSpace(semantics.Hint));

        static void WriteIsAccessibilityElement(UIView platformView, State state, bool value)
        {
            // UIKit sets the default late in the layout cycle; standard controls default to true.
            state.DefaultIsAccessibilityElement ??= platformView.IsAccessibilityElement || platformView is UIControl;
            platformView.IsAccessibilityElement = value;
        }

        static void RestoreIsAccessibilityElement(UIView platformView, State state)
        {
            if (state.DefaultIsAccessibilityElement is bool defaultValue)
            {
                platformView.IsAccessibilityElement = defaultValue;
                state.DefaultIsAccessibilityElement = null;
            }
        }

        static bool HasActionableDescendant(UIView view, int depth)
        {
            if (depth >= MaxDescendantDepth)
            {
                return false;
            }

            foreach (var subview in view.Subviews)
            {
                if (subview.Hidden || subview.AccessibilityElementsHidden)
                {
                    continue;
                }

                if (IsActionable(subview) || HasActionableDescendant(subview, depth + 1))
                {
                    return true;
                }
            }

            return false;
        }

        static bool IsActionable(UIView view)
        {
            const UIAccessibilityTrait actionableTraits =
                UIAccessibilityTrait.Button | UIAccessibilityTrait.Link | UIAccessibilityTrait.Adjustable;

            if ((view.AccessibilityTraits & actionableTraits) != 0)
            {
                return true;
            }

            if (view is UIControl control)
            {
                return control.Enabled;
            }

            if (view is UITextView textView)
            {
                return textView.Editable;
            }

            return s_states.TryGetValue(view, out var state) && state.HasActivationGesture;
        }
    }
}