using System;
using System.Collections.Generic;
using System.Text;
using U9Custom.OutsourceShortageWritebackBP;
using UFIDA.U9.Base.Organization;
using UFIDA.U9.Base.UOM;
using UFIDA.U9.CBO.SCM.Item;
using UFIDA.U9.CBO.SCM.Warehouse;
using UFIDA.U9.CBO.Pub.Controller;
using UFIDA.U9.MO.MO;
using UFIDA.U9.MO.Enums;
using UFIDA.U9.ISV.MO;
using UFIDA.U9.ISV.MO.Proxy;
using UFSoft.UBF.AopFrame;
using UFSoft.UBF.Business;

namespace U9Custom.OutsourceShortageWritebackBP
{
    [Serializable]
    public sealed class WriteToMO
    {
        public WriteToMORequest Request { get; set; }

        [Transaction(UFSoft.UBF.Transactions.TransactionOption.Required)]
        [Logger]
        [Authorize]
        public WriteToMOResult Do()
        {
            if (Request == null) throw new InvalidOperationException("写回参数为空。 ");
            if (Request.MOID <= 0 || Request.OrgID <= 0) throw new InvalidOperationException("生产订单或组织参数无效。 ");
            if (Request.ItemIDs == null || Request.ShortageQtys == null || Request.ItemIDs.Length != Request.ShortageQtys.Length)
                throw new InvalidOperationException("欠料行参数不完整。 ");

            MO mo = new MO.EntityKey(Request.MOID).GetEntity();
            // IsOpenMO means "released after an original completion" in the
            // standard MO entity; it does not mean the UI state "开立".
            if (mo == null || mo.DocState != MOStateEnum.Opened)
                throw new InvalidOperationException("目标生产订单当前状态不可编辑。 ");

            WriteToMOResult result = new WriteToMOResult { MONo = Convert.ToString(mo.DocNo) };
            List<long> deleteIDs = new List<long>();
            Dictionary<long, long> deleteItemIDs = new Dictionary<long, long>();
            Dictionary<long, long> deleteUOMIDs = new Dictionary<long, long>();
            int lineNo = 10;
            if (!Request.SkipDelete)
            {
                foreach (MOPickList oldPick in mo.MOPickLists)
                {
                    if (oldPick.ItemMasterKey != null && oldPick.ItemMasterKey.ID > 0)
                    {
                        deleteIDs.Add(oldPick.ID);
                        deleteItemIDs[oldPick.ID] = oldPick.ItemMasterKey.ID;
                        if (oldPick.IssueUOMKey != null)
                            deleteUOMIDs[oldPick.ID] = oldPick.IssueUOMKey.ID;
                    }
                }
                if (deleteIDs.Count > 0)
                    DeleteSelectedPicks(Request.MOID, deleteIDs, deleteItemIDs, deleteUOMIDs);
                lineNo = 10;
            }
            if (Request.DeleteOnly)
            {
                result.SuccessCount = deleteIDs.Count;
                return result;
            }
            for (int i = 0; i < Request.ItemIDs.Length; i++)
            {
                string step = "开始处理";
                try
                {
                    if (Request.ItemIDs[i] <= 0 || Request.ShortageQtys[i] <= 0m)
                        throw new InvalidOperationException("料品或欠料数量无效。 ");
                    step = "创建标准生产订单备料实体";
                    long uomID = Request.IssueUOMIDs != null && i < Request.IssueUOMIDs.Length
                        ? Request.IssueUOMIDs[i] : 0;
                    long whID = Request.SupplyWhIDs != null && i < Request.SupplyWhIDs.Length
                        ? Request.SupplyWhIDs[i] : 0;
                    if (uomID <= 0)
                        throw new InvalidOperationException("未传入发料单位。 ");

                    using (ISession session = Session.Open())
                    {
                        MO moInSession = new MO.EntityKey(Request.MOID).GetEntity();
                        if (moInSession == null)
                            throw new InvalidOperationException("当前会话中未找到生产订单。 ");
#pragma warning disable 0618
                        MOPickList pick = MOPickList.CreateDefault(moInSession);
#pragma warning restore 0618
                        if (pick == null)
                            pick = MOPickList.Create(moInSession);
                        if (pick == null)
                            throw new InvalidOperationException("无法创建生产订单备料行。 ");
                        ItemMaster item = new ItemMaster.EntityKey(Request.ItemIDs[i]).GetEntity();
                        if (item == null)
                            throw new InvalidOperationException("未找到料品，ID=" + Request.ItemIDs[i] + "。 ");
                        pick.ItemMasterKey = item.Key;
                        pick.IssueUOMKey = new UOM.EntityKey(uomID);
                        pick.SupplyOrgKey = new Organization.EntityKey(Request.OrgID);
                        if (whID > 0)
                            pick.SupplyWhKey = new Warehouse.EntityKey(whID);
                        pick.DocLineNO = lineNo;
                        pick.OperationNum = "010";
                        pick.ActualReqDate = DateTime.Today;
                        pick.ActualReqQty = Request.ShortageQtys[i];
                        pick.BOMReqQty = Request.ShortageQtys[i];
                        pick.STDReqQty = Request.ShortageQtys[i];
                        pick.IsPurchase = true;
                        pick.IsAutoCreate = true;
                        pick.QtyType = UFIDA.U9.CBO.Enums.UsageQuantityTypeEnum.GetFromValue(1);
                        pick.QPA = 1m;
                        pick.WasteRate = 0m;
                        session.InList(pick);
                        step = "提交标准生产订单备料实体";
                        try
                        {
                            session.Commit();
                        }
                        catch (Exception commitError)
                        {
                            // Some U9 builds throw from MO.OnUpdated after the
                            // insert has already been flushed. Verify before failing.
                            MO afterCommitError = new MO.EntityKey(Request.MOID).GetEntity();
                            if (!ContainsPick(afterCommitError, Request.ItemIDs[i], Request.ShortageQtys[i]))
                                throw new InvalidOperationException(commitError.Message, commitError);
                        }
                    }
                    MO savedMO = new MO.EntityKey(Request.MOID).GetEntity();
                    if (!ContainsPick(savedMO, Request.ItemIDs[i], Request.ShortageQtys[i]))
                        throw new InvalidOperationException("提交后未在生产订单备料集合中找到新增行。 ");
                    result.SuccessCount++;
                    lineNo += 10;
                }
                catch (Exception ex)
                {
                    string material = Request.MaterialCodes != null && i < Request.MaterialCodes.Length ? Request.MaterialCodes[i] : string.Empty;
                    result.Failures.Add(material + "：" + step + "失败：" + ex.Message);
                }
            }
            return result;
        }

