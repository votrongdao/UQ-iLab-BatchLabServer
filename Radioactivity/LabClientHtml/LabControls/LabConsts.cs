using System;

namespace LabClientHtml.LabControls
{
    public class LabConsts
    {
        //
        // XML string constants for setup IDs
        //
        public const string STRXML_SetupId_RadioactivityVsTime = "RadioactivityVsTime";
        public const string STRXML_SetupId_RadioactivityVsDistance = "RadioactivityVsDistance";
        public const string STRXML_SetupId_RadioactivityVsAbsorber = "RadioactivityVsAbsorber";
        public const string STRXML_SetupId_SimActivityVsTime = "SimActivityVsTime";
        public const string STRXML_SetupId_SimActivityVsDistance = "SimActivityVsDistance";
        public const string STRXML_SetupId_SimActivityVsTimeNoDelay = "SimActivityVsTimeNoDelay";
        public const string STRXML_SetupId_SimActivityVsDistanceNoDelay = "SimActivityVsDistanceNoDelay";

        //
        // XML Configuration
        //
        public const string STRXML_sources = "sources";
        public const string STRXML_absorbers = "absorbers";
        public const string STRXMLPARAM_default = "@default";
        public const string STRXML_source = "source";
        public const string STRXML_absorber = "absorber";
        public const string STRXML_name = "name";
        public const string STRXML_distances = "distances";
        public const string STRXML_minimum = "minimum";
        public const string STRXML_maximum = "maximum";
        public const string STRXML_stepsize = "stepsize";

        //
        // XML Validation
        //
        public const string STRXML_vdnDistance = "vdnDistance";
        public const string STRXML_vdnDuration = "vdnDuration";
        public const string STRXML_vdnRepeat = "vdnRepeat";
        public const string STRXML_vdnTotaltime = "vdnTotaltime";

        //
        // XML Specification and ExperimentResult
        //
        public const string STRXML_sourceName = "sourceName";
        public const string STRXML_absorberName = "absorberName";
        public const string STRXML_distance = "distance";
        public const string STRXML_duration = "duration";
        public const string STRXML_repeat = "repeat";
        public const char CHR_CsvSplitter = ',';

        //
        // XML ExperimentResult
        //
        public const string STRXML_dataVector = "dataVector";
    }
}
