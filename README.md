# DineClick
## Sprendžiamo uždavinio aprašymas
### Sistemos paskirtis
Kuriamos sistemos tikslas – padaryti restoranų staliukų rezervacijos procesą paprastesnį bei greitesnį, sutelkti informaciją apie daugelį restoranų vienoje vietoje.

Įprastinis sistemos veikimo modelis būtų toks, jog užsiregistravęs sistemoje naudotojas galėtų matyti visų sistemos restoranų sąrašą ir, pasirinkęs jį dominantį restoraną, matytų jo informaciją, tokią kaip nuotrauką, pavadinimą, darbo laiką, internetinės svetainės adresą. Tuomet, paspaudęs mygtuką staliuko rezervacijai ir pasirinkęs norimą datą bei laiką, įvedęs žmonių skaičių, naudotojas galėtų pateikti prašymą rezervuoti staliuką.
### Funkciniai reikalavimai
Registruotas naudotojas, restorano valdytojas, administratorius galės:
1. Prisijungti;
2. Atsijungti;
3. Redaguoti naudotojo informaciją.

Registruotas naudotojas galės:
1. Peržiūrėti restoranų sąrašą;
2. Peržiūrėti restorano informaciją;
3. Rezervuoti staliuką.

Restorano valdytojas galės:
1. Pridėti restoraną;
2. Peržiūrėti savo pridėtų restoranų sąrašą;
3. Peržiūrėti savo pridėto restorano informaciją;
4. Redaguoti savo pridėto restorano informaciją;
5. Šalinti savo pridėtą restoraną;
6. Peržiūrėti savo pridėto restorano staliukų rezervacijos užklausas;
7. Patvirtinti arba atmesti savo pridėto restorano staliukų rezervacijos užklausas.

Administratorius galės:
1. Peržiūrėti naudotojų sąrašą;
2. Peržiūrėti naudotojo informaciją;
3. Šalinti naudotoją;
4. Blokuoti naudotoją.
## Pasirinktų technologijų aprašymas
Vartotojo sąsajai naudosiu *JavaScript* programavimo biblioteką *React*, serverinei daliai *C#* ir karkasą *ASP.NET Core*. Duomenų bazei naudosiu *MySQL* duomenų bazių valdymo sistemą.
## Sistemos architektūra
Sistemos architektūra bus tokia, jog klientas per HTTPS protokolą komunikuos su klientine dalimi, kuri bus atsakingą už naudotojo sąsajos atvaizdavimą. Prireikus tam tikrų duomenų, ar funkcijų, klientas per HTTPS protokolą komunikuos su aplikacijų programavimo sąsaja (angl. application programming interface, API), kuri, esant poreikiui, kreipsis į duomenų bazę per TCP/IP protokolą.

![deployment_diagram](https://github.com/dominykasvenckus/DineClick/assets/124305272/36fb0338-3023-43ad-b275-308320bc091a)

