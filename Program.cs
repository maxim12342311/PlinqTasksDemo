using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace PLinqTasksDemo
{
    internal class Program
    {
        static void Main()
        {
            Console.WriteLine("=== PLINQ demo (.NET 8) ===\n");

            Task1_FilterAndSort();
            Task2_Factorials();
            Task3_MinMax();
            Task4_TextProcessing();
            Task5_SumAndAverage();
            Task6_ComplexMath();

            Console.WriteLine("\nAll tasks finished. Press Enter.");
            Console.ReadLine();
        }

        static long MeasureMs(Action action)
        {
            var sw = Stopwatch.StartNew();
            action();
            sw.Stop();
            return sw.ElapsedMilliseconds;
        }

        // 1. Параллельная фильтрация и сортировка
        static void Task1_FilterAndSort()
        {
            Console.WriteLine("Task 1: Filter + sort large dataset");

            int count = 5_000_000;
            int[] data = GenerateInts(count, seed: 1);

            long seq = MeasureMs(() =>
            {
                var result = data
                    .Where(x => x % 2 == 0 && x > 1000)
                    .OrderByDescending(x => x)
                    .ToArray();
            });

            long par = MeasureMs(() =>
            {
                var result = data
                    .AsParallel()
                    .Where(x => x % 2 == 0 && x > 1000)
                    .OrderByDescending(x => x)
                    .ToArray();
            });

            PrintTiming(seq, par);
            Console.WriteLine();
        }

        // 2. Факториалы 1..20
        static void Task2_Factorials()
        {
            Console.WriteLine("Task 2: Factorials 1..20");

            int[] nums = Enumerable.Range(1, 20).ToArray();

            long seq = MeasureMs(() =>
            {
                foreach (var n in nums)
                    _ = Factorial(n);
            });

            long par = MeasureMs(() =>
            {
                nums
                    .AsParallel()
                    .ForAll(n => { _ = Factorial(n); });
            });

            PrintTiming(seq, par);
            Console.WriteLine();
        }

        // 3. Поиск min/max
        static void Task3_MinMax()
        {
            Console.WriteLine("Task 3: Min/Max in large array");

            int count = 5_000_000;
            int[] data = GenerateInts(count, seed: 2);

            long seq = MeasureMs(() =>
            {
                int min = data.Min();
                int max = data.Max();
            });

            long par = MeasureMs(() =>
            {
                int min = data.AsParallel().Min();
                int max = data.AsParallel().Max();
            });

            PrintTiming(seq, par);
            Console.WriteLine();
        }

        // 4. Обработка строк большого текстового файла
        static void Task4_TextProcessing()
        {
            Console.WriteLine("Task 4: Remove vowels from big text file");

            string dir = Path.GetTempPath();
            string path = Path.Combine(dir, "plinq_big_text.txt");

            if (!File.Exists(path))
                CreateBigTextFile(path, lines: 100_000);

            long seq = MeasureMs(() =>
            {
                var processed = File.ReadLines(path)
                    .Select(RemoveVowels)
                    .ToList();
            });

            long par = MeasureMs(() =>
            {
                var processed = File.ReadLines(path)
                    .AsParallel()
                    .Select(RemoveVowels)
                    .ToList();
            });

            PrintTiming(seq, par);
            Console.WriteLine($"Text file used: {path}");
            Console.WriteLine();
        }

        // 5. Сумма и среднее по нескольким массивам
        static void Task5_SumAndAverage()
        {
            Console.WriteLine("Task 5: Sum + average for multiple arrays");

            int arraysCount = 5;
            int length = 1_000_000;
            var arrays = new List<int[]>(arraysCount);
            for (int i = 0; i < arraysCount; i++)
                arrays.Add(GenerateInts(length, seed: 100 + i));

            long seq = MeasureMs(() =>
            {
                foreach (var arr in arrays)
                {
                    long sum = 0;
                    foreach (var x in arr)
                        sum += x;
                    double avg = sum / (double)arr.Length;
                }
            });

            long par = MeasureMs(() =>
            {
                var results = arrays
                    .AsParallel()
                    .Select(arr =>
                    {
                        long sum = 0;
                        foreach (var x in arr)
                            sum += x;
                        double avg = sum / (double)arr.Length;
                        return (sum, avg);
                    })
                    .ToArray();
            });

            PrintTiming(seq, par);
            Console.WriteLine();
        }

        // 6. Сложные математические операции
        static void Task6_ComplexMath()
        {
            Console.WriteLine("Task 6: Complex math operations over array");

            int count = 3_000_000;
            double[] data = GenerateDoubles(count, seed: 5);

            long seq = MeasureMs(() =>
            {
                var result = data
                    .Select(x => Math.Sqrt(Math.Pow(x, 3) + Math.Sqrt(x)))
                    .ToArray();
            });

            long par = MeasureMs(() =>
            {
                var result = data
                    .AsParallel()
                    .Select(x => Math.Sqrt(Math.Pow(x, 3) + Math.Sqrt(x)))
                    .ToArray();
            });

            PrintTiming(seq, par);
            Console.WriteLine();
        }

        // ---- helpers ----

        static void PrintTiming(long seqMs, long parMs)
        {
            double speedup = parMs == 0 ? 0 : (double)seqMs / parMs;
            Console.WriteLine($"Sequential: {seqMs} ms");
            Console.WriteLine($"Parallel  : {parMs} ms");
            Console.WriteLine($"Speedup   : {speedup:0.00}x");
        }

        static int[] GenerateInts(int count, int seed)
        {
            var rnd = new Random(seed);
            var data = new int[count];
            for (int i = 0; i < count; i++)
                data[i] = rnd.Next();
            return data;
        }

        static double[] GenerateDoubles(int count, int seed)
        {
            var rnd = new Random(seed);
            var data = new double[count];
            for (int i = 0; i < count; i++)
                data[i] = rnd.NextDouble() * 1000.0;
            return data;
        }

        static BigInteger Factorial(int n)
        {
            BigInteger result = 1;
            for (int i = 2; i <= n; i++)
                result *= i;
            return result;
        }

        static void CreateBigTextFile(string path, int lines)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < lines; i++)
            {
                sb.Append("This is a sample line number ")
                  .Append(i)
                  .Append(" with some random vowels aeio uAEIOU.\n");
            }
            File.WriteAllText(path, sb.ToString());
        }

        static string RemoveVowels(string s)
        {
            const string vowels = "aeiouAEIOUаеёиоуыэюяАЕЁИОУЫЭЮЯ";
            var sb = new StringBuilder(s.Length);
            foreach (char c in s)
            {
                if (!vowels.Contains(c))
                    sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
