using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FritzboxLog
{
    public class DslInfo
    {
        public long CurrentDslSyncRateDownstream, CurrentDslSyncRateUpstream;

        public int BitPilotReference;

        public byte LatencyDelayDownstream, LatencyDelayUpstream, SignalNoiseRatioDownstream, SignalNoiseRatioUpstream, LineAttenuationDownstream, LineAttenuationUpstream, DslTrainedModulation, DslTrainingState, DLM;

        public bool LatencyModeDownstream, LatencyModeUpstream, BitswapDownstream, BitswapUpstream, UpgradesManaged;

        public double INPDownstream, INPUpstream, FECPerMinDownstream, FECPerMinUpstream, CRCPerMinDownstream, CRCPerMinUpstream;

        public string LineProfile, DeviceCurrentTime, Annex, DsINP, RFI_mode, UsNoiseBits, AdvisedDownstreamMarginOffset, AdvisedDsINP, AdvisedRFIMode, AdvisedUsNoiseBits, FritzGuiVersion, ATCUId, ATCUVendor, ATCUHybrid, DSLAMId, DSLAMVersion, DSLAMSerial, DSLVersion, SNRGraph, BitLoaderGraph, DownstreamMarginOffset, Firmware, NspVersion, DeviceBootTime, DslConnectedSince;

        /*
         * Need to Refactor below Variables.
         */

        public long LossOfSignalDownstream, LossOfSignalUpstream, LossOfFrameDownstream, LossOfFrameUpstream, FECDownstreamTotal, FECUpstreamTotal, CRCDownstreamTotal, CRCUpstreamTotal, ErroredSecondsDownstream, ErroredSecondsUpstream, SeverelyErroredSecondsDownstream, SeverelyErroredSecondsUpstream;

        public byte CalcuateDLM()
        {
            DLM = 0; //Cant Work Out.

            if (LatencyDelayDownstream == 1 && INPDownstream == 0.0)
            {
                DLM = 1;
            }
            else if (LatencyDelayDownstream == 8 && INPDownstream == 0.0)
            {
                DLM = 2;
            }
            else if (LatencyDelayDownstream == 8 && INPDownstream == 1.0)
            {
                DLM = 4;
            }
            else if (LatencyDelayDownstream == 8 && INPDownstream == 2.0)
            {
                DLM = 6;
            }
            else if (LatencyDelayDownstream == 16 && INPDownstream == 0.0)
            {
                DLM = 3;
            }
            else if (LatencyDelayDownstream == 16 && INPDownstream == 1.0)
            {
                DLM = 5;
            }
            else if (LatencyDelayDownstream == 16 && INPDownstream == 2.0)
            {
                DLM = 7;
            }
            else if (LatencyDelayDownstream == 16 && INPDownstream == 4.0)
            {
                DLM = 8;
            }

            return DLM;
        }
    }
}
