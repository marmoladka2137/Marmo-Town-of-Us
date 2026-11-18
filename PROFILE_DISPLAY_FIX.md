# Poprawka profilu, rozdzielczości i menu — 1.1.0-beta.8

Kandydat zbudowany ze źródeł beta.7. Etykieta w grze pozostaje `1.1 BETA`. Oryginalny projekt i DLL w TEST MODY nie zostały zastąpione. Kopia DLL i prywatnych plików profilu znajduje się lokalnie w katalogu `backup`; pliki profilu nie są częścią paczek.

## Zmiany i ustalone przyczyny

**Znikający napis Discorda.** W rzeczywistym kliencie obiekt przycisku i tekst istniały, ale `TextMeshPro.color` wynosił `(0,0,0,0)`. Nowo utworzony `PassiveButton` miał domyślne przezroczyste kolory stanów i nadpisywał kolor tekstu. Wszystkie jego kolory zostały jawnie ustawione. Napis ma właściwą głębokość UI, jest ponownie włączany po przebudowie menu i nie tworzy duplikatów. Widoczna etykieta to wyłącznie `dc.gg/mtou`, adres kliknięcia: `https://dc.gg/mtou`. Dodano też wersję lobby, ukrywaną przy otwartym czacie, ustawieniach lub szafie i poza lobby.

**Logo.** Oryginalny dostarczony obraz zachowuje proporcje. Zmniejszono rezerwy layoutu: przy 1920×1080 wynik ma około 320×182 px. Znajduje się nad PLAY, pod paskiem konta, bez nakładania na przyciski. Nie generowano nowych grafik.

**Kasowanie stroju.** Początkowy profil miał puste identyfikatory kosmetyków; nie odtwarzano żadnego zgadywanego stroju. Nie odtworzono jednoznacznie pierwotnego powodu utraty wybranego przez użytkownika zestawu. Usunięto zastępowanie obiektów PlayerData/SettingsData podczas uruchamiania zintegrowanego pluginu. Dodano osobny, atomowo zapisywany plik `mtou-cosmetic-selection.json`: przechowuje wyłącznie pięć identyfikatorów kosmetyków, bez danych konta. Jawny wybór w szafie aktualizuje tylko wybrany slot. Celowe zdjęcie kosmetyku zapisuje pustą wartość. Automatyczne wyczyszczenie przy zapisie lub odczycie nie nadpisuje zapamiętanego wyboru. Odświeżenie wyglądu w lobby czeka na asynchronicznie instalowane kosmetyki Corsac.

**Niska rozdzielczość.** Wcześniejsze kopie QA korzystały ze wspólnych ustawień Unity i flag 1280×720. To był błąd izolacji testów. Zapisany rozmiar okna został przywrócony do istniejącej preferowanej rozdzielczości 1920×1080, bez zmiany trybu fullscreen. Nowa DLL nie wymusza rozdzielczości przy starcie. Testy używają osobnego profilu, odmiennego sufiksu plików, blokady zapisów poza jego katalogiem i przywracają klucze rozdzielczości Unity. Launcher odmawia startu przy innej działającej instancji gry.

## Testy

- Release: 0 błędów, 427 ostrzeżeń istniejących w bazie; brak nowych ostrzeżeń kompilatora.
- 12 testów czystego kodu magazynu kosmetyków: ponowny odczyt, ochrona przed pustym seedem, niezależność slotów, celowe zdjęcie, walidacja, odzyskiwanie kopii po uszkodzeniu JSON i brak pozostawionych plików tymczasowych.
- Dwa uruchomienia rzeczywistego klienta IL2CPP z finalną DLL i oddzielnym profilem: 41 oraz 43 asercje. Wybrany identyfikator istniejącego kosmetyku przetrwał zapis, wymuszone wyczyszczenie w pamięci, odczyt oraz restart procesu. Sprawdzono możliwość celowego zdjęcia i podpięcie patchy do pięciu natywnych metod ClickEquip.
- Menu rzeczywiście uruchomiono w 1920×1080. Sprawdzono granice logo, jeden link, nieprzezroczyste glify, kolory stanów przycisku, collider i ponowne włączenie linku. Wygenerowany obraz menu został obejrzany; napis jest widoczny.
- Obliczenia obszaru logo sprawdzono dodatkowo dla 1280×720, 1600×900, 1024×768, 2560×1080 i 800×600. Nie oznacza to uruchomienia gry we wszystkich tych rozdzielczościach.
- Sześć istniejących plików gracza/ustawień w prawdziwym profilu pozostało bitowo bez zmian. Ustawienia wyświetlania Unity zostały przywrócone po testach.

