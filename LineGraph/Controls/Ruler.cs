using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LineGraph.Controls
{
    internal class Ruler : ContentControl
    {
        public double ParentWidth
        {
            get => (double)GetValue(ParentWidthProperty);
            set => SetValue(ParentWidthProperty, value);
        }
        public static readonly DependencyProperty ParentWidthProperty =
            DependencyProperty.Register(nameof(ParentWidth), typeof(double), typeof(Ruler), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public double Scale
        {
            get => (double)GetValue(ScaleProperty);
            set => SetValue(ScaleProperty, value);
        }
        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.Register(nameof(Scale), typeof(double), typeof(Ruler), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public Point ScaleCenter
        {
            get => (Point)GetValue(ScaleCenterProperty);
            set => SetValue(ScaleCenterProperty, value);
        }
        public static readonly DependencyProperty ScaleCenterProperty =
            DependencyProperty.Register(nameof(ScaleCenter), typeof(Point), typeof(Ruler), new FrameworkPropertyMetadata(new Point()));

        public Point Offset
        {
            get => (Point)GetValue(OffsetProperty);
            set => SetValue(OffsetProperty, value);
        }
        public static readonly DependencyProperty OffsetProperty =
            DependencyProperty.Register(nameof(Offset), typeof(Point), typeof(Ruler), new FrameworkPropertyMetadata(new Point(0, 0), FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush ScaleColor
        {
            get => (Brush)GetValue(ScaleColorProperty);
            set => SetValue(ScaleColorProperty, value);
        }
        public static readonly DependencyProperty ScaleColorProperty =
            DependencyProperty.Register(nameof(ScaleColor), typeof(Brush), typeof(Ruler), new FrameworkPropertyMetadata(Brushes.Black, ScaleColorPropertyChanged));

        public Brush LineColor
        {
            get => (Brush)GetValue(LineColorProperty);
            set => SetValue(LineColorProperty, value);
        }
        public static readonly DependencyProperty LineColorProperty =
            DependencyProperty.Register(nameof(LineColor), typeof(Brush), typeof(Ruler), new FrameworkPropertyMetadata(Brushes.Black, LineColorPropertyChanged));

        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(Ruler), new FrameworkPropertyMetadata(Orientation.Horizontal));

        Pen _ScalePen = null;
        Pen _LinePen = null;
        static Typeface Typeface = new Typeface("Verdana");
        static double LineOffset = 3;
        static double LineLength = 5 + LineOffset;
        static double LineDistance = 100;
        static double SubLineOffset = 1;
        static double SubLineLength = 5 + SubLineOffset;
        static double SubLineDistance = LineDistance / 10;

        static void ScaleColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as Ruler).UpdateScalePen();
        }

        static void LineColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as Ruler).UpdateLinePen();
        }

        static Ruler()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Ruler), new FrameworkPropertyMetadata(typeof(Ruler)));
        }

        public Ruler()
        {

        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (_ScalePen == null)
            {
                UpdateScalePen();
            }

            switch (Orientation)
            {
                case Orientation.Horizontal:
                    OnRenderHorizontal(drawingContext);
                    break;
                case Orientation.Vertical:
                    OnRenderVertical(drawingContext);
                    break;
            }
        }

        void OnRenderHorizontal(DrawingContext dc)
        {
            double s = Math.Max(Scale, 1);
            double numScale = Math.Max(1.0 / Scale, 1);

            int init = (int)(-Offset.X / LineDistance) - 1;
            int count = (int)((-Offset.X + ActualWidth) / LineDistance) + 1;
            for (int i = init; i < count; ++i)
            {
                double num = i * LineDistance;
                double x = (num + Offset.X) * s;
                dc.DrawLine(_ScalePen, new Point(x, LineOffset), new Point(x, LineLength));

                for (int j = 1; j < 10; ++j)
                {
                    double sub_x = x + j * SubLineDistance * s;
                    dc.DrawLine(_ScalePen, new Point(sub_x, SubLineOffset), new Point(sub_x, SubLineLength));
                }

                int numText = (int)(num * numScale);
                var text = new FormattedText($"{numText}", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface, 8, ScaleColor, 1.0);
                dc.DrawText(text, new Point(x - text.Width * 0.5, LineLength));
            }
        }

        void OnRenderVertical(DrawingContext dc)
        {
            double s = Math.Max(Scale, 1);
            double numScale = Math.Max(1.0 / Scale, 1);

            int init = (int)(-Offset.Y / LineDistance - numScale * 2) - 1;
            int count = (int)((-Offset.Y + ActualHeight * numScale) / LineDistance) + 1;
            for (int i = init; i < count; ++i)
            {
                double y = i * LineDistance;
                dc.DrawLine(_ScalePen, new Point(LineOffset, y), new Point(LineLength, y));

                for (int j = 1; j < 10; ++j)
                {
                    double sub_y = y + j * SubLineDistance;
                    dc.DrawLine(_ScalePen, new Point(SubLineOffset, sub_y), new Point(SubLineLength, sub_y));
                }

                int numText = (int)(y * numScale);
                var text = new FormattedText($"{numText}", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface, 8, ScaleColor, 1.0);
                var text_y = y - text.Height * 0.5;
                dc.DrawText(text, new Point(LineLength + LineOffset, text_y));

                var line_y = text_y + text.Height * 0.5;
                var line_x = LineLength + LineOffset + text.Width + 4;
                dc.DrawLine(_LinePen, new Point(line_x, line_y), new Point(ParentWidth, line_y));
            }
        }

        void UpdateScalePen()
        {
            _ScalePen = new Pen(ScaleColor, 1);
            _ScalePen.Freeze();
        }

        void UpdateLinePen()
        {
            _LinePen = new Pen(LineColor, 1);
            _LinePen.Freeze();
        }
    }
}
