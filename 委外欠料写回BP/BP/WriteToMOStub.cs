using System;
using System.Collections.Generic;
using System.ServiceModel;
using UFSoft.UBF;
using UFSoft.UBF.Exceptions;
using UFSoft.UBF.Service;
using UFSoft.UBF.Service.Base;
using UFSoft.UBF.Util.Context;

namespace U9Custom.OutsourceShortageWritebackBP
{
    [ServiceImplement]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public sealed class WriteToMOStub : OperationStubBase, IWriteToMO
    {
        public WriteToMOResult Do(IContext context, out IList<MessageBase> outMessages, WriteToMORequest request)
        {
            ICommonDataContract commonData = CommonDataContractFactory.GetCommonData(context, out outMessages);
            return DoEx(commonData, request);
        }

        public WriteToMOResult DoEx(ICommonDataContract commonData, WriteToMORequest request)
        {
            CommonData = commonData;
            const string operationName = "U9Custom.OutsourceShortageWritebackBP.WriteToMO";
            try
            {
                BeforeInvoke(operationName);
                return new WriteToMO { Request = request }.Do();
            }
            catch (Exception ex)
            {
                DealException(ex);
                throw;
            }
            finally
            {
                FinallyInvoke(operationName);
            }
        }
    }
}
