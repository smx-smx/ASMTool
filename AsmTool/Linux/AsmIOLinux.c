/**
 * Copyright (C) 2019 Stefano Moioli <smxdev4@gmail.com>
 */
#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#include <unistd.h>
#include <sys/time.h>
#include <sys/io.h>
#include <pci/pci.h>

typedef unsigned int uint;
typedef unsigned char byte;

#define ASM_REG_REGSEL 0xF8
#define ASM_REG_REGDAT_IN 0xFC
#define ASM_REG_REGDAT_OUT1 0xF0
#define ASM_REG_REGDAT_OUT2 0xF4
#define ASM_REG_CTRL 0xE0

#define DPRINTF(fmt, ...) \
	fprintf(stderr, "[%s:%d] " fmt, __func__, __LINE__, ##__VA_ARGS__)

struct PciAddr {
    uint32_t bus;
    uint32_t dev;
    uint32_t func;
};

struct AsmPacket {
    struct PciAddr addr;
    uint32_t data1;
    uint32_t data2;
    uint8_t unk[24];
};

// read is not exclusive
#define READ_BIT_FLAG 1
// write is exclusive
#define WRITE_BIT_FLAG 2

static struct pci_access *pacc = NULL;

void __attribute__((destructor)) dtor(){
    if(!pacc) return;
    pci_cleanup(pacc);
    pacc = NULL;
}

uint LoadAsmIODriver(){
    if(!pacc){
        pacc = pci_alloc();
        if(!pacc){
            return 0;
        }
        pci_init(pacc);
    }
    return 1;
}

uint32_t PCI_Read_DWORD(uint bus, uint dev, uint func, uint offset){
    struct pci_dev *pdev = pci_get_dev(pacc, 0, bus, dev, func);
    if(!pdev){
        return -1;
    }
    return pci_read_long(pdev, offset);
}

byte PCI_Read_BYTE(uint bus, uint dev, uint func, uint offset){
    struct pci_dev *pdev = pci_get_dev(pacc, 0, bus, dev, func);
    if(!pdev){
        return -1;
    }
    return pci_read_byte(pdev, offset);
}

uint PCI_Write_BYTE(uint bus, uint dev, uint func, uint offset, byte value){
    struct pci_dev *pdev = pci_get_dev(pacc, 0, bus, dev, func);
    if(!pdev){
        return -1;
    }
    pci_write_byte(pdev, offset, value);
    return 0;
}

uint Wait_Read_Ready(uint bus, uint dev, uint func){
    byte flag;
    int cnt = 0;

    struct pci_dev *pdev = pci_get_dev(pacc, 0, bus, dev, func);
    if(!pdev){
        return -1;
    }

    for(;cnt < 20000; cnt++){
        for(
            flag = pci_read_byte(pdev, ASM_REG_CTRL);;
            cnt++, flag = pci_read_byte(pdev, ASM_REG_CTRL)
        ){
            if(flag == 0xFF){
                if(++cnt >= 10)
                    return -2;
            }

            if(!(flag & READ_BIT_FLAG))
                break;

            flag = pci_read_byte(pdev, ASM_REG_CTRL);
            if(flag & READ_BIT_FLAG){
                return 0;
            }
        }
    }

    return -1;
}

uint Wait_Write_Ready(uint bus, uint dev, uint func){
    byte flag;
    int cnt = 0;

    struct pci_dev *pdev = pci_get_dev(pacc, 0, bus, dev, func);
    if(!pdev){
        return -1;
    }

    for(;cnt < 20000; cnt++){
        for(
            flag = pci_read_byte(pdev, ASM_REG_CTRL);;
            cnt++, flag = pci_read_byte(pdev, ASM_REG_CTRL)
        ){
            if(flag == 0xFF){
                if(++cnt >= 10)
                    return -2;
            }

            if(flag & WRITE_BIT_FLAG)
                break;

            flag = pci_read_byte(pdev, ASM_REG_CTRL);
            if(!(flag & WRITE_BIT_FLAG)){
                return 0;
            }
        }
    }

    return -1;
}

static uint32_t ComputeInternalRegister(byte b0, byte b1, byte b2){
    return (0
        | (b2 << 16)
        | (b1 << 8)
        | b0
    );
}

uint ReadCMD(
    uint bus, uint dev, uint func, struct AsmPacket *outPacket
){
    struct pci_dev *pdev = pci_get_dev(pacc, 0, bus, dev, func);
    if(!pdev){
        return -1;
    }

    outPacket->data1 = pci_read_long(pdev, ASM_REG_REGDAT_OUT1);
    outPacket->data2 = pci_read_long(pdev, ASM_REG_REGDAT_OUT2);
    pci_write_byte(pdev, ASM_REG_CTRL, READ_BIT_FLAG);
    return 0;
}

static inline void __attribute__((always_inline)) AsmCmd(
    struct pci_dev *pdev,
    uint reg, uint data, uint type
){
    pci_write_long(pdev, ASM_REG_REGSEL, reg);
    pci_write_long(pdev, ASM_REG_REGDAT_IN, data);
    pci_write_byte(pdev, ASM_REG_CTRL, type);
}

uint WriteCmdALL(
    uint bus, uint dev, uint func,
    byte cmd_reg_b0, byte cmd_reg_b1, byte cmd_reg_b2,
    uint cmd_dat0, uint cmd_dat1, uint cmd_dat2
){
    struct pci_dev *pdev = pci_get_dev(pacc, 0, bus, dev, func);
    if(!pdev){
        return -1;
    }

    uint32_t reg = ComputeInternalRegister(cmd_reg_b0, cmd_reg_b1, cmd_reg_b2);

    int rc;
    if((rc = Wait_Write_Ready(bus, dev, func)) < 0){
        DPRINTF("Wait_Write_Ready failed! (rc=%d)\n", rc);
        return rc;
    }

    AsmCmd(pdev, reg, cmd_dat0, WRITE_BIT_FLAG);

    if((rc = Wait_Write_Ready(bus, dev, func)) < 0){
        DPRINTF("Wait_Write_Ready failed (awaiting first command)! (rc=%d)\n", rc);
        return rc;
    }

    AsmCmd(pdev, cmd_dat1, cmd_dat2, WRITE_BIT_FLAG);
    return 0;
}
