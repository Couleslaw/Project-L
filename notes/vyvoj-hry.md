### Vizualni design hry

Design hry jsem udelal ve Figme, puvodne jsem planoval pouze pro tri hrace, protoze to se nejlepe vejde na obrazovku, ale na konzultaci me pan doktor Holan presvedcil, ze by to mohla byt skoda, proto pridam i moznost hry pro ctyry hrace.

### Objektovy navrh

- zacal jsem psat kostru hry, nektere casti jsou moc komplikovane, hlavne jak mezi sebou veci komunikuji
- Holan navrhl Observer pattern, na ktery to prepisu

### Vysledek konzultace na konci zimniho semestru

- hru budu delat az pro ctyry hrace
- kazdy kdo splni interface AI bude moct dynamicky do hry nacist knihovnu se svym agentem
- detekci kolizi dilku na skladackou budu delat pomoci bitovych operaci, skladacka je 5x5 = 25b, takze se vejde do intu
- puvodne jsem chtel dat do hry nekonecny bank dilku, prestoze ve fyzicke hre je jich omezene mnozstvi
  - duvod je ten, ze ke hre si je mozne dovolit dilky, takze omezeni neni herni mechanika, ale cena
  - lepsi reseni je nechat si hrace zvolit jak bude pocet dilku omezen
  - indikaci vhodne pozice dilku nad skladackou chci udelat pomoci snapping, Holan navrhl jeste highlight
- ve finalnim dokumentu by mel byt Swim lines diagram jak spolu jednotlive objekty komunikuji

### Overovani legality akci

- je potreba mit nejaky ActionVerifier ktery rozhodne zda lze akci provest
- dve moznosti
  - bud budu po AI hraci vyzadovat aby vzdy odevzdal validni akci
  - nebo mu pouze dam k dispozici Verifikator ale nebudu vyzadovat aby odevzdal overenou akci
  - lepsi druhy pristup, protoze pokud by AI hrac mel v sobe chybu a nebyl schopny vygenerovat validni akci, tak by se hra zasekla
  - radeji kdyz zjistim ze mi dal nevalidni akci, tak ji zahodim a pomoci zakladniho AI hrace vygeneruju validni akci
- teoreticky fix by mohl byt kdyby hra vygenerovala vsechny validni akce a poskytla je hraci
  - ale to by bylo narocne na cas i pamet
  - navic AI hrac si z dostupnych informaci muze ty akce vygenerovat sam
  - ale ne kazdy AI hrac by to chtel delat --> zbytecne plytvani zdroji
- reseni pomoci verifikatoru
  - akce nebude mozne modifikovat po jejich vytvoreni
  - defaultne budou mit vsechny akce stav `UNVERIFIED`
  - po tom co zavolam `akce.verify(verifikator)`, tak se stav nastavi na `VERIFIED` nebo `INVALID`
  - `akce.verify` take vraci `VerificationMessage` kde se volajici dozvi co je s akci spatne
    - ucel verifikatoru je primarne overit ze je akce legalni, takze vraci pouze prvni chybu na kterou narazi
  - verifikator ma pristup k aktualnimu stavu hry

### Pravidla

- bile skladacky se pouziji vsechny, cernych jen 12/14/16 z 20 podle poctu hracu.
- kdo zacina se urci nahodne
- na zacatku - 1xlevel1 a 1xlevel2 dilky
- hra konci kdyz je black puzzle deck empty. Hraci dokonci toto kolo a pak hraji jedno posledni kolo
- behem tahu musi hrac vykonat 3 akce
  - take
    - take a puzzle from a puzzle row
    - replace it from the puzzle deck (if not empty)
    - or take a puzzle from the top of a deck (if not empty)
    - max 4 unfinished puzzles
    - if 'the end of the game' has been triggered (black pile empty), you can take only 1 black puzzle per turn
  - recycle
    - choose a puzzle row
    - put the puzzles on the bottom of the deck in your order
    - refill the row
  - take a level 1 piece
    - only available if there are still some left
  - upgrade
    - return a piece to the reserve
    - take a new piece - level_new <= level_old + 1
    - if there are no pieces from the higher level left, you can go one level higher
    - _graphic_ nejak highlight co clovek muze vzit
  - place a piece into puzzle
  - master move
    - once per turn
    - place up to 1 piece into each unfinished puzzle
    - for each puzzle: place piece or skip it
- pokud hrac nejakou akci dokonci skladacku (nebo skladacky)
  - return all pieces from puzzle to personal supply
  - add points for completing the puzzle
  - take a new piece as reward
    - if the piece is not available, take a piece of the next available level

### Faze hry

- start - preparace a zamichani hracu
- normalni hra
- end of the game
- finishing touches
  - kazdy hrac postupne muze prilozit dalsi dilky do skladacek
  - kazdy dilek co pouzije = -1 score
  - kdyz dokonci skladacku, tak nedostane ten dilek ani body !
- final scoring
  - body za dokoncene skladacky, minus body za nedokoncene skladacky, minus body za dilky pouzite ve fazi Finishing touches
  - the player with the most points wins
    - tie -> most puzzles completed wins
    - tie -> most pieces leftover wins
    - tie -> all tied players share the victory

## cteni puzzle config souboru

- dve moznosti

  1. kdyz je nevalidni format radku / puzzlu, tak ten puzzle skipnout, ale nacist ostatni a zapnout hru
  2. nebo vyhodit vyjimku a zavrit reader

- zvolil jsem 2. protoze pokud je nejake puzzle invalid, tak o tom chci vedet a opravit to
