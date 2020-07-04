﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LineGraph.Controls
{
    /// <summary>
    /// このカスタム コントロールを XAML ファイルで使用するには、手順 1a または 1b の後、手順 2 に従います。
    ///
    /// 手順 1a) 現在のプロジェクトに存在する XAML ファイルでこのカスタム コントロールを使用する場合
    /// この XmlNamespace 属性を使用場所であるマークアップ ファイルのルート要素に
    /// 追加します:
    ///
    ///     xmlns:MyNamespace="clr-namespace:LineGraph.Controls"
    ///
    ///
    /// 手順 1b) 異なるプロジェクトに存在する XAML ファイルでこのカスタム コントロールを使用する場合
    /// この XmlNamespace 属性を使用場所であるマークアップ ファイルのルート要素に
    /// 追加します:
    ///
    ///     xmlns:MyNamespace="clr-namespace:LineGraph.Controls;assembly=LineGraph.Controls"
    ///
    /// また、XAML ファイルのあるプロジェクトからこのプロジェクトへのプロジェクト参照を追加し、
    /// リビルドして、コンパイル エラーを防ぐ必要があります:
    ///
    ///     ソリューション エクスプローラーで対象のプロジェクトを右クリックし、
    ///     [参照の追加] の [プロジェクト] を選択してから、このプロジェクトを参照し、選択します。
    ///
    ///
    /// 手順 2)
    /// コントロールを XAML ファイルで使用します。
    ///
    ///     <MyNamespace:LineGraph/>
    ///
    /// </summary>
    public class LineGraph : MultiSelector
    {
        public double Scale
        {
            get => (double)GetValue(ScaleProperty);
            set => SetValue(ScaleProperty, value);
        }
        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.Register(nameof(Scale), typeof(double), typeof(LineGraph), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ScalePropertyChanged));

        public Point Offset
        {
            get => (Point)GetValue(OffsetProperty);
            set => SetValue(OffsetProperty, value);
        }
        public static readonly DependencyProperty OffsetProperty =
            DependencyProperty.Register(nameof(Offset), typeof(Point), typeof(LineGraph), new FrameworkPropertyMetadata(new Point(0, 0), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OffsetPropertyChanged));

        Canvas Canvas { get; set; } = null;
        List<object> _DelayToBindVMs = new List<object>();

        ScaleTransform _Scale = new ScaleTransform();
        TranslateTransform _Translation = new TranslateTransform();

        bool _IsPressedToMove = false;
        Point _CapturedPoint = new Point();

        static LineGraph()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LineGraph), new FrameworkPropertyMetadata(typeof(LineGraph)));
        }

        public LineGraph()
        {
            SizeChanged += LineGraph_SizeChanged;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            Canvas = GetTemplateChild("__LineGraphCanvas__") as Canvas;

            var transform = new TransformGroup();
            transform.Children.Add(_Scale);
            transform.Children.Add(_Translation);
            Canvas.RenderTransform = transform;

            if (_DelayToBindVMs.Count() > 0)
            {
                foreach (var vm in _DelayToBindVMs)
                {
                    Canvas.Children.Add(new LineControl(vm));
                }
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            if(e.RightButton == MouseButtonState.Pressed)
            {
                _IsPressedToMove = true;
                _CapturedPoint = e.GetPosition(this);
                _CapturedPoint.X -= Offset.X;
                _CapturedPoint.Y -= Offset.Y;
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            if(e.RightButton == MouseButtonState.Released)
            {
                _IsPressedToMove = false;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if(_IsPressedToMove == false)
            {
                return;
            }

            var pos = e.GetPosition(this) - _CapturedPoint;
            pos.X = Math.Min(0, pos.X);

            _Translation.X = pos.X;
            _Translation.Y = pos.Y;

            Offset = new Point(pos.X, pos.Y);

            UpdateScaleCenter();
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            Scale += e.Delta > 0 ? +0.1 : -0.1;

            UpdateScaleCenter();
        }

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);

            if (Canvas == null)
            {
                _DelayToBindVMs.AddRange(newValue.OfType<object>());
            }
            else
            {
                if (oldValue != null)
                {
                    var oldVMs = oldValue.OfType<object>().ToArray();
                    var contents = Canvas.Children.OfType<LineControl>().ToArray();
                    var removeContents = contents.Where(arg => oldVMs.Contains(arg.DataContext)).ToArray();

                    foreach (var removeContent in removeContents)
                    {
                        Canvas.Children.Remove(removeContent);
                    }
                }
                if (newValue != null)
                {
                    var newVMs = newValue.OfType<object>().ToArray();

                    foreach (var vm in newVMs)
                    {
                        Canvas.Children.Add(new LineControl(vm));
                    }
                }
            }
        }

        void LineGraph_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateScaleCenter();
        }

        void UpdateScaleCenter()
        {
            _Scale.CenterX = ActualWidth * 0.5 - Offset.X;
            _Scale.CenterY = ActualHeight * 0.5 - Offset.Y;
        }

        static void ScalePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var lineGraph = d as LineGraph;
            lineGraph._Scale.ScaleX = lineGraph.Scale;
            lineGraph._Scale.ScaleY = lineGraph.Scale;
        }

        static void OffsetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var lineGraph = d as LineGraph;
            lineGraph._Translation.X = lineGraph.Offset.X;
            lineGraph._Translation.Y = lineGraph.Offset.Y;
        }
    }
}
