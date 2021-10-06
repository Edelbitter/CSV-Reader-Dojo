using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tabellieren_uebung
{
    public class Tabellizer
    {
        private FileAnalizer fileAnalizer;
        private OutputFormatter outputFormatter;

        public Tabellizer(string filePath,int numberOfLinesPerPage)
        {
            this.fileAnalizer = new FileAnalizer(filePath,numberOfLinesPerPage);
            this.outputFormatter = new OutputFormatter();
        }

        public async Task StartAnalyzingAsync()
        {

        }
    }
}
