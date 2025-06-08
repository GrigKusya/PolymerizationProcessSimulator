//ProcessModel.cs:
using System;
using System.Text;
using PolymerizationProcessSimulator;

namespace PolymerizationProcessSimulator.Models
{
    public class ProcessModel
    {
        // Входные параметры (переименованы в соответствии со старым проектом)
        public double ME { get; private set; }    // ВЫХОД ПОЛИМЕРА (г)
        public double DE { get; private set; }    // ПЛОТНОСТЬ (г/см³)
        public double IE { get; private set; }    // ИНДЕКС РАСПЛАВА (1-20 г/10 мин)
        public double F { get; private set; }     // РЕЖИМ ПРОЦЕССА: 5-стац., 0-нестац.
        public double CB { get; private set; }    // КАТАЛИЗАТОР (0.03-0.1 г)
        public double C { get; private set; }     // C=9.88*CB
        public double T { get; private set; }     // ТЕМПЕРАТУРА ТЕРМОСТАТА (75-100 °C)
        public double R { get; private set; }     // СКОРОСТЬ ТЕПЛОСЪЕМА (0-12 л/мин)
        public double B { get; private set; }     // ИЗОБУТИЛЕН (0-6%)
        public double PB { get; private set; }    // ДАВЛЕНИЕ ЭТИЛЕНА (5-40 МПа)
        public double P { get; private set; }     // P=10*PB
        public double H { get; private set; }     // ВОДОРОД (0-10%)
        public double Y { get; private set; }     // СКОРОСТЬ МЕШАЛКИ (100-1000 об/мин)

        // Переменные реактора
        public double Z { get; private set; }     // время процесса
        public double S { get; private set; }     // температура реактора
        public double M { get; private set; }     // накопление полиэтилена
        public double W { get; private set; }     // степень полимеризации
        public double D { get; private set; }     // плотность полиэтилена
        public double E { get; private set; }     // индекс расплава

        public void SetParameters(double me, double de, double ie, double f,
                                double cb, double t, double r, double b,
                                double pb, double h, double y)
        {
            ME = me;
            DE = de;
            IE = ie;
            F = f;
            CB = cb;
            C = 9.88 * cb;
            T = t;
            R = r;
            B = b;
            PB = pb;
            P = 10 * pb;
            H = h;
            Y = y;
        }

