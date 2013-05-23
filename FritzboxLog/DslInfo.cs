using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FritzboxLog
{
    public class DslInfo
    {
        public long CurrentThroughputRx, CurrentThroughputTx, DslUpTime;

        public int BitPilotReference;

        public byte latancyDelayRx, latancyDelayTx, SignalNoiseRatioRx, SignalNoiseRatioTx, LineAttenuationRx, LineAttenuationTx, DslMode, DslCarrierState, DLM;

        public bool latancyRx, latancyTx, BitswapRx, BitswapTx;

        public double INPRx, INPTx, FECPerMinDevice, FECPerMinExchange, CRCPerMinDevice, CRCPerMinExchange;

        public string LineProfile, LocalTime, Annex, DsINP, RFI_mode, UsNoiseBits, AdvisedDownstreamMarginOffset, AdvisedDsINP, AdvisedRFIMode, AdvisedUsNoiseBits, FritzGuiVersion, ATCUId, ATCUVendor, ATCUHybrid, DSLAMId, DSLAMVersion, DSLAMSerial, DSLVersion, SNRGraph, BitLoaderGraph, DownstreamMarginOffset;

        public byte CalcuateDLM()
        {
            DLM = 0;

            if (latancyDelayRx == 1 && INPRx == 0.0)
            {
                DLM = 1;
            }
            else if (latancyDelayRx == 8 && INPRx == 0.0)
            {
                DLM = 2;
            }
            else if (latancyDelayRx == 8 && INPRx == 1.0)
            {
                DLM = 4;
            }
            else if (latancyDelayRx == 8 && INPRx == 2.0)
            {
                DLM = 6;
            }
            else if (latancyDelayRx == 16 && INPRx == 0.0)
            {
                DLM = 3;
            }
            else if (latancyDelayRx == 16 && INPRx == 1.0)
            {
                DLM = 5;
            }
            else if (latancyDelayRx == 16 && INPRx == 2.0)
            {
                DLM = 7;
            }
            else if (latancyDelayRx == 16 && INPRx == 4.0)
            {
                DLM = 8;
            }

            return DLM;
        }
    }
}
