using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace LineGraph.Controls
{
    public class LineControl : Control
    {
        public bool Transacting
        {
            get => (bool)GetValue(TransactingProperty);
            set => SetValue(TransactingProperty, value);
        }
        public static readonly DependencyProperty TransactingProperty =
            DependencyProperty.Register(nameof(Transacting), typeof(bool), typeof(LineControl), new FrameworkPropertyMetadata(false, TransactingPropertyChanged));

        public double StrokeThickness
        {
            get => (double)GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }
        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register(nameof(StrokeThickness), typeof(double), typeof(LineControl), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender, StrokeThicknessPropertyChanged));

        public IEnumerable Points
        {
            get => (IEnumerable)GetValue(PointsProperty);
            set => SetValue(PointsProperty, value);
        }
        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register(nameof(Points), typeof(IEnumerable), typeof(LineControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, PointsPropertyChanged));

        Pen _LinePen = null;

        public LineControl(object dataContext)
        {
            DataContext = dataContext;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if(Points == null)
            {
                return;
            }

            base.OnRender(drawingContext);

            if (_LinePen == null)
            {
                UpdateLinePen();
            }

            var points = Points.OfType<Point>().ToArray();
            if (points.Length < 2)
            {
                return;
            }

            int n = points.Length - 1;
            for (int i = 0; i < n; ++i)
            {
                drawingContext.DrawLine(_LinePen, points[i + 0], points[i + 1]);
            }
        }

        static void TransactingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as LineControl;
            if((bool)e.NewValue == false)
            {
                control.InvalidateVisual();
            }
        }

        void UpdateLinePen()
        {
            _LinePen = new Pen(Background, StrokeThickness);
        }

        void PointCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Transacting == false)
            {
                InvalidateVisual();
            }
        }

        static void StrokeThicknessPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as LineControl).UpdateLinePen();
        }

        static void PointsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var lineControl = d as LineControl;

            if(e.OldValue != null && e.OldValue is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= lineControl.PointCollectionChanged;
            }
            if(e.NewValue != null && e.NewValue is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += lineControl.PointCollectionChanged;
            }
        }
    }
}
