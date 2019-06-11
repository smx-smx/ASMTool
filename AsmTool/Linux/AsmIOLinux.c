/**
 * Copyright (C) 2019 Stefano Moioli <smxdev4@gmail.com>
 */
#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#include <unistd.h>
#include <sys/time.h>
#include <sys/io.h>

typedef unsigned int uint;
typedef unsigned char byte;

#define PCI_ENABLE_BIT 0x80000000
#define PCI_PORT_ADDR 0xCF8
#define PCI_PORT_DATA 0xCFC

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

#define PCIAddr(bus, dev, func, offset) \
    ((bus << 16) | (dev << 11) | (func << 8) | (offset & 0xFC) | PCI_ENABLE_BIT)

// read is not exclusive
#define READ_BIT_FLAG 1
// write is exclusive
#define WRITE_BIT_FLAG 2

uint LoadAsmIODriver(){
    iopl(3);
    ioperm(PCI_PORT_ADDR, 1, 1);
    ioperm(PCI_PORT_DATA, 1, 1);
    return 1;
}

static inline uint32_t __attribute__((always_inline)) _PCI_Read_DWORD(uint bus, uint dev, uint func, uint offset){
    outl(PCIAddr(bus, dev, func, offset), PCI_PORT_ADDR);
    return inl(PCI_PORT_DATA);
}

static inline byte __attribute__((always_inline)) _PCI_Read_BYTE(uint bus, uint dev, uint func, uint offset){
    outl(PCIAddr(bus, dev, func, offset), PCI_PORT_ADDR);
    return inb(PCI_PORT_DATA);
}

uint32_t PCI_Read_DWORD(uint bus, uint dev, uint func, uint offset){
    return _PCI_Read_DWORD(bus, dev, func, offset);
}

byte PCI_Read_BYTE(uint bus, uint dev, uint func, uint offset){
    return _PCI_Read_BYTE(bus, dev, func, offset);
}


uint Wait_Read_Ready(uint bus, uint dev, uint func){
        byte flag;
    int cnt = 0;

    for(;cnt < 20000; cnt++){
        outl(PCIAddr(bus, dev, func, ASM_REG_CTRL), PCI_PORT_ADDR);
        for(
            flag = inb(PCI_PORT_DATA);;
            cnt++, flag = inb(PCI_PORT_DATA)
        ){
            if(flag == 0xFF){
                if(++cnt >= 10)
                    return -2;
            }

            if(!(flag & READ_BIT_FLAG))
                break;

            outl(PCIAddr(bus, dev, func, ASM_REG_CTRL), PCI_PORT_ADDR);
            flag = inb(PCI_PORT_DATA);

            if(flag & READ_BIT_FLAG){
                return 0;
            }
            
            outl(PCIAddr(bus, dev, func, ASM_REG_CTRL), PCI_PORT_ADDR);
        }
    }

    return -1;
}

uint Wait_Write_Ready(uint bus, uint dev, uint func){
    byte flag;
    int cnt = 0;

    for(;cnt < 20000; cnt++){
        outl(PCIAddr(bus, dev, func, ASM_REG_CTRL), PCI_PORT_ADDR);
        for(
            flag = inb(PCI_PORT_DATA);;
            cnt++, flag = inb(PCI_PORT_DATA)
        ){
            if(flag == 0xFF){
                if(++cnt >= 10)
                    return -2;
            }

            if(flag & WRITE_BIT_FLAG)
                break;

            outl(PCIAddr(bus, dev, func, ASM_REG_CTRL), PCI_PORT_ADDR);
            flag = inb(PCI_PORT_DATA);

            if(!(flag & WRITE_BIT_FLAG)){
                return 0;
            }

            outl(PCIAddr(bus, dev, func, ASM_REG_CTRL), PCI_PORT_ADDR);
        }
    }

    return -1;
}

static uint32_t ComputeInternalRegister(byte b0, byte b1, byte b2){
    return b0 + ((b1 + (b2 << 8)) << 8);
}

uint ReadCMD(
    uint bus, uint dev, uint func, struct AsmPacket *outPacket
){
    outl(PCIAddr(bus, dev, func, ASM_REG_REGDAT_OUT1), PCI_PORT_ADDR);
    outPacket->data1 = inl(PCI_PORT_DATA);

    outl(PCIAddr(bus, dev, func, ASM_REG_REGDAT_OUT2), PCI_PORT_ADDR);
    outPacket->data2 = inl(PCI_PORT_DATA);

    outl(PCIAddr(bus, dev, func, ASM_REG_CTRL), PCI_PORT_ADDR);
    outb(READ_BIT_FLAG, PCI_PORT_DATA);
    return 0;
}

static inline void __attribute__((always_inline)) AsmCmd(
    uint bus, uint dev, uint func,
    uint reg, uint data, uint type
){
    outl(PCIAddr(bus, dev, func, ASM_REG_REGSEL), PCI_PORT_ADDR);
    outl(reg, PCI_PORT_DATA);

    outl(PCIAddr(bus, dev, func, ASM_REG_REGDAT_IN), PCI_PORT_ADDR);
    outl(data, PCI_PORT_DATA);

    outl(PCIAddr(bus, dev, func, ASM_REG_CTRL), PCI_PORT_ADDR);
    outb(type, PCI_PORT_DATA);
}

uint WriteCmdALL(
    uint bus, uint dev, uint func,
    byte cmd_reg_b0, byte cmd_reg_b1, byte cmd_reg_b2,
    uint cmd_dat0, uint cmd_dat1, uint cmd_dat2
){
    uint32_t reg = ComputeInternalRegister(cmd_reg_b0, cmd_reg_b1, cmd_reg_b2);
    
    int rc;
    if((rc = Wait_Write_Ready(bus, dev, func)) < 0){
        DPRINTF("Wait_Write_Ready failed! (rc=%d)\n", rc);
        return rc;
    }

    AsmCmd(bus, dev, func, reg, cmd_dat0, WRITE_BIT_FLAG);

    if((rc = Wait_Write_Ready(bus, dev, func)) < 0){
        DPRINTF("Wait_Write_Ready failed (awaiting first command)! (rc=%d)\n", rc);
        return rc;
    }

    AsmCmd(bus, dev, func, cmd_dat1, cmd_dat2, WRITE_BIT_FLAG);
    return 0;
}
