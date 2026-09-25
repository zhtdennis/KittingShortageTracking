using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using UFIDA.U9.MFG.MO.ManufactureSimulateUIModel;
using UFSoft.UBF.UI;
using UFSoft.UBF.UI.ControlModel;
using UFSoft.UBF.UI.Custom;
using UFSoft.UBF.UI.Engine.Builder;
using UFSoft.UBF.UI.IView;
using UFSoft.UBF.UI.MD.Runtime;
using UFSoft.UBF.UI.WebControlAdapter;

namespace U9Custom.UI.ManufactureSimulateShortageTable
{
    public class ManufactureSimulateShortageButtonPlugin : ExtendedPartBase
    {
        private const string TargetPartFullName = "UFIDA.U9.MFG.MO.ManufactureSimulateUIModel.ManufactureSimulateOptionUIFormWebPart";
        private const string ButtonId = "BtnShortageTable";
        private const string ButtonGuid = "6f6e7a18-2f4d-4d1f-85a7-8f2c8a0db3b1";
        private ManufactureSimulateOptionUIFormWebPart resultPart;
        private IUFDataGrid resultGrid;
        private IUFDataGrid optionGrid;
        private bool buttonAdded;

        public override void AfterInit(IPart part, EventArgs args)
        {
            base.AfterInit(part, args);
            TryInitialize(part, "AfterInit");
        }

        public override void AfterLoad(IPart part, EventArgs args)
        {
            base.AfterLoad(part, args);
            TryInitialize(part, "AfterLoad");
        }

        public override void AfterDataBinding(IPart part)
        {
            base.AfterDataBinding(part);
            TryInitialize(part, "AfterDataBinding");
        }

        private void TryInitialize(IPart part, string stage)
        {
            try
            {
                if (part == null || !string.Equals(part.GetType().FullName, TargetPartFullName, StringComparison.Ordinal))
                {
                    return;
                }

                resultPart = part as ManufactureSimulateOptionUIFormWebPart;
                if (resultPart == null)
                {
                    return;
                }

                resultGrid = part.GetUFControlByName(part.TopLevelContainer, "DataGrid3") as IUFDataGrid;
                optionGrid = part.GetUFControlByName(part.TopLevelContainer, "DataGridOptionBody") as IUFDataGrid;
                AddButton(stage);
            }
            catch (Exception ex)
            {
                if (resultPart != null)
                {
                    resultPart.Model.ErrorMessage.Message = "缺料表按钮加载失败：" + ex.Message;
                }
                Log(stage + " error: " + ex);
            }
        }

        private void AddButton(string stage)
        {
            if (buttonAdded || resultPart == null)
            {
                return;
            }

            if (resultPart.GetUFControlByName(resultPart.TopLevelContainer, ButtonId) != null)
            {
                buttonAdded = true;
                return;
            }

            IUFContainer hostContainer = resultPart.GetUFControlByName(resultPart.TopLevelContainer, "Card0") as IUFContainer;

            if (hostContainer == null)
            {
                IUFButton okButton = resultPart.GetUFControlByName(resultPart.TopLevelContainer, "BtnOk") as IUFButton;
                hostContainer = (okButton as WebControl)?.Parent as IUFContainer;
            }

            if (hostContainer == null)
            {
                IUFButton closeButton = resultPart.GetUFControlByName(resultPart.TopLevelContainer, "BtnClose") as IUFButton;
                hostContainer = (closeButton as WebControl)?.Parent as IUFContainer;
            }

            if (hostContainer == null)
            {
                return;
            }

            IUFButton button = UIControlBuilder.BuilderUFButton(
                hostContainer,
                true,
                ButtonId,
                true,
                true,
                100,
                20,
                2,
                0,
                1,
                1,
                "100",
                string.Empty,
                resultPart.Model.ElementID,
                "OnShortageTable",
                false,
                ButtonGuid,
                ButtonGuid,
                ButtonGuid);

            UIControlBuilder.SetButtonAccessKey(button);
            button.ID = ButtonId;
            button.Text = "缺料表";
            button.AutoPostBack = true;
            button.UIModel = resultPart.Model.ElementID;
            button.Click += Button_Click;
            UIControlBuilder.BuilderUFControl(button, "11");
            hostContainer.Controls.Add(button);
            buttonAdded = true;
        }

        private void Button_Click(object sender, EventArgs e)
        {
            try
            {
                resultPart.Model.ClearErrorMessage();
                if (resultGrid != null)
                {
                    resultGrid.CollectData();
                }
                if (optionGrid != null)
                {
                    optionGrid.CollectData();
                }

                List<ShortageRow> rows = BuildRows();
                if (rows.Count == 0)
                {
                    resultPart.Model.ErrorMessage.Message = "当前单据没有缺料量大于 0 的记录";
                    return;
                }

                RenderOverlay(rows);
            }
            catch (Exception ex)
            {
                resultPart.Model.ErrorMessage.Message = "打开缺料表失败：" + ex.Message;
                Log("Button_Click error: " + ex);
            }
        }

