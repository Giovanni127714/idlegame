# Idlegame

Een idle/incremental desktop game (in de stijl van Cookie Clicker), gebouwd
als schoolproject met C# / .NET 8 en WPF (MVVM). Voortgang wordt lokaal
opgeslagen als JSON.

## Vereisten

- Windows
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (of nieuwer)
- Optioneel: Visual Studio 2022 met de workload ".NET desktop development"

## Bouwen & starten (command line)

```
dotnet build Idlegame/Idlegame.csproj
dotnet run --project Idlegame/Idlegame.csproj
```

Gebruik `-c Release` voor een release-build.

## Bouwen & starten (Visual Studio)

1. Open `Idlegame/Idlegame.slnx` in Visual Studio.
2. Druk op `F5` (of `Ctrl+F5` om te starten zonder debugger).

## Projectstructuur

```
Idlegame/
  Idlegame.csproj      SDK-style .NET 8 WPF project
  App.xaml(.cs)        Applicatie-entrypoint
  Views/                XAML-schermen (code-behind)
  ViewModels/           MVVM-viewmodels (binding-logica)
  Helpers/              MVVM-hulpklassen (ObservableObject, RelayCommand)
  Models/               Databronnen (game state, upgrades, ...) — volgt in latere fases
  Services/             Opslag/laad-logica, timers, etc. — volgt in latere fases
```
