# Projekat iz predmeta Virtuelizacija procesa

## Opis rada aplikacije :
Klijent-Server aplikacija, komunikacija se odvija putem WCF-a(Windows Communication Foundation). <br/>
Na klijentu je konzolna aplikacija, gde je potrebno uneti putanju do foldera sa CSV fajlovima koji se šalje na server preko MemoryStream-a.<br/>
Server se koristi za obradu podataka (podaci su vezani za potrošnju električne energije).<br/>
Server vrši sledeće operacije kako bi obradio podatke :
* Parsira fajlove iz foldera koji su primljeni od klijenta preko MemoryStream-a
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
* Klijent dobija povratnu informaciju o tome da li je obrada uspešna ili nije<br/>
Ažuriranje baze podataka (XML ili In-Memory) vrši se preko Event-a i Delegate-a.<br/>
Na serveru se ispisuje In-Memory baza podataka ako je korišćena.<br/>
Ili u folderu 'LoadsByDate' kreiraju XML fajlovi za svaki Load objekat ako se koristi XML baza.


## Aplikacija sadrži :
    * Client - konzolna aplikacija, unosi se putanja do foldera sa CSV fajlovima
    * Common - class library za klase i interfejs
    * Data Base - za XML i In-Memory baze podataka
    * Server - konzolna aplikacija, ispisuje In-Memory bazu