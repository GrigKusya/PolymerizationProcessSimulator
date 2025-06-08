using PolymerizationProcessSimulator.Models;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace PolymerizationProcessSimulator.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ProcessModel _processModel;
        private string _statusMessage = "Готов";
        private readonly StringBuilder _allRunsLog = new StringBuilder();
        private readonly ObservableCollection<string> _allRecommendations = new ObservableCollection<string>();
        private StringBuilder _currentRunLog = new StringBuilder();
        private ObservableCollection<string> _currentRecommendations = new ObservableCollection<string>();

        public ViewModelLocator ChartData { get; } = ViewModelLocator.Instance;

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ReactorState => _allRunsLog.ToString();
        public string Recommendations => string.Join("\n", _allRecommendations);

        public ICommand InputParamsCommand { get; }
        public ICommand StartProcessCommand { get; }
        public ICommand ShowChartsCommand { get; }
        public ICommand ClearHistoryCommand { get; }

        public MainViewModel()
        {
            _processModel = new ProcessModel();

            InputParamsCommand = new RelayCommand(ExecuteInputParams);
            StartProcessCommand = new RelayCommand(ExecuteStartProcess);
            ShowChartsCommand = new RelayCommand(ExecuteShowCharts);
            ClearHistoryCommand = new RelayCommand(ExecuteClearHistory);
        }

        private void ExecuteInputParams(object parameter)
        {
            var inputDialog = new InputParamsDialog();
            if (inputDialog.ShowDialog() == true)
            {
                _processModel.SetParameters(
                    inputDialog.ME, inputDialog.DE, inputDialog.IE, inputDialog.F,
                    inputDialog.CB, inputDialog.T, inputDialog.R, inputDialog.B,
                    inputDialog.PB, inputDialog.H, inputDialog.Y);
            }
        }

        private void ExecuteStartProcess(object parameter)
        {
            _currentRunLog = new StringBuilder();
            _currentRecommendations = new ObservableCollection<string>();

            StatusMessage = "Процесс запущен...";

            Task.Run(() =>
            {
                var result = _processModel.RunProcess(
                    message => Application.Current.Dispatcher.Invoke(() =>
                    {
                        _currentRunLog.AppendLine(message);
                        _allRunsLog.AppendLine(message);
                        OnPropertyChanged(nameof(ReactorState));
                    }),
                    status => Application.Current.Dispatcher.Invoke(() =>
                    {
                        StatusMessage = status;
                    }),
                    recommendation => Application.Current.Dispatcher.Invoke(() =>
                    {
                        _currentRecommendations.Add(recommendation);
                        _allRecommendations.Add(recommendation);
                        OnPropertyChanged(nameof(Recommendations));
                    }));

                if (!string.IsNullOrEmpty(result))
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        StatusMessage = result;
                        if (result.Contains("Достигнут"))
                        {
                            var finalRecs = new[] {
                                "ЗАДАННЫЙ ВЫХОД ПЭ! ПРОВЕРЬТЕ, СООТВЕТСТВУЮТ ЛИ",
                                "СВОЙСТВА ПОЛИМЕРА ЗАДАННЫМ? Если нет, измените параметры"
                            };
                            foreach (var rec in finalRecs)
                            {
                                _currentRecommendations.Add(rec);
                                _allRecommendations.Add(rec);
                            }
                            OnPropertyChanged(nameof(Recommendations));
                        }
                    });
                }
            });
        }

        private void ExecuteShowCharts(object parameter)
        {
            var chartWindow = new ChartWindow();
            chartWindow.Show();
        }

        private void ExecuteClearHistory(object parameter)
        {
            _allRunsLog.Clear();
            _allRecommendations.Clear();
            OnPropertyChanged(nameof(ReactorState));
            OnPropertyChanged(nameof(Recommendations));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}