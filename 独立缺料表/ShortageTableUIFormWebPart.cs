using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using U9Custom.UI.ManufactureSimulateShortageTable.Independent.Common;
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

namespace U9Custom.UI.ManufactureSimulateShortageTable.Independent
{
    [FormRegister("U9Custom_ShortageTable", "U9Custom.UI.ManufactureSimulateShortageTable.Independent.ShortageTableUIFormWebPart", "U9Custom.UI.ManufactureSimulateShortageTable.Independent", "8FBEE7F8-535F-42F0-8974-DDCADC2E90FF", "WebPart", "True", 1600, 690)]
    public class ShortageTableUIFormWebPart : BaseWebForm
    {
        private ShortageTableModelModel uiModel;
        private FormAdjust adjust;
        private UpdatePanel updatePanel;
        private HiddenField wpFindID;
        private IUFCard filterCard;
        private IUFCard gridCard;
        private IUFDataGrid dataGrid;
        private IUFButton btnQuery;
        private IUFButton btnClear;
        private IUFButton btnOutput;
        private IUFButton btnColumnSettings;
        private HiddenField hiddenVisibleColumns;
        private LinkButton applyColumnsButton;
        private IUFFldTextBox txtDocNo;
        private IUFFldTextBox txtMaterialCode;
        private IUFFldTextBox txtSupplyNo;
        private IUFFldTextBox txtSupplyType;
        private IUFFldTextBox txtProductionOrder;
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
            new ListColumn("ProductionOrder", "生产订单", ListColumnKind.Text, 120, "C817CF3E-0911-457D-9D9D-045DC63FC54F"),
            new ListColumn("MaterialCode", "备料料号", ListColumnKind.Text, 110, "6008A1C9-F67F-49FC-8B2D-7256E78807BF"),
            new ListColumn("MaterialName", "备料品名", ListColumnKind.Text, 150, "BC720D0B-88EC-4A1F-8106-8863A374536C"),
            new ListColumn("DemandQty", "需求量", ListColumnKind.Number, 90, "1E8EAA5A-E34C-4C10-91D6-0232EF125204"),
            new ListColumn("PlanStartDate", "计划开工日期", ListColumnKind.Date, 110, "878EF37D-8F5D-4ACE-8825-64792CA02A57"),
            new ListColumn("ReqDate", "需求日期", ListColumnKind.Date, 100, "32478487-D165-456E-8F47-32C2A26A3DBE"),
            new ListColumn("ScarceQty", "缺料量", ListColumnKind.Number, 90, "14E0C86E-2099-4A6F-923E-1FE63C2BFF17"),
            new ListColumn("SupplyType", "匹配类型", ListColumnKind.Text, 90, "BAA406C4-3488-4F3B-95E3-589241B27CF4"),
            new ListColumn("BusinessPerson", "业务员", ListColumnKind.Text, 100, "C2B3C0AF-2B9E-4B84-AC4B-2D6F3C1D4C01"),
            new ListColumn("SupplyNo", "供应单号", ListColumnKind.Text, 140, "CC75E000-8DA2-41DF-887E-AF8B1C540582"),
            new ListColumn("SupplyLineNo", "行号", ListColumnKind.Text, 70, "E05A5D22-46C5-4A4A-AB46-2A1F0DD5E9A1"),
            new ListColumn("SupplyDate", "供应日期", ListColumnKind.Date, 100, "B4BAFE48-649A-492F-8724-00D882A61FFA"),
            new ListColumn("MatchedQty", "匹配数量", ListColumnKind.Number, 90, "E886BD24-F801-4D00-8A97-B83D4788338E"),
            new ListColumn("RemainingShortage", "剩余缺料", ListColumnKind.Number, 90, "275471AC-203E-482F-8C96-4F0445328CC4")
        };

        private static readonly int GridContentWidth = CalculateGridContentWidth();

        private static int CalculateGridContentWidth()
        {
            int width = 0;
            for (int i = 0; i < ListColumns.Length; i++) width += ListColumns[i].Width;
            return width;
        }

        public ShortageTableUIFormWebPart()
        {
            this.FormID = "8FBEE7F8-535F-42F0-8974-DDCADC2E90FF";
            this.IsAutoSize = true;
        }

        public new ShortageTableModelAction Action
        {
            get { return (ShortageTableModelAction)base.Action; }
            set { base.Action = value; }
        }

        public new ShortageTableModelModel Model
        {
            get
            {
                if (this.uiModel == null)
                {
                    this.uiModel = new ShortageTableModelModel();
                }
                return this.uiModel;
            }
            set { this.uiModel = value; }
        }

        protected override IUIModel UIModel
        {
            get { return this.Model; }
            set { this.Model = value as ShortageTableModelModel; }
        }

        protected override void OnInit(EventArgs e) { this.OnInit2(e); }

        protected override void OnInitDo(EventArgs e)
        {
            this.Page.InitComplete += this.Page_InitComplete;
            WebPartBuilder.InitWebPart(this);
            this.Action = new ShortageTableModelAction(this);
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
                if (this.Model == null) this.Model = new ShortageTableModelModel();
                if (!this.Page.IsPostBack)
                {
                    string code = this.Page.Request.QueryString["__ShortageCode"];
                    long id;
                    if (long.TryParse(this.Page.Request.QueryString["__ShortageID"], out id) && id > 0)
                    {
                        string resolvedCode = this.ResolveDocumentCode(id);
                        if (!string.IsNullOrWhiteSpace(resolvedCode)) code = resolvedCode;
                    }
                    if (!string.IsNullOrWhiteSpace(code))
                    {
                        this.txtDocNo.Text = code.Trim();
                        this.LoadData();
                    }
                }
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
            IUFContainer container = UIControlBuilder.BuildTopLevelContainer(this, "ShortageTableUIForm", true, 1600, 690);
            CommonBuilder.ContainerGridLayoutPropBuilder(container, 1, 2, 0, 10, 10, 10, 10, 10);
            this.InitViewBindingContainer(this, container, null, string.Empty, string.Empty, null, 1, string.Empty);
            UIControlBuilder.BuildContainerGridLayout(container, 10u, new GridColumnDef[] { new GridColumnDef(new Unit(1580), true) }, new GridRowDef[] { new GridRowDef(new Unit(110), true), new GridRowDef(new Unit(540), true) });
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
            this.BuildFilterCard(container);
            UIControlBuilder.BuilderUFControl(this.filterCard, "0");
            this.BuildGridCard(container);
            UIControlBuilder.BuilderUFControl(this.gridCard, "1");
            this.EventBind();
            this.AfterCreateChildControls();
        }

