using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Text;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using UFSoft.UBF.Sys.Database;
using System.Web.UI;
using System.Web.UI.WebControls;
using UFIDA.U9.MFG.MO.StartAnalysisUIModel;
using UFSoft.UBF.UI;
using UFSoft.UBF.UI.ControlModel;
using UFSoft.UBF.UI.Custom;
using UFSoft.UBF.UI.Engine.Builder;
using UFSoft.UBF.UI.IView;
using UFSoft.UBF.UI.WebControlAdapter;

namespace U9Custom.UI.ManufactureSimulateShortageTable
{
    internal static class SqlHelp
    {
        public static DataTable RunSqlDataTable(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(DatabaseManager.GetCurrentConnection().ConnectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = sql;
                command.CommandTimeout = 120;
                if (parameters != null && parameters.Length > 0) command.Parameters.AddRange(parameters);
                var table = new DataTable();
                using (var adapter = new SqlDataAdapter(command)) adapter.Fill(table);
                return table;
            }
        }
    }

    public sealed class SimuDocShortageButtonPlugin : ExtendedPartBase
    {
        private const string ButtonId = "BtnShortageTable";
        private const string ButtonGuid = "6f6e7a18-2f4d-4d1f-85a7-8f2c8a0db3b1";
        private const string OutsourceButtonId = "BtnOutsourceShortageStatistics";
        private const string OutsourceButtonGuid = "D98ED37C-2AA1-4786-8BF8-404955C9E8A7";
        private IPart part;
        private SimuDocUIFormWebPart simuDocPart;
        private bool added;
        private static IPart resultPart;

        public SimuDocShortageButtonPlugin() { WriteLog("plugin constructed"); }

        public override void AfterInit(IPart currentPart, EventArgs args) { base.AfterInit(currentPart, args); Initialize(currentPart); }
        public override void AfterLoad(IPart currentPart, EventArgs args) { base.AfterLoad(currentPart, args); Initialize(currentPart); }
        public override void AfterDataBinding(IPart currentPart) { base.AfterDataBinding(currentPart); Initialize(currentPart); }

        private void Initialize(IPart currentPart)
        {
            if (currentPart == null) return;
            if (added)
            {
                RefreshExistingButtonScript();
                RefreshExistingOutsourceButtonScript();
                return;
            }
            if (currentPart is SimuDocUIFormWebPart)
            {
                simuDocPart = (SimuDocUIFormWebPart)currentPart;
            }
            if (currentPart != null && string.Equals(currentPart.GetType().FullName, "UFIDA.U9.MFG.MO.StartAnalysisUIModel.ManufactureSimuResultUIFormWebPart", StringComparison.Ordinal))
            {
                resultPart = currentPart;
                WriteLog("result part captured: " + currentPart.GetType().FullName);
                return;
            }
            if (currentPart.GetType() != typeof(StartAnalysisMainUIFormWebPart) && currentPart.GetType() != typeof(SimuDocUIFormWebPart)) return;
            part = currentPart;
            IUFToolbar toolbar = part.GetUFControlByName(part.TopLevelContainer, "Toolbar2") as IUFToolbar;
            if (toolbar == null) toolbar = part.GetUFControlByName(part.TopLevelContainer, "Toolbar1") as IUFToolbar;
            if (toolbar == null) { WriteLog("Toolbar2/Toolbar1 not found"); return; }
            IUFButton existingButton = part.GetUFControlByName(part.TopLevelContainer, ButtonId) as IUFButton;
            if (existingButton != null)
            {
                AddOrRefreshOutsourceButton(toolbar, existingButton);
                ApplyOpenShortageScript(existingButton);
                added = true;
                WriteLog("button script refreshed after data binding");
                return;
            }
            if (added) return;
            IUFButton button = UIControlBuilder.BuilderToolbarButton(toolbar, "True", ButtonId, "True", "True", 70, 28, "100", string.Empty, true, false, ButtonGuid, ButtonGuid, ButtonGuid);
            button.ID = ButtonId;
            button.Text = "缺料表";
            button.AutoPostBack = false;
            button.UIModel = part.Model.ElementID;
            ApplyOpenShortageScript(button);
            UFWebToolbarAdapter webToolbar = toolbar as UFWebToolbarAdapter;
            if (webToolbar == null) { WriteLog("toolbar is not UFWebToolbarAdapter: " + toolbar.GetType().FullName); return; }
            webToolbar.Items.Add(button as System.Web.UI.WebControls.WebControl);
            AddOrRefreshOutsourceButton(toolbar, button);
            RegisterOutsourceOrderScript(button as Control);
            added = true;
            WriteLog("button added after Print in " + toolbar.ID);
        }

