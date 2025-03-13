find_library(PCI_LIBRARY pci REQUIRED)

add_library(AsmIoLinux SHARED
	AsmIOLinux.c
)
target_link_libraries(AsmIoLinux ${PCI_LIBRARY})