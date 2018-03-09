# ScopeVNS

This program was written for the Texas Biomedical Device Center to record vagus nerve stimulation pulse trains during behavioral experiments. We delivered VNS to laboratory animals using AM Systems 2100 stimulators. We then verified that stimulation occurred by connecting an oscilloscope in parallel. The exact model oscilloscope varied over the years, but eventually we settled on using digital oscilloscopes obtained from Picotech so that we could better record and save the data for future analysis.

This program, in its current state, can connect to and record signals from any PicoScope 2204A and 2206B. It is able to connect to multiple oscilloscopes at one time. A configuration file is available for editing the trigger threshold and other important parameters on a per-scope basis. Due to restrictions of the oscilloscope hardware, this software only records from channel A of the oscilloscopes. Picotech does not currently enable the ability to simultaneously set independent triggers on 2 separate channels on the same oscilloscope, and then record from each channel separately based upon those independent triggers.

This program was written in C# and WPF, and for it to work properly you need (1) a PicoScope from Picotech, and (2) the PicoScope drivers from their website. Any questions should be directed to David Pruitt.