        private void BuildFilterCard(IUFContainer container)
        {
            this.filterCard = UIControlBuilder.BuildCard(container, "CardFilter", false, "none", true, true, "1", string.Empty, "310603C6-1B11-461B-89FB-1FDE86B1CF4E");
            CommonBuilder.GridLayoutPropBuilder(container, this.filterCard, 1580, 80, 0, 0, 1, 1, "100");
            CommonBuilder.ContainerGridLayoutPropBuilder(this.filterCard, 16, 2, 0, 5, 15, 0, 10, 0);
            this.InitViewBindingContainer(this, this.filterCard, null, string.Empty, string.Empty, null, 1, string.Empty);
            UIControlBuilder.BuildContainerGridLayout(this.filterCard, 5u,
                new GridColumnDef[] { new GridColumnDef(new Unit(90), true), new GridColumnDef(new Unit(145), true), new GridColumnDef(new Unit(90), true), new GridColumnDef(new Unit(145), true), new GridColumnDef(new Unit(90), true), new GridColumnDef(new Unit(145), true), new GridColumnDef(new Unit(90), true), new GridColumnDef(new Unit(145), true), new GridColumnDef(new Unit(90), true), new GridColumnDef(new Unit(30), true), new GridColumnDef(new Unit(90), true), new GridColumnDef(new Unit(30), true), new GridColumnDef(new Unit(90), true), new GridColumnDef(new Unit(30), true), new GridColumnDef(new Unit(90), true), new GridColumnDef(new Unit(250), true) },
                new GridRowDef[] { new GridRowDef(new Unit(30), true), new GridRowDef(new Unit(30), true) });
            this.AddLabel(this.filterCard, "lblDocNo", "单据编码", 0, 0, "1E6523ED-538C-42F2-BF8B-A29D1AD96748", "C89D89A7-82A3-48C4-9B32-88D605561702");
            this.txtDocNo = this.AddTextBox(this.filterCard, "txtDocNo", "lblDocNo", 1, 0, "1E6523ED-538C-42F2-BF8B-A29D1AD96748", "7CB494D3-371B-41F4-811E-8D7FD26F2E31");
            this.AddLabel(this.filterCard, "lblPRItemCode", "备料料号", 2, 0, "1C27A1D8-F6CB-4F4E-B5D7-C0B516DDE789", "DEBD16E3-657D-4203-8D33-6C94C839A3C1");
            this.txtMaterialCode = this.AddTextBox(this.filterCard, "txtMaterialCode", "lblPRItemCode", 3, 0, "1C27A1D8-F6CB-4F4E-B5D7-C0B516DDE789", "3C1A8E94-ADA4-49DA-9E3E-78AA2BEB5DA2");
            this.AddLabel(this.filterCard, "lblPRItemName", "供应单号", 4, 0, "255840EE-77CA-45CF-B2A6-4467C731C9FE", "0F752C2B-CFD5-4623-8072-3A1621F55C6C");
            this.txtSupplyNo = this.AddTextBox(this.filterCard, "txtSupplyNo", "lblPRItemName", 5, 0, "255840EE-77CA-45CF-B2A6-4467C731C9FE", "86B82AFD-D4CD-4108-B7B3-0AA28CB6037B");
            this.AddLabel(this.filterCard, "lblComponentItemCode", "匹配类型", 0, 1, "4848722D-DB7F-4465-9A7D-4761985E3BEC", "62A21A9E-8D7B-47BF-AAEF-A9719054E379");
            this.txtSupplyType = this.AddTextBox(this.filterCard, "txtSupplyType", "lblComponentItemCode", 1, 1, "4848722D-DB7F-4465-9A7D-4761985E3BEC", "036D6C9D-B4DB-4486-9D92-27A613EA4608");
            this.AddLabel(this.filterCard, "lblComponentItemName", "生产订单", 2, 1, "CA930ACD-90A9-4E93-8FD4-D13063FC0D6D", "3C5896F0-893D-4F2E-A6B5-D7AD7EC744E4");
            this.txtProductionOrder = this.AddTextBox(this.filterCard, "txtProductionOrder", "lblComponentItemName", 3, 1, "CA930ACD-90A9-4E93-8FD4-D13063FC0D6D", "1A0D167E-C3E1-4F89-B91E-DB3EBB9DF192");
            this.AddLabel(this.filterCard, "lblFromDate", "供应日期从", 4, 1, "B5B52689-A04F-438A-A5BA-418C65719B36", "4854F6C6-5404-4C5D-B3D5-C42454BE12EB");
            this.dtFromDate = this.AddDatePicker(this.filterCard, "dtFromDate", "lblFromDate", 5, 1, "B5B52689-A04F-438A-A5BA-418C65719B36", "7ABF6173-4D75-411C-8F77-D7BC474373E0");
            this.AddLabel(this.filterCard, "lblToDate", "供应日期到", 6, 1, "9CF69B64-FE87-4984-AD0A-0F97A3C6B4A7", "F65C5857-0603-4458-8E10-F7AE2BB813E0");
            this.dtToDate = this.AddDatePicker(this.filterCard, "dtToDate", "lblToDate", 7, 1, "9CF69B64-FE87-4984-AD0A-0F97A3C6B4A7", "523F95B2-8473-4823-A970-153002C99A87");
            this.btnQuery = UIControlBuilder.BuilderUFButton(this.filterCard, true, "BtnQuery", true, true, 80, 22, 8, 0, 1, 1, "100", string.Empty, this.Model.ElementID, "OnQuery", false, "038DE8AD-C349-4E0E-A338-8E7629E6BAC7", "038DE8AD-C349-4E0E-A338-8E7629E6BAC7", "D62AACCE-8A36-46B1-873F-29E328CC1B06");
            this.btnQuery.Text = "查询";
            UIControlBuilder.BuilderUFControl(this.btnQuery, "8");
            this.btnClear = UIControlBuilder.BuilderUFButton(this.filterCard, true, "BtnClear", true, true, 80, 22, 10, 0, 1, 1, "100", string.Empty, this.Model.ElementID, "OnClear", false, "4F8D8D0E-84C6-4A79-9886-89A645AAE571", "4F8D8D0E-84C6-4A79-9886-89A645AAE571", "FDA196DC-5AF2-4140-8343-BF6951ED2363");
            this.btnClear.Text = "清空";
            UIControlBuilder.BuilderUFControl(this.btnClear, "10");
            this.btnOutput = UIControlBuilder.BuilderUFButton(this.filterCard, true, "BtnOutput", true, true, 80, 22, 12, 0, 1, 1, "100", string.Empty, this.Model.ElementID, "OnOutput", false, "688FD549-5A92-49CC-8072-6D5377F6409A", "688FD549-5A92-49CC-8072-6D5377F6409A", "AB7E0E96-6E12-4C41-BEA7-4884359D3692");
            this.btnOutput.Text = "输出";
            UIControlBuilder.BuilderUFControl(this.btnOutput, "12");
            this.btnColumnSettings = UIControlBuilder.BuilderUFButton(this.filterCard, true, "BtnColumnSettings", true, true, 90, 22, 14, 0, 1, 1, "100", string.Empty, this.Model.ElementID, string.Empty, false, "9B71C576-8245-4E9D-A536-4030527BE4D5", "9B71C576-8245-4E9D-A536-4030527BE4D5", "C04A111C-0212-463D-9D35-83B63255F617");
            this.btnColumnSettings.Text = "列设置";
            WebControl columnSettingsWeb = this.btnColumnSettings as WebControl;
            if (columnSettingsWeb != null)
            {
                columnSettingsWeb.Style["width"] = "90px";
                columnSettingsWeb.Style["min-width"] = "90px";
                columnSettingsWeb.Attributes["value"] = "列设置";
                columnSettingsWeb.Attributes["title"] = "列设置";
            }
            UIControlBuilder.BuilderUFControl(this.btnColumnSettings, "14");
            this.SetColumnSettingsButtonClientClick();
            container.Controls.Add(this.filterCard);
        }

