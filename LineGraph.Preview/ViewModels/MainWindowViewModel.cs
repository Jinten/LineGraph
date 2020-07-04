using Livet;
using Livet.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Media;

namespace LineGraph.Preview.ViewModels
{
    class LineContentViewModel : ViewModel
    {
        public Brush Color
        {
            get => _Color;
            set => RaisePropertyChangedIfSet(ref _Color, value);
        }
        Brush _Color = null;

        public IEnumerable<Point> Points => _Points;
        ObservableCollection<Point> _Points = new ObservableCollection<Point>();

        int _Counter = 0;
        Random _Random = null;
        Point _Offset;

        public LineContentViewModel(Brush color, Point offset)
        {
            Color = color;

            _Offset = offset;
            _Random = new Random();
        }

        public void Update()
        {
            var point = new Point(_Offset.X + _Counter * 10, _Offset.Y + _Random.NextDouble() * 100);

            if (_Points.Count < 100)
            {
                _Points.Add(point);
            }
            else
            {
                _Points[_Counter] = point;
            }

            _Counter = (_Counter + 1) % 100;
        }

        public void Clear()
        {
            _Points.Clear();
            _Counter = 0;
        }
    }

    class MainWindowViewModel : ViewModel
    {
        public Point Offset
        {
            get => _Offset;
            set => RaisePropertyChangedIfSet(ref _Offset, value);
        }
        Point _Offset = new Point(0,0);

        public ViewModelCommand ClearCommand
        {
            get
            {
                if (_ClearCommand == null)
                {
                    _ClearCommand = new ViewModelCommand(Clear);
                }
                return _ClearCommand;
            }
        }
        ViewModelCommand _ClearCommand = null;


        public ViewModelCommand ClosingCommand
        {
            get
            {
                if(_ClosingCommand == null)
                {
                    _ClosingCommand = new ViewModelCommand(Closing);
                }
                return _ClosingCommand;
            }
        }
        ViewModelCommand _ClosingCommand = null;

        public IEnumerable<LineContentViewModel> LineControls => _LineControls;
        ObservableSynchronizedCollection<LineContentViewModel> _LineControls = new ObservableSynchronizedCollection<LineContentViewModel>();

        Timer _Timer = null;

        public MainWindowViewModel()
        {
            if(IsInDesignMode())
            {
                return;
            }
            _LineControls.Add(new LineContentViewModel(Brushes.Pink, new Point(0,0)));
            _LineControls.Add(new LineContentViewModel(Brushes.Green, new Point(0,200)));

            _Timer = new Timer(16);
            _Timer.Elapsed += (sender, e) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _LineControls[0].Update();
                    _LineControls[1].Update();
                });
            };

            _Timer.Start();

            CompositeDisposable.Add(_Timer);
        }

        public bool IsInDesignMode()
        {
            bool result = true;
            var window = Application.Current.MainWindow;

            if (window != null)
            {
                if (!IsInDesignMode(window))
                {
                    result = false;
                }
            }
            return result;
        }

        public bool IsInDesignMode(DependencyObject element)
        {
            return System.ComponentModel.DesignerProperties.GetIsInDesignMode(element);
        }

        void Clear()
        {
            foreach(var control in _LineControls)
            {
                control.Clear();
            }
        }

        void Closing()
        {
            _Timer.Stop();
        }
    }
}
