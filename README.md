# ASMTool
Firmware dumper and various utilities for PCI based ASMedia USB Controllers

It looks like all ICs in the ASM1x4x, ASM2x4x and ASM3x4x family use the same interface and share the same registers, but i only tested this with the ASM2142 Controller that i have in my system.

# Why?
I'm having issues with my ASM2142 controller (lockup with USB 3.1 and large transfers), and i couldn't find a way to dump the current firmware.

The firmware updater can internally read the firmware, but it doesn't offer a way to save it.

# How to use
## Linux
```gcc -shared -o libAsmIOLinux.so -fPIC Linux/AsmIOLinux.c```
Place the resulting `.so` file next to the ASMTool executable (obtained by building this project)

## Windows
You'll need `AsmIo.sys` (for 32bit Windows) or `AsmIo64.sys` (for 64bit Windows).

You will also need `asmiodll.dll`. You can find these files if you google `ASM2142 firmware`.
Download the firmware updater and you'll find the files in there.

Place all files next to the ASMTool executable (obtained by building this project)

# How to contribute?
You can either extend this program and add new functionality, or

open a new Issue and attach the firmware obtained by running this program, so that other users can update their firmwares or try older versions to see if they work better

# They are custom Intel 8051 cores!
It turns out ASMedia USB controllers are custom Intel 8051 cores, and the firmware file can be disassembled into i8051 assembly

# Security implications
It looks like this interface could be used to flash malicious code onto ASMedia chips, as explained by
https://chefkochblog.wordpress.com/2018/03/19/asmedia-usb-3-x-controller-with-keylogger-and-malware-risks/

The chip performs no signature checks on the code being flashed and, being a PCIe device, could abuse DMA to read and write arbitrary memory
