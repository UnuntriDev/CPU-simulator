using System;

namespace Symulator_x86.Tests
{
    internal static class Program
    {
        private static int _passed;

        private static int Main()
        {
            try
            {
                MovCopiesRegisterToRegister();
                MovRejectsMemoryToMemory();
                XchgSwapsValues();
                PushAndPopRegisterRoundTrip();
                PushAndPopMemoryRoundTrip();
                PopFromEmptyStackThrows();
                EffectiveAddressWrapsToWord();
                MemoryDistinguishesUninitializedFromZero();

                Console.WriteLine($"All tests passed: {_passed}");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return 1;
            }
        }

        private static void MovCopiesRegisterToRegister()
        {
            var cpu = new Cpu();
            cpu[Register.AX] = 0x1234;

            cpu.Mov(Operand.BX, Operand.AX, 0);

            AssertEqual((ushort)0x1234, cpu[Register.BX], nameof(MovCopiesRegisterToRegister));
        }

        private static void MovRejectsMemoryToMemory()
        {
            var cpu = new Cpu();

            AssertThrows<InvalidOperationException>(
                () => cpu.Mov(Operand.Memory, Operand.Memory, 0x1000),
                nameof(MovRejectsMemoryToMemory));
        }

        private static void XchgSwapsValues()
        {
            var cpu = new Cpu();
            cpu[Register.AX] = 0x1111;
            cpu[Register.DX] = 0x2222;

            cpu.Xchg(Operand.AX, Operand.DX, 0);

            AssertEqual((ushort)0x2222, cpu[Register.AX], nameof(XchgSwapsValues));
            AssertEqual((ushort)0x1111, cpu[Register.DX], nameof(XchgSwapsValues));
        }

        private static void PushAndPopRegisterRoundTrip()
        {
            var cpu = new Cpu();
            cpu[Register.CX] = 0xCAFE;

            cpu.Push(Operand.CX, 0);
            var value = cpu.Pop(Operand.AX, 0);

            AssertEqual((ushort)0xCAFE, value, nameof(PushAndPopRegisterRoundTrip));
            AssertEqual((ushort)0xCAFE, cpu[Register.AX], nameof(PushAndPopRegisterRoundTrip));
            AssertEqual(Cpu.InitialSP, cpu[Register.SP], nameof(PushAndPopRegisterRoundTrip));
        }

        private static void PushAndPopMemoryRoundTrip()
        {
            var cpu = new Cpu();
            cpu.WriteMem(0x2000, 0xBEEF);

            cpu.Push(Operand.Memory, 0x2000);
            cpu.Pop(Operand.Memory, 0x3000);

            AssertEqual((ushort)0xBEEF, cpu.ReadMem(0x3000), nameof(PushAndPopMemoryRoundTrip));
            AssertTrue(cpu.IsMemoryInitialized(0x3000), nameof(PushAndPopMemoryRoundTrip));
        }

        private static void PopFromEmptyStackThrows()
        {
            var cpu = new Cpu();

            AssertThrows<InvalidOperationException>(
                () => cpu.Pop(Operand.AX, 0),
                nameof(PopFromEmptyStackThrows));
        }

        private static void EffectiveAddressWrapsToWord()
        {
            var cpu = new Cpu();
            cpu[Register.BX] = 0xFFF0;
            cpu[Register.SI] = 0x0020;

            var address = cpu.CalculateEA(Register.BX, Register.SI, 0x0001);

            AssertEqual((ushort)0x0011, address, nameof(EffectiveAddressWrapsToWord));
        }

        private static void MemoryDistinguishesUninitializedFromZero()
        {
            var cpu = new Cpu();

            AssertFalse(cpu.IsMemoryInitialized(0x1234), nameof(MemoryDistinguishesUninitializedFromZero));
            AssertEqual((ushort)0, cpu.ReadMem(0x1234), nameof(MemoryDistinguishesUninitializedFromZero));

            cpu.WriteMem(0x1234, 0);

            AssertTrue(cpu.IsMemoryInitialized(0x1234), nameof(MemoryDistinguishesUninitializedFromZero));
            AssertTrue(cpu.TryReadMem(0x1234, out var value), nameof(MemoryDistinguishesUninitializedFromZero));
            AssertEqual((ushort)0, value, nameof(MemoryDistinguishesUninitializedFromZero));
        }

        private static void AssertEqual<T>(T expected, T actual, string testName)
        {
            if (!Equals(expected, actual))
                throw new InvalidOperationException($"{testName}: expected {expected}, got {actual}.");
            _passed++;
        }

        private static void AssertTrue(bool condition, string testName)
        {
            if (!condition)
                throw new InvalidOperationException($"{testName}: expected true.");
            _passed++;
        }

        private static void AssertFalse(bool condition, string testName)
        {
            if (condition)
                throw new InvalidOperationException($"{testName}: expected false.");
            _passed++;
        }

        private static void AssertThrows<TException>(Action action, string testName)
            where TException : Exception
        {
            try
            {
                action();
            }
            catch (TException)
            {
                _passed++;
                return;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"{testName}: expected {typeof(TException).Name}, got {ex.GetType().Name}.");
            }

            throw new InvalidOperationException($"{testName}: expected {typeof(TException).Name}, no exception was thrown.");
        }
    }
}
