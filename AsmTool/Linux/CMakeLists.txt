find_library(PCI_LIBRARY pci REQUIRED)

add_library(asmIoLinux SHARED
	AsmIOLinux.c
)
target_link_libraries(asmIoLinux ${PCI_LIBRARY})