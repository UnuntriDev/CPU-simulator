# CPU Simulator

CPU simulator implementing instruction execution, registers, and memory management in C#.

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
* .NET
* WinForms

## ▶️ Jak otworzyć

1. Sklonuj repozytorium:

   ```bash
   git clone https://github.com/UnuntriDev/CPU-simulator.git
   ```

2. Otwórz projekt w Visual Studio

3. Uruchom aplikację (F5)

## 🧠 Jak działa

Aplikacja symuluje podstawowe mechanizmy działania procesora x86:

* operacje na rejestrach
* stos (PUSH/POP)
* adresowanie pamięci z wykorzystaniem baz, indeksów i przesunięcia
* obliczanie efektywnego adresu (EA)

Instrukcje wykonywane są krok po kroku, a ich przebieg zapisywany jest w historii operacji.

## 💡 Czego sie nauczyłem

* implementacja logiki niskopoziomowej (symulacja CPU)
* zarządzanie stanem aplikacji
* projektowanie aplikacji desktopowej
* odwzorowanie działania instrukcji asemblerowych

---

Projekt wykonany w celach edukacyjnych.
