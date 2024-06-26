using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive.Linq;
using System;

namespace ES_PercentCalculator.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            InputData = new();
            Current = new(InputData, (18.2, 17.2, 16.7));
            New_Period1 = new(InputData, (14.2, 13.2, 12.7));
            New_Period2 = new(InputData, Current, New_Period1);
        }
        [Reactive] public Input InputData { get; set; }
        [Reactive] public Class_Current Current { get; set; }
        [Reactive] public Class_New_Period1 New_Period1 { get; set; }
        [Reactive] public Class_New_Period2 New_Period2 { get; set; }
        
        // Существующая программа сбера
        public class Input : ReactiveObject
        {
            public const int MinimumWage = 15453;                                           // минимальный прожиточный минимум

            [Reactive] public int PropertyValue { get; set; } = 3000000;                    // стоимость недвижимости
            [Reactive] public int InitialFee { get; set; } = 1000000;                       // первоначальный взнос
            [Reactive] public int CreditTerm { get; set; } = 4;                             // срок кредита (1-30 лет)
            [Reactive] public int Rent { get; set;} = 25000;
            [Reactive] public int ConstructionPeriod { get; set;} = 3;
            [Reactive] public bool CitizenshipRF { get; set; }
            [Reactive] public int Age { get; set; }
            [Reactive] public int Salary { get; set; }
            [Reactive] public int Experience { get; set; }
            [ObservableAsProperty] public int InitialFeeMin { get; }                        // минимальное значение первоначального взноса в зависимости от стоимости недвижимости
            [ObservableAsProperty] public int InitialFeeMax { get; }                        // максимальное значение первоначального взноса в зависимости от стоимости недвижимости
            [ObservableAsProperty] public int MinimumCreditTerm { get; }                    // срок кредита (1-30 лет)
            [ObservableAsProperty] public string? MinimumCreditTerm_Performance { get; }    // срок кредита (1-30 лет)
            [ObservableAsProperty] public string? InitialFeeMin_Performance { get; }        // минимальное значение первоначального взноса в зависимости от стоимости недвижимости
            [ObservableAsProperty] public string? InitialFeeMax_Performance { get; }        // максимальное значение первоначального взноса в зависимости от стоимости недвижимости
            [ObservableAsProperty] public double InitialFeeInPercent { get; }               // первоначальный взнос (в процентах)
            [ObservableAsProperty] public string? InitialFeeInPercent_Performance { get; }  // первоначальный взнос (в процентах)
            public Input()
            {
                this.WhenAnyValue(vm => vm.PropertyValue)
                    .Select(x => (int)(x * 0.151))
                    .ToPropertyEx(this, vm => vm.InitialFeeMin);

                this.WhenAnyValue(vm => vm.InitialFeeMin)
                    .Select(value => value >= 1000000 ? $"{value / 1000000.0:F1} млн. ₽" : $"{value / 1000.0:F1} тыс. ₽")
                    .ToPropertyEx(this, vm => vm.InitialFeeMin_Performance);

                this.WhenAnyValue(vm => vm.PropertyValue)
                    .Select(x => x - 300000)
                    .ToPropertyEx(this, vm => vm.InitialFeeMax);

                this.WhenAnyValue(vm => vm.InitialFeeMax)
                    .Select(value => value >= 1000000 ? $"{value / 1000000.0:F1} млн. ₽" : $"{value / 1000.0:F1} тыс. ₽")
                    .ToPropertyEx(this, vm => vm.InitialFeeMax_Performance);

                this.WhenAnyValue(vm => vm.InitialFee,
                                  vm => vm.PropertyValue,
                                  (initialFee, propertyValue) => initialFee / (double)propertyValue * 100)
                    .ToPropertyEx(this, vm => vm.InitialFeeInPercent);

                this.WhenAnyValue(vm => vm.ConstructionPeriod)
                    .Select(x => x + 1)
                    .ToPropertyEx(this, vm => vm.MinimumCreditTerm);

                this.WhenAnyValue(vm => vm.InitialFeeInPercent).Select(x => $"{x:F1}%").ToPropertyEx(this, vm => vm.InitialFeeInPercent_Performance);
                this.WhenAnyValue(vm => vm.MinimumCreditTerm).Select(x => x % 10 == 1 && x % 100 != 11 ? $"{x} год" :
                                                                          x % 10 > 1 && x % 10 <= 4 &&
                                                                          !(x % 100 > 10 && x % 100 <= 14) ? $"{x} года" :
                                                                          $"{x} лет").ToPropertyEx(this, vm => vm.MinimumCreditTerm_Performance);
            }
        }
        public class Class_Current : ReactiveObject
        {
            private readonly Input _input;                                                                    // инициализация входных данных
            

            #region Рабочие переменные
            [ObservableAsProperty] public double InterestRate { get; }                                        // годовая ставка
            [ObservableAsProperty] public double MonthlyPayment { get; }                                      // ежемесячный платеж
            [ObservableAsProperty] public int CreditAmount { get; }                                           // сумма кредита
            [ObservableAsProperty] public double RequiredIncomeForConstructionPeriod { get; }                 // необходимый доход для периода постройки
            [ObservableAsProperty] public double RequiredIncomeForOtherPeriod { get; }                        // необходимый доход для оставшегося периода
            [ObservableAsProperty] public double FinalOverpayment { get; }                                    // итоговая переплата
            [ObservableAsProperty] public bool IsApproved { get; }                                            // одобрение банка
            #endregion

            #region Переменные представления
            [ObservableAsProperty] public string? InterestRate_Performance { get; }                           // годовая ставка
            [ObservableAsProperty] public string? MonthlyPayment_Performance { get; }                         // ежемесячный платеж
            [ObservableAsProperty] public string? CreditAmount_Performance { get; }                           // сумма кредита
            [ObservableAsProperty] public string? RequiredIncomeForConstructionPeriod_Performance { get; }    // необходимый доход для периода постройки
            [ObservableAsProperty] public string? RequiredIncomeForOtherPeriod_Performance { get; }           // необходимый доход для оставшегося периода
            [ObservableAsProperty] public string? FinalOverpayment_Performance { get; }                       // итоговая переплата
            #endregion

            #region Вспомогательные переменные
            private (double, double, double) _percents;
            private double MonthlyRate => InterestRate / 12;
            #endregion
            public Class_Current(Input input, (double, double, double) percents)
            {
                _input = input;
                _percents = percents;

                this.WhenAnyValue(vm => vm._input.InitialFeeInPercent)
                    .Select(x => x <= 20 ? _percents.Item1 : x <= 30 ? _percents.Item2 : _percents.Item3)
                    .ToPropertyEx(this, vm => vm.InterestRate);

                this.WhenAnyValue(vm => vm._input.PropertyValue,
                              vm => vm._input.InitialFee,
                              (propertyValue, initialFee) => propertyValue - initialFee)
                .ToPropertyEx(this, vm => vm.CreditAmount);

                this.WhenAnyValue(vm => vm.InterestRate,
                                  vm => vm.CreditAmount,
                                  vm => vm._input.CreditTerm,
                                  (interestRate, creditAmount, creditTerm) => MonthlyPaymentCalculate(creditAmount, MonthlyRate / 100, creditTerm * 12))
                    .ToPropertyEx(this, vm => vm.MonthlyPayment);

                this.WhenAnyValue(vm => vm.MonthlyPayment,
                                  vm => vm._input.Rent,
                                  (monthlyPayment, rent) => monthlyPayment + rent + Input.MinimumWage)
                    .ToPropertyEx(this, vm => vm.RequiredIncomeForConstructionPeriod);

                this.WhenAnyValue(vm => vm.MonthlyPayment,
                                  vm => vm._input.Rent,
                                  (monthlyPayment, rent) => monthlyPayment + Input.MinimumWage)
                    .ToPropertyEx(this, vm => vm.RequiredIncomeForOtherPeriod);

                this.WhenAnyValue(vm => vm.MonthlyPayment,
                                  vm => vm.CreditAmount,
                                  vm => vm._input.CreditTerm,
                                  (monthlyPayment, creditAmount, creditTerm) => creditTerm * 12 * monthlyPayment - creditAmount)
                    .ToPropertyEx(this, vm => vm.FinalOverpayment);

                this.WhenAnyValue(vm => vm.InterestRate).Select(x => $"{x:F1}%").ToPropertyEx(this, vm => vm.InterestRate_Performance);
                this.WhenAnyValue(vm => vm.MonthlyPayment).Select(x => $"{x:F0} ₽").ToPropertyEx(this, vm => vm.MonthlyPayment_Performance);
                this.WhenAnyValue(vm => vm.CreditAmount).Select(x => $"{x} ₽").ToPropertyEx(this, vm => vm.CreditAmount_Performance);
                this.WhenAnyValue(vm => vm.RequiredIncomeForConstructionPeriod).Select(x => $"{x:F0} ₽").ToPropertyEx(this, vm => vm.RequiredIncomeForConstructionPeriod_Performance);
                this.WhenAnyValue(vm => vm.RequiredIncomeForOtherPeriod).Select(x => $"{x:F0} ₽").ToPropertyEx(this, vm => vm.RequiredIncomeForOtherPeriod_Performance);
                this.WhenAnyValue(vm => vm.FinalOverpayment).Select(x => $"{x:F0} ₽").ToPropertyEx(this, vm => vm.FinalOverpayment_Performance);

                this.WhenAnyValue(
                    vm => vm.RequiredIncomeForConstructionPeriod,
                    vm => vm._input.CitizenshipRF,
                    vm => vm._input.Age,
                    vm => vm._input.Experience,
                    vm => vm._input.Salary,
                    (reqIncome, citizenshipRF, age, experience, salary) => citizenshipRF && age >= 21 && experience >= 6 && salary >= (int)reqIncome)
                    .ToPropertyEx(this, vm => vm.IsApproved);
            }
            private static double MonthlyPaymentCalculate(int creditAmount, double monthlyRate, int numberOfMonth) =>
                creditAmount * (monthlyRate * Math.Pow(1 + monthlyRate, numberOfMonth)) / (Math.Pow(1 + monthlyRate, numberOfMonth) - 1);
        }
        public class Class_New_Period1 : ReactiveObject
        {
            private readonly Input _input;

            #region Рабочие переменные
            [ObservableAsProperty] public double InterestRate { get; }      // годовая ставка
            [ObservableAsProperty] public double MonthlyPayment { get; }    // ежемесячный платеж
            [ObservableAsProperty] public int CreditAmount { get; }         // сумма кредита
            [ObservableAsProperty] public double RequiredIncome { get; }    // необходимый доход
            [ObservableAsProperty] public double FinalOverpayment { get; }  // итоговая переплата
            #endregion

            #region Переменные представления
            [ObservableAsProperty] public string? InterestRate_Performance { get; }     // годовая ставка
            [ObservableAsProperty] public string? MonthlyPayment_Performance { get; }   // ежемесячный платеж
            [ObservableAsProperty] public string? CreditAmount_Performance { get; }     // сумма кредита
            [ObservableAsProperty] public string? RequiredIncome_Performance { get; }   // необходимый доход
            [ObservableAsProperty] public string? FinalOverpayment_Performance { get; } // итоговая переплата
            #endregion

            #region Вспомогательные переменные
            private (double, double, double) _percents;         // от большего к меньшему
            private double MonthlyRate => InterestRate / 12;    // месечная ставка
            #endregion

            public Class_New_Period1(Input input, (double, double, double) percents)
            {
                _input = input;
                _percents = percents;

                this.WhenAnyValue(vm => vm._input.InitialFeeInPercent)
                    .Select(x => x <= 20 ? _percents.Item1 : x <= 30 ? _percents.Item2 : _percents.Item3)
                    .ToPropertyEx(this, vm => vm.InterestRate);

                this.WhenAnyValue(vm => vm._input.PropertyValue,
                              vm => vm._input.InitialFee,
                              vm => vm._input.CreditTerm,
                              vm => vm._input.ConstructionPeriod,
                              (propertyValue, initialFee, creditTerm, constructPeriod) => (propertyValue - initialFee) / creditTerm * constructPeriod)
                .ToPropertyEx(this, vm => vm.CreditAmount);

                this.WhenAnyValue(vm => vm.InterestRate,
                                  vm => vm.CreditAmount,
                                  vm => vm._input.CreditTerm,
                                  vm => vm._input.ConstructionPeriod,
                                  (interestRate, creditAmount, creditTerm, constructPeriod) => MonthlyPaymentCalculate(creditAmount, MonthlyRate / 100, constructPeriod * 12))
                    .ToPropertyEx(this, vm => vm.MonthlyPayment);

                this.WhenAnyValue(vm => vm.MonthlyPayment,
                                  vm => vm._input.Rent,
                                  (monthlyPayment, rent) => monthlyPayment + rent + Input.MinimumWage)
                    .ToPropertyEx(this, vm => vm.RequiredIncome);

                this.WhenAnyValue(vm => vm.MonthlyPayment,
                                  vm => vm.CreditAmount,
                                  vm => vm._input.CreditTerm,
                                  vm => vm._input.ConstructionPeriod,
                                  (monthlyPayment, creditAmount, creditTerm, constructPeriod) => constructPeriod * 12 * monthlyPayment - creditAmount)
                    .ToPropertyEx(this, vm => vm.FinalOverpayment);

                this.WhenAnyValue(vm => vm.InterestRate).Select(x => $"{x:F1}%").ToPropertyEx(this, vm => vm.InterestRate_Performance);
                this.WhenAnyValue(vm => vm.MonthlyPayment).Select(x => $"{x:F0} ₽").ToPropertyEx(this, vm => vm.MonthlyPayment_Performance);
                this.WhenAnyValue(vm => vm.CreditAmount).Select(x => $"{x} ₽").ToPropertyEx(this, vm => vm.CreditAmount_Performance);
                this.WhenAnyValue(vm => vm.RequiredIncome).Select(x => $"{x:F0} ₽").ToPropertyEx(this, vm => vm.RequiredIncome_Performance);
                this.WhenAnyValue(vm => vm.FinalOverpayment).Select(x => $"{x:F0} ₽").ToPropertyEx(this, vm => vm.FinalOverpayment_Performance);
            }
            private static double MonthlyPaymentCalculate(int creditAmount, double monthlyRate, int numberOfMonth) =>
                (monthlyRate * Math.Pow(1 + monthlyRate, numberOfMonth)) / (Math.Pow(1 + monthlyRate, numberOfMonth) - 1) * creditAmount;
        }
        public class Class_New_Period2 : ReactiveObject
        {
            private readonly Input _input;
            private readonly Class_Current _current;
            private readonly Class_New_Period1 _new_Period1;
            private int NumberOfMonth => (_input.CreditTerm - _input.ConstructionPeriod) * 12;

            #region Рабочие переменные
            [ObservableAsProperty] public double InterestRate { get; }      // годовая ставка
            [ObservableAsProperty] public double MonthlyPayment { get; }    // ежемесячный платеж
            [ObservableAsProperty] public int CreditAmount { get; }         // сумма кредита
            [ObservableAsProperty] public double RequiredIncome { get; }    // необходимый доход
            [ObservableAsProperty] public double FinalOverpayment { get; }  // итоговая переплата
            #endregion

            #region Переменные представления
            [ObservableAsProperty] public string? InterestRate_Performance { get; }      // годовая ставка
            [ObservableAsProperty] public string? MonthlyPayment_Performance { get; }    // ежемесячный платеж
            [ObservableAsProperty] public string? CreditAmount_Performance { get; }      // сумма кредита
            [ObservableAsProperty] public string? RequiredIncome_Performance { get; }    // необходимый доход
            [ObservableAsProperty] public string? FinalOverpayment_Performance { get; }  // итоговая переплата
            #endregion

            public Class_New_Period2(Input input, Class_Current current, Class_New_Period1 new_Period1)
            {
                _input = input;
                _current = current;
                _new_Period1 = new_Period1;

                this.WhenAnyValue(vm => vm._current.CreditAmount,
                                  vm => vm._new_Period1.CreditAmount,
                                  (current, new_period1) => current - new_period1)
                    .ToPropertyEx(this, vm => vm.CreditAmount);

                this.WhenAnyValue(vm => vm._current.FinalOverpayment,
                                  vm => vm._new_Period1.FinalOverpayment,
                                  (current, new_period1) => current - new_period1)
                    .ToPropertyEx(this, vm => vm.FinalOverpayment);

                this.WhenAnyValue(vm => vm.CreditAmount,
                                  vm => vm.FinalOverpayment,
                                  (creditAmount, finalOverpayment) => (creditAmount + finalOverpayment) / NumberOfMonth)
                    .ToPropertyEx(this, vm => vm.MonthlyPayment);

                this.WhenAnyValue(vm => vm.MonthlyPayment,
                                  vm => vm._input.Rent,
                                  (monthlyPayment, rent) => monthlyPayment + Input.MinimumWage)
                    .ToPropertyEx(this, vm => vm.RequiredIncome);

                this.WhenAnyValue(vm => vm.MonthlyPayment,
                                  vm => vm.CreditAmount,
                                  (monthlyPayment, creditAmount) => Models.PercentCalculate.CalculateInterestRate(monthlyPayment, creditAmount, NumberOfMonth))
                    .ToPropertyEx(this, vm => vm.InterestRate);

                this.WhenAnyValue(vm => vm.InterestRate).Select(x => $"{x:F1}%").ToPropertyEx(this, vm => vm.InterestRate_Performance);
                this.WhenAnyValue(vm => vm.MonthlyPayment).Select(x => $"{x:F0} ₽").ToPropertyEx(this, vm => vm.MonthlyPayment_Performance);
                this.WhenAnyValue(vm => vm.CreditAmount).Select(x => $"{x} ₽").ToPropertyEx(this, vm => vm.CreditAmount_Performance);
                this.WhenAnyValue(vm => vm.RequiredIncome).Select(x => $"{x:F0} ₽").ToPropertyEx(this, vm => vm.RequiredIncome_Performance);
                this.WhenAnyValue(vm => vm.FinalOverpayment).Select(x => $"{x:F0} ₽").ToPropertyEx(this, vm => vm.FinalOverpayment_Performance);
            }
        }
    }
}