        public string RunProcess(Action<string> updateReactorState,
                                Action<string> updateStatus,
                                Action<string> addRecommendation)
        {
            if (CheckData(out string errorMessage, addRecommendation))
            {
                updateStatus(errorMessage);
                return errorMessage;
            }
            updateReactorState("========== НОВЫЙ ЗАПУСК ПРОЦЕССА ==========");
            // Инициализация
            double Q = 0;
            Z = 0;
            double GDS = 0.01;
            double J = 0;

            while (true)
            {
                double Z5 = 0;
                if (CheckData(out errorMessage, addRecommendation))
                {
                    updateStatus(errorMessage);
                    return errorMessage;
                }

                while (true)
                {
                    double currentBuf = (P * C / (203 + R * 1000) + 1) * T;
                    S = T + (currentBuf - T) * (1 - GDS);

                    if (S > 102)
                    {
                        addRecommendation("СНИЗИТЬ ТЕМПЕРАТУРУ");
                        updateStatus("Остановка: Превышена температура");
                        return "Остановка: Превышена температура";
                    }

                    J = (P - P * 28 / S) * C * (1 - 0.005 * (5.1 * H + 13.5 * B)) / 10;
                    if (P / J < 1.3)
                    {
                        addRecommendation("ПОВЫСИТЬ ДАВЛЕНИЕ ЭТИЛЕНА");
                        updateStatus("Остановка: Низкое давление этилена");
                        return "Остановка: Низкое давление этилена";
                    }

                    J = J * 305 * 1000 * 100 * Math.Exp(-6429 / (273 + S));
                    if (P / J < 1.3)
                    {
                        addRecommendation("ПОВЫСИТЬ ДАВЛЕНИЕ ЭТИЛЕНА");
                        updateStatus("Остановка: Низкое давление этилена");
                        return "Остановка: Низкое давление этилена";
                    }

                    J = J * (2 * 0.001 * Y - Math.Pow(0.001 * Y, 2) + 0.001);
                    J = J * (1 - GDS);
                    Q = Q + 0.94 * J / C;
                    M = Q * C;
                    Z5 += 1;

                    if (F != 5)
                    {
                        P = P - J;
                    }

                    if (Z5 >= 10) break;
                }

                if (P < 99)
                {
                    addRecommendation("ПОВЫСИТЬ ДАВЛЕНИЕ ЭТИЛЕНА");
                }

                double G = (0.0007 * S / 4 - 0.01);
                D = 1.54 * G + (1 + Q) / ((0.01 * B + 1.05) * Q + 0.5);
                GDS = GDS + 0.03 + 0.0001 * T;
                if (GDS > 1) GDS = 1;

                W = 2.32 * 5 / ((0.0007 * S - 0.05) / Q + (0.0007 * S / 4 - 0.01) * (H / 10 + 1));
                Z += 10;

                double buf = (-0.8) * Math.Log(W / 2.32);
                buf = Math.Exp(buf);
                E = (buf / 0.0007 - 3.8);

                // Вывод информации (как в старом проекте)
                updateReactorState($"РЕАКТОР мин {Z}");
                updateReactorState($"град.Ц. Т = {S:F2}");
                updateReactorState($"ЭТИЛЕН МПа {P / 10:F2}");
                updateReactorState($"НАКОПЛЕНИЕ ПОЛИЭТИЛЕНА г. {M:F2}");
                updateReactorState($"СТЕПЕНЬ ПОЛИМЕРИЗАЦИИ {W:F2}");
                updateReactorState($"ПЛОТНОСТЬ ПЭ г.см3 {D:F4}");
                updateReactorState($"ИНДЕКС РАСПЛАВА г/10 мин {E:F2}");
                updateReactorState("-------------------------------------------------------");

                // Добавление данных на графики
                ViewModelLocator.Instance.AddDataPoint(Z, S, P / 10, M, W, D, E);

                if (M >= ME)
                {
                    addRecommendation("ЗАДАННЫЙ ВЫХОД ПЭ! ПРОВЕРЬТЕ, СООТВЕТСТВУЮТ ЛИ");
                    addRecommendation("СВОЙСТВА ПОЛИМЕРА ЗАДАННЫМ? Если нет, измените параметры");
                    updateStatus("Процесс завершен: Достигнут заданный выход ПЭ");
                    return "Процесс завершен: Достигнут заданный выход ПЭ";
                }

                if (Z >= 240)
                {
                    addRecommendation("НЕ ДОСТИГНУТ ЗАДАННЫЙ ВЫХОД ПЭ!");
                    updateStatus("Остановка: Превышено время процесса");
                    return "Остановка: Превышено время процесса";
                }
            }
        }

        private bool CheckData(out string errorMessage, Action<string> addRecommendation)
        {
            StringBuilder sb = new StringBuilder();
            bool hasError = false;

            if (P < 99)
            {
                string msg = "ПОВЫСИТЬ ДАВЛЕНИЕ ЭТИЛЕНА";
                addRecommendation?.Invoke(msg);
                sb.AppendLine(msg);
                hasError = true;
            }
            if (C >= 0.98)
            {
                string msg = "СНИЗИТЬ ВЕС КАТАЛИЗАТОРА";
                addRecommendation?.Invoke(msg);
                sb.AppendLine(msg);
                hasError = true;
            }

            errorMessage = sb.ToString();
            return hasError;
        }

        public string GetParametersInfo()
        {
            return $"Выход полиэтилена: {ME:F2} г\n" +
                   $"Плотность: {DE:F4} г/см³\n" +
                   $"Индекс расплава: {IE:F2} г/10 мин\n" +
                   $"Режим процесса: {(F == 5 ? "стационарный" : "нестационарный")}\n" +
                   $"Катализатор: {CB:F4} г\n" +
                   $"Температура термостата: {T:F2} °C\n" +
                   $"Скорость теплоотвода: {R:F2} л/мин\n" +
                   $"Изобутилен: {B:F2}%\n" +
                   $"Давление этилена: {PB:F2} МПа\n" +
                   $"Водород: {H:F2}%\n" +
                   $"Скорость мешалки: {Y:F0} об/мин";
        }
    }
}