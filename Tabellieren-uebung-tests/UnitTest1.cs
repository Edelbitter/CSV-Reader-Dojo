using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Tabellieren_uebung;

namespace Tabellieren_uebung_tests
{
    public class Tests
    {
        private string[] input = { "Name;Alter;Ort", "Paul;17;Muenchen" };
        private string[] output = { "+----+-----+--------+", "|Name|Alter|Ort     |", "+----+-----+--------+", "|Paul|17   |Muenchen|", "+----+-----+--------+" };

        
        [Test]
        public void TabellierenWorks()
        {
            var actual = Program.Tabellieren(input);
            actual.Should().BeEquivalentTo(output);
        }

        [Test]
        public void GenerateTextLinesWorks()
        {
            List<int> columnLengths = new List<int>() { 3, 4, 5 };
            List<List<string>> words = new List<List<string>>() { new List<string>() { "123", "456", "789" }, new List<string>() { "123", "456", "789" } };

            string[] result = new string[5];
            Program.GenerateTextLines(5, 3, columnLengths, words, result);

            result.Should().BeEquivalentTo(new string[] {null, "|123|456 |789  |", null, "|123|456 |789  |", null});
        }

        [Test]
        public void CalculateColumnLenghtsWorks()
        {
            var actual = Program.CalculateColumnLenghts(4, 3,
                new List<List<string>>()
                {
                    new List<string>() {"abc", "xxxxx", "1", "99"}, 
                    new List<string>() {"a", "b", "c", "d"},
                    new List<string>() {"xxxxxxxxxxxx", "abcd", "22", "100"}
                });

            actual.Should().BeEquivalentTo(new List<int> {12, 5, 2, 3});
        }

        [Test]
        public void PutWordsIn2DimensionalArrayWorks()
        {
            var actual = Program.PutWordsIn2DimensionalArray(new string[] {"horst;hugo;klaus"}, 1);
            actual.Should().BeEquivalentTo(new List<List<string>> {new List<string>() {"horst", "hugo", "klaus"}});
        }

    }
}