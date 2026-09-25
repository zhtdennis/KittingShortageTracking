using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using U9Custom.UI.OutsourceShortageStatistics.Independent.Common;
using U9Custom.OutsourceShortageWritebackBP;
using U9Custom.OutsourceShortageWritebackBP.Proxy;
using UFIDA.U9.UI.Commands;
using UFIDA.U9.UI.PDHelper;
using UFSoft.UBF.UI.ControlModel;
using UFSoft.UBF.UI.Controls;
using UFSoft.UBF.UI.Engine;
using UFSoft.UBF.UI.Engine.Builder;
using UFSoft.UBF.UI.FormProcess;
using UFSoft.UBF.UI.IView;
using UFSoft.UBF.UI.MD.Runtime;
using UFSoft.UBF.UI.Portal.WebControls.WebParts;
using UFSoft.UBF.UI.UIFormPersonalization;
using UFSoft.UBF.UI.WebControlAdapter;
using UFSoft.UBF.UI.WebControls;

namespace U9Custom.UI.OutsourceShortageStatistics.Independent
{
    [FormRegister("U9Custom_OutsourceShortage", "U9Custom.UI.OutsourceShortageStatistics.Independent.OutsourceShortageUIFormWebPart", "U9Custom.UI.OutsourceShortageStatistics.Independent", "72FA010B-210D-46A8-83C5-5C94F67586B0", "WebPart", "True", 1600, 690)]
    public class OutsourceShortageUIFormWebPart : BaseWebForm
    {
        private OutsourceShortageModel uiModel;
        private FormAdjust adjust;
        private UpdatePanel updatePanel;
        private HiddenField wpFindID;
        private IUFCard filterCard;
        private IUFCard buttonRow;
        private IUFToolbar toolbar;
        private IUFCard gridCard;
        private IUFDataGrid dataGrid;
        private IUFButton btnQuery;
        private IUFButton btnClear;
        private IUFButton btnOutput;
        private IUFButton btnWriteMO;
        private IUFButton btnColumnSettings;
        private HiddenField hiddenVisibleColumns;
        private LinkButton applyColumnsButton;
        private IUFFldTextBox txtMaterialCode;
        private IUFFldTextBox txtSupplier;
        private IUFFldTextBox txtPurchaseOrder;
        private IUFFldTextBox txtWarehouse;
        private IUFFldDatePicker dtFromDate;
        private IUFFldDatePicker dtToDate;
        private const int GridViewportWidth = 1580;

        private enum ListColumnKind
        {
            Text,
            Number,
            Date
        }

        private sealed class ListColumn
        {
            public ListColumn(string fieldName, string title, ListColumnKind kind, int width, string controlId)
            {
                this.FieldName = fieldName;
                this.Title = title;
                this.Kind = kind;
                this.Width = width;
                this.ControlId = controlId;
            }

            public string FieldName { get; private set; }
            public string Title { get; private set; }
            public ListColumnKind Kind { get; private set; }
            public int Width { get; private set; }
            public string ControlId { get; private set; }
            public string GridControlId { get { return this.FieldName + "0"; } }
        }

        // IT维护提示：普通显示列集中加在这里；字段名需要和SQL别名、View字段名保持一致。
        private static readonly ListColumn[] ListColumns = new ListColumn[]
        {
            new ListColumn("SupplierCode", "供应商编码", ListColumnKind.Text, 100, "C817CF3E-0911-457D-9D9D-045DC63FC54F"),
            new ListColumn("SupplierName", "供应商名称", ListColumnKind.Text, 140, "6008A1C9-F67F-49FC-8B2D-7256E78807BF"),
            new ListColumn("PurchaseOrderNo", "采购订单号", ListColumnKind.Text, 140, "BC720D0B-88EC-4A1F-8106-8863A374536C"),
            new ListColumn("POLineNo", "采购行号", ListColumnKind.Number, 80, "1E8EAA5A-E34C-4C10-91D6-0232EF125204"),
            new ListColumn("PickLineNo", "备料行号", ListColumnKind.Number, 80, "878EF37D-8F5D-4ACE-8825-64792CA02A57"),
            new ListColumn("BusinessDate", "订单日期", ListColumnKind.Date, 100, "32478487-D165-456E-8F47-32C2A26A3DBE"),
            new ListColumn("MaterialCode", "备料料号", ListColumnKind.Text, 110, "6008A1C9-F67F-49FC-8B2D-7256E78807BF"),
            new ListColumn("MaterialName", "备料品名", ListColumnKind.Text, 150, "BC720D0B-88EC-4A1F-8106-8863A374536C"),
            new ListColumn("SupplyWhCode", "供应仓库", ListColumnKind.Text, 100, "14E0C86E-2099-4A6F-923E-1FE63C2BFF17"),
            new ListColumn("SupplyWhName", "仓库名称", ListColumnKind.Text, 120, "BAA406C4-3488-4F3B-95E3-589241B27CF4"),
            new ListColumn("ActualReqQty", "实际需求量", ListColumnKind.Number, 90, "CC75E000-8DA2-41DF-887E-AF8B1C540582"),
            new ListColumn("IssuedQty", "已领数量", ListColumnKind.Number, 90, "B4BAFE48-649A-492F-8724-00D882A61FFA"),
            new ListColumn("UnissuedQty", "未领数量", ListColumnKind.Number, 90, "E886BD24-F801-4D00-8A97-B83D4788338E"),
            new ListColumn("InventoryDeductQty", "库存扣减量", ListColumnKind.Number, 100, "275471AC-203E-482F-8C96-4F0445328CC4"),
            new ListColumn("ShortageQty", "欠料数量", ListColumnKind.Number, 90, "E01A06A2-997D-4DEE-8B0F-EB465661F0A0")
        };

        private static readonly int GridContentWidth = CalculateGridContentWidth();

        private static int CalculateGridContentWidth()
        {
            int width = 0;
            for (int i = 0; i < ListColumns.Length; i++) width += ListColumns[i].Width;
            return width;
        }

        public OutsourceShortageUIFormWebPart()
        {
            this.FormID = "3E208EBC-5F10-4376-A875-03BFF1BFD643";
            this.IsAutoSize = true;
        }

        public new OutsourceShortageModelAction Action
        {
            get { return (OutsourceShortageModelAction)base.Action; }
            set { base.Action = value; }
        }

        public new OutsourceShortageModel Model
        {
            get
            {
                if (this.uiModel == null)
                {
                    this.uiModel = new OutsourceShortageModel();
                }
                return this.uiModel;
            }
            set { this.uiModel = value; }
        }

        protected override IUIModel UIModel
        {
            get { return this.Model; }
            set { this.Model = value as OutsourceShortageModel; }
        }

        protected override void OnInit(EventArgs e) { this.OnInit2(e); }

        protected override void OnInitDo(EventArgs e)
        {
            this.Page.InitComplete += this.Page_InitComplete;
            WebPartBuilder.InitWebPart(this);
            this.Action = new OutsourceShortageModelAction(this);
            this.adjust = new FormAdjust();
            this.CreateFormChildControls();
        }

        private void Page_InitComplete(object sender, EventArgs e)
        {
            if (this.adjust != null) this.adjust.ProcessInit(this);
        }

        protected override void OnLoad(EventArgs e) { this.OnLoad2(e); }

        protected override void OnLoadDataDo(EventArgs e)
        {
            if (this.adjust != null) this.adjust.ProcessAdjustBeforeOnLoad(this);
            if (UIEngineHelper.IsDataBind(this.PageStatus, this))
            {
                if (this.Model == null) this.Model = new OutsourceShortageModel();
                if (!this.Page.IsPostBack) this.LoadData();
                this.OnLoadConsumer(new InParameterModel[0], new InParameterModel[0]);
                this.IsDataBinding = true;
            }
            if (this.adjust != null)
            {
                this.adjust.ProcessAdjustAfterOnLoadData(this);
                this.adjust.ProcessAdjustAfterOnLoad(this);
            }
        }

        protected override void OnPreRender(EventArgs e) { this.OnPreRender2(e); }

        protected override void OnPreRenderDo(EventArgs e)
        {
            if (this.adjust != null) this.adjust.ProcessAdjustBeforeOnPreRender(this);
            base.OnPreRender(e);
            this.CurrentState[this.TaskId.ToString()] = this.Model;
            this.RegisterClearWebPartPadding();
            this.RegisterU9CompatibilityScript();
            this.RegisterGridScrollScript();
            this.RegisterColumnSettingsScript();
            this.RegisterOutputCompletionScript();
            FormAuthorityHelper.SetWebPartAuthorization(this);
            if (this.IsDataBinding)
            {
                this.ApplyVisibleGridColumns();
                this.BeforeUIModelBinding();
                if (!this.Page.IsPostBack) EnumTypeBinding.BindEnumControls(this);
                if (this.adjust != null) this.adjust.ProcessAdjustBeforeDataBinding(this);
                this.DataBinding();
                if (this.adjust != null) this.adjust.ProcessAdjustAfterDataBinding(this);
                this.AfterUIModelBinding();
            }
            if (this.adjust != null) this.adjust.ProcessAdjustAfterOnPreRender(this);
            this.ApplyVisibleGridColumns();
        }

