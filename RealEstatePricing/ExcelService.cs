using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealEstatePricing
{
    public  class ExcelService
    {
        public string FileName { get; set; }
        public string Pathway { get; set; }


        public ExcelService(string file, string path)
        {
            FileName = file;
            Pathway = path;
        }

        internal void Add(Town townData)
        {
            string path = Pathway + FileName;
            var exApp = new Microsoft.Office.Interop.Excel.Application();
            var exWbk = exApp.Workbooks.Open(path);
            var exWks = exWbk.Sheets[townData.TownName];
        }
    }
}