        private List<ShortageRow> BuildRows()
        {
            var rows = new List<ShortageRow>();
            if (resultPart == null || resultPart.Model == null || resultPart.Model.SimuScareResultDTO == null)
            {
                return rows;
            }

            Dictionary<long, OptionBodyViewRecord> optionMap = BuildOptionMap();
            OptionBodyViewRecord focusedOption = resultPart.Model.OptionBodyView == null
                ? null
                : resultPart.Model.OptionBodyView.FocusedRecord;

            foreach (IUIRecord uiRecord in resultPart.Model.SimuScareResultDTO.Records)
            {
                SimuScareResultDTORecord rec = uiRecord as SimuScareResultDTORecord;
                if (rec == null || rec.ScarceQty == null || rec.ScarceQty.Value <= 0m)
                {
                    continue;
                }

                OptionBodyViewRecord option = null;
                if (rec.MOID.HasValue)
                {
                    optionMap.TryGetValue(rec.MOID.Value, out option);
                }
                if (option == null && rec.PLSID.HasValue)
                {
                    optionMap.TryGetValue(rec.PLSID.Value, out option);
                }
                if (option == null)
                {
                    option = focusedOption;
                }

                rows.Add(new ShortageRow
                {
                    ProductionOrder = Normalize(rec.MODocNo),
                    ProductionItemCode = option == null ? Normalize(rec.ItemMaster_Code) : Normalize(option.ItemMaster_Code),
                    ProductionItemName = option == null ? Normalize(rec.ItemMaster_Name) : Normalize(option.ItemMaster_Name),
                    MaterialCode = Normalize(rec.ItemMaster_Code),
                    MaterialName = Normalize(rec.ItemMaster_Name),
                    DemandQty = rec.SimuReqQty ?? 0m,
                    PlanStartDate = option == null ? (DateTime?)null : option.PredStartTime,
                    ReqDate = rec.ReqDate,
                    ScarceQty = rec.ScarceQty ?? 0m
                });
            }

            return rows
                .OrderBy(r => r.PlanStartDate ?? DateTime.MaxValue)
                .ThenBy(r => r.ReqDate ?? DateTime.MaxValue)
                .ThenBy(r => r.ProductionOrder, StringComparer.Ordinal)
                .ToList();
        }

