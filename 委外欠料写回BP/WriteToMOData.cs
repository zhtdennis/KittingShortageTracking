using System;
using System.Collections.Generic;
using System.ServiceModel;
using UFSoft.UBF;
using UFSoft.UBF.Exceptions;
using UFSoft.UBF.Service;
using UFSoft.UBF.Util.Context;

namespace U9Custom.OutsourceShortageWritebackBP
{
    [ServiceContract(Namespace = "http://www.UFIDA.org", Name = "U9Custom.OutsourceShortageWritebackBP.IWriteToMO")]
    public interface IWriteToMO
    {
        [ServiceKnownType(typeof(ApplicationContext))]
        [ServiceKnownType(typeof(PlatformContext))]
        [ServiceKnownType(typeof(ThreadContext))]
        [ServiceKnownType(typeof(UFSoft.UBF.Business.BusinessException))]
        [ServiceKnownType(typeof(MessageBase))]
        [FaultContract(typeof(ServiceLostException))]
        [FaultContract(typeof(ServiceException))]
        [FaultContract(typeof(ServiceExceptionDetail))]
        [FaultContract(typeof(Exception))]
        [OperationContract]
        WriteToMOResult Do(IContext context, out IList<MessageBase> outMessages, WriteToMORequest request);
    }

    [Serializable]
    public sealed class WriteToMORequest
    {
        public long MOID;
        public long OrgID;
        public long[] ItemIDs;
        public long[] IssueUOMIDs;
        public long[] SupplyWhIDs;
        public decimal[] ShortageQtys;
        public string[] MaterialCodes;
        public bool DeleteOnly;
        public bool SkipDelete;
    }

    [Serializable]
    public sealed class WriteToMOResult
    {
        public string MONo;
        public int SuccessCount;
        public List<string> Failures = new List<string>();
    }
}
