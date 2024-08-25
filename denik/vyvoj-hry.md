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
