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
* xUnit v3
* Visual Studio 2022
* SDK-style project files

## 📁 Struktura projektu

* `Symulator_x86` - aplikacja WinForms
* `Symulator_x86.Tests` - testy jednostkowe logiki CPU w xUnit
* `Cpu.cs` - rejestry, pamięć, stos i wykonywanie instrukcji
* `Operand.cs` - definicje rejestrów i operandów

## ▶️ Jak otworzyć

1. Sklonuj repozytorium:

   ```bash
   git clone https://github.com/UnuntriDev/CPU-simulator.git
   ```

2. Otwórz plik `Symulator_x86.sln` w Visual Studio

3. Uruchom aplikację (F5)

## ✅ Jak uruchomić testy

W Visual Studio otwórz Test Explorer i uruchom testy z projektu:

```text
Symulator_x86.Tests
```

Możesz też uruchomić testy z katalogu projektu:

```powershell
dotnet test .\Symulator_x86.Tests\Symulator_x86.Tests.csproj
```

## 📦 Gotowa aplikacja

Gotowe paczki aplikacji są publikowane w GitHub Releases po wypchnięciu taga w formacie `v*`, np. `v1.0.0`.

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
