## Developments notes

- using Unity 6
- using [Nuget for Unity](https://github.com/GlitchEnzo/NuGetForUnity)
- using [Ini-parser](https://www.nuget.org/packages/ini-parser-netstandard)
- using [Unity logger](https://github.com/herbou/Unity_Logger)

## Decisions

#### Pause menu

Intutivne setActive(false) / setActive(true), ale vynika problem. Ruzny pocet hracu --> ruzna velikost panelu --> je potreba content size fitter. Ten se ale neaktualizuje kdyz je panel disabled, takze poprve kdyz ze pause menu zobrazi, tak to osklive "skoci". Reseni je dat pause menu CanvasGroup a resit hiding pomoci alpha.

## Specification notes

- max player name length: 15 characters
