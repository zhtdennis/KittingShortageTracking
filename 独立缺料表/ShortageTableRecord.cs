using System;
using UFSoft.UBF.UI.MD.Runtime;
using UFSoft.UBF.UI.MD.Runtime.Implement;
namespace U9Custom.UI.ManufactureSimulateShortageTable.Independent
{
    [Serializable]
    public class ShortageTableRecord : UIRecord
    {
        public ShortageTableRecord(IUIRecordBuilder builder) : base(builder) { }
        private ShortageTableView View { get { return (ShortageTableView)base.ContainerView; } }
        protected override IUIRecord CreateCloneInstance(IUIRecordBuilder builder) { return new ShortageTableRecord(builder); }
        public long ID { get { return base.GetValue<long>(View.FieldID); } set { base[View.FieldID] = value; } }
        public string ProductionOrder { get { return base.GetValue<string>(View.FieldProductionOrder); } set { base[View.FieldProductionOrder] = value; } }
        public string MaterialCode { get { return base.GetValue<string>(View.FieldMaterialCode); } set { base[View.FieldMaterialCode] = value; } }
        public string MaterialName { get { return base.GetValue<string>(View.FieldMaterialName); } set { base[View.FieldMaterialName] = value; } }
        public double? DemandQty { get { return base.GetValue<double?>(View.FieldDemandQty); } set { base[View.FieldDemandQty] = value; } }
        public DateTime? PlanStartDate { get { return base.GetValue<DateTime?>(View.FieldPlanStartDate); } set { base[View.FieldPlanStartDate] = value; } }
        public DateTime? ReqDate { get { return base.GetValue<DateTime?>(View.FieldReqDate); } set { base[View.FieldReqDate] = value; } }
        public double? ScarceQty { get { return base.GetValue<double?>(View.FieldScarceQty); } set { base[View.FieldScarceQty] = value; } }
        public string SupplyType { get { return base.GetValue<string>(View.FieldSupplyType); } set { base[View.FieldSupplyType] = value; } }
        public string BusinessPerson { get { return base.GetValue<string>(View.FieldBusinessPerson); } set { base[View.FieldBusinessPerson] = value; } }
        public string SupplyNo { get { return base.GetValue<string>(View.FieldSupplyNo); } set { base[View.FieldSupplyNo] = value; } }
        public string SupplyLineNo { get { return base.GetValue<string>(View.FieldSupplyLineNo); } set { base[View.FieldSupplyLineNo] = value; } }
        public DateTime? SupplyDate { get { return base.GetValue<DateTime?>(View.FieldSupplyDate); } set { base[View.FieldSupplyDate] = value; } }
        public double? MatchedQty { get { return base.GetValue<double?>(View.FieldMatchedQty); } set { base[View.FieldMatchedQty] = value; } }
        public double? RemainingShortage { get { return base.GetValue<double?>(View.FieldRemainingShortage); } set { base[View.FieldRemainingShortage] = value; } }
    }
}
