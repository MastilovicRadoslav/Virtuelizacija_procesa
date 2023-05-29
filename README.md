# Projekat iz predmeta Virtuelizacija procesa

## Opis rada aplikacije :
Klijent-Server aplikacija, komunikacija se odvija putem WCF-a(Windows Communication Foundation). <br/>
Na klijentu je konzolna aplikacija, gde je potrebno uneti putanju do foldera koji sadrži 'csv.zip'.<br/>
Zip se smešta u MemoryStream koji se putem WCF-a prenosi na server.<br/>
Server prihvata MemoryStream sa zip fajlom koji se raspakuje u folder csv koji će se pojaviti u folderu servera.<br/>
Server se koristi za obradu podataka (podaci su vezani za potrošnju električne energije).<br/>
Server vrši sledeće operacije kako bi obradio podatke :
* Parsira fajlove iz foldera csv koji sadrži podatke.<br/>
* Parsiranje fajlova podrazumeva određivanje :
    * Id
    * Procenjenu vrednost (ForecastValue)
    * Izmerenu vrednost (MeasuredValue)
    * Datum i vreme
    * Naziv fajla
* Nakon parsiranja ovi podaci se upisuju u bazu podataka (XML ili In-Memory) u zavisnosti od opcije u App.config fajlu
* Nakon što su podaci upisani sledi određivanje odstupanja (greške) u zavisnosti od opcije u App.config fajlu :
    * Apsolutno procentualno odstupanje
    * Kvadratno odstupanje
* Posle određenog odstupanja ažurira se baza podataka
* Server putem WCF-a šalje podatke iz baze podataka (XML ili In-Memory) klijentu
* Klijent dobija povratnu informaciju o tome da li je obrada uspešna ili nije i ispisuje obrađene podatke.<br/>
Ažuriranje baze podataka (XML ili In-Memory) vrši se preko Event-a i Delegate-a.<br/>
Na serveru u folderu 'LoadsByDate' kreiraju se XML fajlovi za svaki Load objekat ako se koristi XML baza.


## Aplikacija sadrži :
    * Client - konzolna aplikacija, unosi se putanja do foldera sa 'csv.zip' fajlom i ispisuju se rezultati obrade podataka.
    * Common - class library za klase i interfejs
    * Data Base - za XML i In-Memory baze podataka
    * Server - konzolna aplikacija


## Stanje memorije tokom obrade podataka i nakon ispisa :
![MemorySnapShot](https://github.com/MastilovicRadoslav/Virtuelizacija_procesa/assets/116062572/31617239-0c95-471e-a7a4-5d2113b74628)

Slika prikazuje stanje memorije tokom obrade podataka i nakon ispisa.<br/>
Dolazi do minimalnog oslobađanje memorije a to je zbog upotrebe Despose patterna.<br/>
Pomoću Desposa se oslobađaju MemoryStream-ovi i Event-i za upis u bazu podataka.