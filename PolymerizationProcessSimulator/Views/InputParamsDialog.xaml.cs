//InputParamsDialog.xaml.cs
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace PolymerizationProcessSimulator
{
    public partial class InputParamsDialog : Window
    {
        private readonly TextBox[] _inputBoxes = new TextBox[11];
        private readonly Slider[] _sliders = new Slider[11];

        public double ME { get; set; }    // ВЫХОД ПОЛИМЕРА (г)
        public double DE { get; set; }    // ПЛОТНОСТЬ (г/см³)
        public double IE { get; set; }    // 1 < ИНДЕКС РАСПЛАВА < 20 (г/10 мин)
        public double F { get; set; }     // РЕЖИМ ПРОЦЕССА: стационарный - 5, нестационарный - 0
        public double CB { get; set; }    // 0,03 < катализатор < 0,1 г;
        public double T { get; set; }     // 75 < ТЕМПЕРАТУРА ТЕРМОСТАТА < 100
        public double R { get; set; }     // 0 =< СКОРОСТЬ ТЕПЛОСЪЕМА < 12 л/мин
        public double B { get; set; }     // 0 =< ИЗОБУТИЛЕН <6%
        public double PB { get; set; }    // 5 < ЭТИЛЕН < 40 Мпа
        public double H { get; set; }     // 0 =< водород < 10%
        public double Y { get; set; }     // 100 < мешалка < 1000 об/мин

        public InputParamsDialog()
        {
            InitializeComponent();
            CreateParameterInputs();
        }

        private void CreateParameterInputs()
        {
            string[] captions = {
                "Выход полиэтилена [10-100 г]:",
                "Плотность [0,90-0,96 г/см³]:",
                "Индекс расплава [1-20 (г/10 мин)]:",
                "Режим процесса (5-стац., 0-нестац.):",
                "Катализатор [0,03-0,10 г]:",
                "Температура термостата [75-100 °C]:",
                "Скорость теплоотвода [0-12 л/мин]:",
                "Изобутилен [0-6%]:",
                "Давление этилена [5-40 МПа]:",
                "Водород [0-10%]:",
                "Скорость мешалки [100-1000 об/мин]:"
            };

            double[,] ranges = {
                {10, 100},      // ME
                {0.90, 0.96},   // DE
                {1, 20},        // IE
                {0, 5},        // F
                {0.03, 0.10},   // CB
                {75, 100},      // T
                {0, 12},       // R
                {0, 6},         // B
                {5, 40},       // PB
                {0, 10},        // H
                {100, 1000}     // Y
            };

            for (int i = 0; i < captions.Length; i++)
            {
                // Номер строки
                var numberLabel = new TextBlock
                {
                    Text = (i + 1).ToString(),
                    Margin = new Thickness(5),
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetRow(numberLabel, i + 1);
                Grid.SetColumn(numberLabel, 0);
                ParametersGrid.Children.Add(numberLabel);

                // Описание параметра
                var descLabel = new TextBlock
                {
                    Text = captions[i],
                    Margin = new Thickness(5),
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetRow(descLabel, i + 1);
                Grid.SetColumn(descLabel, 1);
                ParametersGrid.Children.Add(descLabel);

                // Поле ввода
                var inputBox = new TextBox
                {
                    Margin = new Thickness(5),
                    VerticalAlignment = VerticalAlignment.Center,
                    Text = ranges[i, 0].ToString()
                };
                Grid.SetRow(inputBox, i + 1);
                Grid.SetColumn(inputBox, 2);
                ParametersGrid.Children.Add(inputBox);
                _inputBoxes[i] = inputBox;

                // Слайдер (кроме режима процесса)
                if (i != 3)
                {
                    var slider = new Slider
                    {
                        Minimum = ranges[i, 0],
                        Maximum = ranges[i, 1],
                        Value = ranges[i, 0],
                        Margin = new Thickness(5),
                        VerticalAlignment = VerticalAlignment.Center,
                        AutoToolTipPlacement = AutoToolTipPlacement.TopLeft,
                        AutoToolTipPrecision = 2
                    };

                    // Для катализатора (индекс 4)
                    if (i == 4)
                    {
                        slider.TickFrequency = 0.0001;
                        slider.IsSnapToTickEnabled = true;
                        slider.AutoToolTipPrecision = 4;
                        slider.ValueChanged += (s, e) =>
                        {
                            inputBox.Text = slider.Value.ToString("F4");
                        };
                    }
                    // Для скорости мешалки (индекс 10)
                    else if (i == 10)
                    {
                        slider.TickFrequency = 1;
                        slider.IsSnapToTickEnabled = true;
                        slider.AutoToolTipPrecision = 0;
                        slider.ValueChanged += (s, e) =>
                        {
                            inputBox.Text = slider.Value.ToString("F0");
                        };
                    }
                    // Для плотности (индекс 1)
                    else if (i == 1)
                    {
                        slider.TickFrequency = 0.0001;
                        slider.IsSnapToTickEnabled = true;
                        slider.AutoToolTipPrecision = 4;
                        slider.ValueChanged += (s, e) =>
                        {
                            inputBox.Text = slider.Value.ToString("F4");
                        };
                    }
                    // Для остальных параметров
                    else
                    {
                        slider.ValueChanged += (s, e) =>
                        {
                            inputBox.Text = slider.Value.ToString("F2");
                        };
                    }

                    Grid.SetRow(slider, i + 1);
                    Grid.SetColumn(slider, 3);
                    ParametersGrid.Children.Add(slider);
                    _sliders[i] = slider;
                }
                else
                {
                    // Для режима процесса добавляем RadioButton вместо слайдера
                    var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };
                    var rbStationary = new RadioButton { Content = "Стационарный", Margin = new Thickness(5) };
                    var rbNonStationary = new RadioButton { Content = "Нестационарный", Margin = new Thickness(5) };

                    rbStationary.Checked += (s, e) => inputBox.Text = "5";
                    rbNonStationary.Checked += (s, e) => inputBox.Text = "0";
                    rbStationary.IsChecked = true;

                    stackPanel.Children.Add(rbStationary);
                    stackPanel.Children.Add(rbNonStationary);
                    Grid.SetRow(stackPanel, i + 1);
                    Grid.SetColumn(stackPanel, 3);
                    ParametersGrid.Children.Add(stackPanel);
                }
            }
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateAndSaveInputs())
            {
                DialogResult = true;
                Close();
            }
        }

        private bool ValidateAndSaveInputs()
        {
            try
            {
                ME = double.Parse(_inputBoxes[0].Text);
                DE = double.Parse(_inputBoxes[1].Text);
                IE = double.Parse(_inputBoxes[2].Text);
                F = double.Parse(_inputBoxes[3].Text);
                CB = double.Parse(_inputBoxes[4].Text);
                T = double.Parse(_inputBoxes[5].Text);
                R = double.Parse(_inputBoxes[6].Text);
                B = double.Parse(_inputBoxes[7].Text);
                PB = double.Parse(_inputBoxes[8].Text);
                H = double.Parse(_inputBoxes[9].Text);
                Y = double.Parse(_inputBoxes[10].Text);

                // Дополнительная валидация диапазонов
                if (IE < 1 || IE > 20)
                {
                    MessageBox.Show("Индекс расплава должен быть между 1 и 20");
                    return false;
                }
                if (CB < 0.03 || CB > 0.1)
                {
                    MessageBox.Show("Катализатор должен быть между 0.03 и 0.1 г");
                    return false;
                }
                if (T < 75 || T > 100)
                {
                    MessageBox.Show("Температура термостата должна быть между 75 и 100 °C");
                    return false;
                }
                // Добавьте другие проверки по аналогии

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка ввода данных: {ex.Message}");
                return false;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateAndSaveInputs())
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter("Data.ini"))
                    {
                        writer.WriteLine(ME);
                        writer.WriteLine(DE);
                        writer.WriteLine(IE);
                        writer.WriteLine(F);
                        writer.WriteLine(CB);
                        writer.WriteLine(T);
                        writer.WriteLine(R);
                        writer.WriteLine(B);
                        writer.WriteLine(PB);
                        writer.WriteLine(H);
                        writer.WriteLine(Y);
                    }
                    MessageBox.Show("Параметры успешно сохранены");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка сохранения: {ex.Message}");
                }
            }
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string[] lines = ReadFileWithEncoding("Data.ini");
                if (lines.Length >= 11)
                {
                    for (int i = 0; i < 11; i++)
                    {
                        _inputBoxes[i].Text = lines[i];
                        if (_sliders[i] != null)
                        {
                            _sliders[i].Value = double.Parse(lines[i]);
                        }
                    }
                    MessageBox.Show("Параметры успешно загружены");
                }
                else
                {
                    MessageBox.Show("Файл параметров поврежден");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }

        public static string[] ReadFileWithEncoding(string filePath)
        {
            try
            {
                return File.ReadAllLines(filePath, Encoding.UTF8);
            }
            catch
            {
                return File.ReadAllLines(filePath, Encoding.GetEncoding(1251));
            }
        }
    }
}