using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Symulator_x86
{
    public partial class MainForm : Form
    {
        private readonly Color ThemeBgDark = Color.FromArgb(45, 45, 48);
        private readonly Color ThemeBgLight = Color.FromArgb(60, 60, 65);
        private readonly Color ThemeText = Color.FromArgb(240, 240, 240);
        private readonly Color ThemeAccentBlue = Color.FromArgb(0, 122, 204);
        private readonly Color ThemeAccentGreen = Color.FromArgb(90, 210, 90);
        private readonly Color ThemeAccentRed = Color.FromArgb(210, 70, 70);
        private readonly Color ThemeAccentOrange = Color.FromArgb(255, 140, 0);

        private readonly Font FontUI = new Font("Segoe UI", 9.5F, FontStyle.Regular);
        private readonly Font FontTitle = new Font("Segoe UI", 11F, FontStyle.Bold);
        private readonly Font FontHex = new Font("Consolas", 11F, FontStyle.Regular);
        private readonly Font FontHexBold = new Font("Consolas", 11F, FontStyle.Bold);

        private readonly Cpu _cpu = new Cpu();

        private readonly Dictionary<Register, TextBox> _regBoxes = new Dictionary<Register, TextBox>();
        private readonly Dictionary<Register, RadioButton> _baseRadios = new Dictionary<Register, RadioButton>();
        private readonly Dictionary<Register, RadioButton> _indexRadios = new Dictionary<Register, RadioButton>();
        private RadioButton _rbNoneBase, _rbNoneIndex;

        private TextBox _txtDisp, _txtMemoryValue;
        private Label _lblEffectiveAddress;
        private ComboBox _cmbSource, _cmbDest;
        private ListBox _lstLog;

        private readonly HashSet<Control> _opHighlighted = new HashSet<Control>();
        private readonly HashSet<Control> _focused = new HashSet<Control>();

        private bool _suppressSync;

        public MainForm()
        {
            this.Text = "Symulator Procesora x86 - Projekt Zaliczeniowy";
            this.Size = new Size(960, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = ThemeBgDark;
            this.ForeColor = ThemeText;
            this.Font = FontUI;
            this.DoubleBuffered = true;

            BuildInterface();
            InitializeLogic();
            RenderCpuToUi();
            UpdateMemoryPreview();
        }

        // ---------- UI ----------

        private void BuildInterface()
        {
            GroupBox gbReg = CreateModernGroup("Rejestry Danych", 20, 20, 200, 240);
            this.Controls.Add(gbReg);
            AddRegRow(gbReg, Register.AX, 40);
            AddRegRow(gbReg, Register.BX, 90);
            AddRegRow(gbReg, Register.CX, 140);
            AddRegRow(gbReg, Register.DX, 190);

            GroupBox gbAddr = CreateModernGroup("Rejestry Adresowe", 240, 20, 200, 240);
            this.Controls.Add(gbAddr);
            AddRegRow(gbAddr, Register.BP, 40);
            AddRegRow(gbAddr, Register.SI, 90);
            AddRegRow(gbAddr, Register.DI, 140);
            AddRegRow(gbAddr, Register.SP, 190);

            GroupBox gbCmd = CreateModernGroup("Panel Rozkazów", 460, 20, 220, 240);
            this.Controls.Add(gbCmd);

            CreateModernLabel(gbCmd, "Źródło:", 20, 35);
            _cmbSource = CreateOperandCombo(gbCmd, 80, 32, Operand.AX);
            CreateModernLabel(gbCmd, "Cel:", 20, 75);
            _cmbDest = CreateOperandCombo(gbCmd, 80, 72, Operand.BX);

            CreateModernButton(gbCmd, "MOV", 20, 120, ThemeAccentBlue, BtnMov_Click);
            CreateModernButton(gbCmd, "XCHG", 115, 120, ThemeAccentBlue, BtnXchg_Click);

            var btnPush = CreateModernButton(gbCmd, "PUSH", 20, 170, ThemeAccentOrange, BtnPush_Click);
            btnPush.Width = 90;
            var btnPop = CreateModernButton(gbCmd, "POP", 115, 170, ThemeAccentOrange, BtnPop_Click);
            btnPop.Width = 90;

            GroupBox gbMem = CreateModernGroup("Adresowanie Pamięci", 20, 280, 660, 150);
            this.Controls.Add(gbMem);

            Panel pnlBase = new Panel { Location = new Point(10, 30), Size = new Size(280, 40), BackColor = Color.Transparent };
            gbMem.Controls.Add(pnlBase);
            CreateModernLabel(pnlBase, "Baza:", 10, 8);
            _baseRadios[Register.BX] = CreateModernRadio(pnlBase, "BX", 60, 8);
            _baseRadios[Register.BP] = CreateModernRadio(pnlBase, "BP", 120, 8);
            _rbNoneBase = CreateModernRadio(pnlBase, "Brak", 180, 8);
            _rbNoneBase.Checked = true;

            Panel pnlIndex = new Panel { Location = new Point(10, 70), Size = new Size(280, 40), BackColor = Color.Transparent };
            gbMem.Controls.Add(pnlIndex);
            CreateModernLabel(pnlIndex, "Indeks:", 10, 8);
            _indexRadios[Register.SI] = CreateModernRadio(pnlIndex, "SI", 70, 8);
            _indexRadios[Register.DI] = CreateModernRadio(pnlIndex, "DI", 120, 8);
            _rbNoneIndex = CreateModernRadio(pnlIndex, "Brak", 180, 8);
            _rbNoneIndex.Checked = true;

            CreateModernLabel(gbMem, "Offset (Disp):", 300, 35);
            _txtDisp = CreateModernHexBox(gbMem, 410, 32, "0000");

            _lblEffectiveAddress = new Label
            {
                Location = new Point(300, 85),
                AutoSize = true,
                Font = new Font("Consolas", 12F, FontStyle.Bold),
                ForeColor = ThemeText,
                Text = "EA = 0000h"
            };
            gbMem.Controls.Add(_lblEffectiveAddress);

            CreateModernLabel(gbMem, "Wartość w Pamięci:", 500, 35);
            _txtMemoryValue = CreateModernHexBox(gbMem, 520, 60, "0000");
            _txtMemoryValue.ReadOnly = true;
            _txtMemoryValue.BackColor = ThemeBgDark;
            _txtMemoryValue.Size = new Size(80, 30);

            GroupBox gbLog = CreateModernGroup("Historia Operacji", 700, 20, 230, 480);
            this.Controls.Add(gbLog);

            _lstLog = new ListBox
            {
                Location = new Point(15, 30),
                Size = new Size(200, 430),
                BackColor = ThemeBgLight,
                ForeColor = ThemeText,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Consolas", 9F)
            };
            gbLog.Controls.Add(_lstLog);

            var btnReset = CreateModernButton(this, "RESET SYMULATORA", 20, 480, ThemeAccentRed, BtnReset_Click);
            btnReset.Size = new Size(200, 40);

            this.Controls.Add(new Label
            {
                Text = "Created by: Arkadiusz Tokarczyk",
                AutoSize = true,
                Location = new Point(715, 510),
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = Color.Gray
            });
        }

        private void InitializeLogic()
        {
            foreach (Control c in this.Controls)
            {
                AttachHexValidation(c);
                AttachFocusTracking(c);
            }

            foreach (var tb in _regBoxes.Values)
                tb.TextChanged += (s, e) => SyncRegisterFromUi(tb);

            EventHandler refreshEa = (s, e) => UpdateMemoryPreview();
            _regBoxes[Register.BX].TextChanged += refreshEa;
            _regBoxes[Register.BP].TextChanged += refreshEa;
            _regBoxes[Register.SI].TextChanged += refreshEa;
            _regBoxes[Register.DI].TextChanged += refreshEa;
            _txtDisp.TextChanged += refreshEa;
            foreach (var rb in _baseRadios.Values) rb.CheckedChanged += refreshEa;
            foreach (var rb in _indexRadios.Values) rb.CheckedChanged += refreshEa;
            _rbNoneBase.CheckedChanged += refreshEa;
            _rbNoneIndex.CheckedChanged += refreshEa;
        }

        // ---------- Operation handlers ----------

        private void BtnMov_Click(object sender, EventArgs e)
        {
            var src = SelectedOperand(_cmbSource);
            var dst = SelectedOperand(_cmbDest);
            var ea = CurrentEA();

            try { _cpu.Mov(dst, src, ea); }
            catch (InvalidOperationException ex) { ShowError(ex.Message); return; }

            BeginNewHighlight();
            HighlightOperand(dst);
            RenderCpuToUi();
            UpdateMemoryPreview();
            Log($"MOV {dst.Display()}, {src.Display()} [{_cpu.GetOperand(dst, ea):X4}]");
        }

        private void BtnXchg_Click(object sender, EventArgs e)
        {
            var src = SelectedOperand(_cmbSource);
            var dst = SelectedOperand(_cmbDest);
            var ea = CurrentEA();

            try { _cpu.Xchg(src, dst, ea); }
            catch (InvalidOperationException ex) { ShowError(ex.Message); return; }

            BeginNewHighlight();
            HighlightOperand(src);
            HighlightOperand(dst);
            RenderCpuToUi();
            UpdateMemoryPreview();
            Log($"XCHG {dst.Display()}, {src.Display()}");
        }

        private void BtnPush_Click(object sender, EventArgs e)
        {
            var src = SelectedOperand(_cmbSource);
            var ea = CurrentEA();

            try { _cpu.Push(src, ea); }
            catch (InvalidOperationException ex) { ShowError(ex.Message); return; }

            BeginNewHighlight();
            _opHighlighted.Add(_regBoxes[Register.SP]);
            RenderCpuToUi();
            UpdateMemoryPreview();
            Log($"PUSH {src.Display()} -> SP:{_cpu[Register.SP]:X4} [{_cpu.ReadMem(_cpu[Register.SP]):X4}]");
        }

        private void BtnPop_Click(object sender, EventArgs e)
        {
            var dst = SelectedOperand(_cmbDest);
            var ea = CurrentEA();
            var spBefore = _cpu[Register.SP];

            ushort val;
            try { val = _cpu.Pop(dst, ea); }
            catch (InvalidOperationException ex) { ShowError(ex.Message); return; }

            BeginNewHighlight();
            HighlightOperand(dst);
            _opHighlighted.Add(_regBoxes[Register.SP]);
            RenderCpuToUi();
            UpdateMemoryPreview();
            Log($"POP {dst.Display()} <- SP:{spBefore:X4} [{val:X4}]");
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            _cpu.Reset();
            _rbNoneBase.Checked = true;
            _rbNoneIndex.Checked = true;
            _txtDisp.Text = "0000";
            _lstLog.Items.Clear();
            BeginNewHighlight();
            RenderCpuToUi();
            UpdateMemoryPreview();
            Log(">>> SYSTEM ZRESETOWANY <<<");
        }

        // ---------- State <-> UI ----------

        private void SyncRegisterFromUi(TextBox tb)
        {
            if (_suppressSync) return;
            foreach (var kv in _regBoxes)
            {
                if (kv.Value == tb)
                {
                    if (TryParseHex(tb.Text, out var value))
                        _cpu[kv.Key] = value;
                    return;
                }
            }
        }

        private void RenderCpuToUi()
        {
            _suppressSync = true;
            try
            {
                foreach (var kv in _regBoxes)
                    kv.Value.Text = _cpu[kv.Key].ToString("X4");
            }
            finally { _suppressSync = false; }
            RefreshVisuals();
        }

        private ushort CurrentEA()
        {
            Register? baseReg = null;
            foreach (var kv in _baseRadios) if (kv.Value.Checked) { baseReg = kv.Key; break; }
            Register? indexReg = null;
            foreach (var kv in _indexRadios) if (kv.Value.Checked) { indexReg = kv.Key; break; }
            return _cpu.CalculateEA(baseReg, indexReg, ParseHexOrZero(_txtDisp.Text));
        }

        private void UpdateMemoryPreview()
        {
            var ea = CurrentEA();
            _lblEffectiveAddress.Text = $"EA = {ea:X4}h";
            _txtMemoryValue.Text = _cpu.TryReadMem(ea, out var value)
                ? value.ToString("X4")
                : "----";
        }

        // ---------- Highlighting ----------

        private void BeginNewHighlight()
        {
            _opHighlighted.Clear();
            RefreshVisuals();
        }

        private void HighlightOperand(Operand op)
        {
            if (op.IsMemory())
            {
                _opHighlighted.Add(_txtMemoryValue);
                _opHighlighted.Add(_lblEffectiveAddress);
            }
            else
            {
                _opHighlighted.Add(_regBoxes[op.ToRegister()]);
            }
        }

        private void RefreshVisuals()
        {
            foreach (var tb in _regBoxes.Values) ApplyVisual(tb);
            ApplyVisual(_txtDisp);
            ApplyVisual(_txtMemoryValue);

            _lblEffectiveAddress.ForeColor = _opHighlighted.Contains(_lblEffectiveAddress)
                ? ThemeAccentGreen
                : ThemeText;
        }

        private void ApplyVisual(TextBox tb)
        {
            if (_focused.Contains(tb))
            {
                tb.ForeColor = ThemeAccentOrange;
                tb.Font = FontHexBold;
            }
            else if (_opHighlighted.Contains(tb))
            {
                tb.ForeColor = ThemeAccentGreen;
                tb.Font = FontHexBold;
            }
            else
            {
                tb.ForeColor = ThemeText;
                tb.Font = FontHex;
            }
        }

        // ---------- Helpers ----------

        private static bool TryParseHex(string s, out ushort value) =>
            ushort.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value);

        private static ushort ParseHexOrZero(string s) =>
            TryParseHex(s, out var v) ? v : (ushort)0;

        private static Operand SelectedOperand(ComboBox cmb) =>
            ((OperandItem)cmb.SelectedItem).Value;

        private void ShowError(string m) =>
            MessageBox.Show(m, "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);

        private void Log(string m)
        {
            _lstLog.Items.Add($"[{DateTime.Now:HH:mm:ss}] {m}");
            _lstLog.TopIndex = _lstLog.Items.Count - 1;
        }

        private void AttachHexValidation(Control c)
        {
            if (c is TextBox tb && !tb.ReadOnly)
            {
                tb.KeyPress += (s, e) =>
                {
                    if (!Regex.IsMatch(e.KeyChar.ToString(), "[0-9a-fA-F\b]")) e.Handled = true;
                };
                tb.Leave += (s, e) =>
                {
                    tb.Text = ParseHexOrZero(tb.Text).ToString("X4");
                };
            }
            foreach (Control child in c.Controls) AttachHexValidation(child);
        }

        private void AttachFocusTracking(Control c)
        {
            if (c is TextBox tb && !tb.ReadOnly)
            {
                tb.Enter += (s, e) => { _focused.Add(tb); ApplyVisual(tb); };
                tb.Leave += (s, e) => { _focused.Remove(tb); ApplyVisual(tb); };
            }
            foreach (Control child in c.Controls) AttachFocusTracking(child);
        }

        // ---------- UI builders ----------

        private void AddRegRow(GroupBox gb, Register reg, int y)
        {
            CreateModernLabel(gb, reg + ":", 20, y + 3);
            var tb = CreateModernHexBox(gb, 60, y, _cpu[reg].ToString("X4"));
            tb.Name = "txt" + reg;
            _regBoxes[reg] = tb;
        }

        private GroupBox CreateModernGroup(string t, int x, int y, int w, int h) =>
            new GroupBox { Text = t, Location = new Point(x, y), Size = new Size(w, h), ForeColor = ThemeAccentBlue, Font = FontTitle };

        private void CreateModernLabel(Control p, string t, int x, int y) =>
            p.Controls.Add(new Label { Text = t, Location = new Point(x, y), AutoSize = true, ForeColor = ThemeText, Font = FontUI });

        private TextBox CreateModernHexBox(Control p, int x, int y, string def)
        {
            TextBox tb = new TextBox
            {
                Location = new Point(x, y),
                Width = 70,
                Text = def,
                MaxLength = 4,
                BackColor = ThemeBgLight,
                ForeColor = ThemeText,
                BorderStyle = BorderStyle.FixedSingle,
                Font = FontHex,
                TextAlign = HorizontalAlignment.Center,
                CharacterCasing = CharacterCasing.Upper
            };
            p.Controls.Add(tb);
            return tb;
        }

        private RadioButton CreateModernRadio(Control p, string t, int x, int y)
        {
            var r = new RadioButton { Text = t, Location = new Point(x, y), AutoSize = true, ForeColor = ThemeText, Font = FontUI };
            p.Controls.Add(r);
            return r;
        }

        private Button CreateModernButton(Control p, string t, int x, int y, Color bgColor, EventHandler h)
        {
            var b = new Button
            {
                Text = t,
                Location = new Point(x, y),
                Size = new Size(90, 35),
                FlatStyle = FlatStyle.Flat,
                BackColor = bgColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            b.Click += h;
            p.Controls.Add(b);
            return b;
        }

        private ComboBox CreateOperandCombo(Control p, int x, int y, Operand defaultOp)
        {
            var c = new ComboBox
            {
                Location = new Point(x, y),
                Width = 90,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = ThemeBgLight,
                ForeColor = ThemeText,
                FlatStyle = FlatStyle.Flat,
                Font = FontHex
            };
            foreach (Operand op in Enum.GetValues(typeof(Operand)))
                c.Items.Add(new OperandItem(op));
            for (int i = 0; i < c.Items.Count; i++)
            {
                if (((OperandItem)c.Items[i]).Value == defaultOp) { c.SelectedIndex = i; break; }
            }
            p.Controls.Add(c);
            return c;
        }
    }
}