        private static bool ContainsPick(MO mo, long itemID, decimal qty)
        {
            if (mo == null) return false;
            foreach (MOPickList pick in mo.MOPickLists)
            {
                if (pick.ItemMasterKey != null && pick.ItemMasterKey.ID == itemID
                    && pick.ActualReqQty == qty && pick.ActualReqDate.Date == DateTime.Today)
                    return true;
            }
            return false;
        }

        private static void DeleteSelectedPicks(long moID, List<long> pickIDs,
            Dictionary<long, long> itemIDs, Dictionary<long, long> uomIDs)
        {
            if (pickIDs == null || pickIDs.Count == 0) return;

            List<MOPickListDTOData> picks = new List<MOPickListDTOData>();
            foreach (long pickID in pickIDs)
            {
                picks.Add(new MOPickListDTOData {
                    ID = pickID,
                    MO = new CommonArchiveDataDTOData(moID, string.Empty, string.Empty),
                    ItemMaster = new CommonArchiveDataDTOData(itemIDs[pickID], string.Empty, string.Empty),
                    IssueUOM = uomIDs.ContainsKey(pickID)
                        ? new CommonArchiveDataDTOData(uomIDs[pickID], string.Empty, string.Empty)
                        : null,
                    CUD = 8
                });
            }
            MODTOData moData = new MODTOData {
                MOID = moID,
                CUD = 1,
                MOPickListDTOs = picks
            };
            MOModifyDTOData modify = new MOModifyDTOData {
                MOKeyDTO = new MOKeyDTOData(moID, string.Empty, 0, 0, 0, string.Empty),
                MODTO = moData
            };
            ModifyMO4ExternalProxy proxy = new ModifyMO4ExternalProxy {
                MOModifyDTOs = new List<MOModifyDTOData> { modify }
            };
            if (!proxy.Do())
                throw new InvalidOperationException("标准生产订单修改服务未返回成功。 ");

        }

    }
}
