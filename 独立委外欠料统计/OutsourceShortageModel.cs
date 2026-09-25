using System;
using UFSoft.UBF.UI.MD.Runtime;
using UFSoft.UBF.UI.MD.Runtime.Implement;
namespace U9Custom.UI.OutsourceShortageStatistics.Independent
{
    [Serializable]
    public class OutsourceShortageModel : UIModel
    {
        private OutsourceShortageView viewOutsourceShortage;
        public OutsourceShortageModel() : base("OutsourceShortageModel") { InitClass(); base.SetResourceInfo("19CD83B9-7624-4072-B422-AD5B99659A50"); try { AfterInitModel(); } catch (Exception ex) { IUIModel model = this; base.ErrorMessage.SetErrorMessage(ref model, ex); } }
        private OutsourceShortageModel(bool isInit) : base("OutsourceShortageModel") { }
        protected override IUIModel CreateCloneInstance() { return new OutsourceShortageModel(false); }
        public OutsourceShortageView OutsourceShortage { get { return (OutsourceShortageView)base["OutsourceShortage"]; } }
        private void InitClass() { viewOutsourceShortage = new OutsourceShortageView(this); viewOutsourceShortage.SetResourceInfo("7C1F9ACC-1BF0-4355-A8F3-CD1C2CF8CF20"); base.Views.Add(viewOutsourceShortage); }
        public override string AssemblyName { get { return "U9Custom.UI.OutsourceShortageStatistics.Independent"; } }
        public override void AfterInitModel() { }
        public override void OnValidate() { base.OnValidate(); }
    }
}