        private void CreateFormChildControls()
        {
            IUFContainer container = UIControlBuilder.BuildTopLevelContainer(this, "OutsourceShortageUIForm", true, 1600, 690);
            CommonBuilder.ContainerGridLayoutPropBuilder(container, 1, 3, 0, 10, 0, 0, 0, 5);
            this.InitViewBindingContainer(this, container, null, string.Empty, string.Empty, null, 1, string.Empty);
            UIControlBuilder.BuildContainerGridLayout(container, 10u, new GridColumnDef[] { new GridColumnDef(new Unit(1580), true) }, new GridRowDef[] { new GridRowDef(new Unit(28), true), new GridRowDef(new Unit(95), true), new GridRowDef(new Unit(557), true) });
            UIControlBuilder.BuildCommonControls(this, ref this.updatePanel, ref this.wpFindID);
            this.hiddenVisibleColumns = new HiddenField();
            this.hiddenVisibleColumns.ID = "HiddenVisibleColumns";
            this.Controls.Add(this.hiddenVisibleColumns);
            this.applyColumnsButton = new LinkButton();
            this.applyColumnsButton.ID = "ApplyVisibleColumns";
            this.applyColumnsButton.Style["display"] = "none";
            this.applyColumnsButton.CausesValidation = false;
            this.applyColumnsButton.Click += this.ApplyColumnsButton_Click;
            this.Controls.Add(this.applyColumnsButton);
            this.BuildToolbar(container);
            this.BuildFilterCard(container);
            UIControlBuilder.BuilderUFControl(this.filterCard, "0");
            this.BuildGridCard(container);
            UIControlBuilder.BuilderUFControl(this.gridCard, "1");
            this.EventBind();
            this.AfterCreateChildControls();
        }

        private void BuildToolbar(IUFContainer container)
        {
            this.toolbar = UIControlBuilder.BuilderToolBarControl(container, "Toolbar1", true, true, "1", 1580, 28, 0, 0, 1, 1, "100");
            UIControlBuilder.BuilderUFControl(this.toolbar, "1");

            this.btnWriteMO = UIControlBuilder.BuilderToolbarButton(this.toolbar, "True", "BtnWriteMO", "True", "True", 120, 28, "0", string.Empty, true, false, "B45F546D-F8D2-482B-A2B4-E25ABAB90720", "B45F546D-F8D2-482B-A2B4-E25ABAB90720", "FB257490-72E7-4B34-9C07-CE365B69F98D");
            this.btnWriteMO.Text = "写入工单备料";
            this.btnWriteMO.UIModel = this.Model.ElementID;
            this.btnWriteMO.Action = "OnWriteMO";
            UFWebToolbarAdapter toolbarAdapter = this.toolbar as UFWebToolbarAdapter;
            if (toolbarAdapter != null)
            {
                AddToolbarItem(toolbarAdapter, this.btnWriteMO);
                AddToolbarSeparator(toolbarAdapter);
            }
            this.btnOutput = UIControlBuilder.BuilderToolbarButton(this.toolbar, "True", "BtnOutput", "True", "True", 70, 28, "2", string.Empty, true, false, "688FD549-5A92-49CC-8072-6D5377F6409A", "688FD549-5A92-49CC-8072-6D5377F6409A", "AB7E0E96-6E12-4C41-BEA7-4884359D3692");
            this.btnOutput.Text = "输出";
            this.btnOutput.UIModel = this.Model.ElementID;
            this.btnOutput.Action = "OnOutput";
            if (toolbarAdapter != null)
            {
                AddToolbarItem(toolbarAdapter, this.btnOutput);
            }
        }

        private static void AddToolbarItem(UFWebToolbarAdapter toolbarAdapter, IUFButton button)
        {
            WebControl webControl = button as WebControl;
            if (toolbarAdapter == null || webControl == null) return;
            try
            {
                toolbarAdapter.Items.Add(webControl);
            }
            catch (Exception)
            {
                // The builder may have registered the control already.
            }
        }

        private static void AddToolbarSeparator(UFWebToolbarAdapter toolbarAdapter)
        {
            if (toolbarAdapter == null) return;
            try
            {
                toolbarAdapter.Items.Add(new UFWebToolbarSeparatorAdapter());
            }
            catch (Exception)
            {
                // Keep page initialization alive if the toolbar rejects a duplicate item.
            }
        }

        private void BuildFilterCard(IUFContainer container)
        {
            this.filterCard = UIControlBuilder.BuildCard(container, "CardFilter", false, "none", true, true, "1", string.Empty, "310603C6-1B11-461B-89FB-1FDE86B1CF4E");
            CommonBuilder.GridLayoutPropBuilder(container, this.filterCard, 1580, 95, 0, 1, 1, 1, "100");
            CommonBuilder.ContainerGridLayoutPropBuilder(this.filterCard, 16, 2, 0, 2, 8, 0, 6, 0);
            this.InitViewBindingContainer(this, this.filterCard, null, string.Empty, string.Empty, null, 1, string.Empty);
            UIControlBuilder.BuildContainerGridLayout(this.filterCard, 5u,
                new GridColumnDef[] { new GridColumnDef(new Unit(210), true), new GridColumnDef(new Unit(210), true), new GridColumnDef(new Unit(210), true), new GridColumnDef(new Unit(210), true), new GridColumnDef(new Unit(210), true), new GridColumnDef(new Unit(210), true) },
                new GridRowDef[] { new GridRowDef(new Unit(20), true), new GridRowDef(new Unit(28), true), new GridRowDef(new Unit(28), true) });
            this.AddLabel(this.filterCard, "lblSupplier", "供应商", 0, 0, "1E6523ED-538C-42F2-BF8B-A29D1AD96748", "C89D89A7-82A3-48C4-9B32-88D605561702");
            this.txtSupplier = this.AddTextBox(this.filterCard, "txtSupplier", "lblSupplier", 0, 1, 150, "1E6523ED-538C-42F2-BF8B-A29D1AD96748", "7CB494D3-371B-41F4-811E-8D7FD26F2E31");
            this.AddLabel(this.filterCard, "lblPO", "采购订单", 1, 0, "1C27A1D8-F6CB-4F4E-B5D7-C0B516DDE789", "DEBD16E3-657D-4203-8D33-6C94C839A3C1");
            this.txtPurchaseOrder = this.AddTextBox(this.filterCard, "txtPurchaseOrder", "lblPO", 1, 1, 160, "1C27A1D8-F6CB-4F4E-B5D7-C0B516DDE789", "3C1A8E94-ADA4-49DA-9E3E-78AA2BEB5DA2");
            this.AddLabel(this.filterCard, "lblItem", "备料料号", 2, 0, "255840EE-77CA-45CF-B2A6-4467C731C9FE", "0F752C2B-CFD5-4623-8072-3A1621F55C6C");
            this.txtMaterialCode = this.AddTextBox(this.filterCard, "txtMaterialCode", "lblItem", 2, 1, 150, "255840EE-77CA-45CF-B2A6-4467C731C9FE", "86B82AFD-D4CD-4108-B7B3-0AA28CB6037B");
            this.AddLabel(this.filterCard, "lblWh", "供应仓库", 3, 0, "4848722D-DB7F-4465-9A7D-4761985E3BEC", "62A21A9E-8D7B-47BF-AAEF-A9719054E379");
            this.txtWarehouse = this.AddTextBox(this.filterCard, "txtWarehouse", "lblWh", 3, 1, 150, "4848722D-DB7F-4465-9A7D-4761985E3BEC", "036D6C9D-B4DB-4486-9D92-27A613EA4608");
            this.AddLabel(this.filterCard, "lblFromDate", "订单日期从", 4, 0, "B5B52689-A04F-438A-A5BA-418C65719B36", "4854F6C6-5404-4C5D-B3D5-C42454BE12EB");
            this.dtFromDate = this.AddDatePicker(this.filterCard, "dtFromDate", "lblFromDate", 4, 1, 130, "B5B52689-A04F-438A-A5BA-418C65719B36", "7ABF6173-4D75-411C-8F77-D7BC474373E0");
            this.AddLabel(this.filterCard, "lblToDate", "订单日期到", 5, 0, "9CF69B64-FE87-4984-AD0A-0F97A3C6B4A7", "F65C5857-0603-4458-8E10-F7AE2BB813E0");
            this.dtToDate = this.AddDatePicker(this.filterCard, "dtToDate", "lblToDate", 5, 1, 130, "9CF69B64-FE87-4984-AD0A-0F97A3C6B4A7", "523F95B2-8473-4823-A970-153002C99A87");
            this.buttonRow = UIControlBuilder.BuildCard(this.filterCard, "ButtonRow", false, "none", true, true, "1", string.Empty, "9CBF8906-4BBF-4C63-BD49-C48B579D9E8C");
            CommonBuilder.GridLayoutPropBuilder(this.filterCard, this.buttonRow, 190, 28, 0, 2, 1, 1, "100");
            CommonBuilder.ContainerGridLayoutPropBuilder(this.buttonRow, 5, 1, 0, 0, 0, 0, 0, 0);
            UIControlBuilder.BuildContainerGridLayout(this.buttonRow, 0u,
                new GridColumnDef[] { new GridColumnDef(new Unit(60), true), new GridColumnDef(new Unit(5), true), new GridColumnDef(new Unit(60), true), new GridColumnDef(new Unit(5), true), new GridColumnDef(new Unit(60), true) },
                new GridRowDef[] { new GridRowDef(new Unit(28), true) });
            this.InitViewBindingContainer(this, this.buttonRow, null, string.Empty, string.Empty, null, 1, string.Empty);
            this.btnQuery = UIControlBuilder.BuilderUFButton(this.buttonRow, true, "BtnQuery", true, true, 60, 22, 0, 0, 1, 1, "100", string.Empty, this.Model.ElementID, "OnQuery", false, "038DE8AD-C349-4E0E-A338-8E7629E6BAC7", "038DE8AD-C349-4E0E-A338-8E7629E6BAC7", "D62AACCE-8A36-46B1-873F-29E328CC1B06");
            this.btnQuery.Text = "查询";
            SetCompactButtonWidth(this.btnQuery, 60);
            UIControlBuilder.BuilderUFControl(this.btnQuery, "8");
            this.btnClear = UIControlBuilder.BuilderUFButton(this.buttonRow, true, "BtnClear", true, true, 60, 22, 2, 0, 1, 1, "100", string.Empty, this.Model.ElementID, "OnClear", false, "4F8D8D0E-84C6-4A79-9886-89A645AAE571", "4F8D8D0E-84C6-4A79-9886-89A645AAE571", "FDA196DC-5AF2-4140-8343-BF6951ED2363");
            this.btnClear.Text = "清空";
            SetCompactButtonWidth(this.btnClear, 60);
            UIControlBuilder.BuilderUFControl(this.btnClear, "10");
            this.btnColumnSettings = UIControlBuilder.BuilderUFButton(this.buttonRow, true, "BtnColumnSettings", true, true, 60, 22, 4, 0, 1, 1, "100", string.Empty, this.Model.ElementID, string.Empty, false, "9B71C576-8245-4E9D-A536-4030527BE4D5", "9B71C576-8245-4E9D-A536-4030527BE4D5", "C04A111C-0212-463D-9D35-83B63255F617");
            this.btnColumnSettings.Text = "列设置";
            WebControl columnSettingsWeb = this.btnColumnSettings as WebControl;
            if (columnSettingsWeb != null)
            {
                columnSettingsWeb.Style["width"] = "60px";
                columnSettingsWeb.Style["min-width"] = "60px";
                columnSettingsWeb.Style["max-width"] = "60px";
                columnSettingsWeb.Attributes["value"] = "列设置";
                columnSettingsWeb.Attributes["title"] = "列设置";
            }
            UIControlBuilder.BuilderUFControl(this.btnColumnSettings, "14");
            this.SetColumnSettingsButtonClientClick();
            this.filterCard.Controls.Add(this.buttonRow);
            container.Controls.Add(this.filterCard);
        }

