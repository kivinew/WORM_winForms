WormWinForms/
│
├─ src/                     # Исходники
│   ├─ WormWinForms/         # Основное приложение
│   │   ├─ Program.cs       # Точка входа (Main)
│   │   ├─ GameForm.cs      # UI‑форма (WinForms)
│   │   ├─ GameForm.Designer.cs
│   │   ├─ GameLoop.cs      # Таймеры и основной цикл
│   │   ├─ GameState.cs     # Модель: позиция змейки, еда, счёт, жизни
│   │   ├─ Worm.cs          # Класс «змейка» (список Square)
│   │   ├─ Square.cs
│   │   ├─ Input.cs         # Типобезопасный ввод (Dictionary<Keys,bool>)
│   │   ├─ Network/         # Сетевой слой
│   │   │   ├─ IGameServer.cs
│   │   │   ├─ TcpGameClient.cs
│   │   │   └─ ServerMock.cs (для локального теста)
│   │   └─ Resources/       # PNG, ICO, .resx
│   │
│   └─ WormWinForms.Tests/   # Unit‑тесты (xUnit/NUnit)
│       ├─ GameStateTests.cs
│       └─ NetworkTests.cs
│
├─ .github/workflows/
│   └─ build.yml            # CI‑скрипт
│
├─ .editorconfig           # Форматирование кода
├─ README.md
├─ LICENSE
└─ WormWinForms.sln