        private void RefreshExistingButtonScript()
        {
            if (part == null) return;
            IUFButton existingButton = part.GetUFControlByName(part.TopLevelContainer, ButtonId) as IUFButton;
            if (existingButton == null) return;
            ApplyOpenShortageScript(existingButton);
            WriteLog("button script refreshed after data binding");
        }

        private void RefreshExistingOutsourceButtonScript()
        {
            if (part == null) return;
            IUFToolbar toolbar = part.GetUFControlByName(part.TopLevelContainer, "Toolbar2") as IUFToolbar;
            if (toolbar == null) toolbar = part.GetUFControlByName(part.TopLevelContainer, "Toolbar1") as IUFToolbar;
            IUFButton shortage = part.GetUFControlByName(part.TopLevelContainer, ButtonId) as IUFButton;
            if (toolbar != null && shortage != null) AddOrRefreshOutsourceButton(toolbar, shortage);
            RegisterOutsourceOrderScript(shortage as Control);
        }

        private void AddOrRefreshOutsourceButton(IUFToolbar toolbar, IUFButton shortageButton)
        {
            if (toolbar == null || shortageButton == null) return;
            IUFButton existing = part.GetUFControlByName(part.TopLevelContainer, OutsourceButtonId) as IUFButton;
            if (existing == null)
            {
                existing = UIControlBuilder.BuilderToolbarButton(toolbar, "True", OutsourceButtonId, "True", "True", 100, 28, "100", string.Empty, true, false, OutsourceButtonGuid, OutsourceButtonGuid, OutsourceButtonGuid);
                existing.ID = OutsourceButtonId;
                existing.Text = "委外欠料统计";
                existing.AutoPostBack = false;
                existing.UIModel = part.Model.ElementID;
                UFWebToolbarAdapter adapter = toolbar as UFWebToolbarAdapter;
                if (adapter == null) return;
                System.Web.UI.WebControls.WebControl control = existing as System.Web.UI.WebControls.WebControl;
                if (control == null) return;
                int shortageIndex = 0;
                for (int itemIndex = 0; itemIndex < adapter.Items.Count; itemIndex++)
                {
                    System.Web.UI.WebControls.WebControl item = null;
                    PropertyInfo itemProperty = adapter.Items.GetType().GetProperty("Item");
                    if (itemProperty != null)
                    {
                        item = itemProperty.GetValue(adapter.Items, new object[] { itemIndex }) as System.Web.UI.WebControls.WebControl;
                    }
                    if (object.ReferenceEquals(item, shortageButton as System.Web.UI.WebControls.WebControl))
                    {
                        shortageIndex = itemIndex;
                        break;
                    }
                }
                adapter.Items.AddAt(shortageIndex, control);
            }
            ApplyOpenOutsourceScript(existing);
        }

        private string BuildOpenOutsourceScript()
        {
            return "(function(){var u='/U9C/erp/display.aspx?lnk=U9Custom.OutsourceShortageStatistics.Independent&sId=3025nid';try{if(window.parent&&window.parent.TabPanelManager){window.parent.TabPanelManager.addUrlTab(u,'委外欠料统计');return false;}}catch(e){}window.open(u,'_blank');return false;})();";
        }

        private void ApplyOpenOutsourceScript(IUFButton button)
        {
            WebControl webButton = button as WebControl;
            if (webButton != null) webButton.Attributes["onclick"] = BuildOpenOutsourceScript();
        }

