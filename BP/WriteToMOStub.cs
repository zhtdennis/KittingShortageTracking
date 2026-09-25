using System.Collections.Generic;
using System.ServiceModel;
using U9Custom.OutsourceShortageWritebackBP.Proxy;
using UFSoft.UBF;
using UFSoft.UBF.Exceptions;
using UFSoft.UBF.Service;
using UFSoft.UBF.Util.Context;

namespace U9Custom.OutsourceShortageWritebackBP
{
    public sealed class WriteToMOStub : IWriteToMO
    {
        public WriteToMOResult Do(IContext context, out IList<MessageBase> outMessages, WriteToMORequest request)
        {
            outMessages = new List<MessageBase>();
            WriteToMO bp = new WriteToMO { Request = request };
            return bp.Do();
        }
    }
}
