// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HeightAnimationTest;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class BlankPage1 : Page
{
    public static readonly DependencyProperty SpecialHeightProperty =
        DependencyProperty.Register(nameof(SpecialHeight), typeof(double), typeof(BlankPage1), new PropertyMetadata(0.0));

    public double SpecialHeight
    {
        get
        {
            return (double)GetValue(SpecialHeightProperty);
        }
        set
        {
            SetValue(SpecialHeightProperty, value);
        }
    }

    public BlankPage1()
    {
        this.InitializeComponent();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        //// Measure the StackPanel with an infinite height constraint
        //MyStackPanel.Measure(new Windows.Foundation.Size(double.PositiveInfinity, double.PositiveInfinity)); 
        //// Get the desired height
        //double desiredHeight = MyStackPanel.DesiredSize.Height;
        //// Display the desired height
        //System.Diagnostics.Debug.WriteLine($"The StackPanel wants to be {desiredHeight} pixels high.");
        ////HeightAnimation.Begin();

        //var storyboard = new Storyboard();
        //var animation = new DoubleAnimation()
        //{
        //    EnableDependentAnimation = true,
        //    From = 0,
        //    To = desiredHeight,
        //    EasingFunction = new BounceEase()
        //};

        //Storyboard.SetTarget(animation, MyPanel); 
        //Storyboard.SetTargetProperty(animation, "Height");
        //storyboard.Children.Add(animation);
        //storyboard.Begin();
    }
}