        private void SetColumnSettingsButtonClientClick()
        {
            const string script = "if(window.ShortageTableShowColumnSettings){window.ShortageTableShowColumnSettings();}return false;";
            WebControl webControl = this.btnColumnSettings as WebControl;
            if (webControl != null) webControl.Attributes["onclick"] = script;
            SetPropertyIfExists(this.btnColumnSettings, "OnClientClick", script);
            SetPropertyIfExists(this.btnColumnSettings, "ClientClick", script);
        }
        private IUFLabel AddLabel(IUFContainer container, string id, string text, int column, int row, string resourceId, string controlId)
        {
            IUFLabel label = UIControlBuilder.BuilderUFLabel(container, id, string.Empty, "True", "True", "Right", 90, 20, column, row, 1, 1, "100", resourceId, controlId);
            label.Text = text;
            UIControlBuilder.BuilderUFControl(label, BuildTabIndex(column, row));
            return label;
        }

        private IUFFldTextBox AddTextBox(IUFContainer container, string id, string labelId, int column, int row, string labelResourceId, string controlId)
        {
            IUFFldTextBox textBox = UIControlBuilder.BuilderTextBox(container, id, "True", "True", "True", "False", "Left", 0, 60, 0, 140, 20, column, row, 1, 1, "False", "100", string.Empty, TextBoxMode.SingleLine, TextAlign.Left, true, false, labelId, string.Empty, "100", labelResourceId, controlId);
            UIControlBuilder.BuilderUFControl(textBox, BuildTabIndex(column, row));
            return textBox;
        }

        private IUFFldDatePicker AddDatePicker(IUFContainer container, string id, string labelId, int column, int row, string labelResourceId, string controlId)
        {
            IUFFldDatePicker datePicker = UIControlBuilder.BuilderDatePicker(container, id, true, true, true, "Date", "Left", 3, 60, 0, 140, 20, column, row, 1, 1, "100", true, false, labelId, labelResourceId, controlId);
            UIControlBuilder.BuilderUFControl(datePicker, BuildTabIndex(column, row));
            return datePicker;
        }

        private static string BuildTabIndex(int column, int row) { return ((row * 20) + column + 1).ToString(); }