        private void RegisterOutsourceOrderScript(Control control)
        {
            Page page = control == null ? null : control.Page;
            if (page == null && part is Control) page = ((Control)part).Page;
            if (page == null) return;
            string script = @"(function(){function move(){var o=document.querySelector('[id*=BtnOutsourceShortageStatistics]');var s=document.querySelector('[id*=BtnShortageTable]');if(!o||!s)return;var ow=o.closest('.nav-header-item')||o.parentElement;var sw=s.closest('.nav-header-item')||s.parentElement;if(ow&&sw&&ow.parentNode&&ow!==sw)sw.parentNode.insertBefore(ow,sw);}if(document.readyState==='loading')document.addEventListener('DOMContentLoaded',move);else move();setTimeout(move,100);setTimeout(move,500);})();";
            try
            {
                Type smType = Type.GetType("System.Web.UI.ScriptManager, System.Web.Extensions", false);
                MethodInfo getCurrent = smType == null ? null : smType.GetMethod("GetCurrent", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Page) }, null);
                object manager = getCurrent == null ? null : getCurrent.Invoke(null, new object[] { page });
                MethodInfo register = smType == null ? null : smType.GetMethod("RegisterStartupScript", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Control), typeof(Type), typeof(string), typeof(string), typeof(bool) }, null);
                if (manager != null && register != null && control != null)
                {
                    register.Invoke(null, new object[] { control, control.GetType(), "ManufactureSimulateShortageOutsourceOrder", script, true });
                    return;
                }
            }
            catch { }
            try { page.ClientScript.RegisterStartupScript(control == null ? GetType() : control.GetType(), "ManufactureSimulateShortageOutsourceOrder", script, true); } catch { }
        }

        private string BuildOpenShortageScript()
        {
            string code = ReadCurrentDocumentCode();
            long id = ReadCurrentDocumentID();
            string encodedCode = HttpUtility.JavaScriptStringEncode(HttpUtility.UrlEncode(code ?? string.Empty));
            string encodedId = HttpUtility.JavaScriptStringEncode(id > 0 ? id.ToString(CultureInfo.InvariantCulture) : string.Empty);
            return "(function(){var u='../../erp/display.aspx?lnk=U9Custom.ManufactureSimulateShortageTable.Independent&sId=3025nid&__ShortageCode=" + encodedCode + "&__ShortageID=" + encodedId + "';try{if(window.parent&&window.parent.TabPanelManager){window.parent.TabPanelManager.addUrlTab(u,'齐套分析缺料表');return false;}}catch(e){}window.open(u,'_blank');return false;})();";
        }

        private void ApplyOpenShortageScript(IUFButton button)
        {
            WebControl webButton = button as WebControl;
            if (webButton != null) webButton.Attributes["onclick"] = BuildOpenShortageScript();
        }

        private List<ShortageRow> BuildRows()
        {
            string code = ReadCurrentDocumentCode();
            long org = ReadCurrentLong("SimuDocOrg", "Org", "CurrentOrg");
            if (string.IsNullOrWhiteSpace(code)) throw new InvalidOperationException("未获取到当前齐套分析单据编码。");
            if (org <= 0) throw new InvalidOperationException("未获取到当前组织。");

            DataTable demandTable = SqlHelp.RunSqlDataTable(@"
SELECT dp.ID AS DemandID, dp.MOID, dp.DocNo AS ProductionOrder, dp.ItemMaster AS ItemID,
       dp.ItemCode AS MaterialCode, dp.ItemName AS MaterialName,
       dp.ReqNumIssueUOMQty AS DemandQty, dp.ScarceQty, dp.ReqDate,
       w.PlanStartDate
FROM dbo.MO_SimuDoc d
JOIN dbo.MO_ItemWIPSimu w ON w.SimuDoc = d.ID
JOIN dbo.MO_SimuDemandPick dp ON dp.ItemWIPSimu = w.ID
WHERE d.Code = @Code AND d.SimuDocOrg = @Org AND dp.ScarceQty > 0
ORDER BY ISNULL(w.PlanStartDate, '9999-12-31'), dp.ID", 
                new SqlParameter("@Code", SqlDbType.NVarChar, 100) { Value = code },
                new SqlParameter("@Org", SqlDbType.BigInt) { Value = org });

            DataTable poTable = SqlHelp.RunSqlDataTable(@"
SELECT l.ID, l.ItemInfo_ItemID AS ItemID, l.ItemInfo_ItemCode AS MaterialCode,
       l.Status, l.DeficiencyQtyTU, l.DeliveryDate, l.PlanArriveDate,
       poh.DocNo AS PurchaseOrderNo, pol.DocLineNo AS PurchaseOrderLineNo
FROM dbo.PM_POShipLine l
LEFT JOIN dbo.PM_POLine pol ON pol.ID = l.POLine
LEFT JOIN dbo.PM_PurchaseOrder poh ON poh.ID = pol.PurchaseOrder
WHERE l.CurrentOrg = @Org AND l.Status IN (0,1,2) AND l.DeficiencyQtyTU > 0
ORDER BY COALESCE(l.DeliveryDate, l.PlanArriveDate, '9999-12-31'), l.ID",
                new SqlParameter("@Org", SqlDbType.BigInt) { Value = org });

            DataTable receiptTable = SqlHelp.RunSqlDataTable(@"
SELECT r.ID, r.ItemInfo_ItemID AS ItemID, r.ItemInfo_ItemCode AS MaterialCode,
       r.Status, r.SplitFlag, r.RcvQtyTU, r.ArriveQtyTU, r.PlanQtyTU,
       rcv.DocNo AS ReceiptNo, r.DocLineNo AS ReceiptLineNo,
       rcv.BusinessDate AS ReceiptDate
FROM dbo.PM_RcvLine r
LEFT JOIN dbo.PM_Receivement rcv ON rcv.ID = r.Receivement
WHERE r.CurrentOrg = @Org AND r.Status <> 5 AND r.SplitFlag <> 1
ORDER BY ISNULL(r.SrcDoc_SrcDocDate, '9999-12-31'), r.ID",
                new SqlParameter("@Org", SqlDbType.BigInt) { Value = org });

            var poRemaining = poTable.AsEnumerable().ToDictionary(row => row.Field<long>("ID"), row => row.Field<decimal>("DeficiencyQtyTU"));
            var receiptRemaining = receiptTable.AsEnumerable().ToDictionary(row => row.Field<long>("ID"), ReceiptAvailableQty);
            var result = new List<ShortageRow>();
            foreach (DataRow demand in demandTable.Rows)
            {
                long demandId = demand.Field<long>("DemandID");
                long itemId = demand.Field<long>("ItemID");
                string codeValue = Convert.ToString(demand["MaterialCode"], CultureInfo.InvariantCulture);
                decimal remaining = demand.Field<decimal>("ScarceQty");
                ShortageRow baseRow = new ShortageRow {
                    DemandId = demandId,
                    ProductionOrder = Convert.ToString(demand["ProductionOrder"], CultureInfo.InvariantCulture),
                    MaterialCode = codeValue,
                    MaterialName = Convert.ToString(demand["MaterialName"], CultureInfo.InvariantCulture),
                    DemandQty = demand.Field<decimal>("DemandQty"),
                    ScarceQty = remaining,
                    PlanStartDate = NullableDate(demand["PlanStartDate"]),
                    ReqDate = NullableDate(demand["ReqDate"])
                };

                foreach (DataRow po in poTable.Rows)
                {
                    if (!SameItem(po.Field<long>("ItemID"), Convert.ToString(po["MaterialCode"], CultureInfo.InvariantCulture), itemId, codeValue)) continue;
                    decimal matched = Math.Min(remaining, poRemaining[po.Field<long>("ID")]);
                    if (matched <= 0m) continue;
                    remaining -= matched;
                    poRemaining[po.Field<long>("ID")] -= matched;
                    result.Add(baseRow.WithSupply("采购订单", Convert.ToString(po["PurchaseOrderNo"], CultureInfo.InvariantCulture) + "/" + Convert.ToString(po["PurchaseOrderLineNo"], CultureInfo.InvariantCulture), NullableDate(FirstValue(po, "DeliveryDate", "PlanArriveDate")), matched, remaining));
                    if (remaining <= 0m) break;
                }
                if (remaining > 0m)
                {
                    foreach (DataRow receipt in receiptTable.Rows)
                    {
                        if (!SameItem(receipt.Field<long>("ItemID"), Convert.ToString(receipt["MaterialCode"], CultureInfo.InvariantCulture), itemId, codeValue)) continue;
                        decimal matched = Math.Min(remaining, receiptRemaining[receipt.Field<long>("ID")]);
                        if (matched <= 0m) continue;
                        remaining -= matched;
                        receiptRemaining[receipt.Field<long>("ID")] -= matched;
                        result.Add(baseRow.WithSupply("采购收货", Convert.ToString(receipt["ReceiptNo"], CultureInfo.InvariantCulture) + "/" + Convert.ToString(receipt["ReceiptLineNo"], CultureInfo.InvariantCulture), NullableDate(receipt["ReceiptDate"]), matched, remaining));
                        if (remaining <= 0m) break;
                    }
                }
                if (!result.Any(x => x.DemandId == demandId)) result.Add(baseRow.WithSupply("未匹配", string.Empty, null, 0m, remaining));
            }
            WriteLog("database source: code=" + code + ", org=" + org + ", demand=" + demandTable.Rows.Count + ", po=" + poTable.Rows.Count + ", receipt=" + receiptTable.Rows.Count + ", rows=" + result.Count);
            return result;
        }

        private string ReadCurrentDocumentCode()
        {
            string code = ReadCurrentValue("Code", "SimuDocCode", "DocNo");
            if (!string.IsNullOrWhiteSpace(code)) return code.Trim();

            code = StringValue(part == null ? null : part.Action, "Code", "SimuDocCode", "DocNo");
            if (!string.IsNullOrWhiteSpace(code)) return code.Trim();
            code = StringValue(resultPart == null ? null : resultPart.Action, "Code", "SimuDocCode", "DocNo");
            if (!string.IsNullOrWhiteSpace(code)) return code.Trim();

            Page page = null;
            if (part is Control) page = ((Control)part).Page;
            if (page == null && simuDocPart is Control) page = ((Control)simuDocPart).Page;
            string idText = page == null || page.Request == null ? string.Empty : page.Request.QueryString["ID"];
            long id;
            if (!long.TryParse(idText, out id) || id <= 0) return string.Empty;

            DataTable table = SqlHelp.RunSqlDataTable(
                "SELECT TOP 1 Code FROM dbo.MO_SimuDoc WITH(NOLOCK) WHERE ID = @ID",
                new SqlParameter("@ID", SqlDbType.BigInt) { Value = id });
            if (table.Rows.Count == 0 || table.Rows[0]["Code"] == DBNull.Value) return string.Empty;
            code = Convert.ToString(table.Rows[0]["Code"], CultureInfo.InvariantCulture);
            WriteLog("current code resolved from page ID: " + id + " -> " + code);
            return code == null ? string.Empty : code.Trim();
        }

        private long ReadCurrentDocumentID()
        {
            SimuDocRecord record = GetFocusedSimuDocRecord();
            long id = LongValue(record, "ID", "SimuDocID");
            if (id > 0) return id;

            object model = part == null ? null : part.Model;
            object value = ReadFromModel(model, new[] { "SimuDocID", "ID" });
            if (value != null && long.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), out id) && id > 0) return id;

            id = LongValue(part == null ? null : part.Action, "SimuDocID", "ID", "QryModelID");
            if (id > 0) return id;
            id = LongValue(resultPart == null ? null : resultPart.Action, "SimuDocID", "ID", "QryModelID");
            if (id > 0)
            {
                WriteLog("current ID resolved from result action: " + id);
                return id;
            }

            string code = ReadCurrentValue("Code", "SimuDocCode", "DocNo");
            if (!string.IsNullOrWhiteSpace(code))
            {
                DataTable table = SqlHelp.RunSqlDataTable(
                    "SELECT TOP 1 ID FROM dbo.MO_SimuDoc WITH(NOLOCK) WHERE Code = @Code ORDER BY ID DESC",
                    new SqlParameter("@Code", SqlDbType.NVarChar, 100) { Value = code.Trim() });
                if (table.Rows.Count > 0 && table.Rows[0]["ID"] != DBNull.Value)
                {
                    id = Convert.ToInt64(table.Rows[0]["ID"], CultureInfo.InvariantCulture);
                    WriteLog("current ID resolved from code: " + code + " -> " + id);
                    return id;
                }
            }

            Page page = null;
            if (part is Control) page = ((Control)part).Page;
            if (page == null && simuDocPart is Control) page = ((Control)simuDocPart).Page;
            string idText = page == null || page.Request == null ? string.Empty : page.Request.QueryString["ID"];
            return long.TryParse(idText, out id) && id > 0 ? id : 0L;
        }

        private string ReadCurrentValue(params string[] names)
        {
            SimuDocRecord record = GetFocusedSimuDocRecord();
            string direct = StringValue(record, names);
            if (!string.IsNullOrEmpty(direct)) return direct;
            object model = part == null ? null : part.Model;
            object value = ReadFromModel(model, names);
            return value == null ? string.Empty : Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        private long ReadCurrentLong(params string[] names)
        {
            SimuDocRecord record = GetFocusedSimuDocRecord();
            long direct = LongValue(record, names);
            if (direct > 0) return direct;
            object model = part == null ? null : part.Model;
            object value = ReadFromModel(model, names);
            long result;
            return value != null && long.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), out result) ? result : 0L;
        }

        private SimuDocRecord GetFocusedSimuDocRecord()
        {
            SimuDocUIFormWebPart form = simuDocPart ?? (part as SimuDocUIFormWebPart);
            if (form == null || form.Model == null || form.Model.SimuDoc == null) return null;
            return form.Model.SimuDoc.FocusedRecord;
        }

        private static object ReadFromModel(object model, string[] names)
        {
            if (model == null) return null;
            object value = FindNamedValue(model, names, new HashSet<object>(), 0);
            if (value != null) WriteLog("current value found: " + string.Join(",", names) + "=" + Convert.ToString(value, CultureInfo.InvariantCulture));
            else WriteLog("current value not found: " + string.Join(",", names) + ", model=" + model.GetType().FullName);
            return value;
        }

        private static object FindNamedValue(object value, string[] names, ISet<object> visited, int depth)
        {
            if (value == null || depth > 4 || value is string || value.GetType().IsValueType || visited.Contains(value)) return null;
            visited.Add(value);
            foreach (PropertyInfo property in value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!property.CanRead || property.GetIndexParameters().Length != 0 || IsFrameworkProperty(property.Name)) continue;
                object child;
                try { child = property.GetValue(value, null); } catch { continue; }
                if (names.Any(name => string.Equals(name, property.Name, StringComparison.OrdinalIgnoreCase)) && child != null) return child;
            }
            foreach (PropertyInfo property in value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!property.CanRead || property.GetIndexParameters().Length != 0 || IsFrameworkProperty(property.Name)) continue;
                object child;
                try { child = property.GetValue(value, null); } catch { continue; }
                if (child != null && !child.GetType().IsValueType && !(child is string))
                {
                    object found = FindNamedValue(child, names, visited, depth + 1);
                    if (found != null) return found;
                }
            }
            return null;
        }

        private static bool IsFrameworkProperty(string name)
        {
            return name == "Page" || name == "Parent" || name == "Controls" || name == "TopLevelContainer" || name == "UIModel" || name == "Action" || name == "adjust" || name == "Vendor" || name == "ExtendService";
        }

        private static bool SameItem(long valueId, string valueCode, long itemId, string code)
        {
            return (itemId > 0 && valueId == itemId) || (!string.IsNullOrEmpty(code) && string.Equals(valueCode, code, StringComparison.OrdinalIgnoreCase));
        }

        private static DateTime? NullableDate(object value)
        {
            return value == null || value == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(value, CultureInfo.InvariantCulture);
        }

        private static object FirstValue(DataRow row, params string[] names)
        {
            foreach (string name in names)
            {
                object value = row[name];
                if (value != null && value != DBNull.Value) return value;
            }
            return null;
        }

        private static decimal ReceiptAvailableQty(DataRow row)
        {
            foreach (string name in new[] { "RcvQtyTU", "ArriveQtyTU", "PlanQtyTU" })
            {
                decimal value = row.Field<decimal>(name);
                if (value > 0m) return value;
            }
            return 0m;
        }
        private static object Value(object value, params string[] names)
        {
            if (value == null) return null;
            foreach (string name in names) { PropertyInfo p = value.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase); if (p != null) try { return p.GetValue(value, null); } catch { } }
            return null;
        }
        private static string StringValue(object value, params string[] names) { object x = Value(value, names); return x == null ? string.Empty : Convert.ToString(x, CultureInfo.InvariantCulture); }
        private static long LongValue(object value, params string[] names) { long x; object v = Value(value, names); return v != null && long.TryParse(Convert.ToString(v, CultureInfo.InvariantCulture), out x) ? x : 0L; }
        private static int IntValue(object value, params string[] names) { return (int)LongValue(value, names); }
        private static bool BoolValue(object value, params string[] names)
        {
            object raw = Value(value, names);
            if (raw == null) return false;
            if (raw is bool) return (bool)raw;
            string text = Convert.ToString(raw, CultureInfo.InvariantCulture);
            return string.Equals(text, "1", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "true", StringComparison.OrdinalIgnoreCase);
        }
        private static decimal DecimalValue(object value, params string[] names) { decimal x; object v = Value(value, names); return v != null && decimal.TryParse(Convert.ToString(v, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out x) ? x : 0m; }
        private static DateTime? DateValue(object value, params string[] names) { DateTime x; object v = Value(value, names); return v != null && DateTime.TryParse(Convert.ToString(v, CultureInfo.InvariantCulture), CultureInfo.InvariantCulture, DateTimeStyles.None, out x) ? x : (DateTime?)null; }

        private void RenderOverlay(List<ShortageRow> rows, object sender)
        {
            var html = new StringBuilder();
            html.Append("<div id='shortageMask' style='position:fixed;inset:0;background:rgba(0,0,0,.22);z-index:2147483646;'></div><div id='shortagePanel' style='position:fixed;left:50%;top:50%;transform:translate(-50%,-50%);width:1500px;max-width:calc(100vw - 24px);max-height:calc(100vh - 24px);background:#fff;border:1px solid #cfcfcf;box-shadow:0 8px 24px rgba(0,0,0,.2);z-index:2147483647;font-family:Microsoft YaHei,Arial;overflow:hidden;'>");
            html.Append("<div style='display:flex;justify-content:space-between;padding:10px 14px;border-bottom:1px solid #e5e5e5;font-weight:600;'>齐套分析缺料明细<button type='button' id='shortageClose' style='border:0;background:transparent;font-size:22px;cursor:pointer;'>×</button></div><div style='padding:12px;overflow:auto;max-height:calc(100vh - 80px);'><table style='border-collapse:collapse;width:100%;font-size:12px;white-space:nowrap;'><thead><tr style='background:#f5f5f5;'>");
            foreach (string title in new[] { "生产订单", "备料料号", "备料品名", "需求量", "计划开工日期", "需求日期", "缺料量", "匹配类型", "供应单号", "供应日期", "匹配数量", "剩余缺料" }) html.Append("<th style='border:1px solid #d9d9d9;padding:6px 8px;text-align:left;'>").Append(Escape(title)).Append("</th>");
            html.Append("</tr></thead><tbody>");
            foreach (ShortageRow row in rows)
            {
                html.Append("<tr>");
                foreach (string text in new[] { row.ProductionOrder, row.MaterialCode, row.MaterialName, Number(row.DemandQty), Date(row.PlanStartDate), Date(row.ReqDate), Number(row.ScarceQty), row.SupplyType, row.SupplyNo, Date(row.SupplyDate), Number(row.MatchedQty), Number(row.RemainingShortage) }) html.Append(Cell(text));
                html.Append("</tr>");
            }
            if (rows.Count == 0) html.Append("<tr><td colspan='12' style='padding:10px;text-align:center;color:#666;'>无缺料数据</td></tr>");
            html.Append("</tbody></table></div></div>");
            string script = "(function(){var o=document.getElementById('shortageMask');if(o)o.remove();o=document.getElementById('shortagePanel');if(o)o.remove();var w=document.createElement('div');w.innerHTML=" + Js(html.ToString()) + ";document.body.appendChild(w);var c=function(){var a=document.getElementById('shortageMask');if(a)a.remove();var b=document.getElementById('shortagePanel');if(b)b.remove();};document.getElementById('shortageClose').onclick=c;document.getElementById('shortageMask').onclick=c;})();";
            Control control = sender as Control;
            if (control == null) control = part as Control;
            Page page = control == null ? null : control.Page;
            if (page == null && part is Control) page = ((Control)part).Page;
            WriteLog("overlay context: sender=" + (sender == null ? "null" : sender.GetType().FullName)
                + ", control=" + (control == null ? "null" : control.GetType().FullName)
                + ", page=" + (page == null ? "null" : page.GetType().FullName));
            if (page == null)
            {
                WriteLog("overlay registration skipped: page not found");
                return;
            }
            string key = "ManufactureSimulateShortageOverlay_" + DateTime.Now.Ticks;
            try
            {
                Type scriptManagerType = Type.GetType("System.Web.UI.ScriptManager, System.Web.Extensions", false);
                MethodInfo getCurrent = scriptManagerType == null ? null : scriptManagerType.GetMethod("GetCurrent", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Page) }, null);
                object manager = getCurrent == null ? null : getCurrent.Invoke(null, new object[] { page });
                MethodInfo register = scriptManagerType == null ? null : scriptManagerType.GetMethod("RegisterStartupScript", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Control), typeof(Type), typeof(string), typeof(string), typeof(bool) }, null);
                if (manager != null && register != null && control != null)
                {
                    register.Invoke(null, new object[] { control, control.GetType(), key, script, true });
                    WriteLog("overlay registered by ScriptManager");
                    return;
                }
            }
            catch (Exception ex)
            {
                WriteLog("ScriptManager registration failed: " + ex.Message);
            }
            if (control != null)
            {
                try
                {
#pragma warning disable 618
                    AtlasHelper.RegisterStartupScript(control, control.GetType(), key, "<script type=\"text/javascript\">" + script + "</script>", false);
#pragma warning restore 618
                    WriteLog("overlay registered by AtlasHelper");
                    return;
                }
                catch (Exception ex)
                {
                    WriteLog("AtlasHelper registration failed: " + ex.Message);
                }
            }
            page.ClientScript.RegisterStartupScript(control == null ? GetType() : control.GetType(), key, script, true);
            WriteLog("overlay registered by ClientScript");
        }
        private static string Number(decimal x) { return x.ToString("0.####", CultureInfo.InvariantCulture); }
        private static string Date(DateTime? x) { return x.HasValue ? x.Value.ToString("yyyy-MM-dd") : string.Empty; }
        private static string Cell(string x) { return "<td style='border:1px solid #d9d9d9;padding:6px 8px;'>" + Escape(x) + "</td>"; }
        private static string Escape(string x) { return HttpUtility.HtmlEncode(x ?? string.Empty); }
        private static string Js(string x) { return "'" + (x ?? string.Empty).Replace("\\", "\\\\").Replace("'", "\\'").Replace("\r", string.Empty).Replace("\n", string.Empty) + "'"; }
        private static void WriteLog(string message) { try { File.AppendAllText(@"C:\temp\ManufactureSimulateShortageTable.log", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + message + Environment.NewLine); } catch { } }
    }

    internal sealed class ShortageRow
    {
        public long DemandId; public string ProductionOrder; public string MaterialCode; public string MaterialName; public decimal DemandQty; public DateTime? PlanStartDate; public DateTime? ReqDate; public decimal ScarceQty; public string SupplyType; public string SupplyNo; public DateTime? SupplyDate; public decimal MatchedQty; public decimal RemainingShortage;
        public ShortageRow WithSupply(string type, string no, DateTime? date, decimal qty, decimal remaining) { return new ShortageRow { DemandId = DemandId, ProductionOrder = ProductionOrder, MaterialCode = MaterialCode, MaterialName = MaterialName, DemandQty = DemandQty, PlanStartDate = PlanStartDate, ReqDate = ReqDate, ScarceQty = ScarceQty, SupplyType = type, SupplyNo = no, SupplyDate = date, MatchedQty = qty, RemainingShortage = remaining }; }
    }
}


