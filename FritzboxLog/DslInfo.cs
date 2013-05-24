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

        public byte LatencyDelayRx, LatencyDelayTx, SignalNoiseRatioRx, SignalNoiseRatioTx, LineAttenuationRx, LineAttenuationTx, DslMode, DslCarrierState, DLM;

        public bool LatencyRx, LatencyTx, BitswapRx, BitswapTx;

        public double INPRx, INPTx, FECPerMinDevice, FECPerMinExchange, CRCPerMinDevice, CRCPerMinExchange;

        public string LineProfile, LocalTime, Annex, DsINP, RFI_mode, UsNoiseBits, AdvisedDownstreamMarginOffset, AdvisedDsINP, AdvisedRFIMode, AdvisedUsNoiseBits, FritzGuiVersion, ATCUId, ATCUVendor, ATCUHybrid, DSLAMId, DSLAMVersion, DSLAMSerial, DSLVersion, SNRGraph, BitLoaderGraph, DownstreamMarginOffset;

        public byte CalcuateDLM()
        {
            DLM = 0;

            if (LatencyDelayRx == 1 && INPRx == 0.0)
            {
                DLM = 1;
            }
            else if (LatencyDelayRx == 8 && INPRx == 0.0)
            {
                DLM = 2;
            }
            else if (LatencyDelayRx == 8 && INPRx == 1.0)
            {
                DLM = 4;
            }
            else if (LatencyDelayRx == 8 && INPRx == 2.0)
            {
                DLM = 6;
            }
            else if (LatencyDelayRx == 16 && INPRx == 0.0)
            {
                DLM = 3;
            }
            else if (LatencyDelayRx == 16 && INPRx == 1.0)
            {
                DLM = 5;
            }
            else if (LatencyDelayRx == 16 && INPRx == 2.0)
            {
                DLM = 7;
            }
            else if (LatencyDelayRx == 16 && INPRx == 4.0)
            {
                DLM = 8;
            }

            return DLM;
        }
    }
}
