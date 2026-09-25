using System;
using UFSoft.UBF.UI.MD.Runtime;
using UFSoft.UBF.UI.MD.Runtime.Implement;

namespace U9Custom.UI.OutsourceShortageStatistics.Independent
{
    [Serializable]
    public class OutsourceShortageView : UIView
    {
        public OutsourceShortageView(IUIModel model) : base(model, "OutsourceShortage", string.Empty, true) { InitClass(); }
        private OutsourceShortageView() : base(null, "OutsourceShortage", string.Empty, true) { }
        protected override IUIView CreateCloneInstance() { return new OutsourceShortageView(); }
        public IUIField GetField(string name) { return base.Fields[name]; }

        private void InitClass()
        {
            Add("ID", typeof(long), "System.Int64", false, "22964AA7-035D-4B0C-9BA3-123B10A82F98");
            Add("SupplierCode", typeof(string), "System.String", true, "8BF251D3-0C47-4A36-9258-3AFBF91F51DF");
            Add("SupplierName", typeof(string), "System.String", true, "87096366-CCD5-418F-80EC-17AFBBB631BB");
            Add("PurchaseOrderNo", typeof(string), "System.String", true, "A860F530-3528-424C-843B-F25A09D06FBF");
            Add("POLineNo", typeof(long), "System.Int64", true, "86CCC448-9850-48A7-992B-01D8B293E56A");
            Add("PickLineNo", typeof(long), "System.Int64", true, "6EEEC7BA-40EA-450A-B932-2E238A1FEEA0");
            Add("BusinessDate", typeof(DateTime), "System.Date", true, "D44DA663-70DC-4351-B323-67360C8B6FF2");
            Add("MaterialCode", typeof(string), "System.String", true, "DC0367D6-B77A-4A23-8377-D736ECF7EE55");
            Add("MaterialName", typeof(string), "System.String", true, "730F79DD-099A-47E5-9E07-23F653ECCE4C");
            Add("SupplyWhCode", typeof(string), "System.String", true, "4CB6A7F1-34D7-4254-AA17-99DE9F914023");
            Add("SupplyWhName", typeof(string), "System.String", true, "82ECD707-514F-4369-B91A-D9A82D8A1A8B");
            Add("ActualReqQty", typeof(double), "System.Double", true, "98A53531-8206-4981-A38F-19BE18F7168D");
            Add("IssuedQty", typeof(double), "System.Double", true, "069172D1-EDE1-4D31-A130-88AD86EB8464");
            Add("UnissuedQty", typeof(double), "System.Double", true, "1A33C9A3-E636-4B73-AC6B-A4E06B3CDA66");
            Add("InventoryDeductQty", typeof(double), "System.Double", true, "AC09638C-D656-4C71-B5CD-1D485AF11E2A");
            Add("ShortageQty", typeof(double), "System.Double", true, "C81530E8-616E-4861-8797-365461539E46");
            Add("ItemID", typeof(long), "System.Int64", true, "FF9FCC74-0FE8-4A81-961D-32072DCC9FE2");
            Add("IssueUOMID", typeof(long), "System.Int64", true, "0C067673-BDFF-44F9-8A29-0ED142E6550D");
            Add("SupplyWhID", typeof(long), "System.Int64", true, "416E464C-BCA0-42A7-A12F-82CFB81B56D3");
        }

        private void Add(string name, Type type, string dataType, bool nullable, string uid)
        {
            string typeUid = dataType == "System.Int64" ? "ba391065-6c27-4c82-acc8-b52b1c93a910" : dataType == "System.Double" ? "a5242caa-f9ee-4159-b8c9-d0952a79175a" : dataType == "System.Date" ? "c9e6bc50-2e39-4f27-9519-da0c7859d37e" : "3d174255-fd12-47f7-8844-3b5e4fae9e8c";
            UIModelRuntimeFactory.AddNewUIField(this, name, type, nullable, "Misc." + name, dataType, string.Empty, true, true, false, string.Empty, false, UIFieldType.DirectField, typeUid, string.Empty, uid);
        }

        protected override IUIRecord BuildNewRecord(IUIRecordBuilder builder) { return new OutsourceShortageRecord(builder); }
        public new OutsourceShortageRecord FocusedRecord { get { return (OutsourceShortageRecord)base.FocusedRecord; } set { base.FocusedRecord = value; } }
        public new OutsourceShortageRecord AddNewUIRecord() { return (OutsourceShortageRecord)base.AddNewUIRecord(); }
    }
}