        private static void SetCompactButtonWidth(IUFButton button, int width)
        {
            WebControl webControl = button as WebControl;
            if (webControl == null) return;
            string value = width.ToString(CultureInfo.InvariantCulture) + "px";
            webControl.Style["width"] = value;
            webControl.Style["min-width"] = value;
            webControl.Style["max-width"] = value;
        }

        private void SetColumnSettingsButtonClientClick()
        {
            const string script = "if(window.OutsourceShortageShowColumnSettings){window.OutsourceShortageShowColumnSettings();}return false;";
            WebControl webControl = this.btnColumnSettings as WebControl;
            if (webControl != null) webControl.Attributes["onclick"] = script;
            SetPropertyIfExists(this.btnColumnSettings, "OnClientClick", script);
            SetPropertyIfExists(this.btnColumnSettings, "ClientClick", script);
        }
        private IUFLabel AddLabel(IUFContainer container, string id, string text, int column, int row, string resourceId, string controlId)
        {
            IUFLabel label = UIControlBuilder.BuilderUFLabel(container, id, string.Empty, "True", "True", "Left", 90, 20, column, row, 1, 1, "100", resourceId, controlId);
            label.Text = text;
            UIControlBuilder.BuilderUFControl(label, BuildTabIndex(column, row));
            return label;
        }

        private IUFFldTextBox AddTextBox(IUFContainer container, string id, string labelId, int column, int row, int width, string labelResourceId, string controlId)
        {
            IUFFldTextBox textBox = UIControlBuilder.BuilderTextBox(container, id, "True", "True", "True", "False", "Left", 0, 60, 0, width, 20, column, row, 1, 1, "False", "100", string.Empty, TextBoxMode.SingleLine, TextAlign.Left, true, false, labelId, string.Empty, "100", labelResourceId, controlId);
            UIControlBuilder.BuilderUFControl(textBox, BuildTabIndex(column, row));
            return textBox;
        }

        private IUFFldDatePicker AddDatePicker(IUFContainer container, string id, string labelId, int column, int row, int width, string labelResourceId, string controlId)
        {
            IUFFldDatePicker datePicker = UIControlBuilder.BuilderDatePicker(container, id, true, true, true, "Date", "Left", 3, 60, 0, width, 20, column, row, 1, 1, "100", true, false, labelId, labelResourceId, controlId);
            UIControlBuilder.BuilderUFControl(datePicker, BuildTabIndex(column, row));
            return datePicker;
        }

        private static string BuildTabIndex(int column, int row) { return ((row * 20) + column + 1).ToString(); }

        private void BuildGridCard(IUFContainer container)
        {
            this.gridCard = UIControlBuilder.BuildCard(container, "CardGrid", false, "none", true, true, "2", string.Empty, "F060A794-3664-414D-8C04-E6DF567FEF7B");
            CommonBuilder.GridLayoutPropBuilder(container, this.gridCard, 1580, 557, 0, 2, 1, 1, "100");
            CommonBuilder.ContainerGridLayoutPropBuilder(this.gridCard, 1, 1, 0, 5, 0, 0, 0, 0);
            this.InitViewBindingContainer(this, this.gridCard, null, string.Empty, string.Empty, null, 1, string.Empty);
            UIControlBuilder.BuildContainerGridLayout(this.gridCard, 5u, new GridColumnDef[] { new GridColumnDef(new Unit(GridViewportWidth), true) }, new GridRowDef[] { new GridRowDef(new Unit(547), false) });
            this.BuildGrid(this.gridCard);
            UIControlBuilder.BuilderUFControl(this.dataGrid, "0");
            container.Controls.Add(this.gridCard);
        }

        private void ConfigureGridScrollContainer(IUFContainer container)
        {
            WebControl gridCardControl = this.gridCard as WebControl;
            if (gridCardControl != null)
            {
                gridCardControl.Style["overflow-x"] = "auto";
                gridCardControl.Style["overflow-y"] = "hidden";
                gridCardControl.Style["max-width"] = "100%";
            }

            WebControl containerControl = container as WebControl;
            if (containerControl != null)
            {
                containerControl.Style["overflow-x"] = "auto";
                containerControl.Style["overflow-y"] = "hidden";
                containerControl.Style["max-width"] = "100%";
            }
        }

        private void RegisterGridScrollScript()
        {
            string script = @"(function(){var MINW=" + GridContentWidth.ToString(CultureInfo.InvariantCulture) + @";var barId='OutsourcePRPrepareGridHScroll';var syncing=false;function grid(){var g=document.getElementById('DataGrid0');if(g&&g.getElementsByTagName&&g.getElementsByTagName('table').length)return g;var ns=document.querySelectorAll('[id*=DataGrid0]');for(var i=0;i<ns.length;i++){var n=ns[i];if(n.tagName==='INPUT'||n.tagName==='TEXTAREA')continue;if(n.getElementsByTagName&&n.getElementsByTagName('table').length&&n.offsetWidth>200)return n;}return null;}function textOf(n){return(n.innerText||n.value||n.textContent||'').replace(/\s/g,'');}function hideBtns(g){var ns=g.querySelectorAll('button,input[type=button],input[type=submit],a');for(var k=0;k<ns.length;k++){var t=textOf(ns[k]);if(t==='新增'||t==='插入'||t==='删除'){ns[k].style.display='none';}}}function minW(){try{if(window.OutsourceShortageVisibleWidth)return Math.max(320,window.OutsourceShortageVisibleWidth());}catch(e){}return MINW;}function contentWidth(g){var w=Math.max(minW(),g.scrollWidth,g.offsetWidth);var ts=g.getElementsByTagName('table');for(var i=0;i<ts.length;i++){w=Math.max(w,ts[i].scrollWidth,ts[i].offsetWidth);}return w;}function viewOf(g,w){var p=g.parentElement;for(var i=0;i<12&&p;i++,p=p.parentElement){if(p.clientWidth>300&&p.clientWidth<w&&p.clientHeight>100)return p;}return g.parentElement;}function add(arr,n){if(!n)return;if(arr.indexOf(n)<0)arr.push(n);}function scrollNodes(g,v){var arr=[];var p=g.parentElement;for(var i=0;i<12&&p;i++,p=p.parentElement){if(p.clientWidth>200&&p.scrollWidth>p.clientWidth+2)add(arr,p);}if(v){var ds=v.getElementsByTagName('div');for(var j=0;j<ds.length;j++){if(ds[j].clientWidth>200&&ds[j].scrollWidth>ds[j].clientWidth+2)add(arr,ds[j]);}}add(arr,v);return arr;}function ensureBar(){var b=document.getElementById(barId);if(!b){b=document.createElement('div');b.id=barId;var inn=document.createElement('div');inn.style.height='1px';b.appendChild(inn);document.body.appendChild(b);}b.style.position='fixed';b.style.height='18px';b.style.overflowX='scroll';b.style.overflowY='hidden';b.style.zIndex='2147483647';b.style.background='#f2f2f2';b.style.borderTop='1px solid #c8c8c8';b.style.display='block';return b;}function place(b,v,w){var r=v.getBoundingClientRect();var left=Math.max(0,r.left);var width=Math.min(window.innerWidth-left-8,Math.max(320,r.width));b.style.left=left+'px';b.style.bottom='0px';b.style.width=width+'px';if(b.firstChild)b.firstChild.style.width=w+'px';}function apply(){var g=grid();if(!g)return;hideBtns(g);g.style.transform='';g.style.marginLeft='';g.style.minWidth=minW()+'px';g.style.maxWidth='none';var w=contentWidth(g);var v=viewOf(g,w);if(!v)return;v.style.overflowX='auto';v.style.overflowY='hidden';v.style.maxWidth='100%';var b=ensureBar();place(b,v,w);var nodes=scrollNodes(g,v);var syncToGrid=function(){if(syncing)return;syncing=true;var x=b.scrollLeft;for(var i=0;i<nodes.length;i++){try{nodes[i].scrollLeft=x;}catch(e){}}syncing=false;};b.onscroll=syncToGrid;for(var i=0;i<nodes.length;i++){nodes[i].onscroll=function(){if(syncing)return;syncing=true;b.scrollLeft=this.scrollLeft;syncing=false;};}syncToGrid();}function run(){apply();setTimeout(apply,300);setTimeout(apply,1000);setTimeout(apply,2500);}if(document.readyState==='loading'){document.addEventListener('DOMContentLoaded',run);}else{run();}window.OutsourceShortageRefreshScroll=apply;window.addEventListener('resize',apply);document.addEventListener('mouseup',function(){setTimeout(apply,80);setTimeout(apply,400);});if(window.Sys&&Sys.WebForms&&Sys.WebForms.PageRequestManager){Sys.WebForms.PageRequestManager.getInstance().add_endRequest(run);}})();";
            ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "OutsourcePRPrepareGridScroll", script, true);
        }

