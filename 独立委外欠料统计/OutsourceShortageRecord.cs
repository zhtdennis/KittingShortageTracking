using System;
using UFSoft.UBF.UI.MD.Runtime;
using UFSoft.UBF.UI.MD.Runtime.Implement;

namespace U9Custom.UI.OutsourceShortageStatistics.Independent
{
    [Serializable]
    public class OutsourceShortageRecord : UIRecord
    {
        public OutsourceShortageRecord(IUIRecordBuilder builder) : base(builder) { }
        protected override IUIRecord CreateCloneInstance(IUIRecordBuilder builder) { return new OutsourceShortageRecord(builder); }
        private OutsourceShortageView View { get { return (OutsourceShortageView)base.ContainerView; } }
        private T Get<T>(string name) { return base.GetValue<T>(View.GetField(name)); }
        private void Set(string name, object value) { base[View.GetField(name)] = value; }
        public long ID { get { return Get<long>("ID"); } set { Set("ID", value); } }
        public string SupplierCode { get { return Get<string>("SupplierCode"); } set { Set("SupplierCode", value); } }
        public string SupplierName { get { return Get<string>("SupplierName"); } set { Set("SupplierName", value); } }
        public string PurchaseOrderNo { get { return Get<string>("PurchaseOrderNo"); } set { Set("PurchaseOrderNo", value); } }
        public long? POLineNo { get { return Get<long?>("POLineNo"); } set { Set("POLineNo", value); } }
        public long? PickLineNo { get { return Get<long?>("PickLineNo"); } set { Set("PickLineNo", value); } }
        public DateTime? BusinessDate { get { return Get<DateTime?>("BusinessDate"); } set { Set("BusinessDate", value); } }
        public string MaterialCode { get { return Get<string>("MaterialCode"); } set { Set("MaterialCode", value); } }
        public string MaterialName { get { return Get<string>("MaterialName"); } set { Set("MaterialName", value); } }
        public string SupplyWhCode { get { return Get<string>("SupplyWhCode"); } set { Set("SupplyWhCode", value); } }
        public string SupplyWhName { get { return Get<string>("SupplyWhName"); } set { Set("SupplyWhName", value); } }
        public double? ActualReqQty { get { return Get<double?>("ActualReqQty"); } set { Set("ActualReqQty", value); } }
        public double? IssuedQty { get { return Get<double?>("IssuedQty"); } set { Set("IssuedQty", value); } }
        public double? UnissuedQty { get { return Get<double?>("UnissuedQty"); } set { Set("UnissuedQty", value); } }
        public double? InventoryDeductQty { get { return Get<double?>("InventoryDeductQty"); } set { Set("InventoryDeductQty", value); } }
        public double? ShortageQty { get { return Get<double?>("ShortageQty"); } set { Set("ShortageQty", value); } }
        public long? ItemID { get { return Get<long?>("ItemID"); } set { Set("ItemID", value); } }
        public long? IssueUOMID { get { return Get<long?>("IssueUOMID"); } set { Set("IssueUOMID", value); } }
        public long? SupplyWhID { get { return Get<long?>("SupplyWhID"); } set { Set("SupplyWhID", value); } }
    }
}