Testy natywne są fixturem menu i profilu: wybór kosmetyku wywołano przez kod, nie przez ręczne kliknięcie w szafie. Testowano MTOU, MiraAPI, Reactor i Corsac oraz prywatny plugin testowy. Nie uruchomiono pełnego zestawu ośmiu pluginów użytkownika. AchievementsAPI nie było w tej kopii QA; sprawdzenie braku przycisku gwiazdek w tym uruchomieniu nie jest testem obecności tego zewnętrznego pluginu.

## Niezeweryfikowane

- Widoczność i kliknięcie linku w prawdziwym lobby; test menu nie otwierał zewnętrznej strony Discorda.
- Wielomeczowa rozgrywka, multiplayer, wszystkie role i zmiany stroju ról, ręczny wybór niestandardowych kosmetyków oraz interakcje wszystkich zewnętrznych pluginów.
- Całkowite usunięcie pierwotnej przyczyny kasowania stroju na konfiguracji użytkownika. Potwierdzono działanie nowego zabezpieczenia w opisanym zakresie, nie pełną zgodność każdego pluginu.

W logach nadal są wcześniejsze komunikaty MiraAPI/IL2CPP o rejestracjach/duplikatach. Nie uznano ich za naprawione. Opcjonalna bariera QA może zgłosić pojedyncze ostrzeżenie o anulowaniu zapisu poza profilem; jest wyłączona w zwykłym uruchomieniu bez MTOU_QA_PROFILE_ROOT.

## Zmienione źródła

- `Modules/Profile/CosmeticSelectionStore.cs` — magazyn preferencji i odzyskiwanie JSON.
- `Modules/Profile/CosmeticSelectionRuntime.cs` — przechwytywanie jawnego wyboru, ochrona save/load, odświeżenie lobby.
- `Modules/Profile/ProfilePaths.cs` — opt-in izolacja QA i bariera zapisu.
- `TouMarmoPlugin.cs` — inicjalizacja i warunkowe patche QA.
- `IntegratedTownOfUs/TownOfUsPlugin.cs` — usunięcie podmiany obiektów danych.
- `IntegratedTownOfUs/Patches/Misc/SaveFilePatches.cs` — sufiks QA; zwykły `_TOUMira` zachowany.
- `Modules/MarmoSession.cs`, `Modules/MenuBrandingLayout.cs`, `Modules/MenuCommunityLink.cs` — odświeżanie kosmetyków, layout, widoczny link.
- `TOU-Marmo.csproj` — wersja beta.8.
- `tools/ProfileStoreTests/*` — testy niezależne od gry.
- `README.md`, `BUILDING.md`, `INSTALLATION.md`, `CHANGELOG.md`, niniejszy raport — dokumentacja.

## Plik wynikowy

`artifacts/TOU-Marmo.dll`

SHA-256:
`BD9259EB1FB7A554A97CFCFBA7195E1BD1B582366647AE3AC9ECD6F2D71A87B7`

Nie wdrożono DLL do TEST MODY. Instrukcja ręcznej instalacji jest w INSTALACJA_TESTOWA.md. Paczki nie zawierają gry, prywatnego profilu, pluginów QA ani zależności silnika.
