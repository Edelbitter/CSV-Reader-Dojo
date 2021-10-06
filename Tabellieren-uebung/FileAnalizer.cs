using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Tabellieren-uebung-tests")]
namespace Tabellieren_uebung
{
    public class FileAnalizer
    {
        internal string filePath;
        internal int numberOfLinesPerPage;
        internal Encoding encoding;

        internal int buffersize;
        internal List<string> columns;
        internal string titleLine;

        internal List<int> pagePositions = new List<int>() { 0 };

        public List<string> Columns { get; internal set; }
        public bool FileReadFinished { get; internal set; }

        public FileAnalizer(string filePath, int numberOfLinesPerPage, bool isTest = false)
        {
            this.filePath = filePath;
            this.numberOfLinesPerPage = numberOfLinesPerPage;
            this.buffersize = 1024;

            if (!isTest)
            {
                if (!this.GetFileEncoding())
                {
                    throw new Exception();
                }
                this.Initialize();
            }

        }

        internal void Initialize()
        {
            this.SetTitleLine();
            this.Columns = this.titleLine.Split(';').ToList();

            this.FileReadFinished = false;
            _ = this.FindPageStartOffsets();

        }

        internal void SetTitleLine()
        {
            var firstLine = this.GetLinesFromPageNumber(1, 1).FirstOrDefault();
            this.SetFirstPagePositionAtEndOfTitleLine(firstLine);
            this.titleLine = $"No.;{firstLine.Trim()}";
        }

        internal void SetFirstPagePositionAtEndOfTitleLine(string firstLine)
        {
            var titleLineBytes = this.encoding.GetBytes(firstLine);
            this.pagePositions[0] = titleLineBytes.Length;
        }

        internal List<string> GetLinesFromPageNumber(int currentPage, int numberOfLines)
        {
            var task = this.GetLinesFromPageNumberAsync(currentPage, numberOfLines);
            task.Wait();
            return task.Result;
        }

        internal async Task<List<string>> GetLinesFromPageNumberAsync(int currentPage, int numberOfLines)
        {
            await using var fileStream = this.GetFileStreamAtPagePosition(currentPage);
            while (true)
            {
                var lineBlockResult = await this.ReadBlockAsync(fileStream);
                var lines = this.ExtractLines(lineBlockResult.LineBlock);
                if (!lineBlockResult.FileEndReached && this.AdjustBuffersize(lines.Count - 1, numberOfLines))
                {
                    continue;
                }
                return lines.Take(numberOfLines).ToList();
            }
        }

        internal FileStream GetFileStreamAtPagePosition(int currentPage)
        {
            int pageToReturn = currentPage <= this.pagePositions.Count ? currentPage : this.pagePositions.Count;
            var offset = this.pagePositions[pageToReturn - 1];
            FileStream fs = File.OpenRead(this.filePath);
            fs.Seek(offset, SeekOrigin.Begin);
            return fs;
        }

        internal bool AdjustBuffersize(int numberOfAvailableLines, int numberOfRequiredLines)
        {
            if (numberOfAvailableLines < numberOfRequiredLines)
            {
                this.buffersize *= 2;
                return true;
            }

            return false;
        }

        internal async Task<LineBlockReturnModel> ReadBlockAsync(FileStream fs)
        {
            byte[] b = new byte[this.buffersize];
            var numberOfBytesRead= await fs.ReadAsync(b, 0, b.Length);
            var str = this.encoding.GetString(b);
            return new LineBlockReturnModel()
            {
                LineBlock = str,
                FileEndReached = numberOfBytesRead<b.Length
            };
        }

        internal List<string> ExtractLines(string lineBlock)
        {
            var result = new List<string>();

            while (true)
            {
                var nextLine = this.ExtractLine(lineBlock);
                if (nextLine == null)
                {
                    return result.Where(l => l.Length > 0).ToList();
                }

                lineBlock = lineBlock.Substring(nextLine.Length);
                result.Add(nextLine);
            }
        }

        internal string ExtractLine(string lineBlock)
        {
            int indexOfCr = lineBlock.IndexOf('\r');
            int indexOfLf = lineBlock.IndexOf('\n');

            if (indexOfCr == -1 && indexOfLf == -1)
            {
                if (string.IsNullOrEmpty(lineBlock)) return null;
                return lineBlock;
            }
            if (indexOfLf == -1) // \r
            {
                return lineBlock.Substring(0, indexOfCr + 1);
            }
            if (indexOfCr == -1) // \n
            {
                return lineBlock.Substring(0, indexOfLf + 1);
            }
            if (indexOfLf - indexOfCr == 1) // \r\n
            {
                return lineBlock.Substring(0, indexOfLf + 1);
            }
            if (indexOfLf - indexOfCr != 1)
            {
                int firstOccurenceIndex = indexOfLf < indexOfCr ? indexOfLf : indexOfCr;
                return lineBlock.Substring(0, firstOccurenceIndex + 1);
            }

            return null;
        }


        internal async Task FindPageStartOffsets()
        {
            while (true)
            {
                await using var fileStream = this.GetFileStreamAtPagePosition(1);
                var lineBlockResult = await this.ReadBlockAsync(fileStream);
                if (string.IsNullOrWhiteSpace(lineBlockResult.LineBlock))
                {
                    this.FileReadFinished = true;
                    break;
                }

                var lines = this.ExtractLines(lineBlockResult.LineBlock);

                if (!lineBlockResult.FileEndReached && this.AdjustBuffersize(lines.Count - 1, this.numberOfLinesPerPage))
                {
                    continue;
                }

                //if (string.IsNullOrEmpty(titleLine))
                //{
                //    titleLine = lines[0];
                //    lines = lines.Skip(1).ToList();
                //}

                var page = lines.Take(this.numberOfLinesPerPage).ToList();
                int endOfPage = lineBlockResult.LineBlock.IndexOf(lines.Skip(this.numberOfLinesPerPage).Take(1).First());
                var pageBlock = lineBlockResult.LineBlock.Substring(0, endOfPage - 1);
                var pageBlockBytes = this.encoding.GetBytes(pageBlock).Length;
                this.pagePositions.Add(pageBlockBytes + this.pagePositions.LastOrDefault());


                //if (bytesRead < buffersize)
                //{
                //    break;
                //}

                fileStream.Seek(this.pagePositions.Last(), SeekOrigin.Begin);
            }
        }

        // aus Zeitgründen kann die Funktion nur wenige Encodings erkennen
        internal bool GetFileEncoding()
        {
            using var reader = new StreamReader(this.filePath, true);

            reader.Peek();
            try
            {
                this.encoding = reader.CurrentEncoding;
            }
            catch (Exception ex)
            {
                Console.WriteLine("file encoding not supported, quitting");
                return false;
            }

            return this.encoding != null;
        }
    }
}
