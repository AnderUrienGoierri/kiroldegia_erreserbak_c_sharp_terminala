# Kiroldegia Erreserbak - C# Terminala

Ordiziako Majori Kiroldegiko erreserbak kudeatzeko C# kontsola aplikazioa (.NET 9.0).

Aplikazio honek zuzenean kontsolatik pistak erreserbatu, dauden erreserbak kontsultatu, ezeztatu/ezabatu (Soft eta Hard Delete) eta bonoak kudeatzeko balio du, MySQL datu-basearekin zuzenean komunikatuz ADO.NET transakzio atomikoen bidez (`MySqlTransaction` eta `SELECT ... FOR UPDATE`).

---

## Ezaugarri Nagusiak

1. **Pisten Kontsulta**: Pista libreen eta okupatuen egoera denbora errealean bistaratu.
2. **Erreserba Berriak Formulario Bidez**: Pista bat aukeratu eta erreserba berria sortu transakzio atomiko batean (blokeoak eta konkurrentzia bermatuz).
3. **Erreserbak Ezeztatu / Ezabatu**:
   - **Soft Delete**: Erreserba logikoki ezeztatu, pista libre utziz baina erregistroa mantenduz.
   - **Hard Delete**: Datu-basetik fisikoki ezabatu.
   - **Restore**: Ezeztatutako erreserbak berreskuratu.
4. **Bonoen eta Ordainketen Kudeaketa**: Bono bidezko transakzioak eta saldo deskontuak.

---

## Baldintzak eta Abiaraztea

1. **.NET 9.0 SDK** instalatuta izatea.
2. **MySQL zerbitzaria** martxan egotea (`kiroldegia` datu-basearekin).

### Exekutatzeko agindua:

```bash
# Terminalaren karpetan sartu
cd kiroldegia_erreserbak_c_sharp_terminala/kiroldegia_erreserbak_c_sharp_terminala

# Eraiki eta exekutatu
dotnet run
```
