using System;
using System.Collections.Generic;
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
            int pagelength = 10;
            if (args.Length > 0)
                int.TryParse(args?[1], out pagelength);

            string path = @"C:\Users\rekr\Downloads\CSVViewer\personen.csv";
            List<int> pagePositions = new List<int>() { 0 };

            int numberOfLines = 0;
            string titleLine = (await GetLinesFromPageNumber(1, pagePositions, path, 1024, 1)).FirstOrDefault();
            pagePositions[0] = new UTF8Encoding().GetBytes(titleLine +'\n').Length;
            var pagePosisTask =  FindPageStartOffsetsAndTitleLine(path, pagelength);
            
            var columns = titleLine.Split(';').ToList();
            titleLine = "No.;" + titleLine;


            string userInput = "F";
            int currentPage = 1;
            int lastPage = 1;//lines.Length / pagelength + (lines.Length % pagelength > 0 ? 1 : 0);
            

            while (true)
            {
                if (pagePosisTask.IsCompleted)
                {
                    //lastPage = numberOfLines / pagelength + (numberOfLines % pagelength > 0 ? 1 : 0);
                    pagePositions = pagePosisTask.Result;
                    var numberOfPages = pagePosisTask.Result.Count;
                    lastPage = numberOfPages;
                }
                if (userInput == "E")
                {
                    return;
                }

                currentPage = GetCurrentPageFromUserInput(userInput, lastPage,currentPage);
                var lines = await GetLinesFromPageNumber(currentPage,pagePositions,path,1024,numberOfLines);
                var linesNumbered = lines.Select((l, i) => $"{ i + 1};{l}").ToArray();
                var linesToPrint = linesNumbered.Skip(pagelength * (currentPage - 1)).Take(pagelength).ToList();
                linesToPrint.Insert(0, titleLine);
                var output = Tabellieren(linesToPrint);

                for (int i = 0; i < output.Length; i++)
                {
                    Console.WriteLine(output[i]);
                }
                Console.WriteLine($"Page {currentPage} of {lastPage}{(pagePosisTask.IsCompleted ? null : '?')}");
                Console.WriteLine("F)irst page, P)revious page, N)ext page, L)ast page, J)ump to page, S)ort, E)xit");
                userInput = Console.ReadLine();
            }
            int x = 1;
        }

        public static async Task<List<string>> GetLinesFromPageNumber(int currentPage, List<int> pagePosis, string path,int buffersize,int numberOfLines)
        {
            var offset = pagePosis[currentPage-1];
            using (FileStream fs = File.OpenRead(path))
            {
                byte[] b = new byte[buffersize];
                UTF8Encoding temp = new UTF8Encoding(true);
                fs.Seek(offset, SeekOrigin.Begin);
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
                    return lines;
                } 
            }
        }

        public static int GetCurrentPageFromUserInput(string userInput,int lastPage, int currentPage)
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

        private static async Task<List<int>> FindPageStartOffsetsAndTitleLine(string path, int pagelength)
        {
            var pagePosis = new List<int>();
            string titleLine = "";
            using (FileStream fs = File.OpenRead(path))
            {
                int buffersize = 1024;
                byte[] b = new byte[buffersize];
                UTF8Encoding temp = new UTF8Encoding(true);

                var bytesRead = 0;
                do
                {
                    bytesRead = await fs.ReadAsync(b, 0, b.Length);
                    var str = temp.GetString(b);

                    var lines = str.Split('\n').Where(l => l.Length > 0).ToList();

                    if (bytesRead < buffersize)
                    {
                        break;
                    }

                    if (lines.Count - 1 < pagelength)
                    {
                        buffersize *= 2;
                        continue;
                    }

                    if (string.IsNullOrEmpty(titleLine))
                    {
                        titleLine = lines[0];
                        lines = lines.Skip(1).ToList();
                    }

                    var page = lines.Take(pagelength).ToList();
                    int endOfPage = str.IndexOf(lines.Skip(pagelength).Take(1).First());
                    var pageBlock = str.Substring(0, endOfPage - 1);
                    var pageBlockBytes = temp.GetBytes(pageBlock).Length;
                    pagePosis.Add(pageBlockBytes + pagePosis.LastOrDefault());
                    
                    fs.Seek(pagePosis.Last(), SeekOrigin.Begin);
                    
                } while (bytesRead > 0);
            }

            return pagePosis;
        }

        public async Task<List<string>> GetLinesFromOffset(int numberOfLines, int offset, string path,int buffersize)
        {
            using (FileStream fs = File.OpenRead(path))
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

        private static async Task<int> CountLinesAsync(string path)
        {
            int nolines = 0;
            using (StreamReader sr = File.OpenText(path))
            {
                string s = "";
                while ((s = await sr.ReadLineAsync()) != null)
                {
                    if (s.Length > 1) nolines++;
                    Console.WriteLine(s);
                }
            }

            return nolines;
        }

        //private static async Task<string> GetSpecificLineAsync(string path)
        //{
        //    int nolines = 0;
        //    using (StreamReader sr = File.OpenText(path))
        //    {
        //        string s = "";
        //        while ((s = await sr..ReadLineAsync()) != null)
        //        {
        //            if (s.Length > 1) nolines++;
        //            Console.WriteLine(s);
        //        }
        //    }

        //    return nolines;
        //}

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
