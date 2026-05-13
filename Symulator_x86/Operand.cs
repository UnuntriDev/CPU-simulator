using System;

namespace Symulator_x86
{
    public enum Register { AX, BX, CX, DX, BP, SI, DI, SP }

    public enum Operand { AX, BX, CX, DX, BP, SI, DI, SP, Memory }

    public static class OperandExtensions
    {
        public const string MemoryDisplay = "PAMIEĆ";

        public static bool IsMemory(this Operand op) => op == Operand.Memory;

        public static Register ToRegister(this Operand op)
        {
            switch (op)
            {
                case Operand.AX: return Register.AX;
                case Operand.BX: return Register.BX;
                case Operand.CX: return Register.CX;
                case Operand.DX: return Register.DX;
                case Operand.BP: return Register.BP;
                case Operand.SI: return Register.SI;
                case Operand.DI: return Register.DI;
                case Operand.SP: return Register.SP;
                default: throw new InvalidOperationException($"Operand {op} is not a register.");
            }
        }

        public static string Display(this Operand op) =>
            op == Operand.Memory ? MemoryDisplay : op.ToString();
    }

    public sealed class OperandItem
    {
        public Operand Value { get; }
        public OperandItem(Operand v) { Value = v; }
        public override string ToString() => Value.Display();
    }
}
