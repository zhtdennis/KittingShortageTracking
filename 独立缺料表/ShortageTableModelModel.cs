using System;
using UFSoft.UBF.UI.MD.Runtime;
using UFSoft.UBF.UI.MD.Runtime.Implement;
namespace U9Custom.UI.ManufactureSimulateShortageTable.Independent
{
    [Serializable]
    public class ShortageTableModelModel : UIModel
    {
        private ShortageTableView viewShortageTable;
        public ShortageTableModelModel() : base("ShortageTableModel") { InitClass(); base.SetResourceInfo("06EA90C8-AC69-4CF0-8BB1-F12C7BE098E5"); try { AfterInitModel(); } catch (Exception ex) { IUIModel model = this; base.ErrorMessage.SetErrorMessage(ref model, ex); } }
        private ShortageTableModelModel(bool isInit) : base("ShortageTableModel") { }
        protected override IUIModel CreateCloneInstance() { return new ShortageTableModelModel(false); }
        public ShortageTableView ShortageTable { get { return (ShortageTableView)base["ShortageTable"]; } }
        private void InitClass() { viewShortageTable = new ShortageTableView(this); viewShortageTable.SetResourceInfo("A79676C5-BB1B-4E9D-891E-743581D457F7"); base.Views.Add(viewShortageTable); }
        public override string AssemblyName { get { return "U9Custom.UI.ManufactureSimulateShortageTable.Independent"; } }
        public override void AfterInitModel() { }
        public override void OnValidate() { base.OnValidate(); }
    }
}
