using System;
using System.Collections;
using System.Collections.Generic;
using System.ServiceModel;
using U9Custom.OutsourceShortageWritebackBP;
using UFSoft.UBF;
using UFSoft.UBF.Service;
using UFSoft.UBF.Service.Base;
using UFSoft.UBF.Util.Context;
using UFSoft.UBF.Exceptions;

namespace U9Custom.OutsourceShortageWritebackBP.Proxy
{
    [Serializable]
    public sealed class WriteToMOProxy : OperationProxyBase
    {
        public WriteToMORequest Request { get; set; }

        public WriteToMOResult Do()
        {
            InitKeyList();
            WriteToMOResult result = (WriteToMOResult)InvokeAgent<IWriteToMO>();
            return result;
        }

        protected override object InvokeImplement<T>(T channelObject)
        {
            IWriteToMO channel = channelObject as IWriteToMO;
            if (channel == null) return null;
            return channel.Do(ContextManager.Context, out returnMsgs, Request);
        }

        private void InitKeyList()
        {
            Hashtable dict = new Hashtable();
        }
    }
}