        private void BuildGridCard(IUFContainer container)
        {
            this.gridCard = UIControlBuilder.BuildCard(container, "CardGrid", false, "none", true, true, "2", string.Empty, "F060A794-3664-414D-8C04-E6DF567FEF7B");
            CommonBuilder.GridLayoutPropBuilder(container, this.gridCard, 1580, 540, 0, 1, 1, 1, "100");
            CommonBuilder.ContainerGridLayoutPropBuilder(this.gridCard, 1, 1, 0, 5, 0, 0, 0, 0);
            this.InitViewBindingContainer(this, this.gridCard, null, string.Empty, string.Empty, null, 1, string.Empty);
            UIControlBuilder.BuildContainerGridLayout(this.gridCard, 5u, new GridColumnDef[] { new GridColumnDef(new Unit(GridViewportWidth), true) }, new GridRowDef[] { new GridRowDef(new Unit(530), false) });
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
            string script = @"(function(){var MINW=" + GridContentWidth.ToString(CultureInfo.InvariantCulture) + @";var barId='OutsourcePRPrepareGridHScroll';var syncing=false;function grid(){var g=document.getElementById('DataGrid0');if(g&&g.getElementsByTagName&&g.getElementsByTagName('table').length)return g;var ns=document.querySelectorAll('[id*=DataGrid0]');for(var i=0;i<ns.length;i++){var n=ns[i];if(n.tagName==='INPUT'||n.tagName==='TEXTAREA')continue;if(n.getElementsByTagName&&n.getElementsByTagName('table').length&&n.offsetWidth>200)return n;}return null;}function textOf(n){return(n.innerText||n.value||n.textContent||'').replace(/\s/g,'');}function hideBtns(g){var ns=g.querySelectorAll('button,input[type=button],input[type=submit],a');for(var k=0;k<ns.length;k++){var t=textOf(ns[k]);if(t==='新增'||t==='插入'||t==='删除'){ns[k].style.display='none';}}}function minW(){try{if(window.ShortageTableVisibleWidth)return Math.max(320,window.ShortageTableVisibleWidth());}catch(e){}return MINW;}function contentWidth(g){var w=Math.max(minW(),g.scrollWidth,g.offsetWidth);var ts=g.getElementsByTagName('table');for(var i=0;i<ts.length;i++){w=Math.max(w,ts[i].scrollWidth,ts[i].offsetWidth);}return w;}function viewOf(g,w){var p=g.parentElement;for(var i=0;i<12&&p;i++,p=p.parentElement){if(p.clientWidth>300&&p.clientWidth<w&&p.clientHeight>100)return p;}return g.parentElement;}function add(arr,n){if(!n)return;if(arr.indexOf(n)<0)arr.push(n);}function scrollNodes(g,v){var arr=[];var p=g.parentElement;for(var i=0;i<12&&p;i++,p=p.parentElement){if(p.clientWidth>200&&p.scrollWidth>p.clientWidth+2)add(arr,p);}if(v){var ds=v.getElementsByTagName('div');for(var j=0;j<ds.length;j++){if(ds[j].clientWidth>200&&ds[j].scrollWidth>ds[j].clientWidth+2)add(arr,ds[j]);}}add(arr,v);return arr;}function ensureBar(){var b=document.getElementById(barId);if(!b){b=document.createElement('div');b.id=barId;var inn=document.createElement('div');inn.style.height='1px';b.appendChild(inn);document.body.appendChild(b);}b.style.position='fixed';b.style.height='18px';b.style.overflowX='scroll';b.style.overflowY='hidden';b.style.zIndex='2147483647';b.style.background='#f2f2f2';b.style.borderTop='1px solid #c8c8c8';b.style.display='block';return b;}function place(b,v,w){var r=v.getBoundingClientRect();var left=Math.max(0,r.left);var width=Math.min(window.innerWidth-left-8,Math.max(320,r.width));b.style.left=left+'px';b.style.bottom='0px';b.style.width=width+'px';if(b.firstChild)b.firstChild.style.width=w+'px';}function apply(){var g=grid();if(!g)return;hideBtns(g);g.style.transform='';g.style.marginLeft='';g.style.minWidth=minW()+'px';g.style.maxWidth='none';var w=contentWidth(g);var v=viewOf(g,w);if(!v)return;v.style.overflowX='auto';v.style.overflowY='hidden';v.style.maxWidth='100%';var b=ensureBar();place(b,v,w);var nodes=scrollNodes(g,v);var syncToGrid=function(){if(syncing)return;syncing=true;var x=b.scrollLeft;for(var i=0;i<nodes.length;i++){try{nodes[i].scrollLeft=x;}catch(e){}}syncing=false;};b.onscroll=syncToGrid;for(var i=0;i<nodes.length;i++){nodes[i].onscroll=function(){if(syncing)return;syncing=true;b.scrollLeft=this.scrollLeft;syncing=false;};}syncToGrid();}function run(){apply();setTimeout(apply,300);setTimeout(apply,1000);setTimeout(apply,2500);}if(document.readyState==='loading'){document.addEventListener('DOMContentLoaded',run);}else{run();}window.ShortageTableRefreshScroll=apply;window.addEventListener('resize',apply);document.addEventListener('mouseup',function(){setTimeout(apply,80);setTimeout(apply,400);});if(window.Sys&&Sys.WebForms&&Sys.WebForms.PageRequestManager){Sys.WebForms.PageRequestManager.getInstance().add_endRequest(run);}})();";
            ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "OutsourcePRPrepareGridScroll", script, true);
        }
        private void RegisterColumnSettingsScript()
        {
            string hiddenId = this.hiddenVisibleColumns == null ? string.Empty : this.hiddenVisibleColumns.ClientID;
            string applyButtonId = this.applyColumnsButton == null ? string.Empty : this.applyColumnsButton.ClientID;
            string applyButtonTarget = this.applyColumnsButton == null ? string.Empty : this.applyColumnsButton.UniqueID;
            string storageKey = "ShortageTable.VisibleColumns." + this.GetCurrentUserKey();
            string script = @"(function(){var columns=" + BuildColumnSettingsJson() + @";var hiddenId='" + JsString(hiddenId) + @"';var applyId='" + JsString(applyButtonId) + @"';var applyTarget='" + JsString(applyButtonTarget) + @"';var storageKey='" + JsString(storageKey) + @"';var maskId='OutsourcePRPrepareColumnMask';var dialogId='OutsourcePRPrepareColumnDialog';function allFields(){var a=[];for(var i=0;i<columns.length;i++)a.push(columns[i].f);return a;}function split(v){if(!v)return[];var p=v.split(','),a=[];for(var i=0;i<p.length;i++){var s=p[i].replace(/^\s+|\s+$/g,'');if(s)a.push(s);}return a;}function map(a){var m={};for(var i=0;i<a.length;i++)m[a[i]]=true;return m;}function valid(a){var cm=map(allFields()),r=[];for(var i=0;i<a.length;i++){if(cm[a[i]]&&r.indexOf(a[i])<0)r.push(a[i]);}return r;}function same(a,b){if(a.length!==b.length)return false;for(var i=0;i<a.length;i++){if(a[i]!==b[i])return false;}return true;}function saved(){var v='';try{v=localStorage.getItem(storageKey)||'';}catch(e){}var a=valid(split(v));return a.length?a:allFields();}function hidden(){return document.getElementById(hiddenId);}function setHidden(a){var h=hidden();if(h)h.value=a.join(',');}function requestApply(a){setHidden(a);if(applyTarget&&typeof window.__doPostBack==='function'){window.__doPostBack(applyTarget,'');return;}var b=document.getElementById(applyId);if(b&&typeof b.click==='function')b.click();}function close(){var m=document.getElementById(maskId),d=document.getElementById(dialogId);if(m)m.style.display='none';if(d)d.style.display='none';}function ensure(){var mask=document.getElementById(maskId),dlg=document.getElementById(dialogId);if(mask&&dlg)return dlg;mask=document.createElement('div');mask.id=maskId;mask.style.cssText='display:none;position:fixed;left:0;top:0;right:0;bottom:0;background:rgba(0,0,0,.18);z-index:2147483646;';document.body.appendChild(mask);dlg=document.createElement('div');dlg.id=dialogId;dlg.style.cssText='display:none;position:fixed;left:50%;top:50%;transform:translate(-50%,-50%);width:520px;max-width:calc(100vw - 32px);background:#fff;border:1px solid #cfcfcf;box-shadow:0 8px 26px rgba(0,0,0,.18);z-index:2147483647;font-size:14px;color:#333;';dlg.innerHTML='<div style=""height:42px;line-height:42px;padding:0 16px;border-bottom:1px solid #e5e5e5;font-weight:600;"">列设置</div><div id=""oprColumnList"" style=""padding:12px 16px;display:grid;grid-template-columns:repeat(3,1fr);gap:10px 12px;max-height:320px;overflow:auto;""></div><div style=""padding:10px 16px;border-top:1px solid #e5e5e5;text-align:right;""><button type=""button"" id=""oprColumnAll"" style=""margin-right:8px;min-width:72px;height:28px;"">全选</button><button type=""button"" id=""oprColumnReset"" style=""margin-right:8px;min-width:72px;height:28px;"">恢复默认</button><button type=""button"" id=""oprColumnCancel"" style=""margin-right:8px;min-width:72px;height:28px;"">取消</button><button type=""button"" id=""oprColumnSave"" style=""min-width:72px;height:28px;background:#18b681;color:#fff;border:0;"">保存</button></div>';document.body.appendChild(dlg);mask.onclick=close;document.getElementById('oprColumnCancel').onclick=close;document.getElementById('oprColumnAll').onclick=function(){var xs=dlg.querySelectorAll('input[type=checkbox]');for(var i=0;i<xs.length;i++)xs[i].checked=true;};document.getElementById('oprColumnReset').onclick=function(){try{localStorage.removeItem(storageKey);}catch(e){}close();requestApply(allFields());};document.getElementById('oprColumnSave').onclick=function(){var xs=dlg.querySelectorAll('input[type=checkbox]'),a=[];for(var i=0;i<xs.length;i++){if(xs[i].checked)a.push(xs[i].value);}if(!a.length){alert('请至少保留一列');return;}try{if(same(a,allFields()))localStorage.removeItem(storageKey);else localStorage.setItem(storageKey,a.join(','));}catch(e){}close();requestApply(a);};return dlg;}window.ShortageTableShowColumnSettings=function(){var d=ensure(),list=document.getElementById('oprColumnList'),vis=map(saved());list.innerHTML='';for(var i=0;i<columns.length;i++){var lab=document.createElement('label');lab.style.cssText='display:flex;align-items:center;gap:6px;white-space:nowrap;';var cb=document.createElement('input');cb.type='checkbox';cb.value=columns[i].f;cb.checked=!!vis[columns[i].f];lab.appendChild(cb);lab.appendChild(document.createTextNode(columns[i].t));list.appendChild(lab);}document.getElementById(maskId).style.display='block';d.style.display='block';};function run(){var a=saved(),h=hidden(),posted=valid(split(h?h.value:''));setHidden(a);if(posted.length===0&&!same(a,allFields()))setTimeout(function(){requestApply(a);},0);}if(document.readyState==='loading'){document.addEventListener('DOMContentLoaded',run);}else{run();}if(window.Sys&&Sys.WebForms&&Sys.WebForms.PageRequestManager){Sys.WebForms.PageRequestManager.getInstance().add_endRequest(run);}})();";
            ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "OutsourcePRPrepareColumnSettings", script, true);
        }
        private void RegisterOutputCompletionScript()
        {
            string outputId = this.btnOutput == null ? string.Empty : ((Control)this.btnOutput).ClientID;
            string script = @"(function(){var outputId='" + JsString(outputId) + @"';var frameId='OutsourcePRPrepareExportFrame';function textOf(n){return(n.innerText||n.value||n.textContent||'').replace(/^\s+|\s+$/g,'');}function clearMask(){var ns=document.querySelectorAll('.x-mask,.x-mask-msg,.ext-el-mask,.uf-loading,.uf-mask,[id*=loading],[id*=Loading],[id*=mask],[id*=Mask]');for(var i=0;i<ns.length;i++){if(ns[i].id==='OutsourcePRPrepareColumnMask')continue;ns[i].style.display='none';}}function frame(){var f=document.getElementById(frameId);if(!f){f=document.createElement('iframe');f.id=frameId;f.name=frameId;f.style.display='none';document.body.appendChild(f);}return f;}function outputButton(n){for(var i=0;i<5&&n;i++,n=n.parentElement){if(n.id===outputId||textOf(n)==='输出')return n;}return null;}function prepare(e){var b=outputButton(e.target);if(!b)return;var form=b.form||document.forms[0];if(!form)return;frame();var oldTarget=form.target||'';form.target=frameId;setTimeout(clearMask,100);setTimeout(clearMask,500);setTimeout(clearMask,1500);setTimeout(clearMask,4000);setTimeout(function(){if(form.target===frameId)form.target=oldTarget;},2000);}function bind(){if(document._oprOutputBound)return;document._oprOutputBound=true;document.addEventListener('click',prepare,true);}if(document.readyState==='loading'){document.addEventListener('DOMContentLoaded',bind);}else{bind();}if(window.Sys&&Sys.WebForms&&Sys.WebForms.PageRequestManager){Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function(){document._oprOutputBound=false;bind();});}})();";
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
            this.InitViewBindingContainer(this, this.dataGrid, this.Model.ShortageTable, "ShortageTable", string.Empty, null, 25, string.Empty);
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

            this.AddHiddenNumberColumn("ID0", this.Model.ShortageTable.FieldID, "ID", "3B67284C-C2E3-4650-9BA8-3E10BF0871B8");
            for (int i = 0; i < ListColumns.Length; i++)
            {
                this.AddListColumn(ListColumns[i]);
            }
        }


        private void AddListColumn(ListColumn column)
        {
            IUIField field = this.Model.ShortageTable.GetField(column.FieldName);
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
                case "ID": return "47EFAE85-EEA2-4096-B339-05580797A865";
                case "ProductionOrder": return "C5B1EA92-9699-4FFA-B214-60DBA137CE2F";
                case "MaterialCode": return "09A0A048-EFBA-4216-969E-10878968CE1C";
                case "MaterialName": return "D1635DFA-2156-47AE-9EAB-8484FDC1CB97";
                case "DemandQty": return "F89E24E8-091D-4018-8417-3ECE022EBB5B";
                case "PlanStartDate": return "94FB779A-0351-4BDA-9B65-E7B73EC87BFB";
                case "ReqDate": return "E8956492-BF0E-490D-AEE7-27F4C3D33F87";
                case "ScarceQty": return "666008E6-0DF0-4EC3-A05C-42B4FC1178AE";
                case "SupplyType": return "A6EE5BE1-2598-413E-95A0-96005B05026B";
                case "BusinessPerson": return "C2B3C0AF-2B9E-4B84-AC4B-2D6F3C1D4C01";
                case "SupplyNo": return "204433DC-4A28-4D11-8345-BFD651A3B7B2";
                case "SupplyLineNo": return "E05A5D22-46C5-4A4A-AB46-2A1F0DD5E9A1";
                case "SupplyDate": return "A49BA199-0863-41C0-B2B9-4C1D6DC59443";
                case "MatchedQty": return "93F71466-D9C1-4D7A-BFCE-AA5EF37342D8";
                case "RemainingShortage": return "C34435DD-3F59-474F-B648-05008A56665D";
                default: return fieldName;
            }
        }

        private void EventBind()
        {
            this.btnQuery.Click += this.BtnQuery_Click;
            this.btnClear.Click += this.BtnClear_Click;
            this.btnOutput.Click += this.BtnOutput_Click;
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
            this.Model.ShortageTable.Clear();
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

        private void RegisterPostBackForOutput()
        {
            ScriptManager scriptManager = this.Page == null ? null : ScriptManager.GetCurrent(this.Page);
            Control outputControl = this.btnOutput as Control;
            if (scriptManager != null && outputControl != null)
            {
                scriptManager.RegisterPostBackControl(outputControl);
            }
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
                this.Model.ShortageTable.Clear();
                DataTable table = this.BuildMatchedTable();
                foreach (DataRow row in table.Rows)
                {
                    ShortageTableRecord record = this.Model.ShortageTable.AddNewUIRecord();
                    record.ID = GetLong(row, "ID");
                    record.ProductionOrder = GetString(row, "ProductionOrder");
                    record.MaterialCode = GetString(row, "MaterialCode");
                    record.MaterialName = GetString(row, "MaterialName");
                    record.DemandQty = GetNullableDouble(row, "DemandQty");
                    record.PlanStartDate = GetNullableDateTime(row, "PlanStartDate");
                    record.ReqDate = GetNullableDateTime(row, "ReqDate");
                    record.ScarceQty = GetNullableDouble(row, "ScarceQty");
                    record.SupplyType = GetString(row, "SupplyType");
                    record.BusinessPerson = GetString(row, "BusinessPerson");
                    record.SupplyNo = GetString(row, "SupplyNo");
                    record.SupplyLineNo = GetString(row, "SupplyLineNo");
                    record.SupplyDate = GetNullableDateTime(row, "SupplyDate");
                    record.MatchedQty = GetNullableDouble(row, "MatchedQty");
                    record.RemainingShortage = GetNullableDouble(row, "RemainingShortage");
                }
                if (this.Model.ShortageTable.Records.Count > 0) this.Model.ShortageTable.FocusedIndex = 0;
            }
            catch (Exception ex)
            {
                this.Model.ShortageTable.Clear();
                this.Model.ErrorMessage.Message = "缺料表查询失败：" + ex.Message;
            }
        }

        private string BuildQuerySql(out List<SqlParameter> parameters)
        {
            parameters = new List<SqlParameter>();
            string code = GetText(this.txtDocNo);
            List<string> filters = new List<string>();
            StringBuilder sql = new StringBuilder(@"
SELECT dp.ID AS DemandID, dp.ItemMaster AS ItemID, dp.DocNo AS ProductionOrder,
       dp.ItemCode AS MaterialCode, dp.ItemName AS MaterialName,
       dp.ReqNumIssueUOMQty AS DemandQty, dp.ScarceQty, dp.ReqDate, w.PlanStartDate
FROM dbo.MO_SimuDoc d WITH(NOLOCK)
JOIN dbo.MO_ItemWIPSimu w WITH(NOLOCK) ON w.SimuDoc = d.ID
JOIN dbo.MO_SimuDemandPick dp WITH(NOLOCK) ON dp.ItemWIPSimu = w.ID
WHERE d.Code = @Code AND d.SimuDocOrg = @Org AND dp.ScarceQty > 0");
            parameters.Add(new SqlParameter("@Code", SqlDbType.NVarChar, 100) { Value = code.Trim() });
            parameters.Add(new SqlParameter("@Org", SqlDbType.BigInt) { Value = GetCurrentOrgID() });
            AddLikeFilter(filters, parameters, "dp.ItemCode", "@MaterialCode", GetText(this.txtMaterialCode));
            AddLikeFilter(filters, parameters, "dp.DocNo", "@ProductionOrder", GetText(this.txtProductionOrder));
            for (int i = 0; i < filters.Count; i++) sql.Append(" AND ").Append(filters[i]);
            sql.Append(" ORDER BY COALESCE(w.PlanStartDate, '9999-12-31'), dp.ID");
            return sql.ToString();
        }

        private DataTable BuildMatchedTable()
        {
            List<SqlParameter> demandParameters;
            DataTable demandTable = SqlHelp.RunSqlDataTable(BuildQuerySql(out demandParameters), demandParameters.ToArray());
            long org = GetCurrentOrgID();
            DataTable poTable = SqlHelp.RunSqlDataTable(@"
SELECT l.ID, l.ItemInfo_ItemID AS ItemID, l.ItemInfo_ItemCode AS MaterialCode,
       l.DeficiencyQtyTU, l.DeliveryDate, l.PlanArriveDate,
       poh.DocNo AS PurchaseOrderNo, pol.DocLineNo AS PurchaseOrderLineNo,
       ISNULL(poperl.Name, N'') AS BusinessPerson
FROM dbo.PM_POShipLine l WITH(NOLOCK)
LEFT JOIN dbo.PM_POLine pol WITH(NOLOCK) ON pol.ID = l.POLine
LEFT JOIN dbo.PM_PurchaseOrder poh WITH(NOLOCK) ON poh.ID = pol.PurchaseOrder
LEFT JOIN dbo.CBO_Operators poper WITH(NOLOCK) ON poper.ID = poh.PurOper AND poper.Org = poh.Org
LEFT JOIN dbo.CBO_Operators_Trl poperl WITH(NOLOCK) ON poperl.ID = poper.ID AND poperl.SysMLFlag = N'zh-CN'
WHERE l.CurrentOrg = @Org AND pol.CurrentOrg = @Org AND pol.Status IN (0,1,2) AND l.DeficiencyQtyTU > 0
ORDER BY COALESCE(l.DeliveryDate, l.PlanArriveDate, '9999-12-31'), l.ID",
                new SqlParameter("@Org", SqlDbType.BigInt) { Value = org });
            DataTable receiptTable = SqlHelp.RunSqlDataTable(@"
SELECT r.ID, r.ItemInfo_ItemID AS ItemID, r.ItemInfo_ItemCode AS MaterialCode,
       r.RcvQtyTU, r.ArriveQtyTU, r.PlanQtyTU,
       rcv.DocNo AS ReceiptNo, r.DocLineNo AS ReceiptLineNo,
       rcv.BusinessDate AS ReceiptDate
FROM dbo.PM_RcvLine r WITH(NOLOCK)
LEFT JOIN dbo.PM_Receivement rcv WITH(NOLOCK) ON rcv.ID = r.Receivement
WHERE r.CurrentOrg = @Org AND r.Status <> 5 AND r.SplitFlag <> 1
ORDER BY COALESCE(r.SrcDoc_SrcDocDate, '9999-12-31'), r.ID",
                new SqlParameter("@Org", SqlDbType.BigInt) { Value = org });

            DataTable moTable = SqlHelp.RunSqlDataTable(@"
SELECT m.ID, m.ItemMaster AS ItemID,
       m.ProductQty, m.TotalRcvQty, m.CompleteDate, m.DocNo AS MONo,
       ISNULL(moperl.Name, N'') AS BusinessPerson
FROM dbo.MO_MO m WITH(NOLOCK)
LEFT JOIN dbo.CBO_Operators moper WITH(NOLOCK) ON moper.ID = m.BusinessPerson AND moper.Org = m.Org
LEFT JOIN dbo.CBO_Operators_Trl moperl WITH(NOLOCK) ON moperl.ID = moper.ID AND moperl.SysMLFlag = N'zh-CN'
WHERE m.Org = @Org
  AND m.DocState <> 3
  AND m.IsCancel <> 1
  AND m.IsHoldRelease <> 1
  AND ISNULL(m.ProductQty, 0) - ISNULL(m.TotalRcvQty, 0) > 0
ORDER BY COALESCE(m.CompleteDate, '9999-12-31'), m.ID",
                new SqlParameter("@Org", SqlDbType.BigInt) { Value = org });

            DataTable result = new DataTable();
            string[] columns = { "ID", "ProductionOrder", "MaterialCode", "MaterialName", "DemandQty", "PlanStartDate", "ReqDate", "ScarceQty", "SupplyType", "BusinessPerson", "SupplyNo", "SupplyLineNo", "SupplyDate", "MatchedQty", "RemainingShortage" };
            Type[] types = { typeof(long), typeof(string), typeof(string), typeof(string), typeof(double), typeof(DateTime), typeof(DateTime), typeof(double), typeof(string), typeof(string), typeof(string), typeof(string), typeof(DateTime), typeof(double), typeof(double) };
            for (int i = 0; i < columns.Length; i++) result.Columns.Add(columns[i], types[i]);
            Dictionary<long, decimal> poRemaining = new Dictionary<long, decimal>(); foreach (DataRow r in poTable.Rows) poRemaining[GetLong(r, "ID")] = GetDecimal(r, "DeficiencyQtyTU");
            Dictionary<long, decimal> receiptRemaining = new Dictionary<long, decimal>(); foreach (DataRow r in receiptTable.Rows) receiptRemaining[GetLong(r, "ID")] = ReceiptAvailableQty(r);
            Dictionary<long, decimal> moRemaining = new Dictionary<long, decimal>(); foreach (DataRow r in moTable.Rows) moRemaining[GetLong(r, "ID")] = Math.Max(0m, GetDecimal(r, "ProductQty") - GetDecimal(r, "TotalRcvQty"));
            long rowId = 0;
            foreach (DataRow demand in demandTable.Rows)
            {
                long itemId = GetLong(demand, "ItemID");
                string itemCode = GetString(demand, "MaterialCode");
                decimal remaining = GetDecimal(demand, "ScarceQty");
                decimal original = remaining;
                foreach (DataRow po in poTable.Rows)
                {
                    if (!SameItem(GetLong(po, "ItemID"), GetString(po, "MaterialCode"), itemId, itemCode)) continue;
                    decimal matched = Math.Min(remaining, poRemaining[GetLong(po, "ID")]);
                    if (matched <= 0m) continue;
                    remaining -= matched; poRemaining[GetLong(po, "ID")] -= matched;
                    AddResultRow(result, ref rowId, demand, "采购订单", GetString(po, "BusinessPerson"), GetString(po, "PurchaseOrderNo"), GetString(po, "PurchaseOrderLineNo"), GetDateValue(po, "DeliveryDate", "PlanArriveDate"), matched, remaining);
                    if (remaining <= 0m) break;
                }
                if (remaining > 0m)
                {
                    foreach (DataRow receipt in receiptTable.Rows)
                    {
                        if (!SameItem(GetLong(receipt, "ItemID"), GetString(receipt, "MaterialCode"), itemId, itemCode)) continue;
                        decimal matched = Math.Min(remaining, receiptRemaining[GetLong(receipt, "ID")]);
                        if (matched <= 0m) continue;
                        remaining -= matched; receiptRemaining[GetLong(receipt, "ID")] -= matched;
                        AddResultRow(result, ref rowId, demand, "采购收货", string.Empty, GetString(receipt, "ReceiptNo"), GetString(receipt, "ReceiptLineNo"), GetDateValue(receipt, "ReceiptDate"), matched, remaining);
                        if (remaining <= 0m) break;
                    }
                }
                if (remaining > 0m)
                {
                    foreach (DataRow mo in moTable.Rows)
                    {
                        if (GetLong(mo, "ItemID") != itemId) continue;
                        decimal matched = Math.Min(remaining, moRemaining[GetLong(mo, "ID")]);
                        if (matched <= 0m) continue;
                        remaining -= matched;
                        moRemaining[GetLong(mo, "ID")] -= matched;
                        AddResultRow(result, ref rowId, demand, "生产订单", GetString(mo, "BusinessPerson"), GetString(mo, "MONo"), string.Empty, GetDateValue(mo, "CompleteDate"), matched, remaining);
                        if (remaining <= 0m) break;
                    }
                }
                if (remaining > 0m) AddResultRow(result, ref rowId, demand, "未匹配", string.Empty, string.Empty, string.Empty, null, 0m, remaining);
            }
            return this.ApplyResultFilters(result);
        }

        private static void AddResultRow(DataTable table, ref long rowId, DataRow demand, string type, string businessPerson, string no, string lineNo, DateTime? date, decimal matched, decimal remaining)
        {
            DataRow row = table.NewRow(); row["ID"] = ++rowId; row["ProductionOrder"] = GetString(demand, "ProductionOrder"); row["MaterialCode"] = GetString(demand, "MaterialCode"); row["MaterialName"] = GetString(demand, "MaterialName"); row["DemandQty"] = Convert.ToDouble(GetDecimal(demand, "DemandQty")); row["PlanStartDate"] = DbDate(demand, "PlanStartDate"); row["ReqDate"] = DbDate(demand, "ReqDate"); row["ScarceQty"] = Convert.ToDouble(GetDecimal(demand, "ScarceQty")); row["SupplyType"] = type; row["BusinessPerson"] = businessPerson ?? string.Empty; row["SupplyNo"] = no ?? string.Empty; row["SupplyLineNo"] = lineNo ?? string.Empty; row["SupplyDate"] = date.HasValue ? (object)date.Value : DBNull.Value; row["MatchedQty"] = Convert.ToDouble(matched); row["RemainingShortage"] = Convert.ToDouble(remaining); table.Rows.Add(row);
        }

        private static bool SameItem(long id1, string code1, long id2, string code2) { return (id1 > 0 && id1 == id2) || (!string.IsNullOrEmpty(code1) && string.Equals(code1, code2, StringComparison.OrdinalIgnoreCase)); }
        private DataTable ApplyResultFilters(DataTable source)
        {
            string supplyType = GetText(this.txtSupplyType);
            string supplyNo = GetText(this.txtSupplyNo);
            DateTime? fromDate = GetDate(this.dtFromDate);
            DateTime? toDate = GetDate(this.dtToDate);
            if (string.IsNullOrWhiteSpace(supplyType) && string.IsNullOrWhiteSpace(supplyNo) && !fromDate.HasValue && !toDate.HasValue) return source;

            DataTable filtered = source.Clone();
            foreach (DataRow row in source.Rows)
            {
                if (!string.IsNullOrWhiteSpace(supplyType) && !ContainsText(GetString(row, "SupplyType"), supplyType)) continue;
                if (!string.IsNullOrWhiteSpace(supplyNo) && !ContainsText(GetString(row, "SupplyNo"), supplyNo)) continue;
                DateTime? supplyDate = GetNullableDateTime(row, "SupplyDate");
                if (fromDate.HasValue && (!supplyDate.HasValue || supplyDate.Value.Date < fromDate.Value.Date)) continue;
                if (toDate.HasValue && (!supplyDate.HasValue || supplyDate.Value.Date > toDate.Value.Date)) continue;
                filtered.ImportRow(row);
            }
            return filtered;
        }

        private static bool ContainsText(string value, string search)
        {
            return value != null && search != null && value.IndexOf(search.Trim(), StringComparison.OrdinalIgnoreCase) >= 0;
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

            string fileName = "齐套分析缺料表_" + DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture) + ".xls";
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
            this.txtDocNo.Text = string.Empty;
            this.txtMaterialCode.Text = string.Empty;
            this.txtSupplyNo.Text = string.Empty;
            this.txtSupplyType.Text = string.Empty;
            this.txtProductionOrder.Text = string.Empty;
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