        private void RegisterU9CompatibilityScript()
        {
            // This page can be loaded without the legacy U9 Array.contains
            // helper. U9's submit lock calls it before the postback is sent.
            string script = @"(function(){if(!Array.prototype.contains){Array.prototype.contains=function(v){return this.indexOf(v)>=0;};}})();";
            ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "OutsourceShortageU9Compatibility", script, true);
        }
        private void RegisterColumnSettingsScript()
        {
            string hiddenId = this.hiddenVisibleColumns == null ? string.Empty : this.hiddenVisibleColumns.ClientID;
            string applyButtonId = this.applyColumnsButton == null ? string.Empty : this.applyColumnsButton.ClientID;
            string applyButtonTarget = this.applyColumnsButton == null ? string.Empty : this.applyColumnsButton.UniqueID;
            string storageKey = "OutsourceShortage.VisibleColumns." + this.GetCurrentUserKey();
            string script = @"(function(){var columns=" + BuildColumnSettingsJson() + @";var hiddenId='" + JsString(hiddenId) + @"';var applyId='" + JsString(applyButtonId) + @"';var applyTarget='" + JsString(applyButtonTarget) + @"';var storageKey='" + JsString(storageKey) + @"';var maskId='OutsourcePRPrepareColumnMask';var dialogId='OutsourcePRPrepareColumnDialog';function allFields(){var a=[];for(var i=0;i<columns.length;i++)a.push(columns[i].f);return a;}function split(v){if(!v)return[];var p=v.split(','),a=[];for(var i=0;i<p.length;i++){var s=p[i].replace(/^\s+|\s+$/g,'');if(s)a.push(s);}return a;}function map(a){var m={};for(var i=0;i<a.length;i++)m[a[i]]=true;return m;}function valid(a){var cm=map(allFields()),r=[];for(var i=0;i<a.length;i++){if(cm[a[i]]&&r.indexOf(a[i])<0)r.push(a[i]);}return r;}function same(a,b){if(a.length!==b.length)return false;for(var i=0;i<a.length;i++){if(a[i]!==b[i])return false;}return true;}function saved(){var v='';try{v=localStorage.getItem(storageKey)||'';}catch(e){}var a=valid(split(v));return a.length?a:allFields();}function hidden(){return document.getElementById(hiddenId);}function setHidden(a){var h=hidden();if(h)h.value=a.join(',');}function requestApply(a){setHidden(a);if(applyTarget&&typeof window.__doPostBack==='function'){window.__doPostBack(applyTarget,'');return;}var b=document.getElementById(applyId);if(b&&typeof b.click==='function')b.click();}function close(){var m=document.getElementById(maskId),d=document.getElementById(dialogId);if(m)m.style.display='none';if(d)d.style.display='none';}function ensure(){var mask=document.getElementById(maskId),dlg=document.getElementById(dialogId);if(mask&&dlg)return dlg;mask=document.createElement('div');mask.id=maskId;mask.style.cssText='display:none;position:fixed;left:0;top:0;right:0;bottom:0;background:rgba(0,0,0,.18);z-index:2147483646;';document.body.appendChild(mask);dlg=document.createElement('div');dlg.id=dialogId;dlg.style.cssText='display:none;position:fixed;left:50%;top:50%;transform:translate(-50%,-50%);width:520px;max-width:calc(100vw - 32px);background:#fff;border:1px solid #cfcfcf;box-shadow:0 8px 26px rgba(0,0,0,.18);z-index:2147483647;font-size:14px;color:#333;';dlg.innerHTML='<div style=""height:42px;line-height:42px;padding:0 16px;border-bottom:1px solid #e5e5e5;font-weight:600;"">列设置</div><div id=""oprColumnList"" style=""padding:12px 16px;display:grid;grid-template-columns:repeat(3,1fr);gap:10px 12px;max-height:320px;overflow:auto;""></div><div style=""padding:10px 16px;border-top:1px solid #e5e5e5;text-align:right;""><button type=""button"" id=""oprColumnAll"" style=""margin-right:8px;min-width:72px;height:28px;"">全选</button><button type=""button"" id=""oprColumnReset"" style=""margin-right:8px;min-width:72px;height:28px;"">恢复默认</button><button type=""button"" id=""oprColumnCancel"" style=""margin-right:8px;min-width:72px;height:28px;"">取消</button><button type=""button"" id=""oprColumnSave"" style=""min-width:72px;height:28px;background:#18b681;color:#fff;border:0;"">保存</button></div>';document.body.appendChild(dlg);mask.onclick=close;document.getElementById('oprColumnCancel').onclick=close;document.getElementById('oprColumnAll').onclick=function(){var xs=dlg.querySelectorAll('input[type=checkbox]');for(var i=0;i<xs.length;i++)xs[i].checked=true;};document.getElementById('oprColumnReset').onclick=function(){try{localStorage.removeItem(storageKey);}catch(e){}close();requestApply(allFields());};document.getElementById('oprColumnSave').onclick=function(){var xs=dlg.querySelectorAll('input[type=checkbox]'),a=[];for(var i=0;i<xs.length;i++){if(xs[i].checked)a.push(xs[i].value);}if(!a.length){alert('请至少保留一列');return;}try{if(same(a,allFields()))localStorage.removeItem(storageKey);else localStorage.setItem(storageKey,a.join(','));}catch(e){}close();requestApply(a);};return dlg;}window.OutsourceShortageShowColumnSettings=function(){var d=ensure(),list=document.getElementById('oprColumnList'),vis=map(saved());list.innerHTML='';for(var i=0;i<columns.length;i++){var lab=document.createElement('label');lab.style.cssText='display:flex;align-items:center;gap:6px;white-space:nowrap;';var cb=document.createElement('input');cb.type='checkbox';cb.value=columns[i].f;cb.checked=!!vis[columns[i].f];lab.appendChild(cb);lab.appendChild(document.createTextNode(columns[i].t));list.appendChild(lab);}document.getElementById(maskId).style.display='block';d.style.display='block';};function run(){var a=saved(),h=hidden(),posted=valid(split(h?h.value:''));setHidden(a);if(posted.length===0&&!same(a,allFields()))setTimeout(function(){requestApply(a);},0);}if(document.readyState==='loading'){document.addEventListener('DOMContentLoaded',run);}else{run();}if(window.Sys&&Sys.WebForms&&Sys.WebForms.PageRequestManager){Sys.WebForms.PageRequestManager.getInstance().add_endRequest(run);}})();";
            ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "OutsourcePRPrepareColumnSettings", script, true);
        }
        private void RegisterOutputCompletionScript()
        {
            string outputId = this.btnOutput == null ? string.Empty : ((Control)this.btnOutput).ClientID;
            string outputTarget = this.btnOutput == null ? string.Empty : ((Control)this.btnOutput).UniqueID;
            string script = @"(function(){var outputId='" + JsString(outputId) + @"';var outputTarget='" + JsString(outputTarget) + @"';var frameId='OutsourcePRPrepareExportFrame';function textOf(n){return(n.innerText||n.value||n.textContent||'').replace(/^\s+|\s+$/g,'');}function frame(){var f=document.getElementById(frameId);if(!f){f=document.createElement('iframe');f.id=frameId;f.name=frameId;f.style.display='none';document.body.appendChild(f);}return f;}function outputButton(n){for(var i=0;i<6&&n;i++,n=n.parentElement){if(n.id===outputId||textOf(n)==='输出')return n;}return null;}function finish(b,form,oldTarget,oldEventTarget,oldEventArgument){if(form&&form.target===frameId)form.target=oldTarget;if(form){var et=form.elements['__EVENTTARGET'];if(et)et.value=oldEventTarget;var ea=form.elements['__EVENTARGUMENT'];if(ea)ea.value=oldEventArgument;}if(!b)return;b.disabled=false;b.removeAttribute('disabled');b.removeAttribute('aria-busy');b.classList.remove('loading','is-loading','uf-loading');var ns=b.querySelectorAll('span,i');for(var i=0;i<ns.length;i++)ns[i].classList.remove('loading','is-loading','uf-loading');}function prepare(e){var b=outputButton(e.target);if(!b||b.getAttribute('data-opr-output-running')==='1')return;if(!outputTarget)return;var form=b.form||document.forms[0];if(!form)return;e.preventDefault();e.stopImmediatePropagation();b.setAttribute('data-opr-output-running','1');b.disabled=true;var f=frame();var oldTarget=form.target||'';var et=form.elements['__EVENTTARGET'];var ea=form.elements['__EVENTARGUMENT'];var oldEventTarget=et?et.value:'';var oldEventArgument=ea?ea.value:'';form.target=frameId;if(et)et.value=outputTarget;if(ea)ea.value='';f.onload=function(){finish(b,form,oldTarget,oldEventTarget,oldEventArgument);};HTMLFormElement.prototype.submit.call(form);setTimeout(function(){finish(b,form,oldTarget,oldEventTarget,oldEventArgument);},5000);}function bind(){if(document._oprOutputBound)return;document._oprOutputBound=true;document.addEventListener('click',prepare,true);}if(document.readyState==='loading')document.addEventListener('DOMContentLoaded',bind);else bind();if(window.Sys&&Sys.WebForms&&Sys.WebForms.PageRequestManager)Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function(){document._oprOutputBound=false;bind();});})();";
            ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "OutsourcePRPrepareOutputCompletion", script, true);
        }        private static string BuildColumnSettingsJson()
        {
            StringBuilder json = new StringBuilder();
            json.Append("[");
            for (int i = 0; i < ListColumns.Length; i++)
            {
                if (i > 0) json.Append(",");
                json.Append("{f:'").Append(JsString(ListColumns[i].FieldName)).Append("',t:'").Append(JsString(ListColumns[i].Title)).Append("',w:").Append(ListColumns[i].Width.ToString(CultureInfo.InvariantCulture)).Append("}");
            }
            json.Append("]");
            return json.ToString();
        }

        private string GetCurrentUserKey()
        {
            string user = ReadContextValue(PDContext.Current, "UserCode", "UserID", "UserId", "UserName", "UserDisplayName");
            if (string.IsNullOrWhiteSpace(user) && this.Context != null && this.Context.User != null && this.Context.User.Identity != null)
            {
                user = this.Context.User.Identity.Name;
            }
            if (string.IsNullOrWhiteSpace(user)) user = "default";
            return user;
        }

        private static string ReadContextValue(object context, params string[] propertyNames)
        {
            if (context == null) return string.Empty;
            for (int i = 0; i < propertyNames.Length; i++)
            {
                try
                {
                    System.Reflection.PropertyInfo property = context.GetType().GetProperty(propertyNames[i]);
                    if (property == null) continue;
                    object value = property.GetValue(context, null);
                    if (value != null) return Convert.ToString(value, CultureInfo.InvariantCulture);
                }
                catch { }
            }
            return string.Empty;
        }

        private static string JsString(string value)
        {
            if (value == null) return string.Empty;
            return value.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\r", "\\r").Replace("\n", "\\n");
        }
        private void BuildGrid(IUFContainer container)
        {
            this.dataGrid = UIControlBuilder.BuildGridControl("DataGrid0", UFSoft.UBF.UI.ControlModel.EditStatus.Browse, true, true, true, true, true, true, 25, true, false);
            UIControlBuilder.BuilderUFControl(this.dataGrid, "True", "True", "0");
            CommonBuilder.GridLayoutPropBuilder(container, this.dataGrid, GridContentWidth, 530, 0, 0, 1, 1, "100");
            this.ConfigureGridScrollContainer(container);
            this.InitViewBindingContainer(this, this.dataGrid, this.Model.OutsourceShortage, "OutsourceShortage", string.Empty, null, 25, string.Empty);
            ((UFWebDataGridAdapter)this.dataGrid).PagingStrategy = (GridPagingStrategy)0;
            ((UFWebDataGridAdapter)this.dataGrid).GridSelectAllPageHandler += new GridSelectAllPageDelegate(PDListHelper.UFGridDataGrid_GridSelectAllPageDelegate);
            this.dataGrid.AllowSelectAllPage = true;
            UFGrid ufGrid = this.dataGrid as UFGrid;
            if (ufGrid != null)
            {
                ufGrid.GridWidth = GridContentWidth;
                ufGrid.Width = new Unit(GridContentWidth);
            }
            container.Controls.Add(this.dataGrid);

            this.AddHiddenNumberColumn("ID0", this.Model.OutsourceShortage.GetField("ID"), "ID", "3B67284C-C2E3-4650-9BA8-3E10BF0871B8");
            for (int i = 0; i < ListColumns.Length; i++)
            {
                this.AddListColumn(ListColumns[i]);
            }
        }


        private void AddListColumn(ListColumn column)
        {
            IUIField field = this.Model.OutsourceShortage.GetField(column.FieldName);
            if (column.Kind == ListColumnKind.Number)
            {
                this.AddNumberColumn(column.GridControlId, field, column.FieldName, column.Title, column.Width, column.ControlId);
            }
            else if (column.Kind == ListColumnKind.Date)
            {
                this.AddDateColumn(column.GridControlId, field, column.FieldName, column.Title, column.Width, column.ControlId);
            }
            else
            {
                this.AddTextColumn(column.GridControlId, field, column.FieldName, column.Title, column.Width, column.ControlId);
            }
        }
        private void AddTextColumn(string id, IUIField field, string fieldName, string title, int width, string controlId)
        {
            string fieldUid = GetFieldUid(fieldName);
            IUFDataGridColumn col = GridControlBuilder.GridColumnBuilder(this.dataGrid, id, "TextBoxColumnModel", title, 0, field, fieldName, false, true, false, false, false, true, 0, width, "100", true, false, string.Empty, fieldUid, fieldUid, controlId);
            SetColumnTitle(col, title);
            GridControlBuilder.GridTextBoxColumnBuilder((IUFTextBoxColumn)col, string.Empty, TextAlign.Left, false, string.Empty, false, "1", "1", "100");
        }

        private void AddNumberColumn(string id, IUIField field, string fieldName, string title, int width, string controlId)
        {
            string fieldUid = GetFieldUid(fieldName);
            IUFDataGridColumn col = GridControlBuilder.GridColumnBuilder(this.dataGrid, id, "NumberColumnModel", title, 0, field, fieldName, false, true, false, false, false, true, 7, width, string.Empty, true, false, string.Empty, fieldUid, fieldUid, controlId);
            SetColumnTitle(col, title);
            GridControlBuilder.GridNumberColumnBuilder((IUFNumberColumn)col, (NumbericType)1, decimal.MaxValue, decimal.MinValue, null, null, null, null, true, string.Empty, false, "1", "1");
        }

        private void AddHiddenNumberColumn(string id, IUIField field, string fieldName, string controlId)
        {
            string fieldUid = GetFieldUid(fieldName);
            IUFDataGridColumn col = GridControlBuilder.GridColumnBuilder(this.dataGrid, id, "NumberColumnModel", string.Empty, 0, field, fieldName, true, false, true, false, false, true, 7, 80, "8", true, false, string.Empty, fieldUid, fieldUid, controlId);
            GridControlBuilder.GridNumberColumnBuilder((IUFNumberColumn)col, (NumbericType)1, decimal.MaxValue, decimal.MinValue, null, null, null, null, true, string.Empty, false, "1", "1");
        }

        private void AddDateColumn(string id, IUIField field, string fieldName, string title, int width, string controlId)
        {
            string fieldUid = GetFieldUid(fieldName);
            IUFDataGridColumn col = GridControlBuilder.GridColumnBuilder(this.dataGrid, id, "DatePickerColumnModel", title, 0, field, fieldName, false, true, false, false, false, true, 3, width, "8", true, false, string.Empty, fieldUid, fieldUid, controlId);
            SetColumnTitle(col, title);
            ((IUFDatePickerColumn)col).DateTimeType = 0;
            ((IUFDatePickerColumn)col).DateTimeFormat = base.CurrentState._I18N._DateTimeFormatInfo;
        }


        private static void SetColumnTitle(object column, string title)
        {
            SetPropertyIfExists(column, "Caption", title);
            SetPropertyIfExists(column, "HeaderText", title);
            SetPropertyIfExists(column, "Text", title);
            SetPropertyIfExists(column, "DisplayName", title);
            SetPropertyIfExists(column, "Title", title);
        }

        private static void SetPropertyIfExists(object target, string propertyName, object value)
        {
            if (target == null) return;
            System.Reflection.PropertyInfo property = null;
            try { property = target.GetType().GetProperty(propertyName); }
            catch (System.Reflection.AmbiguousMatchException)
            {
                System.Reflection.PropertyInfo[] properties = target.GetType().GetProperties();
                for (int i = 0; i < properties.Length; i++)
                {
                    if (properties[i].Name == propertyName && properties[i].CanWrite)
                    {
                        property = properties[i];
                        break;
                    }
                }
            }
            if (property == null || !property.CanWrite) return;
            try { property.SetValue(target, value, null); } catch { }
        }
        private static string GetFieldUid(string fieldName)
        {
            switch (fieldName)
            {
                case "ID": return "22964AA7-035D-4B0C-9BA3-123B10A82F98";
                case "SupplierCode": return "8BF251D3-0C47-4A36-9258-3AFBF91F51DF";
                case "SupplierName": return "87096366-CCD5-418F-80EC-17AFBBB631BB";
                case "PurchaseOrderNo": return "A860F530-3528-424C-843B-F25A09D06FBF";
                case "POLineNo": return "86CCC448-9850-48A7-992B-01D8B293E56A";
                case "PickLineNo": return "6EEEC7BA-40EA-450A-B932-2E238A1FEEA0";
                case "BusinessDate": return "D44DA663-70DC-4351-B323-67360C8B6FF2";
                case "MaterialCode": return "DC0367D6-B77A-4A23-8377-D736ECF7EE55";
                case "MaterialName": return "730F79DD-099A-47E5-9E07-23F653ECCE4C";
                case "SupplyWhCode": return "4CB6A7F1-34D7-4254-AA17-99DE9F914023";
                case "SupplyWhName": return "82ECD707-514F-4369-B91A-D9A82D8A1A8B";
                case "ActualReqQty": return "98A53531-8206-4981-A38F-19BE18F7168D";
                case "IssuedQty": return "069172D1-EDE1-4D31-A130-88AD86EB8464";
                case "UnissuedQty": return "1A33C9A3-E636-4B73-AC6B-A4E06B3CDA66";
                case "InventoryDeductQty": return "AC09638C-D656-4C71-B5CD-1D485AF11E2A";
                case "ShortageQty": return "C81530E8-616E-4861-8797-365461539E46";
                case "ItemID": return "FF9FCC74-0FE8-4A81-961D-32072DCC9FE2";
                case "IssueUOMID": return "0C067673-BDFF-44F9-8A29-0ED142E6550D";
                case "SupplyWhID": return "416E464C-BCA0-42A7-A12F-82CFB81B56D3";
                default: return fieldName;
            }
        }

        private void EventBind()
        {
            if (this.btnQuery != null) this.btnQuery.Click += this.BtnQuery_Click;
            if (this.btnClear != null) this.btnClear.Click += this.BtnClear_Click;
            if (this.btnOutput != null) this.btnOutput.Click += this.BtnOutput_Click;
            if (this.btnWriteMO != null) this.btnWriteMO.Click += this.BtnWriteMO_Click;
            this.RegisterPostBackForOutput();
            this.RegisterPostBackForColumnApply();
            ((UFWebDataGridAdapter)this.dataGrid).GridMakePageEventHandler += new GridMakePageDelegate(this.UFGridDataGrid0_GridMakePageEventHandler);
            ((UFWebDataGridAdapter)this.dataGrid).GridCustomFilterHandler += new GridCustomFilterDelegate(this.UFGridDataGrid0_GridCustomFilterHandler);
            this.AfterEventBind();
        }

        private void ApplyColumnsButton_Click(object sender, EventArgs e)
        {
            this.OnDataCollect(this);
            this.IsDataBinding = true;
            this.IsConsuming = false;
        }

        private void ApplyVisibleGridColumns()
        {
            Dictionary<string, bool> visible = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            string value = this.hiddenVisibleColumns == null ? string.Empty : this.hiddenVisibleColumns.Value;
            if (string.IsNullOrWhiteSpace(value) && this.Page != null && this.Page.Request != null && this.hiddenVisibleColumns != null)
            {
                value = this.Page.Request.Form[this.hiddenVisibleColumns.UniqueID];
            }
            if (!string.IsNullOrWhiteSpace(value))
            {
                string[] fields = value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < fields.Length; i++) visible[fields[i].Trim()] = true;
            }

            for (int i = 0; i < ListColumns.Length; i++)
            {
                IUFDataGridColumn gridColumn = this.dataGrid.Columns[ListColumns[i].FieldName];
                if (gridColumn == null) continue;
                bool isVisible = visible.Count == 0 || visible.ContainsKey(ListColumns[i].FieldName);
                gridColumn.Visible = isVisible;
                GridColumn concreteColumn = gridColumn as GridColumn;
                if (concreteColumn != null) concreteColumn.VisibleEx = isVisible;
            }
        }        private void BtnQuery_Click(object sender, EventArgs e)
        {
            this.OnDataCollect(this);
            this.Model.ClearErrorMessage();
            this.LoadData();
            this.IsDataBinding = true;
            this.IsConsuming = false;
            this.Action.OnQuery(sender, e);
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            this.OnDataCollect(this);
            this.ClearFilters();
            this.Model.ClearErrorMessage();
            this.Model.OutsourceShortage.Clear();
            this.IsDataBinding = true;
            this.IsConsuming = false;
            this.Action.OnClear(sender, e);
        }

        private void BtnOutput_Click(object sender, EventArgs e)
        {
            this.OnDataCollect(this);
            this.Model.ClearErrorMessage();
            this.IsDataBinding = false;
            this.IsConsuming = false;
            this.Action.OnOutput(sender, e);
            this.ExportCurrentQuery();
        }

        private void BtnWriteMO_Click(object sender, EventArgs e)
        {
            this.OnDataCollect(this);
            this.Model.ClearErrorMessage();
            string message;
            try
            {
                DataTable rows = this.BuildMatchedTable(false);
                if (rows.Rows.Count == 0)
                {
                    message = "当前组织没有欠料结果，未修改生产订单备料。";
                }
                else
                {
                    WriteToMOResult result = this.WriteToMO(rows);
                    message = string.Format(CultureInfo.InvariantCulture,
                        "目标生产订单：{0}；成功 {1} 行，失败 {2} 行；最近更新时间：{3}{4}",
                        result.MONo, result.SuccessCount, result.Failures.Count,
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                        result.Failures.Count == 0 ? string.Empty : "；失败原因：" + string.Join("；", result.Failures.ToArray()));
                }
            }
            catch (Exception ex)
            {
                message = "写入工单备料失败：" + ex.Message;
            }
            this.Model.ClearErrorMessage();
            this.ShowWindowStatus(message, true, true);
            this.IsDataBinding = true;
            this.IsConsuming = false;
        }

        private void RegisterPostBackForOutput()
        {
            // Output is submitted by the dedicated hidden-frame script below.
            // Registering it as a normal U9 postback leaves the page loading mask active.
        }

        private void RegisterPostBackForColumnApply()
        {
            ScriptManager scriptManager = this.Page == null ? null : ScriptManager.GetCurrent(this.Page);
            if (scriptManager != null && this.applyColumnsButton != null)
            {
                scriptManager.RegisterPostBackControl(this.applyColumnsButton);
            }
        }
        private void UFGridDataGrid0_GridMakePageEventHandler(object sender, GridMakePageEventArgs e)
        {
            CommandFactory.DoCommand("GridMakePage", this.Action, this.dataGrid, e);
        }

        private void UFGridDataGrid0_GridCustomFilterHandler(object sender, GridCustomFilterArgs e)
        {
            CommandFactory.DoCommand("GridCustomFilter", this.Action, this.dataGrid, e);
        }

        private void LoadData()
        {
            try
            {
                this.Model.OutsourceShortage.Clear();
                DataTable table = this.BuildMatchedTable();
                foreach (DataRow row in table.Rows)
                {
                    OutsourceShortageRecord record = this.Model.OutsourceShortage.AddNewUIRecord();
                    record.ID = GetLong(row, "ID");
                    record.SupplierCode = GetString(row, "SupplierCode");
                    record.SupplierName = GetString(row, "SupplierName");
                    record.PurchaseOrderNo = GetString(row, "PurchaseOrderNo");
                    record.POLineNo = GetLong(row, "POLineNo");
                    record.PickLineNo = GetLong(row, "PickLineNo");
                    record.BusinessDate = GetNullableDateTime(row, "BusinessDate");
                    record.MaterialCode = GetString(row, "MaterialCode");
                    record.MaterialName = GetString(row, "MaterialName");
                    record.SupplyWhCode = GetString(row, "SupplyWhCode");
                    record.SupplyWhName = GetString(row, "SupplyWhName");
                    record.ActualReqQty = GetNullableDouble(row, "ActualReqQty");
                    record.IssuedQty = GetNullableDouble(row, "IssuedQty");
                    record.UnissuedQty = GetNullableDouble(row, "UnissuedQty");
                    record.InventoryDeductQty = GetNullableDouble(row, "InventoryDeductQty");
                    record.ShortageQty = GetNullableDouble(row, "ShortageQty");
                    record.ItemID = GetLong(row, "ItemID");
                    record.IssueUOMID = GetLong(row, "IssueUOMID");
                    record.SupplyWhID = GetLong(row, "SupplyWhID");
                }
                if (this.Model.OutsourceShortage.Records.Count > 0) this.Model.OutsourceShortage.FocusedIndex = 0;
            }
            catch (Exception ex)
            {
                this.Model.OutsourceShortage.Clear();
                this.Model.ErrorMessage.Message = "委外欠料统计查询失败：" + ex.Message;
            }
        }

        private static bool ContainsText(string value, string search)
        {
            return value != null && search != null && value.IndexOf(search.Trim(), StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private DataTable BuildMatchedTable() { return this.BuildMatchedTable(true); }

        private DataTable BuildMatchedTable(bool applyFilters)
        {
            long org = this.GetCurrentOrgID();
            DataTable source = SqlHelp.RunSqlDataTable(@"
SELECT s.ID, p.Supplier_Code AS SupplierCode, p.Supplier_ShortName AS SupplierName,
       p.DocNo AS PurchaseOrderNo, l.DocLineNo AS POLineNo, s.PickLineNo,
       p.BusinessDate, s.ItemInfo_ItemID AS ItemID, s.ItemInfo_ItemCode AS MaterialCode,
       s.ItemInfo_ItemName AS MaterialName, s.SupplyWh AS SupplyWhID,
       wh.Code AS SupplyWhCode, whl.Name AS SupplyWhName,
       s.ActualReqQty, s.IssuedQty, s.IssueUOM AS IssueUOMID,
       NULLIF(s.IUToIBURate, 0) AS IUToIBURate
FROM dbo.PM_PurchaseOrder p WITH(NOLOCK)
JOIN dbo.PM_POLine l WITH(NOLOCK) ON l.PurchaseOrder = p.ID
JOIN dbo.CBO_SCMPickHead h WITH(NOLOCK) ON h.POLine = l.ID
JOIN dbo.CBO_SCMPickList s WITH(NOLOCK) ON s.PicKHead = h.ID
LEFT JOIN dbo.CBO_Wh wh WITH(NOLOCK) ON wh.ID = s.SupplyWh AND wh.Org = p.Org
LEFT JOIN dbo.CBO_Wh_Trl whl WITH(NOLOCK) ON whl.ID = wh.ID AND whl.SysMLFlag = N'zh-CN'
WHERE p.Org = @Org AND p.BizType = (
    SELECT TOP 1 EValue FROM dbo.UBF_Sys_ExtEnumValue WITH(NOLOCK) WHERE Code = N'PM055')
  AND l.Status IN (0,1,2)
  AND ISNULL(s.ActualReqQty,0) - ISNULL(s.IssuedQty,0) > 0
ORDER BY p.BusinessDate, l.DocLineNo, s.PickLineNo, s.ID",
                new SqlParameter("@Org", SqlDbType.BigInt) { Value = org });
            DataTable inventory = SqlHelp.RunSqlDataTable(@"
SELECT ItemInfo_ItemID AS ItemID, Wh AS SupplyWhID,
       SUM(CASE WHEN ISNULL(StoreBaseUOM,0)=0 OR ISNULL(SUToSBURate,0)=0
                THEN ISNULL(StoreQty,0)-ISNULL(ResvStQty,0)-ISNULL(ResvOccupyStQty,0)
                ELSE (ISNULL(StoreQty,0)-ISNULL(ResvStQty,0)-ISNULL(ResvOccupyStQty,0))*SUToSBURate END) AS AvailableBaseQty
FROM dbo.InvTrans_WhQoh WITH(NOLOCK)
WHERE LogisticOrg = @Org
GROUP BY ItemInfo_ItemID, Wh",
                new SqlParameter("@Org", SqlDbType.BigInt) { Value = org });

            Dictionary<string, decimal> availableBase = new Dictionary<string, decimal>();
            foreach (DataRow row in inventory.Rows)
                availableBase[InventoryKey(GetLong(row, "ItemID"), GetLong(row, "SupplyWhID"))] = Math.Max(0m, GetDecimal(row, "AvailableBaseQty"));

            DataTable result = CreateResultTable();
            long rowNo = 0;
            foreach (DataRow sourceRow in source.Rows)
            {
                decimal actual = GetDecimal(sourceRow, "ActualReqQty");
                decimal issued = GetDecimal(sourceRow, "IssuedQty");
                decimal unissued = Math.Max(0m, actual - issued);
                decimal deducted = 0m;
                long warehouseId = GetLong(sourceRow, "SupplyWhID");
                if (warehouseId > 0)
                {
                    string key = InventoryKey(GetLong(sourceRow, "ItemID"), warehouseId);
                    decimal baseQty;
                    decimal issueToBase = GetDecimal(sourceRow, "IUToIBURate");
                    if (issueToBase <= 0m) throw new InvalidOperationException("料号 " + GetString(sourceRow, "MaterialCode") + " 缺少标准发料单位换算率。");
                    if (availableBase.TryGetValue(key, out baseQty) && baseQty > 0m)
                    {
                        decimal availableIssueQty = baseQty / issueToBase;
                        deducted = Math.Min(unissued, availableIssueQty);
                        availableBase[key] = Math.Max(0m, baseQty - deducted * issueToBase);
                    }
                }
                decimal shortage = unissued - deducted;
                if (shortage <= 0m) continue;
                DataRow row = result.NewRow();
                row["ID"] = ++rowNo;
                foreach (string name in new[] { "SupplierCode", "SupplierName", "PurchaseOrderNo", "POLineNo", "PickLineNo", "BusinessDate", "MaterialCode", "MaterialName", "SupplyWhCode", "SupplyWhName", "ItemID", "IssueUOMID", "SupplyWhID" })
                    row[name] = sourceRow[name] == DBNull.Value ? DBNull.Value : sourceRow[name];
                row["ActualReqQty"] = Convert.ToDouble(actual);
                row["IssuedQty"] = Convert.ToDouble(issued);
                row["UnissuedQty"] = Convert.ToDouble(unissued);
                row["InventoryDeductQty"] = Convert.ToDouble(deducted);
                row["ShortageQty"] = Convert.ToDouble(shortage);
                result.Rows.Add(row);
            }
            return applyFilters ? ApplyResultFilters(result) : result;
        }

        private static DataTable CreateResultTable()
        {
            DataTable table = new DataTable();
            string[] names = { "ID", "SupplierCode", "SupplierName", "PurchaseOrderNo", "POLineNo", "PickLineNo", "BusinessDate", "MaterialCode", "MaterialName", "SupplyWhCode", "SupplyWhName", "ActualReqQty", "IssuedQty", "UnissuedQty", "InventoryDeductQty", "ShortageQty", "ItemID", "IssueUOMID", "SupplyWhID" };
            Type[] types = { typeof(long), typeof(string), typeof(string), typeof(string), typeof(long), typeof(long), typeof(DateTime), typeof(string), typeof(string), typeof(string), typeof(string), typeof(double), typeof(double), typeof(double), typeof(double), typeof(double), typeof(long), typeof(long), typeof(long) };
            for (int i = 0; i < names.Length; i++) table.Columns.Add(names[i], types[i]);
            return table;
        }

        private DataTable ApplyResultFilters(DataTable source)
        {
            string supplier = GetText(this.txtSupplier);
            string po = GetText(this.txtPurchaseOrder);
            string item = GetText(this.txtMaterialCode);
            string warehouse = GetText(this.txtWarehouse);
            DateTime? from = GetDate(this.dtFromDate);
            DateTime? to = GetDate(this.dtToDate);
            DataTable filtered = source.Clone();
            foreach (DataRow row in source.Rows)
            {
                if (!string.IsNullOrWhiteSpace(supplier) && !ContainsText(GetString(row, "SupplierCode") + " " + GetString(row, "SupplierName"), supplier)) continue;
                if (!string.IsNullOrWhiteSpace(po) && !ContainsText(GetString(row, "PurchaseOrderNo"), po)) continue;
                if (!string.IsNullOrWhiteSpace(item) && !ContainsText(GetString(row, "MaterialCode") + " " + GetString(row, "MaterialName"), item)) continue;
                if (!string.IsNullOrWhiteSpace(warehouse) && !ContainsText(GetString(row, "SupplyWhCode") + " " + GetString(row, "SupplyWhName"), warehouse)) continue;
                DateTime? date = GetNullableDateTime(row, "BusinessDate");
                if (from.HasValue && (!date.HasValue || date.Value < from.Value)) continue;
                if (to.HasValue && (!date.HasValue || date.Value > to.Value)) continue;
                filtered.ImportRow(row);
            }
            return filtered;
        }

        private static string InventoryKey(long itemId, long warehouseId) { return itemId.ToString(CultureInfo.InvariantCulture) + ":" + warehouseId.ToString(CultureInfo.InvariantCulture); }

        private WriteToMOResult WriteToMO(DataTable rows)
        {
            DataTable target = SqlHelp.RunSqlDataTable(@"
SELECT TOP 1 m.ID, m.DocNo, m.DocState
FROM dbo.MO_MO m WITH(NOLOCK)
JOIN dbo.MO_MODocType t WITH(NOLOCK) ON t.ID = m.MODocType
WHERE m.Org = @Org AND t.Code = N'QT01'
ORDER BY m.BusinessDate, m.ID",
                new SqlParameter("@Org", SqlDbType.BigInt) { Value = this.GetCurrentOrgID() });
            if (target.Rows.Count == 0) throw new InvalidOperationException("当前组织未找到单据类型 QT01 的生产订单。");
            long moId = GetLong(target.Rows[0], "ID");
            string moNo = GetString(target.Rows[0], "DocNo");
            if (Convert.ToInt32(target.Rows[0]["DocState"], CultureInfo.InvariantCulture) != 0)
                throw new InvalidOperationException("第一张 QT01 生产订单 " + moNo + " 当前状态不可编辑，未尝试下一张。");
            WriteToMORequest request = new WriteToMORequest {
                MOID = moId, OrgID = this.GetCurrentOrgID(),
                ItemIDs = new long[rows.Rows.Count], IssueUOMIDs = new long[rows.Rows.Count],
                SupplyWhIDs = new long[rows.Rows.Count], ShortageQtys = new decimal[rows.Rows.Count],
                MaterialCodes = new string[rows.Rows.Count]
            };
            for (int i = 0; i < rows.Rows.Count; i++)
            {
                DataRow row = rows.Rows[i];
                request.ItemIDs[i] = GetLong(row, "ItemID");
                request.IssueUOMIDs[i] = GetLong(row, "IssueUOMID");
                request.SupplyWhIDs[i] = GetLong(row, "SupplyWhID");
                request.ShortageQtys[i] = GetDecimal(row, "ShortageQty");
                request.MaterialCodes[i] = GetString(row, "MaterialCode");
            }
            WriteToMOProxy proxy = new WriteToMOProxy { Request = request };
            request.DeleteOnly = true;
            WriteToMOResult deleteResult = proxy.Do();
            if (deleteResult == null) throw new InvalidOperationException("清理原备料时 BP 未返回结果。");
            if (deleteResult.Failures.Count > 0)
                throw new InvalidOperationException("原备料清理失败：" + string.Join("；", deleteResult.Failures.ToArray()));

            request.DeleteOnly = false;
            request.SkipDelete = true;
            proxy = new WriteToMOProxy { Request = request };
            WriteToMOResult result = proxy.Do();
            if (result == null) throw new InvalidOperationException("新增欠料备料时 BP 未返回结果。");
            if (string.IsNullOrWhiteSpace(result.MONo)) result.MONo = moNo;
            if (result.Failures.Count > 0)
                result.Failures.Insert(0, "原备料已清空，新增阶段存在失败行");
            return result;
        }

        private static decimal ReceiptAvailableQty(DataRow row) { foreach (string n in new[] { "RcvQtyTU", "ArriveQtyTU", "PlanQtyTU" }) { decimal v = GetDecimal(row, n); if (v > 0m) return v; } return 0m; }
        private static DateTime? GetDateValue(DataRow row, params string[] names) { foreach (string n in names) { DateTime? v = DbDate(row, n); if (v.HasValue) return v; } return null; }
        private static DateTime? DbDate(DataRow row, string name) { if (row[name] == DBNull.Value || row[name] == null) return null; return Convert.ToDateTime(row[name]); }
        private static decimal GetDecimal(DataRow row, string name) { return row[name] == DBNull.Value || row[name] == null ? 0m : Convert.ToDecimal(row[name], CultureInfo.InvariantCulture); }
        private long GetCurrentOrgID()
        {
            string orgIdText = PDContext.Current == null ? string.Empty : Convert.ToString(PDContext.Current.OrgID);
            long orgId;
            if (!long.TryParse(orgIdText, out orgId) || orgId <= 0)
            {
                throw new InvalidOperationException("无法获取当前登录组织，已停止查询以避免显示跨组织数据。");
            }
            return orgId;
        }

        private string ResolveDocumentCode(long id)
        {
            DataTable table = SqlHelp.RunSqlDataTable(@"
SELECT TOP 1 Code
FROM dbo.MO_SimuDoc WITH(NOLOCK)
WHERE ID = @ID AND SimuDocOrg = @Org",
                new SqlParameter("@ID", SqlDbType.BigInt) { Value = id },
                new SqlParameter("@Org", SqlDbType.BigInt) { Value = this.GetCurrentOrgID() });
            if (table.Rows.Count == 0 || table.Rows[0]["Code"] == DBNull.Value) return string.Empty;
            return Convert.ToString(table.Rows[0]["Code"], CultureInfo.InvariantCulture).Trim();
        }

        private static void AddLikeFilter(List<string> filters, List<SqlParameter> parameters, string columnName, string parameterName, string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            filters.Add(columnName + " LIKE " + parameterName);
            parameters.Add(new SqlParameter(parameterName, SqlDbType.NVarChar, 200) { Value = "%" + value.Trim() + "%" });
        }

        private string GetText(IUFFldTextBox textBox) { return textBox == null || textBox.Text == null ? string.Empty : textBox.Text.Trim(); }

        private DateTime? GetDate(IUFFldDatePicker datePicker)
        {
            if (datePicker == null || !datePicker.Value.HasValue) return null;
            return datePicker.Value.Value.Date;
        }

        private void ExportCurrentQuery()
        {
            try
            {
                DataTable table = this.BuildMatchedTable();
                this.WriteExcelResponse(table);
            }
            catch (Exception ex)
            {
                this.Model.ErrorMessage.Message = "缺料表输出失败：" + ex.Message;
            }
        }

        private void WriteExcelResponse(DataTable table)
        {
            List<ListColumn> columns = this.GetVisibleExportColumns(table);
            StringBuilder html = new StringBuilder();
            html.Append("<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" /></head><body><table border=\"1\"><tr>");
            for (int i = 0; i < columns.Count; i++)
            {
                html.Append("<th>").Append(System.Web.HttpUtility.HtmlEncode(columns[i].Title)).Append("</th>");
            }
            html.Append("</tr>");

            foreach (DataRow row in table.Rows)
            {
                html.Append("<tr>");
                for (int i = 0; i < columns.Count; i++)
                {
                    html.Append("<td>").Append(System.Web.HttpUtility.HtmlEncode(FormatExportValue(row, columns[i].FieldName))).Append("</td>");
                }
                html.Append("</tr>");
            }
            html.Append("</table></body></html>");

            string fileName = "委外欠料统计_" + DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture) + ".xls";
            this.Page.Response.Clear();
            this.Page.Response.Buffer = true;
            this.Page.Response.Charset = "utf-8";
            this.Page.Response.ContentEncoding = Encoding.UTF8;
            this.Page.Response.ContentType = "application/vnd.ms-excel";
            this.Page.Response.AddHeader("Content-Disposition", "attachment;filename=" + System.Web.HttpUtility.UrlEncode(fileName, Encoding.UTF8));
            this.Page.Response.Write(html.ToString());
            this.Page.Response.Flush();
            System.Web.HttpContext.Current.ApplicationInstance.CompleteRequest();
        }

        private List<ListColumn> GetVisibleExportColumns(DataTable table)
        {
            Dictionary<string, bool> visibleFields = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            string visibleText = this.hiddenVisibleColumns == null ? string.Empty : this.hiddenVisibleColumns.Value;
            if (string.IsNullOrWhiteSpace(visibleText) && this.Page != null && this.Page.Request != null && this.hiddenVisibleColumns != null)
            {
                visibleText = this.Page.Request.Form[this.hiddenVisibleColumns.UniqueID];
            }
            if (!string.IsNullOrWhiteSpace(visibleText))
            {
                string[] fields = visibleText.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < fields.Length; i++)
                {
                    string field = fields[i].Trim();
                    if (!string.IsNullOrEmpty(field)) visibleFields[field] = true;
                }
            }

            List<ListColumn> columns = new List<ListColumn>();
            for (int i = 0; i < ListColumns.Length; i++)
            {
                if (!table.Columns.Contains(ListColumns[i].FieldName)) continue;
                if (visibleFields.Count > 0 && !visibleFields.ContainsKey(ListColumns[i].FieldName)) continue;
                columns.Add(ListColumns[i]);
            }
            if (columns.Count == 0)
            {
                for (int i = 0; i < ListColumns.Length; i++)
                {
                    if (table.Columns.Contains(ListColumns[i].FieldName)) columns.Add(ListColumns[i]);
                }
            }
            return columns;
        }
        private static string FormatExportValue(DataRow row, string columnName)
        {
            if (row[columnName] == DBNull.Value || row[columnName] == null) return string.Empty;
            object value = row[columnName];
            if (value is DateTime) return ((DateTime)value).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            if (value is decimal) return ((decimal)value).ToString("0.#########", CultureInfo.InvariantCulture);
            if (value is double) return ((double)value).ToString("0.#########", CultureInfo.InvariantCulture);
            if (value is float) return ((float)value).ToString("0.#########", CultureInfo.InvariantCulture);
            return Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        private void ClearFilters()
        {
            this.txtSupplier.Text = string.Empty;
            this.txtPurchaseOrder.Text = string.Empty;
            this.txtMaterialCode.Text = string.Empty;
            this.txtWarehouse.Text = string.Empty;
            this.dtFromDate.Value = null;
            this.dtToDate.Value = null;
        }

        private static string GetString(DataRow row, string columnName)
        {
            if (row[columnName] == DBNull.Value || row[columnName] == null) return string.Empty;
            return row[columnName].ToString();
        }

        private static long GetLong(DataRow row, string columnName)
        {
            if (row[columnName] == DBNull.Value || row[columnName] == null) return 0;
            return Convert.ToInt64(row[columnName]);
        }

        private static int? GetNullableInt(DataRow row, string columnName)
        {
            if (row[columnName] == DBNull.Value || row[columnName] == null) return null;
            return Convert.ToInt32(row[columnName]);
        }

        private static double? GetNullableDouble(DataRow row, string columnName)
        {
            if (row[columnName] == DBNull.Value || row[columnName] == null) return null;
            return Convert.ToDouble(row[columnName]);
        }

        private static DateTime? GetNullableDateTime(DataRow row, string columnName)
        {
            if (row[columnName] == DBNull.Value || row[columnName] == null) return null;
            return Convert.ToDateTime(row[columnName]).Date;
        }

        public void AfterCreateChildControls() { }
        public void AfterEventBind() { }
        public void BeforeUIModelBinding() { }
        public void AfterUIModelBinding() { }
    }
}






