//ChartWindow.xaml.cs
using System.Windows;

namespace PolymerizationProcessSimulator
{
    public partial class ChartWindow : Window
    {
        public ChartWindow()
        {
            InitializeComponent();
            DataContext = ViewModelLocator.Instance;
        }
    }
}   