using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using InventoryManagement.Models;
using System.IO;
using NPOI.XSSF.UserModel;
using System;
using Microsoft.AspNetCore.Http;
using NPOI.SS.UserModel;

namespace InventoryManagement.Controllers
{
    /// <summary>
    /// Class to handle all requests on the / route
    /// </summary>
    [ApiController]
    [Route("")]
    public class BaseController : ControllerBase
    {
        /// <summary>
        /// Internal logger provided by ASP.Net
        /// </summary>
        private readonly ILogger<BaseController> _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger provided by ASP.Net</param>
        public BaseController(ILogger<BaseController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Respond to a GET request on all ItemTypes
        /// </summary>
        /// <returns>A list of all itemtypes</returns>
        [HttpGet]
        public ActionResult<string> Get()
        {
            if (!AuthenticationController.CheckToken(HttpContext, out string _))
            {
                return Ok(@"Not logged in yet, go to https://{{site}}/authenticate to log in.");
            }
            else
            {
                return Ok(@"Logged in, you can use the api now.");
            }
        }

        [HttpGet("checkauth")]
        public IActionResult CheckAuth()
        {
            if (!AuthenticationController.CheckToken(HttpContext, out string _))
            {
                return Unauthorized();
            }
            else
            {
                return Ok();
            }
        }

        [HttpGet("export")]
        public IActionResult Export()
        {
            if (!AuthenticationController.CheckToken(HttpContext, out string _))
            {
                return Unauthorized();
            }

            var stream = new MemoryStream();

            var excelFile = new XSSFWorkbook();

            var creationHelper = excelFile.GetCreationHelper();

            var textStyle = excelFile.CreateCellStyle();
            textStyle.DataFormat = creationHelper.CreateDataFormat().GetFormat(@"@");

            var upcStyle = excelFile.CreateCellStyle();
            upcStyle.DataFormat = creationHelper.CreateDataFormat().GetFormat(@"0");

            var currencyStyle = excelFile.CreateCellStyle();
            currencyStyle.DataFormat = creationHelper.CreateDataFormat().GetFormat(@"$#,##0.00_);[Red]($#,##0.00)");

            var dateStyle = excelFile.CreateCellStyle();
            dateStyle.DataFormat = creationHelper.CreateDataFormat().GetFormat(@"[$-en-US]m/d/yy h:mm AM/PM;@");

            var itemTypesSheet = excelFile.CreateSheet("ItemTypes");

            // add the header rows
            var headerRow = itemTypesSheet.CreateRow(0);

            var cell = headerRow.CreateCell(0);
            cell.SetCellValue("Name");
            cell.CellStyle = textStyle;

            cell = headerRow.CreateCell(1);
            cell.SetCellValue("UPC");
            cell.CellStyle = textStyle;

            cell = headerRow.CreateCell(2);
            cell.SetCellValue("ImageURL");
            cell.CellStyle = textStyle;

            var itemsSheet = excelFile.CreateSheet("Items");

            headerRow = itemsSheet.CreateRow(0);

            cell = headerRow.CreateCell(0);
            cell.SetCellValue("Id");
            cell.CellStyle = textStyle;

            cell = headerRow.CreateCell(1);
            cell.SetCellValue("ItemTypeUPC");
            cell.CellStyle = textStyle;

            cell = headerRow.CreateCell(2);
            cell.SetCellValue("OrderNumberToSeller");
            cell.CellStyle = textStyle;

            cell = headerRow.CreateCell(3);
            cell.SetCellValue("OrderNumberToBuyer");
            cell.CellStyle = textStyle;

            cell = headerRow.CreateCell(4);
            cell.SetCellValue("QRCode");
            cell.CellStyle = textStyle;

            cell = headerRow.CreateCell(5);
            cell.SetCellValue("ItemStatus");
            cell.CellStyle = textStyle;

            cell = headerRow.CreateCell(6);
            cell.SetCellValue("PricePaidBySeller");
            cell.CellStyle = textStyle;

            cell = headerRow.CreateCell(7);
            cell.SetCellValue("TaxPaidBySeller");
            cell.CellStyle = textStyle;

            cell = headerRow.CreateCell(8);
            cell.SetCellValue("ShippingCostToSeller");
            cell.CellStyle = textStyle;

            cell = headerRow.CreateCell(9);
            cell.SetCellValue("ShippingCostToBuyer");
            cell.CellStyle = textStyle;

            cell = headerRow.CreateCell(10);
            cell.SetCellValue("Fees");
            cell.CellStyle = textStyle;

            cell = headerRow.CreateCell(11);
            cell.SetCellValue("OtherExpenses");
            cell.CellStyle = textStyle;

            cell = headerRow.CreateCell(12);
            cell.SetCellValue("ShippingPaidByBuyer");
            cell.CellStyle = textStyle;

            cell = headerRow.CreateCell(13);
            cell.SetCellValue("PricePaidByBuyer");
            cell.CellStyle = textStyle;

            cell = headerRow.CreateCell(14);
            cell.SetCellValue("TimeStampOrdered");
            cell.CellStyle = textStyle;

            cell = headerRow.CreateCell(15);
            cell.SetCellValue("TimeStampReceived");
            cell.CellStyle = textStyle;

            cell = headerRow.CreateCell(16);
            cell.SetCellValue("TimeStampSold");
            cell.CellStyle = textStyle;


            using (var db = new DatabaseAccess())
            {
                var itemTypes = db.ItemTypes.ToArray();

                for (int i = 0; i < itemTypes.Length; i++)
                {
                    var row = itemTypesSheet.CreateRow(i + 1);

                    var itemType = itemTypes[i];

                    cell = row.CreateCell(0);
                    cell.SetCellValue(itemType.Name ?? "");
                    cell.CellStyle = textStyle;

                    cell = row.CreateCell(1);
                    cell.SetCellValue(itemType.UPC.ToString());
                    cell.CellStyle = upcStyle;

                    cell = row.CreateCell(2);
                    cell.SetCellValue(itemType.ImageURL ?? "");
                    cell.CellStyle = textStyle;

                }

                var items = db.Items.ToArray();

                for (int i = 0; i < items.Length; i++)
                {
                    var row = itemsSheet.CreateRow(i + 1);

                    var item = items[i];

                    cell = row.CreateCell(0);
                    cell.SetCellValue(item.Id.ToString());
                    cell.CellStyle = upcStyle;

                    cell = row.CreateCell(1);
                    cell.SetCellValue(item.ItemTypeUPC.ToString());
                    cell.CellStyle = upcStyle;

                    cell = row.CreateCell(2);
                    cell.SetCellValue(item.OrderNumberToSeller ?? "");
                    cell.CellStyle = textStyle;

                    cell = row.CreateCell(3);
                    cell.SetCellValue(item.OrderNumberToBuyer ?? "");
                    cell.CellStyle = textStyle;

                    cell = row.CreateCell(4);
                    cell.SetCellValue(item.QRCode ?? "");
                    cell.CellStyle = textStyle;

                    cell = row.CreateCell(5);
                    cell.SetCellValue(Enum.GetName(typeof(ItemStatus), item.ItemStatus));
                    cell.CellStyle = textStyle;

                    var cell2 = row.CreateCell(6);
                    cell2.SetCellValue((double)item.PricePaidBySeller);
                    cell2.CellStyle = currencyStyle;

                    cell = row.CreateCell(7);
                    cell.SetCellValue((double)item.TaxPaidBySeller);
                    cell.CellStyle = currencyStyle;

                    cell = row.CreateCell(8);
                    cell.SetCellValue((double)item.ShippingCostToSeller);
                    cell.CellStyle = currencyStyle;

                    cell = row.CreateCell(9);
                    cell.SetCellValue((double)item.ShippingCostToBuyer);
                    cell.CellStyle = currencyStyle;

                    cell = row.CreateCell(10);
                    cell.SetCellValue((double)item.Fees);
                    cell.CellStyle = currencyStyle;

                    cell = row.CreateCell(11);
                    cell.SetCellValue((double)item.OtherExpenses);
                    cell.CellStyle = currencyStyle;

                    cell = row.CreateCell(12);
                    cell.SetCellValue((double)item.ShippingPaidByBuyer);
                    cell.CellStyle = currencyStyle;

                    cell = row.CreateCell(13);
                    cell.SetCellValue((double)item.PricePaidByBuyer);
                    cell.CellStyle = currencyStyle;

                    cell = row.CreateCell(14);
                    cell.SetCellValue(item.TimeStampOrdered);
                    cell.CellStyle = dateStyle;

                    cell = row.CreateCell(15);
                    cell.SetCellValue(item.TimeStampReceived);
                    cell.CellStyle = dateStyle;

                    cell = row.CreateCell(16);
                    cell.SetCellValue(item.TimeStampSold);
                    cell.CellStyle = dateStyle;
                }
            }
            excelFile.Write(stream, true);

            stream.Position = 0;
            string excelName = $"DatabaseDump-{DateTime.UtcNow:yyyy-MM-dd-hh:mm:ss:fffffff}.xlsx";

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
        }

        [HttpPut("import")]
        public IActionResult Import()
        {
            if (!AuthenticationController.CheckToken(HttpContext, out string _))
            {
                return Unauthorized();
            }

            IFormFile file = Request.Form.Files[0];

            using (var stream = new MemoryStream())
            {
                // copy file into memory
                file.CopyTo(stream);

                // reset stream position
                stream.Position = 0;

                // create excel file abstraction over memory bytes
                var excelFile = new XSSFWorkbook(stream);

                var itemTypesTable = excelFile.GetSheet("ItemTypes");
                var itemsTable = excelFile.GetSheet("Items");

                var headerRow = itemTypesTable.GetRow(0).Cells;

                List<int> itemTypesColumnIndices = new List<int>(headerRow.Count)
                {
                    headerRow.Where(cell => cell.StringCellValue == "Name").First().ColumnIndex,
                    headerRow.Where(cell => cell.StringCellValue == "UPC").First().ColumnIndex,
                    headerRow.Where(cell => cell.StringCellValue == "ImageURL").First().ColumnIndex
                };

                headerRow = itemsTable.GetRow(0).Cells;

                List<int> itemsColumnIndices = new List<int>(headerRow.Count)
                {
                    headerRow.Where(cell => cell.StringCellValue == "Id").First().ColumnIndex,
                    headerRow.Where(cell => cell.StringCellValue == "ItemTypeUPC").First().ColumnIndex,
                    headerRow.Where(cell => cell.StringCellValue == "OrderNumberToSeller").First().ColumnIndex,
                    headerRow.Where(cell => cell.StringCellValue == "OrderNumberToBuyer").First().ColumnIndex,
                    headerRow.Where(cell => cell.StringCellValue == "QRCode").First().ColumnIndex,
                    headerRow.Where(cell => cell.StringCellValue == "ItemStatus").First().ColumnIndex,
                    headerRow.Where(cell => cell.StringCellValue == "PricePaidBySeller").First().ColumnIndex,
                    headerRow.Where(cell => cell.StringCellValue == "TaxPaidBySeller").First().ColumnIndex,
                    headerRow.Where(cell => cell.StringCellValue == "ShippingCostToSeller").First().ColumnIndex,
                    headerRow.Where(cell => cell.StringCellValue == "ShippingCostToBuyer").First().ColumnIndex,
                    headerRow.Where(cell => cell.StringCellValue == "Fees").First().ColumnIndex,
                    headerRow.Where(cell => cell.StringCellValue == "OtherExpenses").First().ColumnIndex,
                    headerRow.Where(cell => cell.StringCellValue == "ShippingPaidByBuyer").First().ColumnIndex,
                    headerRow.Where(cell => cell.StringCellValue == "PricePaidByBuyer").First().ColumnIndex,
                    headerRow.Where(cell => cell.StringCellValue == "TimeStampOrdered").First().ColumnIndex,
                    headerRow.Where(cell => cell.StringCellValue == "TimeStampReceived").First().ColumnIndex,
                    headerRow.Where(cell => cell.StringCellValue == "TimeStampSold").First().ColumnIndex
                };

                using var db = new DatabaseAccess();

                // clear table
                db.ItemTypes.RemoveRange(db.ItemTypes);

                DataFormatter formatter = new DataFormatter();

                var getStringValue = new Func<ICell, string>(cell =>
                {
                    var val = formatter.FormatCellValue(cell);
                    return string.IsNullOrEmpty(val) ? null : val;
                });

                for (int i = 1; i < itemTypesTable.PhysicalNumberOfRows; i++)
                {
                    var row = itemTypesTable.GetRow(i);

                    var itemType = new ItemType
                    {
                        Name = getStringValue(row.GetCell(itemTypesColumnIndices[0])),
                        UPC = long.Parse(formatter.FormatCellValue(row.GetCell(itemTypesColumnIndices[1]))),
                        ImageURL = getStringValue(row.GetCell(itemTypesColumnIndices[2]))
                    };

                    db.ItemTypes.Add(itemType);
                }

                // clear table
                db.Items.RemoveRange(db.Items);

                for (int i = 1; i < itemsTable.PhysicalNumberOfRows; i++)
                {
                    var row = itemsTable.GetRow(i);

                    var extractDate = new Func<ICell, DateTime>(cell =>
                    {
                        if (cell.CellType == CellType.Numeric && cell.NumericCellValue != -1)
                        {
                            return cell.DateCellValue;
                        }
                        else
                        {
                            return new DateTime();
                        }
                    });

                    var item = new Item
                    {
                        Id = 0,
                        ItemTypeUPC = long.Parse(formatter.FormatCellValue(row.GetCell(itemsColumnIndices[1]))),
                        OrderNumberToSeller = getStringValue(row.GetCell(itemsColumnIndices[2])),
                        OrderNumberToBuyer = getStringValue(row.GetCell(itemsColumnIndices[3])),
                        QRCode = getStringValue(row.GetCell(itemsColumnIndices[4])),
                        ItemStatus = (ItemStatus)Enum.Parse(typeof(ItemStatus), row.GetCell(itemsColumnIndices[5]).StringCellValue),
                        PricePaidBySeller = (decimal)row.GetCell(itemsColumnIndices[6]).NumericCellValue,
                        TaxPaidBySeller = (decimal)row.GetCell(itemsColumnIndices[7]).NumericCellValue,
                        ShippingCostToSeller = (decimal)row.GetCell(itemsColumnIndices[8]).NumericCellValue,
                        ShippingCostToBuyer = (decimal)row.GetCell(itemsColumnIndices[9]).NumericCellValue,
                        Fees = (decimal)row.GetCell(itemsColumnIndices[10]).NumericCellValue,
                        OtherExpenses = (decimal)row.GetCell(itemsColumnIndices[11]).NumericCellValue,
                        ShippingPaidByBuyer = (decimal)row.GetCell(itemsColumnIndices[12]).NumericCellValue,
                        PricePaidByBuyer = (decimal)row.GetCell(itemsColumnIndices[13]).NumericCellValue,
                        TimeStampOrdered = extractDate(row.GetCell(itemsColumnIndices[14])),
                        TimeStampReceived = extractDate(row.GetCell(itemsColumnIndices[15])),
                        TimeStampSold = extractDate(row.GetCell(itemsColumnIndices[16])),
                    };

                    db.Items.Add(item);
                }

                db.SaveChanges();
            }

            return Accepted();
        }
    }
}
