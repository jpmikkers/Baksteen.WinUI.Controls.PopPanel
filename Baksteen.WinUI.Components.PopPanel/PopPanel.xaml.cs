// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Baksteen.WinUI.Controls;

using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core.AnimationMetrics;

public enum PopDirection
{
    LeftToRight,
    RightToLeft,
    TopToBottom,
    BottomToTop
}

[ContentProperty(Name = "InnerContent")]
public sealed partial class PopPanel : UserControl
{
    private long cbToken1;
    private long cbToken2;
    private Storyboard? activeStoryboard;

    public static readonly DependencyProperty InnerContentProperty =
    DependencyProperty.Register(nameof(InnerContent), typeof(object), typeof(PopPanel), new PropertyMetadata(null));

    public static readonly DependencyProperty IsExpandedProperty =
    DependencyProperty.Register(nameof(IsExpanded), typeof(bool), typeof(PopPanel), new PropertyMetadata(true));

    public static readonly DependencyProperty PopDirectionProperty =
    DependencyProperty.Register(nameof(PopDirection), typeof(PopDirection), typeof(PopPanel), new PropertyMetadata(PopDirection.TopToBottom));

    public static readonly DependencyProperty CollapseDurationProperty =
    DependencyProperty.Register(nameof(CollapseDuration), typeof(Duration), typeof(PopPanel), new PropertyMetadata(new Duration(TimeSpan.FromSeconds(0.5))));

    public static readonly DependencyProperty ExpandDurationProperty =
    DependencyProperty.Register(nameof(ExpandDuration), typeof(Duration), typeof(PopPanel), new PropertyMetadata(new Duration(TimeSpan.FromSeconds(0.5))));

    public object InnerContent
    {
        get
        {
            return GetValue(InnerContentProperty);
        }
        set
        {
            SetValue(InnerContentProperty, value);
        }
    }

    public bool IsExpanded
    {
        get
        {
            return (bool)GetValue(IsExpandedProperty);
        }
        set
        {
            SetValue(IsExpandedProperty, value);
        }
    }

    public PopDirection PopDirection
    {
        get
        {
            return (PopDirection)GetValue(PopDirectionProperty);
        }
        set
        {
            SetValue(PopDirectionProperty, value);
        }
    }

    public Duration CollapseDuration
    {
        get
        {
            return (Duration)GetValue(CollapseDurationProperty);
        }
        set
        {
            SetValue(CollapseDurationProperty, value);
        }
    }

    public Duration ExpandDuration
    {
        get
        {
            return (Duration)GetValue(ExpandDurationProperty);
        }
        set
        {
            SetValue(ExpandDurationProperty, value);
        }
    }

    public PopPanel()
    {
        this.InitializeComponent();
        this.Loaded += UserControl2_Loaded;
        this.Unloaded += UserControl2_Unloaded;
        measureStack.SizeChanged += MeasureStack_SizeChanged;
    }

    private double latchedHeight;
    private double latchedWidth;

