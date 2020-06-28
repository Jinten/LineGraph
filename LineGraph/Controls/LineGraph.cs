using System;
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
        Canvas Canvas { get; set; } = null;
        List<object> _DelayToBindVMs = new List<object>();

        static LineGraph()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LineGraph), new FrameworkPropertyMetadata(typeof(LineGraph)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            Canvas = GetTemplateChild("__LineGraphCanvas__") as Canvas;

            if (_DelayToBindVMs.Count() > 0)
            {
                foreach (var vm in _DelayToBindVMs)
                {
                    Canvas.Children.Add(new LineControl(vm));
                }
            }
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
    }
}
