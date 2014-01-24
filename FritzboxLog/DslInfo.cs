using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public double INPDownstream, INPUpstream, FECPerMinDownstream, FECPerMinUpstream, CRCPerMinDownstream, CRCPerMinUpstream, DslUptimeTotalSeconds;

        public string TextualLineProfile, DeviceCurrentTime, Annex, DsINP, RFI_mode, UsNoiseBits, AdvisedDownstreamMarginOffset, AdvisedDsINP, AdvisedRFIMode, AdvisedUsNoiseBits, FritzGuiVersion, ATCUId, ATCUVendor, ATCUHybrid, DSLAMId, DSLAMVersion, DSLAMSerial, DSLVersion, SNRGraph, BitLoaderGraph, DownstreamMarginOffset, Firmware, NspVersion, DeviceBootTime, DslConnectedSince;

        public VdslProfiles VdslLineProfile;
        public VdslTransferModes VdslTransferMode;

        public double CalcuatedFECsPerMinDownstream, CalcuatedFECsPerMinUpstream, CalcuatedCRCsPerMinDownstream, CalcuatedCRCsPerMinUpstream, DownstreamTrainingMargin, TxTotalPower, FeTxTotalPower, GHsEstimatedNearEndLoopLengthFt, GHsEstimatedNearEndLoopLengthMeters, GHsEstimatedFarEndLoopLengthFt, GHsEstimatedFarEndLoopLengthMeters, NearSNRMargin, NearAtteuation, NearAverageSnrMagin, NearAvaerageSnr, VdslEstimatedLoopLengthMeters;

        public double[] NearSnrMagins = new double[5];
        public double[] NearLineAttenuations = new double[5];
        public double[] NearSignalAttenations = new double[5];

        //public List<RelationalDouble> dsfecCounts, usfecCounts, dscrcCounts, uscrcCounts;

        /*
         * Need to Refactor below Variables.
         */

        public long LossOfSignalDownstream, LossOfSignalUpstream, LossOfFrameDownstream, LossOfFrameUpstream, FECDownstreamTotal, FECUpstreamTotal, CRCDownstreamTotal, CRCUpstreamTotal, ErroredSecondsDownstream, ErroredSecondsUpstream, SeverelyErroredSecondsDownstream, SeverelyErroredSecondsUpstream, VdslEstimatedLoopLengthFt;

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

        public enum VdslProfiles
        {
            Unknown,
            _8a,
            _8b,
            _8c,
            _8d,
            _12a,
            _12b,
            _17a,
            _30a
        };

        public enum VdslTransferModes
        {
            Unknown,
            ATM,
            PTM
        }
    }
}
