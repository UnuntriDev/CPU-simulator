using System;
using Xunit;

namespace Symulator_x86.Tests
{
    public sealed class CpuTests
    {
        [Fact]
        public void MovCopiesRegisterToRegister()
        {
            var cpu = new Cpu();
            cpu[Register.AX] = 0x1234;

            cpu.Mov(Operand.BX, Operand.AX, 0);

            Assert.Equal((ushort)0x1234, cpu[Register.BX]);
        }

        [Fact]
        public void MovRejectsMemoryToMemory()
        {
            var cpu = new Cpu();

            Assert.Throws<InvalidOperationException>(
                () => cpu.Mov(Operand.Memory, Operand.Memory, 0x1000));
        }

        [Fact]
        public void XchgSwapsValues()
        {
            var cpu = new Cpu();
            cpu[Register.AX] = 0x1111;
            cpu[Register.DX] = 0x2222;

            cpu.Xchg(Operand.AX, Operand.DX, 0);

            Assert.Equal((ushort)0x2222, cpu[Register.AX]);
            Assert.Equal((ushort)0x1111, cpu[Register.DX]);
        }

        [Fact]
        public void PushAndPopRegisterRoundTrip()
        {
            var cpu = new Cpu();
            cpu[Register.CX] = 0xCAFE;

            cpu.Push(Operand.CX, 0);
            var value = cpu.Pop(Operand.AX, 0);

            Assert.Equal((ushort)0xCAFE, value);
            Assert.Equal((ushort)0xCAFE, cpu[Register.AX]);
            Assert.Equal(Cpu.InitialSP, cpu[Register.SP]);
        }

        [Fact]
        public void PushAndPopMemoryRoundTrip()
        {
            var cpu = new Cpu();
            cpu.WriteMem(0x2000, 0xBEEF);

            cpu.Push(Operand.Memory, 0x2000);
            cpu.Pop(Operand.Memory, 0x3000);

            Assert.Equal((ushort)0xBEEF, cpu.ReadMem(0x3000));
            Assert.True(cpu.IsMemoryInitialized(0x3000));
        }

        [Fact]
        public void PopIntoStackPointerUsesPoppedValue()
        {
            var cpu = new Cpu();
            cpu[Register.AX] = 0x3456;

            cpu.Push(Operand.AX, 0);
            var value = cpu.Pop(Operand.SP, 0);

            Assert.Equal((ushort)0x3456, value);
            Assert.Equal((ushort)0x3456, cpu[Register.SP]);
        }

        [Fact]
        public void PopFromEmptyStackThrows()
        {
            var cpu = new Cpu();

            Assert.Throws<InvalidOperationException>(
                () => cpu.Pop(Operand.AX, 0));
        }

        [Fact]
        public void EffectiveAddressWrapsToWord()
        {
            var cpu = new Cpu();
            cpu[Register.BX] = 0xFFF0;
            cpu[Register.SI] = 0x0020;

            var address = cpu.CalculateEA(Register.BX, Register.SI, 0x0001);

            Assert.Equal((ushort)0x0011, address);
        }

        [Fact]
        public void MemoryDistinguishesUninitializedFromZero()
        {
            var cpu = new Cpu();

            Assert.False(cpu.IsMemoryInitialized(0x1234));
            Assert.Equal((ushort)0, cpu.ReadMem(0x1234));

            cpu.WriteMem(0x1234, 0);

            Assert.True(cpu.IsMemoryInitialized(0x1234));
            Assert.True(cpu.TryReadMem(0x1234, out var value));
            Assert.Equal((ushort)0, value);
        }
    }
}
