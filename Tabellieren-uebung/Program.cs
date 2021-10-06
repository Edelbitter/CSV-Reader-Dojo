using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Tabellieren_uebung
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            if (!GetPathFromArgs(args, out var path)) return;
            var numberOfLinesPerPage = GetPageLengthFromArgs(args, defaultLength: 10);

           
            string userInput = "F";
            int currentPage = 1;
            int lastPage = 1;//lines.Length / pagelength + (lines.Length % pagelength > 0 ? 1 : 0);
            bool pagePositionsFound = false;

            while (true)
            {
                //if (pagePositionsTask.IsCompleted && !pagePositionsFound)
                //{
                //    //lastPage = numberOfLines / pagelength + (numberOfLines % pagelength > 0 ? 1 : 0);
                //    pagePositions.AddRange(pagePositionsTask.Result);
                //    var numberOfPages = pagePositionsTask.Result.Count + 1;
                //    lastPage = numberOfPages;
                //    pagePositionsFound = true;
                //}
                //if (userInput == "E")
                //{
                //    return;
                //}

                //currentPage = GetCurrentPageFromUserInput(userInput, lastPage, currentPage);
                //var lines = await GetLinesFromPageNumber(currentPage, pagePositions, path, 1024, numberOfLinesPerPage);
                //var linesNumbered = lines.Select((l, i) => $"{ 1 + i + ((currentPage - 1) * numberOfLinesPerPage)};{l}").ToArray();
                //var linesToPrint = linesNumbered.Take(numberOfLinesPerPage).ToList();
                //linesToPrint.Insert(0, titleLine);
                //var output = Tabellieren(linesToPrint);

                //for (int i = 0; i < output.Length; i++)
                //{
                //    Console.WriteLine(output[i]);
                //}
                //Console.WriteLine($"Page {currentPage} of {lastPage}{(pagePositionsFound ? null : '?')}");
                //Console.WriteLine("F)irst page, P)revious page, N)ext page, L)ast page, J)ump to page, E)xit");
                //userInput = Console.ReadLine();
            }
        }


        private static int GetPageLengthFromArgs(string[] args, int defaultLength)
        {
            int pagelength = defaultLength;
            if (args.Length > 1)
                int.TryParse(args[1], out pagelength);
            return pagelength;
        }

        private static bool GetPathFromArgs(string[] args, out string path)
        {
            path = "";
            if (args.Length < 1)
            {
                Console.WriteLine("please provide a file");
                return false;
            }

            path = args[0];
            return true;
        }

        public static int GetCurrentPageFromUserInput(string userInput, int lastPage, int currentPage)
        {
            var pageSelectionRegex = new Regex(@"J\d+");

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

            return currentPage;
        }

        public async Task<List<string>> GetLinesFromOffset(int numberOfLines, int offset, string path, int buffersize)
        {
            await using (FileStream fs = File.OpenRead(path))
            {
                byte[] b = new byte[buffersize];
                UTF8Encoding temp = new UTF8Encoding(true);

                while (true)
                {
                    await fs.ReadAsync(b, 0, b.Length);
                    var str = temp.GetString(b);

                    var lines = str.Split('\n').Where(l => l.Length > 0).ToList();
                    if (lines.Count - 1 < numberOfLines)
                    {
                        buffersize *= 2;
                        continue;
                    }

                    return lines.Take(numberOfLines).ToList();
                }
            }
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
