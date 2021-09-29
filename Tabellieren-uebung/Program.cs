using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Tabellieren_uebung
{
    public class Program
    {
        static void Main(string[] args)
        {
            int pagelength = 10;
            if (args.Length > 0)
                int.TryParse(args?[1], out pagelength);

            string[] lines = System.IO.File.ReadAllLines(@"C:\Users\rekr\Downloads\CSVViewer\personen.csv");
            lines = lines.Where(l => !string.IsNullOrEmpty(l)).ToArray();
            string titleLine = "No.;" + lines[0];
            lines = lines.Skip(1).Select((l, i) => $"{ i + 1};{l}").ToArray();
            string userInput = "F";
            int currentPage = 1;
            int lastPage = lines.Length / pagelength + (lines.Length % pagelength > 0 ? 1 : 0);

            var pageSelectionRegex = new Regex(@"J\d+");


            while (true)
            {
                if (userInput == "F")
                {
                    currentPage = 1;
                }
                else if (userInput == "P")
                {
                    currentPage = currentPage - 1 < 1 ? 1 : currentPage - 1;
                }
                else if (userInput == "N")
                {
                    currentPage = currentPage + 1 < lastPage ? currentPage + 1 : lastPage;
                }
                else if (userInput == "L")
                {
                    currentPage = lastPage;
                }
                else if (pageSelectionRegex.IsMatch(userInput))
                {
                    currentPage = int.Parse(userInput.Substring(1));
                }
                else if (userInput == "E")
                {
                    return;
                }

                var linesToPrint = lines.Skip(pagelength * (currentPage - 1)).Take(pagelength).ToList();
                linesToPrint.Insert(0, titleLine);
                var output = Tabellieren(linesToPrint);

                for (int i = 0; i < output.Length; i++)
                {
                    Console.WriteLine(output[i]);
                }
                Console.WriteLine($"Page {currentPage} of {lastPage}");
                Console.WriteLine("F)irst page, P)revious page, N)ext page, L)ast page, J)ump to page, E)xit");
                userInput = Console.ReadLine();
            }
            int x = 1;
        }

        public static string[] Tabellieren(List<string> csvZeilen)
        {
            int numberOfInputLines = csvZeilen.Count;
            int numberOfColumns = csvZeilen[0].Split(";").Length;
            int numberOfOutputLines = numberOfInputLines * 2 + 1;

            //string[] result = new string[numberOfOutputLines];

            var words = PutWordsIn2DimensionalArray(csvZeilen, numberOfInputLines);
            var columnLengths = CalculateColumnLenghts(numberOfColumns, numberOfInputLines, words);

            var result = Combine(columnLengths, numberOfInputLines, words);
            return result.ToArray();
        }

        public static List<string> Combine(List<int> columnLengths, int numberOfInputLines, List<List<string>> words)
        {
            var result = new List<string>();
            result.Add(GenerateOneSeparatorLine(columnLengths));
            for (int i = 0; i < numberOfInputLines; i++)
            {
                result.Add(GenerateOneTextLine(columnLengths, words[i]));
                result.Add(GenerateOneSeparatorLine(columnLengths));
            }

            return result;
        }

        public static string GenerateOneTextLine(List<int> columnLengths, List<string> words)
        {
            string output = "|";
            for (int j = 0; j < columnLengths.Count; j++)
            {
                int numberOfWhitespaces = columnLengths[j] - words[j].Length;
                output += words[j];
                output += new string(' ', numberOfWhitespaces);
                output += "|";
            }

            return output;
        }

        public static string GenerateOneSeparatorLine(List<int> columnLengths)
        {
            string output = "+";
            foreach (var colLength in columnLengths)
            {
                output += new string('-', colLength) + "+";
            }
            return output;
        }

        public static List<int> CalculateColumnLenghts(int numberOfColumns, int numberOfInputLines, List<List<string>> words)
        {
            var columnLengths = new List<int>();

            for (int j = 0; j < numberOfColumns; j++)
            {
                columnLengths.Add(0);
            }

            for (int i = 0; i < numberOfInputLines; i++)
            {
                for (int j = 0; j < numberOfColumns; j++)
                {
                    columnLengths[j] = columnLengths[j] < words[i][j].Length ? words[i][j].Length : columnLengths[j];
                }
            }

            return columnLengths;
        }

        public static List<List<string>> PutWordsIn2DimensionalArray(List<string> csvZeilen, int numberOfInputLines)
        {
            var words = new List<List<string>>();
            for (int i = 0; i < numberOfInputLines; i++)
            {
                var lineWords = csvZeilen[i].Split(";");
                words.Add(lineWords.ToList());
            }

            return words;
        }
    }
}
