using System;
using System.Collections.Generic;

namespace Symulator_x86
{
    public sealed class Cpu
    {
        public const ushort InitialSP = 0xFFFE;

        private readonly Dictionary<Register, ushort> _regs = new Dictionary<Register, ushort>();
        private readonly Dictionary<ushort, ushort> _memory = new Dictionary<ushort, ushort>();

        public IReadOnlyDictionary<ushort, ushort> Memory => _memory;

        public Cpu() { Reset(); }

        public ushort this[Register r]
        {
            get => _regs.TryGetValue(r, out var v) ? v : (ushort)0;
            set => _regs[r] = value;
        }

        public ushort ReadMem(ushort addr) =>
            _memory.TryGetValue(addr, out var v) ? v : (ushort)0;

        public bool TryReadMem(ushort addr, out ushort val) =>
            _memory.TryGetValue(addr, out val);

        public bool IsMemoryInitialized(ushort addr) =>
            _memory.ContainsKey(addr);

        public void WriteMem(ushort addr, ushort val) => _memory[addr] = val;

        public ushort GetOperand(Operand op, ushort ea) =>
            op.IsMemory() ? ReadMem(ea) : this[op.ToRegister()];

        public void SetOperand(Operand op, ushort ea, ushort val)
        {
            if (op.IsMemory()) WriteMem(ea, val);
            else this[op.ToRegister()] = val;
        }

        public void Mov(Operand dst, Operand src, ushort ea)
        {
            if (dst.IsMemory() && src.IsMemory())
                throw new InvalidOperationException("Operacja Pamięć-Pamięć zabroniona.");
            SetOperand(dst, ea, GetOperand(src, ea));
        }

        public void Xchg(Operand a, Operand b, ushort ea)
        {
            if (a.IsMemory() && b.IsMemory())
                throw new InvalidOperationException("Operacja Pamięć-Pamięć zabroniona.");
            var va = GetOperand(a, ea);
            var vb = GetOperand(b, ea);
            SetOperand(a, ea, vb);
            SetOperand(b, ea, va);
        }

        public void Push(Operand src, ushort ea)
        {
            var val = GetOperand(src, ea);
            this[Register.SP] = (ushort)((this[Register.SP] - 2) & 0xFFFF);
            WriteMem(this[Register.SP], val);
        }

        public ushort Pop(Operand dst, ushort ea)
        {
            var sp = this[Register.SP];
            if (!TryReadMem(sp, out var val))
                throw new InvalidOperationException("Stos jest pusty.");
            this[Register.SP] = (ushort)((sp + 2) & 0xFFFF);
            SetOperand(dst, ea, val);
            return val;
        }

        public ushort CalculateEA(Register? baseReg, Register? indexReg, ushort disp)
        {
            int b = baseReg.HasValue ? this[baseReg.Value] : 0;
            int i = indexReg.HasValue ? this[indexReg.Value] : 0;
            return (ushort)((b + i + disp) & 0xFFFF);
        }

        public void Reset()
        {
            _memory.Clear();
            foreach (Register r in Enum.GetValues(typeof(Register)))
                _regs[r] = 0;
            _regs[Register.SP] = InitialSP;
        }
    }
}
