# ASMTool
Firmware dumper and various utilities for PCI based ASMedia USB Controllers

This was tested with an ASM2142 Controller.

ASM3142 support was not tested, use at your own risk or contact me if you have information about them.

ASM1xxx is not supported yet. Contact me if you have a PCIe based device.

# Why?
I'm having issues with my ASM2142 controller (lockup with USB 3.1 and large transfers), and i couldn't find a way to dump the current firmware.

The firmware updater can internally read the firmware, but it doesn't offer a way to save it.

# How to use
To use this, you'll need `AsmIo.sys` (for 32bit Windows) or `AsmIo64.sys` (for 64bit Windows).

You will also need `asmiodll.dll`. You can find these files if you google `ASM2142 firmware`.
Download the firmware updater and you'll find the files in there.

Place all files next to the ASMTool executable (obtained by building this project)

# How to contribute?
You can either extend this program and add new functionality, or

open a new Issue and attach the firmware obtained by running this program, so that other users can update their firmwares or try older versions to see if they work better
