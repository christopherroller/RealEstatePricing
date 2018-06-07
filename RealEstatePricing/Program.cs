using HtmlAgilityPack;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace RealEstatePricing
{
    public class Program
    {
        //Get Config stuff:
        readonly static string[] towns = ConfigurationManager.AppSettings["towns"].Split('|');
        readonly static string rightMoveUrl = ConfigurationManager.AppSettings["rightMoveUrl"];
        readonly static string zooplaUrls = ConfigurationManager.AppSettings["zooplaUrl"];
        readonly static string fileName = ConfigurationManager.AppSettings["excelFileName"];
        readonly static string pathway = ConfigurationManager.AppSettings["excelFilePath"];

        public static void Main(string[] args)
        {
            var allTowns = new List<Town>();
            foreach(var town in towns)
            {
                Town townData = new Town();
                townData.TownName = town;
                townData.Month = DateTime.Today.ToString("MMM");
                //Get town data
                var data = GetTownData(town);
                
                //Parse pricing data:
                townData = GetPercentages(townData, data[2]);
                townData = GetAveragePrice(townData, data[1]);
                townData = GetSemiDetachedPrice(townData, data[0]);
                townData = GetTerracedPrice(townData, data[0]);
                townData = GetDetachedPrice(townData, data[0]);

                allTowns.Add(townData);

                ExcelService es = new ExcelService(fileName, pathway);
                es.Add(townData);
            }
        }

        private static Town GetDetachedPrice(Town town, string v)
        {
            Town t = town;
            if (v.IndexOf(Keywords.detachedProperties, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (v.Substring(v.IndexOf(Keywords.detachedProperties, StringComparison.OrdinalIgnoreCase) - "semi-".Length, "semi-".Length).Equals("semi-", StringComparison.OrdinalIgnoreCase)){
                    v = v.Substring(v.IndexOf(Keywords.detachedProperties, StringComparison.OrdinalIgnoreCase) + Keywords.detachedProperties.Length);
                }
                string val = string.Empty;
                var stringWithDetachedPrice = v.IndexOf(Keywords.detachedProperties, StringComparison.OrdinalIgnoreCase) >= 0 ? v.Substring(v.IndexOf(Keywords.detachedProperties, StringComparison.OrdinalIgnoreCase)) : v.Substring(0);
                var poundIndex = stringWithDetachedPrice.Substring(stringWithDetachedPrice.IndexOf(Keywords.poundEscapeChar) + Keywords.poundEscapeChar.Length);
                foreach (char c in poundIndex)
                {
                    if (char.IsDigit(c) || c == ',')
                    {
                        val += c.ToString();
                    }
                    else
                    {
                        break;
                    }
                }
                t.DetachedPrice = Convert.ToDecimal(val);
            }
            return t;
        }

        private static Town GetTerracedPrice(Town town, string v)
        {
            Town t = town;
            if (v.IndexOf(Keywords.terracedProperties, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                string val = string.Empty;
                var stringWithTerracedPrice = v.Substring(v.IndexOf(Keywords.terracedProperties, StringComparison.OrdinalIgnoreCase), Keywords.semiDetached.Length + 50);
                var poundIndex = stringWithTerracedPrice.Substring(stringWithTerracedPrice.IndexOf(Keywords.poundEscapeChar) + Keywords.poundEscapeChar.Length);
                foreach (char c in poundIndex)
                {
                    if (char.IsDigit(c) || c == ',')
                    {
                        val += c.ToString();
                    }
                    else
                    {
                        break;
                    }
                }
                t.TerracePrice = Convert.ToDecimal(val);
            }
            return t;
        }

        private static Town GetSemiDetachedPrice(Town town, string v)
        {
            Town t = town;
            if (v.IndexOf(Keywords.semiDetached, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                string val = string.Empty;
                var stringWithDetachedPrice = v.Substring(v.IndexOf(Keywords.semiDetached, StringComparison.OrdinalIgnoreCase));
                var poundIndex = stringWithDetachedPrice.Substring(stringWithDetachedPrice.IndexOf(Keywords.poundEscapeChar) + Keywords.poundEscapeChar.Length);
                foreach (char c in poundIndex)
                {
                    if (char.IsDigit(c) || c == ',')
                    {
                        val += c.ToString();
                    }
                    else
                    {
                        break;
                    }
                }
                t.SemiDetatchedPrice = Convert.ToDecimal(val);
            }
            return t;
        }

        private static Town GetAveragePrice(Town town, string v)
        { 
            Town t = town;
            if (v.Contains(Keywords.averagePrice))
            {
                string val = string.Empty;
                var indexOfAvgPhrase = v.Substring(v.IndexOf(Keywords.averagePrice) + Keywords.averagePrice.Length + 1);
                var poundIndex = indexOfAvgPhrase.Substring(indexOfAvgPhrase.IndexOf(Keywords.poundEscapeChar) + Keywords.poundEscapeChar.Length);
                foreach(char c in poundIndex)
                {                  
                    if(char.IsDigit(c) || c == ',')
                    {
                        val += c.ToString();
                    }
                    else
                    {
                        break;
                    }
                }
                t.AveragePrice = Convert.ToDecimal(val);
            }
            return t;
        }

        public static List<string> GetTownData(string townName)
        {
            List<string> townDataParagraphs = new List<string>();
            HtmlWeb web = new HtmlWeb();
            var townUrl = rightMoveUrl + townName + ".html";
            var htmlDoc = web.Load(townUrl);
            if (htmlDoc != null)
            {
                var div = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'content first')]/p");
                if (div != null && div.Count == 3)
                {
                    townDataParagraphs.Add(div[0].InnerText);
                    townDataParagraphs.Add(div[1].InnerText);
                    townDataParagraphs.Add(div[2].InnerText);
                }
                else
                {
                    Console.WriteLine("There was an issue getting data for " + townName);
                    Console.WriteLine("Check this URL works: " + townUrl);
                }
            }
            return townDataParagraphs;
        }

        public static Town  GetPercentages(Town town, string paragraph)
        {
            Town t = town;
            string expression = @"(\d{1,3})%";
            var avgLastYear = Regex.Match(paragraph, expression).ToString().Replace("%", "");
            paragraph = paragraph.Remove(0, paragraph.IndexOf(avgLastYear) + avgLastYear.Length + 1);
            var avg2015 = Regex.Match(paragraph, expression).ToString().Replace("%", "");
            t.PastYearsAvgPercent = avg2015 != null ? Decimal.Parse(avg2015) : 0;
            t.Last12MonthsAvgPercent = avgLastYear != null ? Decimal.Parse(avgLastYear) : 0;        
            return t;
        }
    }
}
