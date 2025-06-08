//ViewModelLocator.cs
using System;
using System.Collections.Generic;
using System.Linq;
using LiveCharts;
using LiveCharts.Wpf;

namespace PolymerizationProcessSimulator
{
    public class ViewModelLocator
    {
        private static ViewModelLocator? _instance;
        public static ViewModelLocator Instance => _instance ??= new ViewModelLocator();

        public SeriesCollection TemperatureSeries { get; set; }
        public SeriesCollection PressureSeries { get; set; }
        public SeriesCollection PolymerSeries { get; set; }
        public SeriesCollection PolymerizationSeries { get; set; }
        public SeriesCollection DensitySeries { get; set; }
        public SeriesCollection MeltIndexSeries { get; set; }

        private readonly List<double> _timeValues = new();
        private readonly List<double> _temperatureValues = new();
        private readonly List<double> _pressureValues = new();
        private readonly List<double> _polymerValues = new();
        private readonly List<double> _polymerizationValues = new();
        private readonly List<double> _densityValues = new();
        private readonly List<double> _meltIndexValues = new();
        public Func<double, string> XFormatter { get; set; }
        public Func<double, string> YFormatter { get; set; }

        public ViewModelLocator()
        {
            XFormatter = value => value.ToString("N1");
            YFormatter = value => value.ToString("N2");

            TemperatureSeries = new SeriesCollection
    {
        new LineSeries
        {
            Title = "Температура",
            Values = new ChartValues<double>(),
            PointGeometry = DefaultGeometries.Circle,
            PointGeometrySize = 5,
            LineSmoothness = 0 // Отключаем сглаживание
        }
    };

            PressureSeries = new SeriesCollection
    {
        new LineSeries
        {
            Title = "Давление",
            Values = new ChartValues<double>(),
            PointGeometry = DefaultGeometries.Circle,
            PointGeometrySize = 5,
            LineSmoothness = 0 // Отключаем сглаживание
        }
    };

            PolymerSeries = new SeriesCollection
    {
        new LineSeries
        {
            Title = "Накопление ПЭ",
            Values = new ChartValues<double>(),
            PointGeometry = DefaultGeometries.Circle,
            PointGeometrySize = 5,
            LineSmoothness = 0 // Отключаем сглаживание
        }
    };

            PolymerizationSeries = new SeriesCollection
    {
        new LineSeries
        {
            Title = "Степень полимеризации",
            Values = new ChartValues<double>(),
            PointGeometry = DefaultGeometries.Circle,
            PointGeometrySize = 5,
            LineSmoothness = 0 // Отключаем сглаживание
        }
    };

            DensitySeries = new SeriesCollection
    {
        new LineSeries
        {
            Title = "Плотность",
            Values = new ChartValues<double>(),
            PointGeometry = DefaultGeometries.Circle,
            PointGeometrySize = 5,
            LineSmoothness = 0 // Отключаем сглаживание
        }
    };

            MeltIndexSeries = new SeriesCollection
    {
        new LineSeries
        {
            Title = "Индекс расплава",
            Values = new ChartValues<double>(),
            PointGeometry = DefaultGeometries.Circle,
            PointGeometrySize = 5,
            LineSmoothness = 0 // Отключаем сглаживание
        }
    };
        }

        // Изменить метод AddDataPoint:
        public void AddDataPoint(double time, double temp, double pressure,
               double polymer, double polymerization,
               double density, double meltIndex)
        {
            // Добавляем текущие данные
            _timeValues.Add(time);
            _temperatureValues.Add(temp);
            _pressureValues.Add(pressure);
            _polymerValues.Add(polymer);
            _polymerizationValues.Add(polymerization);
            _densityValues.Add(density);
            _meltIndexValues.Add(meltIndex);

            // Обновляем серии (теперь без очистки)
            UpdateAllSeries();
        }

        private void UpdateAllSeries()
        {
            UpdateSeries(TemperatureSeries, _temperatureValues);
            UpdateSeries(PressureSeries, _pressureValues);
            UpdateSeries(PolymerSeries, _polymerValues);
            UpdateSeries(PolymerizationSeries, _polymerizationValues);
            UpdateSeries(DensitySeries, _densityValues);
            UpdateSeries(MeltIndexSeries, _meltIndexValues);
        }

        private static void UpdateSeries(SeriesCollection series, List<double> values)
        {
            if (series[0].Values is ChartValues<double> chartValues)
            {
                // Убрали Clear(), чтобы сохранять все точки
                chartValues.AddRange(values.Skip(chartValues.Count)); // Добавляем только новые точки
            }
        }

        private static void ClearSeries(SeriesCollection series)
        {
            if (series[0].Values is ChartValues<double> chartValues)
            {
                chartValues.Clear();
            }
        }
    }
}