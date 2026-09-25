using System;
using UFSoft.UBF.UI.MD.Runtime;
using UFSoft.UBF.UI.MD.Runtime.Implement;

namespace U9Custom.UI.ManufactureSimulateShortageTable.Independent
{
    [Serializable]
    public class ShortageTableView : UIView
    {
        public ShortageTableView(IUIModel model) : base(model, "ShortageTable", string.Empty, true) { InitClass(); }
        private ShortageTableView() : base(null, "ShortageTable", string.Empty, true) { }
        protected override IUIView CreateCloneInstance() { return new ShortageTableView(); }
        public IUIField FieldID { get { return base.Fields["ID"]; } }
        public IUIField FieldProductionOrder { get { return base.Fields["ProductionOrder"]; } }
        public IUIField FieldMaterialCode { get { return base.Fields["MaterialCode"]; } }
        public IUIField FieldMaterialName { get { return base.Fields["MaterialName"]; } }
        public IUIField FieldDemandQty { get { return base.Fields["DemandQty"]; } }
        public IUIField FieldPlanStartDate { get { return base.Fields["PlanStartDate"]; } }
        public IUIField FieldReqDate { get { return base.Fields["ReqDate"]; } }
        public IUIField FieldScarceQty { get { return base.Fields["ScarceQty"]; } }
        public IUIField FieldSupplyType { get { return base.Fields["SupplyType"]; } }
        public IUIField FieldBusinessPerson { get { return base.Fields["BusinessPerson"]; } }
        public IUIField FieldSupplyNo { get { return base.Fields["SupplyNo"]; } }
        public IUIField FieldSupplyLineNo { get { return base.Fields["SupplyLineNo"]; } }
        public IUIField FieldSupplyDate { get { return base.Fields["SupplyDate"]; } }
        public IUIField FieldMatchedQty { get { return base.Fields["MatchedQty"]; } }
        public IUIField FieldRemainingShortage { get { return base.Fields["RemainingShortage"]; } }
        public IUIField GetField(string fieldName) { return base.Fields[fieldName]; }
        private void InitClass()
        {
            AddField(this, "ID", typeof(long), "Key.ID", "System.Int64", false, "47EFAE85-EEA2-4096-B339-05580797A865");
            AddField(this, "ProductionOrder", typeof(string), "Misc.ProductionOrder", "System.String", true, "C5B1EA92-9699-4FFA-B214-60DBA137CE2F");
            AddField(this, "MaterialCode", typeof(string), "Misc.MaterialCode", "System.String", true, "09A0A048-EFBA-4216-969E-10878968CE1C");
            AddField(this, "MaterialName", typeof(string), "Misc.MaterialName", "System.String", true, "D1635DFA-2156-47AE-9EAB-8484FDC1CB97");
            AddField(this, "DemandQty", typeof(double), "Misc.DemandQty", "System.Double", true, "F89E24E8-091D-4018-8417-3ECE022EBB5B");
            AddField(this, "PlanStartDate", typeof(DateTime), "Misc.PlanStartDate", "System.Date", true, "94FB779A-0351-4BDA-9B65-E7B73EC87BFB");
            AddField(this, "ReqDate", typeof(DateTime), "Misc.ReqDate", "System.Date", true, "E8956492-BF0E-490D-AEE7-27F4C3D33F87");
            AddField(this, "ScarceQty", typeof(double), "Misc.ScarceQty", "System.Double", true, "666008E6-0DF0-4EC3-A05C-42B4FC1178AE");
            AddField(this, "SupplyType", typeof(string), "Misc.SupplyType", "System.String", true, "A6EE5BE1-2598-413E-95A0-96005B05026B");
            AddField(this, "BusinessPerson", typeof(string), "Misc.BusinessPerson", "System.String", true, "C2B3C0AF-2B9E-4B84-AC4B-2D6F3C1D4C01");
            AddField(this, "SupplyNo", typeof(string), "Misc.SupplyNo", "System.String", true, "204433DC-4A28-4D11-8345-BFD651A3B7B2");
            AddField(this, "SupplyLineNo", typeof(string), "Misc.SupplyLineNo", "System.String", true, "E05A5D22-46C5-4A4A-AB46-2A1F0DD5E9A1");
            AddField(this, "SupplyDate", typeof(DateTime), "Misc.SupplyDate", "System.Date", true, "A49BA199-0863-41C0-B2B9-4C1D6DC59443");
            AddField(this, "MatchedQty", typeof(double), "Misc.MatchedQty", "System.Double", true, "93F71466-D9C1-4D7A-BFCE-AA5EF37342D8");
            AddField(this, "RemainingShortage", typeof(double), "Misc.RemainingShortage", "System.Double", true, "C34435DD-3F59-474F-B648-05008A56665D");
        }
        private static void AddField(UIView view, string name, Type type, string tip, string dataType, bool nullable, string uid)
        {
            string typeUid = dataType == "System.Int64" ? "ba391065-6c27-4c82-acc8-b52b1c93a910" : dataType == "System.Double" ? "a5242caa-f9ee-4159-b8c9-d0952a79175a" : dataType == "System.Date" ? "c9e6bc50-2e39-4f27-9519-da0c7859d37e" : "3d174255-fd12-47f7-8844-3b5e4fae9e8c";
            UIModelRuntimeFactory.AddNewUIField(view, name, type, nullable, tip, dataType, string.Empty, true, true, false, string.Empty, false, UIFieldType.DirectField, typeUid, string.Empty, uid);
        }
        protected override IUIRecord BuildNewRecord(IUIRecordBuilder builder) { return new ShortageTableRecord(builder); }
        public new ShortageTableRecord FocusedRecord { get { return (ShortageTableRecord)base.FocusedRecord; } set { base.FocusedRecord = value; } }
        public new ShortageTableRecord AddNewUIRecord() { return (ShortageTableRecord)base.AddNewUIRecord(); }
    }
}
