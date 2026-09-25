using System;
using UFSoft.UBF.UI.ActionProcess;
using UFSoft.UBF.UI.IView;
namespace U9Custom.UI.ManufactureSimulateShortageTable.Independent
{
    public class ShortageTableModelAction : BaseAction
    {
        public ShortageTableModelAction(IPart part) : base(part) { }
        public new ShortageTableModelModel CurrentModel { get { return (ShortageTableModelModel)base.CurrentModel; } }
        public void OnQuery(object sender, EventArgs e) { }
        public void OnClear(object sender, EventArgs e) { }
        public void OnOutput(object sender, EventArgs e) { }
    }
}