        private Dictionary<long, OptionBodyViewRecord> BuildOptionMap()
        {
            var map = new Dictionary<long, OptionBodyViewRecord>();
            if (resultPart == null || resultPart.Model == null || resultPart.Model.OptionBodyView == null)
            {
                return map;
            }

            foreach (IUIRecord uiRecord in resultPart.Model.OptionBodyView.Records)
            {
                OptionBodyViewRecord rec = uiRecord as OptionBodyViewRecord;
                if (rec != null && rec.ID > 0 && !map.ContainsKey(rec.ID))
                {
                    map.Add(rec.ID, rec);
                }
            }

            return map;
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        private void RenderOverlay(List<ShortageRow> rows)
        {
            StringBuilder table = new StringBuilder();
            table.Append("<div id='shortageMask' style='position:fixed;inset:0;background:rgba(0,0,0,.22);z-index:2147483646;'></div>");
            table.Append("<div id='shortagePanel' style='position:fixed;left:50%;top:50%;transform:translate(-50%,-50%);width:1360px;max-width:calc(100vw - 24px);max-height:calc(100vh - 24px);background:#fff;border:1px solid #cfcfcf;box-shadow:0 8px 24px rgba(0,0,0,.2);z-index:2147483647;font-family:Microsoft YaHei, Arial;overflow:hidden;'>");
            table.Append("<div style='display:flex;align-items:center;justify-content:space-between;padding:10px 14px;border-bottom:1px solid #e5e5e5;font-size:14px;font-weight:600;'>");
            table.Append("<span>齐套分析缺料明细</span>");
            table.Append("<button type='button' id='shortageClose' style='border:0;background:transparent;font-size:22px;line-height:22px;cursor:pointer;'>×</button>");
            table.Append("</div>");
            table.Append("<div style='padding:12px;overflow:auto;max-height:calc(100vh - 80px);'>");
            table.Append("<table style='border-collapse:collapse;width:100%;font-size:12px;white-space:nowrap;'>");
            table.Append("<thead><tr style='background:#f5f5f5;'>");
            foreach (string title in new[] { "生产订单", "生产料号", "生产名称", "备料料号", "备料品名", "实际需求量", "计划开工日期", "需求日期", "缺料量" })
            {
                table.Append("<th style='border:1px solid #d9d9d9;padding:6px 8px;text-align:left;'>").Append(Escape(title)).Append("</th>");
            }
            table.Append("</tr></thead><tbody>");

            if (rows.Count == 0)
            {
                table.Append("<tr><td colspan='9' style='border:1px solid #d9d9d9;padding:10px;text-align:center;color:#666;'>无缺料数据</td></tr>");
            }
            else
            {
                foreach (ShortageRow row in rows)
                {
                    table.Append("<tr>");
                    table.Append(Cell(row.ProductionOrder));
                    table.Append(Cell(row.ProductionItemCode));
                    table.Append(Cell(row.ProductionItemName));
                    table.Append(Cell(row.MaterialCode));
                    table.Append(Cell(row.MaterialName));
                    table.Append(Cell(row.DemandQty.ToString("0.####")));
                    table.Append(Cell(row.PlanStartDate.HasValue ? row.PlanStartDate.Value.ToString("yyyy-MM-dd") : string.Empty));
                    table.Append(Cell(row.ReqDate.HasValue ? row.ReqDate.Value.ToString("yyyy-MM-dd") : string.Empty));
                    table.Append(Cell(row.ScarceQty.ToString("0.####")));
                    table.Append("</tr>");
                }
            }

            table.Append("</tbody></table></div></div>");

            string script =
                "(function(){var old=document.getElementById('shortageMask');if(old&&old.parentNode)old.parentNode.removeChild(old);"
                + "old=document.getElementById('shortagePanel');if(old&&old.parentNode)old.parentNode.removeChild(old);"
                + "var wrap=document.createElement('div');wrap.innerHTML=" + ToJsString(table.ToString()) + ";document.body.appendChild(wrap);"
                + "var close=function(){var m=document.getElementById('shortageMask');if(m&&m.parentNode)m.parentNode.removeChild(m);var p=document.getElementById('shortagePanel');if(p&&p.parentNode)p.parentNode.removeChild(p);};"
                + "var btn=document.getElementById('shortageClose');if(btn)btn.onclick=close;"
                + "var mask=document.getElementById('shortageMask');if(mask)mask.onclick=close;"
                + "})();";

            Control control = resultPart as Control;
            Page page = control == null ? null : control.Page;
            if (page == null)
            {
                return;
            }

            string key = "ManufactureSimulateShortageOverlay_" + DateTime.Now.Ticks;
            string scriptBlock = "<script type=\"text/javascript\">" + script + "</script>";

            if (TryRegisterScriptManager(page, control, key, script))
            {
                return;
            }

            try
            {
#pragma warning disable 618
                AtlasHelper.RegisterStartupScript(control, control.GetType(), key, scriptBlock, false);
#pragma warning restore 618
                return;
            }
            catch
            {
            }

            page.ClientScript.RegisterStartupScript(control.GetType(), key, script, true);
        }

        private static string Cell(string value)
        {
            return "<td style='border:1px solid #d9d9d9;padding:6px 8px;'>" + Escape(value) + "</td>";
        }

        private static string Escape(string value)
        {
            return System.Web.HttpUtility.HtmlEncode(value ?? string.Empty);
        }

        private static string ToJsString(string value)
        {
            string escaped = (value ?? string.Empty)
                .Replace("\\", "\\\\")
                .Replace("'", "\\'")
                .Replace("\r", string.Empty)
                .Replace("\n", string.Empty);
            return "'" + escaped + "'";
        }

        private static bool TryRegisterScriptManager(Page page, Control control, string key, string script)
        {
            Type scriptManagerType = Type.GetType(
                "System.Web.UI.ScriptManager, System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35",
                false);
            if (scriptManagerType == null)
            {
                return false;
            }

            MethodInfo getCurrent = scriptManagerType.GetMethod(
                "GetCurrent",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new Type[] { typeof(Page) },
                null);
            if (getCurrent == null || getCurrent.Invoke(null, new object[] { page }) == null)
            {
                return false;
            }

            MethodInfo register = scriptManagerType.GetMethod(
                "RegisterStartupScript",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new Type[] { typeof(Control), typeof(Type), typeof(string), typeof(string), typeof(bool) },
                null);
            if (register == null)
            {
                return false;
            }

            register.Invoke(null, new object[] { control, control.GetType(), key, script, true });
            return true;
        }

        private static void Log(string message)
        {
            try
            {
                string dir = @"C:\temp";
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                File.AppendAllText(
                    Path.Combine(dir, "ManufactureSimulateShortageTable.log"),
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + message + "\r\n",
                    Encoding.UTF8);
            }
            catch
            {
            }
        }
    }

    internal sealed class ShortageRow
    {
        public string ProductionOrder { get; set; }
        public string ProductionItemCode { get; set; }
        public string ProductionItemName { get; set; }
        public string MaterialCode { get; set; }
        public string MaterialName { get; set; }
        public decimal DemandQty { get; set; }
        public DateTime? PlanStartDate { get; set; }
        public DateTime? ReqDate { get; set; }
        public decimal ScarceQty { get; set; }
    }

}
