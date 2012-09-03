using System;
using Library.LabClient;

namespace LabClientHtml.LabControls
{
    public class ResultInfo : ExperimentResultInfo
    {
        public struct Measurement
        {
            public string name;
            public string units;
            public string format;
            public string values;
        }

        public Measurement fieldCurrent;
        public Measurement speed;
        public Measurement voltage;
        public Measurement statorCurrent;

        //
        // Pre-synchronisation
        //
        public Measurement speedSetpoint;
        public Measurement syncVoltage;
        public Measurement syncFrequency;
        public Measurement mainsVoltage;
        public Measurement mainsFrequency;
        public Measurement syncMainsPhase;
        public Measurement synchronism;

        //
        // Synchronisation
        //
        public Measurement torqueSetpoint;
        public Measurement powerFactor;
        public Measurement realPower;
        public Measurement reactivePower;
        public Measurement phaseCurrent;

    }
}
