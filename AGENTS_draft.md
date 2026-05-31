# vertr-market — agent instructions

## Tech stack
- **.NET 10.0 SDK** — target framework is `net10.0`.

## Правила написания кода C#
1. Используй File-Scoped Namespaces (например, `namespace MyProject.Api;`).
2. Для DTO и Command/Query всегда используй `public record`.
3. Все методы ввода-вывода должны быть асинхронными (`async Task`) и принимать `CancellationToken`.

## Conventions
- `Directory.Build.props` enforces: `<Nullable>enable</Nullable>`, `<ImplicitUsings>enable</ImplicitUsings>`, `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`.
- Global analyzer: `Workleap.DotNet.CodingStandards` (v1.2.4).
- Test framework: **NUnit 4.x** with NUnit3TestAdapter. No inline `[SetUp]` overrides — use constructor or `[OneTimeSetUp]`.
- `KeyValueStore.slnx` is the solution file.

## Локальный запуск
- **Start**: `dotnet run KeyValueStore.slnx`
- **Test**: `dotnet test`
- **Build**: `dotnet build KeyValueStore.slnx`