    private void MeasureStack_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if(measureStack.ActualHeight > 0) latchedHeight = measureStack.ActualHeight;
        if(measureStack.ActualWidth > 0) latchedWidth = measureStack.ActualWidth;
    }

    private void PropChanged(DependencyObject sender, DependencyProperty dp)
    {
        HandleExpand();
    }

    private void PopDirectionChanged(DependencyObject sender, DependencyProperty dp)
    {
        activeStoryboard?.Stop();
        ConfigureMeasureStack();
        measureStack.Orientation = PopDirection == PopDirection.TopToBottom ? Orientation.Vertical : Orientation.Horizontal;
        measureStack.HorizontalAlignment = userControl.HorizontalAlignment == HorizontalAlignment.Left ? HorizontalAlignment.Right : HorizontalAlignment.Left;
    }

    private void UserControl2_Loaded(object sender, RoutedEventArgs e)
    {
        cbToken1 = RegisterPropertyChangedCallback(IsExpandedProperty, PropChanged);
        cbToken2 = RegisterPropertyChangedCallback(PopDirectionProperty, PopDirectionChanged);
        ConfigureMeasureStack();
        //HandleExpand();
    }

    private void ConfigureMeasureStack()
    {
        switch(PopDirection)
        {
            case PopDirection.LeftToRight:
                measureStack.Orientation = Orientation.Horizontal;
                measureStack.HorizontalAlignment = HorizontalAlignment.Right;
                measureStack.VerticalAlignment = VerticalAlignment.Stretch;
                break;

            case PopDirection.RightToLeft:
                measureStack.Orientation = Orientation.Horizontal;
                measureStack.HorizontalAlignment = HorizontalAlignment.Left;
                measureStack.VerticalAlignment = VerticalAlignment.Stretch;
                break;

            case PopDirection.TopToBottom:
                measureStack.Orientation = Orientation.Vertical;
                measureStack.HorizontalAlignment = HorizontalAlignment.Stretch;
                measureStack.VerticalAlignment = VerticalAlignment.Bottom;
                break;

            case PopDirection.BottomToTop:
                measureStack.Orientation = Orientation.Vertical;
                measureStack.HorizontalAlignment = HorizontalAlignment.Stretch;
                measureStack.VerticalAlignment = VerticalAlignment.Top;
                break;
        }
    }

    private void UserControl2_Unloaded(object sender, RoutedEventArgs e)
    {
        this.UnregisterPropertyChangedCallback(IsExpandedProperty, cbToken2);
        this.UnregisterPropertyChangedCallback(IsExpandedProperty, cbToken1);
    }

    private bool IsVertical() => PopDirection switch
    {
        PopDirection.RightToLeft or PopDirection.LeftToRight => false,
        PopDirection.TopToBottom or PopDirection.BottomToTop => true,
        _ => false,
    };

    private void HandleExpand()
    {
        if(activeStoryboard is not null)
        {
            activeStoryboard.Stop();
            activeStoryboard = null;
        }

        if(IsExpanded)
        {
            measureStack.Visibility = Visibility.Visible;

            // Measure the StackPanel with an infinite height constraint
            measureStack.Measure(new Windows.Foundation.Size(double.PositiveInfinity, double.PositiveInfinity));

            // Get the actual and desired height
            double actualDim = IsVertical() ? contentPanel.ActualHeight : contentPanel.ActualWidth;
            double desiredDim = IsVertical() ? measureStack.DesiredSize.Height : measureStack.DesiredSize.Width;

            if(IsVertical())
            {
                // before starting the vertical animation, let the width autosize
                contentPanel.Width = double.NaN;
            }
            else
            {
                // before starting the horizontal animation, let the height autosize
                contentPanel.Height = double.NaN;
            }

            var storyboard = new Storyboard();
            var animation = new DoubleAnimation()
            {
                Duration = ExpandDuration,
                EnableDependentAnimation = true,
                From = actualDim,
                To = desiredDim,
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 3 },
            };

            Storyboard.SetTarget(animation, contentPanel);
            Storyboard.SetTargetProperty(animation, IsVertical() ? "Height" : "Width");

            var opacityAnimation = new DoubleAnimation()
            {
                Duration = ExpandDuration,
                From = 0,
                To = 1,
                //EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 3 },
            };

            Storyboard.SetTarget(opacityAnimation, contentPanel);
            Storyboard.SetTargetProperty(opacityAnimation, "Opacity");

            storyboard.Children.Add(animation);
            storyboard.Children.Add(opacityAnimation);
            storyboard.Begin();
            storyboard.Completed += (sender, args) =>
            {
                contentPanel.Width = double.NaN;
                contentPanel.Height = double.NaN;
            };
            activeStoryboard = storyboard;
        }
        else
        {
            double actualDim = IsVertical() ? contentPanel.ActualHeight : contentPanel.ActualWidth;
            double desiredDim = 0;

            var storyboard = new Storyboard();
            var animation = new DoubleAnimation()
            {
                Duration = CollapseDuration,
                EnableDependentAnimation = true,
                From = actualDim,
                To = desiredDim,
                EasingFunction = new ExponentialEase{ EasingMode = EasingMode.EaseOut, Exponent = 3 },
            };

            Storyboard.SetTarget(animation, contentPanel);
            Storyboard.SetTargetProperty(animation, IsVertical() ? "Height" : "Width");

            var opacityAnimation = new DoubleAnimation()
            {
                Duration = CollapseDuration,
                From = 1,
                To = 0,
                //EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 3 },
            };

            Storyboard.SetTarget(opacityAnimation, contentPanel);
            Storyboard.SetTargetProperty(opacityAnimation, "Opacity");

            storyboard.Children.Add(animation);
            storyboard.Children.Add(opacityAnimation);
            storyboard.Begin();
            storyboard.Completed += (sender, args) => {
                if(IsVertical())
                {
                    if(latchedWidth > 0)
                    {
                        contentPanel.Width = latchedWidth;
                    }
                }
                else
                {
                    if(latchedHeight > 0)
                    {
                        contentPanel.Height = latchedHeight;
                    }
                }

                measureStack.Visibility = Visibility.Collapsed; 
            };
            activeStoryboard = storyboard;
        }
    }
}
