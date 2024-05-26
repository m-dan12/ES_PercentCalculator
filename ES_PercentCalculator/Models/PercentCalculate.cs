using ES_PercentCalculator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ES_PercentCalculator.Models
{
    public class PercentCalculate
    {
        // Метод для вычисления процентной ставки
        public static double CalculateInterestRate(double P, double S, int n)
        {
            double r = 0.05;  // начальное приближение
            for (int i = 0; i < 1000; i++)
            {
                double f = P * (Math.Pow(1 + r, n) - 1) - S * r * Math.Pow(1 + r, n);
                double f_prime = P * n * Math.Pow(1 + r, n - 1) - S * (Math.Pow(1 + r, n) + r * n * Math.Pow(1 + r, n - 1));
                double r_new = r - f / f_prime;
                if (Math.Abs(r_new - r) < 1e-10)
                    break;
                r = r_new;
            }
            return r * 12 * 100;  // годовая процентная ставка в процентах
        }
    }
}

/*static double CalculateInterestRate(double S, double P, int n)
{
    double r = 0.05; // начальное приближение
    for (int i = 0; i < 1000; i++)
    {
        double f = P * (Math.Pow(1 + r, n) - 1) - S * r * (Math.Pow(1 + r, n));
        double f_prime = P * n * Math.Pow(1 + r, (n - 1)) - S * (Math.Pow(1 + r, n) + r * n * Math.Pow(1 + r, (n - 1)));
        double r_new = r - f / f_prime;
        if (Math.Abs(r_new - r) < 1e-10)
        {
            break;
        }
        r = r_new;
    }
    return r * 12 * 100; // годовая процентная ставка в процентах
}*/
/* Пример использовния
class Program
{
    static void Main()
    {
        double S = 100000;  // сумма кредита
        double P = 3000;    // ежемесячный платеж
        int n = 36;         // количество месяцев

        PercentCalculate percentCalculate = new PercentCalculate(S, P, n);
        double annualInterestRate = percentCalculate.CalculateInterestRate();
        Console.WriteLine($"Годовая процентная ставка: {annualInterestRate:F2}%");
    }
}*/

/* Python Code
    def calculate_interest_rate(S, P, n):
    r = 0.05  # начальное приближение
    for i in range(1000):
        f = P * ((1 + r) * *n - 1) - S * r * (1 + r) * *n
        f_prime = P * n * (1 + r) * *(n - 1) - S * ((1 + r) * *n + r * n * (1 + r) * *(n - 1))
        r_new = r - f / f_prime
        if abs(r_new - r) < 1e-10:
            break
        r = r_new
    return r * 12 * 100  # годовая процентная ставка в процентах

S = 100000  # сумма кредита
n = 36      # количество месяцев
P = 3000    # ежемесячный платеж

annual_interest_rate = calculate_interest_rate(S, P, n)
print(f"Годовая процентная ставка: {annual_interest_rate:.2f}%")*/
