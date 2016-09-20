//using Microsoft.Office.Interop.Excel;
//using System;
//using System.IO;

//namespace DST_PMPortal.Services
//{
//    class ExcelOperations
//    {
//        internal static void WriteJSONtoXLSX(string _filePath, string _json)
//        {
//            var xlApp = new Application();
//            var xlWorkbook = xlApp.Application.Workbooks.Add();
//            var xlWorksheet = xlWorkbook.Worksheets[1];
//            var usedRange = xlWorksheet.UsedRange;
//            //var lastRow = usedRange.Find("*", SearchOrder: XlSearchOrder.xlByRows, SearchDirection: XlSearchDirection.xlPrevious).Row;
//            //var range = xlWorksheet.Range[xlWorksheet.Cells[3, 2], xlWorksheet.Cells[lastRow, 9]];

//            //xlWorksheet.Cells[1, 1] = $"{}";

//            xlWorkbook.SaveAs(_filePath);
//            xlWorkbook.Close();
//            xlApp.Quit();
//        }
//        internal static void WritePortfoliotoXLSX(string _filePath, Portfolio _Portfolio)
//        {
//            var xlApp = new Portfolio();
//            xlApp.DisplayAlerts = false;
//            var xlWorkbook = xlApp.Application.Workbooks.Add();
//            var xlWorksheet = xlWorkbook.Worksheets[1];

//            try
//            {
//                ////var usedRange = xlWorksheet.UsedRange;
//                ////var lastRow = usedRange.Find("*", SearchOrder: XlSearchOrder.xlByRows, SearchDirection: XlSearchDirection.xlPrevious).Row;
//                ////var range = xlWorksheet.Range[xlWorksheet.Cells[3, 2], xlWorksheet.Cells[lastRow, 9]];

//                //xlWorksheet.Cells[1, 1] = $"{_Portfolio.PMName}'s Pokedex";
//                //xlWorksheet.Cells[2, 1] = "Name";
//                //xlWorksheet.Cells[2, 2] = "In Pokedex?";
//                //xlWorksheet.Cells[2, 3] = "# of Pokemon";
//                //xlWorksheet.Cells[2, 4] = "# of Candies";
//                //xlWorksheet.Cells[2, 5] = "Evolution Cost (Candy)";
//                //xlWorksheet.Cells[2, 6] = "Next Stage";
//                //var r = 3;
//                //foreach (var p in _pokedex.Inventory)
//                //{
//                //    var c = 1;
//                //    xlWorksheet.Cells[r, c++] = p.name;
//                //    xlWorksheet.Cells[r, c++] = p.inPokedex;
//                //    xlWorksheet.Cells[r, c++] = p.qtyPokemon;
//                //    xlWorksheet.Cells[r, c++] = p.qtyCandy;
//                //    xlWorksheet.Cells[r, c++] = p.candyToEvolve;
//                //    xlWorksheet.Cells[r, c++] = p.nextStage;
//                //    r++;
//                //}
//                //if (File.Exists(_filePath)) File.Delete(_filePath);
//                //xlWorkbook.SaveAs(_filePath);

//            }
//            catch (Exception _ExcelWriteData)
//            {

//            }
//            finally
//            {
//                xlWorkbook.Close();
//                xlApp.Quit();
//            }
//        }
//        internal static Portfolio GetData(string _filePath)
//        {
//            var dex = new Portfolio();
//            var xlApp = new Application();
//            xlApp.DisplayAlerts = false;
//            var xlWorkbook = xlApp.Application.Workbooks.Open(_filePath);
//            var xlWorksheet = xlWorkbook.Worksheets[1];

//            try
//            {
//                //dex.userName = xlWorksheet.Cells[1, 1].Value;
//                //var r = 3;
//                //while (xlWorksheet.Cells[r, 1].Value != string.Empty && xlWorksheet.Cells[r, 1].Value != null)
//                //{
//                //    var c = 1;
//                //    string name = xlWorksheet.Cells[r, c++].Value;
//                //    bool inPokedex = xlWorksheet.Cells[r, c++].Value;
//                //    int qtyPokemon = int.Parse(xlWorksheet.Cells[r, c++].Value.ToString());
//                //    int qtyCandy = int.Parse(xlWorksheet.Cells[r, c++].Value.ToString());
//                //    int candyToEvolve = int.Parse(xlWorksheet.Cells[r, c++].Value.ToString());
//                //    string nextStage = xlWorksheet.Cells[r, c++].Value;
//                //    dex.Inventory.Add(new Pokemon(name, inPokedex, qtyPokemon, qtyCandy, candyToEvolve, nextStage));
//                //    r++;
//                //}
//                return dex;
//            }
//            catch (Exception _ExcelReadData)
//            {
//                return null;
//            }
//            finally
//            {
//                xlWorkbook.Close();
//                xlApp.Quit();
//            }
//        }
//    }

//}
