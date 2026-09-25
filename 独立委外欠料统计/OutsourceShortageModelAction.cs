using System;
using UFSoft.UBF.UI.ActionProcess;
using UFSoft.UBF.UI.IView;
namespace U9Custom.UI.OutsourceShortageStatistics.Independent
{
    public class OutsourceShortageModelAction : BaseAction
    {
        public OutsourceShortageModelAction(IPart part) : base(part) { }
        public new OutsourceShortageModel CurrentModel { get { return (OutsourceShortageModel)base.CurrentModel; } }
        public void OnQuery(object sender, EventArgs e) { }
        public void OnClear(object sender, EventArgs e) { }
        public void OnOutput(object sender, EventArgs e) { }
        public void OnWriteMO(object sender, EventArgs e) { }
    }
}

