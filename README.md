# CPU Simulator

CPU simulator implementing basic instruction execution, registers, stack operations, and memory addressing in C#.

Graficzny symulator procesora x86 umożliwiający wykonywanie podstawowych operacji na rejestrach oraz pamięci.

## 🖥️ Podgląd aplikacji

<img width="941" height="594" alt="CPU Simulator Screenshot" src="https://github.com/user-attachments/assets/84eb6015-5834-412f-8451-1774546d6449" />

## 🚀 Funkcjonalności

* obsługa rejestrów danych: AX, BX, CX, DX
* obsługa rejestrów adresowych: BP, SI, DI, SP
* wykonywanie instrukcji:

  * MOV
  * XCHG
  * PUSH / POP
* adresowanie pamięci (Base + Index + Offset)
* wizualizacja efektywnego adresu (EA)
* historia wykonanych operacji
* reset stanu symulatora

## 🛠️ Technologie

* C#
* .NET Framework 4.7.2
* WinForms
* Visual Studio 2022
* SDK-style project files

## 📁 Struktura projektu

* `Symulator_x86_Arkadiusz_Tokarczyk` - aplikacja WinForms
* `Symulator_x86.Tests` - prosty runner testów logiki CPU
* `Cpu.cs` - rejestry, pamięć, stos i wykonywanie instrukcji
* `Operand.cs` - definicje rejestrów i operandów

## ▶️ Jak otworzyć

1. Sklonuj repozytorium:

   ```bash
   git clone https://github.com/UnuntriDev/CPU-simulator.git
   ```

2. Otwórz plik `Symulator_x86_Arkadiusz_Tokarczyk.sln` w Visual Studio

3. Uruchom aplikację (F5)

## ✅ Jak uruchomić testy

W Visual Studio zbuduj całe rozwiązanie, a następnie uruchom projekt:

```text
Symulator_x86.Tests
```

Możesz też uruchomić test runner z katalogu projektu po zbudowaniu rozwiązania:

```powershell
.\Symulator_x86.Tests\bin\Debug\net472\Symulator_x86.Tests.exe
```

## 🧠 Jak działa

Aplikacja symuluje podstawowe mechanizmy działania procesora x86:

* operacje na rejestrach
* stos (PUSH/POP)
* rozróżnienie pamięci niezainicjalizowanej od komórki zapisanej wartością `0000`
* adresowanie pamięci z wykorzystaniem baz, indeksów i przesunięcia
* obliczanie efektywnego adresu (EA)

Instrukcje wykonywane są krok po kroku, a ich przebieg zapisywany jest w historii operacji.

## 💡 Czego się nauczyłem

* implementacja logiki niskopoziomowej (symulacja CPU)
* zarządzanie stanem aplikacji
* projektowanie aplikacji desktopowej
* odwzorowanie działania instrukcji asemblerowych

---

Projekt wykonany w celach edukacyjnych.
