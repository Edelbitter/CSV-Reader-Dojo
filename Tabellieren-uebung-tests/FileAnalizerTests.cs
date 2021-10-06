using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Tabellieren_uebung;

namespace Tabellieren_uebung_tests
{
    public class FileAnalizerTests
    {
        private FileAnalizer fileAnalizer;

        [SetUp]
        public void Setup()
        {
            
        }

        [Test]
        public void SetTitleLineSetsCorrectLine()
        {
            // arrange
            this.fileAnalizer = new FileAnalizer("personen.csv", 5, true);
            this.fileAnalizer.GetFileEncoding();

            // act
            this.fileAnalizer.SetTitleLine();

            // assert
            this.fileAnalizer.titleLine.Should().BeEquivalentTo("No.;Name;Vorname;Strasse;Ort;Alter");
            this.fileAnalizer.pagePositions.First().Should().Be(31);
        }

        [Test]
        public void SetFirstPagePositionAtEndOfTitleLineSetsCorrectValues()
        {
            // arrange
            this.fileAnalizer = new FileAnalizer("personen.csv", 5, true);
            this.fileAnalizer.GetFileEncoding();

            // act
            this.fileAnalizer.SetFirstPagePositionAtEndOfTitleLine("Name;Vorname;Strasse;Ort;Alter\n");

            // assert
            this.fileAnalizer.pagePositions.First().Should().Be(31);
        }
        
        [Test]
        public async Task GetLinesFromPageNumberAsyncReturnsCorrectLines()
        {
            // arrange
            this.fileAnalizer = new FileAnalizer("personen.csv", 5, true);
            this.fileAnalizer.GetFileEncoding();

            // act
            var actualLines =
                await this.fileAnalizer.GetLinesFromPageNumberAsync(3, this.fileAnalizer.numberOfLinesPerPage);

            // assert
            actualLines.Count.Should().Be(5);

        }

        [Test]
        public void GetFileStreamAtPagePositionReturnsCorrectStream()
        {
            // arrange
            this.fileAnalizer = new FileAnalizer("leer.csv", 5, true);
            this.fileAnalizer.pagePositions = new List<int>(){ 1, 2, 3 };

            // act
            using var actualStream = this.fileAnalizer.GetFileStreamAtPagePosition(3);

            // assert
            actualStream.Should().NotBeNull();
            actualStream.Position.Should().Be(3);

        }

        [TestCase(2,3,true,200)]
        [TestCase(3,3,false,100)]
        [TestCase(4,3,false,100)]
        public void AdjustBuffersizeAdustsCorrectly(int first,int second, bool shouldAdjust,int expectedBuffersize)
        {
            // arrange
            this.fileAnalizer = new FileAnalizer("leer.csv", 5, true);
            this.fileAnalizer.buffersize = 100;

            // act
            var actuallyAdjusted = this.fileAnalizer.AdjustBuffersize(first, second);

            // assert
            actuallyAdjusted.Should().Be(shouldAdjust);
            this.fileAnalizer.buffersize.Should().Be(expectedBuffersize);
        }

        [Test]
        public async Task ReadBlockReturnsCorrectString()
        {
            // arrange
            this.fileAnalizer = new FileAnalizer("personen.csv", 5, true);
            this.fileAnalizer.GetFileEncoding();

            // act
            await using FileStream fs = File.OpenRead(this.fileAnalizer.filePath);
            var actual = await this.fileAnalizer.ReadBlockAsync(fs);

            // assert
            actual.LineBlock.Should().BeEquivalentTo("Name;Vorname;Strasse;Ort;Alter\nYang;Paloma;P.O. Box 451, 8598 Donec Road;Bell;88\nDiaz;Jayme;Ap #486-1122 Vestibulum Road;Lafayette;45\nValencia;Kelly;755-2020 Erat. Rd.;Lincoln;60\nLindsay;Knox;Ap #732-8577 Pharetra Ave;West Sacramento;68\nDonovan;Rowan;P.O. Box 356, 2474 Pede. St.;Jacksonville;75\nNielsen;Keane;851-1918 Morbi St.;Wilmington;12\nUnderwood;Cassidy;Ap #267-5405 Faucibus Rd.;La Palma;67\nDickerson;Finn;Ap #563-8001 Est. Avenue;San Bernardino;52\nBonner;Alexa;Ap #566-2220 Eget Avenue;St. George;87\nBates;Amal;1158 Vulputate, St.;Santa Monica;48\nSanders;Zephania;Ap #370-6019 Habitant Street;Jeffersonville;26\nChapman;Vladimir;Ap #248-9755 Aliquam Avenue;Logan;8\nNielsen;Alec;3483 Nisl Rd.;Milford;83\nLarsen;Kaseem;P.O. Box 855, 3312 Convallis Street;Brea;11\nBeck;Hollee;Ap #891-4333 Vel Rd.;Saginaw;75\nWalters;Victoria;P.O. Box 526, 4759 Ultricies Rd.;Frankfort;90\nHuff;Mary;P.O. Box 175, 4057 Dignissim Road;Littleton;12\nDickerson;Sage;6454 Mollis. Road;Beckley;34\nGay;Wayne;7379 Erat St.;Attleboro;53\nCline;Anth");
            actual.FileEndReached.Should().BeFalse();
        }

        [TestCase("abc\ndef\rghi\r\njkl", "abc\n","def\r","ghi\r\n",4)]
        [TestCase("abc\ndef\rghi\r\njkl\n", "abc\n","def\r","ghi\r\n",4)]
        public void ExtractLinesExtractsCorrectLines(string input, string output1,string output2,string output3,int numberOfLinesFound)
        {
            // arrange
            this.fileAnalizer = new FileAnalizer("leer.csv", 5, true);

            // act
            var actual = this.fileAnalizer.ExtractLines(input);

            // assert
            actual[0].Should().BeEquivalentTo(output1);
            actual[1].Should().BeEquivalentTo(output2);
            actual[2].Should().BeEquivalentTo(output3);
            actual.Count.Should().Be(numberOfLinesFound);
        }

        [TestCase("abc\ndef\rghi\r\njkl", "abc\n")]
        [TestCase("abc\rdef\rghi\r\njkl", "abc\r")]
        [TestCase("abc\rdef\nghi\r\njkl", "abc\r")]
        [TestCase("abc\ndef","abc\n")]
        [TestCase("abc\r\ndef","abc\r\n")]
        [TestCase("abc\rdef","abc\r")]
        [TestCase("abcdef","abcdef")]
        [TestCase("",null)]
        public void ExtractLineExtractsFirstLine(string input, string output)
        {
            // arrange
            this.fileAnalizer = new FileAnalizer("leer.csv", 5, true);

            // act
            var actual = this.fileAnalizer.ExtractLine(input);

            // assert
            actual.Should().BeEquivalentTo(output);
        }

        // aus Zeitgründen kann die Funktion nur wenige Encodings erkennen
        [TestCase("personen.csv","Unicode (UTF-8)")]
        [TestCase("utf16.txt","Unicode")]
        public void CorrectEncodingIsFound(string filename, string encodingName)
        {
            // arrange
            this.fileAnalizer = new FileAnalizer(filename, 5,true);

            // act
            var foundEncoding = this.fileAnalizer.GetFileEncoding();

            // assert
            foundEncoding.Should().BeTrue();
            this.fileAnalizer.encoding.EncodingName.Should().BeEquivalentTo(encodingName);

        }
    }
}
