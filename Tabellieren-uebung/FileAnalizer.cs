using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tabellieren_uebung
{
    public class FileAnalizer
    {
        private string filePath;
        private int numberOfLinesPerPage;
        private Encoding encoding;

        private int buffersize;
        private List<string> columns;
        private string titleLine;

        private List<int> pagePositions = new List<int>() { 0 };

        public List<string> Columns { get; private set; }
        public bool FileReadFinished { get; private set; }

        public FileAnalizer(string filePath, int numberOfLinesPerPage, Encoding encoding)
        {
            this.filePath = filePath;
            this.numberOfLinesPerPage = numberOfLinesPerPage;
            this.encoding = encoding;

            this.buffersize = 1024;
            this.Initialize();

        }

        private void Initialize()
        {
            this.GetTitleLine();
            this.Columns = this.titleLine.Split(';').ToList();

            this.FileReadFinished = false;
            _ = this.FindPageStartOffsets();

        }

        private void GetTitleLine()
        {
            var firstLine = this.GetLinesFromPageNumber(1, 1).FirstOrDefault();
            var titleLineBytes = this.encoding.GetBytes(firstLine);
            this.pagePositions[0] = titleLineBytes.Length;
            this.titleLine = $"No.{firstLine.Trim()}";
        }

        private List<string> GetLinesFromPageNumber(int currentPage, int numberOfLines)
        {
            var task = this.GetLinesFromPageNumberAsync(currentPage, numberOfLines);
            task.Wait();
            return task.Result;
        }

        private async Task<List<string>> GetLinesFromPageNumberAsync(int currentPage, int numberOfLines)
        {
            var fileStream = this.GetFileStreamAtPagePosition(currentPage);
            while (true)
            {
                var lineBlock = await this.ReadBlockAsync(fileStream);
                var lines = this.ExtractLines(lineBlock);
                if (this.AdjustBuffersize(lines.Count - 1, numberOfLines))
                {
                    continue;
                }
                return lines;
            }
        }

        private FileStream GetFileStreamAtPagePosition(int currentPage)
        {
            var offset = this.pagePositions[currentPage - 1];
            using FileStream fs = File.OpenRead(this.filePath);
            fs.Seek(offset, SeekOrigin.Begin);
            return fs;
        }

        private bool AdjustBuffersize(int numberOfAvailableLines, int numberOfRequiredLines)
        {
            if (numberOfAvailableLines < numberOfRequiredLines)
            {
                this.buffersize *= 2;
                return true;
            }

            return false;
        }

        private async Task<string> ReadBlockAsync(FileStream fs)
        {
            byte[] b = new byte[this.buffersize];
            await fs.ReadAsync(b, 0, b.Length);
            var str = this.encoding.GetString(b);
            return str;
        }

        private List<string> ExtractLines(string lineBlock)
        {
            var result = new List<string>();

            while (true)
            {
                var nextLine = this.ExtractLine(lineBlock);
                if (nextLine == null)
                {
                    return result.Where(l => l.Length > 0).ToList();
                }
                result.Add(nextLine);
            }
        }

        private string ExtractLine(string lineBlock)
        {
            int indexOfCr = lineBlock.IndexOf('\r');
            int indexOfLf = lineBlock.IndexOf('\n');

            if (indexOfCr == -1 && indexOfLf == -1)
            {
                if (string.IsNullOrEmpty(lineBlock)) return null;
                return lineBlock;
            }
            if (indexOfLf == -1)
            {
                return lineBlock.Substring(0, indexOfCr);
            }
            if (indexOfCr == -1)
            {
                return lineBlock.Substring(0, indexOfLf);
            }
            if (indexOfLf - indexOfCr == 1)
            {
                return lineBlock.Substring(0, indexOfLf);
            }

            return null;
        }


        private async Task FindPageStartOffsets()
        {
            while (true)
            {
                var fileStream = this.GetFileStreamAtPagePosition(1);
                var lineBlock = await this.ReadBlockAsync(fileStream);
                if (string.IsNullOrWhiteSpace(lineBlock))
                {
                    this.FileReadFinished = true;
                    break;
                }

                var lines = this.ExtractLines(lineBlock);

                if (this.AdjustBuffersize(lines.Count - 1, this.numberOfLinesPerPage)
                && this.pagePositions.Count == 1)
                {
                    continue;
                }

                //if (string.IsNullOrEmpty(titleLine))
                //{
                //    titleLine = lines[0];
                //    lines = lines.Skip(1).ToList();
                //}

                var page = lines.Take(this.numberOfLinesPerPage).ToList();
                int endOfPage = lineBlock.IndexOf(lines.Skip(this.numberOfLinesPerPage).Take(1).First());
                var pageBlock = lineBlock.Substring(0, endOfPage - 1);
                var pageBlockBytes = this.encoding.GetBytes(pageBlock).Length;
                this.pagePositions.Add(pageBlockBytes + this.pagePositions.LastOrDefault());


                //if (bytesRead < buffersize)
                //{
                //    break;
                //}

                fileStream.Seek(this.pagePositions.Last(), SeekOrigin.Begin);
            }
        }


    }
}
