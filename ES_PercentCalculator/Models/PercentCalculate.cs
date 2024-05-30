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
            double r = 5;  // начальное приближение
            for (int i = 0; i < 1000; i++)
            {
                double f = P * (Math.Pow(1 + r, n) - 1) - S * r * Math.Pow(1 + r, n);
                double f_prime = P * n * Math.Pow(1 + r, n - 1) - S * (Math.Pow(1 + r, n) + r * n * Math.Pow(1 + r, n - 1));
                if (f_prime == 0) break;
                double r_new = r - f / f_prime;
                if (Math.Abs(r_new - r) < 1e-6)
                    break;
                r = r_new;
            }
            return r * 12 * 100;  // годовая процентная ставка в процентах
        }
    }
}