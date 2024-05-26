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
            Current = new(InputData, null, (18.2, 17.2, 16.7));
            New_Period1 = new(InputData, InputData.ConstructionPeriod * 12, (14.2, 13.2, 12.7));
            New_Period2 = new(InputData, Current, New_Period1);
        }
        [Reactive] public Input InputData { get; set; }
        [Reactive] public Program Current { get; set; }
        [Reactive] public Program New_Period1 { get; set; }
        [Reactive] public NewProgramLastPeriod New_Period2 { get; set; }
        public class Input : ReactiveObject
        {
            public const int MinimumWage = 13890;

            [Reactive] public int PropertyValue { get; set; } = 3000000;                        // стоимость недвижимости
            [Reactive] public int InitialFee { get; set; } = 1000000;                         // первоначальный взнос
            [Reactive] public int CreditTerm { get; set; } = 10;                             // срок кредита (1-30 лет)
            [Reactive] public int Rent { get; set;} = 25000;
            [Reactive] public int ConstructionPeriod { get; set;} = 3;
            [ObservableAsProperty] public int InitialFeeMin { get; }                        // минимальное значение первоначального взноса в зависимости от стоимости недвижимости
            [ObservableAsProperty] public int InitialFeeMax { get; }                        // максимальное значение первоначального взноса в зависимости от стоимости недвижимости
            [ObservableAsProperty] public string? InitialFeeMin_Performance { get; }        // минимальное значение первоначального взноса в зависимости от стоимости недвижимости
            [ObservableAsProperty] public string? InitialFeeMax_Performance { get; }        // максимальное значение первоначального взноса в зависимости от стоимости недвижимости
            [ObservableAsProperty] public double InitialFeeInPercent { get; }               // первоначальный взнос (в процентах)
            
            public Input()
            {
                this.WhenAnyValue(vm => vm.PropertyValue)
                    .Select(x => (int)(x * 0.151))
                    .ToPropertyEx(this, vm => vm.InitialFeeMin);

                this.WhenAnyValue(vm => vm.InitialFeeMin)
                    .Select(value => value >= 1000000 ? $"{value / 1000000.0:F1} млн. Р" : $"{value / 1000.0:F1} тыс. Р")
                    .ToPropertyEx(this, vm => vm.InitialFeeMin_Performance);

                this.WhenAnyValue(vm => vm.PropertyValue)
                    .Select(x => x - 300000)
                    .ToPropertyEx(this, vm => vm.InitialFeeMax);

                this.WhenAnyValue(vm => vm.InitialFeeMax)
                    .Select(value => value >= 1000000 ? $"{value / 1000000.0:F1} млн. Р" : $"{value / 1000.0:F1} тыс. Р")
                    .ToPropertyEx(this, vm => vm.InitialFeeMax_Performance);

                this.WhenAnyValue(vm => vm.InitialFee,
                                  vm => vm.PropertyValue,
                                  (initialFee, propertyValue) => Math.Round(initialFee / (double)propertyValue * 100, 1))
                    .ToPropertyEx(this, vm => vm.InitialFeeInPercent);

                
            }
        }
        public class Program : ReactiveObject
        {
            private Input _input;
            private double _monthlyRate => InterestRate / 12;

            private int? _numberOfMonth;
            public int NumberOfMonth => _numberOfMonth ?? _input.CreditTerm * 12;

            private (double, double, double) _percents; // от большего к меньшему

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

            public Program(Input input, int? numberOfMonth, (double, double, double) percents)
            {
                _input = input;
                _numberOfMonth = numberOfMonth;
                _percents = percents;

                this.WhenAnyValue(vm => vm._input.InitialFeeInPercent)
                    .Select(x => x <= 20 ? _percents.Item1 : x <= 30 ? _percents.Item2 : _percents.Item3)
                    .ToPropertyEx(this, vm => vm.InterestRate);

                this.WhenAnyValue(vm => vm._input.PropertyValue,
                              vm => vm._input.InitialFee,
                              vm => vm._input.CreditTerm,
                              (propertyValue, initialFee, creditTerm) => (value: propertyValue - initialFee, allNumberOfMonth: creditTerm * 12))
                .Select(x => NumberOfMonth != numberOfMonth ? x.value : x.value * NumberOfMonth / x.allNumberOfMonth)
                .ToPropertyEx(this, vm => vm.CreditAmount);

                this.WhenAnyValue(vm => vm.InterestRate,
                                  vm => vm.CreditAmount,
                                  vm => vm._input.CreditTerm,
                                  (interestRate, creditAmount, creditTerm) => MonthlyPaymentCalculate(creditAmount, _monthlyRate / 100, NumberOfMonth))
                    .ToPropertyEx(this, vm => vm.MonthlyPayment);

                this.WhenAnyValue(vm => vm.MonthlyPayment,
                                  vm => vm._input.Rent,
                                  (monthlyPayment, rent) => monthlyPayment + rent + Input.MinimumWage)
                    .ToPropertyEx(this, vm => vm.RequiredIncome);

                this.WhenAnyValue(vm => vm.MonthlyPayment,
                                  vm => vm.CreditAmount,
                                  vm => vm._input.CreditTerm,
                                  (monthlyPayment, creditAmount, creditTerm) => NumberOfMonth * monthlyPayment - creditAmount)
                    .ToPropertyEx(this, vm => vm.FinalOverpayment);

                this.WhenAnyValue(vm => vm.InterestRate).Select(x => $"{x:F1}%").ToPropertyEx(this, vm => vm.InterestRate_Performance);
                this.WhenAnyValue(vm => vm.MonthlyPayment).Select(x => $"{x:F0} ₽").ToPropertyEx(this, vm => vm.MonthlyPayment_Performance);
                this.WhenAnyValue(vm => vm.CreditAmount).Select(x => $"{x} ₽").ToPropertyEx(this, vm => vm.CreditAmount_Performance);
                this.WhenAnyValue(vm => vm.RequiredIncome).Select(x => $"{x:F0} ₽").ToPropertyEx(this, vm => vm.RequiredIncome_Performance);
                this.WhenAnyValue(vm => vm.FinalOverpayment).Select(x => $"{x:F0} ₽").ToPropertyEx(this, vm => vm.FinalOverpayment_Performance);
            }
            private static double MonthlyPaymentCalculate(int creditAmount, double monthlyRate, int numberOfMonth) =>
                creditAmount * (monthlyRate * Math.Pow(1 + monthlyRate, numberOfMonth)) / (Math.Pow(1 + monthlyRate, numberOfMonth) - 1);
        }
        public class NewProgramLastPeriod : ReactiveObject
        {
            private Input _input;
            private Program _current;
            private Program _new_Period1;

            //private double _monthlyRate => InterestRate / 12;

            private int? _numberOfMonth;
            public int NumberOfMonth => _numberOfMonth ?? _current.NumberOfMonth - _new_Period1.NumberOfMonth;

            private (double, double, double) _percents; // от большего к меньшему

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

            public NewProgramLastPeriod(Input input, Program current, Program new_Period1)
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
                                  (monthlyPayment, rent) => monthlyPayment + rent + Input.MinimumWage)
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
