# Безопасная разработка (Secure Coding): защита от атак и уязвимостей

## Цели занятия

- применять техники защитного программирования (defensive programming) для повышения надежности сервиса;
- реализовывать защиту от атак на отказ в обслуживании (Denial of Service);
- выполнять строгую валидацию всех входящих данных от недоверенных источников;
- анализировать зависимости проекта на предмет известных уязвимостей.

## Краткое содержание

- принцип "Никогда не доверяй клиенту" и валидация ввода;
- защита от DoS-атак: ограничение размера входящих данных, таймауты на операции, ограничение количества подключений
- предотвращение утечки информации: правильная обработка и логирование ошибок;
- безопасность зависимостей: использование dotnet list package --vulnerable;
- практические примеры защиты парсера и сетевого уровня.

## Результаты

- модифицированный код сетевого обработчика, включающий валидацию входных данных и ограничение ресурсов для защиты от DoS-атак.

## Links

https://learn.microsoft.com/en-us/dotnet/standard/security/secure-coding-guidelines

https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/security-warnings

https://cheatsheetseries.owasp.org/cheatsheets/DotNet_Security_Cheat_Sheet.html

https://owasp.org/Top10/2025/

https://owasp.org/www-project-smart-contract-top-10/

https://docs.github.com/en/code-security/tutorials/secure-your-dependencies/dependabot-quickstart

https://www.microsoft.com/en-us/securityengineering/sdl

https://habr.com/ru/companies/jugru/articles/341792/

